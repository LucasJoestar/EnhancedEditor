// ===== Enhanced Editor - https://github.com/LucasJoestar/EnhancedEditor ===== //
// 
// Notes:
//
// ============================================================================ //

using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace EnhancedEditor
{
    /// <summary>
    /// Base class for bundle asynchronous operation inherited by
    /// <see cref="LoadBundleAsyncOperation"/> and <see cref="UnloadBundleAsyncOperation"/>.
    /// <para/> You should simply ignore this.
    /// </summary>
    public abstract class BundleAsyncOperation : CustomYieldInstruction, IComparable<BundleAsyncOperation>
    {
        #region Global Members
        protected AsyncOperation currentOperation = null;
        protected SceneBundle bundle = null;
        protected int sceneIndex = 0;

        protected bool isDone = false;
        protected float progress = 0f;
        protected int priority = 0;

        public abstract bool IsDone { get; }

        /// <summary>
        /// The <see cref="SceneBundle"/> associated with thtis operation.
        /// </summary>
        public SceneBundle Bundle {
            get {
                return bundle;
            }
        }

        /// <summary>
        /// This full operation progress.
        /// <br/> Starts at 0.0 and finishes when this value reaches 1.0.
        /// </summary>
        public float Progress
        {
            get
            {
                float _value = isDone
                             ? 1f
                             : (progress + (currentOperation.progress / bundle.Scenes.Length));

                return _value;
            }
        }

        /// <summary>
        /// Priority lets you tweak in which order async operation calls will be performed.
        /// </summary>
        public int Priority
        {
            get => priority;
            set
            {
                if (!isDone)
                    currentOperation.priority = value;

                priority = value;
            }
        }

        public override bool keepWaiting => !isDone;
        #endregion

        #region Comparison
        int IComparable<BundleAsyncOperation>.CompareTo(BundleAsyncOperation _other) {
            return _other.priority.CompareTo(priority);
        }
        #endregion

        #region Behaviour
        protected abstract void OnOperationComplete(AsyncOperation _operation);
        #endregion
    }

    /// <summary>
    /// Asynchronous operation used for loading a <see cref="SceneBundle"/>.
    /// <br/> Can be used as a yield instruction, checked manually to know when the operation is done,
    /// or used to get a callback on specific steps.
    /// </summary>
    public class LoadBundleAsyncOperation : BundleAsyncOperation
    {
        public static LoadBundleAsyncOperation CompleteOperation => new LoadBundleAsyncOperation();

        #region Global Members
        /// <summary>
        /// Called whenver a scene in this bundle has been fully loaded.
        /// </summary>
        public event Action<Scene> OnSceneLoaded = null;

        /// <summary>
        /// Called once all scenes in this bundle have been fully loaded.
        /// </summary>
        public event Action<LoadBundleAsyncOperation> OnCompleted = null;

        private readonly LoadSceneParameters parameters = default;
        private bool allowFirstSceneActivation = true;

        /// <summary>
        /// Has the operation finished, that is all scenes in this bundle been fully loaded?
        /// </summary>
        public override bool IsDone => isDone;

        /// <summary>
        /// Allows the first scene of the bundle being loaded to be
        /// activated as soon as it is ready (true by default).
        /// </summary>
        public bool AllowFirstSceneActivation
        {
            get => allowFirstSceneActivation;
            set
            {
                if (sceneIndex == 0)
                    currentOperation.allowSceneActivation = value;

                allowFirstSceneActivation = value;
            }
        }

        /// <summary>
        /// When <see cref="AllowFirstSceneActivation"/> is set to false,
        /// indicates if the first scene has been loaded and is waiting to be activated.
        /// </summary>
        public bool IsFirstSceneReady
        {
            get
            {
                bool _value = (sceneIndex == 0) && (currentOperation.progress >= .9f);
                return _value;
            }
        }
        #endregion

        #region Behaviour
        internal LoadBundleAsyncOperation()
        {
            isDone = true;
            progress = 1f;
        }

        internal LoadBundleAsyncOperation(SceneBundle _bundle, LoadSceneParameters _parameters)
        {
            bundle = _bundle;
            parameters = new LoadSceneParameters(LoadSceneMode.Additive, _parameters.localPhysicsMode);

            // Launch first operation.
            if (LoadNextScene(_parameters))
            {
                currentOperation.completed += OnOperationComplete;
            }
        }

        // -----------------------

        protected override void OnOperationComplete(AsyncOperation _operation)
        {
            // Loaded scene callback.
            if (OnSceneLoaded != null)
            {
                int _sceneIndex = bundle.Scenes[sceneIndex].BuildIndex;
                Scene _scene = SceneManager.GetSceneByBuildIndex(_sceneIndex);

                OnSceneLoaded.Invoke(_scene);
            }

            // Scene activation.
            // Do not use the OnSceneLoaded callback to avoid extra costs.
            if (bundle.ActiveSceneIndex == sceneIndex)
            {
                int _sceneIndex = bundle.Scenes[sceneIndex].BuildIndex;
                Scene _scene = SceneManager.GetSceneByBuildIndex(_sceneIndex);

                SceneManager.SetActiveScene(_scene);
            }

            sceneIndex++;

            // Complete once all scenes have been loaded.
            if (sceneIndex == bundle.Scenes.Length)
            {
                // Set active scene.
                isDone = true;
                progress = 1f;

                OnCompleted?.Invoke(this);

                return;
            }

            // Load next scene.
            if (LoadNextScene(parameters))
            {
                currentOperation.completed += OnOperationComplete;
                currentOperation.priority = priority;

                progress = (float)bundle.Scenes.Length / sceneIndex;
            }
        }

        private bool LoadNextScene(LoadSceneParameters _parameters)
        {
            while (!bundle.Scenes[sceneIndex].LoadAsync(out currentOperation, _parameters))
            {
                sceneIndex++;

                // Complete operation.
                if (sceneIndex == bundle.Scenes.Length)
                {
                    isDone = true;
                    progress = 1f;

                    OnCompleted?.Invoke(this);

                    return false;
                }
            }

            return true;
        }
        #endregion
    }

    /// <summary>
    /// Asynchronous operation used for unloading a <see cref="SceneBundle"/>.
    /// <br/> Can be used as a yield instruction, checked manually to know when the operation is done,
    /// or used to get a callback on specific steps.
    /// </summary>
    public class UnloadBundleAsyncOperation : BundleAsyncOperation
    {
        public static UnloadBundleAsyncOperation CompleteOperation => new UnloadBundleAsyncOperation();

        #region Global Members
        /// <summary>
        /// Called whenver a scene in this bundle has been unloaded.
        /// </summary>
        public event Action<Scene> OnSceneUnloaded = null;

        /// <summary>
        /// Called once all scenes in this bundle have been fully unloaded.
        /// </summary>
        public event Action<UnloadBundleAsyncOperation> OnCompleted = null;

        private readonly UnloadSceneOptions options = 0;

        /// <summary>
        /// Has the operation finished, that is all scenes in this bundle been fully unloaded?
        /// </summary>
        public override bool IsDone => isDone;
        #endregion

        #region Behaviour
        internal UnloadBundleAsyncOperation()
        {
            isDone = true;
            progress = 1f;
        }

        internal UnloadBundleAsyncOperation(SceneBundle _bundle, UnloadSceneOptions _options)
        {
            bundle = _bundle;
            options = _options;

            // Launch first operation.
            if (UnloadNextScene())
            {
                currentOperation.completed += OnOperationComplete;
            }
        }

        // -----------------------

        protected override void OnOperationComplete(AsyncOperation _operation)
        {
            // Unloaded scene callback.
            if (OnSceneUnloaded != null)
            {
                int _sceneIndex = bundle.Scenes[sceneIndex].BuildIndex;
                Scene _scene = SceneManager.GetSceneByBuildIndex(_sceneIndex);

                OnSceneUnloaded.Invoke(_scene);
            }

            sceneIndex++;

            // Complete once all scenes have been unloaded.
            if (sceneIndex == bundle.Scenes.Length)
            {
                isDone = true;
                progress = 1f;

                OnCompleted?.Invoke(this);

                return;
            }

            // Unload next scene.
            if (UnloadNextScene())
            {
                currentOperation.completed += OnOperationComplete;
                currentOperation.priority = priority;

                progress = (float)bundle.Scenes.Length / sceneIndex;
            }
        }

        private bool UnloadNextScene()
        {
            bool _canUnloadScene;
            while (!(_canUnloadScene = SceneManager.sceneCount > 1) || !bundle.Scenes[sceneIndex].UnloadAsync(out currentOperation, options))
            {
                sceneIndex++;

                // Complete operation.
                if (!_canUnloadScene || (sceneIndex == bundle.Scenes.Length))
                {
                    isDone = true;
                    progress = 1f;

                    OnCompleted?.Invoke(this);

                    return false;
                }
            }

            return true;
        }
        #endregion
    }
}
