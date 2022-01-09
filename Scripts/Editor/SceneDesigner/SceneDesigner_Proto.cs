// ===== Enhanced Editor - https://github.com/LucasJoestar/EnhancedEditor ===== //
// 
// Notes:
//
// ============================================================================ //

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEditor;

using Object = UnityEngine.Object;

namespace EnhancedEditor.Editor
{
    public class SceneDesigner_Proto : EditorWindow
    {
        /* SceneDesigner :
        *
        * Window utilities smoothing scene edition and design
        * 
        * Next Time Work :
        *       - Optimiser le collider virtuel
        *       - Sauvegarder les paramètres de la fenêtre
        *       - Créer un système de mesh visible uniquement en éditeur
        *       - Améliorer l'interface
        *       - Créer une fenêtre de configuration permettant de personnaliser des raccourcis
        *       - Améliorer le système de rotation et de changement de taille
        *       - Placer un objet lorsque le bouton de la souris est relâché
        * 
        * Features :
        *       - Place mulitple game objects and prefabs in the scene more easier and preciesly and fast than before
        */

        #region Global Members
        [SerializeField] private bool isPlacingObject = false;
        /// <summary>
        /// Is the user currently placing objects in the scene or not ?
        /// </summary>
        public bool IsPlacingObject
        {
            get { return isPlacingObject; }
            set
            {
                if (!placeable) value = false;
                isPlacingObject = value;

                Repaint();
            }
        }

        // Is the user currently selecting a new placeable ?
        [SerializeField] private bool isSelectingPlaceable = false;
        /// <summary>
        /// Is the user currently selecting a new placeable ?
        /// </summary>
        public bool IsSelectingPlaceable
        {
            get { return isSelectingPlaceable; }
        }

        // Is the selection window currently opened ?
        private bool isSelectionWindowOpened = false;

        // Is the placeable locked ? When locked, placeable does not follow mouse position and allows Transform Tools use
        [SerializeField] private bool isPlaceableLocked = false;
        /// <summary>
        /// Is the placeable locked ?
        /// When locked, placeable does not follow mouse position and allows Transform Tools use
        /// </summary>
        public bool IsPlaceableLocked
        {
            get { return isPlaceableLocked; }
            set
            {
                isPlaceableLocked = value;
                Repaint();
                if (value == true) Selection.activeGameObject = null;
            }
        }

        // -----------------------

        // The last selected game object to place in the scene
        [SerializeField] private GameObject placeable = null;
        /// <summary>
        /// The last selected game object to place in the scene
        /// </summary>
        public GameObject Placeable
        {
            get { return placeable; }
            set
            {
                placeable = value;

                isPlacingObject = value;

                IsPlaceableLocked = false;

                // Reset placeable meshes & collider
                placeableMeshes.Clear();
                placeableCollider = new VirtualBoxCollider();

                // If a new non null placeable is selected, get informations about it
                if (value != null)
                {
                    // Get the rotation & scale of the placeable to use for preview as default
                    placeablePreviewRotation = placeable.transform.rotation;
                    placeablePreviewScale = placeable.transform.localScale;

                    // Update mesh and virtual collider values

                    Vector2 _xMinMax = Vector2.zero;
                    Vector2 _yMinMax = Vector2.zero;
                    Vector2 _zMinMax = Vector2.zero;

                    MeshRenderer _meshRenderer = null;
                    MeshFilter[] _meshFilters = value.GetComponentsInChildren<MeshFilter>().ToArray();
                    if (_meshFilters.Length > 0)
                    {
                        _xMinMax = new Vector2(_meshFilters[0].sharedMesh.bounds.center.x - _meshFilters[0].sharedMesh.bounds.extents.x, _meshFilters[0].sharedMesh.bounds.center.x + _meshFilters[0].sharedMesh.bounds.extents.x);
                        _yMinMax = new Vector2(_meshFilters[0].sharedMesh.bounds.center.y - _meshFilters[0].sharedMesh.bounds.extents.y, _meshFilters[0].sharedMesh.bounds.center.y + _meshFilters[0].sharedMesh.bounds.extents.y);
                        _zMinMax = new Vector2(_meshFilters[0].sharedMesh.bounds.center.z - _meshFilters[0].sharedMesh.bounds.extents.z, _meshFilters[0].sharedMesh.bounds.center.z + _meshFilters[0].sharedMesh.bounds.extents.z);
                    }
                    foreach (MeshFilter _meshFilter in _meshFilters)
                    {
                        Vector3 _center = _meshFilter.sharedMesh.bounds.center + (_meshFilter.transform.position - placeable.transform.position);
                        Vector3 _extents = _meshFilter.sharedMesh.bounds.extents;

                        if (_center.x - _extents.x < _xMinMax.x) _xMinMax.x = _center.x - _extents.x;
                        if (_center.x + _extents.x > _xMinMax.y) _xMinMax.y = _center.x + _extents.x;
                        if (_center.y - _extents.y < _yMinMax.x) _yMinMax.x = _center.y - _extents.y;
                        if (_center.y + _extents.y > _yMinMax.y) _yMinMax.y = _center.y + _extents.y;
                        if (_center.z - _extents.z < _zMinMax.x) _zMinMax.x = _center.z - _extents.z;
                        if (_center.z + _extents.z > _zMinMax.y) _zMinMax.y = _center.z + _extents.z;

                        if (_meshRenderer = _meshFilter.GetComponent<MeshRenderer>())
                        {
                            placeableMeshes.Add(new PlaceableMeshInfos(_meshFilter.sharedMesh, _meshRenderer.sharedMaterial, placeable.transform, _meshFilter.transform));
                        }
                        else
                        {
                            placeableMeshes.Add(new PlaceableMeshInfos(_meshFilter.sharedMesh, placeable.transform, _meshFilter.transform));
                        }
                    }

                    placeableCollider.Extents = new Vector3(_xMinMax.y - _xMinMax.x, _yMinMax.y - _yMinMax.x, _zMinMax.y - _zMinMax.x) / 2;
                    placeableCollider.CenterPosition = new Vector3(_xMinMax.x + placeableCollider.Extents.x, _yMinMax.x + placeableCollider.Extents.y, _zMinMax.x + placeableCollider.Extents.z);
                }

                // Get if the placeable has mesh to draw or not
                hasPlaceableMesh = placeableMeshes.Count > 0;

                // Repaint the window
                Repaint();
            }
        }

