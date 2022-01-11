using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SettingsManagement;
using UnityEditorInternal;
using UnityEngine;

namespace Coimbra.StronglyTypedStrings
{
    /// <summary>
    /// Specialized <see cref="GeneratorUnit"/> class to expose strongly-typed paths to the content inside special folders across your project.
    /// </summary>
    public abstract class SpecialFolderContentGeneratorUnit : GeneratorUnit
    {
        private const string FoldersToScanToolTip = "Add here the folders which the generator should search for content.";

        private static readonly GUIContent FoldersToScanLabel = new GUIContent("Folders To Scan", FoldersToScanToolTip);

        private readonly ReorderableList FoldersToScanDrawer = new ReorderableList(null, typeof(string));

        /// <summary>
        /// The special folder name.
        /// </summary>
        protected abstract string SpecialFolderName { get; }
        /// <summary>
        /// Should the file extension be hidden?
        /// </summary>
        protected virtual bool HideExtension { get; } = false;
        /// <summary>
        /// Customize the order in the Project Settings menu.
        /// </summary>
        protected override int Priority { get; } = 100;

        /// <summary>
        /// Draw both "Scan All Folders" toggle and "Folders To Scan" list in the editor.
        /// </summary>
        /// <param name="searchContext"></param>
        /// <param name="scanAllFolders"></param>
        /// <param name="foldersToScan"></param>
        protected void DrawContentSettings(string searchContext, GeneratorSetting<bool> scanAllFolders, GeneratorSetting<string[]> foldersToScan)
        {
            scanAllFolders.value = SettingsGUILayout.SettingsToggle("Scan All Folders", scanAllFolders, searchContext);

            if (scanAllFolders || GeneratorUtility.TryMatchSearch(searchContext, FoldersToScanLabel.text) == false)
            {
                return;
            }

            if (AreEqual(foldersToScan.value, FoldersToScanDrawer.list) == false)
            {
                FoldersToScanDrawer.list = foldersToScan.value.ToList();
                FoldersToScanDrawer.onAddCallback = list => list.list.Add(string.Empty);
                FoldersToScanDrawer.onChangedCallback = list => foldersToScan.SetValue(list.list.Cast<string>().ToArray(), true);
                FoldersToScanDrawer.drawHeaderCallback = rect => EditorGUI.LabelField(rect, FoldersToScanLabel);

                FoldersToScanDrawer.drawElementCallback = (rect, index, active, focused) =>
                {
                    using (var changeCheckScope = new EditorGUI.ChangeCheckScope())
                    {
                        string value = EditorGUI.DelayedTextField(rect, GUIContent.none, (string)FoldersToScanDrawer.list[index]);

                        if (changeCheckScope.changed)
                        {
                            FoldersToScanDrawer.list[index] = GeneratorUtility.GetValidFolderPath(value);
                            foldersToScan.value = FoldersToScanDrawer.list.Cast<string>().ToArray();
                        }
                    }
                };
            }

            FoldersToScanDrawer.DoLayoutList();
            SettingsGUILayout.DoResetContextMenuForLastRect(foldersToScan);
        }

        /// <summary>
        /// Implement this to provide your own validation logic for the files to be included in the generated file.
        /// </summary>
        /// <param name="entry">The current file being scanned.</param>
        protected abstract bool IsValidEntry(string entry);

        /// <summary>
        /// Implement your generator logic here.
        /// </summary>
        protected override void OnGenerate()
        {
            var names = new List<string>();
            var values = new List<string>();
            var duplicates = new List<int>();
            var paths = new HashSet<string>();
            string separator = $"/{SpecialFolderName}/";

            foreach (string assetPath in AssetDatabase.GetAllAssetPaths())
            {
                if (IsValidEntry(assetPath) == false)
                {
                    continue;
                }

                int removeCount = assetPath.IndexOf(separator, StringComparison.Ordinal) + separator.Length;
                string path = assetPath.Remove(0, removeCount);
                string name = Path.GetFileNameWithoutExtension(path);
                string value = HideExtension ? path.Replace(Path.GetFileName(path), name) : path;

                if (names.Contains(name))
                {
                    if (paths.Contains(path))
                    {
                        continue;
                    }

                    if (values.Contains(value))
                    {
                        continue;
                    }

                    if (duplicates.Count == 0)
                    {
                        duplicates.Add(names.IndexOf(name));
                    }

                    duplicates.Add(names.Count);
                }

                names.Add(name);
                values.Add(value);
                paths.Add(path);
            }

            foreach (int i in duplicates)
            {
                names[i] = values[i];

                if (HideExtension)
                {
                    continue;
                }

                string extension = Path.GetExtension(values[i]);

                if (string.IsNullOrWhiteSpace(extension))
                {
                    continue;
                }

                names[i] = names[i].Replace(extension, $"_{extension}");
            }

            int length = Mathf.Min(names.Count, values.Count);

            for (int i = 0; i < length; i++)
            {
                Append(GeneratorUtility.GetValidMemberName(names[i]), values[i]);
            }
        }

        private static bool AreEqual(string[] source, IList comparer)
        {
            if (comparer == null || source.Length != comparer.Count)
            {
                return false;
            }

            for (var i = 0; i < source.Length; i++)
            {
                if (source[i] != (string)comparer[i])
                {
                    return false;
                }
            }

            return true;
        }
    }
}
