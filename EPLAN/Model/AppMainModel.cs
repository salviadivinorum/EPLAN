using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;

namespace EPLAN.Model
{
	/// <summary>
	/// Main model = application business logic
	/// </summary>
	public class AppMainModel
	{
		public AppMainModel()
		{
			WireBundle = new WireBundle();
		}

		/// <summary>
		/// Inner cables in bundle represented as a text
		/// </summary>
		public string CablesDiameters { get; set; }

		/// <summary>
		/// Bundle (bigest circle) of smaller circles (cables) inside the bundle
		/// </summary>
		public WireBundle WireBundle { get; set; }

		/// <summary>
		/// Read input of cable radii from the user text file
		/// </summary>
		/// <returns>Read radii of cables</returns>
		public IEnumerable<string> ReadCablesFromFile()
		{
			var inputPath = System.IO.Path.Combine(AppDomain.CurrentDomain.SetupInformation.ApplicationBase, "input.txt");
			var lines = System.IO.File.ReadLines(inputPath);
			lines = lines.Where(x => !x.StartsWith('#'));
			return lines;
		}

		/// <summary>
		/// Conver circles (cables) to the Path shapes
		/// </summary>
		/// <returns>List of elipsess</returns>
		public List<Path> GetCirclesAsPaths()
		{
			if (!WireBundle.Circles.Any()) return null;
			// Bundle cable should be red, rest is black
			var circles = WireBundle.Circles.Select(x => CreateEllipse(x.Center, x.Radius, x.Brush)).ToList();
			return circles;
		}

		/// <summary>
		/// Calcluate minimal bundle of cables (radii)
		/// </summary>
		public async Task<List<Circle>> CalculateBundles(IEnumerable<double>[] permutations)
		{
			// separate the calculation into multiple Tasks
			var minRadius = double.MaxValue;
			List<Circle> minShape = new();
			var c = permutations.Length;
			var tasks = new Task[c];
			var locker = new object();

			for (int i = 0; i < c; i++)
			{
				int index = i;
				tasks[i] = Task.Run(() =>
				{
					var p = permutations[index];
					(List<Circle> circles, double bundleRadius) result = CalculateBundle(p);
					if (result != default)
					{
						lock (locker)
						{
							// critical section
							if (result.bundleRadius < minRadius)
							{
								minShape = result.circles;
								minRadius = result.bundleRadius;
							}
						}
					}
				});
			}
			await Task.WhenAll(tasks);
			return minShape;
		}

		/// <summary>
		/// Caulculate one possible bundle of circles from given radii permutation
		/// </summary>
		/// <param name="p"></param>
		/// <returns>Possible 2D placement of circles with bundle radius</returns>
		private (List<Circle> circles, double bundleRadius) CalculateBundle(IEnumerable<double> p)
		{
			var radii = p.ToList();
			var count = p.Count();
			if (count <= 1) return default;

			// the first 2 circle (radii) are taken as the start
			var R1 = radii[0];
			var R2 = radii[1];
			var C1 = new Circle(0, 0, R1);
			var C2 = new Circle(R1 + R2, 0, R2);
			var circles = new List<Circle> { C1, C2 };

			if (count > 2)
			{
				// other circles (radii) - try to add them to C1, C2 in 2D scene
				for (int x = 2; x < count; x++)
				{
					var Rx = radii[x];

					// combine circles as pairs
					var combis = circles.Combinations(2);
					foreach (var c in combis)
					{
						bool wasAdded = false;
						var c1 = c[0].Center;
						var r1 = c[0].Radius;

						var c2 = c[1].Center;
						var r2 = c[1].Radius;

						// find 2 position of third touching circle
						var thirdCircles = FindThirdCircles(c1, r1, c2, r2, Rx);
						if (thirdCircles == null)
						{
							continue;
						}
						foreach (var touch in thirdCircles)
						{
							bool intersect = false;
							foreach (var bundleCirc in circles)
							{
								// my third circle should not intersect other circles in bundle
								var position = GetCirclesPosition(bundleCirc, touch);
								intersect = position == CirclePosition.CirclesIntersects;
								if (intersect) break;
							}

							if (!intersect)
							{
								circles.Add(touch);
								wasAdded = true;
								break;
							}
						}
						if (wasAdded) break;
					}
				}
			}

			if (circles.Count != p.Count())
			{
				// not correct set of all inner circles
				return default;
			}
			var ec = CreateEnclosingCircle(circles);
			ec.Brush = Brushes.Red;
			circles.Add(ec);
			return (circles, ec.Radius);
		}

		/// <summary>
		/// Create Ellipse as Path
		/// </summary>
		/// <param name="pp"></param>
		/// <param name="radius"></param>
		/// <returns>Path</returns>
		private Path CreateEllipse(Point pp, double radius, Brush stroke)
		{
			var path = new Path()
			{
				Data = new EllipseGeometry(pp, radius, radius),
				Stroke = stroke,
				StrokeThickness = 1,
			};
			return path;
		}


		private Circle[] FindThirdCircles(Point p1, double r1, Point p2, double r2, double r3)
		{
			(double[] var1, double[] var2) = ThirdVertex(p1.X, p1.Y, p2.X, p2.Y, r1 + r3, r2 + r3);
			if (var1.Any(x => double.IsNaN(x)) || var2.Any(x => double.IsNaN(x)))
			{
				// valid solution not found
				return null;
			}
			var circles = new Circle[] { new Circle(var1[0], var1[1], r3), new Circle(var2[0], var2[1], r3) };
			return circles;
		}

