using UnityEditor;
using UnityEditor.Callbacks;

namespace Digger.Modules.Core.Editor
{
    [InitializeOnLoad]
    public class DiggerDefines
    {
        private const string DiggerDefine = "__DIGGER__";

        static DiggerDefines()
        {
            InitDefine(DiggerDefine);
        }

        public static void InitDefine(string def)
        {
            var target = EditorUserBuildSettings.selectedBuildTargetGroup;
            var defines = PlayerSettings.GetScriptingDefineSymbolsForGroup(target);
            if (defines.Contains(def))
                return;

            if (string.IsNullOrEmpty(defines)) {
                PlayerSettings.SetScriptingDefineSymbolsForGroup(target, def);
            }
            else {
                if (!defines[defines.Length - 1].Equals(';')) {
                    defines += ';';
                }

                defines += def;
                PlayerSettings.SetScriptingDefineSymbolsForGroup(target, defines);
            }
        }

        [PostProcessScene(0)]
        public static void OnPostprocessScene()
        {
            InitDefine(DiggerDefine);
        }
    }
}