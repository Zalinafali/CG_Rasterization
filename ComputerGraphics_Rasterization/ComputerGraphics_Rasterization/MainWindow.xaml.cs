using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Drawing;
using System.IO;
using System.Drawing.Imaging;
using Rectangle = System.Drawing.Rectangle;
using Point = System.Windows.Point;

namespace ComputerGraphics_Rasterization
{
    public partial class MainWindow : Window
    {
        int width = 600;
        int height = 400;
        int around = 5;
        int maxThickness = 21;

        System.Drawing.Color mainColor = System.Drawing.Color.Black;
        System.Drawing.Color bgColor = System.Drawing.Color.Azure;
        System.Drawing.Color fillColor = System.Drawing.Color.DarkRed;

        bool isDrawing = false;
        bool drawingLine = false;
        bool drawingPolygon = false;
        bool drawingCircle = false;
        bool drawingCapsule = false;
        bool drawingRectangle = false;

        bool deleteObject = false;

        bool antialiasing = false;

        // moving points
        bool movingPoints = false;
        bool havePointToMove = false;
        bool moveLine = false;
        bool movePolygon = false;
        bool moveRectangle = false;
        MyRectangle rectangleToDelete;
        int indx_polygons;
        int indx_polygon;
        Point toMove;

        // moving rectangle
        bool haveEdgeToMove = false;
        int p1, p2;
        bool moveWholeRectangle = false;
        bool haveRectangleToMove = false;
        MyRectangle myRectangleToMove;

        // clipping
        bool selectForClipping = false;
        bool haveFigureToClip = false;
        bool haveClippingFigure = false;
        MyPolygon clippingPolygon;
        MyRectangle clippingRectangle;
        MyPolygon polygonToClip;

        // fill polygon/rectangle
        bool fillingColor = false;

        int thickness = 1;

        Point start;
        Point end;

        Point rad;
        int num_p = 0;

        List<Tuple<Point,Point>> lines;
        List<List<Point>> polygons;
        List<Point> polygon;
        List<Tuple<Point, Point>> circles;
        List<List<Point>> rectangles;

        List<MyEdge> myLines;
        MyPolygon myPolygon;
        List<MyPolygon> myPolygons;
        List<MyRectangle> myRectangles;

        Bitmap bmp = null;
        BitmapImage ibmp = null;

        public MainWindow()
        {
            InitializeComponent();

            lines = new List<Tuple<Point,Point>>();
            polygons = new List<List<Point>>();
            circles = new List<Tuple<Point, Point>>();
            rectangles = new List<List<Point>>();

            myLines = new List<MyEdge>();
            myPolygons = new List<MyPolygon>();
            myRectangles = new List<MyRectangle>();

            DrawObjects();
        }

        private Bitmap CreateWhiteBitmap(int x, int y)
        {
            Bitmap nbmp = new Bitmap(x, y);
            using (Graphics graph = Graphics.FromImage(nbmp))
            {
                Rectangle ImageSize = new Rectangle(0, 0, x, y);
                graph.FillRectangle(System.Drawing.Brushes.Azure, ImageSize);
            }

            return nbmp;
        }

        private void ShowBitmap()
        {
            ibmp = Bitmap2BitmapImage(bmp);
            image.Source = ibmp;
        }

        private BitmapImage Bitmap2BitmapImage(Bitmap bitmap)
        {
            using (MemoryStream memory = new MemoryStream())
            {
                bitmap.Save(memory, ImageFormat.Bmp);
                memory.Position = 0;
                BitmapImage bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = memory;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();
                return bitmapImage;
            }
        }

        private void DrawObjects()
        {
            Bitmap nbmp = CreateWhiteBitmap(width, height);

            //foreach (var line in lines)
            //{
            //    SymmetricMidpointLine(nbmp, (int)line.Item1.X, (int)line.Item1.Y, (int)line.Item2.X, (int)line.Item2.Y, System.Drawing.Color.Black);
            //}

            foreach(var myLine in myLines)
            {
                SymmetricMidpointLine(nbmp, (int)myLine.p.X, (int)myLine.p.Y, (int)myLine.q.X, (int)myLine.q.Y, myLine.color);
            }

            foreach(var circle in circles)
            {
                AlternativeMidpointCircle(nbmp, (int)circle.Item1.X, (int)circle.Item1.Y, (int)circle.Item2.X, (int)circle.Item2.Y, System.Drawing.Color.Black);
            }

            //foreach(var rectangle in rectangles)
            //{
            //    for(int i = 0; i < 3; i++)
            //    {
            //        SymmetricMidpointLine(nbmp, (int)rectangle[i].X, (int)rectangle[i].Y, (int)rectangle[i + 1].X, (int)rectangle[i + 1].Y, System.Drawing.Color.Black);
            //    }
            //    SymmetricMidpointLine(nbmp, (int)rectangle[3].X, (int)rectangle[3].Y, (int)rectangle[0].X, (int)rectangle[0].Y, System.Drawing.Color.Black);
            //}

            foreach (var rectangle in myRectangles)
            {
                foreach (var line in rectangle.edges)
                {
                    SymmetricMidpointLine(nbmp, (int)line.p.X, (int)line.p.Y, (int)line.q.X, (int)line.q.Y, line.color);
                }

                if (rectangle.isFilledColor)
                {
                    FillColorRectangle(nbmp, rectangle);
                }
            }

            //foreach(var polyg in polygons)
            //{
            //    for (int i = 0; i < polyg.Count - 1; i++)
            //    {
            //        SymmetricMidpointLine(nbmp, (int)polyg[i].X, (int)polyg[i].Y, (int)polyg[i + 1].X, (int)polyg[i + 1].Y, System.Drawing.Color.Black);
            //    }
            //}

            foreach (var polyg in myPolygons)
            {
                foreach(var line in polyg.edges)
                {
                    SymmetricMidpointLine(nbmp, (int)line.p.X, (int)line.p.Y, (int)line.q.X, (int)line.q.Y, line.color);
                }

                if (polyg.clipToPolygon || polyg.clipToRectangle)
                {
                    polyg.Clipping();

                    foreach (var line in polyg.clippedLines)
                    {
                        SymmetricMidpointLine(nbmp, (int)line.p.X, (int)line.p.Y, (int)line.q.X, (int)line.q.Y, line.color);
                    }
                }

                if (polyg.isFilledColor)
                {
                    FillColorPolygon(nbmp, polyg);
                }
            }

            bmp = nbmp;

            ShowBitmap();
        }

