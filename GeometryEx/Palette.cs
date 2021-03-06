using Elements.Geometry;
using System.Collections.Generic;

namespace GeometryEx
{
    /// <summary>
    /// Commonly used colors for Space rendering.
    /// These colors are translucent to allow viewing of representions several layers deep.
    /// </summary>
    /// TODO: Move To ELEMENTS
    public static class Palette
    {
        public static Dictionary<string, Color> Colors
        {
            get
            {
                return
                    new Dictionary<string, Color>
                    {
                        { "Amber",  Amber},
                        { "Aqua", Aqua },
                        { "Beige", Beige },
                        { "Black", Black },
                        { "Blue", Blue },
                        { "Brown", Brown },
                        { "Cobalt", Cobalt },
                        { "Coral", Coral },
                        { "Crimson", Crimson },
                        { "Cyan", Cyan },
                        { "Darkgray", Darkgray },
                        { "Emerald", Emerald },
                        { "Granite", Granite },
                        { "Gray", Gray },
                        { "Green", Green },
                        { "Lavender", Lavender },
                        { "Lime", Lime },
                        { "Magenta", Magenta },
                        { "Maroon", Maroon },
                        { "Mint", Mint },
                        { "Navy", Navy },
                        { "Ochre", Ochre },
                        { "Olive", Olive },
                        { "Orange", Orange },
                        { "Pink", Pink },
                        { "Purple", Purple },
                        { "Red", Red },
                        { "Sand", Sand },
                        { "Stone", Stone },
                        { "Teal", Teal },
                        { "White", White },
                        { "Yellow", Yellow }
                    };
            }
        }

        /// <summary>
        /// Returns all the Palette Colors as a list.
        /// </summary>
        /// <returns></returns>
        public static List<Color> ColorList()
        {
            var colors = new List<Color>();
            foreach (var key in Colors.Keys)
            {
                colors.Add(Colors[key]);
            }
            return colors;
        }

        /// <summary>
        /// Amber
        /// </summary>
        public static Color Amber => new Color(0.9f, 0.6f, 0.1f, 0.6f);

        /// <summary>
        /// Aqua
        /// </summary>
        public static Color Aqua => new Color(0.3f, 0.7f, 0.7f, 0.6f);

        /// <summary>
        /// Beige
        /// </summary>
        public static Color Beige => new Color(1.0f, 0.9f, 0.6f, 0.6f);

        /// <summary>
        /// Black
        /// </summary>
        public static Color Black => new Color(0.0f, 0.0f, 0.0f, 0.6f);

        /// <summary>
        /// Blue
        /// </summary>
        public static Color Blue => new Color(0.0f, 0.0f, 1.0f, 0.6f);

        /// <summary>
        /// Brown
        /// </summary>
        public static Color Brown => new Color(0.6f, 0.6f, 0.2f, 0.6f);

        /// <summary>
        /// Cobalt
        /// </summary>
        public static Color Cobalt => new Color(0.0f, 0.6f, 1.0f, 0.6f);

        /// <summary>
        /// Coral
        /// </summary>
        public static Color Coral => new Color(1.0f, 0.6f, 0.7f, 0.6f);

        /// <summary>
        /// Crimson
        /// </summary>
        public static Color Crimson => new Color(1.0f, 0.0f, 0.0f, 0.6f);

        /// <summary>
        /// Cyan
        /// </summary>
        public static Color Cyan => new Color(0.3f, 0.9f, 0.9f, 0.6f);

        /// <summary>
        /// Dark Gray
        /// </summary>
        public static Color Darkgray => new Color(0.2f, 0.2f, 0.2f, 0.6f);

        /// <summary>
        /// Emerald
        /// </summary>
        public static Color Emerald => new Color(0.2f, 0.7f, 0.3f, 0.6f);

        /// <summary>
        /// Granite
        /// </summary>
        public static Color Granite => new Color(0.6f, 0.6f, 0.6f, 0.6f);

        /// <summary>
        /// Gray
        /// </summary>
        public static Color Gray => new Color(0.5f, 0.5f, 0.5f, 0.6f);

        /// <summary>
        /// Green
        /// </summary>
        public static Color Green => new Color(0.0f, 1.0f, 0.0f, 0.6f);

        /// <summary>
        /// Lavender
        /// </summary>
        public static Color Lavender => new Color(0.9f, 0.7f, 1.0f, 0.6f);

        /// <summary>
        /// Lime
        /// </summary>
        public static Color Lime => new Color(0.6f, 0.9f, 0.3f, 0.6f);

        /// <summary>
        /// Magenta
        /// </summary>
        public static Color Magenta => new Color(0.9f, 0.2f, 0.9f, 0.6f);

        /// <summary>
        /// Maroon
        /// </summary>
        public static Color Maroon => new Color(0.5f, 0.0f, 0.3f, 0.6f);

        /// <summary>
        /// Mint
        /// </summary>
        public static Color Mint => new Color(0.6f, 1.0f, 0.7f, 0.6f);

        /// <summary>
        /// Navy
        /// </summary>
        public static Color Navy => new Color(0.0f, 0.0f, 0.5f, 0.6f);

        /// <summary>
        /// 
        /// </summary>
        public static Color Ochre => new Color(0.8f, 0.4f, 0.0f, 0.6f);

        /// <summary>
        /// Olive
        /// </summary>
        public static Color Olive => new Color(0.5f, 0.5f, 0.0f, 0.6f);

        /// <summary>
        /// Orange
        /// </summary>
        public static Color Orange => new Color(1.0f, 0.5f, 0.1f, 0.6f);

        /// <summary>
        /// Pink
        /// </summary>
        public static Color Pink => new Color(1.0f, 0.3f, 0.5f, 0.6f);

        /// <summary>
        /// Purple
        /// </summary>
        public static Color Purple => new Color(0.7f, 0.1f, 1.0f, 0.6f);

        /// <summary>
        /// Red
        /// </summary>
        public static Color Red => new Color(1.0f, 0.0f, 0.0f, 0.6f);

        /// <summary>
        /// Sand
        /// </summary>
        public static Color Sand => new Color(1.0f, 0.6f, 0.6f, 0.6f);

        /// <summary>
        /// Stone
        /// </summary>
        public static Color Stone => new Color(0.1f, 0.1f, 0.1f, 0.6f);

        /// <summary>
        /// Teal
        /// </summary>
        public static Color Teal => new Color(0.0f, 0.5f, 0.5f, 0.6f);

        /// <summary>
        /// White
        /// </summary>
        public static Color White => new Color(1.0f, 1.0f, 1.0f, 0.6f);

        /// <summary>
        /// Yellow
        /// </summary>
        public static Color Yellow => new Color(1.0f, 0.9f, 0.1f, 0.6f);


    }
}