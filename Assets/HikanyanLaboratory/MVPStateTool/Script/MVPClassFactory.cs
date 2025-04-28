using System.Collections.Generic;
using System.IO;
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
                { "PREFABNAMEMModel", prefabName }
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
    }
}