        private void Image_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (movingPoints)
            {
                if (!havePointToMove) {
                    Point pos = e.GetPosition(image);

                    int x_max = (int)pos.X + around;
                    int x_min = (int)pos.X - around;
                    int y_max = (int)pos.Y + around;
                    int y_min = (int)pos.Y - around;

                    foreach (var myLine in myLines)
                    {
                        if (myLine.p.X > x_min && myLine.p.X < x_max && myLine.p.Y > y_min && myLine.p.Y < y_max)
                        {
                            toMove = myLine.p;
                            havePointToMove = true;
                            moveLine = true;
                            break;
                        }
                        if (myLine.q.X > x_min && myLine.q.X < x_max && myLine.q.Y > y_min && myLine.q.Y < y_max)
                        {
                            toMove = myLine.q;
                            havePointToMove = true;
                            moveLine = true;
                            break;
                        }
                    }

                    if (!havePointToMove)
                    {
                        for(int i = 0; i < myPolygons.Count; i++)
                        {
                            for (int j = 0; j < myPolygons[i].points.Count; j++)
                            {
                                if(myPolygons[i].points[j].X > x_min && myPolygons[i].points[j].X < x_max && myPolygons[i].points[j].Y > y_min && myPolygons[i].points[j].Y < y_max)
                                {
                                    indx_polygons = i;
                                    indx_polygon = j;
                                    toMove = polygons[i][j];
                                    movePolygon = true;
                                    havePointToMove = true;
                                    break;
                                }
                            }
                        }
                    }

                    if (!havePointToMove)
                    {
                        foreach (var rectangle in myRectangles)
                        {
                            for(int i = 0; i < 4; i++)
                            {
                                if (rectangle.points[i].X > x_min && rectangle.points[i].X < x_max && rectangle.points[i].Y > y_min && rectangle.points[i].Y < y_max)
                                {
                                    havePointToMove = true;
                                    moveRectangle = true;
                                    start = rectangle.points[(i + 2) % 4];
                                    myRectangleToMove = rectangle;
                                    break;
                                }
                            }

                            if (!havePointToMove)
                            {
                                if (rectangle.points[0].X < x_min && rectangle.points[1].X > x_max)
                                {
                                    if (rectangle.points[0].Y > y_min && rectangle.points[0].Y < y_max)
                                    {
                                        havePointToMove = true;
                                        moveRectangle = true;
                                        haveEdgeToMove = true;
                                        p1 = 0;
                                        p2 = 1;
                                        myRectangleToMove = rectangle;
                                        break;
                                    }
                                    else if (rectangle.points[2].Y > y_min && rectangle.points[2].Y < y_max)
                                    {
                                        havePointToMove = true;
                                        moveRectangle = true;
                                        haveEdgeToMove = true;
                                        p1 = 2;
                                        p2 = 3;
                                        myRectangleToMove = rectangle;
                                        break;
                                    }
                                }
                                else if (rectangle.points[3].Y > y_min && rectangle.points[0].Y < y_max)
                                {
                                    if (rectangle.points[0].X > x_min && rectangle.points[0].X < x_max)
                                    {
                                        havePointToMove = true;
                                        moveRectangle = true;
                                        haveEdgeToMove = true;
                                        p1 = 3;
                                        p2 = 0;
                                        myRectangleToMove = rectangle;
                                        break;
                                    }
                                    else if (rectangle.points[2].X > x_min && rectangle.points[2].X < x_max)
                                    {
                                        havePointToMove = true;
                                        moveRectangle = true;
                                        haveEdgeToMove = true;
                                        p1 = 1;
                                        p2 = 2;
                                        myRectangleToMove = rectangle;
                                        break;
                                    }
                                }
                            }
                        }
                    }
                }
                else
                {
                    Point pos = e.GetPosition(image);

                    if (moveLine)
                    {
                        foreach(var line in myLines)
                        {
                            if (line.p == toMove)
                            {
                                myLines.Add(new MyEdge(pos, line.q, line.color));
                                myLines.Remove(line);
                                havePointToMove = false;
                                moveLine = false;
                                break;
                            }
                            if (line.q == toMove)
                            {
                                myLines.Add(new MyEdge(line.p, pos, line.color));
                                myLines.Remove(line);
                                havePointToMove = false;
                                moveLine = false;
                                break;
                            }
                        }
                    }
                    else if (movePolygon)
                    {
                        myPolygons[indx_polygons].MovePoint(indx_polygon, pos);

                        movePolygon = false;
                        havePointToMove = false;
                    }
                    else if (moveRectangle)
                    {
                        if (!haveEdgeToMove)
                        {
                            end = pos;
                            MyRectangle movedRectangle = new MyRectangle(start, end);
                            myRectangles.Find(x => x == myRectangleToMove).SetPointsAndEdges(movedRectangle.points, movedRectangle.edges);

                            moveRectangle = false;
                            havePointToMove = false;
                        }
                        else
                        {
                            double d;
                            Point p = new Point();
                            Point q = new Point();
                            if (p1%2 == 0)
                            {
                                if (p1 == 0)
                                {
                                    d = pos.Y - myRectangleToMove.points[0].Y;
                                    p = new Point((int)(myRectangleToMove.points[0].X), (int)(myRectangleToMove.points[0].Y + d));
                                    q = new Point((int)(myRectangleToMove.points[2].X), (int)(myRectangleToMove.points[2].Y));
                                }
                                else
                                {
                                    d = pos.Y - myRectangleToMove.points[2].Y;
                                    p = new Point((int)(myRectangleToMove.points[0].X), (int)(myRectangleToMove.points[0].Y));
                                    q = new Point((int)(myRectangleToMove.points[2].X), (int)(myRectangleToMove.points[2].Y + d));
                                }
                            }
                            else
                            {
                                if (p1 == 1)
                                {
                                    d = pos.X - myRectangleToMove.points[2].X;
                                    p = new Point((int)(myRectangleToMove.points[0].X), (int)(myRectangleToMove.points[0].Y));
                                    q = new Point((int)(myRectangleToMove.points[2].X + d), (int)(myRectangleToMove.points[2].Y));                                }
                                else
                                {
                                    d = pos.X - myRectangleToMove.points[0].X;
                                    p = new Point((int)(myRectangleToMove.points[0].X + d), (int)(myRectangleToMove.points[0].Y));
                                    q = new Point((int)(myRectangleToMove.points[2].X), (int)(myRectangleToMove.points[2].Y));
                                }
                            }
                            MyRectangle movedRectangle = new MyRectangle(p, q);
                            myRectangles.Find(x => x == myRectangleToMove).SetPointsAndEdges(movedRectangle.points, movedRectangle.edges);
                            haveEdgeToMove = false;
                            moveRectangle = false;
                            havePointToMove = false;
                        }
                    }
                    DrawObjects();
                }
            }
            else if (deleteObject)
            {
                DeleteObject(e);
            }
            else if (moveWholeRectangle)
            {
                if (haveRectangleToMove)
                {
                    Point pos = e.GetPosition(image);

                    double dx = pos.X - toMove.X;
                    double dy = pos.Y - toMove.Y;

                    Point p = new Point((int)(myRectangleToMove.points[0].X + (int)dx), (int)(myRectangleToMove.points[0].Y + (int)dy));
                    Point q = new Point((int)(myRectangleToMove.points[2].X + (int)dx), (int)(myRectangleToMove.points[2].Y + (int)dy));

                    MyRectangle movedRectangle = new MyRectangle(p, q);
                    myRectangles.Find(x => x == myRectangleToMove).SetPointsAndEdges(movedRectangle.points, movedRectangle.edges);

                    haveRectangleToMove = false;
                    DrawObjects();
                }
                else
                {
                    Point p = e.GetPosition(image);

                    int x_max = (int)p.X + around;
                    int x_min = (int)p.X - around;
                    int y_max = (int)p.Y + around;
                    int y_min = (int)p.Y - around;

                    foreach (var rectangle in myRectangles)
                    {
                        for (int i = 0; i < 4; i++)
                        {
                            if (rectangle.points[i].X > x_min && rectangle.points[i].X < x_max && rectangle.points[i].Y > y_min && rectangle.points[i].Y < y_max)
                            {
                                haveRectangleToMove = true;
                                myRectangleToMove = rectangle;
                                toMove = rectangle.points[i];
                                break;
                            }
                        }
                    }
                }
            }
            else if (selectForClipping)
            {
                Point pos = e.GetPosition(image);

                int x_max = (int)pos.X + around;
                int x_min = (int)pos.X - around;
                int y_max = (int)pos.Y + around;
                int y_min = (int)pos.Y - around;

                if (!haveFigureToClip)
                {
                    for (int i = 0; i < myPolygons.Count; i++)
                    {
                        for (int j = 0; j < myPolygons[i].points.Count; j++)
                        {
                            if (myPolygons[i].points[j].X > x_min && myPolygons[i].points[j].X < x_max && myPolygons[i].points[j].Y > y_min && myPolygons[i].points[j].Y < y_max)
                            {
                                MessageBox.Show("Have polygon to clip");
                                polygonToClip = myPolygons[i];
                                haveFigureToClip = true;
                                break;
                            }
                        }
                    }
                }
                else
                {

                    if (!haveClippingFigure)
                    {
                        for (int i = 0; i < myPolygons.Count; i++)
                        {
                            for (int j = 0; j < myPolygons[i].points.Count; j++)
                            {
                                if (myPolygons[i].points[j].X > x_min && myPolygons[i].points[j].X < x_max && myPolygons[i].points[j].Y > y_min && myPolygons[i].points[j].Y < y_max)
                                {
                                    if (polygonToClip == myPolygons[i])
                                    {
                                        MessageBox.Show("Same polygon");
                                        continue;
                                    }
                                    else
                                    {
                                        MessageBox.Show("Have clipping polygon");
                                        clippingPolygon = myPolygons[i];
                                        haveClippingFigure = true;
                                        polygonToClip.ClipToPolygon(clippingPolygon);
                                        break;
                                    }
                                }
                            }
                        }

                        if (!haveClippingFigure)
                        {
                            foreach (var rectangle in myRectangles)
                            {
                                for (int i = 0; i < 4; i++)
                                {
                                    if (rectangle.points[i].X > x_min && rectangle.points[i].X < x_max && rectangle.points[i].Y > y_min && rectangle.points[i].Y < y_max)
                                    {
                                        MessageBox.Show("Have clipping rectangle");
                                        clippingRectangle = rectangle;
                                        haveClippingFigure = true;
                                        polygonToClip.ClipToRectangle(clippingRectangle);
                                        break;
                                    }
                                }
                            }
                        }
                    }

                    if (haveClippingFigure)
                    {
                        polygonToClip.Clipping();
                        DrawObjects();
                    }
                }

            }
            else if (fillingColor)
            {
                Point p = e.GetPosition(image);

                int x_max = (int)p.X + around;
                int x_min = (int)p.X - around;
                int y_max = (int)p.Y + around;
                int y_min = (int)p.Y - around;

                foreach (var rectangle in myRectangles)
                {
                    for (int i = 0; i < 4; i++)
                    {
                        if (rectangle.points[i].X > x_min && rectangle.points[i].X < x_max && rectangle.points[i].Y > y_min && rectangle.points[i].Y < y_max)
                        {
                            MessageBox.Show("Prepare to fill rectangle");
                            rectangle.FillColor(fillColor);
                            DrawObjects();
                            return;
                        }
                    }
                }

                foreach(var polyg in myPolygons)
                {
                    for(int i = 0; i < polyg.points.Count; i++)
                    {
                        if (polyg.points[i].X > x_min && polyg.points[i].X < x_max && polyg.points[i].Y > y_min && polyg.points[i].Y < y_max)
                        {
                            MessageBox.Show("Prepare to fill polygon");
                            polyg.FillColor(fillColor);
                            DrawObjects();
                            return;
                        }
                    }
                }
            }
            else if (!isDrawing)
            {
                start = e.GetPosition(image);

                x1.Content = start.X;
                y1.Content = start.Y;
                isDrawing = true;

                if (drawingPolygon)
                {
                    polygon = new List<Point>();
                    polygon.Add(start);

                    myPolygon = new MyPolygon(mainColor);
                    myPolygon.points.Add(start);
                }
            }
            else if (isDrawing && drawingLine)
            {
                end = e.GetPosition(image);

                x2.Content = end.X;
                y2.Content = end.Y;
                isDrawing = false;

                SymmetricMidpointLine(bmp, (int)start.X, (int)start.Y, (int)end.X, (int)end.Y, System.Drawing.Color.Black);
                lines.Add(new Tuple<Point, Point>(start, end));
                myLines.Add(new MyEdge(start, end, mainColor));
            }
            else if(isDrawing && drawingPolygon)
            {
                Point np = e.GetPosition(image);
                x2.Content = np.X;
                y2.Content = np.Y;

                if (np.X > start.X - around && np.X < start.X + around && np.Y > start.Y - around && np.Y < start.Y + around)
                {
                    np = start;
                    SymmetricMidpointLine(bmp, (int)polygon[polygon.Count - 1].X, (int)polygon[polygon.Count - 1].Y, (int)np.X, (int)np.Y, System.Drawing.Color.Black);
                    polygon.Add(np);
                    myPolygon.ClosePolygon();

                    polygons.Add(polygon);
                    myPolygons.Add(myPolygon);

                    isDrawing = false;                  
                }
                else
                {
                    SymmetricMidpointLine(bmp, (int)polygon[polygon.Count - 1].X, (int)polygon[polygon.Count - 1].Y, (int)np.X, (int)np.Y, System.Drawing.Color.Black);
                    polygon.Add(np);
                    myPolygon.AddPoint(np);
                }
            }
            else if(isDrawing && drawingCircle)
            {
                end = e.GetPosition(image);

                x2.Content = end.X;
                y2.Content = end.Y;
                isDrawing = false;

                AlternativeMidpointCircle(bmp, (int)start.X, (int)start.Y, (int)end.X, (int)end.Y, System.Drawing.Color.Black);
                circles.Add(new Tuple<Point, Point>(start, end));
            }
            else if(isDrawing && drawingCapsule)
            {
                if(2 == num_p)
                {
                    rad = e.GetPosition(image);
                    DrawCapsule(bmp, (int)start.X, (int)start.Y, (int)end.X, (int)end.Y, (int)rad.X, (int)rad.Y, System.Drawing.Color.Black);
                    isDrawing = false;
                    num_p = 1;
                }
                else if(1 == num_p)
                {
                    end = e.GetPosition(image);
                    num_p++;
                }
            }
            else if(isDrawing && drawingRectangle)
            {
                end = e.GetPosition(image);

                x2.Content = end.X;
                y2.Content = end.Y;
                isDrawing = false;

                List<Point> result = CreateRectangle((int)start.X, (int)start.Y, (int)end.X, (int)end.Y);

                rectangles.Add(result);
                myRectangles.Add(new MyRectangle(start, end, mainColor));

                DrawObjects();
            }
        }

