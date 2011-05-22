using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Ignition;
using Ignition.Presentation;
using Lunar;

namespace Solar.Dialogs
{
#pragma warning disable 1591

	/// <summary>
	/// AccountsWindow.xaml の相互作用ロジック
	/// </summary>
	public partial class AccountsWindow : Window
	{
		AccountsWindowViewModel ViewModel
		{
			get
			{
				return (AccountsWindowViewModel)this.DataContext;
			}
		}

		public AccountsWindow(ICollection<AccountToken> accounts)
		{
			InitializeComponent();

			this.ViewModel.RequestShowDialog += (sender, e) => e.ShowDialog(this);
			this.ViewModel.RequestClose += (sender, e) => this.Close();
			this.ViewModel.Apply(accounts);
		}

		void Window_Closed(object sender, EventArgs e)
		{
			this.ViewModel.Closed();
		}
	}

	public class AccountsWindowViewModel : NotifyObject
	{
		internal event EventHandler<ShowDialogEventArgs> RequestShowDialog;
		public event EventHandler RequestClose;

		ICollection<AccountToken> accounts;

		#region Commands

		public ICommand AddCommand
		{
			get
			{
				return new RelayCommand(_ =>
				{
					using (var auth = new OAuthAuthorization())
					{
						var e = new ShowDialogEventArgs(new AuthenticateWindow(auth));

						RequestShowDialog.RaiseEvent(this, e);

						if (e.DialogResult == true)
						{
							if (this.Accounts.Contains(auth.Token))
								this.Accounts.Remove(auth.Token);

							this.Accounts.Add(auth.Token);

							this.SelectedAccount = auth.Token;
						}
					}
				});
			}
		}

		public ICommand RemoveCommand
		{
			get
			{
				return new RelayCommand(_ => this.SelectedAccount != null, _ => this.Accounts.Remove(this.SelectedAccount));
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

		public AccountToken SelectedAccount
		{
			get
			{
				return GetValue(() => this.SelectedAccount);
			}
			set
			{
				SetValue(() => this.SelectedAccount, value);
			}
		}

		public NotifyCollection<AccountToken> Accounts
		{
			get;
			private set;
		}

		public AccountsWindowViewModel()
		{
			this.Accounts = new NotifyCollection<AccountToken>();
		}

		public void Apply(ICollection<AccountToken> accounts)
		{
			this.accounts = accounts;
			this.Accounts.AddRange(accounts);
		}

		public void Closed()
		{
			foreach (var i in accounts.Except(this.Accounts))
				accounts.Remove(i);

			foreach (var i in this.Accounts.Except(accounts))
				accounts.Add(i);
		}
	}

#pragma warning restore 1591
}
