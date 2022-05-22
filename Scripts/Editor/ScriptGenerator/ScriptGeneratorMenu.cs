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
        [MenuItem(ScriptCreatorSubMenu + "Enhanced Editor/PREFIX_Template Example", false, MenuItemOrder)]
        public static void CreatePREFIX_TemplateExample() => ScriptGeneratorWindow.GetWindow("EnhancedEditor/_InternalResources/Editor/ScriptTemplates/EnhancedEditor/PREFIX_TemplateExample.txt");

        [MenuItem(ScriptCreatorSubMenu + "Enhanced Editor/Template Example", false, MenuItemOrder)]
        public static void CreateTemplateExample() => ScriptGeneratorWindow.GetWindow("EnhancedEditor/_InternalResources/Editor/ScriptTemplates/EnhancedEditor/TemplateExample.txt");

        [MenuItem(ScriptCreatorSubMenu + "Enhanced Editor/Template Example_SUFFIX", false, MenuItemOrder)]
        public static void CreateTemplateExample_SUFFIX() => ScriptGeneratorWindow.GetWindow("EnhancedEditor/_InternalResources/Editor/ScriptTemplates/EnhancedEditor/TemplateExample_SUFFIX.txt");
    }
}