        private System.Drawing.Color AntialiasingColor(System.Drawing.Color color, double colorVal, System.Drawing.Color bkgrColor, double bkgrColorVal)
        {
            int red = Convert.ToInt32(color.R * colorVal + bkgrColor.R * bkgrColorVal);
            int green = Convert.ToInt32(color.G * colorVal +bkgrColor.G * bkgrColorVal);
            int blue = Convert.ToInt32(color.B * colorVal + bkgrColor.B * bkgrColorVal);

            return System.Drawing.Color.FromArgb(red, green, blue);
        }

        // Drawing algorithms

        private void SymmetricMidpointLine(Bitmap cbmp, int x1, int y1, int x2, int y2, System.Drawing.Color color)
        {
            int dx = x2 - x1;
            int dy = y2 - y1;
            int xf = x1, yf = y1;
            int xb = x2, yb = y2;

            ldx.Content = dx;
            ldy.Content = dy;

            cbmp.SetPixel(xf, yf, color);
            cbmp.SetPixel(xb, yb, color);

            if (dx > 0 && dy > 0)
            {
                if(dx > dy)
                {
                    if (!antialiasing)
                    {
                        int d = 2 * dy - dx;
                        int dE = 2 * dy;
                        int dNE = 2 * (dy - dx);

                        while (xf < xb)
                        {
                            ++xf; --xb;
                            if (d < 0)
                                d += dE;
                            else
                            {
                                d += dNE;
                                ++yf;
                                --yb;
                            }
                            cbmp.SetPixel(xf, yf, color);
                            cbmp.SetPixel(xb, yb, color);

                            if ((bool)thickLineCheckBox.IsChecked)
                            {
                                for (int i = 1; i < thickness; i++)
                                {
                                    cbmp.SetPixel(xf, yf + i, color);
                                    cbmp.SetPixel(xb, yb + i, color);

                                    cbmp.SetPixel(xf, yf - i, color);
                                    cbmp.SetPixel(xb, yb - i, color);
                                }
                            }

                        }
                    }
                    else
                    {
                        int tx = x1;
                        double ty = y1;
                        double m = (double)dy / dx;
                        while (tx < x2)
                        {
                            double floatingPart = ty - Math.Truncate(ty);

                            System.Drawing.Color c1 = AntialiasingColor(color, 1 - floatingPart, bgColor, floatingPart);
                            System.Drawing.Color c2 = AntialiasingColor(color, floatingPart, bgColor, 1 - floatingPart);

                            cbmp.SetPixel(tx, (int)Math.Floor(ty), c1);
                            cbmp.SetPixel(tx, (int)Math.Floor(ty) + 1, c2);

                            tx++;
                            ty += m;
                        }
                    }
                }
                else if(dx > -dy)
                {
                    if (!antialiasing)
                    {
                        int d = 2 * dx - dy;
                        int dN = 2 * dx;
                        int dNE = 2 * (dx - dy);

                        while (yf < yb)
                        {
                            ++yf; --yb;
                            if (d < 0)
                                d += dN;
                            else
                            {
                                d += dNE;
                                ++xf;
                                --xb;
                            }
                            cbmp.SetPixel(xf, yf, color);
                            cbmp.SetPixel(xb, yb, color);

                            if ((bool)thickLineCheckBox.IsChecked)
                            {
                                for (int i = 1; i < thickness; i++)
                                {
                                    cbmp.SetPixel(xf + 1, yf, color);
                                    cbmp.SetPixel(xb + 1, yb, color);

                                    cbmp.SetPixel(xf - 1, yf, color);
                                    cbmp.SetPixel(xb - 1, yb, color);
                                }
                            }
                        }
                    }
                    else
                    {
                        double tx = x1;
                        int ty = y1;
                        double m = (double)dx / dy;
                        while (ty < y2)
                        {
                            double floatingPart = tx - Math.Truncate(tx);

                            System.Drawing.Color c1 = AntialiasingColor(color, 1 - floatingPart, bgColor, floatingPart);
                            System.Drawing.Color c2 = AntialiasingColor(color, floatingPart, bgColor, 1 - floatingPart);

                            cbmp.SetPixel((int)Math.Floor(tx), ty, c1);
                            cbmp.SetPixel((int)Math.Floor(tx) + 1, ty, c2);

                            ty++;
                            tx += m;
                        }
                    }
                }
            }

            else if (dx > 0 && dy < 0)
            {
                if(dx > -dy)
                {
                    if (!antialiasing)
                    {
                        int d = 2 * dy + dx;
                        int dE = 2 * dy;
                        int dSE = 2 * (dy + dx);

                        while (xf < xb)
                        {
                            ++xf; --xb;
                            if (d > 0)
                                d += dE;
                            else
                            {
                                d += dSE;
                                --yf;
                                ++yb;
                            }
                            cbmp.SetPixel(xf, yf, color);
                            cbmp.SetPixel(xb, yb, color);

                            if ((bool)thickLineCheckBox.IsChecked)
                            {
                                for (int i = 1; i < thickness; i++)
                                {
                                    cbmp.SetPixel(xf, yf + i, color);
                                    cbmp.SetPixel(xb, yb + i, color);

                                    cbmp.SetPixel(xf, yf - i, color);
                                    cbmp.SetPixel(xb, yb - i, color);
                                }
                            }
                        }
                    }
                    else
                    {
                        int tx = x1;
                        double ty = y1;
                        double m = (double)dy / dx;
                        while (tx < x2)
                        {
                            double floatingPart = ty - Math.Truncate(ty);

                            System.Drawing.Color c1 = AntialiasingColor(color, 1 - floatingPart, bgColor, floatingPart);
                            System.Drawing.Color c2 = AntialiasingColor(color, floatingPart, bgColor, 1 - floatingPart);

                            cbmp.SetPixel(tx, (int)Math.Floor(ty), c1);
                            cbmp.SetPixel(tx, (int)Math.Floor(ty) + 1, c2);

                            tx++;
                            ty += m;
                        }
                    }
                }
                else if(dx > dy)
                {
                    if (!antialiasing)
                    {
                        int d = 2 * dx + dy;
                        int dS = 2 * dx;
                        int dSE = 2 * (dx + dy);

                        while (yf > yb)
                        {
                            --yf; ++yb;
                            if (d < 0)
                                d += dS;
                            else
                            {
                                d += dSE;
                                ++xf;
                                --xb;
                            }
                            cbmp.SetPixel(xf, yf, color);
                            cbmp.SetPixel(xb, yb, color);

                            if ((bool)thickLineCheckBox.IsChecked)
                            {
                                for (int i = 1; i < thickness; i++)
                                {
                                    cbmp.SetPixel(xf + 1, yf, color);
                                    cbmp.SetPixel(xb + 1, yb, color);

                                    cbmp.SetPixel(xf - 1, yf, color);
                                    cbmp.SetPixel(xb - 1, yb, color);
                                }
                            }
                        }
                    }
                    else
                    {
                        double tx = x2;
                        int ty = y2;
                        double m = (double)dx / dy;
                        while (ty < y1)
                        {
                            double floatingPart = tx - Math.Truncate(tx);

                            System.Drawing.Color c1 = AntialiasingColor(color, 1 - floatingPart, bgColor, floatingPart);
                            System.Drawing.Color c2 = AntialiasingColor(color, floatingPart, bgColor, 1 - floatingPart);

                            cbmp.SetPixel((int)Math.Floor(tx), ty, c1);
                            cbmp.SetPixel((int)Math.Floor(tx) + 1, ty, c2);

                            ty++;
                            tx += m;
                        }
                    }
                }
            }

            else if (dx < 0 && dy > 0)
            {
                if(dx > -dy)
                {
                    if (!antialiasing)
                    {
                        int d = 2 * dx + dy;
                        int dN = 2 * dx;
                        int dNW = 2 * (dx + dy);

                        while (yf < yb)
                        {
                            ++yf; --yb;
                            if (d > 0)
                                d += dN;
                            else
                            {
                                d += dNW;
                                --xf;
                                ++xb;
                            }
                            cbmp.SetPixel(xf, yf, color);
                            cbmp.SetPixel(xb, yb, color);

                            if ((bool)thickLineCheckBox.IsChecked)
                            {
                                for (int i = 1; i < thickness; i++)
                                {
                                    cbmp.SetPixel(xf + i, yf, color);
                                    cbmp.SetPixel(xb + i, yb, color);

                                    cbmp.SetPixel(xf - i, yf, color);
                                    cbmp.SetPixel(xb - i, yb, color);
                                }
                            }
                        }
                    }
                    else
                    {
                        double tx = x1;
                        int ty = y1;
                        double m = (double)dx / dy;
                        while (ty < y2)
                        {
                            double floatingPart = tx - Math.Truncate(tx);

                            System.Drawing.Color c1 = AntialiasingColor(color, 1 - floatingPart, bgColor, floatingPart);
                            System.Drawing.Color c2 = AntialiasingColor(color, floatingPart, bgColor, 1 - floatingPart);

                            cbmp.SetPixel((int)Math.Floor(tx), ty, c1);
                            cbmp.SetPixel((int)Math.Floor(tx) + 1, ty, c2);

                            ty++;
                            tx += m;
                        }
                    }
                }
                else if(-dx > dy)
                {
                    if (!antialiasing)
                    {
                        int d = 2 * dy + dx;
                        int dW = 2 * dy;
                        int dNW = 2 * (dy + dx);

                        while (xb < xf)
                        {
                            ++xb; --xf;
                            if (d < 0)
                                d += dW;
                            else
                            {
                                d += dNW;
                                ++yf;
                                --yb;
                            }
                            cbmp.SetPixel(xf, yf, color);
                            cbmp.SetPixel(xb, yb, color);

                            if ((bool)thickLineCheckBox.IsChecked)
                            {
                                for (int i = 1; i < thickness; i++)
                                {
                                    cbmp.SetPixel(xf, yf + i, color);
                                    cbmp.SetPixel(xb, yb + i, color);

                                    cbmp.SetPixel(xf, yf - i, color);
                                    cbmp.SetPixel(xb, yb - i, color);
                                }
                            }
                        }
                    }
                    else
                    {
                        int tx = x2;
                        double ty = y2;
                        double m = (double)dy / dx;
                        while (tx < x1)
                        {
                            double floatingPart = ty - Math.Truncate(ty);

                            System.Drawing.Color c1 = AntialiasingColor(color, 1 - floatingPart, bgColor, floatingPart);
                            System.Drawing.Color c2 = AntialiasingColor(color, floatingPart, bgColor, 1 - floatingPart);

                            cbmp.SetPixel(tx, (int)Math.Floor(ty), c1);
                            cbmp.SetPixel(tx, (int)Math.Floor(ty) + 1, c2);

                            tx++;
                            ty += m;
                        }
                    }
                }
            }

            else if(dx < 0 && dy < 0)
            {
                if (dx < dy)
                {
                    if (!antialiasing)
                    {
                        int d = 2 * dy - dx;
                        int dW = 2 * dy;
                        int dSW = 2 * (dy - dx);

                        while (xf > xb)
                        {
                            --xf; ++xb;
                            if (d > 0)
                                d += dW;
                            else
                            {
                                d += dSW;
                                --yf;
                                ++yb;
                            }
                            cbmp.SetPixel(xf, yf, color);
                            cbmp.SetPixel(xb, yb, color);

                            if ((bool)thickLineCheckBox.IsChecked)
                            {
                                for (int i = 1; i < thickness; i++)
                                {
                                    cbmp.SetPixel(xf, yf + i, color);
                                    cbmp.SetPixel(xb, yb + i, color);

                                    cbmp.SetPixel(xf, yf - i, color);
                                    cbmp.SetPixel(xb, yb - i, color);
                                }
                            }
                        }
                    }
                    else
                    {
                        int tx = x2;
                        double ty = y2;
                        double m = (double)dy / dx;
                        while (tx < x1)
                        {
                            double floatingPart = ty - Math.Truncate(ty);

                            System.Drawing.Color c1 = AntialiasingColor(color, 1 - floatingPart, bgColor, floatingPart);
                            System.Drawing.Color c2 = AntialiasingColor(color, floatingPart, bgColor, 1 - floatingPart);

                            cbmp.SetPixel(tx, (int)Math.Floor(ty), c1);
                            cbmp.SetPixel(tx, (int)Math.Floor(ty) + 1, c2);

                            tx++;
                            ty += m;
                        }
                    }
                }
                else if (dx > dy)
                {
                    if (!antialiasing)
                    {
                        int d = 2 * dx - dy;
                        int dS = 2 * dx;
                        int dSW = 2 * (dx - dy);

                        while (yf > yb)
                        {
                            --yf; ++yb;
                            if (d > 0)
                                d += dS;
                            else
                            {
                                d += dSW;
                                --xf;
                                ++xb;
                            }
                            cbmp.SetPixel(xf, yf, color);
                            cbmp.SetPixel(xb, yb, color);

                            if ((bool)thickLineCheckBox.IsChecked)
                            {
                                for (int i = 1; i < thickness; i++)
                                {
                                    cbmp.SetPixel(xf + i, yf, color);
                                    cbmp.SetPixel(xb + i, yb, color);

                                    cbmp.SetPixel(xf - i, yf, color);
                                    cbmp.SetPixel(xb - i, yb, color);
                                }
                            }
                        }
                    }
                    else
                    {
                        double tx = x2;
                        int ty = y2;
                        double m = (double)dx / dy;
                        while (ty < y1)
                        {
                            double floatingPart = tx - Math.Truncate(tx);

                            System.Drawing.Color c1 = AntialiasingColor(color, 1 - floatingPart, bgColor, floatingPart);
                            System.Drawing.Color c2 = AntialiasingColor(color, floatingPart, bgColor, 1 - floatingPart);

                            cbmp.SetPixel((int)Math.Floor(tx), ty, c1);
                            cbmp.SetPixel((int)Math.Floor(tx) + 1, ty, c2);

                            ty++;
                            tx += m;
                        }
                    }
                }
            }
            else if(dx == 0)
            {
                if(dy > 0)
                {
                    while(yf < yb)
                    {
                        ++yf; --yb;
                        cbmp.SetPixel(xf, yf, color);
                        cbmp.SetPixel(xb, yb, color);

                        if ((bool)thickLineCheckBox.IsChecked)
                        {
                            for (int i = 1; i < thickness; i++)
                            {
                                cbmp.SetPixel(xf + i, yf, color);
                                cbmp.SetPixel(xb + i, yb, color);

                                cbmp.SetPixel(xf - i, yf, color);
                                cbmp.SetPixel(xb - i, yb, color);
                            }
                        }
                    }
                }
                if(dy <= 0)
                {
                    while (yf > yb)
                    {
                        --yf; ++yb;
                        cbmp.SetPixel(xf, yf, color);
                        cbmp.SetPixel(xb, yb, color);

                        if ((bool)thickLineCheckBox.IsChecked)
                        {
                            for (int i = 1; i < thickness; i++)
                            {
                                cbmp.SetPixel(xf + i, yf, color);
                                cbmp.SetPixel(xb + i, yb, color);

                                cbmp.SetPixel(xf - i, yf, color);
                                cbmp.SetPixel(xb - i, yb, color);
                            }
                        }
                    }
                }
            }
            else if (dy == 0)
            {
                if (dx > 0)
                {
                    while (xf < xb)
                    {
                        ++xf; --xb;
                        cbmp.SetPixel(xf, yf, color);
                        cbmp.SetPixel(xb, yb, color);

                        if ((bool)thickLineCheckBox.IsChecked)
                        {
                            for (int i = 1; i < thickness; i++)
                            {
                                cbmp.SetPixel(xf, yf + i, color);
                                cbmp.SetPixel(xb, yb + i, color);

                                cbmp.SetPixel(xf, yf - i, color);
                                cbmp.SetPixel(xb, yb - i, color);
                            }
                        }
                    }
                }
                if (dx < 0)
                {
                    while (xf > xb)
                    {
                        --xf; ++xb;
                        cbmp.SetPixel(xf, yf, color);
                        cbmp.SetPixel(xb, yb, color);

                        if ((bool)thickLineCheckBox.IsChecked)
                        {
                            for (int i = 1; i < thickness; i++)
                            {
                                cbmp.SetPixel(xf, yf + i, color);
                                cbmp.SetPixel(xb, yb + i, color);

                                cbmp.SetPixel(xf, yf - i, color);
                                cbmp.SetPixel(xb, yb - i, color);
                            }
                        }
                    }
                }
            }

            ShowBitmap();
        }

