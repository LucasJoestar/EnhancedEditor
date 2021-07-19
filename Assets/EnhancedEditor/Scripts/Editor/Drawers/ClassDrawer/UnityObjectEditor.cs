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
            public MethodInfo MethodInfo = null;

            // -----------------------

            public void DrawMethodDrawers(bool _isTop)
            {
                Rect _position = EditorGUILayout.GetControlRect(false, -2f);

                // Pre GUI callback.
                for (int _i = 0; _i < MethodDrawers.Length; _i++)
                {
                    MethodDrawer _drawer = MethodDrawers[_i];
                    if ((_isTop == _drawer.Attribute.IsDrawnOnTop) && _drawer.OnBeforeGUI(_drawer.Label))
                        return;
                }

                // Method GUI.
                for (int _i = 0; _i < MethodDrawers.Length; _i++)
                {
                    MethodDrawer _drawer = MethodDrawers[_i];
                    if ((_isTop == _drawer.Attribute.IsDrawnOnTop) && _drawer.OnGUI(MethodInfo, _drawer.Label))
                        break;
                }

                // Post GUI callback.
                for (int _i = 0; _i < MethodDrawers.Length; _i++)
                {
                    MethodDrawer _drawer = MethodDrawers[_i];
                    if (_isTop == _drawer.Attribute.IsDrawnOnTop)
                        _drawer.OnAfterGUI(_drawer.Label);
                }

                // Context click menu. 
                Event _event = Event.current;
                _position.height = GUILayoutUtility.GetLastRect().yMax - _position.y;

                if ((_event.type == EventType.MouseDown) && (_event.button == 1) && _position.Contains(_event.mousePosition))
                {
                    GenericMenu _menu = new GenericMenu();
                    for (int _i = 0; _i < MethodDrawers.Length; _i++)
                        MethodDrawers[_i].OnContextMenu(_menu, MethodInfo);

                    if (_menu.GetItemCount() > 0)
                    {
                        _menu.ShowAsContext();
                        _event.Use();
                    }
                }
            }
        }
        #endregion

        #region Editor Content
        private static readonly BindingFlags methodInfoFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static;

        private UnityObjectDrawer[] objectDrawers = new UnityObjectDrawer[] { };
        private MethodDrawerGroup[] methodDrawerGroups = new MethodDrawerGroup[] { };

        // -----------------------

        public override void OnInspectorGUI()
        {
            // Draw inspector while authorized.
            for (int _i = 0; _i < objectDrawers.Length; _i++)
            {
                UnityObjectDrawer _drawer = objectDrawers[_i];
                if (!_drawer.OnInspectorGUI())
                    return;
            }

            // Draw top method drawers.
            for (int _i = 0; _i < methodDrawerGroups.Length; _i++)
            {
                MethodDrawerGroup _group = methodDrawerGroups[_i];
                _group.DrawMethodDrawers(true);
            }

            base.OnInspectorGUI();

            // Bottom method drawers.
            for (int _i = 0; _i < methodDrawerGroups.Length; _i++)
            {
                MethodDrawerGroup _group = methodDrawerGroups[_i];
                _group.DrawMethodDrawers(false);
            }
        }

        protected virtual void OnEnable()
        {
            try
            {
                if (serializedObject.targetObject == null)
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

                objectDrawers = new UnityObjectDrawer[] { };
                foreach (KeyValuePair<Type, Type> _pair in EnhancedDrawerUtility.GetCustomDrawers())
                {
                    foreach (EnhancedClassAttribute _attribute in _attributes)
                    {
                        if (_pair.Value == _attribute.GetType())
                        {
                            UnityObjectDrawer _customDrawer = UnityObjectDrawer.CreateInstance(_pair.Key, serializedObject, _attribute);
                            _customDrawer.OnEnable();

                            UnityEditor.ArrayUtility.Add(ref objectDrawers, _customDrawer);
                        }
                    }
                }
            }

            // Do the same for method attributes.
            {
                MethodInfo[] _allMethods = _type.GetMethods(methodInfoFlags);
                foreach (KeyValuePair<Type, Type> _pair in EnhancedDrawerUtility.GetMethodDrawers())
                {
                    foreach (MethodInfo _method in _allMethods)
                    {
                        var _attributes = _method.GetCustomAttributes(typeof(EnhancedMethodAttribute), true) as EnhancedMethodAttribute[];

                        // Skip methods without any desired attribute.
                        if (_attributes.Length == 0)
                            continue;

                        MethodDrawerGroup _group = new MethodDrawerGroup()
                        {
                            MethodInfo = _method
                        };

                        foreach (EnhancedMethodAttribute _attribute in _attributes)
                        {
                            if (_pair.Value == _attribute.GetType())
                            {
                                MethodDrawer _customDrawer = MethodDrawer.CreateInstance(_pair.Key, serializedObject, _attribute, _method);
                                _customDrawer.OnEnable();

                                UnityEditor.ArrayUtility.Add(ref _group.MethodDrawers, _customDrawer);
                            }
                        }

                        Array.Sort(_group.MethodDrawers, (a, b) => a.Attribute.Order.CompareTo(b.Attribute.Order));
                        UnityEditor.ArrayUtility.Add(ref methodDrawerGroups, _group);
                    }
                }
            }

            // Add GameObject-extended component if none is attached.
            foreach (var _object in serializedObject.targetObjects)
            {
                if (_object is Component _component)
                {
                    var _behaviour = _component.gameObject.AddComponentIfNone<EnhancedBehaviour>();

                    do { }
                    while (ComponentUtility.MoveComponentUp(_behaviour));
                }
            }
        }

        protected virtual void OnDisable()
        {
            for (int _i = 0; _i < objectDrawers.Length; _i++)
                objectDrawers[_i].OnDisable();
        }
        #endregion
    }
}
