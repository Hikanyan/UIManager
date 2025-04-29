using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.UIElements;

namespace HikanyanLaboratory.MVPStateTool
{
    public class MVPStateGeneratorWindow : EditorWindow
    {
        [SerializeField] private MVPStateGeneratorSettings _settings;

        private static string _settingsPath;
        private VisualElement _contentContainer;

        private List<WindowNodeInfo> _windowNodeInfos = new();
        private List<ScreenNodeInfo> _screenNodeInfos = new();

        private Dictionary<string, List<ScreenNodeInfo>> _screenNodeInfosByWindow = new();

        private string _selectedParentWindow;

        private ReorderableList _windowList;


        [MenuItem("HikanyanTools/MVP State Generator")]
        public static void ShowWindow()
        {
            GetWindow<MVPStateGeneratorWindow>("MVP State Generator");
        }

        private void OnEnable()
        {
            LoadOrCreateSettings();
            CreateUI();

            if (_settings != null)
            {
                MVPClassFactory.TemplatesFolderPath = _settings.TemplatesFolderPath;
                SetupWindowList();
            }
        }

        private void SetupWindowList()
        {
            _windowList = new ReorderableList(_settings.WindowGenerators, typeof(WindowData), true, true, true, true);

            _windowList.drawHeaderCallback = rect => { EditorGUI.LabelField(rect, "Window Generators"); };

            _windowList.drawElementCallback = (rect, index, isActive, isFocused) =>
            {
                var element = _settings.WindowGenerators[index];
                rect.y += 2;
                float halfWidth = rect.width / 2f;

                element.ScriptName = EditorGUI.TextField(
                    new Rect(rect.x, rect.y, halfWidth - 5, EditorGUIUtility.singleLineHeight),
                    element.ScriptName);

                element.Prefab = (GameObject)EditorGUI.ObjectField(
                    new Rect(rect.x + halfWidth + 5, rect.y, halfWidth - 5, EditorGUIUtility.singleLineHeight),
                    element.Prefab, typeof(GameObject), false);
            };

            _windowList.onAddCallback = list =>
            {
                int newIndex = _settings.WindowGenerators.Count + 1;
                _settings.WindowGenerators.Add(new WindowData
                {
                    ScriptName = $"NewWindow{newIndex:00}"
                });
                EditorUtility.SetDirty(_settings);
            };

            _windowList.onRemoveCallback = list =>
            {
                if (list.index >= 0 && list.index < _settings.WindowGenerators.Count)
                {
                    _settings.WindowGenerators.RemoveAt(list.index);
                    EditorUtility.SetDirty(_settings);
                }
            };
        }


        private void LoadOrCreateSettings()
        {
            var prefabGuids = AssetDatabase.FindAssets("t:MVPStateGeneratorSettings");
            MVPStateGeneratorSettings foundSettings = null;

            foreach (string guid in prefabGuids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                foundSettings = AssetDatabase.LoadAssetAtPath<MVPStateGeneratorSettings>(path);
                if (foundSettings != null)
                {
                    _settingsPath = path;
                    break;
                }
            }

            _settings = foundSettings;

            if (_settings == null)
            {
                Debug.LogWarning("設定ファイルが見つかりません。新規作成します。");
                _settings = CreateInstance<MVPStateGeneratorSettings>();
                _settingsPath = "Assets/UIToolSystem/MVPStateGeneratorSettings.asset";

                string directory = Path.GetDirectoryName(_settingsPath);
                if (!AssetDatabase.IsValidFolder(directory))
                {
                    CreateFolderRecursive(directory);
                }

                AssetDatabase.CreateAsset(_settings, _settingsPath);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }
        }

        private static void CreateFolderRecursive(string folderPath)
        {
            string[] parts = folderPath.Split('/');
            string currentPath = "";

            foreach (var part in parts)
            {
                currentPath = string.IsNullOrEmpty(currentPath) ? part : $"{currentPath}/{part}";

                if (!AssetDatabase.IsValidFolder(currentPath))
                {
                    string parent = Path.GetDirectoryName(currentPath)?.Replace("\\", "/");
                    string folderName = Path.GetFileName(currentPath);
                    AssetDatabase.CreateFolder(parent, folderName);
                }
            }
        }

