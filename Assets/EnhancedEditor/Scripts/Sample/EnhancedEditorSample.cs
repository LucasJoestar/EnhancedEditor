// ===== Enhanced Editor - https://github.com/LucasJoestar/EnhancedEditor ===== //
// 
// Notes:
//
// ============================================================================ //

using UnityEngine;

namespace EnhancedEditor.Sample
{
    #pragma warning disable 0414
    public class EnhancedEditorSample : MonoBehaviour
    {
        [Section("SAMPLE SCRIPT")]

        [SerializeField, BeginFoldout("GLOBAL", order = 0), Required(order = 1)] private Transform required = null;
        [SerializeField, Required, AssetPreview] private GameObject preview = null;
        [SerializeField, Min(0), Max(100)] private float percent = 100f;
        [SerializeField, ProgressBar("Exemple", 100f, SuperColor.Crimson, 25, true)] private float progressBar = 25f;
        [SerializeField, PropertyField] private int property = 0;
        [SerializeField, ReadOnly] private float readonlyBool = 0;
        [SerializeField, EnhancedCurve(SuperColor.Sapphire, 0, 0, 1, 1)] private AnimationCurve myCurve = new AnimationCurve();

        [SerializeField, EnhancedRange(0f, 100f)] private float _precision = 0f;

        [SerializeField] private bool showCondition = false;
        [SerializeField, ShowIf("showCondition", ConditionType.False)] private GameObject hiddenObject = null;
        [SerializeField] private string niceText = "Nice Text";

        [SerializeField, BeginFoldout("TEST", SuperColor.Sapphire, order = 0), Section("FOLDERS", order = 1)] private bool test = false;
        [SerializeField, EndFoldout()] private int endTest = 5;
        [SerializeField, BeginFoldout("Foldout", SuperColor.Crimson, order = 0), Required(order = 1)] private GameObject withinFoldout = null;
        [SerializeField] private string inFoldout = "Foldout String";
        [SerializeField, BeginFoldout("#2 Foldout", SuperColor.Pumpkin, order = 0), HorizontalLine(2, SuperColor.Black, order = 1)] private float secondFoldout = 10f;
        [SerializeField, EndFoldout()] private GameObject withinFoldout2 = null;
        [SerializeField, EndFoldout()] private string endFoldout = "End Foldout String";
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
        public bool ShowMethod() => showCondition;

        [Button(ActivationMode.Editor, "ShowProperty", ConditionType.False, SuperColor.Aquamarine)]
        public void ButtonMethod()
        {
            this.LogError("Call Method");
        }

        [Button(ActivationMode.Always, SuperColor.Raspberry)]
        public void ButtonMethodWithParameters(Transform _reference, string _message = "Message")
        {
            _reference.LogError($"Call Method => {_message}");
        }
    }
}
