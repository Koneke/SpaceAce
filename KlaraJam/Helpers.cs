using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace KlaraJam
{
    class Helpers
    {
        public static Rectangle Scale(Rectangle r, float s) {
            return new Rectangle(
                (int)(r.X*s), (int)(r.Y*s), (int)(r.Width*s), (int)(r.Height*s));
        }

        public static Color Scale(Color c, float s, bool alpha = false) {
            return new Color(
                c.R*s,
                c.G*s,
                c.B*s,
                c.A*(alpha?s:1));
        }

        public static Color Add(Color color1, Color color2, bool alpha = false) {
            return new Color(
                color1.R+color2.R,
                color1.G+color2.G,
                color1.B+color2.B,
                alpha?(color1.A+color2.A):255);
        }
    }
}