        // The placeable virtual box collider used to place it in the scene
        [SerializeField] private VirtualBoxCollider placeableCollider = new VirtualBoxCollider();
        /// <summary>
        /// The placeable virtual box collider used to place it in the scene
        /// </summary>
        public VirtualBoxCollider PlaceableCollider
        {
            get { return placeableCollider; }
        }

        // All the placeable meshes informations
        [SerializeField] private List<PlaceableMeshInfos> placeableMeshes = new List<PlaceableMeshInfos>();
        /// <summary>
        /// All the placeable meshes informations
        /// </summary>
        public List<PlaceableMeshInfos> PlaceableMeshes
        {
            get { return placeableMeshes; }
        }

        // Does the placeable has mesh to draw
        [SerializeField] private bool hasPlaceableMesh = false;
        /// <summary>
        /// Does the placeable has mesh to draw
        /// </summary>
        public bool HasPlaceableMesh
        {
            get { return hasPlaceableMesh; }
        }

        // Position where to preview the placeable in the scene
        private Vector3 placeablePreviewPosition = new Vector3();
        /// <summary>
        /// Position where to preview the placeable in the scene
        /// </summary>
        public Vector3 PlaceablePreviewPosition
        {
            get { return placeablePreviewPosition; }
            set
            {
                placeablePreviewPosition = value;
            }
        }

        // Rotation used to preview the placeable in the scene
        [SerializeField] private Quaternion placeablePreviewRotation = Quaternion.identity;
        /// <summary>
        /// Rotation used to preview the placeable in the scene
        /// </summary>
        public Quaternion PlaceablePreviewRotation
        {
            get { return placeablePreviewRotation; }
            set
            {
                placeablePreviewRotation = value;
            }
        }

        // Scale used to preview the placeable in the scene
        [SerializeField] private Vector3 placeablePreviewScale = Vector3.one;
        /// <summary>
        /// Scale used to preview the placeable in the scene
        /// </summary>
        public Vector3 PlaceablePreviewScale
        {
            get { return placeablePreviewScale; }
            set
            {
                placeablePreviewScale = value;
            }
        }

        // -----------------------

        /// <summary>
        /// Key code used to lock / unlock the placeable in the scene
        /// When locked, the placeable stop following the moise and the user can change its position, rotation and scale using the Transform Tools
        /// </summary>
        public KeyCode LockPlaceableKey = KeyCode.LeftShift;

        /// <summary>
        /// Key code used to place a locked placeable in the scene
        /// </summary>
        public KeyCode PlaceLockedPlaceableKey = KeyCode.F;
        #endregion

