using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SettingsManagement;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.Assertions;

namespace Coimbra.StronglyTypedStrings
{
    internal static class Generator
    {
        private static class ProgressBar
        {
            internal const string GeneratingTitle = "Generating files...";
            internal const string MovingTitle = "Moving files...";
            internal const string Info = "It may take a few seconds.";
        }

        private const string GenerateAllLabel = "Generate All";
        private const string MenuItemName = "Tools/Coimbra/Generate Strongly Typed Strings";
        private const string PackageName = "com.coimbra.stronglytypedstrings";
        private const string UserSettingsPath = "Project/Strongly Typed Strings";

        private static readonly GUIContent AssemblyDefinitionFileLabel = new GUIContent("Assembly Definition File*", "You can choose to generate the files within an existing assembly inside the Assets folder.");
        private static readonly GUIContent AssemblyRelativeFolderLabel = new GUIContent("Assembly Relative Folder*", "The Assembly-relative folder to place the files.");
        private static readonly GUIContent AssetsRelativeFolderLabel = new GUIContent("Assets Relative Folder*", "The Assets-relative folder to place the files.");
        private static readonly GUIContent DummyLabel = new GUIContent(" ");
        private static readonly GUIContent GenerateButtonLabel = new GUIContent("Generate");
        private static readonly GUIContent LineEndingsLabel = new GUIContent("Line Endings*", "Select which line endings to use in the generated files.");
        private static readonly GUIContent NamespaceLabel = new GUIContent("Namespace*", "Set a namespace for the generated types.");
        private static readonly char[] FolderSeparator = { '/' };
        private static readonly List<GeneratorUnit> FileGenerators;
        private static readonly Dictionary<LineEndingsMode, string> LineEndings;

        [UserSetting] private static readonly GeneratorSetting<string> AssemblyDefinitionFileGuidSetting = new GeneratorSetting<string>("general.assemblyDefinitionFileGuid", "");
        [UserSetting] private static readonly GeneratorSetting<string> AssemblyRelativeFolderSetting = new GeneratorSetting<string>("general.assemblyRelativeFolder", "Generated");
        [UserSetting] private static readonly GeneratorSetting<string> NamespaceSetting = new GeneratorSetting<string>("general.namespace", "");
        [UserSetting] private static readonly GeneratorSetting<LineEndingsMode> LineEndingsSetting = new GeneratorSetting<LineEndingsMode>("general.lineEndings", LineEndingsMode.OSNative);

        private static string _previousFolder;
        private static Settings s_Instance;
        private static UserSettingsProvider _provider;

        static Generator()
        {
#if UNITY_2019_2_OR_NEWER
            TypeCache.TypeCollection types = TypeCache.GetTypesWithAttribute<GeneratorExtensionAttribute>();
            FileGenerators = new List<GeneratorUnit>(types.Count);

            foreach (Type type in types)
            {
                if (typeof(GeneratorUnit).IsAssignableFrom(type))
                {
                    FileGenerators.Add(Activator.CreateInstance(type) as GeneratorUnit);
                }
            }
#else
            FileGenerators = new List<GeneratorUnit>();

            foreach (System.Reflection.Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                foreach (Type type in assembly.GetTypes())
                {
                    if (typeof(GeneratorUnit).IsAssignableFrom(type) && System.Reflection.CustomAttributeExtensions.GetCustomAttribute<GeneratorExtensionAttribute>(type) != null)
                    {
                        FileGenerators.Add(Activator.CreateInstance(type) as GeneratorUnit);
                    }
                }
            }
#endif
            FileGenerators.Sort(new GeneratorUnit.LabelComparer());
            FileGenerators.Sort(new GeneratorUnit.PriorityComparer());

            LineEndings = new Dictionary<LineEndingsMode, string>
            {
                [LineEndingsMode.OSNative] = Environment.NewLine,
                [LineEndingsMode.Unix] = "\n",
                [LineEndingsMode.Windows] = "\r\n"
            };
        }

