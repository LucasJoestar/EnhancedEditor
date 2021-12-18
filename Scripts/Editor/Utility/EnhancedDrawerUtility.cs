// ===== Enhanced Editor - https://github.com/LucasJoestar/EnhancedEditor ===== //
// 
// Notes:
//
// ============================================================================ //

using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;

namespace EnhancedEditor.Editor
{
    /// <summary>
    /// Contains multiple <see cref="EnhancedEditor"/>-internal utility methods related to custom drawers.
    /// </summary>
	internal static class EnhancedDrawerUtility
    {
        #region Content
        /// <summary>
        /// <see cref="UnityObjectDrawer"/> as key, target class as value.
        /// </summary>
        private static Dictionary<Type, Type> objectDrawers = null;

        /// <summary>
        /// <see cref="EnhancedPropertyDrawer"/> as key, target class as value.
        /// </summary>
        private static Dictionary<Type, Type> propertyDrawers = null;

        /// <summary>
        /// <see cref="MethodDrawer"/> as key, target class as value.
        /// </summary>
        private static Dictionary<Type, Type> methodDrawers = null;

        // -----------------------

        public static Dictionary<Type, Type> GetObjectDrawers()
        {
            LoadDrawers(ref objectDrawers, typeof(UnityObjectDrawer));
            return objectDrawers;
        }

        public static Dictionary<Type, Type> GetPropertyDrawers()
        {
            LoadDrawers(ref propertyDrawers, typeof(EnhancedPropertyDrawer));
            return propertyDrawers;
        }

        public static Dictionary<Type, Type> GetMethodDrawers()
        {
            LoadDrawers(ref methodDrawers, typeof(MethodDrawer));
            return methodDrawers;
        }

        // -----------------------

        private static void LoadDrawers(ref Dictionary<Type, Type> _data, Type _drawer)
        {
            if (_data == null)
            {
                _data = new Dictionary<Type, Type>();

                // Search for all target drawers in the project.
                #if UNITY_2019_2_OR_NEWER
                var _types = TypeCache.GetTypesDerivedFrom(_drawer);
                foreach (Type _type in _types)
                {
                    // Only register the drawer if it has a target class.
                    if (!_type.IsAbstract)
                    {
                        RegisterDrawer(_data, _type);
                    }
                }
                #else
                foreach (Assembly _assembly in AppDomain.CurrentDomain.GetAssemblies())
                {
                    foreach (Type _type in _assembly.GetTypes())
                    {
                        // Register drawer if having a target class.
                        if (_type.IsSubclassOf(_drawer) && !_type.IsAbstract)
                        {
                            RegisterDrawer(_data, _type);
                        }
                    }
                }
                #endif
            }

            void RegisterDrawer(Dictionary<Type, Type> _dataDictionary, Type _typeToRegister)
            {
                var _customDrawer = _typeToRegister.GetCustomAttribute<CustomDrawerAttribute>(true);
                if (_customDrawer != null)
                {
                    _dataDictionary[_typeToRegister] = _customDrawer.TargetType;
                }
            }
        }
        #endregion
    }
}
