using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace ComputerGraphics_Rasterization
{
    public class MyRectangle
    {
        public List<Point> points;
        public List<MyEdge> edges;

        public bool isFilledColor = false;
        public System.Drawing.Color fillColor;

        public System.Drawing.Color color = System.Drawing.Color.Black;

        public MyRectangle()
        {
            points = new List<Point>();
            edges = new List<MyEdge>();
        }

        public MyRectangle(System.Drawing.Color color)
        {
            points = new List<Point>();
            edges = new List<MyEdge>();
            this.color = color;
        }

        public MyRectangle(Point p, Point q)
        {
            points = new List<Point>();
            edges = new List<MyEdge>();

            if (p.X < q.X)
            {
                if (p.Y < q.Y)
                {
                    points.Add(new Point(p.X, p.Y));
                    points.Add(new Point(q.X, p.Y));
                    points.Add(new Point(q.X, q.Y));
                    points.Add(new Point(p.X, q.Y));
                }
                else
                {
                    points.Add(new Point(p.X, q.Y));
                    points.Add(new Point(q.X, q.Y));
                    points.Add(new Point(q.X, p.Y));
                    points.Add(new Point(p.X, p.Y));
                }
            }
            else
            {
                if (p.Y < q.Y)
                {
                    points.Add(new Point(q.X, p.Y));
                    points.Add(new Point(p.X, p.Y));
                    points.Add(new Point(p.X, q.Y));
                    points.Add(new Point(q.X, q.Y));
                }
                else
                {
                    points.Add(new Point(q.X, q.Y));
                    points.Add(new Point(p.X, q.Y));
                    points.Add(new Point(p.X, p.Y));
                    points.Add(new Point(q.X, p.Y));
                }
            }

            for(int i = 0; i < 3; i++)
                edges.Add(new MyEdge(points[i], points[i + 1], color));
            edges.Add(new MyEdge(points[3], points[0], color));
        }

        public MyRectangle(Point p, Point q, System.Drawing.Color color)
        {
            points = new List<Point>();
            edges = new List<MyEdge>();
            this.color = color;

            if (p.X < q.X)
            {
                if (p.Y < q.Y)
                {
                    points.Add(new Point(p.X, p.Y));
                    points.Add(new Point(q.X, p.Y));
                    points.Add(new Point(q.X, q.Y));
                    points.Add(new Point(p.X, q.Y));
                }
                else
                {
                    points.Add(new Point(p.X, q.Y));
                    points.Add(new Point(q.X, q.Y));
                    points.Add(new Point(q.X, p.Y));
                    points.Add(new Point(p.X, p.Y));
                }
            }
            else
            {
                if (p.Y < q.Y)
                {
                    points.Add(new Point(q.X, p.Y));
                    points.Add(new Point(p.X, p.Y));
                    points.Add(new Point(p.X, q.Y));
                    points.Add(new Point(q.X, q.Y));
                }
                else
                {
                    points.Add(new Point(q.X, q.Y));
                    points.Add(new Point(p.X, q.Y));
                    points.Add(new Point(p.X, p.Y));
                    points.Add(new Point(q.X, p.Y));
                }
            }

            for (int i = 0; i < 3; i++)
                edges.Add(new MyEdge(points[i], points[i + 1], color));
            edges.Add(new MyEdge(points[3], points[0], color));
        }

        public MyRectangle(List<Point> listOfPoints)
        {
            points = listOfPoints;
            edges = new List<MyEdge>();
            for (int i = 0; i < listOfPoints.Count - 1; i++)
                edges.Add(new MyEdge(listOfPoints[i], listOfPoints[i + 1], color));
            edges.Add(new MyEdge(listOfPoints[listOfPoints.Count - 1], listOfPoints[0], color));
        }

        public MyRectangle(List<Point> listOfPoints, System.Drawing.Color color)
        {
            this.color = color;
            points = listOfPoints;
            edges = new List<MyEdge>();
            for (int i = 0; i < listOfPoints.Count - 1; i++)
                edges.Add(new MyEdge(listOfPoints[i], listOfPoints[i + 1], color));
            edges.Add(new MyEdge(listOfPoints[listOfPoints.Count - 1], listOfPoints[0], color));
        }

        public void SetPointsAndEdges(List<Point> points, List<MyEdge> edges)
        {
            this.points = points;
            this.edges = edges;
        }

        public void FillColor(System.Drawing.Color color)
        {
            isFilledColor = true;
            fillColor = color;
        }

        public void FillPattern()
        {

        }
    }
}