        #region Custom
        // Opens the placeable selection window
        private void OpenPlaceableSelectionWindow(Vector2 _scenePosition, Rect _buttonRect)
        {
            // If the user was already selecting a placeable, do not open another selection window
            if (isSelectingPlaceable && !isSelectionWindowOpened)
            {
                isSelectingPlaceable = false;
                return;
            }
            else
            {
                isSelectingPlaceable = true;
                isSelectionWindowOpened = true;
            }

            // Get the rect to give to the window
            Rect _windowRect = new Rect((_scenePosition + new Vector2(_buttonRect.position.x + 3, _buttonRect.position.y + 37)), _buttonRect.size);

            // Creates a new placeable selection window and shows it as drop down at the button rect
            PlaceableSelectionWindow _selectionWindow = CreateInstance<PlaceableSelectionWindow>();
            _selectionWindow.Init(this, _windowRect, new Vector2(250, 300));

            // When the selection window is closed, user is no longer selecting a placeable
            _selectionWindow.OnClosed += (() => isSelectionWindowOpened = false);
        }

        // Place the placeable in the scene at the preview position
        private void PlacePlaceable()
        {
            GameObject _instance = PrefabUtility.InstantiatePrefab(placeable) as GameObject;
            _instance.transform.position = placeablePreviewPosition;
            _instance.transform.rotation = placeablePreviewRotation;
            _instance.transform.localScale = placeablePreviewScale;

            Undo.RegisterCreatedObjectUndo(_instance, "placeable instantiation");

            if (!isPlaceableLocked)
            {
                GUIUtility.hotControl = 0;
                Selection.activeGameObject = _instance;
            }
        }
        #endregion

        #region Unity Events Methods
        // Draw meshes before any camera starts culling
        private void DrawMeshes(Camera _camera)
        {
            // If not placing object or there is a hot control, do not placeable meshes for preview
            if (!isPlacingObject || (GUIUtility.hotControl != 0 && !isPlaceableLocked)) return;

            // If placeable has mesh(es), draw each mesh of the placeable at the preview point
            if (hasPlaceableMesh)
            {
                // Create variable
                Matrix4x4 _previewMatrix = new Matrix4x4();

                foreach (PlaceableMeshInfos _meshInfos in placeableMeshes)
                {
                    // X = 1 | Y = 0 | Z = 0
                    // 
                    // Axe Z =>
                    //          X = (X * Cos(Z)) ?? + (Y * (Sin(Z))
                    //          Y = (X * Sin(Z)) ?? + (Y * (Cos(Z))
                    // Axe Y =>
                    //          X = (X * Cos(Y)) ?? + (Z * Sin(Y))
                    //          Z = (X * Sin(Y)) ?? + (Z * Cos(Y))
                    // ???
                    // Axe X =>
                    //          Y = (Y * Cos(X)) + (Z * Sin(X))
                    //          Z = (Z * Cos(X)) + (Y * Sin(X))
                    //
                    //      X = (((X * Cos(Z)) ?? + (Y * (Sin(Z))) + ((X * Cos(Y)) ?? + (Z * Sin(Y)))) / 2
                    // 
                    // Que se passe-t-il si l'objet a une rotation non nulle dès le départ ?

                    Vector3 _position = placeablePreviewPosition + _meshInfos.Position;

                    // Set the matrix to use for preview
                    _previewMatrix.SetTRS(_position, Quaternion.Euler(placeablePreviewRotation.eulerAngles + _meshInfos.Rotation.eulerAngles), Vector3.Scale(placeablePreviewScale, _meshInfos.Scale));

                    // Draw all placeable meshes in preview on layout ignore raycast
                    Graphics.DrawMesh(_meshInfos.Mesh, _previewMatrix, _meshInfos.Material, 2, _camera);

                    // DEBUG
                    Handles.color = Color.magenta;
                    //Handles.DrawWireCube(placeablePreviewPosition + _meshInfos.Position + (_meshInfos.Mesh.bounds.center - placeable.transform.position), _meshInfos.Mesh.bounds.extents * 2);
                }
            }
        }

