using JetBrains.Annotations;
using UnityEditorInternal;
using UnityEngine;

namespace Coimbra.StronglyTypedStrings.DefaultUnits
{
    /// <summary>
    /// Generates strongly-typed strings for the Unity tags.
    /// </summary>
    [GeneratorExtension, UsedImplicitly]
    public sealed class TagsGeneratorUnit : GeneratorUnit
    {
        /// <summary>
        /// The label to be displayed on the Project Settings menu.
        /// </summary>
        public override GUIContent Label { get; } = new GUIContent("Tags");

        protected override int Priority { get; } = 10;
        protected override string Type { get; } = "Tag";

        protected override void OnGenerate()
        {
            foreach (string tag in InternalEditorUtility.tags)
            {
                Append(tag);
            }
        }
    }
}
