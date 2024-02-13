using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows;
using System.ComponentModel;

namespace Debricked.Shapes
{
    public sealed class Arc : FrameworkElement
    {
        [Category("Arc")]
        public Point Center
        {
            get { return (Point)GetValue(CenterProperty); }
            set { SetValue(CenterProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Center.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CenterProperty =
            DependencyProperty.Register("Center", typeof(Point), typeof(Arc), new FrameworkPropertyMetadata(new Point(0, 0), FrameworkPropertyMetadataOptions.AffectsRender));

        /// <summary>
        /// Forces the Arc to the center of the Parent container.
        /// </summary>
        [Category("Arc")]
        public bool OverrideCenter
        {
            get { return (bool)GetValue(OverrideCenterProperty); }
            set { SetValue(OverrideCenterProperty, value); }
        }

        // Using a DependencyProperty as the backing store for OverrideCenter.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty OverrideCenterProperty =
            DependencyProperty.Register("OverrideCenter", typeof(bool), typeof(Arc), new FrameworkPropertyMetadata((bool)false, FrameworkPropertyMetadataOptions.AffectsRender));

        /// <summary>
        /// Start angle of arc, using standard coordinates. (Zero is right, CCW positive direction)
        /// </summary>
        [Category("Arc")]
        public double StartAngle
        {
            get { return (double)GetValue(StartAngleProperty); }
            set { SetValue(StartAngleProperty, value); }
        }

        // Using a DependencyProperty as the backing store for StartAngle.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty StartAngleProperty =
            DependencyProperty.Register("StartAngle", typeof(double), typeof(Arc), new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.AffectsRender));

        /// <summary>
        /// Length of Arc in degrees.
        /// </summary>
        [Category("Arc")]
        public double SweepAngle
        {
            get { return (double)GetValue(SweepAngleProperty); }
            set { SetValue(SweepAngleProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SweepAngle.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SweepAngleProperty =
            DependencyProperty.Register("SweepAngle", typeof(double), typeof(Arc), new FrameworkPropertyMetadata((double)180, FrameworkPropertyMetadataOptions.AffectsRender));

        /// <summary>
        /// Size of Arc.
        /// </summary>
        [Category("Arc")]
        public double Radius
        {
            get { return (double)GetValue(RadiusProperty); }
            set { SetValue(RadiusProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Radius.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty RadiusProperty =
            DependencyProperty.Register("Radius", typeof(double), typeof(Arc), new FrameworkPropertyMetadata(10.0, FrameworkPropertyMetadataOptions.AffectsRender));

        [Category("Arc")]
        public Brush Stroke
        {
            get { return (Brush)GetValue(StrokeProperty); }
            set { SetValue(StrokeProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Stroke.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty StrokeProperty =
            DependencyProperty.Register("Stroke", typeof(Brush), typeof(Arc), new FrameworkPropertyMetadata((Brush)Brushes.Black, FrameworkPropertyMetadataOptions.AffectsRender));

        [Category("Arc")]
        public double StrokeThickness
        {
            get { return (double)GetValue(StrokeThicknessProperty); }
            set { SetValue(StrokeThicknessProperty, value); }
        }

        // Using a DependencyProperty as the backing store for StrokeThickness.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty StrokeThicknessProperty =
            DependencyProperty.Register("StrokeThickness", typeof(double), typeof(Arc), new FrameworkPropertyMetadata((double)1, FrameworkPropertyMetadataOptions.AffectsRender));

        protected override void OnRender(DrawingContext dc)
        {
            base.OnRender(dc);
            Draw(dc);
        }

        private void Draw(DrawingContext dc)
        {
            Point center = new Point();
            if (OverrideCenter)
            {
                Rect rect = new Rect(RenderSize);
                center = CenterPoint(rect);
            }
            else
            {
                center = Center;
            }

            Point startPoint = PolarToCartesian(StartAngle, Radius, center);
            Point endPoint = PolarToCartesian(StartAngle + SweepAngle, Radius, center);
            Size size = new Size(Radius, Radius);

            bool isLarge = (StartAngle + SweepAngle) - StartAngle > 180;

            List<PathSegment> segments = new List<PathSegment>(1);
            segments.Add(new ArcSegment(endPoint, new Size(Radius, Radius), 0.0, isLarge, SweepDirection.Clockwise, true));

            List<PathFigure> figures = new List<PathFigure>(1);
            PathFigure pf = new PathFigure(startPoint, segments, true);
            pf.IsClosed = false;
            figures.Add(pf);
            Geometry g = new PathGeometry(figures, FillRule.EvenOdd, null);

            dc.DrawGeometry(null, new Pen(Stroke, StrokeThickness), g);
        }

        public static Point PolarToCartesian(double angle, double radius, Point center)
        {
            return new Point((center.X + (radius * Math.Cos(DegreesToRadian(angle)))), (center.Y + (radius * Math.Sin(DegreesToRadian(angle)))));
        }


        /// <summary>
        /// Given a center point and radius, find the top left point for a rectangle and its size.
        /// </summary>
        /// <param name="centerPoint"></param>
        /// <param name="radius"></param>
        /// <returns></returns>
        public static Rect RectFromCenterPoint(Point centerPoint, int radius)
        {
            Point p = new Point(centerPoint.X - radius, centerPoint.Y - radius);
            return new Rect(p, new Size(radius * 2, radius * 2));
        }

        /// <summary>
        /// Finds the center point of a Rect
        /// </summary>
        /// <param name="rect"></param>
        /// <returns></returns>
        public static Point CenterPoint(Rect rect)
        {
            return new Point(rect.Width / 2, rect.Height / 2);
        }

        /// <summary>
        /// Returns a radius value equal to the smallest side.
        /// </summary>
        /// <param name="rect"></param>
        /// <returns></returns>
        public static double getRadius(Rect rect)
        {
            double dbl = Math.Min(rect.Width, rect.Height);
            return dbl / 2;
        }


        /// <summary>
        /// Since Windows Forms consider an Angle of Zero to be at the 3:00 position and an Angle of 90
        /// to be at the 12:00 position, it is sometimes difficult to visualize where 
        /// 
        /// </summary>
        /// <param name="Angle"></param>
        /// <param name="Offset"></param>
        /// <returns></returns>
        /// <remarks></remarks>
        public static float ReversePolarDirection(float Angle, int Offset)
        {
            return ((360 - Angle) + Offset) % 360;
        }

        /// <summary>
        /// Circumference: C = 2*Pi*r = Pi*d; r=Radius, d=Diameter
        /// </summary>
        /// <param name="Diameter"></param>
        /// <returns></returns>
        /// <remarks></remarks>
        public static double CircumferenceD(double Diameter)
        {
            return Diameter * Math.PI;
        }
        /// <summary>
        /// Circumference: C = 2*Pi*r = Pi*d; r=Radius, d=Diameter
        /// </summary>
        /// <param name="Radius"></param>
        /// <returns></returns>
        /// <remarks></remarks>
        public static double CircumferenceR(double Radius)
        {
            return Radius * Math.PI;
        }
        public static double ScaleWithParam(double Input, double InputMin, double InputMax, double ScaledMin, double ScaledMax)
        {
            //Out = (((ScMax-ScMin)/(InMax-InMin))*Input)+(ScMin-(InMin*((ScMax-ScMin)/(InMax-InMin))
            return (((ScaledMax - ScaledMin) / (InputMax - InputMin)) * Input) + (ScaledMin - (InputMin * ((ScaledMax - ScaledMin) / (InputMax - InputMin))));

        }
        public static double DegreesToRadian(double degrees)
        {
            //Return 2 * Math.PI * degrees / 360.0
            return degrees * (Math.PI / 180);
        }
        private static double RadianToDegrees(double radian)
        {
            return radian * 180 / Math.PI;
        }

        public static double ArcLength(double radius, double radian)
        {
            return radius * radian;
        }
    }
}
