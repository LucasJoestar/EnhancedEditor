// ===== Enhanced Editor - https://github.com/LucasJoestar/EnhancedEditor ===== //
// 
// Notes:
//
// ============================================================================ //

using System;
using UnityEngine;

namespace EnhancedEditor.AttributeSample {
    public interface ISerializable { }

    #pragma warning disable 0414
    public class EnhancedEditorAttributeSample : MonoBehaviour, ISerializable
    {
        #region Sample Class
        [Serializable]
        public class SampleClass
        {
            [Enhanced, Required] public GameObject FirstObject = null;
            [Enhanced, Required] public GameObject SecondObject = null;
        }
        #endregion

        #region Content
        [Section("SAMPLE SCRIPT")]

        [Enhanced, Folder] public string Folder = string.Empty;
        public SceneBundle Bundle = null;

        [Enhanced, EndFoldout]
        [BeginFoldout("Small Group"), BeginFoldout("Wrapper")]

        [AssetPreview] public GameObject AssetPreview = null;

        [Enhanced, EndFoldout, EndFoldout] public GameObject Ender = null;

        [Enhanced, BeginFoldout("Large Group")]

        [Block, DisplayName("This is a Block", "Yes it is")] public EnhancedEditorAdvancedAttributeSample.SampleClass BlockClass = null;
        [Enhanced, Block] public SampleClass MySample = null;

        public bool Boolean = true;

        [Enhanced, ShowIf("Boolean", ConditionType.False)]
        [HelpBox("This is a help box. My content is very long, so make sure my width is supported for dynamic height.", MessageType.Info, true)]
        public int HelpBox = 0;

        [Enhanced, Inline] public SampleClass Inline = null;
        [Enhanced, Min(0f), Max(100f)] public float MinMax = 500f;
        [Enhanced, MinMax(0f, 100f)] public Vector2 MinMaxField = new Vector2(3, 50);

        [Enhanced, ColorPalette(order = 0), EndFoldout(order = 1)]
        public Color MyColor = Color.red;

        [Enhanced, Required, Picker(typeof(Transform))] public GameObject Picker = null;
        [Enhanced, PrecisionSlider(0f, 100f, .5f)] public float Precision = 50f;
        [Enhanced, ProgressBar(100f, 25f, SuperColor.Chocolate, true)] public float ProgressBar = 10f;
        [Enhanced, ValidationMember("ValidationProperty", ActivationMode.Editor)] public float ValidationField = 10f;
        [Enhanced, ReadOnly(true)] public bool Readonly = true;

        [Enhanced, EnhancedCurve(0f, 0f, 1f, 1f, SuperColor.Crimson)] public AnimationCurve MyCurve = new AnimationCurve();
        [Enhanced, EnhancedTextArea] public string TextArea = "This is a text area.";

        [Enhanced, EnhancedBounds] public Bounds MyBounds = new Bounds();
        public string EndOfTheLine = "End of the Line";
        public SerializedInterface<ISerializable> Interface = null;

        // -----------------------

        public float MaxProperty => 100f;

        public float ValidationProperty
        {
            set
            {
                this.Log("Validation property value => " + ValidationField);
            }
        }

        // -----------------------

        [Button(SuperColor.Green)]
        public void TestInterface()
        {
            var _interface = Interface.Interface;
            this.Log("Is Interface valid => " + (_interface != null));
        }
        #endregion
    }
}