        // Scene view GUI delegate to draw things in the scene view windows
        private void OnSceneGUI(SceneView _sceneView)
        {
            // Get the current event
            Event _event = Event.current;

            // Begins a 2D GUI block to draw 2D things in the scene
            Handles.BeginGUI();

            // ********** PLACEABLE **********

            // Get Vector2 to draw buttons
            Vector2 _topMiddleScreenPos = new Vector2(Screen.width / 2, 0);

            Vector2 _buttonSize = new Vector2(30, 30);
            Vector2 _dropDownButtonSize = new Vector2(15, 30);

            // Get rects to draw buttons
            Rect _buttonRect = new Rect(new Vector2(_topMiddleScreenPos.x - (_buttonSize.x / 2), _topMiddleScreenPos.y), _buttonSize);
            Rect _dropDownButtonRect = new Rect(new Vector2(_topMiddleScreenPos.x + (_buttonSize.x / 2), _topMiddleScreenPos.y), _dropDownButtonSize);

            // Button enabling / disabling placeable tool
            if (GUI.Button(_buttonRect, "X"))
            {
                // If the user active the placing object tool and has no placeable selected, open the selection window
                if (!placeable)
                {
                    OpenPlaceableSelectionWindow(_sceneView.position.position, _buttonRect);
                }
                else
                {
                    IsPlacingObject = !IsPlacingObject;
                }
                return;
            }

            // Drop down button to select a new placeable
            if (EditorGUI.DropdownButton(_dropDownButtonRect, GUIContent.none, FocusType.Keyboard))
            {
                OpenPlaceableSelectionWindow(_sceneView.position.position, _buttonRect);
                return;
            }

            // Show others options if placing an object
            if (isPlacingObject)
            {
                // Get the rect of the button
                _buttonRect = new Rect(new Vector2(10, 10), _buttonSize);

                // Button to lock / unlock placeable
                GUI.color = isPlaceableLocked ? Color.grey : Color.white;

                if (GUI.Button(_buttonRect, "L"))
                {
                    isPlaceableLocked = !isPlaceableLocked;
                }

                GUI.color = Color.white;

                // If placeable is locked, show button to place the object
                if (isPlaceableLocked)
                {
                    // Get the rect of the button
                    _buttonRect = new Rect(new Vector2(_buttonRect.position.x + _buttonSize.x, _buttonRect.position.y), _buttonSize);

                    // Place the placeable on button click
                    if (GUI.Button(_buttonRect, "P"))
                    {
                        PlacePlaceable();
                    }

                    // Edit the position, rotation & scale of the placeable
                    Rect _positionRect = new Rect(10, 75, 200, 25);
                    Rect _rotationRect = new Rect(10, 100, 200, 25);
                    Rect _scaleRect = new Rect(10, 125, 200, 25);

                    GUI.Box(new Rect(5, 70, 210, 75), GUIContent.none);
                    EditorGUI.Vector3Field(_positionRect, string.Empty, placeablePreviewPosition);
                    EditorGUI.Vector3Field(_rotationRect, string.Empty, placeablePreviewRotation.eulerAngles);
                    EditorGUI.Vector3Field(_scaleRect, string.Empty, placeablePreviewScale);
                }
            }

            // Ends the 2D GUI block to get back to a 3D editing of the scene
            Handles.EndGUI();

            // If a placeable is selected
            if (isPlacingObject)
            {
                // If mouse is over scene view window
                if (mouseOverWindow == _sceneView)
                {
                    // If there is a hot control, just check if a placeable need to be placed
                    if (GUIUtility.hotControl != 0)
                    {
                        // If user click on the mouse left button, instantiate the placeable at the preview position
                        if ((!isPlaceableLocked && (_event.type == EventType.MouseDown && _event.button == 0)) || (isPlaceableLocked && (_event.type == EventType.KeyDown && _event.keyCode == PlaceLockedPlaceableKey)))
                        {
                            PlacePlaceable();
                        }
                    }

                    // If the placeable is not locked, set its position following the mouse
                    if (!isPlaceableLocked)
                    {
                        // Create variables
                        RaycastHit _hit = new RaycastHit();

                        // Get a ray going from mouse position to world point
                        Ray _mouseRay = HandleUtility.GUIPointToWorldRay(_event.mousePosition);

                        // If the ray hit something, draw the preview at this point, adjusted compared to the placeable collider
                        if (Physics.Raycast(_mouseRay, out _hit, 1000))
                        {
                            // Get the hit normal, rounded to the nearest int for each axis
                            Vector3 _roundedNormal = new Vector3(Mathf.RoundToInt(_hit.normal.x), Mathf.RoundToInt(_hit.normal.y), Mathf.RoundToInt(_hit.normal.z));

                            // Set the preview position relative to the virtual collider
                            placeablePreviewPosition = _hit.point - placeableCollider.CenterPosition + (Vector3.Scale(placeableCollider.Extents, _roundedNormal));
                        }
                        // If nothing is hit, draw the preview along the ray at a certain range from the camera
                        else
                        {
                            placeablePreviewPosition = _mouseRay.origin + (_mouseRay.direction * 25);
                        }

                        // Draw a position handle
                        Handles.color = Handles.xAxisColor;
                        Handles.ArrowHandleCap(0, placeablePreviewPosition, Quaternion.LookRotation(Vector3.right), 1, EventType.Repaint);
                        Handles.color = Handles.yAxisColor;
                        Handles.ArrowHandleCap(0, placeablePreviewPosition, Quaternion.LookRotation(Vector3.up), 1, EventType.Repaint);
                        Handles.color = Handles.zAxisColor;
                        Handles.ArrowHandleCap(0, placeablePreviewPosition, Quaternion.LookRotation(Vector3.forward), 1, EventType.Repaint);

                        // Draw the collider (Debug)
                        if (placeableCollider != null && placeableCollider.Extents != Vector3.zero && GUIUtility.hotControl == 0)
                        {
                            placeableCollider.DrawCollider(placeablePreviewPosition);
                        }
                    }
                }

                // On lock key press, lock or unlock the placeable
                if (_event.type == EventType.KeyDown && _event.keyCode == LockPlaceableKey)
                {
                    IsPlaceableLocked = !isPlaceableLocked;
                }

                // If the placeable is locked, allows the Transform Tools use
                if (isPlaceableLocked)
                {
                    switch (Tools.current)
                    {
                        case Tool.View:
                            // Do nothing
                            break;

                        case Tool.Move:
                            // Move object with position handle
                            EditorGUI.BeginChangeCheck();

                            Vector3 _newPosition = Handles.PositionHandle(placeablePreviewPosition, Tools.pivotRotation == PivotRotation.Global ? Quaternion.identity : placeablePreviewRotation);

                            if (EditorGUI.EndChangeCheck())
                            {
                                placeablePreviewPosition = _newPosition;
                            }
                            break;

                        case Tool.Rotate:
                            // Rotate object with rotation handle
                            EditorGUI.BeginChangeCheck();

                            Quaternion _newRotation = Handles.RotationHandle(placeablePreviewRotation, placeablePreviewPosition);

                            if (EditorGUI.EndChangeCheck())
                            {
                                placeablePreviewRotation = _newRotation;
                            }
                            break;

                        case Tool.Scale:
                            // Scale object with scale handle
                            EditorGUI.BeginChangeCheck();

                            Vector3 _newScale = Handles.ScaleHandle(placeablePreviewScale, placeablePreviewPosition, Tools.pivotRotation == PivotRotation.Global ? Quaternion.identity : placeablePreviewRotation, HandleUtility.GetHandleSize(placeablePreviewPosition));

                            if (EditorGUI.EndChangeCheck())
                            {
                                placeablePreviewScale = _newScale;
                            }
                            break;

                        case Tool.Rect:
                            // For now, do nothing
                            break;

                        case Tool.Transform:
                            // What is this ??
                            break;

                        case Tool.None:
                            // Do nothing
                            break;

                        default:
                            break;
                    }
                }
            }
        }
        #endregion

