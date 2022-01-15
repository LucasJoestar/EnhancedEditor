// ===== Enhanced Editor - https://github.com/LucasJoestar/EnhancedEditor ===== //
// 
// Notes:
//
// ============================================================================ //

using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;

namespace EnhancedEditor.Editor
{
    /// <summary>
    /// Base class to derive any <see cref="UnityEngine.Object"/> custom editor from (instead of <see cref="UnityEditor.Editor"/>),
    /// performing additional operations related to the <see cref="EnhancedEditor"/> plugin.
    /// </summary>
    [CustomEditor(typeof(UnityEngine.Object), true), CanEditMultipleObjects]
	public class UnityObjectEditor : UnityEditor.Editor
    {
        #region Method Drawer Group
        private class MethodDrawerGroup
        {
            public MethodDrawer[] MethodDrawers = new MethodDrawer[] { };

            // -----------------------

            public void DrawMethodDrawers(bool _isOnTop)
            {
                Rect _position = EditorGUILayout.GetControlRect(false, -EditorGUIUtility.standardVerticalSpacing);

                // Pre GUI callback.
                foreach (MethodDrawer _drawer in MethodDrawers)
                {
                    if ((_isOnTop == _drawer.Attribute.IsDrawnOnTop) && _drawer.OnBeforeGUI())
                        return;
                }

                // Method GUI.
                foreach (MethodDrawer _drawer in MethodDrawers)
                {
                    if ((_isOnTop == _drawer.Attribute.IsDrawnOnTop) && _drawer.OnGUI())
                        break;
                }

                // Post GUI callback.
                foreach (MethodDrawer _drawer in MethodDrawers)
                {
                    if (_isOnTop == _drawer.Attribute.IsDrawnOnTop)
                        _drawer.OnAfterGUI();
                }

                // Context click menu. 
                _position.height = GUILayoutUtility.GetLastRect().yMax - _position.y;

                if (EnhancedEditorGUIUtility.ContextClick(_position))
                {
                    GenericMenu _menu = new GenericMenu();
                    foreach (MethodDrawer _drawer in MethodDrawers)
                        _drawer.OnContextMenu(_menu);

                    if (_menu.GetItemCount() > 0)
                        _menu.ShowAsContext();
                }
            }
        }
        #endregion

        #region Editor Content
        private static readonly BindingFlags methodInfoFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static;

        private UnityObjectDrawer[] objectDrawers = new UnityObjectDrawer[] { };
        private MethodDrawerGroup[] methodDrawerGroups = new MethodDrawerGroup[] { };

        // -----------------------

        protected virtual void OnEnable()
        {
            try
            {
                if (!serializedObject.targetObject)
                {
                    DestroyImmediate(this);
                    return;
                }
            }
            catch
            {
                DestroyImmediate(this);
                return;
            }

            // Get all class attributes from editing object, then create and enable a drawer for each of them.
            Type _type = serializedObject.targetObject.GetType();
            {
                var _attributes = _type.GetCustomAttributes(typeof(EnhancedClassAttribute), true) as EnhancedClassAttribute[];

                foreach (KeyValuePair<Type, Type> _pair in EnhancedDrawerUtility.GetObjectDrawers())
                {
                    foreach (EnhancedClassAttribute _attribute in _attributes)
                    {
                        if (_pair.Value == _attribute.GetType())
                        {
                            UnityObjectDrawer _customDrawer = UnityObjectDrawer.CreateInstance(_pair.Key, serializedObject, _attribute);
                            ArrayUtility.Add(ref objectDrawers, _customDrawer);
                        }
                    }
                }
            }

            // Do the same for method attributes.
            {
                MethodInfo[] _allMethods = _type.GetMethods(methodInfoFlags);
                var _methodDrawers = EnhancedDrawerUtility.GetMethodDrawers();

                foreach (MethodInfo _method in _allMethods)
                {
                    var _attributes = _method.GetCustomAttributes(typeof(EnhancedMethodAttribute), true) as EnhancedMethodAttribute[];

                    // Skip methods without any desired attribute.
                    if (_attributes.Length == 0)
                        continue;

                    MethodDrawerGroup _group = new MethodDrawerGroup();

                    foreach (KeyValuePair<Type, Type> _pair in _methodDrawers)
                    {
                        foreach (EnhancedMethodAttribute _attribute in _attributes)
                        {
                            if (_pair.Value == _attribute.GetType())
                            {
                                MethodDrawer _customDrawer = MethodDrawer.CreateInstance(_pair.Key, serializedObject, _attribute, _method);
                                ArrayUtility.Add(ref _group.MethodDrawers, _customDrawer);
                            }
                        } 
                    }

                    Array.Sort(_group.MethodDrawers, (a, b) => a.Attribute.Order.CompareTo(b.Attribute.Order));
                    ArrayUtility.Add(ref methodDrawerGroups, _group);
                }
            }

            // Add the EnhancedEditor GameObject-extending component if none is attached.
            if (!Application.isPlaying)
            {
                foreach (var _object in serializedObject.targetObjects)
                {
                    // If this object is a prefab, only add the component to the origin asset to avoid troubles with the prefab override system.
                    if (PrefabUtility.IsPartOfPrefabInstance(_object) || PrefabUtility.IsAddedComponentOverride(_object) || PrefabUtility.IsPartOfImmutablePrefab(_object))
                        continue;

                    if (_object is Component _component)
                    {
                        var _behaviour = _component.gameObject.AddComponentIfNone<EnhancedBehaviour>();

                        do { }
                        while (ComponentUtility.MoveComponentUp(_behaviour));
                    }
                }
            }
        }

        public override void OnInspectorGUI()
        {
            try
            {
                // Top method drawers.
                foreach (MethodDrawerGroup _group in methodDrawerGroups)
                {
                    _group.DrawMethodDrawers(true);
                }

                // Inspector.
                bool _drawInspector = true;
                foreach (UnityObjectDrawer _drawer in objectDrawers)
                {
                    if (_drawer.OnInspectorGUI())
                    {
                        _drawInspector = false;
                        break;
                    }
                }

                if (_drawInspector)
                    base.OnInspectorGUI();

                GUILayout.Space(10f);

                // Bottom method drawers.
                foreach (MethodDrawerGroup _group in methodDrawerGroups)
                {
                    _group.DrawMethodDrawers(false);
                }
            }
            catch (InvalidOperationException) { }
        }

        protected virtual void OnDisable()
        {
            foreach (var _drawer in objectDrawers)
                _drawer.OnDisable();
        }
        #endregion
    }
}