		/// <summary>
		///  Algorithm to find the third vertex of triangle if I known 2 vertices and length of triangle sides.
		///  Based on answer: https://stackoverflow.com/questions/56427300/how-to-find-the-third-vertices-of-a-triangle-when-lengths-are-unequal
		/// </summary>
		/// <returns>Two possible Circles</returns>
		private (double[] var1, double[] var2) ThirdVertex(double x2, double y2, double x3, double y3, double d1, double d3)
		{
			double d2 = Math.Sqrt(Math.Pow(x3 - x2, 2) + Math.Pow(y3 - y2, 2)); // distance between vertex 2 and 3

			// Orthogonal projection of side 12 onto side 23, calculated using 
			// the Law of cosines:
			double k = (Math.Pow(d2, 2) + Math.Pow(d1, 2) - Math.Pow(d3, 2)) / (2 * d2);
			// height from vertex 1 to side 23 calculated by Pythagoras' theorem:
			double h = Math.Sqrt(Math.Pow(d1, 2) - Math.Pow(k, 2));

			// calculating the output: the coordinates of vertex 1, there are two solutions: 
			double[] vertex_1a = new double[2];
			vertex_1a[0] = x2 + (k / d2) * (x3 - x2) - (h / d2) * (y3 - y2);
			vertex_1a[1] = y2 + (k / d2) * (y3 - y2) + (h / d2) * (x3 - x2);

			double[] vertex_1b = new double[2];
			vertex_1b[0] = x2 + (k / d2) * (x3 - x2) + (h / d2) * (y3 - y2);
			vertex_1b[1] = y2 + (k / d2) * (y3 - y2) - (h / d2) * (x3 - x2);

			return (vertex_1a, vertex_1b);
		}

		/// <summary>
		/// Find large enclosing circle of smaller inner circles
		/// </summary>
		/// <returns>Enclosing circle</returns>
		private Circle CreateEnclosingCircle(List<Circle> inners)
		{
			// combine circles as pairs
			var combis = inners.Combinations(2).ToList();

			var longest = double.MinValue;
			var farther1 = new Point();
			var farther2 = new Point();
			Circle longestCirc1 = null;
			Circle longestCirc2 = null;

			foreach (var p in combis)
			{
				var pc = p.ToList();

				var c1 = pc[0];
				var c2 = pc[1];

				var x1 = c1.X;
				var x2 = c2.X;

				var y1 = c1.Y;
				var y2 = c2.Y;


				double d = Math.Sqrt(Math.Pow(x1 - x2, 2) + Math.Pow(y1 - y2, 2));
				d += (c1.Radius + c2.Radius);

				if (d > longest)
				{
					longest = d;

					farther1 = c1.Center;
					farther2 = c2.Center;

					longestCirc1 = c1;
					longestCirc2 = c2;
				}
			}

			// shift points
			var vectorMove2 = farther2 - farther1;
			vectorMove2.Normalize();
			vectorMove2 *= (longestCirc2.Radius);

			var f2 = new Point(farther2.X, farther2.Y);
			f2 = Point.Add(f2, vectorMove2);

			var vectorMove1 = farther1 - farther2;
			vectorMove1.Normalize();
			vectorMove1 *= (longestCirc1.Radius);

			var f1 = new Point(farther1.X, farther1.Y);
			f1 = Point.Add(f1, vectorMove1);

			// f1 --- f2 is this new connectiong line
			// find center of it

			var fx = (f1.X + f2.X) / 2;
			var fy = (f1.Y + f2.Y) / 2;

			double fd = Math.Sqrt(Math.Pow(f1.X - f2.X, 2) + Math.Pow(f1.Y - f2.Y, 2));
			var fcircle = new Circle(fx, fy, fd / 2);
			return fcircle;

		}

		/// <summary>
		/// Check relation between circles
		/// </summary>
		/// <param name="c1">circ1 </param>
		/// <param name="c2">circ 2</param>
		private CirclePosition GetCirclesPosition(Circle c1, Circle c2)
		{
			Point p1 = c1.Center;
			double r1 = c1.Radius;
			Point p2 = c2.Center;
			double r2 = c2.Radius;

			double x1 = p1.X;
			double y1 = p1.Y;
			double x2 = p2.X;
			double y2 = p2.Y;

			// Calculate the distance between the two centers
			double d = Math.Sqrt(Math.Pow(x1 - x2, 2) + Math.Pow(y1 - y2, 2));

			var BisInsideA = d <= r1 - r2;
			var AisInsideB = d <= r2 - r1;
			var CirclesTouches = d == r1 + r2;
			var CirclesLiesOutsideEachOuther = d > r1 + r2 || d < Math.Abs(r1 - r2);
			var CirclesIntersects = d < (r1 + r2);
			var AreIdentical = d == 0 && r1 == r2;

			if (AreIdentical)
			{
				return CirclePosition.CirclesIntersects;
			}

			var bools = new bool[] { BisInsideA, AisInsideB, CirclesTouches, CirclesLiesOutsideEachOuther, CirclesIntersects, AreIdentical };
			var position = Array.FindIndex(bools, b => b);

			switch (position)
			{
				case 0:
					return CirclePosition.BisInsideA;
				case 1:
					return CirclePosition.AisInsideB;
				case 2:
					return CirclePosition.CirclesTouches;
				case 3:
					return CirclePosition.CirclesLiesOutsideEachOuther;
				case 4:
					return CirclePosition.CirclesIntersects;
				case 5:
					return CirclePosition.AreIdentical;
				default:
					return CirclePosition.NotDefined;
			}
		}

		public enum CirclePosition
		{
			NotDefined,
			AisInsideB,
			BisInsideA,
			CirclesTouches,
			CirclesIntersects,
			CirclesLiesOutsideEachOuther,
			AreIdentical,
		}
	}
}
