using System;
using System.Windows;
using System.Windows.Input;
using Ignition;
using Solar.Models;

namespace Solar.Dialogs
{
#pragma warning disable 1591

	/// <summary>
	/// FootersWindow.xaml の相互作用ロジック
	/// </summary>
	public partial class FootersWindow : Window
	{
		FootersWindowViewModel ViewModel
		{
			get
			{
				return (FootersWindowViewModel)this.DataContext;
			}
		}

		public FootersWindow()
		{
			InitializeComponent();

			this.ViewModel.RequestEdit += (sender, e) => listBox.ScrollIntoView(listBox.SelectedItem = e.Value);
			this.ViewModel.RequestClose += (sender, e) => this.Close();
		}
	}

	public class FootersWindowViewModel : NotifyObject
	{
		public event EventHandler<EventArgs<Footer>> RequestEdit;
		public event EventHandler RequestClose;

		#region Commands

		public ICommand AddCommand
		{
			get
			{
				return new RelayCommand(_ =>
				{
					var item = new Footer();

					Settings.Default.Post.Footers.Add(item);
					RequestEdit.RaiseEvent(this, new EventArgs<Footer>(item));
				});
			}
		}

		public ICommand RemoveCommand
		{
			get
			{
				return new RelayCommand<Footer>(_ => _ != null, _ => Settings.Default.Post.Footers.Remove(_));
			}
		}

		public ICommand CloseCommand
		{
			get
			{
				return new RelayCommand(_ => RequestClose.RaiseEvent(this, EventArgs.Empty));
			}
		}

		#endregion
	}

#pragma warning restore 1591
}
