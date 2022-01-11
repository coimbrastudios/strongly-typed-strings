using JetBrains.Annotations;
using UnityEditor.SettingsManagement;
using UnityEngine;

namespace Coimbra.StronglyTypedStrings.DefaultUnits
{
    /// <summary>
    /// Generates strongly-typed paths for the contents of all special "Resources" folders in the project.
    /// </summary>
    [GeneratorExtension, UsedImplicitly]
    public sealed class ResourcesGeneratorUnit : SpecialFolderContentGeneratorUnit
    {
        private static readonly string[] DefaultFoldersToScan = { "Assets" };

        [UserSetting] private static readonly GeneratorSetting<bool> ScanAllFoldersSetting = new GeneratorSetting<bool>("resources.scanAllFolders", false);
        [UserSetting] private static readonly GeneratorSetting<string[]> FoldersToScanSetting = new GeneratorSetting<string[]>("resources.foldersToScan", DefaultFoldersToScan);

        /// <summary>
        /// The label to be displayed on the Project Settings menu.
        /// </summary>
        public override GUIContent Label { get; } = new GUIContent("Resources", "Generator for any Resources folder inside your project (including Packages).");

        protected override bool HideExtension { get; } = true;
        protected override string SpecialFolderName { get; } = "Resources";
        protected override string Type { get; } = "Resource";

        /// <summary>
        /// This method is called automatically by the <see cref="Generator"/> if the <see cref="GeneratorExtensionAttribute"/> is present.
        /// </summary>
        public override void DrawSettings(string searchContext)
        {
            DrawContentSettings(searchContext, ScanAllFoldersSetting, FoldersToScanSetting);
        }

        protected override bool IsValidEntry(string entry)
        {
            if (entry.Contains($"/{SpecialFolderName}/") == false)
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

                if (entry.StartsWith(folder))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
