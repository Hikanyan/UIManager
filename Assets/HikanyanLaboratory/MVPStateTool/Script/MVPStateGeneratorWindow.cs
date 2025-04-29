using System;
using System.Collections.Generic;
using System.IO;
using Unity.VisualScripting;
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
        private VisualElement _screenListContainer;

        private List<WindowNodeInfo> _windowNodeInfos = new();
        private List<ScreenNodeInfo> _screenNodeInfos = new();

        private Dictionary<string, List<ScreenNodeInfo>> _screenNodeInfosByWindow = new();

        private string _selectedParentWindow;

        private ReorderableList _windowList;
        private ReorderableList _screenList;


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
            // WindowNodeInfo にバインドするよう修正！
            _windowList = new ReorderableList(_windowNodeInfos, typeof(WindowNodeInfo), true, true, true, true);

            _windowList.drawHeaderCallback = rect => { EditorGUI.LabelField(rect, "Window Generators"); };

            _windowList.drawElementCallback = (rect, index, isActive, isFocused) =>
            {
                var element = _windowNodeInfos[index];
                rect.y += 2;
                float widthUnit = rect.width / 3f;

                // ScriptName
                element.ScriptName = EditorGUI.TextField(
                    new Rect(rect.x, rect.y, widthUnit - 5, EditorGUIUtility.singleLineHeight),
                    element.ScriptName);

                // Prefab
                element.Prefab = (GameObject)EditorGUI.ObjectField(
                    new Rect(rect.x + widthUnit + 5, rect.y, widthUnit - 5, EditorGUIUtility.singleLineHeight),
                    element.Prefab, typeof(GameObject), false);

                // IsGenerated Toggle（WindowNodeInfoには GenerateScript が該当）
                element.GenerateScript = EditorGUI.Toggle(
                    new Rect(rect.x + (widthUnit * 2) + 10, rect.y, 20, EditorGUIUtility.singleLineHeight),
                    element.GenerateScript);
            };

            _windowList.onAddCallback = list =>
            {
                _windowNodeInfos.Add(new WindowNodeInfo
                {
                    GenerateEnum = true,
                    GenerateScript = true,
                    ScriptName = $"NewWindow{_windowNodeInfos.Count + 1:00}"
                });

                SaveWindowNodeInfos();
            };

            _windowList.onRemoveCallback = list =>
            {
                if (list.index >= 0 && list.index < _windowNodeInfos.Count)
                {
                    _windowNodeInfos.RemoveAt(list.index);
                    SaveWindowNodeInfos();
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

            _windowNodeInfos.Clear();
            foreach (var windowData in _settings.WindowGenerators)
            {
                _windowNodeInfos.Add(new WindowNodeInfo
                {
                    GenerateEnum = true,
                    GenerateScript = true,
                    EnumName = windowData.ScriptName,
                    ScriptName = windowData.ScriptName,
                    Prefab = windowData.Prefab
                });
            }

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

            // 生成成功後、全部GenerateScript=falseにする
            foreach (var node in _windowNodeInfos)
            {
                node.GenerateScript = false;
            }

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
            _contentContainer.Clear();

            // WindowState一覧取得
            Type windowStateType = Type.GetType($"{_settings.NameSpace}.WindowState");
            if (windowStateType == null)
            {
                _contentContainer.Add(new Label("⚠ WindowState Enumが見つかりません"));
                return;
            }

            var windowNames = Enum.GetNames(windowStateType);

            // PopupFieldで選択
            var windowDropdown = new PopupField<string>(new List<string>(windowNames), 0);
            windowDropdown.RegisterValueChangedCallback(evt =>
            {
                _selectedParentWindow = evt.newValue;
                SetupScreenList();
            });
            _contentContainer.Add(windowDropdown);

            _selectedParentWindow = windowDropdown.value;

            _screenNodeInfosByWindow.Clear();
            foreach (var windowName in windowNames)
            {
                _screenNodeInfosByWindow[windowName] = new List<ScreenNodeInfo>();
            }

            foreach (var group in _settings.ScreenGeneratorsByWindow)
            {
                if (!_screenNodeInfosByWindow.ContainsKey(group.ParentWindowName))
                {
                    _screenNodeInfosByWindow[group.ParentWindowName] = new List<ScreenNodeInfo>();
                }

                foreach (var screen in group.Screens)
                {
                    _screenNodeInfosByWindow[group.ParentWindowName].Add(new ScreenNodeInfo
                    {
                        ScriptName = screen.ScriptName,
                        Prefab = screen.Prefab,
                        GenerateEnum = true,
                        GenerateScript = true,
                        IsGenerated = screen.IsGenerated
                    });
                }
            }

            // リストエリアとボタンを配置
            _screenListContainer = new VisualElement();
            _contentContainer.Add(_screenListContainer);

            SetupScreenList();

            var generateButton = new Button(GenerateScreenScripts)
            {
                text = "Generate Screen Scripts"
            };
            _contentContainer.Add(generateButton);
        }


        private void SetupScreenList()
        {
            if (string.IsNullOrEmpty(_selectedParentWindow)) return;
            if (!_screenNodeInfosByWindow.ContainsKey(_selectedParentWindow)) return;

            _screenListContainer.Clear();

            _screenList = new ReorderableList(_screenNodeInfosByWindow[_selectedParentWindow], typeof(ScreenNodeInfo),
                true, true, true, true);

            _screenList.drawHeaderCallback = rect =>
            {
                EditorGUI.LabelField(rect, $"Screens for {_selectedParentWindow}");
            };

            _screenList.drawElementCallback = (rect, index, isActive, isFocused) =>
            {
                var element = _screenNodeInfosByWindow[_selectedParentWindow][index];
                rect.y += 2;
                float widthUnit = rect.width / 3;

                // Script
                element.ScriptName = EditorGUI.TextField(
                    new Rect(rect.x, rect.y, widthUnit - 5, EditorGUIUtility.singleLineHeight),
                    element.ScriptName);

                // Prefab
                element.Prefab = (GameObject)EditorGUI.ObjectField(
                    new Rect(rect.x + widthUnit + 5, rect.y, widthUnit - 5, EditorGUIUtility.singleLineHeight),
                    element.Prefab, typeof(GameObject), false);

                // Generate
                element.GenerateScript = EditorGUI.Toggle(
                    new Rect(rect.x + (widthUnit * 2) + 10, rect.y, 20, EditorGUIUtility.singleLineHeight),
                    element.GenerateScript);
            };

            _screenList.onAddCallback = list =>
            {
                _screenNodeInfosByWindow[_selectedParentWindow].Add(new ScreenNodeInfo
                {
                    GenerateEnum = true,
                    GenerateScript = true,
                    ScriptName = "NewScreen"
                });
                SaveScreenNodeInfos();
            };

            _screenList.onRemoveCallback = list =>
            {
                if (list.index >= 0 && list.index < _screenNodeInfosByWindow[_selectedParentWindow].Count)
                {
                    _screenNodeInfosByWindow[_selectedParentWindow].RemoveAt(list.index);
                    SaveScreenNodeInfos();
                }
            };

            _screenListContainer.Add(new IMGUIContainer(() => _screenList.DoLayoutList()));
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

                SaveScreenNodeInfos();
                listView.RefreshItems();
            };

            listView.itemsRemoved += indices => { SaveScreenNodeInfos(); };
        }

        private void SaveScreenNodeInfos()
        {
            if (_settings == null) return;

            _settings.ScreenGeneratorsByWindow.Clear();

            foreach (var kvp in _screenNodeInfosByWindow)
            {
                var group = new ScreenDataGroup
                {
                    ParentWindowName = kvp.Key,
                    Screens = new List<ScreenData>()
                };

                foreach (var nodeInfo in kvp.Value)
                {
                    group.Screens.Add(new ScreenData
                    {
                        ScriptName = nodeInfo.ScriptName,
                        Prefab = nodeInfo.Prefab,
                        IsGenerated = nodeInfo.IsGenerated
                    });
                }

                _settings.ScreenGeneratorsByWindow.Add(group);
            }

            EditorUtility.SetDirty(_settings);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private void SaveWindowNodeInfos()
        {
            if (_settings == null) return;

            _settings.WindowGenerators.Clear();

            foreach (var nodeInfo in _windowNodeInfos)
            {
                _settings.WindowGenerators.Add(new WindowData
                {
                    ScriptName = nodeInfo.ScriptName,
                    Prefab = nodeInfo.Prefab,
                    IsGenerated = nodeInfo is WindowNodeInfo windowNode ? windowNode.GenerateScript : false
                });
            }

            EditorUtility.SetDirty(_settings);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }


        private VisualElement CreateScreenNodeRow()
        {
            var row = new VisualElement
            {
                style =
                {
                    flexDirection = FlexDirection.Row,
                    alignItems = Align.Center,
                    marginBottom = 4,
                    width = Length.Percent(100)
                }
            };

            // Script名 TextField
            var scriptField = new TextField
            {
                label = "Script",
                style =
                {
                    flexGrow = 1,
                    minWidth = 100,
                    marginRight = 8
                }
            };
            scriptField.labelElement.style.minWidth = 60;
            row.Add(scriptField);

            // Prefab ObjectField
            var prefabField = new ObjectField
            {
                label = "Prefab",
                objectType = typeof(GameObject),
                allowSceneObjects = false,
                style =
                {
                    flexGrow = 1,
                    minWidth = 100,
                    marginRight = 8
                }
            };
            prefabField.labelElement.style.minWidth = 60;
            row.Add(prefabField);

            // GenerateScript Toggle
            var generateScriptToggle = new Toggle
            {
                label = "Generate",
                style =
                {
                    width = 90,
                    flexShrink = 0
                }
            };
            generateScriptToggle.labelElement.style.minWidth = 60;
            row.Add(generateScriptToggle);

            return row;
        }


        private void BindScreenNodeRow(VisualElement element, ScreenNodeInfo data)
        {
            var scriptField = (TextField)element[0];
            scriptField.value = data.ScriptName;
            scriptField.RegisterValueChangedCallback(evt =>
            {
                data.ScriptName = evt.newValue;
                SaveScreenNodeInfos();
            });

            var prefabField = (ObjectField)element[1];
            prefabField.value = data.Prefab;
            prefabField.RegisterValueChangedCallback(evt =>
            {
                data.Prefab = evt.newValue as GameObject;
                SaveScreenNodeInfos();
            });

            var generateToggle = (Toggle)element[2];
            generateToggle.value = data.GenerateScript;
            generateToggle.RegisterValueChangedCallback(evt =>
            {
                data.GenerateScript = evt.newValue;
                SaveScreenNodeInfos();
            });
        }


        private void GenerateScreenScripts()
        {
            if (_settings == null) return;
            if (string.IsNullOrEmpty(_selectedParentWindow)) return;
            if (!_screenNodeInfosByWindow.TryGetValue(_selectedParentWindow, out var screenNodes)) return;

            var validScreenNodes = screenNodes.FindAll(node => node.GenerateScript);

            if (validScreenNodes.Count == 0)
            {
                Debug.LogWarning("有効なScreenノードがありません（すべてGenerateScript=false）。");
                return;
            }

            // スクリプト生成処理
            MVPClassFactory.GenerateScriptsAndPrefabs<ScreenNodeInfo, ScreenData>(
                validScreenNodes,
                _settings.OutputDirectory,
                _settings.NameSpace,
                _settings.ScreenTemplatePrefab,
                new List<ScreenData>()
            );

            // ここで、生成成功したノードのToggleをOFFにする
            foreach (var node in validScreenNodes)
            {
                node.GenerateScript = false;
            }

            // 保存して終了
            SaveScreenNodeInfos();

            EditorUtility.SetDirty(_settings);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("Screenノードのスクリプトを生成＆保存しました！");
        }
    }
}