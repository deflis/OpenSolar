using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Ignition;
using Ignition.Presentation;
using Lunar;
using System.Windows.Threading;

namespace Solar.Dialogs
{
#pragma warning disable 1591

	/// <summary>
	/// AuthenticateWindow.xaml の相互作用ロジック
	/// </summary>
	public partial class AuthenticateWindow : Window
	{
		AuthenticateWindowViewModel ViewModel
		{
			get
			{
				return (AuthenticateWindowViewModel)this.DataContext;
			}
		}

		public AuthenticateWindow(OAuthAuthorization authorization)
		{
			InitializeComponent();

			if (authorization.Token != null)
				this.Title += ": " + authorization.Token.Name;

			this.ViewModel.Authorization = authorization;
			this.ViewModel.RequestClose += (sender, e) => this.DialogResult = e.Value;
			this.ViewModel.RequestError += (sender, e) => Application.Current.Dispatcher.BeginInvoke((Action)(() => MessageBoxEx.Show(this, e.Value.Message, this.Title, MessageBoxButton.OK, MessageBoxImage.Exclamation)), DispatcherPriority.Background);
		}
	}

	public class AuthenticateWindowViewModel : NotifyObject
	{
		public event EventHandler<EventArgs<bool?>> RequestClose;
		public event EventHandler<EventArgs<Exception>> RequestError;

		#region Commands

		public ICommand OpenAuthorizationCommand
		{
			get
			{
				return new RelayCommand(_ => this.CanInput, _ => Task.Factory.StartNew(() =>
				{
					try
					{
						this.IsLoading = true;
						Process.Start(this.Authorization.GetAuthorizationUri().AbsoluteUri);
					}
					catch (Exception ex)
					{
						RequestError.DispatchEvent(this, new EventArgs<Exception>(App.Log(ex)));
					}
					finally
					{
						this.IsLoading = false;
					}
				}));
			}
		}

		public ICommand AuthenticateCommand
		{
			get
			{
				return new RelayCommand(_ => this.CanAuthenticate && this.CanInput, _ =>
				{
					Task.Factory.StartNew(() =>
					{
						try
						{
							this.IsLoading = true;
							this.Authorization.Authenticate(this.PIN);
							RequestClose.DispatchEvent(this, new EventArgs<bool?>(true));
						}
						catch (Exception ex)
						{
							RequestError.DispatchEvent(this, new EventArgs<Exception>(App.Log(ex)));
						}
						finally
						{
							this.IsLoading = false;
						}
					});
				});
			}
		}

		public ICommand CancelCommand
		{
			get
			{
				return new RelayCommand(_ => RequestClose.RaiseEvent(this, new EventArgs<bool?>(false)));
			}
		}

		#endregion

		public OAuthAuthorization Authorization
		{
			get;
			set;
		}

		public bool CanInput
		{
			get
			{
				return GetValue(() => this.CanInput);
			}
			private set
			{
				SetValue(() => this.CanInput, value);
			}
		}

		public bool IsLoading
		{
			get
			{
				return GetValue(() => this.IsLoading);
			}
			private set
			{
				SetValue(() => this.IsLoading, value);
				this.CanInput = !value;
			}
		}

		public bool CanAuthenticate
		{
			get
			{
				return GetValue(() => this.CanAuthenticate);
			}
			private set
			{
				SetValue(() => this.CanAuthenticate, value);
			}
		}

		public string PIN
		{
			get
			{
				return GetValue(() => this.PIN);
			}
			set
			{
				SetValue(() => this.PIN, value);
				this.CanAuthenticate = !string.IsNullOrEmpty(this.PIN) && this.PIN.Length == 7 && this.PIN.All(char.IsDigit);
			}
		}

		public AuthenticateWindowViewModel()
		{
			this.IsLoading = false;
		}
	}

#pragma warning restore 1591
}