        private void CreateUI()
        {
            rootVisualElement.Clear();

            var mainContainer = new VisualElement
            {
                style = { flexDirection = FlexDirection.Row }
            };
            rootVisualElement.Add(mainContainer);

            var sidebar = new VisualElement
            {
                style =
                {
                    width = 150,
                    minWidth = 150,
                    backgroundColor = new Color(0.15f, 0.15f, 0.15f),
                    paddingTop = 10
                }
            };
            mainContainer.Add(sidebar);

            _contentContainer = new VisualElement
            {
                style = { flexGrow = 1, paddingLeft = 10 }
            };
            mainContainer.Add(_contentContainer);

            var stateButton = new Button(() => ShowSettings("StateGenerator")) { text = "State Generator" };
            var windowButton = new Button(() => ShowSettings("WindowGenerator")) { text = "Window Generator" };
            var screenButton = new Button(() => ShowSettings("ScreenGenerator")) { text = "Screen Generator" };
            var settingsButton = new Button(() => ShowSettings("Settings")) { text = "⚙ Settings" };

            sidebar.Add(stateButton);
            sidebar.Add(windowButton);
            sidebar.Add(screenButton);
            sidebar.Add(new VisualElement { style = { height = 10 } });
            sidebar.Add(settingsButton);

            ShowSettings("StateGenerator");
        }

        private void ShowSettings(string category)
        {
            _contentContainer.Clear();

            var titleLabel = new Label($"{category} Settings")
            {
                style =
                {
                    unityFontStyleAndWeight = FontStyle.Bold,
                    fontSize = 16,
                    marginBottom = 10
                }
            };
            _contentContainer.Add(titleLabel);

            switch (category)
            {
                case "StateGenerator":
                    CreateReorderableList("Window State", _settings?.WindowStates, _contentContainer);
                    CreateReorderableList("Screen State", _settings?.ScreenStates, _contentContainer);

                    var generateButton = new Button(() =>
                    {
                        if (_settings == null)
                        {
                            Debug.LogWarning("設定ファイルが選択されていません。");
                            return;
                        }

                        string outputDir = _settings.OutputDirectory;
                        if (!Directory.Exists(outputDir))
                        {
                            Directory.CreateDirectory(outputDir);
                            AssetDatabase.Refresh();
                        }

                        MVPClassFactory.GenerateEnumClass(
                            "WindowState", _settings.WindowStates, outputDir, _settings.NameSpace);
                        MVPClassFactory.GenerateEnumClass(
                            "ScreenState", _settings.ScreenStates, outputDir, _settings.NameSpace);


                        Debug.Log("Enumファイルを生成しました！");
                        AssetDatabase.Refresh();
                    })
                    {
                        text = "Generate Enum"
                    };
                    _contentContainer.Add(generateButton);
                    break;
                case "WindowGenerator":
                    ShowWindowNodeGenerator();
                    break;
                case "ScreenGenerator":
                    ShowScreenNodeGenerator();
                    break;
                case "Settings":
                    ShowSettingsPage();
                    break;
            }
        }

        private void ShowSettingsPage()
        {
            _contentContainer.Clear();

            var titleLabel = new Label("Settings")
            {
                style =
                {
                    unityFontStyleAndWeight = FontStyle.Bold,
                    fontSize = 16,
                    marginBottom = 10
                }
            };
            _contentContainer.Add(titleLabel);

            var row = new VisualElement
            {
                style = { flexDirection = FlexDirection.Row }
            };
            _contentContainer.Add(row);

            var settingsField = new ObjectField
            {
                objectType = typeof(MVPStateGeneratorSettings),
                value = _settings,
                style = { flexGrow = 1 }
            };
            settingsField.RegisterValueChangedCallback(evt =>
            {
                _settings = evt.newValue as MVPStateGeneratorSettings;
            });
            row.Add(settingsField);

            var createButton = new Button(() =>
            {
                CreateNewSettings();
                settingsField.value = _settings;
            })
            {
                text = "＋Create",
                style = { width = 80, marginLeft = 4 }
            };
            row.Add(createButton);
            ShowGeneralSettings();
        }

