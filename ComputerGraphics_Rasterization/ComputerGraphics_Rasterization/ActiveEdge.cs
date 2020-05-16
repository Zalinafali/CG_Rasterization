using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace ComputerGraphics_Rasterization
{

    public class ActiveEdge
    {
        static int counter = 0;

        public int ymin;
        public int ymax;
        public double x;
        public double? m;

        public ActiveEdge(int ymin, int ymax, double x, double m)
        {
            this.ymin = ymin;
            this.ymax = ymax;
            this.x = x;
            this.m = m;
        }

        public ActiveEdge(Point p, Point q)
        {
            if (p.Y > q.Y)
            {
                ymin = (int)q.Y;
                ymax = (int)p.Y;
                x = q.X;

                m = (p.Y - q.Y) / (p.X - q.X);
            }
            else if (p.Y < q.Y)
            {
                ymin = (int)p.Y;
                ymax = (int)q.Y;
                x = p.X;

                m = (p.Y - q.Y) / (p.X - q.X);
            }
            else
            {
                ymin = (int)p.Y;
                ymax = ymin;
                if (p.X < q.X)
                    x = p.X;
                else
                    x = q.X;

                m = null;
            }

            counter++;

            MessageBox.Show("Edge: " + counter + " x: " + x + " m: " + m + " ymin: " + ymin + " ymax: " + ymax);
            MessageBox.Show("P.X: " + p.X + " P.Y: " + p.Y + " Q.X: " + q.X + " Q.Y: " + q.Y);
        }
    }
}