        #region Unity Menu Navigation
        /// <summary>
        /// Call existing Scene Designer window or create a new one and show it
        /// </summary>
        //[MenuItem(InternalUtility.MenuItemPath + "Scene Designer")]
        public static void CallWindow()
        {
            GetWindow<SceneDesigner_Proto>("Scene Designer", true).Show();
        }
        #endregion

        #region Unity Methods
        private void OnDisable()
        {
            // When disabled, removes this script OnSceneGUI & camera drawings
            SceneView.duringSceneGui -= OnSceneGUI;
            Camera.onPreCull -= DrawMeshes;

            // Save this window settings
            string _data = JsonUtility.ToJson(this, false);
            EditorPrefs.SetString("SceneDesigner", _data);
        }

        private void OnEnable()
        {
            // When enabled, refresh this script OnSceneGUI & camera drawings
            SceneView.duringSceneGui -= OnSceneGUI;
            SceneView.duringSceneGui += OnSceneGUI;
            Camera.onPreCull -= DrawMeshes;
            Camera.onPreCull += DrawMeshes;

            // Load this window settings
            string _data = EditorPrefs.GetString("SceneDesigner", JsonUtility.ToJson(this));
            JsonUtility.FromJsonOverwrite(_data, this);
        }

        private void OnGUI()
        {
            // Show the selected placeable and allow to change it
            EditorGUI.BeginChangeCheck();

            placeable = EditorGUILayout.ObjectField("Placeable : ", placeable, typeof(GameObject), true) as GameObject;

            // If changed, update it
            if (EditorGUI.EndChangeCheck())
            {
                Placeable = placeable;
            }

            // Set if the user is currently placing object or not
            IsPlacingObject = EditorGUILayout.Toggle("Placing Object :", isPlacingObject);

            // Set keyboard shortuct for locking placeable
            LockPlaceableKey = (KeyCode)EditorGUILayout.EnumPopup("Lock Key :", LockPlaceableKey);

            // Set keyboard shortuct for placing locked placeable
            PlaceLockedPlaceableKey = (KeyCode)EditorGUILayout.EnumPopup("Place locked placeable Key :", PlaceLockedPlaceableKey);

            // If placing object, set settings about it
            if (isPlacingObject)
            {
                // Set if the placeable is currently locked or not
                bool _isPlaceableLocked = EditorGUILayout.Toggle("Placeable Locked :", isPlaceableLocked);
                if (_isPlaceableLocked != isPlaceableLocked) IsPlaceableLocked = _isPlaceableLocked;
            }
        }