        private void ShowGeneralSettings()
        {
            if (_settings == null)
            {
                var warningLabel = new Label("⚠ 設定ファイルが未設定です");
                _contentContainer.Add(warningLabel);
                return;
            }

            // Output Directory (D&D対応版)
            var outputDirField = new ObjectField("Output Directory")
            {
                objectType = typeof(DefaultAsset),
                allowSceneObjects = false,
                value = AssetDatabase.LoadAssetAtPath<DefaultAsset>(_settings.OutputDirectory)
            };
            outputDirField.RegisterValueChangedCallback(evt =>
            {
                var selected = evt.newValue as DefaultAsset;
                if (selected != null)
                {
                    string path = AssetDatabase.GetAssetPath(selected);
                    if (AssetDatabase.IsValidFolder(path))
                    {
                        _settings.OutputDirectory = path;
                        EditorUtility.SetDirty(_settings);
                    }
                    else
                    {
                        Debug.LogWarning("フォルダ以外が選択されました");
                    }
                }
            });
            _contentContainer.Add(outputDirField);

            // Templates Folder (D&D対応版)
            var templatesFolderField = new ObjectField("Templates Folder")
            {
                objectType = typeof(DefaultAsset),
                allowSceneObjects = false,
                value = AssetDatabase.LoadAssetAtPath<DefaultAsset>(_settings.TemplatesFolderPath)
            };
            templatesFolderField.RegisterValueChangedCallback(evt =>
            {
                var selected = evt.newValue as DefaultAsset;
                if (selected != null)
                {
                    string path = AssetDatabase.GetAssetPath(selected);
                    if (AssetDatabase.IsValidFolder(path))
                    {
                        _settings.TemplatesFolderPath = path;
                        MVPClassFactory.TemplatesFolderPath = path;
                        EditorUtility.SetDirty(_settings);
                    }
                    else
                    {
                        Debug.LogWarning("フォルダ以外が選択されました");
                    }
                }
            });
            _contentContainer.Add(templatesFolderField);

            // NameSpace
            var namespaceField = new TextField("Namespace")
            {
                value = _settings.NameSpace
            };
            namespaceField.RegisterValueChangedCallback(evt =>
            {
                _settings.NameSpace = evt.newValue;
                EditorUtility.SetDirty(_settings);
            });
            _contentContainer.Add(namespaceField);

            // Window用テンプレートPrefab
            var windowTemplatePrefabField = new ObjectField("Window Template Prefab")
            {
                objectType = typeof(GameObject),
                allowSceneObjects = false,
                value = _settings.WindowTemplatePrefab
            };
            windowTemplatePrefabField.RegisterValueChangedCallback(evt =>
            {
                _settings.WindowTemplatePrefab = evt.newValue as GameObject;
                EditorUtility.SetDirty(_settings);
            });
            _contentContainer.Add(windowTemplatePrefabField);

            // Screen用テンプレートPrefab
            var screenTemplatePrefabField = new ObjectField("Screen Template Prefab")
            {
                objectType = typeof(GameObject),
                allowSceneObjects = false,
                value = _settings.ScreenTemplatePrefab
            };
            screenTemplatePrefabField.RegisterValueChangedCallback(evt =>
            {
                _settings.ScreenTemplatePrefab = evt.newValue as GameObject;
                EditorUtility.SetDirty(_settings);
            });
            _contentContainer.Add(screenTemplatePrefabField);
        }


        private void ShowWindowNodeGenerator()
        {
            _contentContainer.Clear();

            var container = new IMGUIContainer(() => { _windowList?.DoLayoutList(); });
            _contentContainer.Add(container);

            var generateButton = new Button(GenerateWindowScripts)
            {
                text = "Generate Window Scripts"
            };
            _contentContainer.Add(generateButton);
        }

        private void GenerateWindowScripts()
        {
            if (_settings == null) return;

            MVPClassFactory.GenerateScriptsAndPrefabs<WindowNodeInfo, WindowData>(
                _windowNodeInfos,
                _settings.OutputDirectory,
                _settings.NameSpace,
                _settings.WindowTemplatePrefab,
                _settings.WindowGenerators
            );


            EditorUtility.SetDirty(_settings);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("Windowノードのスクリプトを生成＆保存しました！");
        }


        private void CreateNewSettings()
        {
            _settings = CreateInstance<MVPStateGeneratorSettings>();
            string path = EditorUtility.SaveFilePanelInProject(
                "Save MVP State Generator Settings",
                "MVPStateGeneratorSettings",
                "asset",
                "保存する場所を選んでください");

            if (!string.IsNullOrEmpty(path))
            {
                string directory = Path.GetDirectoryName(path);
                if (!AssetDatabase.IsValidFolder(directory))
                {
                    CreateFolderRecursive(directory);
                }

                AssetDatabase.CreateAsset(_settings, path);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                _settingsPath = path;
            }
        }

        private static void CreateReorderableList(string label, List<string> list, VisualElement parent)
        {
            if (list == null) return;

            var container = new VisualElement();
            parent.Add(container);

            var listLabel = new Label(label)
            {
                style =
                {
                    unityFontStyleAndWeight = FontStyle.Bold,
                    fontSize = 12,
                    marginBottom = 5
                }
            };
            container.Add(listLabel);

            var listView = new ListView(list, 20, () => new TextField(), (element, index) =>
            {
                var textField = (TextField)element;
                textField.value = list[index];
                textField.RegisterValueChangedCallback(evt => list[index] = evt.newValue);
            })
            {
                showAddRemoveFooter = true,
                reorderable = true,
                showBorder = true
            };

            container.Add(listView);
        }

