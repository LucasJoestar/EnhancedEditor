// ===== Enhanced Editor - https://github.com/LucasJoestar/EnhancedEditor ===== //
// 
// Notes:
//
// ============================================================================ //

using System;
using UnityEngine;

namespace EnhancedEditor.Sample
{
    public interface MyInterface { }

    #pragma warning disable 0414
    [InstanceTracker]
    public class EnhancedEditorAdvancedSample : MonoBehaviour, MyInterface
    {
        [Serializable]
        public class SampleClass
        {
            [Enhanced, EnhancedTextArea] public string Name = "SubSample";
            public bool Enabled = true;

            //public Document.Section Section = null;
        }

        #region Content
        public GameObject AssetPreview = null;
        public SampleClass Readonly = null;
        public SampleClass Inline = null;
        public GameObject Picker = null;
        public Animator PickerAnimator = null;
        public Texture2D Required = null;
        public SerializedInterface<MyInterface> Interface = null;

        public Tag Tag = new Tag();
        public TagGroup TagGroup = new TagGroup();
        public SceneAsset SceneAsset = new SceneAsset();

        public float MinFloat = 7f;
        public int MaxInt = 5;
        public int ProgressBar = 90;
        public float PrecisionSlider = 7f;
        public float ValidationFloat = 10f;

        public bool Boolean = true;
        public string FolderPath = string.Empty;
        public string TextArea = "Text Area";

        public Color Color = Color.white;
        public Vector2 MinMaxFloat = new Vector2();
        public Vector2Int MinMaxInt = new Vector2Int();

        public float ValidationFloatProperty
        {
            get => ValidationFloat;
            set
            {
                ValidationFloat = value;
                Debug.Log("Validation Property => " + value);
            }
        }

        public int ProgressBarMax()
        {
            return 250;
        }

        public void ValidationFloatMethod(float _value)
        {
            Debug.Log("Validation Method => " + _value);
        }
        #endregion
    }
}