        void Update()
        {
            // If it's selecting placeable but the selection window is closed, it's no longer selecting placeable
            // ***** Event on window destroyed is called too soon, and OnSceneGUI is called in a strange way each frame, so here's a check in the update ; and that work *****
            if (isSelectingPlaceable && !isSelectionWindowOpened)
            {
                isSelectingPlaceable = false;
            }
        }
        #endregion
    }

    [Serializable]
    public class PlaceableMeshInfos
    {
        #region Global Members
        // Mesh, of this... mesh
        [SerializeField] private Mesh mesh = null;
        /// <summary>
        /// Mesh, of this... mesh
        /// </summary>
        public Mesh Mesh { get { return mesh; } }

        // Material of this mesh
        [SerializeField] private Material material = null;
        /// <summary>
        /// Material of this mesh
        /// </summary>
        public Material Material { get { return material; } }

        // Position relative to the placeable root transform
        [SerializeField] private Vector3 position = new Vector3();
        /// <summary>
        /// Position relative to the placeable root transform
        /// </summary>
        public Vector3 Position { get { return position; } }

        // Rotation relative to the placeable root transform
        [SerializeField] private Quaternion rotation = new Quaternion();
        /// <summary>
        /// Rotation relative to the placeable root transform
        /// </summary>
        public Quaternion Rotation { get { return rotation; } }

        // Scale relative to the placeable root transform
        [SerializeField] private Vector3 scale = new Vector3();
        /// <summary>
        /// Scale relative to the placeable root transform
        /// </summary>
        public Vector3 Scale { get { return scale; } }
        #endregion

        #region Constructors
        /// <summary>
        /// Creates a new placeable mesh infos, used to get infos about the selected placeable meshes
        /// </summary>
        /// <param name="_mesh">Mesh of the placeable</param>
        /// <param name="_placeableTransform">Placeable root transform</param>
        /// <param name="_meshTransform">Mesh transform</param>
        public PlaceableMeshInfos(Mesh _mesh, Transform _placeableTransform, Transform _meshTransform)
        {
            mesh = _mesh;
            position = _meshTransform.position - _placeableTransform.position;
            rotation = Quaternion.Euler(_meshTransform.rotation.eulerAngles - _placeableTransform.rotation.eulerAngles);
            scale = new Vector3(_meshTransform.lossyScale.x / _placeableTransform.localScale.x, _meshTransform.lossyScale.y / _placeableTransform.localScale.y, _meshTransform.lossyScale.z / _placeableTransform.localScale.z);
        }

        /// <summary>
        /// Creates a new placeable mesh infos, used to get infos about the selected placeable meshes
        /// </summary>
        /// <param name="_mesh">Mesh of the placeable</param>
        /// <param name="_material">Material used with the mesh</param>
        /// <param name="_placeableTransform">Placeable root transform</param>
        /// <param name="_meshTransform">Mesh transform</param>
        public PlaceableMeshInfos(Mesh _mesh, Material _material, Transform _placeableTransform, Transform _meshTransform)
        {
            mesh = _mesh;
            material = _material;
            position = _meshTransform.position - _placeableTransform.position;
            rotation = Quaternion.Euler(_meshTransform.rotation.eulerAngles - _placeableTransform.rotation.eulerAngles);
            scale = new Vector3(_meshTransform.lossyScale.x / _placeableTransform.lossyScale.x, _meshTransform.lossyScale.y / _placeableTransform.lossyScale.y, _meshTransform.lossyScale.z / _placeableTransform.lossyScale.z);
        }
        #endregion
    }

    [Serializable]
    public class VirtualBoxCollider
    {
        /* VirtualBoxCollider :
         * 
         * 
        */

        #region Fields / Properties
        // ********** Collider Settings **********

