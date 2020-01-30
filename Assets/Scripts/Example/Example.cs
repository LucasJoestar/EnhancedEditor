using EnhancedEditor;
using UnityEngine;

public class Example : MonoBehaviour
{
    [HorizontalLine(1, order = 0)]
    [Section("NEW SUPER SECTION MUCH TOO MUCH TOO MUCH TOO LONG", order = 1)]
    [Space(order = 2)]

    [SerializeField]
    private string myString = "This is a string";

    [HorizontalLine(2, SuperColor.Red)]

    [SerializeField]
    private string myString2 = "This is another string";

    [HorizontalLine(2, SuperColor.Sapphire)]

    [SerializeField]
    private float myFloat = .75f;

    [HorizontalLine(2, SuperColor.Green)]

    [SerializeField]
    private float myInt = 3f;
}
