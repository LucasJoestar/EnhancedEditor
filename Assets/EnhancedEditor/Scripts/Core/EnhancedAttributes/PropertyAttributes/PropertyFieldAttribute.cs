// ===== Enhanced Editor - https://github.com/LucasJoestar/EnhancedEditor ===== //
// 
// Notes:
//
// ============================================================================ //

namespace EnhancedEditor
{
    /// <summary>
    /// Set a C# property value whenever this field value has changed.
    /// <para/>
    /// By default, C# property name is the name of this field as PascalCase.
    /// </summary>
    public class PropertyFieldAttribute : EnhancedPropertyAttribute
    {
        #region Global Members
        public readonly string PropertyName = string.Empty;
        public readonly ActivationMode Mode = ActivationMode.Always;

        // -----------------------

        /// <inheritdoc cref="PropertyFieldAttribute(string, ActivationMode)"/>
        public PropertyFieldAttribute(ActivationMode _mode = ActivationMode.Always)
        {
            Mode = _mode;
        }

        /// <inheritdoc cref="PropertyFieldAttribute"/>
        /// <param name="_propertyName">Name of the associated class C# property.</param>
        /// <param name="_mode">Determines when the associated property will be set.</param>
        public PropertyFieldAttribute(string _propertyName, ActivationMode _mode = ActivationMode.Always) : this(_mode)
        {
            PropertyName = _propertyName;
        }
        #endregion
    }
}
