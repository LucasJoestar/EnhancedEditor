// ===== Enhanced Editor - https://github.com/LucasJoestar/EnhancedEditor ===== //
//
// Notes:
//
// ============================================================================ //

using UnityEditor;

namespace EnhancedEditor.Editor
{
    public static partial class ScriptGenerator
    {
        [MenuItem(ScriptCreatorSubMenu + "Twin Peaks Behaviour", false, MenuItemOrder)]
        public static void CreateTwinPeaksBehaviour() => ScriptGeneratorWindow.GetWindow("700 EnhancedEngine/EnhancedEditor/Editor/ScriptTemplates/TwinPeaksBehaviour.txt");
    }
}