        private void AlternativeMidpointCircle(Bitmap cbmp, int x1, int y1, int x2, int y2, System.Drawing.Color color)
        {
            int R = (int)Math.Sqrt(Math.Pow((Math.Abs(x2) - Math.Abs(x1)), 2) + Math.Pow((Math.Abs(y2) - Math.Abs(y1)), 2));

            int d = 1 - R;
            int x = 0;
            int y = R;
            int draw_x = x + x1;
            int draw_y = y + y1;

            if (draw_x < width && draw_x > 0 && draw_y < height && draw_y > 0)
                cbmp.SetPixel(draw_x, draw_y, color);

            int dE = 3;
            int dSE = 5 - 2 * R;

            while(y > x)
            {
                if(d < 0)   // move to E
                {
                    d += dE;
                    dE += 2;
                    dSE += 2;
                }
                else        // move to SE
                {
                    d += dSE;
                    dE += 2;
                    dSE += 4;
                    --y;
                }
                ++x;

                draw_x = y + x1;
                draw_y = x + y1;
                if (draw_x < width && draw_x > 0 && draw_y < height && draw_y > 0)
                    cbmp.SetPixel(draw_x, draw_y, color);
                draw_x = x + x1;
                draw_y = y + y1;
                if (draw_x < width && draw_x > 0 && draw_y < height && draw_y > 0)
                    cbmp.SetPixel(draw_x, draw_y, color);
                draw_x = x + x1;
                draw_y = y1 - y;
                if (draw_x < width && draw_x > 0 && draw_y < height && draw_y > 0)
                    cbmp.SetPixel(draw_x, draw_y, color);
                draw_x = y + x1;
                draw_y = y1 - x;
                if (draw_x < width && draw_x > 0 && draw_y < height && draw_y > 0)
                    cbmp.SetPixel(draw_x, draw_y, color);
                draw_x = x1 - y;
                draw_y = y1 - x;
                if (draw_x < width && draw_x > 0 && draw_y < height && draw_y > 0)
                    cbmp.SetPixel(draw_x, draw_y, color);
                draw_x = x1 - x;
                draw_y = y1 - y;
                if (draw_x < width && draw_x > 0 && draw_y < height && draw_y > 0)
                    cbmp.SetPixel(draw_x, draw_y, color);
                draw_x = x1 - x;
                draw_y = y + y1;
                if (draw_x < width && draw_x > 0 && draw_y < height && draw_y > 0)
                    cbmp.SetPixel(draw_x, draw_y, color);
                draw_x = x1 - y;
                draw_y = x + y1;
                if (draw_x < width && draw_x > 0 && draw_y < height && draw_y > 0)
                    cbmp.SetPixel(draw_x, draw_y, color);
            }

            ShowBitmap();
        }

