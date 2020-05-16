using System;
using System.Collections.Generic;
using System.Windows;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ComputerGraphics_Rasterization
{
    public class MyEdge
    {
        public Point p;
        public Point q;
        public System.Drawing.Color color;

        public MyEdge(Point p, Point q, System.Drawing.Color color)
        {
            this.p = p;
            this.q = q;
            this.color = color;
        }
    }
}
