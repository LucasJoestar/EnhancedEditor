// ===== Enhanced Editor - https://github.com/LucasJoestar/EnhancedEditor ===== //
// 
// Notes:
//
//  • BuildPipeline --> Only build on current target (no selection)
//  • Multi selection click --> last selected index
//  • Animation Tracker --> Which method is available (EnhancedBehaviour.UpdateInfos)
//  • EnhancedBehaviour component on prefabs
//  • Optimize eternal dictionary by clearing them when a specific limit is exceeded
//
// ============================================================================ //

#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

internal class CodeSamples : EditorWindow
{
    // --- Inter-Region Title --- \\

    // ===== Inside Region Title ===== \\

    // -------------------------------------------
    // Header
    // -------------------------------------------

    // ----- Local Methods ----- \\

    /// <inheritdoc cref="Doc(bool)"/>
    public void Doc()
    {
        using (var _scope = new GUILayout.HorizontalScope())
        {

        }
    }

    /// <summary>
    /// This is a documentation method.
    /// </summary>
    /// <param name="_value">Random value.</param>
    /// <returns>
    /// <inheritdoc cref="Doc(bool)" path="/param[@name='_value']"/></returns>
    public bool Doc(bool _value) => _value;

    // -----------------------

    /// <summary>
    /// Returns the first <see cref="CodeSamples"/> currently on screen.
    /// <br/> Creates and shows a new instance if there is none.
    /// </summary>
    /// <returns><see cref="CodeSamples"/> instance on screen.</returns>
    public static CodeSamples GetWindow()
    {
        CodeSamples _window = GetWindow<CodeSamples>("My Window");
        _window.Show();

        return _window;
    }

    /// <summary>
    /// Contains multiple <see cref="CodeSamples"/>-related extension methods.
    /// </summary>
    private class CodeSamplesExtensions { }
}
#endif
