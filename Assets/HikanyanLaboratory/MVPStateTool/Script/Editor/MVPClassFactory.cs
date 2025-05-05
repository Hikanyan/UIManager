using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEditor.Compilation;

namespace HikanyanLaboratory.MVPStateTool
{
    public static class MVPClassFactory
    {
        public static string TemplatesFolderPath { get; set; } =
            "Assets/HikanyanLaboratory/UIToolSystem/MVPStateGenerator/Templates";

        private static Action<object> _onCompilationFinishedHandler;

        private static void GenerateFromTemplate(string templateFileName, string outputPath,
            Dictionary<string, string> replacements)
        {
            string templatePath = Path.Combine(TemplatesFolderPath, templateFileName);
            if (!File.Exists(templatePath))
            {
                Debug.LogError($"テンプレートファイルが見つかりません: {templatePath}");
                return;
            }

            string templateContent = File.ReadAllText(templatePath);
            foreach (var kvp in replacements)
            {
                templateContent = templateContent.Replace($"＃{kvp.Key}", kvp.Value);
                templateContent = templateContent.Replace($"#{kvp.Key}", kvp.Value);
            }

            if (!replacements.TryGetValue("CLASSNAME", out string className))
            {
                Debug.LogError("置換リストに CLASSNAME が含まれていません。");
                return;
            }

            string filePath = Path.Combine(outputPath, $"{className}.cs");
            File.WriteAllText(filePath, templateContent);
            Debug.Log($"{className} を {filePath} に生成しました。");
        }

        public static void GeneratePresenterClass(string prefabName, string outputPath, string nameSpace)
        {
            var replacements = new Dictionary<string, string>
            {
                { "NAMESPACE", nameSpace },
                { "CLASSNAME", $"{prefabName}Presenter" },
                { "PREFABNAME", prefabName },
                { "PREFABNAMEModel", prefabName }
            };

            GenerateFromTemplate("PresenterTemplate.txt", outputPath, replacements);
        }

        public static void GenerateViewClass(string prefabName, string outputPath, string nameSpace)
        {
            var replacements = new Dictionary<string, string>
            {
                { "NAMESPACE", nameSpace },
                { "CLASSNAME", $"{prefabName}View" }
            };

            GenerateFromTemplate("ViewTemplate.txt", outputPath, replacements);
        }

        public static void GenerateModelClass(string prefabName, string outputPath, string nameSpace)
        {
            var replacements = new Dictionary<string, string>
            {
                { "NAMESPACE", nameSpace },
                { "CLASSNAME", $"{prefabName}Model" }
            };

            GenerateFromTemplate("ModelTemplate.txt", outputPath, replacements);
        }

        public static void GenerateEnumClass(string enumName, List<string> enumValues, string outputPath,
            string nameSpace)
        {
            if (!Directory.Exists(outputPath))
            {
                Directory.CreateDirectory(outputPath);
                Debug.Log($"ディレクトリを作成しました: {outputPath}");
            }

            string enumBody = string.Join(",\n        ", enumValues);
            var replacements = new Dictionary<string, string>
            {
                { "NAMESPACE", nameSpace },
                { "ENUMENAME", enumName },
                { "ENUMNAME", enumBody },
                { "CLASSNAME", enumName } // ダミー
            };

            GenerateFromTemplate("EnumTemplate.txt", outputPath, replacements);
        }

        public static GameObject CreatePrefabFromTemplate(string prefabName, GameObject templatePrefab,
            string saveFolderPath)
        {
            if (templatePrefab == null)
            {
                Debug.LogError("テンプレートPrefabが設定されていません。");
                return null;
            }

            if (!Directory.Exists(saveFolderPath))
            {
                Directory.CreateDirectory(saveFolderPath);
                AssetDatabase.Refresh();
            }

            var instance = PrefabUtility.InstantiatePrefab(templatePrefab) as GameObject;
            if (instance == null)
            {
                Debug.LogError("Prefabのインスタンス化に失敗しました。");
                return null;
            }

            instance.name = prefabName;
            string savePath = Path.Combine(saveFolderPath, $"{prefabName}.prefab").Replace("\\", "/");
            var newPrefab = PrefabUtility.SaveAsPrefabAsset(instance, savePath);
            UnityEngine.Object.DestroyImmediate(instance);

            Debug.Log($"Prefabを作成しました: {savePath}");
            return newPrefab;
        }

