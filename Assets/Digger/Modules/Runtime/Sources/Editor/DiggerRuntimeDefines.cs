using Digger.Modules.Core.Editor;
using UnityEditor;
using UnityEditor.Callbacks;

namespace Digger.Modules.Runtime.Sources.Editor
{
    [InitializeOnLoad]
    public class DiggerRuntimeDefines
    {
        private const string DiggerRuntimeDefine = "__DIGGER_RUNTIME__";

        static DiggerRuntimeDefines()
        {
            DiggerDefines.InitDefine(DiggerRuntimeDefine);
        }

        [PostProcessScene(0)]
        public static void OnPostprocessScene()
        {
            DiggerDefines.InitDefine(DiggerRuntimeDefine);
        }
    }
}