        int Sign(int dx, int dy, int ex, int ey, int fx, int fy)
        {
            return Math.Sign((ex - dx) * (fy - dy) - (ey - dy) * (fx - dx));
        }

        private void DrawCapsule(Bitmap cbmp, int x1, int y1, int x2, int y2, int x3, int y3, System.Drawing.Color color)
        {
            int R = (int)Math.Sqrt(Math.Pow((Math.Abs(x3) - Math.Abs(x2)), 2) + Math.Pow((Math.Abs(y3) - Math.Abs(y2)), 2));

            int vx = x2 - x1;
            int vy = y2 - y1;
            double v_len = Math.Sqrt(Math.Pow((double)(x2 - x1), 2) + Math.Pow(((double)(y2 - y1)), 2));

            int wx = (int)((vy / v_len) * R);
            int wy = (int)((-vx / v_len) * R);

            SymmetricMidpointLine(cbmp, x1 + wx, y1 + wy, x2 + wx, y2 + wy, color);
            SymmetricMidpointLine(cbmp, x1 - wx, y1 - wy, x2 - wx, y2 - wy, color);

            int sign = 0;
            int ex = x1 + wx;
            int ey = y1 + wy;
            int fx = x2 - wx;
            int fy = y2 - wy;

            int d = 1 - R;
            int x = 0;
            int y = R;
            int draw_x1 = x + x1 + wx;
            int draw_y1 = y + y1 + wy;
            int draw_x2 = x + x2 + wx;
            int draw_y2 = y + y2 + wy;

            int dE = 3;
            int dSE = 5 - 2 * R;

            while (y > x)
            {
                if (d < 0)   // move to E
                {
                    d += dE;
                    dE += 2;
                    dSE += 2;
                }
                else        // move to SE
                {
                    d += dSE;
                    dE += 2;
                    dSE += 4;
                    --y;
                }
                ++x;

                // Point A (x1,y1)

                draw_x1 = y + x1;
                draw_y1 = x + y1;
                sign = Sign(x1, y1, ex, ey, draw_x1, draw_y1);
                if (sign <= 0 && draw_x1 < width && draw_x1 > 0 && draw_y1 < height && draw_y1 > 0)
                    cbmp.SetPixel(draw_x1, draw_y1, color);
                draw_x1 = x + x1;
                draw_y1 = y + y1;
                sign = Sign(x1, y1, ex, ey, draw_x1, draw_y1);
                if (sign <= 0 && draw_x1 < width && draw_x1 > 0 && draw_y1 < height && draw_y1 > 0)
                    cbmp.SetPixel(draw_x1, draw_y1, color);
                draw_x1 = x + x1;
                draw_y1 = y1 - y;
                sign = Sign(x1, y1, ex, ey, draw_x1, draw_y1);
                if (sign <= 0 && draw_x1 < width && draw_x1 > 0 && draw_y1 < height && draw_y1 > 0)
                    cbmp.SetPixel(draw_x1, draw_y1, color);
                draw_x1 = y + x1;
                draw_y1 = y1 - x;
                sign = Sign(x1, y1, ex, ey, draw_x1, draw_y1);
                if (sign <= 0 && draw_x1 < width && draw_x1 > 0 && draw_y1 < height && draw_y1 > 0)
                    cbmp.SetPixel(draw_x1, draw_y1, color);
                draw_x1 = x1 - y;
                draw_y1 = y1 - x;
                sign = Sign(x1, y1, ex, ey, draw_x1, draw_y1);
                if (sign <= 0 && draw_x1 < width && draw_x1 > 0 && draw_y1 < height && draw_y1 > 0)
                    cbmp.SetPixel(draw_x1, draw_y1, color);
                draw_x1 = x1 - x;
                draw_y1 = y1 - y;
                sign = Sign(x1, y1, ex, ey, draw_x1, draw_y1);
                if (sign <= 0 && draw_x1 < width && draw_x1 > 0 && draw_y1 < height && draw_y1 > 0)
                    cbmp.SetPixel(draw_x1, draw_y1, color);
                draw_x1 = x1 - x;
                draw_y1 = y + y1;
                sign = Sign(x1, y1, ex, ey, draw_x1, draw_y1);
                if (sign <= 0 && draw_x1 < width && draw_x1 > 0 && draw_y1 < height && draw_y1 > 0)
                    cbmp.SetPixel(draw_x1, draw_y1, color);
                draw_x1 = x1 - y;
                draw_y1 = x + y1;
                sign = Sign(x1, y1, ex, ey, draw_x1, draw_y1);
                if (sign <= 0 && draw_x1 < width && draw_x1 > 0 && draw_y1 < height && draw_y1 > 0)
                    cbmp.SetPixel(draw_x1, draw_y1, color);

                // Point B (x2,y2)

                draw_x2 = y + x2;
                draw_y2 = x + y2;
                sign = Sign(x2, y2, fx, fy, draw_x2, draw_y2);
                if (sign <= 0 && draw_x2 < width && draw_x2 > 0 && draw_y2 < height && draw_y2 > 0)
                    cbmp.SetPixel(draw_x2, draw_y2, color);
                draw_x2 = x + x2;
                draw_y2 = y + y2;
                sign = Sign(x2, y2, fx, fy, draw_x2, draw_y2);
                if (sign <= 0 && draw_x2 < width && draw_x2 > 0 && draw_y2 < height && draw_y2 > 0)
                    cbmp.SetPixel(draw_x2, draw_y2, color);
                draw_x2 = x + x2;
                draw_y2 = y2 - y;
                sign = Sign(x2, y2, fx, fy, draw_x2, draw_y2);
                if (sign <= 0 && draw_x2 < width && draw_x2 > 0 && draw_y2 < height && draw_y2 > 0)
                    cbmp.SetPixel(draw_x2, draw_y2, color);
                draw_x2 = y + x2;
                draw_y2 = y2 - x;
                sign = Sign(x2, y2, fx, fy, draw_x2, draw_y2);
                if (sign <= 0 && draw_x2 < width && draw_x2 > 0 && draw_y2 < height && draw_y2 > 0)
                    cbmp.SetPixel(draw_x2, draw_y2, color);
                draw_x2 = x2 - y;
                draw_y2 = y2 - x;
                sign = Sign(x2, y2, fx, fy, draw_x2, draw_y2);
                if (sign <= 0 && draw_x2 < width && draw_x2 > 0 && draw_y2 < height && draw_y2 > 0)
                    cbmp.SetPixel(draw_x2, draw_y2, color);
                draw_x2 = x2 - x;
                draw_y2 = y2 - y;
                sign = Sign(x2, y2, fx, fy, draw_x2, draw_y2);
                if (sign <= 0 && draw_x2 < width && draw_x2 > 0 && draw_y2 < height && draw_y2 > 0)
                    cbmp.SetPixel(draw_x2, draw_y2, color);
                draw_x2 = x2 - x;
                draw_y2 = y + y2;
                sign = Sign(x2, y2, fx, fy, draw_x2, draw_y2);
                if (sign <= 0 && draw_x2 < width && draw_x2 > 0 && draw_y2 < height && draw_y2 > 0)
                    cbmp.SetPixel(draw_x2, draw_y2, color);
                draw_x2 = x2 - y;
                draw_y2 = x + y2;
                sign = Sign(x2, y2, fx, fy, draw_x2, draw_y2);
                if (sign <= 0 && draw_x2 < width && draw_x2 > 0 && draw_y2 < height && draw_y2 > 0)
                    cbmp.SetPixel(draw_x2, draw_y2, color);
            }

            ShowBitmap();
        }

