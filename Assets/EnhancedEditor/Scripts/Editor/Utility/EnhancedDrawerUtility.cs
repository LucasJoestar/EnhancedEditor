// ===== Enhanced Editor - https://github.com/LucasJoestar/EnhancedEditor ===== //
// 
// Notes:
//
// ============================================================================ //

using System;
using System.Collections.Generic;
using System.Reflection;

namespace EnhancedEditor.Editor
{
    /// <summary>
    /// Multiple useful utility methods related to <see cref="EnhancedEditor"/> internal drawers behaviour.
    /// </summary>
	internal static class EnhancedDrawerUtility
    {
        #region Unity Object Drawers
        /// <summary>
        /// <see cref="UnityObjectDrawer"/> as key, target class as value.
        /// </summary>
        private static Dictionary<Type, Type> objectDrawers = new Dictionary<Type, Type>();
        private static bool areObjectDrawersInitialized = false;

        // -----------------------

        public static Dictionary<Type, Type> GetCustomDrawers()
        {
            // Search for all unity object drawers among project.
            if (!areObjectDrawersInitialized)
            {
                areObjectDrawersInitialized = true;
                foreach (Assembly _assembly in AppDomain.CurrentDomain.GetAssemblies())
                {
                    foreach (Type _type in _assembly.GetTypes())
                    {
                        // Register drawer if having a target class.
                        if (_type.IsSubclassOf(typeof(UnityObjectDrawer)) && !_type.IsAbstract)
                        {
                            CustomDrawerAttribute _drawer = _type.GetCustomAttribute<CustomDrawerAttribute>(true);
                            if (_drawer != null)
                                objectDrawers[_type] = _drawer.TargetType;
                        }
                    }
                }
            }

            return objectDrawers;
        }
        #endregion

        #region Property Drawers
        /// <summary>
        /// <see cref="EnhancedPropertyDrawer"/> as key, target class as value.
        /// </summary>
        private static Dictionary<Type, Type> propertyDrawers = new Dictionary<Type, Type>();
        private static bool arePropertyDrawersInitialized = false;

        // -----------------------

        public static Dictionary<Type, Type> GetPropertyDrawers()
        {
            // Search for all property drawers among project.
            if (!arePropertyDrawersInitialized)
            {
                arePropertyDrawersInitialized = true;
                foreach (Assembly _assembly in AppDomain.CurrentDomain.GetAssemblies())
                {
                    foreach (Type _type in _assembly.GetTypes())
                    {
                        // Register drawer if having a target class.
                        if (_type.IsSubclassOf(typeof(EnhancedPropertyDrawer)) && !_type.IsAbstract)
                        {
                            CustomDrawerAttribute _drawer = _type.GetCustomAttribute<CustomDrawerAttribute>(true);
                            if (_drawer != null)
                                propertyDrawers[_type] = _drawer.TargetType;
                        }
                    }
                }
            }

            return propertyDrawers;
        }
        #endregion

        #region Method Drawers
        /// <summary>
        /// <see cref="EnhancedMethodAttribute"/> as key, target class as value.
        /// </summary>
        private static Dictionary<Type, Type> methodDrawers = new Dictionary<Type, Type>();
        private static bool areMehodDrawersInitialized = false;

        // -----------------------

        public static Dictionary<Type, Type> GetMethodDrawers()
        {
            // Search for all method drawers among project.
            if (!areMehodDrawersInitialized)
            {
                areMehodDrawersInitialized = true;
                foreach (Assembly _assembly in AppDomain.CurrentDomain.GetAssemblies())
                {
                    foreach (Type _type in _assembly.GetTypes())
                    {
                        // Register drawer if having a target class.
                        if (_type.IsSubclassOf(typeof(MethodDrawer)) && !_type.IsAbstract)
                        {
                            CustomDrawerAttribute _drawer = _type.GetCustomAttribute<CustomDrawerAttribute>(true);
                            if (_drawer != null)
                                methodDrawers[_type] = _drawer.TargetType;
                        }
                    }
                }
            }

            return methodDrawers;
        }
        #endregion
    }
}
