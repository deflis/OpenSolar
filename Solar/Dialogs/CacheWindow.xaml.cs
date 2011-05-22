using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Ignition;
using Lunar;
using Solar.Models;

namespace Solar.Dialogs
{
#pragma warning disable 1591

	/// <summary>
	/// CacheWindow.xaml の相互作用ロジック
	/// </summary>
	public partial class CacheWindow : Window
	{
		CacheWindowViewModel ViewModel
		{
			get
			{
				return (CacheWindowViewModel)this.DataContext;
			}
		}

		public CacheWindow(StatusCache statusCache, Action clearStatuses)
		{
			InitializeComponent();
			this.ViewModel.Apply(statusCache, clearStatuses);
		}

		void Button_Click(object sender, RoutedEventArgs e)
		{
			if (this.ViewModel.Cleared)
				this.DialogResult = true;
			else
				this.DialogResult = false;
		}
	}

	public class CacheWindowViewModel : NotifyObject
	{
		StatusCache statusCache;
		Action clearStatuses;

		public bool Cleared
		{
			get;
			private set;
		}

		public bool IsClearing
		{
			get
			{
				return GetValue(() => this.IsClearing);
			}
			private set
			{
				SetValue(() => this.IsClearing, value);
			}
		}

		public int StatusCount
		{
			get
			{
				return GetValue(() => this.StatusCount);
			}
			private set
			{
				SetValue(() => this.StatusCount, value);
			}
		}

		public int UserCount
		{
			get
			{
				return GetValue(() => this.UserCount);
			}
			private set
			{
				SetValue(() => this.UserCount, value);
			}
		}

		public int ImageCount
		{
			get
			{
				return GetValue(() => this.ImageCount);
			}
			private set
			{
				SetValue(() => this.ImageCount, value);
			}
		}

		public double TotalSize
		{
			get
			{
				return GetValue(() => this.TotalSize);
			}
			private set
			{
				SetValue(() => this.TotalSize, value);
			}
		}

		public ICommand ClearCommand
		{
			get
			{
				return new RelayCommand(_ => Task.Factory.StartNew(() =>
				{
					this.IsClearing = true;

					clearStatuses();
					statusCache.Clear();
					UriBitmapConverter.Clear();

					GC.Collect();
					GC.WaitForPendingFinalizers();
					Apply(statusCache, clearStatuses);
					Client.Instance.OnClearCache();
					GC.Collect();

					this.IsClearing = false;
					this.Cleared = true;
				}));
			}
		}

		public ICommand CleanCommand
		{
			get
			{
				return new RelayCommand(_ => Task.Factory.StartNew(() =>
				{
					this.IsClearing = true;

					statusCache.Clean();
					UriBitmapConverter.Clean();

					GC.Collect();
					GC.WaitForPendingFinalizers();
					Apply(statusCache, clearStatuses);
					Client.Instance.OnClearCache();
					GC.Collect();

					this.IsClearing = false;
					this.Cleared = true;
				}));
			}
		}

		public void Apply(StatusCache statusCache, Action clearStatuses)
		{
			this.statusCache = statusCache;
			this.StatusCount = statusCache.StatusCount;
			this.UserCount = statusCache.UserCount;
			this.ImageCount = UriBitmapConverter.CacheCount;
			this.TotalSize = GC.GetTotalMemory(false) / 1024.0;
			this.clearStatuses = clearStatuses;
		}
	}

#pragma warning restore 1591
}
