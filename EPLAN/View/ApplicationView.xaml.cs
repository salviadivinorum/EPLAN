using EPLAN.ViewModel;
using System.Windows;
namespace EPLAN.View
{
	/// <summary>
	/// Interaction logic for ApplicationView.xaml
	/// </summary>
	public partial class ApplicationView : Window
	{
		private readonly AppMainVM _viewModel;

		public ApplicationView()
		{
			InitializeComponent();
			_viewModel = new AppMainVM();
			DataContext = _viewModel;
		}
	}
}
