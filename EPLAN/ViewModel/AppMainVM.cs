using EPLAN.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace EPLAN.ViewModel
{
	/// <summary>
	/// Application main Viewmodel = layer between UI (View) and business logic
	/// </summary>
	sealed class AppMainVM : INotifyPropertyChanged
	{
		private AppMainModel model;
		private List<Path> shapes = new List<Path>();
		private double[] cablesD;
		private double scale;
		private string status;
		private ICommand updater;

		public AppMainVM()
		{
			model = new AppMainModel();
			scale = 10;
			var lines = model.ReadCablesFromFile();
			model.CablesDiameters = string.Join(" ", lines);
		}

		/// <summary>
		///  Inner cables in bundle represented as a text
		/// </summary>
		public string CablesDiameters
		{
			get { return model.CablesDiameters; }
			set
			{
				if (model.CablesDiameters != value)
				{
					model.CablesDiameters = value;
					OnPropertyChange(nameof(CablesDiameters));
				}
			}
		}

		/// <summary>
		/// Scale of circle for UI puprose
		/// </summary>
		public double Scale
		{
			get { return scale <= 0 ? 1 : scale; }
			set
			{
				if (scale != value)
				{
					scale = value;
					OnPropertyChange(nameof(Scale));
				}
			}
		}

		private double bundleDiameter;
		public double BundleDiameter
		{
			get { return bundleDiameter; }
			set
			{
				if (bundleDiameter != value)
				{
					bundleDiameter = value;
					OnPropertyChange(nameof(BundleDiameter));
				}
			}
		}

		/// <summary>
		/// UI representation (Ellipses) of Cable wires bundled in 1 cable
		/// </summary>
		public List<Path> ItemsToShowInCanvas
		{
			get
			{
				if (shapes.Count > 0)
				{
					var biggest = shapes.MaxBy(s => s.Data.Bounds.Width);
					var tl = biggest.Data.Bounds.TopLeft;
					BundleDiameter = biggest.Data.Bounds.Width / Scale;
					var vec = new Point() - tl;
					foreach (var s in shapes)
					{
						// shift the circles in scene 2D
						s.RenderTransform = new TranslateTransform(vec.X, vec.Y);
					}
				}
				return shapes;
			}
			set
			{
				if (value != null)
				{
					shapes = value;
					OnPropertyChange(nameof(ItemsToShowInCanvas));
				}
			}
		}

		/// <summary>
		/// ICommand connected to UI to perform calculation
		/// </summary>
		public ICommand UpdateCommand
		{
			get
			{
				if (updater == null)
				{
					updater = new Updater(param => CanExecute(), param => UpdateCircles());
				}
				return updater;
			}
			set
			{
				updater = value;
			}
		}

		/// <summary>
		/// Accept only valid input of wires
		/// </summary>
		/// <returns>true if cablee radii are in valid format</returns>
		private bool CanExecute()
		{
			var cablesS = CablesDiameters.Split(' ');

			// scale these small cables
			cablesD = cablesS.Select(x => double.TryParse(x, NumberStyles.Any, CultureInfo.InvariantCulture, out var value) ?
			value * Scale : double.NaN).ToArray();
			return cablesD.All(c => !double.IsNaN(c));
		}

		/// <summary>
		/// App basic status information
		/// </summary>
		public string Status
		{
			get
			{
				return status;
			}
			set
			{
				if (status != value)
				{
					status = value;
					OnPropertyChange(nameof(Status));
				}
			}
		}

		/// <summary>
		/// ICommand execution flow - event from UI
		/// </summary>
		private async void UpdateCircles()
		{
			ItemsToShowInCanvas = new List<Path>();
			Status = "Calculating ...";

			// execute the bundle calc in different thread not blocking the main UI thread
			await Task.Run(() =>
			{
				// find possible permutation between the circles (cables)
				var permutations = cablesD.GetPermutationsC().ToArray();

				var minShape = model.CalculateBundles(permutations);
				model.WireBundle.Circles = minShape?.Result;
			});

			ItemsToShowInCanvas = model.GetCirclesAsPaths();
			Status = string.Empty;
		}

		/// <summary>
		/// ICommand implementation for execute, can execute
		/// </summary>
		private class Updater : ICommand
		{
			private readonly Predicate<object> _canExecute;
			private readonly Action<object> _execute;

			public Updater(Predicate<object> canExecute, Action<object> execute)
			{
				_canExecute = canExecute;
				_execute = execute;
			}


			public bool CanExecute(object parameter)
			{
				return _canExecute(parameter);
			}

			public event EventHandler CanExecuteChanged
			{
				add => CommandManager.RequerySuggested += value;
				remove => CommandManager.RequerySuggested -= value;
			}

			public void Execute(object parameter)
			{
				_execute(parameter);
			}
		}

		public event PropertyChangedEventHandler PropertyChanged;

		public void OnPropertyChange(string propertyName)
		{
			if (PropertyChanged != null)
			{
				PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
			}
		}
	}
}
