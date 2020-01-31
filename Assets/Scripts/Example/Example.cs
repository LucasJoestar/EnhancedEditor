using EnhancedEditor;
using UnityEngine;

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


    /**************************
     *****   PROPERTIES   *****
     *************************/

    public float MyPropertyField
    {
        set
        {
            Debug.Log("Property Set");
        }
    }
}
