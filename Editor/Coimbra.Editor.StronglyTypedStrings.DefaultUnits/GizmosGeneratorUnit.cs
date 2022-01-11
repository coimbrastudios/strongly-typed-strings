using JetBrains.Annotations;
using UnityEditor.SettingsManagement;
using UnityEngine;

namespace Coimbra.StronglyTypedStrings.DefaultUnits
{
    /// <summary>
    /// Generates strongly-typed paths for the contents of the special folder "Gizmos".
    /// </summary>
    [GeneratorExtension, UsedImplicitly]
    public sealed class GizmosGeneratorUnit : SpecialFolderContentGeneratorUnit
    {
        [UserSetting] private static readonly GeneratorSetting<bool> ScanAllFoldersSetting = new GeneratorSetting<bool>("gizmos.scanAllFolders", true);
        [UserSetting] private static readonly GeneratorSetting<string[]> FoldersToScanSetting = new GeneratorSetting<string[]>("gizmos.foldersToScan", new string[0]);

        /// <summary>
        /// The label to be displayed on the Project Settings menu.
        /// </summary>
        public override GUIContent Label { get; } = new GUIContent("Gizmos*", "Generator for the Assets/Gizmos folder.");

        protected override int Priority { get; } = 200;
        protected override string SpecialFolderName { get; } = "Gizmos";
        protected override string Type { get; } = "Gizmo";

        /// <summary>
        /// This method is called automatically by the <see cref="Generator"/> if the <see cref="GeneratorExtensionAttribute"/> is present.
        /// </summary>
        public override void DrawSettings(string searchContext)
        {
            DrawContentSettings(searchContext, ScanAllFoldersSetting, FoldersToScanSetting);
        }

        protected override bool IsValidEntry(string entry)
        {
            string requiredPrefix = $"Assets/{SpecialFolderName}/";

            if (entry.StartsWith(requiredPrefix) == false)
            {
                return false;
            }

            if (ScanAllFoldersSetting)
            {
                return true;
            }

            foreach (string folder in FoldersToScanSetting.value)
            {
                if (string.IsNullOrWhiteSpace(folder))
                {
                    continue;
                }

                if (entry.StartsWith($"{requiredPrefix}{folder}"))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