        public static void GenerateScriptsAndPrefabs<TNode, TData>(
            List<TNode> nodes,
            string outputRootDir,
            string nameSpace,
            GameObject templatePrefab,
            List<TData> outputDataList
        )
            where TNode : IGeneratorNode
            where TData : IDataEntry, new()
        {
            if (nodes == null || nodes.Count == 0)
            {
                Debug.LogError("ノードリストが空です。スクリプトおよびPrefabを生成できません。");
                return;
            }

            foreach (var node in nodes)
            {
                if (!node.GenerateScript) continue;

                if (string.IsNullOrEmpty(node.ScriptName))
                {
                    Debug.LogWarning("ScriptNameが空だったので自動で付与します。");
                    node.ScriptName = $"Auto_{Guid.NewGuid().ToString("N").Substring(0, 6)}";
                }

                string baseFolder = Path.Combine(outputRootDir, node.ScriptName);
                string scriptFolder = Path.Combine(baseFolder, "Script");
                string prefabFolder = Path.Combine(baseFolder, "Resources");

                try
                {
                    Directory.CreateDirectory(scriptFolder);
                    Directory.CreateDirectory(prefabFolder);
                }
                catch (Exception ex)
                {
                    Debug.LogError($"フォルダ作成エラー: {ex.Message}");
                    continue;
                }

                try
                {
                    GenerateViewClass(node.ScriptName, scriptFolder, nameSpace);
                    GenerateModelClass(node.ScriptName, scriptFolder, nameSpace);
                    GeneratePresenterClass(node.ScriptName, scriptFolder, nameSpace);
                }
                catch (Exception ex)
                {
                    Debug.LogError($"スクリプト生成エラー（{node.ScriptName}）: {ex.Message}");
                    continue;
                }
            }
            
            // Prefabを先に作る（後でスクリプトAttachするため）
            List<(GameObject prefab, string scriptName)> prefabsToFix = new();

            foreach (var node in nodes)
            {
                if (!node.GenerateScript) continue;

                string baseFolder = Path.Combine(outputRootDir, node.ScriptName);
                string prefabFolder = Path.Combine(baseFolder, "Resources");

                var newPrefab = CreatePrefabFromTemplate(node.ScriptName, templatePrefab, prefabFolder);
                if (newPrefab == null)
                {
                    Debug.LogError($"Prefab生成に失敗しました: {node.ScriptName}");
                    continue;
                }

                prefabsToFix.Add((newPrefab, node.ScriptName));

                if (node is ScreenNodeInfo screenNode) screenNode.IsGenerated = true;
                if (node is WindowNodeInfo windowNode) windowNode.GenerateScript = false;

                try
                {
                    var existing = outputDataList.Find(x => x.ScriptName == node.ScriptName);
                    if (existing != null)
                    {
                        existing.Prefab = node.Prefab != null ? node.Prefab : newPrefab;
                    }
                    else
                    {
                        outputDataList.Add(new TData
                        {
                            ScriptName = node.ScriptName,
                            Prefab = node.Prefab != null ? node.Prefab : newPrefab
                        });
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogError($"出力リスト登録エラー（{node.ScriptName}）: {ex.Message}");
                }
            }

            // Refreshしてコンパイルを促す
            // AssetDatabase.Refresh();
            
            RegisterPostCompileAttachTask(prefabsToFix, nameSpace);
        }

        private static void RegisterCompilationFinishedCallback(
            List<(GameObject prefab, string scriptName)> prefabsToFix, string nameSpace)
        {
            _onCompilationFinishedHandler = (assemblies) =>
            {
                CompilationPipeline.compilationFinished -= _onCompilationFinishedHandler;

                try
                {
                    foreach (var (prefab, scriptName) in prefabsToFix)
                    {
                        AttachScriptsToPrefab(prefab, nameSpace, scriptName);
                    }

                    Debug.Log("PrefabにView/Presenterを追加しました！");
                }
                catch (Exception e)
                {
                    Debug.LogError($"Prefabへのスクリプトアタッチ処理中に例外が発生しました: {e}");
                }
            };

            CompilationPipeline.compilationFinished += _onCompilationFinishedHandler;
        }


        public static void AttachScriptsToPrefab(GameObject prefab, string nameSpace, string scriptName)
        {
            if (prefab == null) return;

            string viewScriptFullName = $"{nameSpace}.{scriptName}View";
            string presenterScriptFullName = $"{nameSpace}.{scriptName}Presenter";

            TryAddComponent(prefab, viewScriptFullName);
            TryAddComponent(prefab, presenterScriptFullName);

            PrefabUtility.SavePrefabAsset(prefab);
            Debug.Log($"PrefabにView/Presenterを追加しました: {prefab.name}");
        }


        private static void TryAddComponent(GameObject prefab, string fullTypeName)
        {
            var type = FindTypeByName(fullTypeName);
            if (type == null)
            {
                Debug.LogWarning($"スクリプトが見つかりませんでした: {fullTypeName}");
                return;
            }

            if (prefab.GetComponent(type) == null)
            {
                try
                {
                    prefab.AddComponent(type);
                    Debug.Log($"Prefabにコンポーネントを追加しました: {type.Name}");
                }
                catch (Exception ex)
                {
                    Debug.LogError($"コンポーネント追加エラー ({type.Name}): {ex.Message}");
                }
            }
        }
        public static void RegisterPostCompileAttachTask(
            List<(GameObject prefab, string scriptName)> prefabsToFix, string nameSpace)
        {
#if UNITY_EDITOR
            if (prefabsToFix == null || prefabsToFix.Count == 0)
            {
                Debug.LogWarning("登録対象のPrefabがありません。");
                return;
            }

            var state = MVPPostCompileState.instance;
            state.step = 1;
            state.nameSpace = nameSpace;
            state.prefabPaths = prefabsToFix
                .Select(pair => AssetDatabase.GetAssetPath(pair.prefab))
                .ToArray();
            state.scriptNames = prefabsToFix
                .Select(pair => pair.scriptName)
                .ToArray();
            state.Save();

            // アセットリフレッシュ & 再コンパイルを明示的に要求
            AssetDatabase.Refresh();
            CompilationPipeline.RequestScriptCompilation();
#endif
        }

        private static Type FindTypeByName(string fullName)
        {
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                var type = assembly.GetType(fullName);
                if (type != null)
                    return type;
            }

            return null;
        }
    }
}