        private void ShowScreenNodeGenerator()
        {
            if (_settings == null)
            {
                _contentContainer.Add(new Label("⚠ 設定ファイルが未設定です"));
                return;
            }

            _screenNodeInfosByWindow.Clear();
            _contentContainer.Clear();

            Type windowStateType = Type.GetType($"{_settings.NameSpace}.WindowState");
            if (windowStateType == null)
            {
                _contentContainer.Add(new Label("⚠ WindowState Enumが見つかりません"));
                return;
            }

            var windowNames = Enum.GetNames(windowStateType);

            var windowDropdown = new PopupField<string>(new List<string>(windowNames), 0);
            windowDropdown.RegisterValueChangedCallback(evt =>
            {
                _selectedParentWindow = evt.newValue;
                RefreshScreenNodeList();
            });
            _contentContainer.Add(windowDropdown);

            _selectedParentWindow = windowDropdown.value;

            // 各Window用の空リスト初期化
            foreach (var windowName in windowNames)
            {
                _screenNodeInfosByWindow[windowName] = new List<ScreenNodeInfo>();
            }

            // リストを入れる専用コンテナ
            var listArea = new VisualElement { name = "ScreenListArea" };
            _contentContainer.Add(listArea);

            // Generateボタンを別で用意（常に一個だけ）
            var generateButton = new Button(GenerateScreenScripts)
            {
                text = "Generate Screen Scripts"
            };
            _contentContainer.Add(generateButton);

            // 最初にリスト表示
            RefreshScreenNodeList();
        }


        private void RefreshScreenNodeList()
        {
            var listArea = _contentContainer.Q<VisualElement>(name: "ScreenListArea");
            if (listArea == null) return;

            listArea.Clear(); // リストエリアだけクリアする

            if (string.IsNullOrEmpty(_selectedParentWindow)) return;

            if (!_screenNodeInfosByWindow.ContainsKey(_selectedParentWindow))
            {
                _screenNodeInfosByWindow[_selectedParentWindow] = new List<ScreenNodeInfo>();
            }

            var currentList = _screenNodeInfosByWindow[_selectedParentWindow];

            var listView = new ListView(currentList, 30,
                () => CreateScreenNodeRow(),
                (element, index) => BindScreenNodeRow(element, currentList[index]))
            {
                reorderable = true,
                showAddRemoveFooter = true,
                showBorder = true
            };
            listArea.Add(listView);

            listView.itemsAdded += indices =>
            {
                foreach (var index in indices)
                {
                    if (index >= 0 && index < currentList.Count)
                    {
                        currentList[index] = new ScreenNodeInfo
                        {
                            GenerateEnum = true,
                            GenerateScript = true,
                            EnumName = "NewScreen",
                            ScriptName = "NewScreen"
                        };
                    }
                }

                listView.RefreshItems();
            };
        }


        private VisualElement CreateScreenNodeRow()
        {
            var row = new VisualElement { style = { flexDirection = FlexDirection.Row, alignItems = Align.Center } };

            var enumField = new TextField { style = { width = 120 } };
            row.Add(enumField);

            var scriptField = new TextField { style = { flexGrow = 1 } };
            row.Add(scriptField);

            var prefabField = new ObjectField
            {
                objectType = typeof(GameObject),
                allowSceneObjects = false,
                style = { width = 150 }
            };
            row.Add(prefabField);

            var generateToggle = new Toggle("Script") { value = true, style = { width = 60 } };
            row.Add(generateToggle);

            return row;
        }

        private void BindScreenNodeRow(VisualElement element, ScreenNodeInfo data)
        {
            ((TextField)element[0]).value = data.EnumName;
            ((TextField)element[0]).RegisterValueChangedCallback(evt => data.EnumName = evt.newValue);

            ((TextField)element[1]).value = data.ScriptName;
            ((TextField)element[1]).RegisterValueChangedCallback(evt => data.ScriptName = evt.newValue);

            ((ObjectField)element[2]).value = data.Prefab;
            ((ObjectField)element[2]).RegisterValueChangedCallback(evt => data.Prefab = evt.newValue as GameObject);

            ((Toggle)element[3]).value = data.GenerateScript;
            ((Toggle)element[3]).RegisterValueChangedCallback(evt => data.GenerateScript = evt.newValue);
        }

        private void GenerateScreenScripts()
        {
            if (_settings == null) return;

            if (string.IsNullOrEmpty(_selectedParentWindow)) return;
            if (!_screenNodeInfosByWindow.TryGetValue(_selectedParentWindow, out var screenNodes)) return;

            MVPClassFactory.GenerateScriptsAndPrefabs<ScreenNodeInfo, ScreenData>(
                screenNodes,
                _settings.OutputDirectory,
                _settings.NameSpace,
                _settings.ScreenTemplatePrefab,
                _settings.ScreenGenerators
            );

            EditorUtility.SetDirty(_settings);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("Screenノードのスクリプトを生成＆保存しました！");
        }
    }
}