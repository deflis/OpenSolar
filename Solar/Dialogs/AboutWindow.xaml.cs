using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using Ignition;

namespace Solar.Dialogs
{
#pragma warning disable 1591

	/// <summary>
	/// AboutWindow.xaml の相互作用ロジック
	/// </summary>
	public partial class AboutWindow : Window
	{
		public AboutWindow()
		{
			InitializeComponent();
		}

		void Button_Click(object sender, RoutedEventArgs e)
		{
			this.Close();
		}
	}

	public class AboutWindowViewModel : NotifyObject
	{
		#region Commands

		public ICommand OpenUriCommand
		{
			get
			{
				return new RelayCommand<string>(_ => Process.Start(_));
			}
		}

		#endregion
	}

#pragma warning restore 1591
}
