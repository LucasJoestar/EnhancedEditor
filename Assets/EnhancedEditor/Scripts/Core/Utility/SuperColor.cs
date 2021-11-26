// ===== Enhanced Editor - https://github.com/LucasJoestar/EnhancedEditor ===== //
// 
// Notes:
//
// ============================================================================ //

using UnityEngine;

namespace EnhancedEditor
{
    /// <summary>
    /// Super color: use the extension method <see cref="SuperColorExtensions.Get(SuperColor, float)"/>
    /// to get associated color value.
    /// <para/>
    /// Example: <c>myColor.Get(<see cref="float"/>)</c>
    /// </summary>
    public enum SuperColor
    {
        White,
        Silver,
        Grey,
        DarkGrey,
        SmokyBlack,
        Black,
        Maroon,
        Brown,
        Chocolate,
        Red,
        Crimson,
        Orange,
        DarkOrange,
        Pumpkin,
        HarvestGold,
        Yellow,
        Lime,
        Green,
        Aquamarine,
        Turquoise,
        Cyan,
        Sapphire,
        Blue,
        Navy,
        Purple,
        Lavender,
        Indigo,
        Raspberry,
        SalmonPink
    }

    /// <summary>
    /// Contains multiple <see cref="SuperColor"/>-related extension methods.
    /// </summary>
    public static class SuperColorExtensions
    {
        /// <summary>
        /// Get the associated <see cref="Color"/> value.
        /// </summary>
        /// <param name="_superColor">Enum value to get color from.</param>
        /// <param name="_alpha"><see cref="Color"/> alpha value (from 0 to 1).</param>
        /// <returns>New <see cref="Color"/> associated with this <see cref="SuperColor"/>.</returns>
        public static Color Get(this SuperColor _superColor, float _alpha = 1f)
        {
            switch (_superColor)
            {
                // White to Black
                case SuperColor.White:
                    return new Color(1f, 1f, 1f, _alpha);

                case SuperColor.Silver:
                    return new Color(.753f, .753f, .753f, _alpha);

                case SuperColor.Grey:
                    return new Color(.502f, .502f, .502f, _alpha);

                case SuperColor.DarkGrey:
                    return new Color(.150f, .150f, .150f, _alpha);

                case SuperColor.SmokyBlack:
                    return new Color(.063f, .047f, .031f, _alpha);

                case SuperColor.Black:
                    return new Color(0f, 0f, 0f, _alpha);

                // Maroon to Red
                case SuperColor.Maroon:
                    return new Color(.502f, 0f, 0f, _alpha);

                case SuperColor.Brown:
                    return new Color(.588f, .294f, 0f, _alpha);

                case SuperColor.Chocolate:
                    return new Color(.482f, .247f, 0f, _alpha);

                case SuperColor.Red:
                    return new Color(1f, 0f, 0f, _alpha);

                case SuperColor.Crimson:
                    return new Color(.863f, .078f, .235f, _alpha);

                // Orange to Yellow
                case SuperColor.Orange:
                    return new Color(1f, .647f, .0f, _alpha);

                case SuperColor.DarkOrange:
                    return new Color(1f, .549f, .0f, _alpha);

                case SuperColor.Pumpkin:
                    return new Color(1f, .459f, .094f, _alpha);

                case SuperColor.HarvestGold:
                    return new Color(.855f, .568f, .0f, _alpha);

                case SuperColor.Yellow:
                    return new Color(1f, 1f, .0f, _alpha);

                // Lime to Turquoise
                case SuperColor.Lime:
                    return new Color(0f, 1f, .0f, _alpha);

                case SuperColor.Green:
                    return new Color(.133f, .8f, 0f, _alpha);

                case SuperColor.Aquamarine:
                    return new Color(.498f, 1f, .831f, _alpha);

                case SuperColor.Turquoise:
                    return new Color(.251f, .878f, .816f, _alpha);

                // Cyan to Navy
                case SuperColor.Cyan:
                    return new Color(0f, 1f, 1f, _alpha);

                case SuperColor.Sapphire:
                    return new Color(.059f, .322f, .729f, _alpha);

                case SuperColor.Blue:
                    return new Color(0f, 0f, 1f, _alpha);

                case SuperColor.Navy:
                    return new Color(0f, 0f, .502f, _alpha);

                // Purple to Pink
                case SuperColor.Purple:
                    return new Color(.627f, .125f, .941f, _alpha);

                case SuperColor.Lavender:
                    return new Color(.710f, .494f, .863f, _alpha);

                case SuperColor.Indigo:
                    return new Color(.294f, 0f, .510f, _alpha);

                case SuperColor.Raspberry:
                    return new Color(.890f, .043f, .365f, _alpha);

                case SuperColor.SalmonPink:
                    return new Color(1f, .869f, .643f, _alpha);

                // Return white as default color
                default:
                    return new Color(1f, 1f, 1f, _alpha);
            }
        }
    }
}