        private List<Point> CreateRectangle(int x1, int y1, int x2, int y2)
        {
            List<Point> result = new List<Point>();

            if (x1 < x2)
            {
                if (y1 < y2)
                {
                    result.Add(new Point(x1, y1));
                    result.Add(new Point(x2, y1));
                    result.Add(new Point(x2, y2));
                    result.Add(new Point(x1, y2));
                }
                else
                {
                    result.Add(new Point(x1, y2));
                    result.Add(new Point(x2, y2));
                    result.Add(new Point(x2, y1));
                    result.Add(new Point(x1, y1));
                }
            }
            else
            {
                if(y1 < y2)
                {
                    result.Add(new Point(x2, y1));
                    result.Add(new Point(x1, y1));
                    result.Add(new Point(x1, y2));
                    result.Add(new Point(x2, y2));
                }
                else
                {
                    result.Add(new Point(x2, y2));
                    result.Add(new Point(x1, y2));
                    result.Add(new Point(x1, y1));
                    result.Add(new Point(x2, y1));
                }
            }

            return result;
        }

        private void FillColorRectangle(Bitmap cbmp, MyRectangle rectangle)
        {
            int xMin = (int)rectangle.points[0].X + 1;
            int xMax = (int)rectangle.points[1].X;

            int yMax = (int)rectangle.points[3].Y;
            int y = (int)rectangle.points[0].Y + 1;

            while(y < yMax)
            {
                int x = xMin;
                while(x < xMax)
                {
                    cbmp.SetPixel(x, y, rectangle.fillColor);
                    x++;
                }
                y++;
            }
        }

