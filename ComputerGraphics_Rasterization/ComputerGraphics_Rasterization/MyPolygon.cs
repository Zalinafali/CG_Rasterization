using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace ComputerGraphics_Rasterization
{
    public class MyPolygon
    {
        public List<Point> points;
        public List<MyEdge> edges;

        System.Drawing.Color color = System.Drawing.Color.Black;
        System.Drawing.Color cllipedColor = System.Drawing.Color.Red;

        // clipping
        public bool clipToPolygon = false;
        public MyPolygon clippingPolygon;
        public bool clipToRectangle = false;
        public MyRectangle clippingRectangle;
        public List<MyEdge> clippedLines;

        // filling
        public bool isFilledColor = false;
        public System.Drawing.Color fillColor;


        public MyPolygon()
        {
            points = new List<Point>();
            edges = new List<MyEdge>();
        }

        public MyPolygon(System.Drawing.Color color)
        {
            this.color = color;
            points = new List<Point>();
            edges = new List<MyEdge>();
        }

        public MyPolygon(List<Point> listOfPoints)
        {
            points = listOfPoints;
            edges = new List<MyEdge>();
            for(int i = 0; i < listOfPoints.Count - 1; i++)
                edges.Add(new MyEdge(listOfPoints[i], listOfPoints[i + 1], color));
            edges.Add(new MyEdge(listOfPoints[listOfPoints.Count - 1], listOfPoints[0], color));
        }

        public MyPolygon(List<Point> listOfPoints, System.Drawing.Color color)
        {
            points = listOfPoints;
            this.color = color;

            edges = new List<MyEdge>();
            for (int i = 0; i < listOfPoints.Count - 1; i++)
                edges.Add(new MyEdge(listOfPoints[i], listOfPoints[i + 1], color));
            edges.Add(new MyEdge(listOfPoints[listOfPoints.Count - 1], listOfPoints[0], color));
        }

        public bool ColorEdge(Point x1, Point x2, System.Drawing.Color newColor)
        {
            foreach(var edge in edges)
            {
                if( edge.p == x1 && edge.q == x2)
                {
                    edge.color = newColor;
                    return true;
                }
            }
            return false;
        }

        public void AddPoint(Point x)
        {
            edges.Add(new MyEdge(points[points.Count - 1], x, color));
            points.Add(x);
        }

        public void AddPoint(Point x, System.Drawing.Color newColor)
        {
            edges.Add(new MyEdge(points[points.Count - 1], x, newColor));
            points.Add(x);
        }

        public void ClosePolygon()
        {
            edges.Add(new MyEdge(points[points.Count - 1], points[0], color));
            points.Add(points[0]);
        }

        public void ClosePolygon(System.Drawing.Color newColor)
        {
            edges.Add(new MyEdge(points[points.Count - 1], points[0], newColor));
            points.Add(points[0]);
        }

        public void MovePoint(int pointIndx, Point newPos)
        {
            Point oldP = points[pointIndx];
        
            for(int i = 0; i < edges.Count; i++)
            {
               if(edges[i].p == oldP)
               {
                   MyEdge temp = edges[i];
                   edges.RemoveAt(i);
                   edges.Insert(i, new MyEdge(newPos, temp.q, temp.color));
               }
               else if(edges[i].q == oldP)
               {
                   MyEdge temp = edges[i];
                   edges.RemoveAt(i);
                   edges.Insert(i, new MyEdge(temp.p, newPos, temp.color));
               }
            }

            for(int i = 0; i < points.Count; i++)
            {
                if(points[i] == oldP)
                {
                    points.RemoveAt(i);
                    points.Insert(i, newPos);
                }
            }
        }

        public void ClipToPolygon(MyPolygon polygon)
        {
            clippingPolygon = polygon;
            clipToPolygon = true;
        }

        public void ClipToRectangle(MyRectangle rectangle)
        {
            clippingRectangle = rectangle;
            clipToRectangle = true;
        }

        public void Clipping()
        {
            clippedLines = new List<MyEdge>();

            // clipping only for convex polygons
            if (!clippingPolygon.CheckIfConvex())
                return;

            if (clipToRectangle)
            {
                if (clippingRectangle != null)
                {
                    // calculate normals
                    int nOfPoints = clippingRectangle.points.Count;
                    Vector[] normal = new Vector[nOfPoints];
                    for (int i = 0; i < nOfPoints; i++)
                    {
                        normal[i].X = clippingRectangle.points[i].Y - clippingRectangle.points[(i + 1) % nOfPoints].Y;
                        normal[i].Y = clippingRectangle.points[(i + 1) % nOfPoints].X - clippingRectangle.points[i].X;
                    }

                    foreach (var edge in edges)
                    {
                        MyEdge result = this.CyrusBeck(edge, normal, clippingRectangle.points);
                        if (result != null)
                            clippedLines.Add(result);
                    }
                }
                else
                    clipToRectangle = false;
            }

            else if(clipToPolygon)
            {
                if (clippingPolygon != null)
                {
                    int nOfPoints = clippingPolygon.points.Count - 1;
                    Vector[] normal = new Vector[nOfPoints];
                    for (int i = 0; i < nOfPoints; i++)
                    {
                        normal[i].X = clippingPolygon.points[i].Y - clippingPolygon.points[(i + 1) % nOfPoints].Y;
                        normal[i].Y = clippingPolygon.points[(i + 1) % nOfPoints].X - clippingPolygon.points[i].X;
                    }

                    foreach (var edge in edges)
                    {
                        MyEdge result = this.CyrusBeck(edge, normal, clippingPolygon.points);
                        if (result != null)
                            clippedLines.Add(result);
                    }
                }
                else
                    clipToPolygon = false;
            }
        }

        private MyEdge CyrusBeck(MyEdge line, Vector[] normal, List<Point> figurePoints)
        {

            int n = normal.Length;
            Vector P1_P0 = new Vector(line.q.X - line.p.X, line.q.Y - line.p.Y);

            Vector[] PEi_P0 = new Vector[n];
            for(int i = 0; i < n; i++)
            {
                PEi_P0[i].X = figurePoints[i].X - line.p.X;
                PEi_P0[i].Y = figurePoints[i].Y - line.p.Y;
            }

            int[] numerator = new int[n];
            int[] denominator = new int[n];

            for(int i = 0; i < n; i++)
            {
                numerator[i] = dot(normal[i], PEi_P0[i]);
                denominator[i] = dot(normal[i], P1_P0);
            }

            double[] t = new double[n];
            List<double> tE = new List<double>();
            List<double> tL = new List<double>();

            for(int i = 0; i < n; i++)
            {
                t[i] = (double)numerator[i] / denominator[i];

                if (denominator[i] > 0)
                    tE.Add(t[i]);
                else
                    tL.Add(t[i]);
            }

            double tEMax, tLMin;
            tE.Add(0);
            tL.Add(1);

            tEMax = tE.Max();
            tLMin = tL.Min();

            if (tEMax > tLMin)
            {
                return null;
            }

            Point p = new Point(line.p.X + P1_P0.X * tEMax, line.p.Y + P1_P0.Y * tEMax);
            Point q = new Point(line.p.X + P1_P0.X * tLMin, line.p.Y + P1_P0.Y * tLMin);

            return new MyEdge(p, q, cllipedColor);
        }

        private bool CheckIfConvex()
        {
            bool negative = false;
            bool positive = false;
            int num_points = points.Count - 1;
            int B, C;
            for(int A = 0; A < num_points; A++)
            {
                B = (A + 1) % num_points;
                C = (B + 1) % num_points;

                double cross_product = CrossProductLength(points[A], points[B], points[C]);

                if (cross_product < 0)
                    negative = true;
                else if (cross_product > 0)
                    positive = true;

                if (negative && positive)
                    return false;
            }
            return true;
        }

        private double CrossProductLength(Point A, Point B, Point C)
        {
            double BAx = A.X - B.X;
            double BAy = A.Y - B.Y;
            double BCx = C.X - B.X;
            double BCy = C.Y - B.Y;

            return (BAx * BCy - BAy * BCx);
        }

        private int dot(Vector p1, Vector p2)
        {
            return (int)(p1.X * p2.X + p1.Y * p2.Y);
        }

        public void FillColor(System.Drawing.Color color)
        {
            isFilledColor = true;
            this.fillColor = color;
        }
    }
}
