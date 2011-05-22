using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media.Animation;
using Ignition;
using Ignition.Presentation;
using Lunar;
using Solar.Models;

namespace Solar
{
#pragma warning disable 1591

	/// <summary>
	/// NotifyWindow.xaml の相互作用ロジック
	/// </summary>
	public partial class NotifyWindow : Window
	{
		Window owner;

		[SuppressUnmanagedCodeSecurity, DllImport("user32", EntryPoint = "GetWindowLong")]
		static extern IntPtr GetWindowLong32(IntPtr hWnd, int nIndex);
		[SuppressUnmanagedCodeSecurity, DllImport("user32", EntryPoint = "GetWindowLongPtr")]
		static extern IntPtr GetWindowLong64(IntPtr hWnd, int nIndex);
		[SuppressUnmanagedCodeSecurity, DllImport("user32", EntryPoint = "SetWindowLong")]
		static extern IntPtr SetWindowLong32(IntPtr hWnd, int nIndex, int dwNewLong);
		[SuppressUnmanagedCodeSecurity, DllImport("user32", EntryPoint = "SetWindowLongPtr")]
		static extern IntPtr SetWindowLong64(IntPtr hWnd, int nIndex, int dwNewLong);

		static IntPtr GetWindowLong(IntPtr hWnd, int nIndex)
		{
			if (IntPtr.Size == 8)
				return GetWindowLong64(hWnd, nIndex);
			else
				return GetWindowLong32(hWnd, nIndex);
		}

		static IntPtr SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong)
		{
			if (IntPtr.Size == 8)
				return SetWindowLong64(hWnd, nIndex, dwNewLong);
			else
				return SetWindowLong32(hWnd, nIndex, dwNewLong);
		}

		NotifyWindowViewModel ViewModel
		{
			get
			{
				return (NotifyWindowViewModel)this.DataContext;
			}
		}

		public IList<IEntry> Statuses
		{
			get
			{
				return this.ViewModel.Statuses;
			}
		}

		public NotifyWindow(Window owner, IList<IEntry> newStatuses)
		{
			InitializeComponent();
			this.owner = owner;
			this.ViewModel.Apply(newStatuses);
		}

		void Window_Loaded(object sender, RoutedEventArgs e)
		{
			var scr = ScreenHelper.GetWorkingAreaFromWindow(this);
			var hlp = new WindowInteropHelper(this);
			var isBottom = Settings.Default.Interface.NotifyLocation == NotifyLocation.BottomRight || Settings.Default.Interface.NotifyLocation == NotifyLocation.BottomLeft;
			var top = isBottom
				? scr.Y + scr.Height - App.Current.Windows.OfType<NotifyWindow>()
														  .Sum(_ => _.Height)
				: App.Current.Windows.OfType<NotifyWindow>()
									 .Sum(_ => _.Height) - this.Height + 8;

			top = top + App.Current.Windows.OfType<NotifyWindow>()
										   .Where(_ => _ != this && _.Top == top)
										   .Sum(_ => _.Height * (isBottom ? 1 : -1));

			this.Left = Settings.Default.Interface.NotifyLocation == NotifyLocation.TopLeft || Settings.Default.Interface.NotifyLocation == NotifyLocation.BottomLeft ? 8 : scr.X + scr.Width - this.Width;
			this.Top = top;

			SetWindowLong(hlp.Handle, -20, GetWindowLong(hlp.Handle, -20).ToInt32() | 0x80);	// EX_STYLE |= WS_EX_TOOLWINDOW
		}

		void Loaded_Completed(object sender, EventArgs e)
		{
			var view = CollectionViewSource.GetDefaultView(this.ViewModel.Statuses);

			if (view.CurrentPosition < 4 &&
				view.CurrentPosition < this.ViewModel.Statuses.Count - 1 &&
				view.MoveCurrentToNext())
				((Storyboard)this.Resources["Wait"]).Begin(this);
			else
				((Storyboard)this.Resources["Unloading"]).Begin(this);
		}

		void Unloading_Completed(object sender, EventArgs e)
		{
			this.Close();
		}

		void Grid_MouseDown(object sender, MouseButtonEventArgs e)
		{
			if (owner.WindowState == WindowState.Minimized)
				owner.WindowState = WindowState.Normal;

			((MainWindow)owner).Wakeup();
		}
	}

	public class NotifyWindowViewModel : NotifyObject
	{
		public IList<IEntry> Statuses
		{
			get
			{
				return GetValue(() => this.Statuses);
			}
			set
			{
				SetValue(() => this.Statuses, value);
			}
		}

		public void Apply(IList<IEntry> newStatuses)
		{
			this.Statuses = newStatuses;
		}
	}

#pragma warning restore 1591
}