        private void FillColorPolygon(Bitmap cbmp, MyPolygon polyg)
        {
            int N = polyg.points.Count - 1;

            List<ActiveEdge> AET = new List<ActiveEdge>();

            int[] indices = new int[N];
            for (int j = 0; j < N; j++)
                indices[j] = j;

            indices = BubbleSortVertices(indices, polyg.points);

            int k = 0;
            int i = indices[k];

            int y = (int)polyg.points[indices[0]].Y;
            int ymin = y;
            int ymax = (int)polyg.points[indices[N - 1]].Y;

            while(y < ymax)
            {
                while((int)polyg.points[i].Y == y)
                {
                    if (polyg.points[((i - 1)%N + N)%N].Y > polyg.points[i].Y)
                    {
                        ActiveEdge newEdge = new ActiveEdge(polyg.points[i], polyg.points[((i - 1) % N + N) % N]);
                        if(newEdge.m != null)
                            AET.Add(newEdge);
                    }
                    if (polyg.points[((i + 1) % N + N) % N].Y > polyg.points[i].Y)
                    {
                        ActiveEdge newEdge = new ActiveEdge(polyg.points[i], polyg.points[((i + 1) % N + N) % N]);
                        if (newEdge.m != null)
                            AET.Add(newEdge);
                    }
                       
                    ++k;
                    i = indices[k];
                }

                AET.Sort(SortAET);

                bool even = true;
                int indx = 0;
                int x = 0;
                int xmax = (int)AET[AET.Count - 1].x;

                while (x < xmax)
                {
                    if (x >= AET[indx].x)
                    {
                        if (even)
                            even = false;
                        else
                            even = true;
                        indx++;
                    }

                    if (!even)
                        cbmp.SetPixel(x, y, polyg.fillColor);
                    x++;
                }

                //MessageBox.Show("Next row");
                //for (int j = 0; j < AET.Count; j += 2)
                //{
                //    MessageBox.Show("Next pair");
                //    int xx = (int)AET[j].x + 1;

                //    while(xx < (int)AET[j+1].x)
                //        cbmp.SetPixel(xx++, y, polyg.fillColor);
                //}

                y++;

                List<ActiveEdge> toRemove = new List<ActiveEdge>();

                foreach (var edge in AET)
                {
                    if (edge.ymax <= y)
                        toRemove.Add(edge);
                }

                foreach(var edge in toRemove)
                {
                    AET.Remove(edge);
                }

                foreach(var edge in AET)
                {
                    if (edge.m != null)
                        edge.x += (double)edge.m;
                }
            }


        }

        private int[] BubbleSortVertices(int[] indices, List<Point> points)
        {
            for (int i = 0; i < indices.Length; i++)
            {
                for (int j = 0; j < indices.Length - 1; j++)
                {
                    if (points[indices[j]].Y > points[indices[j+1]].Y)
                    {
                        int temp = indices[j];
                        indices[j] = indices[j + 1];
                        indices[j + 1] = temp;
                    }
                }
            }
            return indices;
        }

        private int SortAET(ActiveEdge x, ActiveEdge y)
        {
            if (x.x > y.x)
                return 1;
            else if (x.x < y.x)
                return -1;
            else
                return 0;
        }

        // Menu checkboxes