        internal static Settings Instance => s_Instance ?? (s_Instance = new Settings(PackageName));

        private static string LineEnding => LineEndings[LineEndingsSetting];

        [MenuItem(MenuItemName, true)]
        private static bool CanGenerateFiles()
        {
            return !EditorApplication.isPlaying && !EditorApplication.isCompiling;
        }

        [MenuItem(MenuItemName)]
        private static void GenerateFiles()
        {
            Assert.IsTrue(CanGenerateFiles());

            try
            {
                string fileFolder = GetFileFolder(false);
                EnsureDirectoryExists(fileFolder);
                fileFolder = GetFileFolder(fileFolder, true);
                EditorUtility.DisplayCancelableProgressBar(ProgressBar.GeneratingTitle, ProgressBar.Info, 0f);

                for (int i = 0; i < FileGenerators.Count; i++)
                {
                    float progress = Mathf.InverseLerp(0, FileGenerators.Count, i + 1);

                    if (EditorUtility.DisplayCancelableProgressBar(ProgressBar.GeneratingTitle, ProgressBar.Info, progress))
                    {
                        break;
                    }

                    FileGenerators[i].Generate(fileFolder, NamespaceSetting, LineEnding, false);
                }
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
            }

            EditorUtility.ClearProgressBar();
            AssetDatabase.Refresh();
        }

        [SettingsProvider]
        private static SettingsProvider CreateGeneratorProvider()
        {
            return _provider ?? (_provider = new UserSettingsProvider(UserSettingsPath, Instance, new[] { typeof(Generator).Assembly }, SettingsScope.Project));
        }

        [UserSettingBlock(" ")]
        private static void DrawMainBlock(string searchContext)
        {
            if (!GeneratorUtility.TryMatchSearch(searchContext, GenerateAllLabel))
            {
                return;
            }

            using (new EditorGUI.DisabledScope(!CanGenerateFiles()))
            {
                if (GUILayout.Button(GenerateAllLabel))
                {
                    GenerateFiles();
                }
            }
        }

        [UserSettingBlock("General")]
        private static void DrawGeneralBlock(string searchContext)
        {
            using (var blockScope = new EditorGUI.ChangeCheckScope())
            {
                using (new EditorGUI.DisabledScope(!CanGenerateFiles()))
                {
                    DrawAssemblyDefinitionFileGuidField(searchContext);
                    DrawAssemblyRelativeFolderField(searchContext);
                }

                UpdateFolder();
                DrawNamespaceField(searchContext);
                DrawLineEndingsField(searchContext);
                DrawGenerators(searchContext);

                if (blockScope.changed)
                {
                    Instance.Save();
                }
            }
        }

        private static void DrawAssemblyDefinitionFileGuidField(string searchContext)
        {
            if (!GeneratorUtility.TryMatchSearch(searchContext, AssemblyDefinitionFileLabel.text))
            {
                return;
            }

            using (var fieldScope = new EditorGUI.ChangeCheckScope())
            {
                AssemblyDefinitionAsset asset = null;

                if (!string.IsNullOrWhiteSpace(AssemblyDefinitionFileGuidSetting))
                {
                    string assetPath = AssetDatabase.GUIDToAssetPath(AssemblyDefinitionFileGuidSetting);
                    asset = AssetDatabase.LoadAssetAtPath<AssemblyDefinitionAsset>(assetPath);
                }

                var value = (AssemblyDefinitionAsset)EditorGUILayout.ObjectField(AssemblyDefinitionFileLabel, asset, typeof(AssemblyDefinitionAsset), false);

                SettingsGUILayout.DoResetContextMenuForLastRect(AssemblyDefinitionFileGuidSetting);

                if (!fieldScope.changed)
                {
                    return;
                }

                string from = GetFileFolder(false);

                if (value == null)
                {
                    AssemblyDefinitionFileGuidSetting.value = "";
                }
                else
                {
                    string path = AssetDatabase.GetAssetPath(value);

                    AssemblyDefinitionFileGuidSetting.value = path.StartsWith("Assets/") ? AssetDatabase.AssetPathToGUID(path) : "";
                }

                string to = GetFileFolder(false);

                MoveGeneratedFiles(from, to);

                _previousFolder = to;
            }
        }

