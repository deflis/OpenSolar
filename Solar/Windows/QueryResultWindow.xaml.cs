using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Ignition;
using Ignition.Presentation;
using Ignition.Linq;
using Lunar;
using Solar.Filtering;
using Solar.Models;

namespace Solar
{
#pragma warning disable 1591

	/// <summary>
	/// QueryResultWindow.xaml の相互作用ロジック
	/// </summary>
	public partial class QueryResultWindow : Window
	{
		readonly string title;

		QueryResultWindowViewModel ViewModel
		{
			get
			{
				return (QueryResultWindowViewModel)this.DataContext;
			}
		}

		/// <summary>
		/// 表示しているカテゴリを取得します。
		/// </summary>
		public Category Category
		{
			get
			{
				return this.ViewModel.Category;
			}
		}

		QueryResultWindow(string title, ClientParameters cp)
		{
			InitializeComponent();

			this.ViewModel.RequestShow += (sender, e) => e.Show(this.Owner);
			this.ViewModel.RequestMessageBox += (sender, e) => e.Value(this);
			this.ViewModel.RequestClose += (sender, e) => this.Close();
			this.ViewModel.RequestUpdateCount += (sender, e) => this.Dispatcher.Invoke((Action)(() => this.Title = this.title + " " + this.ViewModel.Statuses.Count + " 件"));
			this.Title = this.title = title + " (" + cp.Accounts.Join(", ") + ")";
		}

		internal QueryResultWindow(string title, ClientParameters cp, User user, FilterSource source)
			: this(title, cp)
		{
			this.ViewModel.Apply(cp, user, source);
		}

		internal QueryResultWindow(ClientParameters cp, User user)
			: this(user.Name + " について", cp)
		{
			this.ViewModel.Apply(cp, user, new UserFilterSource
			{
				UserName = user.Name,
				UserID = user.UserID,
			});
		}

		internal QueryResultWindow(ClientParameters cp, string user)
			: this(user + " について", cp)
		{
			this.ViewModel.Apply(cp, user, new UserFilterSource
			{
				UserName = user,
			});
		}

		void Window_SizeChanged(object sender, SizeChangedEventArgs e)
		{
			if (this.SizeToContent != SizeToContent.Manual)
			{
				var sz = e.NewSize;

				if (sz.Width > 640)
					sz.Width = 640;

				if (sz.Height > 640)
					sz.Height = 640;

				if (sz != e.NewSize)
				{
					e.Handled = true;

					if (sz.Width < 640)
						this.SizeToContent = SizeToContent.Width;
					else if (sz.Height < 640)
						this.SizeToContent = SizeToContent.Height;
					else
						this.SizeToContent = SizeToContent.Manual;

					this.ResizeInScreen(this.Left + this.Width / 2 - sz.Width / 2, this.Top + this.Height / 2 - sz.Height / 2, sz.Width, sz.Height);
				}

				if (!this.ViewModel.IsLoading)
					this.ViewModel.MaxUserDescriptionWidth = double.PositiveInfinity;
			}
		}

