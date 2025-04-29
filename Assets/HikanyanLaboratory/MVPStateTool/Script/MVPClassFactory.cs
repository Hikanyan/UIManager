using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace HikanyanLaboratory.MVPStateTool
{
    public static class MVPClassFactory
    {
        public static string TemplatesFolderPath { get; set; } =
            "Assets/HikanyanLaboratory/UIToolSystem/MVPStateGenerator/Templates";


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

            // ＃または#どちらも対応して置換
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

            // 生成先のフォルダ確認＆作成
            if (!Directory.Exists(saveFolderPath))
            {
                Directory.CreateDirectory(saveFolderPath);
                AssetDatabase.Refresh();
            }

            // テンプレートPrefabをインスタンス化
            var instance = PrefabUtility.InstantiatePrefab(templatePrefab) as GameObject;
            if (instance == null)
            {
                Debug.LogError("Prefabのインスタンス化に失敗しました。");
                return null;
            }

            // 名前を指定
            instance.name = prefabName;

            // 保存先パスを決定
            string savePath = Path.Combine(saveFolderPath, $"{prefabName}.prefab").Replace("\\", "/");

            // Prefabとして保存
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

                // ScriptName空チェックと初期化
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
                    if (!Directory.Exists(scriptFolder)) Directory.CreateDirectory(scriptFolder);
                    if (!Directory.Exists(prefabFolder)) Directory.CreateDirectory(prefabFolder);
                }
                catch (Exception ex)
                {
                    Debug.LogError($"フォルダ作成エラー: {ex.Message}");
                    continue;
                }

                try
                {
                    // --- スクリプト作成 ---
                    GenerateViewClass(node.ScriptName, scriptFolder, nameSpace);
                    GenerateModelClass(node.ScriptName, scriptFolder, nameSpace);
                    GeneratePresenterClass(node.ScriptName, scriptFolder, nameSpace);
                }
                catch (Exception ex)
                {
                    Debug.LogError($"スクリプト生成エラー（{node.ScriptName}）: {ex.Message}");
                    continue;
                }

                // --- Prefab作成 ---
                var newPrefab = CreatePrefabFromTemplate(node.ScriptName, templatePrefab, prefabFolder);

                if (newPrefab == null)
                {
                    Debug.LogError($"Prefab生成に失敗しました: {node.ScriptName}");
                    continue;
                }
                if (node is ScreenNodeInfo screenNode)
                {
                    screenNode.IsGenerated = true;
                }
                // --- Viewコンポーネントを追加 ---
                string viewScriptFullName = $"{nameSpace}.{node.ScriptName}View";
                var viewType = Type.GetType(viewScriptFullName);

                if (viewType != null)
                {
                    if (newPrefab.GetComponent(viewType) == null)
                    {
                        try
                        {
                            newPrefab.AddComponent(viewType);
                            PrefabUtility.SavePrefabAsset(newPrefab);
                            Debug.Log($"PrefabにViewコンポーネントを追加しました: {viewType.Name}");
                        }
                        catch (Exception ex)
                        {
                            Debug.LogError($"PrefabへのViewコンポーネント追加エラー（{viewType.Name}）: {ex.Message}");
                        }
                    }
                }
                else
                {
                    Debug.LogWarning($"Viewスクリプトが見つかりませんでした: {viewScriptFullName}");
                }
                
                // --- Presenterコンポーネントも追加 ---
                string presenterScriptFullName = $"{nameSpace}.{node.ScriptName}Presenter";
                var presenterType = Type.GetType(presenterScriptFullName);

                if (presenterType != null)
                {
                    if (newPrefab.GetComponent(presenterType) == null)
                    {
                        try
                        {
                            newPrefab.AddComponent(presenterType);
                            Debug.Log($"PrefabにPresenterコンポーネントを追加しました: {presenterType.Name}");
                        }
                        catch (Exception ex)
                        {
                            Debug.LogError($"PrefabへのPresenterコンポーネント追加エラー（{presenterType.Name}）: {ex.Message}");
                        }
                    }
                }
                else
                {
                    Debug.LogWarning($"Presenterスクリプトが見つかりませんでした: {presenterScriptFullName}");
                }

                // 最後にPrefab保存
                PrefabUtility.SavePrefabAsset(newPrefab);
                // --- 出力リストに登録または更新 ---
                try
                {
                    var existing = outputDataList.Find(x => x.ScriptName == node.ScriptName);
                    if (existing != null)
                    {
                        existing.Prefab = node.Prefab != null ? node.Prefab : newPrefab;
                    }
                    else
                    {
                        var newData = new TData
                        {
                            ScriptName = node.ScriptName,
                            Prefab = node.Prefab != null ? node.Prefab : newPrefab
                        };
                        outputDataList.Add(newData);
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogError($"出力リスト登録エラー（{node.ScriptName}）: {ex.Message}");
                }
            }
        }
    }
}