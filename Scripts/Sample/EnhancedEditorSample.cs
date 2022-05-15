// ===== Enhanced Editor - https://github.com/LucasJoestar/EnhancedEditor ===== //
// 
// Notes:
//
// ============================================================================ //

using UnityEngine;

namespace EnhancedEditor.Sample
{
    public interface ISerializable { }

    #pragma warning disable 0414
    public class EnhancedEditorSample : MonoBehaviour, ISerializable
    {
        [System.Serializable]
        public class Sample
        {
            [Enhanced, Required] public GameObject DoYouSeeMe = null;
        }

        [Section("SAMPLE SCRIPT")]

        [Enhanced, Folder] public string Folder = string.Empty;
        public SceneBundle bundle = null;

        [Enhanced, EndFoldout]
        [BeginFoldout("Small Group"), BeginFoldout("Wrapper")]

        [AssetPreview] public GameObject AssetPreview = null;

        [Enhanced, EndFoldout, EndFoldout] public GameObject Ender = null;

        [Enhanced, BeginFoldout("Large Group")]

        [Block, DisplayName("This is a Block", "Yes it is")] public EnhancedEditorAdvancedSample.SampleClass BlockClass = null;
        [Enhanced, Block] public Sample MySample = null;

        public bool Boolean = true;

        [Enhanced, ShowIf("Boolean", ConditionType.False), HelpBox("This is a help box. My content is very long, so make sure my width is supported for dynamic height.", MessageType.Info, true)]
        public int HelpBox = 0;

        [Enhanced, Inline] public Sample Inline = null;
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

        public float MaxProperty => 100f;

        public float ValidationProperty
        {
            set
            {
                Debug.Log("Value => " + ValidationField);
            }
        }

        public SerializedInterface<ISerializable> Interface = null;

        [Button(SuperColor.Green)]
        public void InterfaceTest()
        {
            var _interface = Interface.Interface;
            Debug.LogError("Interface => " + (_interface != null));
        }

        /*[SerializeField, BeginFoldout("GLOBAL", order = 0), Required(order = 1)] private Transform required = null;
        [SerializeField, Required, AssetPreview] private GameObject preview = null;
        [SerializeField, Min(0), Max(100)] private float percent = 100f;
        [SerializeField, ProgressBar(100f, SuperColor.Crimson, true)] private float progressBar = 25f;
        [SerializeField, ValidationMember("Property")] private int property = 0;
        [SerializeField, ReadOnly] private float readonlyBool = 0;
        [SerializeField, EnhancedCurve(0, 0, 1, 1)] private AnimationCurve myCurve = new AnimationCurve();

        [SerializeField, PrecisionSlider(0f, 100f)] private float _precision = 0f;

        [SerializeField] private bool showCondition = false;
        [SerializeField, ShowIf("showCondition", ConditionType.False)] private GameObject hiddenObject = null;
        [SerializeField] private string niceText = "Nice Text";

        [SerializeField, BeginFoldout("TEST", SuperColor.Sapphire, order = 0), Section("FOLDERS", order = 1)] private bool test = false;
        [SerializeField, EndFoldout()] private int endTest = 5;
        [SerializeField, BeginFoldout("Foldout", SuperColor.Crimson, order = 0), Required(order = 1)] private GameObject withinFoldout = null;
        [SerializeField] private string inFoldout = "Foldout String";
        [SerializeField, BeginFoldout("#2 Foldout", SuperColor.Pumpkin, order = 0), HorizontalLine(SuperColor.Black, 2f, order = 1)] private float secondFoldout = 10f;
        [SerializeField, EndFoldout(), EndFoldout()] private GameObject withinFoldout2 = null;
        [SerializeField] private string endFoldout = "End Foldout String";
        [SerializeField] private string inBetweenFoldout = "Foldout??";
        [SerializeField, BeginFoldout("#3rd", SuperColor.Indigo), EndFoldout()] private string finalFoldout = "Final Foldout String";

        [SerializeField] private string text = "Text";
        [SerializeField, EndFoldout()] private string text2 = "Text";

        public int Property
        {
            get => property;
            set
            {
                property = Mathf.Clamp(value, 0, 100);
            }
        }

        public bool ShowProperty => showCondition;
        public bool ShowMethod() => showCondition;*/

        /*[Button(ActivationMode.Editor, "ShowProperty", ConditionType.False, SuperColor.Aquamarine)]
        public void ButtonMethod()
        {
            this.LogError("Call Method => " + (Interface.Interface != null));
        }

        [Button(ActivationMode.Always, SuperColor.Raspberry)]
        public void ButtonMethodWithParameters(Transform _reference, string _message = "Message")
        {
            _reference.LogError($"Call Method => {_message}");
        }*/
    }
}
