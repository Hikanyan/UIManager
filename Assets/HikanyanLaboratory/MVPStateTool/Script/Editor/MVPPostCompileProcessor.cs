using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;

namespace HikanyanLaboratory.MVPStateTool
{
    public static class MVPPostCompileProcessor
    {
        [DidReloadScripts]
        public static void OnScriptsReloaded()
        {
            var state = MVPPostCompileState.instance;
            if (state.step == 0) return;

            try
            {
                for (int i = 0; i < state.prefabPaths.Length; i++)
                {
                    var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(state.prefabPaths[i]);
                    var scriptName = state.scriptNames[i];

                    MVPClassFactory.AttachScriptsToPrefab(prefab, state.nameSpace, scriptName);
                }

                Debug.Log("<color=green>✅ View/Presenter を Prefab に自動アタッチしました</color>");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"自動アタッチ処理で例外発生: {e}");
            }
            finally
            {
                state.Clear();
            }
        }
    }
}