		void TextBox_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.Key == Key.Enter && e.KeyboardDevice.Modifiers == ModifierKeys.None)
			{
				((TextBox)sender).GetBindingExpression(TextBox.TextProperty).UpdateSource();
				this.ViewModel.Update();
				e.Handled = true;
			}
		}

		void TextBox_Loaded(object sender, RoutedEventArgs e)
		{
			((TextBox)sender).Focus();
		}

		void statusesListBox_RequestNewPage(object sender, RoutedEventArgs e)
		{
			this.ViewModel.RequestNewPage();
		}

		protected override void OnClosed(EventArgs e)
		{
			this.ViewModel.Close();
			base.OnClosed(e);
		}
	}

	public class QueryResultWindowViewModel : ViewModel<Client>
	{
		bool canRequestNewPage;
		ClientParameters cp;
		LinkedList<IDisposable> handlers = new LinkedList<IDisposable>();
		string setUserName;

		internal event EventHandler<ShowDialogEventArgs> RequestShow;
		public event EventHandler<EventArgs<Action<Window>>> RequestMessageBox;
		public event EventHandler RequestClose;
		public event EventHandler RequestUpdateCount;

		public double MaxUserDescriptionWidth
		{
			get
			{
				return GetValue(() => this.MaxUserDescriptionWidth);
			}
			set
			{
				SetValue(() => this.MaxUserDescriptionWidth, value);
			}
		}

		public Category Category
		{
			get
			{
				return GetValue(() => this.Category);
			}
			private set
			{
				SetValue(() => this.Category, value);
				OnPropertyChanged("Statuses");
				OnPropertyChanged("IsUser");
				OnPropertyChanged("Source");
				OnPropertyChanged("IsSearch");
				OnPropertyChanged("IsNotSearch");
				OnPropertyChanged("IsSavableOrSearch");
				OnPropertyChanged("IsSearchAndEmpty");
			}
		}

		public FilterSource Source
		{
			get
			{
				if (this.Category == null)
					return null;

				return this.Category.Filter.Sources.SingleOrDefault();
			}
		}

		public IList<IEntry> Statuses
		{
			get
			{
				if (this.Category == null)
					return null;

				return this.Category.Statuses;
			}
		}

		public bool IsSearch
		{
			get
			{
				return this.Source is SearchFilterSource
					|| this.Source is SearchUsersFilterSource
					|| this.Source is SearchCacheFilterSource
					|| this.Source is QueryCacheFilterSource;
			}
		}

		public bool IsNotSearch
		{
			get
			{
				return !this.IsSearch;
			}
		}

		public bool IsSearchAndEmpty
		{
			get
			{
				return this.Source is SearchFilterSource && string.IsNullOrEmpty(((SearchFilterSource)this.Source).Query)
					|| this.Source is SearchUsersFilterSource && string.IsNullOrEmpty(((SearchUsersFilterSource)this.Source).Query)
					|| this.Source is SearchCacheFilterSource && string.IsNullOrEmpty(((SearchCacheFilterSource)this.Source).Query)
					|| this.Source is QueryCacheFilterSource && string.IsNullOrEmpty(((QueryCacheFilterSource)this.Source).Query);
			}
		}

		public bool IsUser
		{
			get
			{
				return this.Source is UserFilterSource;
			}
		}

		#region Commands

		public ICommand CloseCommand
		{
			get
			{
				return new RelayCommand(_ => RequestClose.RaiseEvent(this, EventArgs.Empty));
			}
		}

		public ICommand AddToTabCommand
		{
			get
			{
				return new RelayCommand(p =>
				{
					this.Category.Name = this.Source.TypeMatch
					(
						(UserFilterSource _) => "@" + _.UserName,
						(ListFilterSource _) => _.Path,
						(SearchFilterSource _) => _.Query
					);
					cp.AddCategory(this.Category.Clone());
				});
			}
		}

		public ICommand RefreshCommand
		{
			get
			{
				return new RelayCommand(_ => this.Update());
			}
		}

		public ICommand UserDetailsCommand
		{
			get
			{
				return new RelayCommand(_ => Process.Start((Settings.Default.Connection.UseHttps ? "https" : "http") + "://twitter.com/" + this.User.Name));
			}
		}
		public ICommand ShowWebSiteCommand
		{
			get
			{
				return new RelayCommand(_ => Process.Start(this.User.WebSite.AbsoluteUri));
			}
		}

		public ICommand ShowListsCommand
		{
			get
			{
				return new RelayCommand(_ => RequestShow.RaiseEvent(this, new ShowDialogEventArgs(new QueryResultWindow
				(
					this.User.Name + " によるリスト",
					cp,
					this.User,
					new ListIndexFilterSource
					{
						UserName = this.User.Name,
					}
				))));
			}
		}

		public ICommand ShowSubscriptionsCommand
		{
			get
			{
				return new RelayCommand(_ => RequestShow.RaiseEvent(this, new ShowDialogEventArgs(new QueryResultWindow
				(
					this.User.Name + " がフォローしているリスト",
					cp,
					this.User,
					new ListIndexFilterSource
					{
						UserName = this.User.Name,
						Mode = ListIndexFilterSource.IndexMode.Subscriptions,
					}
				))));
			}
		}

		public ICommand ShowMembershipsCommand
		{
			get
			{
				return new RelayCommand(_ => RequestShow.RaiseEvent(this, new ShowDialogEventArgs(new QueryResultWindow
				(
					this.User.Name + " が登録されているリスト",
					cp,
					this.User,
					new ListIndexFilterSource
					{
						UserName = this.User.Name,
						Mode = ListIndexFilterSource.IndexMode.Memberships,
					}
				))));
			}
		}

		public ICommand GetFollowingCommand
		{
			get
			{
				return new RelayCommand(_ => RequestShow.RaiseEvent(this, new ShowDialogEventArgs(new QueryResultWindow
				(
					this.User.Name + " のフォロー",
					cp,
					this.User,
					new FollowingFilterSource
					{
						UserName = this.User.Name,
					}
				))));
			}
		}

		public ICommand GetFollowersCommand
		{
			get
			{
				return new RelayCommand(_ => RequestShow.RaiseEvent(this, new ShowDialogEventArgs(new QueryResultWindow
				(
					this.User.Name + " のフォロワー",
					cp,
					this.User,
					new FollowersFilterSource
					{
						UserName = this.User.Name,
					}
				))));
			}
		}

		public ICommand GetFavoritesCommand
		{
			get
			{
				return new RelayCommand(_ => RequestShow.RaiseEvent(this, new ShowDialogEventArgs(new QueryResultWindow
				(
					this.User.Name + " のお気に入り",
					cp,
					this.User,
					new FavoritesFilterSource
					{
						UserName = this.User.Name
					}
				))));
			}
		}

		public ICommand FollowCommand
		{
			get
			{
				return new RelayCommand(_ => Task.Factory.StartNew(() =>
				{
					try
					{
						using (new TwitterClient(cp.Accounts.First(), cp.StatusCache))
							if (this.IsUserFollowing)
								this.User.Follow();
							else
								this.User.Unfollow();
					}
					catch (ContentedWebException ex)
					{
						RequestMessageBox.DispatchEvent(this, new EventArgs<Action<Window>>(window => MessageBoxEx.Show(window, ex.Message, "送信中にエラーが発生しました", MessageBoxButton.OK, MessageBoxImage.Warning)));
					}
				}));
			}
		}

		public ICommand BlockCommand
		{
			get
			{
				return new RelayCommand(_ =>
				{
					if (this.IsUserBlocking)
					{
						bool rt = false;

						RequestMessageBox.RaiseEvent(this, new EventArgs<Action<Window>>(window => rt = MessageBoxEx.Show(window, "このユーザをブロックしますか？", "確認", MessageBoxButton.YesNo, MessageBoxImage.Information) == MessageBoxResult.Yes));

						if (!rt)
							return;
					}

					Task.Factory.StartNew(() =>
					{
						try
						{
							using (var client = new TwitterClient(cp.Accounts.First(), cp.StatusCache))
								if (this.IsUserBlocking)
									client.Blocks.Create(this.User.UserID);
								else
									client.Blocks.Destroy(this.User.UserID);
						}
						catch (ContentedWebException ex)
						{
							RequestMessageBox.DispatchEvent(this, new EventArgs<Action<Window>>(window => MessageBoxEx.Show(window, ex.Message, "送信中にエラーが発生しました", MessageBoxButton.OK, MessageBoxImage.Warning)));
						}
					});
				});
			}
		}

		public ICommand ReportForSpamCommand
		{
			get
			{
				return new RelayCommand(_ =>
				{
					bool rt = false;

					RequestMessageBox.RaiseEvent(this, new EventArgs<Action<Window>>(window => rt = MessageBoxEx.Show(window, "このユーザをスパムとして報告しますか？", "確認", MessageBoxButton.YesNo, MessageBoxImage.Information) == MessageBoxResult.Yes));

					if (!rt)
						return;

					Task.Factory.StartNew(() =>
					{
						try
						{
							using (var client = new TwitterClient(cp.Accounts.First(), cp.StatusCache))
								client.ReportSpam(this.User.UserID);
						}
						catch (ContentedWebException ex)
						{
							RequestMessageBox.DispatchEvent(this, new EventArgs<Action<Window>>(window => MessageBoxEx.Show(window, ex.Message, "送信中にエラーが発生しました", MessageBoxButton.OK, MessageBoxImage.Warning)));
						}
					});
				});
			}
		}

		#endregion

		public bool HasUserLocation
		{
			get
			{
				return GetValue(() => this.HasUserLocation);
			}
			private set
			{
				SetValue(() => this.HasUserLocation, value);
			}
		}

		public bool HasUserWebSite
		{
			get
			{
				return GetValue(() => this.HasUserWebSite);
			}
			private set
			{
				SetValue(() => this.HasUserWebSite, value);
			}
		}

		public bool HasUserDescription
		{
			get
			{
				return GetValue(() => this.HasUserDescription);
			}
			private set
			{
				SetValue(() => this.HasUserDescription, value);
			}
		}

		public bool IsLoadingUser
		{
			get
			{
				return GetValue(() => this.IsLoadingUser);
			}
			private set
			{
				SetValue(() => this.IsLoadingUser, value);
			}
		}

		public bool IsUserFollowing
		{
			get
			{
				return GetValue(() => this.IsUserFollowing);
			}
			set
			{
				SetValue(() => this.IsUserFollowing, value);
			}
		}

		public bool IsUserBlocking
		{
			get
			{
				return GetValue(() => this.IsUserBlocking);
			}
			set
			{
				SetValue(() => this.IsUserBlocking, value);
			}
		}

		public string UserDescription
		{
			get
			{
				return GetValue(() => this.UserDescription);
			}
			private set
			{
				SetValue(() => this.UserDescription, value);
			}
		}

		public StatusesListBoxCommandHandler StatusesListBoxCommandHandler
		{
			get
			{
				return GetValue(() => this.StatusesListBoxCommandHandler);
			}
			private set
			{
				SetValue(() => this.StatusesListBoxCommandHandler, value);
			}
		}

		public bool HasErrorText
		{
			get
			{
				return !string.IsNullOrEmpty(this.ErrorText);
			}
		}

		public string ErrorText
		{
			get
			{
				return GetValue(() => this.ErrorText);
			}
			private set
			{
				SetValue(() => this.ErrorText, value);
				OnPropertyChanged("HasErrorText");
			}
		}

		public bool HasUserErrorText
		{
			get
			{
				return !string.IsNullOrEmpty(this.UserErrorText);
			}
		}

		public string UserErrorText
		{
			get
			{
				return GetValue(() => this.UserErrorText);
			}
			private set
			{
				SetValue(() => this.UserErrorText, value);
				OnPropertyChanged("HasUserErrorText");
			}
		}

		public bool IsSavable
		{
			get
			{
				return GetValue(() => this.IsSavable);
			}
			private set
			{
				SetValue(() => this.IsSavable, value);
				OnPropertyChanged("IsSavableOrSearch");
			}
		}

		public bool IsSavableOrSearch
		{
			get
			{
				return this.IsSavable
					|| this.IsSearch;
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
			}
		}

		public User User
		{
			get
			{
				return GetValue(() => this.User);
			}
			private set
			{
				SetValue(() => this.User, value);
			}
		}

		public QueryResultWindowViewModel()
			: base(Client.Instance)
		{
			this.IsSavable = true;
			this.MaxUserDescriptionWidth = 320;
			handlers.AddLast(DisposableEventHandler.Create<EventArgs<Category>>(_ => this.Model.Refreshing += _, _ => this.Model.Refreshing -= _, (sender, e) =>
			{
				if (e.Value == this.Category)
					this.IsLoading = true;
			}));
			handlers.AddLast(DisposableEventHandler.Create<EventArgs<Category, IList<IEntry>>>(_ => this.Model.Refreshed += _, _ => this.Model.Refreshed -= _, (sender, e) =>
			{
				if (e.Value1 == this.Category)
					this.IsLoading = false;
			}));
			handlers.AddLast(DisposableEventHandler.Create<EventArgs<Category>>(_ => this.Model.RequestingNewPage += _, _ => this.Model.RequestingNewPage -= _, (sender, e) =>
			{
				if (e.Value == this.Category)
					this.IsLoading = true;
			}));
			handlers.AddLast(DisposableEventHandler.Create<EventArgs<Category>>(_ => this.Model.RequestedNewPage += _, _ => this.Model.RequestedNewPage -= _, (sender, e) =>
			{
				if (e.Value == this.Category)
					this.IsLoading = false;
			}));
			handlers.AddLast(DisposableEventHandler.Create<EventArgs<Category, Exception>>(_ => this.Model.ThrowWarning += _, _ => this.Model.ThrowWarning -= _, (sender, e) =>
			{
				if (e.Value1 == this.Category)
					if (!this.Statuses.Any())
						this.ErrorText = e.Value2.Message;
					else
						MessageBoxEx.Show(Application.Current.MainWindow, e.Value2.Message, e.Value2.GetType().FullName, MessageBoxButton.OK, MessageBoxImage.Warning);
			}));
		}

		internal void Apply(ClientParameters cp, string userName, FilterSource source)
		{
			setUserName = userName;
			Apply(cp, (User)null, source);
		}

		internal void Apply(ClientParameters cp, User user, FilterSource source)
		{
			this.cp = cp;
			this.User = user;
			this.StatusesListBoxCommandHandler = cp.StatusesListBoxCommandHandler;
			this.Category = source == null ? new Category() : new Category(null, source);
			this.IsSavable = source != null && source.Serializable;
			canRequestNewPage = source != null && source.Pagable;

			if (source != null)
				source.Account = cp.Accounts.Count() > 1 ? 0 : cp.Accounts.Single().UserID;

			Update();
		}

		public void Close()
		{
			handlers.Run(_ => _.Dispose());
			handlers.Clear();
			this.Category.RemoveFromInstanceList();
		}

		public void Update()
		{
			OnPropertyChanged("IsSearchAndEmpty");
			Update(new StatusRange());
		}

		public void Update(StatusRange range)
		{
			if (this.IsUser)
				Task.Factory.StartNew(() =>
				{
					this.UserErrorText = null;

					try
					{
						using (new ReduceAuthenticatedQueryScope())
						using (var client = new TwitterClient(cp.Accounts.First(), cp.StatusCache))
						{
							this.IsLoadingUser = true;

							if (this.User == null)
								this.User = client.Users.Get(setUserName);

							((UserFilterSource)this.Source).UserID = this.User.UserID;
							this.HasUserLocation = !string.IsNullOrEmpty(this.User.Location);
							this.HasUserWebSite = this.User.WebSite != null;
							this.HasUserDescription = !string.IsNullOrEmpty(this.User.Description);
							this.UserDescription = this.HasUserDescription ? this.User.Description.Replace("\r", "...").TakeWhile(_ => _ != '\n').Join("") : null;
							this.IsUserFollowing = client.Friendships.Get(cp.Accounts.First().Name, this.User.Name).SourceFollowingTarget;
							this.IsUserBlocking = client.Blocks.Exists(this.User.Name);

							this.IsLoadingUser = false;
						}
					}
					catch (Exception ex)
					{
						this.UserErrorText = App.Log(ex).Message;
					}
				});

			this.ErrorText = null;
			this.Statuses.Clear();
			this.Model.Update(this.Category).ContinueWith(_ => RequestUpdateCount.RaiseEvent(this, EventArgs.Empty));
			canRequestNewPage = this.Source != null && this.Source.Pagable;
		}

		public void RequestNewPage()
		{
			if (canRequestNewPage && !this.Category.IsRequestingNewPage)
			{
				var count = this.Category.Statuses.Count;

				this.Model.RequestNewPage(this.Category).ContinueWith(_ =>
				{
					if (_.Result)
						canRequestNewPage = this.Category.Statuses.Count > count;

					RequestUpdateCount.RaiseEvent(this, EventArgs.Empty);
				});
			}
		}
	}

	class MessageIfNullConverter : OneWayValueConverter<object, string>
	{
		public string NullMessage
		{
			get;
			set;
		}

		public string NonNullMessage
		{
			get;
			set;
		}

		protected override string ConvertFromSource(object value, object parameter)
		{
			return value == null ? this.NullMessage : this.NonNullMessage;
		}
	}

#pragma warning restore 1591
}
