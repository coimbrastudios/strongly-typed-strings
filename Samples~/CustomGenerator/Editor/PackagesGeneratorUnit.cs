using Coimbra.StronglyTypedStrings;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SettingsManagement;
using UnityEngine;

namespace Coimbra.Samples.StronglyTypedStrings.CustomGenerator
{
    // You must inherit from GeneratorUnit (or a sub-class of it) and you must add the GeneratorExtension attribute for it show up in the Project Settings windows
    [GeneratorExtension]
    public sealed class PackagesGeneratorUnit : GeneratorUnit
    {
        private static readonly GUIContent PretendThereIsNoPackageLabel = new GUIContent("Pretend There Is No Package", "If checked, it will act as if the Packages folder was empty.");

        // Use the UnityEditor.SettingsManagement.UserSettings attribute together with the GeneratorSetting<T> type to expose a setting in the Project Settings window
        [UserSetting] private static readonly GeneratorSetting<bool> PretendThereIsNoPackageSetting = new GeneratorSetting<bool>("packages.pretendThereIsNoPackage", false);

        // The name to show next to the Generate button in the Project Settings windows
        public override GUIContent Label { get; } = new GUIContent("Packages");

        // Units with same Priority are ordered by the Label, here we are forcing the item to show on the top of the Project Settings window
        protected override int Priority { get; } = int.MinValue;

        // The final type name will have the word "Type" appended to it
        protected override string Type { get; } = "Package";

        // The Project Settings window draw logic goes here
        public override void DrawSettings(string searchContext)
        {
            PretendThereIsNoPackageSetting.value = SettingsGUILayout.SettingsToggle(PretendThereIsNoPackageLabel, PretendThereIsNoPackageSetting, searchContext);
        }

        // The generator logic goes here, it is called when the respective Generate button (or the Generate All button) is clicked
        protected override void OnGenerate()
        {
            if (PretendThereIsNoPackageSetting)
            {
                return;
            }

            var values = new HashSet<string>();

            foreach (string path in AssetDatabase.GetAllAssetPaths())
            {
                const string prefix = "Packages/";

                if (!path.StartsWith(prefix))
                {
                    continue;
                }

                string value = path.Remove(0, prefix.Length);
                int i = value.IndexOf("/", StringComparison.Ordinal);

                if (i >= 0)
                {
                    value = value.Remove(i, value.Length - i);
                }

                if (!values.Add(value))
                {
                    continue;
                }

                string name = value.Replace(".", "_");

                // Call Append to add content in the generated file
                Append(name, value);
            }
        }
    }
}
