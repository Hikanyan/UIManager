using System.IO;
using UnityEngine;

namespace HikanyanLaboratory.MVPStateTool
{
    public static class MVPClassFactory
    {
        public static void GenerateClass(string className, string classType, string outputPath,
            string extraContent = "")
        {
            string filePath = Path.Combine(outputPath, $"{className}.cs");

            string scriptContent = $@"
namespace HikanyanLaboratory.UISystem
{{
    public class {className} {classType}
    {{
        {extraContent}
    }}
}}";

            File.WriteAllText(filePath, scriptContent);
            Debug.Log($"{classType} クラス {className} を {filePath} に生成しました。");
        }

        public static void GeneratePresenterClass(string prefabName, string outputPath)
        {
            string className = $"{prefabName}Presenter";
            string extraContent = $@"
        using Cysharp.Threading.Tasks;
        using System.Threading;

        public override UniTask InitializeAsync(CancellationToken ct)
        {{
            return default;
        }}";

            GenerateClass(className, ": PresenterBase<" + prefabName + "View, " + prefabName + "Model>", outputPath,
                extraContent);
        }

        public static void GenerateViewClass(string prefabName, string outputPath)
        {
            string className = $"{prefabName}View";
            GenerateClass(className, ": UIViewBase", outputPath);
        }

        public static void GenerateModelClass(string prefabName, string outputPath)
        {
            string className = $"{prefabName}Model";
            GenerateClass(className, "", outputPath);
        }

        public static void GenerateEnumClass(string className, System.Collections.Generic.List<string> enumValues,
            string outputPath)
        {
            // 出力ディレクトリを確認し、存在しない場合は作成する
            if (!Directory.Exists(outputPath))
            {
                Directory.CreateDirectory(outputPath);
                Debug.Log($"ディレクトリを作成しました: {outputPath}");
            }
            
            string filePath = Path.Combine(outputPath, $"{className}.cs");

            string enumContent = $@"
namespace HikanyanLaboratory.UISystem
{{
    public enum {className}
    {{
        {string.Join(",\n        ", enumValues)}
    }}
}}";

            File.WriteAllText(filePath, enumContent);
            Debug.Log($"Enum {className} を {filePath} に生成しました。");
        }
    }
}