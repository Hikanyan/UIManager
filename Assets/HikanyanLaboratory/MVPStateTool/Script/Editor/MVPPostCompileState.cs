using UnityEditor;

namespace HikanyanLaboratory.MVPStateTool
{
    internal class MVPPostCompileState : ScriptableSingleton<MVPPostCompileState>
    {
        public int step = 0;
        public string nameSpace;
        public string[] prefabPaths;
        public string[] scriptNames;

        public void Save()
        {
            Save(true);
        }

        public void Clear()
        {
            step = 0;
            nameSpace = null;
            prefabPaths = null;
            scriptNames = null;
            Save();
        }
    }
}