        private static void DrawAssemblyRelativeFolderField(string searchContext)
        {
            GUIContent assemblyRelativeFolderLabel = string.IsNullOrWhiteSpace(AssemblyDefinitionFileGuidSetting)
                                                         ? AssetsRelativeFolderLabel
                                                         : AssemblyRelativeFolderLabel;

            if (!GeneratorUtility.TryMatchSearch(searchContext, assemblyRelativeFolderLabel.text))
            {
                return;
            }

            using (new EditorGUI.IndentLevelScope())
            {
                using (var fieldScope = new EditorGUI.ChangeCheckScope())
                {
                    string value = EditorGUILayout.DelayedTextField(assemblyRelativeFolderLabel, AssemblyRelativeFolderSetting);

                    SettingsGUILayout.DoResetContextMenuForLastRect(AssemblyRelativeFolderSetting);

                    if (fieldScope.changed == false)
                    {
                        return;
                    }

                    string from = GetFileFolder(false);

                    AssemblyRelativeFolderSetting.value = GeneratorUtility.GetValidFolderPath(value);

                    string to = GetFileFolder(false);

                    MoveGeneratedFiles(from, to);

                    _previousFolder = to;
                }
            }
        }

        private static void DrawGenerators(string searchContext)
        {
            float buttonWidth = GUI.skin.button.CalcSize(GenerateButtonLabel).x;
            string fileFolder = GetFileFolder(false);

            foreach (GeneratorUnit fileGenerator in FileGenerators)
            {
                if (string.IsNullOrWhiteSpace(searchContext))
                {
                    EditorGUILayout.Separator();
                }

                if (GeneratorUtility.TryMatchSearch(searchContext, GenerateButtonLabel.text)
                 || GeneratorUtility.TryMatchSearch(searchContext, fileGenerator.Label.text))
                {
                    Rect totalPosition = EditorGUILayout.GetControlRect();
                    Rect currentPosition = EditorGUI.PrefixLabel(totalPosition, DummyLabel, EditorStyles.boldLabel);
                    currentPosition.width -= buttonWidth - 16;

                    string scriptPath = $"{fileFolder}/{fileGenerator.FileName}";
                    var script = AssetDatabase.LoadAssetAtPath<MonoScript>(scriptPath);

                    if (script != null)
                    {
                        using (new EditorGUI.DisabledScope(true))
                        {
                            EditorGUI.ObjectField(currentPosition, script, typeof(MonoScript), false);
                        }
                    }

                    currentPosition.x += currentPosition.width - 16;
                    currentPosition.width = buttonWidth;

                    EditorGUI.LabelField(totalPosition, fileGenerator.Label, EditorStyles.boldLabel);

                    using (new EditorGUI.DisabledScope(!CanGenerateFiles()))
                    {
                        if (GUI.Button(currentPosition, GenerateButtonLabel, EditorStyles.miniButton))
                        {
                            EnsureDirectoryExists(fileFolder);
                            string fileFolderWithDataPath = GetFileFolder(true);
                            fileGenerator.Generate(fileFolderWithDataPath, NamespaceSetting, LineEnding, true);
                        }
                    }
                }

                fileGenerator.DrawSettings(searchContext);
            }
        }

        private static void DrawLineEndingsField(string searchContext)
        {
            if (GeneratorUtility.TryMatchSearch(searchContext, LineEndingsLabel.text))
            {
                LineEndingsSetting.value = (LineEndingsMode)EditorGUILayout.EnumPopup(LineEndingsLabel, LineEndingsSetting);

                SettingsGUILayout.DoResetContextMenuForLastRect(LineEndingsSetting);
            }
        }