        // Center position of the collider (local space)
        [SerializeField] private Vector3 centerPosition = new Vector3();
        /// <summary>
        /// Center position of the collider (local space)
        /// </summary>
        public Vector3 CenterPosition
        {
            get { return centerPosition; }
            set
            {
                centerPosition = value;
            }
        }

        // Extents (half size) of the collider
        [SerializeField] private Vector3 extents = new Vector3();
        /// <summary>
        /// Extents (half size) of the collider
        /// </summary>
        public Vector3 Extents
        {
            get { return extents; }
            set
            {
                extents = value;
            }
        }
        #endregion

        #region Constructor
        /// <summary>
        /// Creates a new virtual collider at point zero, with a size of one
        /// </summary>
        public VirtualBoxCollider()
        {
            centerPosition = Vector3.zero;
            extents = Vector3.one / 2;
        }
        /// <summary>
        /// Creates a new virtual collider
        /// </summary>
        /// <param name="_centerPosition">Center position of the collider (local space)</param>
        /// <param name="_extents">Extents (half size) of the collider</param>
        public VirtualBoxCollider(Vector3 _centerPosition, Vector3 _extents)
        {
            centerPosition = _centerPosition;
            extents = _extents;
        }
        #endregion

        #region Methods
        /// <summary>
        /// Call this method in OnSceneGUI method to draw this collider in the scene
        /// </summary>
        /// <param name="_relativePosition">Position relative to the collider to draw it</param>
        public void DrawCollider(Vector3 _relativePosition)
        {
            Handles.color = Color.green;

            Handles.DrawWireCube(_relativePosition + centerPosition, extents * 2);

            Handles.color = Color.white;
        }
        /// <summary>
        /// Call this method in OnSceneGUI method to draw this collider in the scene
        /// </summary>
        /// <param name="_relativePosition">Position relative to the collider to draw it</param>
        /// <param name="_color">Color to draw the collider</param>
        public void DrawCollider(Vector3 _relativePosition, Color _color)
        {
            Handles.color = _color;

            Handles.DrawWireCube(_relativePosition + centerPosition, extents * 2);

            Handles.color = Color.white;
        }
        #endregion
    }

    public class PlaceableSelectionWindow : EditorWindow
    {
        /* PlaceableSelectionWindow :
         * 
         * [Write the Description of the Script here !]
        */

        #region Events
        // When this window is closed
        public event Action OnClosed = null;
        #endregion

        #region Fields / Properties
        // ********** Reference **********

        // The scene designer reference of this window
        private SceneDesigner_Proto sceneDesigner = null;

        // ********** Game Objects selection **********

        // Folders path and their files
        private Dictionary<string, SelectionWindowFolder> foldersFiles = new Dictionary<string, SelectionWindowFolder>();

        // ********* Editor Elements **********

        // Horizontal & Vertical scroll
        private Vector2 scroll = new Vector2();
        #endregion

        #region Methods
        #region Original Methods
        /// <summary>
        /// Initializes this window with a scene designer window and shows it as drop down
        /// </summary>
        /// <param name="_sceneDesigner">Scene designer reference</param>
        /// <param name="_buttonRect">Rect of the button to display at</param>
        /// <param name="_size">Initial size of the window</param>
        public void Init(SceneDesigner_Proto _sceneDesigner, Rect _buttonRect, Vector2 _size)
        {
            // Get the scene designer reference and show this window as drop down
            sceneDesigner = _sceneDesigner;
            ShowAsDropDown(_buttonRect, _size);

            // Get all game objects in prefab folders of this project

            // Create variables
            string _objPath = string.Empty;
            string _objName = string.Empty;

            // Get all objects in this project Prefabs folders (Get path from Asset Database, remove everything before)
            string[] _objects = Directory.EnumerateDirectories(Application.dataPath, "Prefabs", SearchOption.AllDirectories).SelectMany(d => Directory.GetFiles(d, "*", SearchOption.AllDirectories).Where(f => f.EndsWith(".fbx") || f.EndsWith(".prefab"))).Select(o => o.Remove(0, Application.dataPath.Length - 6)).ToArray();

            // Add each object with its directory to the list
            foreach (string _obj in _objects)
            {
                // Get object directory and name
                _objPath = Path.GetDirectoryName(_obj);
                _objName = Path.GetFileName(_obj);

                // Add the object to the list
                if (foldersFiles.ContainsKey(_objPath))
                {
                    if (!foldersFiles[_objPath].FolderFiles.Any(f => f.CompleteName == _objName))
                    {
                        foldersFiles[_objPath].FolderFiles.Add(new SelectionWindowFile(_obj));
                    }
                }
                else
                {
                    foldersFiles.Add(_objPath, new SelectionWindowFolder(_objPath, _obj));
                }
            }
        }
        #endregion

