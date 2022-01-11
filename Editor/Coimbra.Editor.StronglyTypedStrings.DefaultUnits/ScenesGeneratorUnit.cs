using JetBrains.Annotations;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SettingsManagement;
using UnityEngine;

namespace Coimbra.StronglyTypedStrings.DefaultUnits
{
    /// <summary>
    /// Generates strongly-typed strings for the Unity scenes. It also generates strongly-typed ints for them.
    /// </summary>
    [GeneratorExtension, UsedImplicitly]
    public sealed class ScenesGeneratorUnit : GeneratorUnit
    {
        private static readonly GUIContent IgnoredDisabledScenesLabel = new GUIContent("Ignore Disabled Scenes*", "You can choose to ignore disabled scenes when generating the scenes file.");

        [UserSetting] private static readonly GeneratorSetting<bool> IgnoreDisabledScenesSetting = new GeneratorSetting<bool>("scenes.ignoreDisabledScenes", true);

        /// <summary>
        /// The label to be displayed on the Project Settings menu.
        /// </summary>
        public override GUIContent Label { get; } = new GUIContent("Scenes");

        protected override int Priority { get; } = 10;
        protected override string Type { get; } = "Scene";

        /// <summary>
        /// This method is called automatically by the <see cref="Generator"/> if the <see cref="GeneratorExtensionAttribute"/> is present.
        /// </summary>
        public override void DrawSettings(string searchContext)
        {
            if (GeneratorUtility.TryMatchSearch(searchContext, IgnoredDisabledScenesLabel.text))
            {
                IgnoreDisabledScenesSetting.value = SettingsGUILayout.SettingsToggle(IgnoredDisabledScenesLabel, IgnoreDisabledScenesSetting, searchContext);
            }
        }

        protected override void OnGenerate()
        {
            EditorBuildSettingsScene[] scenes = EditorBuildSettings.scenes;

            if (scenes == null || scenes.Length <= 0)
            {
                return;
            }

            var names = new List<string>();
            var paths = new List<string>();
            var duplicates = new List<int>();

            foreach (EditorBuildSettingsScene scene in scenes)
            {
                if (!scene.enabled)
                {
                    continue;
                }

                string path = scene.path;

                if (string.IsNullOrWhiteSpace(path) || path.Length <= "Assets/".Length + ".unity".Length)
                {
                    continue;
                }

                string name = Path.GetFileNameWithoutExtension(path);

                path = path.Remove(0, "Assets/".Length);
                path = path.Remove(path.Length - ".unity".Length);

                if (names.Contains(name))
                {
                    if (duplicates.Count == 0)
                    {
                        duplicates.Add(names.IndexOf(name));
                    }

                    duplicates.Add(names.Count);
                }

                names.Add(name);
                paths.Add(path);
            }

            foreach (int i in duplicates)
            {
                names[i] = paths[i];
            }

            for (int i = 0; i < paths.Count; i++)
            {
                Append(GeneratorUtility.GetValidMemberName(names[i]), paths[i], i);
            }
        }
    }
}
