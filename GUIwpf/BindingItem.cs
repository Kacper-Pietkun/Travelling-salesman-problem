using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GUIwpf
{
    public class BindingItem
    {
        public string Id{ get; set; }
        public string PosX{ get; set; }
        public string PosY { get; set; }

        public BindingItem((int, PointF) node)
        {
            Id = node.Item1.ToString();
            PosX = node.Item2.X.ToString();
            PosY = node.Item2.Y.ToString();
        }
    }
}