        private static void DrawNamespaceField(string searchContext)
        {
            using (var fieldScope = new EditorGUI.ChangeCheckScope())
            {
                string value = SettingsGUILayout.SettingsTextField(NamespaceLabel, NamespaceSetting, searchContext);

                if (fieldScope.changed)
                {
                    NamespaceSetting.value = GeneratorUtility.GetValidNamespaceName(value);
                }
            }
        }

        private static void EnsureDirectoryExists(string directory)
        {
            if (AssetDatabase.IsValidFolder(directory))
            {
                return;
            }

            string[] folders = directory.Split(FolderSeparator, StringSplitOptions.RemoveEmptyEntries);
            string path = folders[0];

            for (int i = 1; i < folders.Length; i++)
            {
                string parent = path;
                path += $"/{folders[i]}";

                if (!AssetDatabase.IsValidFolder(path))
                {
                    AssetDatabase.CreateFolder(parent, folders[i]);
                }
            }
        }

        private static string GetFileFolder(bool withDataPath)
        {
            var fileFolder = "Assets";

            if (!string.IsNullOrWhiteSpace(AssemblyDefinitionFileGuidSetting))
            {
                string path = AssetDatabase.GUIDToAssetPath(AssemblyDefinitionFileGuidSetting);

                if (!string.IsNullOrEmpty(path))
                {
                    fileFolder = $"{Path.GetDirectoryName(path)}";
                }
            }

            if (!string.IsNullOrWhiteSpace(AssemblyRelativeFolderSetting))
            {
                fileFolder += $"/{AssemblyRelativeFolderSetting.value}";
            }

            return GetFileFolder(fileFolder, withDataPath);
        }

        private static string GetFileFolder(string fileFolder, bool withDataPath)
        {
            string dataPath = Application.dataPath.Replace("\\", "/");

            return withDataPath ? $"{dataPath}{fileFolder.Remove(0, "Assets".Length)}" : fileFolder;
        }

        private static void MoveGeneratedFiles(string from, string to)
        {
            try
            {
                EditorUtility.DisplayCancelableProgressBar(ProgressBar.MovingTitle, ProgressBar.Info, 0f);

                for (var i = 0; i < FileGenerators.Count; i++)
                {
                    float progress = Mathf.InverseLerp(0, FileGenerators.Count, i);

                    if (EditorUtility.DisplayCancelableProgressBar(ProgressBar.MovingTitle, ProgressBar.Info, progress))
                    {
                        break;
                    }

                    GeneratorUnit fileGenerator = FileGenerators[i];
                    string oldPath = $"{from}/{fileGenerator.FileName}";

                    if (AssetDatabase.LoadAssetAtPath<MonoScript>(oldPath) == null)
                    {
                        continue;
                    }

                    string newPath = $"{to}/{fileGenerator.FileName}";

                    EnsureDirectoryExists(to);

                    string response = AssetDatabase.ValidateMoveAsset(oldPath, newPath);

                    if (string.IsNullOrWhiteSpace(response))
                    {
                        AssetDatabase.MoveAsset(oldPath, newPath);
                    }
                    else
                    {
                        Debug.LogError(response);
                    }
                }

                if (Directory.Exists(from) && !EditorUtility.DisplayCancelableProgressBar(ProgressBar.MovingTitle, ProgressBar.Info, 1))
                {
                    string directory = Path.GetFullPath(from);

                    do
                    {
                        if (Directory.GetFiles(directory).Length > 0)
                        {
                            break;
                        }

                        Directory.Delete(directory);
                        File.Delete($"{directory}.meta");

                        directory = Path.GetDirectoryName(directory);
                    } while (Directory.Exists(directory));
                }
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
            }

            EditorUtility.ClearProgressBar();
            AssetDatabase.Refresh();
        }

        private static void UpdateFolder()
        {
            if (!CanGenerateFiles())
            {
                return;
            }

            string currentFolder = GetFileFolder(false);

            if (!string.IsNullOrWhiteSpace(_previousFolder) && _previousFolder != currentFolder)
            {
                MoveGeneratedFiles(_previousFolder, currentFolder);
            }

            _previousFolder = currentFolder;
        }
    }
}
