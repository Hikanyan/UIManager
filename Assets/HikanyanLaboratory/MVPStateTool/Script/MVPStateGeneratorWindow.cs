using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.UIElements;
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
        private string _selectedParentWindow;

        [MenuItem("HikanyanTools/MVP State Generator")]
        public static void ShowWindow()
        {
            GetWindow<MVPStateGeneratorWindow>("MVP State Generator");
        }

        private void OnEnable()
        {
            LoadOrCreateSettings();
            CreateUI();
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

                        MVPClassFactory.GenerateEnumClass("WindowState", _settings.WindowStates, outputDir);
                        MVPClassFactory.GenerateEnumClass("ScreenState", _settings.ScreenStates, outputDir);

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
        }

        private void ShowGeneralSettings()
        {
            if (_settings == null)
            {
                var warningLabel = new Label("⚠ 設定ファイルが未設定です");
                _contentContainer.Add(warningLabel);
                return;
            }

            var outputDirField = new TextField("Output Directory")
            {
                value = _settings.OutputDirectory
            };
            outputDirField.RegisterValueChangedCallback(evt => { _settings.OutputDirectory = evt.newValue; });
            _contentContainer.Add(outputDirField);

            var namespaceField = new TextField("Namespace")
            {
                value = _settings.NameSpace
            };
            namespaceField.RegisterValueChangedCallback(evt => { _settings.NameSpace = evt.newValue; });
            _contentContainer.Add(namespaceField);
        }

        private void ShowWindowNodeGenerator()
        {
            if (_settings == null)
            {
                _contentContainer.Add(new Label("⚠ 設定ファイルが未設定です"));
                return;
            }

            _windowNodeInfos.Clear();
            _contentContainer.Clear();

            // WindowStateのEnumを探す
            Type windowStateType = Type.GetType($"{_settings.NameSpace}.WindowState");
            if (windowStateType == null)
            {
                _contentContainer.Add(new Label("⚠ WindowState Enumが見つかりません"));
                return;
            }

            var enumNames = Enum.GetNames(windowStateType);

            foreach (var name in enumNames)
            {
                var nodeInfo = new WindowNodeInfo
                {
                    Generate = true,
                    EnumName = name,
                    ScriptName = $"{name}Window"
                };
                _windowNodeInfos.Add(nodeInfo);

                var row = new VisualElement
                {
                    style = { flexDirection = FlexDirection.Row, alignItems = Align.Center }
                };

                var toggle = new Toggle
                {
                    value = true,
                    style = { width = 20 }
                };
                toggle.RegisterValueChangedCallback(evt => nodeInfo.Generate = evt.newValue);
                row.Add(toggle);

                var enumLabel = new Label(name)
                {
                    style = { width = 120 }
                };
                row.Add(enumLabel);

                var scriptField = new TextField
                {
                    value = nodeInfo.ScriptName,
                    style = { flexGrow = 1 }
                };
                scriptField.RegisterValueChangedCallback(evt => nodeInfo.ScriptName = evt.newValue);
                row.Add(scriptField);

                var prefabField = new ObjectField
                {
                    objectType = typeof(GameObject),
                    allowSceneObjects = false,
                    style = { width = 150 }
                };
                prefabField.RegisterValueChangedCallback(evt => { nodeInfo.Prefab = evt.newValue as GameObject; });
                row.Add(prefabField);

                _contentContainer.Add(row);
            }

            var generateButton = new Button(GenerateWindowScripts)
            {
                text = "Generate Scripts"
            };
            _contentContainer.Add(generateButton);
        }

        private void ShowScreenNodeGenerator()
        {
            if (_settings == null)
            {
                _contentContainer.Add(new Label("⚠ 設定ファイルが未設定です"));
                return;
            }

            _screenNodeInfos.Clear();
            _contentContainer.Clear();

            // WindowStateのEnumを探す
            Type windowStateType = Type.GetType($"{_settings.NameSpace}.WindowState");
            if (windowStateType == null)
            {
                _contentContainer.Add(new Label("⚠ WindowState Enumが見つかりません"));
                return;
            }

            var enumNames = Enum.GetNames(windowStateType);

            // 親Window選択ドロップダウン
            var windowDropdown = new PopupField<string>(new List<string>(enumNames), 0);
            windowDropdown.RegisterValueChangedCallback(evt =>
            {
                _selectedParentWindow = evt.newValue;
                RefreshScreenNodeList();
            });
            _contentContainer.Add(windowDropdown);

            _selectedParentWindow = windowDropdown.value;

            // 初期ロード
            RefreshScreenNodeList();
        }

        private void RefreshScreenNodeList()
        {
            // 既存リストをクリア
            var childrenToRemove = new List<VisualElement>();

            foreach (var child in _contentContainer.Children())
            {
                if (child is ScrollView)
                    childrenToRemove.Add(child);
            }

            foreach (var child in childrenToRemove)
            {
                child.RemoveFromHierarchy();
            }


            if (string.IsNullOrEmpty(_selectedParentWindow)) return;

            Type screenStateType = Type.GetType($"{_settings.NameSpace}.ScreenState");
            if (screenStateType == null)
            {
                _contentContainer.Add(new Label("⚠ ScreenState Enumが見つかりません"));
                return;
            }

            var enumNames = Enum.GetNames(screenStateType);

            var scroll = new ScrollView();
            _contentContainer.Add(scroll);

            foreach (var name in enumNames)
            {
                var nodeInfo = new ScreenNodeInfo
                {
                    Generate = true,
                    EnumName = name,
                    ScriptName = $"{name}Screen"
                };
                _screenNodeInfos.Add(nodeInfo);

                var row = new VisualElement
                {
                    style = { flexDirection = FlexDirection.Row, alignItems = Align.Center }
                };

                var toggle = new Toggle
                {
                    value = true,
                    style = { width = 20 }
                };
                toggle.RegisterValueChangedCallback(evt => nodeInfo.Generate = evt.newValue);
                row.Add(toggle);

                var enumLabel = new Label(name)
                {
                    style = { width = 120 }
                };
                row.Add(enumLabel);

                var scriptField = new TextField
                {
                    value = nodeInfo.ScriptName,
                    style = { flexGrow = 1 }
                };
                scriptField.RegisterValueChangedCallback(evt => nodeInfo.ScriptName = evt.newValue);
                row.Add(scriptField);

                var prefabField = new ObjectField
                {
                    objectType = typeof(GameObject),
                    allowSceneObjects = false,
                    style = { width = 150 }
                };
                prefabField.RegisterValueChangedCallback(evt => { nodeInfo.Prefab = evt.newValue as GameObject; });
                row.Add(prefabField);

                scroll.Add(row);
            }

            var generateButton = new Button(GenerateScreenScripts)
            {
                text = "Generate Scripts"
            };
            scroll.Add(generateButton);
        }

        private void GenerateScreenScripts()
        {
            if (_settings == null) return;

            string outputDir = _settings.OutputDirectory;
            if (!Directory.Exists(outputDir))
            {
                Directory.CreateDirectory(outputDir);
            }

            _settings.ScreenGenerators.Clear(); // 一回リセットする

            foreach (var node in _screenNodeInfos)
            {
                if (!node.Generate) continue;

                MVPClassFactory.GenerateViewClass(node.ScriptName, outputDir);
                MVPClassFactory.GenerateModelClass(node.ScriptName, outputDir);
                MVPClassFactory.GeneratePresenterClass(node.ScriptName, outputDir);

                // ScriptableObjectに登録
                var screenData = new ScreenData
                {
                    ScriptName = node.ScriptName,
                    Prefab = node.Prefab
                };
                _settings.ScreenGenerators.Add(screenData);
            }

            EditorUtility.SetDirty(_settings); // Settingsファイルに変更通知
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("Screenノードのスクリプトを生成＆保存しました！");
        }


        private void GenerateWindowScripts()
        {
            if (_settings == null) return;

            string outputDir = _settings.OutputDirectory;
            if (!Directory.Exists(outputDir))
            {
                Directory.CreateDirectory(outputDir);
            }

            _settings.WindowGenerators.Clear(); // 一回リセットする

            foreach (var node in _windowNodeInfos)
            {
                if (!node.Generate) continue;

                MVPClassFactory.GenerateViewClass(node.ScriptName, outputDir);
                MVPClassFactory.GenerateModelClass(node.ScriptName, outputDir);
                MVPClassFactory.GeneratePresenterClass(node.ScriptName, outputDir);

                // ScriptableObjectに登録
                var windowData = new WindowData
                {
                    ScriptName = node.ScriptName,
                    Prefab = node.Prefab
                };
                _settings.WindowGenerators.Add(windowData);
            }

            EditorUtility.SetDirty(_settings); // Settingsファイルに変更通知
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

        private static void CreatePrefabList<T>(List<T> list, VisualElement parent) where T : class, new()
        {
            if (list == null) return;

            var listView = new ListView(list, 60, () =>
                {
                    var container = new VisualElement();
                    var scriptField = new TextField("Script Name");
                    container.Add(scriptField);
                    var prefabField = new ObjectField("Prefab") { objectType = typeof(GameObject) };
                    container.Add(prefabField);
                    return container;
                },
                (element, index) =>
                {
                    var container = (VisualElement)element;
                    var scriptField = (TextField)container[0];
                    var prefabField = (ObjectField)container[1];

                    if (list[index] == null) return;

                    if (typeof(T) == typeof(WindowData))
                    {
                        var item = (WindowData)(object)list[index];
                        scriptField.value = item.ScriptName ?? "";
                        scriptField.RegisterValueChangedCallback(evt => item.ScriptName = evt.newValue);
                        prefabField.value = item.Prefab;
                        prefabField.RegisterValueChangedCallback(evt => item.Prefab = evt.newValue as GameObject);
                    }
                    else if (typeof(T) == typeof(ScreenData))
                    {
                        var item = (ScreenData)(object)list[index];
                        scriptField.value = item.ScriptName ?? "";
                        scriptField.RegisterValueChangedCallback(evt => item.ScriptName = evt.newValue);
                        prefabField.value = item.Prefab;
                        prefabField.RegisterValueChangedCallback(evt => item.Prefab = evt.newValue as GameObject);
                    }
                })
            {
                reorderable = true,
                showAddRemoveFooter = true,
                showBorder = true
            };

            listView.itemsAdded += indices =>
            {
                foreach (var index in indices)
                {
                    list.Insert(index, Activator.CreateInstance<T>());
                }

                listView.Rebuild();
            };

            parent.Add(listView);
        }
    }
}