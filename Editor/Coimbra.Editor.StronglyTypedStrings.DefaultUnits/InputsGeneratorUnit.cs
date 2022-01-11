using JetBrains.Annotations;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Coimbra.StronglyTypedStrings.DefaultUnits
{
    /// <summary>
    /// Generates strongly-typed strings for the Unity inputs.
    /// </summary>
    [GeneratorExtension, UsedImplicitly]
    public sealed class InputsGeneratorUnit : GeneratorUnit
    {
        /// <summary>
        /// The label to be displayed on the Project Settings menu.
        /// </summary>
        public override GUIContent Label { get; } = new GUIContent("Inputs");

        protected override int Priority { get; } = 10;
        protected override string Type { get; } = "Input";

        protected override void OnGenerate()
        {
            var namesSet = new HashSet<string>();
            var serializedObject = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/InputManager.asset")[0]);
            SerializedProperty axesProperty = serializedObject.FindProperty("m_Axes");

            foreach (SerializedProperty property in axesProperty)
            {
                string input = property.FindPropertyRelative("m_Name").stringValue;
                string name = GeneratorUtility.GetValidMemberName(input);

                if (namesSet.Contains(name))
                {
                    continue;
                }

                namesSet.Add(name);
                Append(name, input);
            }
        }
    }
}