        private void UncheckAll()
        {
            drawingLine = false;
            drawLineCheckBox.IsChecked = false;
            drawingPolygon = false;
            drawPolygonCheckBox.IsChecked = false;
            drawingCircle = false;
            drawCircleCheckBox.IsChecked = false;
            movingPoints = false;
            movePointsCheckBox.IsChecked = false;
            deleteObject = false;
            deleteCheckBox.IsChecked = false;
            drawingCapsule = false;
            drawCapsuleCheckBox.IsChecked = false;
            drawingRectangle = false;
            drawRectangleCheckBox.IsChecked = false;
            moveWholeRectangle = false;
            moveRectangleCheckBox.IsChecked = false;
            selectForClipping = false;
            clippingCheckBox.IsChecked = false;
            fillingColor = false;
            fillColorCheckBox.IsChecked = false;
        }

        private void DrawLineCheckBox_Click(object sender, RoutedEventArgs e)
        {
            if (drawingLine)
            {
                drawingLine = false;
                isDrawing = false;
            }
            else
            {
                UncheckAll();
                drawingLine = true;
                drawLineCheckBox.IsChecked = true;
                isDrawing = false;
            }
        }

        private void DrawPolygonCheckBox_Click(object sender, RoutedEventArgs e)
        {
            if (drawingPolygon)
            {
                drawingPolygon = false;
                isDrawing = false;
            }
            else
            {
                UncheckAll();
                drawingPolygon = true;
                drawPolygonCheckBox.IsChecked = true;
                isDrawing = false;
            }
        }

        private void ThickLineCheckBox_Click(object sender, RoutedEventArgs e)
        {
            thickness = Convert.ToInt32(thickLineTextBox.Text);
            if (thickness < 1)
                thickness = 1;
            else if (thickness % 2 == 0)
                thickness -= 1;
            if (thickness > maxThickness)
                thickness = maxThickness;

            thickness -= 1;
            thickness /= 2;
        }

        private void DrawCircleCheckBox_Click(object sender, RoutedEventArgs e)
        {
            if (drawingCircle)
            {
                drawingCircle = false;
                isDrawing = false;
            }
            else
            {
                UncheckAll();
                drawingCircle = true;
                drawCircleCheckBox.IsChecked = true;
                isDrawing = false;
            }
        }

        private void DrawButton_Click(object sender, RoutedEventArgs e)
        {
            DrawObjects();
        }

        private void DrawCapsuleCheckBox_Click(object sender, RoutedEventArgs e)
        {
            if (drawingCapsule)
            {
                drawingCapsule = false;
                isDrawing = false;
            }
            else
            {
                UncheckAll();
                drawingCapsule = true;
                num_p = 1;
                drawCapsuleCheckBox.IsChecked = true;
                isDrawing = false;
            }
        }

        private void MovePointsCheckBox_Click(object sender, RoutedEventArgs e)
        {
            if (movingPoints)
            {
                movingPoints = false;
                isDrawing = false;
            }
            else
            {
                UncheckAll();
                movingPoints = true;
                movePointsCheckBox.IsChecked = true;
                isDrawing = false;
            }
        }

        private void DeleteCheckBox_Click(object sender, RoutedEventArgs e)
        {
            if (deleteObject)
            {
                deleteObject = false;
                isDrawing = false;
            }
            else
            {
                UncheckAll();
                deleteObject = true;
                deleteCheckBox.IsChecked = true;
                isDrawing = false;
            }
        }

        private void DeleteObject(MouseButtonEventArgs e)
        {
            Point p = e.GetPosition(image);
            bool flag = true;

            int x_max = (int)p.X + around;
            int x_min = (int)p.X - around;
            int y_max = (int)p.Y + around;
            int y_min = (int)p.Y - around;

            if (flag)
            {
                foreach(var line in lines)
                {
                    if (line.Item1.X > x_min && line.Item1.X < x_max && line.Item1.Y > y_min && line.Item1.Y < y_max)
                    {
                        flag = false;
                        lines.Remove(line);
                        break;
                    }
                    if (line.Item2.X > x_min && line.Item2.X < x_max && line.Item2.Y > y_min && line.Item2.Y < y_max)
                    {
                        flag = false;
                        lines.Remove(line);
                        break;
                    }
                }
            }

            if (flag)
            {
                for (int i = 0; i < myPolygons.Count; i++)
                {
                    for (int j = 0; j < myPolygons[i].points.Count; j++)
                    {
                        if (myPolygons[i].points[j].X > x_min && myPolygons[i].points[j].X < x_max && myPolygons[i].points[j].Y > y_min && myPolygons[i].points[j].Y < y_max)
                        {
                            flag = false;

                            foreach (var polyg in myPolygons)
                            {
                                if (polyg.clipToPolygon && polyg.clippingPolygon == myPolygons[i])
                                {
                                    polyg.clippingPolygon = null;
                                    polyg.clipToPolygon = false;
                                }
                            }

                            myPolygons.RemoveAt(i);

                            break;
                        }
                    }
                }
            }

            if (flag)
            {
                foreach (var circle in circles) // click on the center only atm
                {
                    if (circle.Item1.X > x_min && circle.Item1.X < x_max && circle.Item1.Y > y_min && circle.Item1.Y < y_max)
                    {
                        flag = false;
                        circles.Remove(circle);
                        break;
                    }
                }
            }

            if (flag)
            {
                foreach(var rectangle in myRectangles)
                {
                    foreach(var vertex in rectangle.points)
                    {
                        if (vertex.X > x_min && vertex.X < x_max && vertex.Y > y_min && vertex.Y < y_max)
                        {
                            flag = false;
                            rectangleToDelete = rectangle;
                            break;
                        }
                    }
                }
                if (!flag)
                {
                    foreach(var polyg in myPolygons)
                    {
                        if(polyg.clipToRectangle && polyg.clippingRectangle == rectangleToDelete)
                        {
                            polyg.clipToRectangle = false;
                            polyg.clippingRectangle = null;
                        }
                    }
                    myRectangles.Remove(rectangleToDelete);
                }
            }

            if (!flag)
                DrawObjects();
        }

        private void DrawRectangleCheckBox_Click(object sender, RoutedEventArgs e)
        {
            if (drawingRectangle)
            {
                drawingRectangle = false;
                isDrawing = false;
            }
            else
            {
                UncheckAll();
                drawingRectangle = true;
                drawRectangleCheckBox.IsChecked = true;
                isDrawing = false;
            }
        }

        private void MoveRectangleCheckBox_Click(object sender, RoutedEventArgs e)
        {
            if (moveWholeRectangle)
            {
                moveWholeRectangle = false;
                isDrawing = false;
            }
            else
            {
                UncheckAll();
                moveWholeRectangle = true;
                moveRectangleCheckBox.IsChecked = true;
                isDrawing = false;
            }
        }

        private void ClippingCheckBox_Click(object sender, RoutedEventArgs e)
        {
            if (selectForClipping)
            {
                selectForClipping = false;
                haveFigureToClip = false;
                haveClippingFigure = false;
                isDrawing = false;
            }
            else
            {
                UncheckAll();
                selectForClipping = true;
                haveFigureToClip = false;
                haveClippingFigure = false;
                clippingCheckBox.IsChecked = true;
                isDrawing = false;
            }
        }

        private void FillColorCheckBox_Click(object sender, RoutedEventArgs e)
        {
            if (fillingColor)
            {
                fillingColor = false;
                isDrawing = false;
            }
            else
            {
                UncheckAll();
                fillingColor = true;
                fillColorCheckBox.IsChecked = true;
            }
        }

        private void AntialiasingCheckBox_Click(object sender, RoutedEventArgs e)
        {
            if (antialiasing)
            {
                antialiasing = false;
                isDrawing = false;
                DrawObjects();
            }
            else
            {
                antialiasing = true;
                antialiasingCheckBox.IsChecked = true;
                DrawObjects();
            }
        }
    }
}
