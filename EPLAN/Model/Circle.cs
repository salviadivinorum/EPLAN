using System.Windows;
using System.Windows.Media;

namespace EPLAN.Model
{
	/// <summary>
	/// Representation of cable wire
	/// </summary>
	public class Circle
	{
		public double X { get; set; }
		public double Y { get; set; }
		public double Radius { get; set; }
		public Point Center { get; set; }
		public Rect Bounds { get; set; }
		public Brush Brush { get; set; }

		public Circle(double x, double y, double radius)
		{
			X = x;
			Y = y;
			Radius = radius;
			Center= new Point(x, y);
			Bounds = new Rect(new Point(x - (radius * 0.5), y + (radius * 0.5)), new Size(radius * 2, radius * 2));
			Brush = Brushes.Black;
		}
	}
}