        #region Unity Methods
        // OnDestroy is called to close the EditorWindow window
        private void OnDestroy()
        {
            // Calls the on close event
            OnClosed?.Invoke();
        }

        // Implement your own editor GUI here
        private void OnGUI()
        {
            // Begins horizonal & vertical scroll
            scroll = EditorGUILayout.BeginScrollView(scroll);

            // Show each file in folders available to select
            foreach (KeyValuePair<string, SelectionWindowFolder> _foldFiles in foldersFiles)
            {
                // Folder indication
                _foldFiles.Value.IsVisible = EditorGUILayout.Foldout(_foldFiles.Value.IsVisible, _foldFiles.Value.DirectoryName);

                // Show all objects in folder if visible
                if (_foldFiles.Value.IsVisible)
                {
                    foreach (SelectionWindowFile _file in _foldFiles.Value.FolderFiles)
                    {
                        // Begins horizontal and adds space before the button
                        EditorGUILayout.BeginHorizontal();
                        GUILayout.Space(50);
                        // If the file has a thumbnail, draw it
                        if (_file.Thumbnail)
                        {
                            EditorGUI.DrawPreviewTexture(new Rect(GUILayoutUtility.GetLastRect().position + new Vector2(15, 2.5f), new Vector2(20, 20)), _file.Thumbnail);
                        }

                        // This file button
                        if (GUILayout.Button(_file.Name))
                        {
                            sceneDesigner.Placeable = AssetDatabase.LoadAssetAtPath<GameObject>(_foldFiles.Value.Directory + "\\" + _file.CompleteName);

                            Close();
                        }

                        // Ends horizontal
                        EditorGUILayout.EndHorizontal();
                    }
                }
            }

            // Ends horizontal & vertical scroll
            EditorGUILayout.EndScrollView();
        }
        #endregion
        #endregion
    }

    [Serializable]
    public class SelectionWindowFolder
    {
        /* SelectionWindowFolder :
         * 
         * 
        */

        #region Fields / Properties
        /// <summary>
        /// Directory path from Asset Database
        /// </summary>
        public string Directory = string.Empty;

        /// <summary>
        /// Directory name
        /// </summary>
        public string DirectoryName = string.Empty;

        /// <summary>
        /// All objects in this folder
        /// </summary>
        public List<SelectionWindowFile> FolderFiles = new List<SelectionWindowFile>();

        /// <summary>
        /// Boolean used to show / hide this folder in the window
        /// </summary>
        public bool IsVisible = false;
        #endregion

        #region Constructor
        /// <summary>
        /// Creates a new selection window folder with a specified folder and first value
        /// </summary>
        /// <param name="_folder">Folder path to use</param>
        /// <param name="_firstObject">First object of the folder to initialize with</param>
        public SelectionWindowFolder(string _folder, string _firstObject)
        {
            Directory = _folder;
            DirectoryName = Path.GetFileName(_folder);

            FolderFiles = new List<SelectionWindowFile>() { new SelectionWindowFile(_firstObject) };
        }
        #endregion
    }

    [Serializable]
    public class SelectionWindowFile
    {
        /* SelectionWindowFile :
         * 
         * 
        */

        #region Fields / Properties
        /// <summary>
        /// Complete name of the file, with extension
        /// </summary>
        public string CompleteName = string.Empty;

        /// <summary>
        /// Name of the file, without extension
        /// </summary>
        public string Name = string.Empty;

        /// <summary>
        /// Thumbnail of the element, used for preview
        /// </summary>
        public Texture2D Thumbnail = null;
        #endregion

        #region Constructor
        /// <summary>
        /// Creates a new selection window file from a file path
        /// </summary>
        /// <param name="_filePath">File path, from Asset Database</param>
        public SelectionWindowFile(string _filePath)
        {
            CompleteName = Path.GetFileName(_filePath);
            Name = Path.GetFileNameWithoutExtension(_filePath);

            Object _object = AssetDatabase.LoadAssetAtPath<Object>(_filePath);
            Thumbnail = AssetPreview.GetAssetPreview(_object);
            if (!Thumbnail && !AssetPreview.IsLoadingAssetPreview(_object.GetInstanceID())) Thumbnail = AssetPreview.GetMiniThumbnail(AssetDatabase.LoadAssetAtPath<Object>(_filePath));
        }
        #endregion
    }
}
