using EnhancedEditor;
using UnityEngine;

#pragma warning disable
public class Example : MonoBehaviour
{
    /**************************
     *******   FIELDS   *******
     *************************/

    [HorizontalLine(1, order = 0)]
    [Section("NEW SUPER SECTION MUCH TOO MUCH TOO MUCH TOO LONG", 50, 0, order = 1)]
    [Space(order = 2)]

    [SerializeField]
    private string myString = "This is a string";

    [HorizontalLine(2, SuperColor.Red)]
    [SerializeField]
    private string myString2 = "This is another string";

    [HorizontalLine(2, SuperColor.Sapphire)]

    [SerializeField, PropertyField]
    private float myPropertyField = .75f;

    [HorizontalLine(2, SuperColor.Green)]
    [SerializeField, PropertyField("MyPropertyField")]
    private int myInt = 3;

    [HelpBox("This is a help box", HelpBoxType.Warning)]

    [HorizontalLine(2, SuperColor.Chocolate)]
    [SerializeField, ReadOnly]
    private bool myBool = false;

    [HorizontalLine(2, SuperColor.Lavender)]
    [SerializeField, Required]
    private Sprite mySprite = null;

    [HorizontalLine(2, SuperColor.HarvestGold)]
    [SerializeField, ProgressBar("MAGIC", "maxValue", SuperColor.Crimson, 35, true)]
    private float myProgressBar = 7;

    private int maxValue = 30;

    [HorizontalLine(2, SuperColor.Crimson)]
    [SerializeField, AssetPreview]
    private GameObject myAssetPreview = null;


    /**************************
     *****   PROPERTIES   *****
     *************************/

    public bool MyBool
    {
        get { return myBool; }
        private set
        {
            Debug.Log("Set bool => " + value);

            #if UNITY_EDITOR
            if (!Application.isPlaying) return;
            #endif

            Debug.Log("Do it");
        }
    }

    public float MyPropertyField { set { Debug.Log("Property Set => " + value); } }
}
