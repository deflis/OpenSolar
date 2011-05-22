using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Media;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using Ignition;
using Ignition.Linq;
using Ignition.Presentation;
using Lunar;
using Microsoft.Scripting;
using Solar.Dialogs;
using Solar.Filtering;
using Solar.Models;

namespace Solar
{
	/// <summary>
	/// メインウィンドウです。Solar のクライアント機能へのユーザのグラフィカルアクセスを提供します。
	/// </summary>
	public partial class MainWindow : Window
	{
		ICommand postCommand;
		WindowState lastWindowState;
		bool close;
		static readonly DependencyPropertyKey PostTextBoxBackgroundPropertyKey = DependencyProperty.RegisterReadOnly("PostTextBoxBackground", typeof(Color), typeof(MainWindow), new UIPropertyMetadata(SystemColors.WindowColor));
		/// <summary>
		/// PostTextBoxBackground プロパティ
		/// </summary>
		public static readonly DependencyProperty PostTextBoxBackgroundProperty = PostTextBoxBackgroundPropertyKey.DependencyProperty;
		readonly AutoCompletion<User> userNameAutoCompletion;
		readonly AutoCompletion<string> hashAutoCompletion;
		readonly Dictionary<CategoryGroup, Category> lastSelected = new Dictionary<CategoryGroup, Category>();
		readonly Dictionary<Category, ContextMenu> tabContextMenuCollection = new Dictionary<Category, ContextMenu>();

		/// <summary>
		/// 投稿ボックスの背景色を取得します。
		/// </summary>
		public Color PostTextBoxBackground
		{
			get
			{
				return (Color)GetValue(PostTextBoxBackgroundProperty);
			}
			private set
			{
				SetValue(PostTextBoxBackgroundPropertyKey, value);
			}
		}

		MainWindowViewModel ViewModel
		{
			get
			{
				return (MainWindowViewModel)this.DataContext;
			}
		}

		/// <summary>
		/// メイン メニューを取得します。
		/// </summary>
		public Menu MainMenu
		{
			get
			{
				return mainMenu;
			}
		}

		/// <summary>
		/// 投稿ボックスを取得します。
		/// </summary>
		public TextBox PostTextBox
		{
			get
			{
				return postTextBox;
			}
		}

		/// <summary>
		/// 現在の投稿先アカウントを取得または設定します。
		/// </summary>
		public AccountToken PostAccount
		{
			get
			{
				return this.ViewModel.PostAccount.InternalAccount;
			}
			set
			{
				this.ViewModel.PostAccount = this.ViewModel.Accounts.Single(_ => _.InternalAccount.UserID == value.UserID);
			}
		}

		/// <summary>
		/// 投稿ボックスのコンテキストメニューを取得します。
		/// </summary>
		public ContextMenu PostTextBoxContextMenu
		{
			get
			{
				return postTextBoxContextMenu;
			}
		}

		/// <summary>
		/// ステータスバーを取得します。
		/// </summary>
		public StatusBar StatusBar
		{
			get
			{
				return statusBar;
			}
		}

		/// <summary>
		/// MainWindow の新しいインスタンスを初期化します。
		/// </summary>
		public MainWindow()
		{
			InitializeComponent();

			this.ViewModel.RequestShow += (sender, e) => e.Show(this);
			this.ViewModel.RequestShowDialog += (sender, e) => e.ShowDialog(this);
			this.ViewModel.RequestFocusStartPostTextBox += (sender, e) =>
			{
				postTextBox.SelectionStart =
					postTextBox.SelectionLength = 0;
				postTextBox.Focus();
			};
			this.ViewModel.RequestFocusEndPostTextBox += (sender, e) =>
			{
				postTextBox.SelectionStart = postTextBox.Text.Length;
				postTextBox.Focus();
			};
			this.ViewModel.RequestLoadSettings += (sender, e) =>
			{
				if (!e.Value.Interface.Bounds.IsEmpty)
				{
					this.Left = e.Value.Interface.Bounds.X;
					this.Top = e.Value.Interface.Bounds.Y;
					this.Width = e.Value.Interface.Bounds.Width;
					this.Height = e.Value.Interface.Bounds.Height;
					this.WindowState = e.Value.Interface.WindowState;
				}
			};
			this.ViewModel.RequestSaveSettings += (sender, e) =>
			{
				if (double.IsNaN(this.Left) ||
					double.IsNaN(this.Top))
					return;

				e.Value.Interface.Bounds = this.RestoreBounds.IsEmpty ? new Rect(this.Left, this.Top, this.Width, this.Height) : this.RestoreBounds;
				e.Value.Interface.WindowState = this.WindowState;
			};
			this.ViewModel.RequestUpdatePostTextSource += (sender, e) => postTextBox.GetBindingExpression(TextBox.TextProperty).UpdateSource();
			userNameAutoCompletion = new AutoCompletion<User>(postTextBox, postUserPopup, postUserListBox, '@', this.ViewModel.StatusCache.GetUsers().Where(_ => _ != null).Distinct(new InstantEqualityComparer<User>(_ => _.Name)), _ => "@" + _.Name)
			{
				OffsetX = -22,
			};
			hashAutoCompletion = new AutoCompletion<string>(postTextBox, postHashPopup, postHashListBox, '#', this.ViewModel.StatusCache.GetStatuses().Where(_ => _ != null).Select(_ => _.Text).Concat(Settings.Default.Post.Footers.Select(_ => _.Text)).SelectMany(_ => LinkConverter.HashRegex.Matches(_).Cast<Match>()).Select(_ => _.Value).Distinct(), _ => _);
			this.ViewModel.RequestPostCompleted += (sender, e) =>
			{
				if (e.Value &&
					string.IsNullOrEmpty(postTextBox.Text) &&
					Settings.Default.Post.Footers.Any(_ => _.Use) &&
					postTextBox.IsFocused)
				{
					postTextBox.Text = " " + Settings.Default.Post.Footers.Where(_ => _.Use).Join(" ");
					this.Dispatcher.BeginInvoke((Action)(() => postTextBox.Select(0, 0)));
				}
			};
		}

		/// <summary>
		/// サブタイムラインウィンドウを開きます。
		/// </summary>
		/// <param name="title">タイトル。</param>
		/// <param name="source">フィルタ ソース。</param>
		public QueryResultWindow OpenSubTimelineWindow(string title, FilterSource source)
		{
			return OpenSubTimelineWindow(title, source, this.ViewModel.Accounts.Select(_ => _.InternalAccount).ToArray());
		}

		/// <summary>
		/// サブタイムラインウィンドウを開きます。
		/// </summary>
		/// <param name="title">タイトル。</param>
		/// <param name="source">フィルタ ソース。</param>
		/// <param name="accounts">アカウント。</param>
		public QueryResultWindow OpenSubTimelineWindow(string title, FilterSource source, params AccountToken[] accounts)
		{
			return new QueryResultWindow(title, new ClientParameters(accounts, this.ViewModel.StatusCache, this.ViewModel.StatusesListBoxCommandHandler), null, source)
			{
				Owner = this,
			}.Apply(_ => _.Show());
		}

		void Window_Loaded(object sender, RoutedEventArgs e)
		{
			this.ViewModel.Run(DesignerProperties.GetIsInDesignMode(this));
		}

		void Window_Closing(object sender, CancelEventArgs e)
		{
			if (!Settings.Default.Interface.CloseToTray || close)
			{
				userNameAutoCompletion.Dispose();
				hashAutoCompletion.Dispose();
				this.ViewModel.Closed();
			}
			else
			{
				e.Cancel = true;
				this.Visibility = Visibility.Collapsed;
				notificationAreaIcon.Visibility = Visibility.Visible;
			}
		}

		void postTextBox_PreviewKeyDown(object sender, KeyEventArgs e)
		{
			if (!postUserPopup.IsOpen &&
				!postHashPopup.IsOpen)
			{
				if (e.Key == Key.Enter &&
				   (!Settings.Default.Interface.PostByControlEnter && e.KeyboardDevice.Modifiers == ModifierKeys.None || Settings.Default.Interface.PostByControlEnter && e.KeyboardDevice.Modifiers == ModifierKeys.Control))
				{
					if ((postCommand ?? (postCommand = this.ViewModel.PostCommand)).CanExecute(postTextBox.Text))
					{
						postCommand.Execute(postTextBox.Text);
						e.Handled = true;
					}
				}
				else if (e.Key == Key.Escape)
				{
					this.ViewModel.ClearDirectMessageCommand.Execute(null);
					this.ViewModel.ClearInReplyToCommand.Execute(null);
				}
			}
		}

		void FooterMenuItem_Click(object sender, RoutedEventArgs e)
		{
			var footer = (Footer)((MenuItem)sender).DataContext;

			if (postTextBox.Text.Trim() == Settings.Default.Post.Footers.Where(_ => _ == footer ? !_.Use : _.Use).Join(" "))
				postTextBox.Text = " " + Settings.Default.Post.Footers.Where(_ => _.Use).Join(" ");
		}

		void ExitMenuItem_Click(object sender, RoutedEventArgs e)
		{
			close = true;
			this.Close();
		}

		void StatusesListBox_RequestNewPage(object sender, RoutedEventArgs e)
		{
			this.ViewModel.RequestNewPage((Category)((StatusesListBox)e.Source).DataContext);
		}

		void NotificationAreaIcon_MouseClick(object sender, MouseButtonEventArgs e)
		{
			Wakeup();
		}

		void Window_StateChanged(object sender, EventArgs e)
		{
			if (this.WindowState == WindowState.Minimized && Settings.Default.Interface.MinimizeToTray)
			{
				this.Visibility = Visibility.Collapsed;
				this.WindowState = lastWindowState;
				notificationAreaIcon.Visibility = Visibility.Visible;
			}
			else
				lastWindowState = this.WindowState;
		}

		void postTextBox_GotFocus(object sender, RoutedEventArgs e)
		{
			if (string.IsNullOrEmpty(postTextBox.Text) &&
				Settings.Default.Post.Footers.Any(_ => _.Use))
			{
				postTextBox.Text = " " + Settings.Default.Post.Footers.Where(_ => _.Use).Join(" ");
				this.Dispatcher.BeginInvoke((Action)(() => postTextBox.Select(0, 0)));
			}
		}

		void postTextBox_LostFocus(object sender, RoutedEventArgs e)
		{
			if (postTextBox.Text == " " + Settings.Default.Post.Footers.Where(_ => _.Use).Join(" "))
				postTextBox.Text = null;
		}

		void postTextBox_TextChanged(object sender, TextChangedEventArgs e)
		{
			PostTextBoxBackground = postTextBox.Text.Length > 140 ? Colors.MistyRose : SystemColors.WindowColor;
		}

		internal void Wakeup()
		{
			this.WindowState = WindowState.Normal;
			this.Visibility = Visibility.Visible;
			notificationAreaIcon.Visibility = Visibility.Hidden;
			this.Activate();
		}

		void AccountCheckBox_MouseDown(object sender, MouseButtonEventArgs e)
		{
			if (e.RightButton != MouseButtonState.Pressed)
			{
				var c = (CheckBox)sender;
				var a = (MainWindowViewModel.AccountTokenViewModel)c.DataContext;

				if (a.IsSelected)
					this.ViewModel.Accounts.Run(_ => _.IsSelected = true);
				else
					this.ViewModel.PostAccount = a;

				e.Handled = true;
			}
		}

		void StatusBarItem_MouseDown(object sender, MouseButtonEventArgs e)
		{
			ProgressBlock.Clear();
		}

		void TabControl_SelectionChanged(dynamic sender, SelectionChangedEventArgs e)
		{
			TabControl tabControl = sender;

			if (!(tabControl.DataContext is CategoryGroup))
				return;

			CategoryGroup group = sender.DataContext;
			var category = (Category)tabControl.SelectedItem;

			if (Settings.Default.Timeline.ResetNewCountOnTabShow)
				category.ClearUnreads();

			if (Settings.Default.Timeline.ResetNewCountOnTabHide)
				if (lastSelected.ContainsKey(group) &&
				   lastSelected[group] != category &&
					lastSelected[group] != null)
					lastSelected[group].ClearUnreads();

			lastSelected[group] = category;
		}

		void tabContextMenu_Loaded(object sender, RoutedEventArgs e)
		{
			tabContextMenuCollection[(Category)((ContextMenu)sender).DataContext] = (ContextMenu)sender;
		}
	}

#pragma warning disable 1591

	public class MainWindowViewModel : ViewModel<Client>, IProgressHost
	{
		ICommand refreshCommand;
		ICommand clearInReplyToCommand;
		ICommand clearDirectMessageCommand;

		internal event EventHandler<ShowDialogEventArgs> RequestShow;
		internal event EventHandler<ShowDialogEventArgs> RequestShowDialog;
		public event EventHandler<EventArgs<Settings>> RequestLoadSettings;
		public event EventHandler<EventArgs<Settings>> RequestSaveSettings;
		public event EventHandler RequestFocusStartPostTextBox;
		public event EventHandler RequestFocusEndPostTextBox;
		public event EventHandler RequestUpdatePostTextSource;
		public event EventHandler<EventArgs<bool>> RequestPostCompleted;
		readonly Dictionary<Category, ProgressBlock> progresses = new Dictionary<Category, ProgressBlock>();

		public MainWindowViewModel()
			: base(Client.Instance)
		{
			ProgressBlock.ProgressHost = this;

			this.Accounts = this.Model.Accounts.Select(_ => new AccountTokenViewModel(this, _)).Freeze();
			this.RateLimits = new List<RateLimitViewModel>();
			this.Model.PropertyChanged += (sender, e) => OnPropertyChanged(e);
			this.Model.ThrowError += (sender, e) => MessageBoxEx.Show(Application.Current.MainWindow, App.Log(e.Value2).Message, "送信中にエラーが発生しました", MessageBoxButton.OK, MessageBoxImage.Warning);
			this.Model.ThrowWarning += (sender, e) =>
			{
				if (e.Value1 == null || this.Model.Groups.SelectMany(_ => _).Contains(e.Value1))
					new ProgressBlock(App.Log(e.Value2).Message, true);
			};
			this.Model.ThrowScriptError += (sender, e) =>
			{
				var message = App.Log(e.Value2).ToString();

				if (e.Value2 is SyntaxErrorException)
				{
					var ex = (SyntaxErrorException)e.Value2;

					message = string.Format(@"{0}
{1} line {2}, {3}", ex.Message, ex.SourcePath, ex.Line, ex.Column);
				}

				MessageBoxEx.Show(Application.Current.MainWindow, message, e.Value1, MessageBoxButton.OK, MessageBoxImage.Warning);
			};
			this.Model.Posting += (sender, e) => this.IsPosting = true;
			this.Model.Posted += (sender, e) =>
			{
				this.IsPosting = false;

				if (e.Value)
				{
					this.ClearInReplyToCommand.Execute(null);
					this.ClearDirectMessageCommand.Execute(null);
					this.PostText = null;
				}
			};
			this.Model.Authenticate += (sender, e2) =>
			{
				using (new ProgressBlock("認証を要求しています..."))
				{
					ShowDialogEventArgs e = null;

					Application.Current.Dispatcher.Invoke((Action)delegate
					{
						e = new ShowDialogEventArgs(new AuthenticateWindow(e2.Value));
						RequestShowDialog.RaiseEvent(this, e);
					});

					if (e.DialogResult != true)
						return;
				}
			};
			this.Model.Refreshing += (sender, e) =>
			{
				if (!this.Model.Groups.SelectMany().Contains(e.Value))
					return;

				if (progresses.ContainsKey(e.Value))
					progresses[e.Value].Dispose();

				progresses[e.Value] = new ProgressBlock(e.Value.Name + " を更新しています...");
			};
			this.Model.Refreshed += (sender, e) =>
			{
				if (!this.Model.Groups.SelectMany().Contains(e.Value1))
					return;

				try
				{
					if (e.Value2 != null && e.Value2.Any())
					{
						if (e.Value1.NotifyUpdates &&
							App.Current.Windows.OfType<NotifyWindow>().All(_ => !_.Statuses.SequenceEqual(e.Value2)))
							RequestShow.RaiseEvent(this, new ShowDialogEventArgs(_ => new NotifyWindow(_, e.Value2), false));

						if (!string.IsNullOrEmpty(e.Value1.NotifySound) &&
							File.Exists(e.Value1.NotifySound))
							Task.Factory.StartNew(() =>
							{
								using (var s = new SoundPlayer(e.Value1.NotifySound))
									s.PlaySync();
							});
					}
				}
				catch (Exception ex)
				{
					ex.ToString();
				}

				if (progresses.ContainsKey(e.Value1))
				{
					progresses[e.Value1].Dispose();
					progresses.Remove(e.Value1);
				}
			};
			this.Model.RequestingNewPage += (sender, e) =>
			{
				if (this.Model.Groups.SelectMany().Contains(e.Value))
					new ProgressBlock(e.Value.Name + " のスクロール先を取得しています...");
			};
			this.Model.RequestedNewPage += (sender, e) =>
			{
				if (this.Model.Groups.SelectMany().Contains(e.Value))
					ProgressBlock.Pop();
			};
			this.Model.LoadSettings += (sender, e) => RequestLoadSettings.RaiseEvent(sender, e);
			this.Model.SaveSettings += (sender, e) => RequestSaveSettings.RaiseEvent(sender, e);
			this.Model.Accounts.CollectionChanged += (sender, e) => this.Accounts = this.Model.Accounts.Select(_ => new AccountTokenViewModel(this, _)).Freeze();
			this.Model.RateLimits.CollectionChanged += (sender, e) =>
			{
				this.RateLimits = this.Model.RateLimits.Select(_ => new RateLimitViewModel(_)
				{
					IsStreaming = this.Model.IsStreaming(_.Account),
				}).Freeze();
				OnPropertyChanged("RateLimit");
			};
			this.Model.StreamConnected += (sender, e) =>
			{
				this.RateLimits.Where(_ => _.InternalRateLimit.Account == e.Value).Run(_ => _.IsStreaming = true);
				OnPropertyChanged("RateLimits");
			};
			this.Model.StreamDisconnected += (sender, e) =>
			{
				this.RateLimits.Where(_ => _.InternalRateLimit.Account == e.Value).Run(_ => _.IsStreaming = false);
				OnPropertyChanged("RateLimits");
			};

			this.StatusesListBoxCommandHandler = new StatusesListBoxCommandHandler
			{
				DoubleClickCommand = new RelayCommand<IEntry>(_ =>
				{
					if (_ is Status &&
						this.StatusesListBoxCommandHandler.DoubleClickStatusCommand.CanExecute(_))
						this.StatusesListBoxCommandHandler.DoubleClickStatusCommand.Execute(_);
					else if (_ is List
						 && this.StatusesListBoxCommandHandler.OpenListCommand.CanExecute(_))
						this.StatusesListBoxCommandHandler.OpenListCommand.Execute(_);
				}),
				DoubleClickStatusCommand = new RelayCommand<Status>(_ => _ != null, _ =>
				{
					switch (Settings.Default.Timeline.DoubleClickAction)
					{
						case ClickAction.Copy:
							if (this.StatusesListBoxCommandHandler.CopyStatusCommand.CanExecute(_))
								this.StatusesListBoxCommandHandler.CopyStatusCommand.Execute(_);

							break;
						case ClickAction.ReplyTo:
							if (this.StatusesListBoxCommandHandler.ReplyToStatusCommand.CanExecute(_))
								this.StatusesListBoxCommandHandler.ReplyToStatusCommand.Execute(_);

							break;
						case ClickAction.Quote:
							if (this.StatusesListBoxCommandHandler.QuoteStatusCommand.CanExecute(_))
								this.StatusesListBoxCommandHandler.QuoteStatusCommand.Execute(_);

							break;
						case ClickAction.Favorite:
							if (this.StatusesListBoxCommandHandler.FavoriteStatusCommand.CanExecute(_))
								this.StatusesListBoxCommandHandler.FavoriteStatusCommand.Execute(_);

							break;
						case ClickAction.StatusDetails:
							if (this.StatusesListBoxCommandHandler.StatusDetailsCommand.CanExecute(_))
								this.StatusesListBoxCommandHandler.StatusDetailsCommand.Execute(_);

							break;
						case ClickAction.UserDetails:
							if (this.StatusesListBoxCommandHandler.UserDetailsCommand.CanExecute(_))
								this.StatusesListBoxCommandHandler.UserDetailsCommand.Execute(_);

							break;
						case ClickAction.NearStatuses:
							if (this.StatusesListBoxCommandHandler.NearStatusesCommand.CanExecute(_))
								this.StatusesListBoxCommandHandler.NearStatusesCommand.Execute(_);

							break;
					}
				}),
				CopyStatusCommand = new RelayCommand<Status>(_ => _ != null, _ => Clipboard.SetText(string.Format("{0}:{1} [{2}]", _.UserName, _.Text, _.Uri))),
				CopyStatusUriCommand = new RelayCommand<Status>(_ => _ != null, _ => Clipboard.SetText(_.Uri)),
				CopyStatusUserCommand = new RelayCommand<Status>(_ => _ != null, _ => Clipboard.SetText(_.UserName)),
				CopyStatusBodyCommand = new RelayCommand<Status>(_ => _ != null, _ => Clipboard.SetText(_.Text)),
				ReplyToStatusCommand = new RelayCommand<Status>(_ => _ != null && !_.IsDirectMessage, _ =>
				{
					_ = _.RetweetedStatusOrSelf;

					if (Settings.Default.Post.AutoSwitchAccount)
						this.PostAccount = this.Accounts.FirstOrDefault(a => a.InternalAccount.Equals(_.Account));

					if (this.InReplyToStatus == null ||
						!this.PostText.Contains("@" + this.InReplyToStatus.UserName))
						this.InReplyToStatus = _;
					else
						this.PostText += "@" + _.UserName + " ";
				}),
				RetweetStatusCommand = new RelayCommand<Status>(_ => _ != null && !_.IsDirectMessage, _ =>
				{
					if (Settings.Default.Post.AutoSwitchAccount)
						this.PostAccount = this.Accounts.FirstOrDefault(a => a.InternalAccount.Equals(_.Account));

					if (MessageBoxEx.Show(Application.Current.MainWindow, "以下のつぶやきを " + this.PostAccount + " で公式 Retweet しますか？\r\n" + _.UserName + ": " + _.Text, "Retweet (" + this.PostAccount + ")", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
						this.Model.Retweet(this.PostAccount.InternalAccount, _);
				}),
				QuoteStatusCommand = new RelayCommand<Status>(_ => _ != null && !_.IsDirectMessage, _ => Task.Factory.StartNew(() =>
				{
					if (Settings.Default.Post.AutoSwitchAccount)
						this.PostAccount = this.Accounts.FirstOrDefault(a => a.InternalAccount.Equals(_.Account));

					this.PostText = " RT @" + _.UserName + ": " + _.Text;
					RequestFocusStartPostTextBox.DispatchEvent(this, EventArgs.Empty);
				})),
				FavoriteStatusCommand = new RelayCommand<Status>(_ => _ != null && !_.IsDirectMessage, _ => this.Model.ToggleFavorite(_.Account, _)),
				UserDetailsCommand = new RelayCommand(value => value.TypeMatch
				(
					(Status _) =>
					{
						if (_.User == null)
							RequestShow.RaiseEvent(this, new ShowDialogEventArgs(new QueryResultWindow(CreateClientParameters(_.Account), _.UserName)));
						else
							RequestShow.RaiseEvent(this, new ShowDialogEventArgs(new QueryResultWindow(CreateClientParameters(_.Account), _.User)));
					},
					(User _) => RequestShow.RaiseEvent(this, new ShowDialogEventArgs(new QueryResultWindow(CreateClientParameters(_.Account), _))),
					(LinkConverter.LinkInfo _) => RequestShow.RaiseEvent(this, new ShowDialogEventArgs(new QueryResultWindow(CreateClientParameters(_.Status.Account), _.Link))),
					(string _) => RequestShow.RaiseEvent(this, new ShowDialogEventArgs(new QueryResultWindow(CreateClientParameters(), _)))
				)),
				StatusDetailsCommand = new RelayCommand<Status>(_ => _ != null && !_.IsDirectMessage, _ => Process.Start(_.Uri)),
				ReplyToDetailsCommand = new RelayCommand<Status>(_ => _ != null && _.IsReply, _ => RequestShow.RaiseEvent(this, new ShowDialogEventArgs(new QueryResultWindow
				(
					"返信履歴",
					CreateClientParameters(_.Account),
					_.User,
					new ReplyChainFilterSource
					{
						Account = _.Account.UserID,
						RootStatus = _.StatusID,
					}
				)))),
				DeleteStatusCommand = new RelayCommand<Status>(_ => _ != null && _.IsOwned, _ => this.Model.Remove(_)),
				SearchCommand = new RelayCommand(value => value.TypeMatch
				(
					(LinkConverter.LinkInfo _) => RequestShow.RaiseEvent(this, new ShowDialogEventArgs(new QueryResultWindow
					(
						_.Link + " の検索結果",
						CreateClientParameters(_.Status.Account),
						null,
						new SearchFilterSource
						{
							Account = _.Status.Account.UserID,
							Query = _.Link,
						}
					))),
					(string _) => RequestShow.RaiseEvent(this, new ShowDialogEventArgs(new QueryResultWindow
					(
						_ + " の検索結果",
						CreateClientParameters(),
						null,
						new SearchFilterSource
						{
							Query = _,
						}
					)))
				)),
				OpenUriCommand = new RelayCommand<Uri>(_ => Process.Start(new ProcessStartInfo(Environment.GetEnvironmentVariable("comspec"), "/c start " + _.AbsoluteUri.Replace("&", "^&"))
				{
					CreateNoWindow = true,
					UseShellExecute = false,
					WindowStyle = ProcessWindowStyle.Hidden,
				})),
				DirectMessageCommand = new RelayCommand<Status>(_ =>
				{
					if (_ != null)
						this.DirectMessageDestination = _.UserName;

					this.IsDirectMessage = true;
				}),
				NearStatusesCommand = new RelayCommand<Status>(_ => _ != null && !_.IsDirectMessage, _ => RequestShow.RaiseEvent(this, new ShowDialogEventArgs(new QueryResultWindow
				(
					_.UserName + ": " + _.Text,
					CreateClientParameters(_.Account),
					null,
					new NearStatusFilterSource
					{
						RootStatus = _.StatusID,
					}
				)))),
				OpenListCommand = new RelayCommand(_ => _ != null && _ is List, value => value.TypeMatch
				(
					(List _) => RequestShow.RaiseEvent(this, new ShowDialogEventArgs(new QueryResultWindow
					(
						"@" + _.User.Name + "/" + _.Name,
						CreateClientParameters(_.Account),
						_.User,
						new ListFilterSource
						{
							UserID = _.User.UserID,
							UserName = _.User.Name,
							ListID = _.ListID,
							ListName = _.Name,
						}
					)))
				)),
			};
		}

		#region Commands

		public ICommand SetTimelineCommand
		{
			get
			{
				return new RelayCommand(value => value.TypeMatch
				(
					(TimelineStyle _) => Settings.Default.Timeline.TimelineStyle = _,
					(string _) => Settings.Default.Timeline.TimelineStyle = (TimelineStyle)Enum.Parse(typeof(TimelineStyle), _)
				));
			}
		}

		public ICommand CheckSoftwareUpdatesCommand
		{
			get
			{
				return new RelayCommand(_ => App.CheckUpdates(false));
			}
		}

		public ICommand PostCommand
		{
			get
			{
				return refreshCommand ?? (refreshCommand = new RelayCommand<string>(_ => !string.IsNullOrEmpty(_), _ =>
				{
					if (_.Length > 140)
						MessageBoxEx.Show("140 文字までしか投稿できません。\r\n140 文字以内になるよう修正してください。", "投稿", MessageBoxButton.OK, MessageBoxImage.Information);
					else
						if (this.IsDirectMessage)
						{
							if (this.Accounts.Count(a => a.IsSelected) < 2 ||
								MessageBoxEx.Show("複数のアカウントが選択されています。\r\n本当に複数のアカウントでダイレクトメッセージを送信してよろしいですか？", "投稿", MessageBoxButton.YesNo, MessageBoxImage.Information) == MessageBoxResult.Yes)
								foreach (var i in this.Accounts.Where(a => a.IsSelected))
								{
									var task = this.Model.Post(i.InternalAccount, _, this.DirectMessageDestination);

									if (task != null)
										task.ContinueWith(rt => RequestPostCompleted.DispatchEvent(this, new EventArgs<bool>(rt.Result)));
								}
						}
						else if (this.InReplyToStatus == null)
							foreach (var i in this.Accounts.Where(a => a.IsSelected))
							{
								var task = this.Model.Post(i.InternalAccount, _);

								if (task != null)
									task.ContinueWith(rt => RequestPostCompleted.DispatchEvent(this, new EventArgs<bool>(rt.Result)));
							}
						else
						{
							if (this.Accounts.Count(a => a.IsSelected) < 2 ||
								MessageBoxEx.Show("複数のアカウントが選択されています。\r\n本当に複数のアカウントで返信してよろしいですか？", "投稿", MessageBoxButton.YesNo, MessageBoxImage.Information) == MessageBoxResult.Yes)
								foreach (var i in this.Accounts.Where(a => a.IsSelected))
								{
									var task = this.Model.Post(i.InternalAccount, _, this.InReplyToStatus);

									if (task != null)
										task.ContinueWith(rt => RequestPostCompleted.DispatchEvent(this, new EventArgs<bool>(rt.Result)));
								}
						}
				}));
			}
		}

		public ICommand ShowProfileCommand
		{
			get
			{
				return new RelayCommand(_ => this.Model.Accounts != null && this.Model.Accounts.Any(), value => value.TypeMatch
				(
					(AccountToken _) => RequestShow.RaiseEvent(this, new ShowDialogEventArgs(new QueryResultWindow(CreateClientParameters(_), _.Name))),
					_ => RequestShow.RaiseEvent(this, new ShowDialogEventArgs(new QueryResultWindow(CreateClientParameters(this.Model.Accounts.First()), this.Model.Accounts.First().Name)))
				));
			}
		}

		public ICommand ShowFollowingCommand
		{
			get
			{
				return new RelayCommand(_ => this.Model.Accounts != null && this.Model.Accounts.Any(), value => value.TypeMatch
				(
					(AccountToken _) => RequestShow.RaiseEvent(this, new ShowDialogEventArgs(new QueryResultWindow
					(
						_.Name + " のフォロー",
						CreateClientParameters(_),
						null,
						new FollowingFilterSource
						{
							UserName = _.Name,
						}
					))),
					_ => RequestShow.RaiseEvent(this, new ShowDialogEventArgs(new QueryResultWindow
					(
						this.Model.Accounts.Join(", ") + " のフォロー",
						CreateClientParameters(),
						null,
						new FollowingFilterSource()
					)))
				));
			}
		}

		public ICommand ShowFollowersCommand
		{
			get
			{
				return new RelayCommand(_ => this.Model.Accounts != null && this.Model.Accounts.Any(), value => value.TypeMatch
				(
					(AccountToken _) => RequestShow.RaiseEvent(this, new ShowDialogEventArgs(new QueryResultWindow
					(
						_.Name + " のフォロワー",
						CreateClientParameters(_),
						null,
						new FollowersFilterSource
						{
							UserName = _.Name,
						}
					))),
					_ => RequestShow.RaiseEvent(this, new ShowDialogEventArgs(new QueryResultWindow
					(
						this.Model.Accounts.Join(", ") + " のフォロワー",
						CreateClientParameters(),
						null,
						new FollowersFilterSource()
					)))
				));
			}
		}

		public ICommand ShowListsCommand
		{
			get
			{
				return new RelayCommand(_ => this.Model.Accounts != null && this.Model.Accounts.Any(), value => value.TypeMatch
				(
					(AccountToken _) => RequestShow.RaiseEvent(this, new ShowDialogEventArgs(new QueryResultWindow
					(
						_.Name + " によるリスト",
						CreateClientParameters(_),
						null,
						new ListIndexFilterSource
						{
							UserName = _.Name,
						}
					))),
					_ => RequestShow.RaiseEvent(this, new ShowDialogEventArgs(new QueryResultWindow
					(
						this.Model.Accounts.Join(", ") + " によるリスト",
						CreateClientParameters(),
						null,
						new ListIndexFilterSource()
					)))
				));
			}
		}

		public ICommand ShowSubscriptionsCommand
		{
			get
			{
				return new RelayCommand(_ => this.Model.Accounts != null && this.Model.Accounts.Any(), value => value.TypeMatch
				(
					(AccountToken _) => RequestShow.RaiseEvent(this, new ShowDialogEventArgs(new QueryResultWindow
					(
						_.Name + " がフォローしているリスト",
						CreateClientParameters(_),
						null,
						new ListIndexFilterSource
						{
							UserName = _.Name,
							Mode = ListIndexFilterSource.IndexMode.Subscriptions,
						}
					))),
					_ => RequestShow.RaiseEvent(this, new ShowDialogEventArgs(new QueryResultWindow
					(
						this.Model.Accounts.Join(", ") + " がフォローしているリスト",
						CreateClientParameters(),
						null,
						new ListIndexFilterSource
						{
							Mode = ListIndexFilterSource.IndexMode.Subscriptions,
						}
					)))
				));
			}
		}

		public ICommand ShowMembershipsCommand
		{
			get
			{
				return new RelayCommand(_ => this.Model.Accounts != null && this.Model.Accounts.Any(), value => value.TypeMatch
				(
					(AccountToken _) => RequestShow.RaiseEvent(this, new ShowDialogEventArgs(new QueryResultWindow
					(
						_.Name + " が登録されているリスト",
						CreateClientParameters(_),
						null,
						new ListIndexFilterSource
						{
							UserName = _.Name,
							Mode = ListIndexFilterSource.IndexMode.Memberships,
						}
					))),
					_ => RequestShow.RaiseEvent(this, new ShowDialogEventArgs(new QueryResultWindow
					(
						this.Model.Accounts.Join(", ") + " が登録されているリスト",
						CreateClientParameters(),
						null,
						new ListIndexFilterSource
						{
							Mode = ListIndexFilterSource.IndexMode.Memberships,
						}
					)))
				));
			}
		}

		public ICommand RefreshCommand
		{
			get
			{
				return new RelayCommand<Category>(_ => this.Model.Accounts != null && this.Model.Accounts.Any(), _ => this.Model.Update(_));
			}
		}

		public ICommand ClearCommand
		{
			get
			{
				return new RelayCommand<Category>(_ => _ != null, _ => _.ClearStatuses());
			}
		}

		public ICommand CloseTabCommand
		{
			get
			{
				return new RelayCommand<Category>(target =>
				{
					if (target == null)
						return false;

					var group = this.Model.Groups.SingleOrDefault(_ => _.Contains(target));

					if (group == null)
						return false;
					else
						return group.Count > 1;
				}, target => this.Model.Groups.Single(_ => _.Contains(target)).Remove(target));
			}
		}

		public ICommand ClearUnreadsCommand
		{
			get
			{
				return new RelayCommand<Category>(_ => this.Model.ClearUnreads(_));
			}
		}

		public ICommand ClearInReplyToCommand
		{
			get
			{
				return clearInReplyToCommand ?? (clearInReplyToCommand = new RelayCommand(_ => this.InReplyToStatus = null));
			}
		}

		public ICommand ClearDirectMessageCommand
		{
			get
			{
				return clearDirectMessageCommand ?? (clearDirectMessageCommand = new RelayCommand(_ => this.IsDirectMessage = false));
			}
		}

		public ICommand SearchStatusesCommand
		{
			get
			{
				return new RelayCommand(_ => RequestShow.RaiseEvent(this, new ShowDialogEventArgs(new QueryResultWindow
				(
					"つぶやきの検索",
					CreateClientParameters(),
					null,
					new SearchFilterSource()
				))));
			}
		}

		public ICommand SearchUsersCommand
		{
			get
			{
				return new RelayCommand(_ => RequestShow.RaiseEvent(this, new ShowDialogEventArgs(new QueryResultWindow
				(
					"ユーザの検索",
					CreateClientParameters(),
					null,
					new SearchUsersFilterSource()
				))));
			}
		}

		public ICommand SearchCacheCommand
		{
			get
			{
				return new RelayCommand(_ => RequestShow.RaiseEvent(this, new ShowDialogEventArgs(new QueryResultWindow
				(
					"キャッシュから検索",
					CreateClientParameters(),
					null,
					new SearchCacheFilterSource()
				))));
			}
		}

		public ICommand QueryCacheCommand
		{
			get
			{
				return new RelayCommand(_ => RequestShow.RaiseEvent(this, new ShowDialogEventArgs(new QueryResultWindow
				(
					"キャッシュへクエリ",
					CreateClientParameters(),
					null,
					new QueryCacheFilterSource()
				))));
			}
		}

		public ICommand FootersCommand
		{
			get
			{
				return new RelayCommand(_ => RequestShowDialog.RaiseEvent(this, new ShowDialogEventArgs(new FootersWindow())));
			}
		}

		public ICommand CacheCommand
		{
			get
			{
				return new RelayCommand(_ => RequestShowDialog.RaiseEvent(this, new ShowDialogEventArgs(new CacheWindow(this.Model.StatusCache, () => this.Model.ClearStatuses()))));
			}
		}

		public ICommand AccountsCommand
		{
			get
			{
				return new RelayCommand(_ => RequestShowDialog.RaiseEvent(this, new ShowDialogEventArgs(new AccountsWindow(this.Model.Accounts))));
			}
		}

		public ICommand ConsoleCommand
		{
			get
			{
				return new RelayCommand(_ => RequestShow.RaiseEvent(this, new ShowDialogEventArgs(new ConsoleWindow())));
			}
		}

		public ICommand LayoutCommand
		{
			get
			{
				return new RelayCommand<Category>(target =>
				{
					var lw = new LayoutWindow(Settings.Default.GlobalFilterTerms, this.Model.Groups, this.Model.Accounts, target == null ? null : target);
					var e = new ShowDialogEventArgs(lw);

					RequestShowDialog.RaiseEvent(this, e);

					if (e.DialogResult == true)
					{
						var tabs = this.Model.Groups.SelectMany().Freeze();

						Settings.Default.GlobalFilterTerms.Clear();
						Settings.Default.GlobalFilterTerms.AddRange(lw.GlobalTerms);

						this.Model.Groups.Clear();
						this.Model.Groups.AddRange(lw.CategoryGroups);

						this.Model.ApplyFilter();
						this.Model.Update(this.Model.Groups.SelectMany()
														   .Where(_ => !_.Statuses.Any()));
					}
				});
			}
		}

		public ICommand OrganizeFollowsCommand
		{
			get
			{
				return new RelayCommand(_ => this.Model.Accounts != null && this.Model.Accounts.Any(), value => value.TypeMatch
				(
					(AccountToken _) => RequestShow.RaiseEvent(this, new ShowDialogEventArgs(new FollowManagerWindow(this.StatusesListBoxCommandHandler, CreateClientParameters(_)))),
					_ => RequestShow.RaiseEvent(this, new ShowDialogEventArgs(new FollowManagerWindow(this.StatusesListBoxCommandHandler, CreateClientParameters(this.Model.Accounts.First()))))
				));
			}
		}

		public ICommand AboutCommand
		{
			get
			{
				return new RelayCommand(_ => RequestShowDialog.RaiseEvent(this, new ShowDialogEventArgs(new AboutWindow())));
			}
		}

		public ICommand ShortenUrlCommand
		{
			get
			{
				return new RelayCommand<string>(_ =>
				{
					RequestUpdatePostTextSource.RaiseEvent(this, EventArgs.Empty);

					Task.Factory.StartNew(() =>
					{
						this.IsPosting = true;

						try
						{
							this.PostText = this.Model.ShortenUrls(_, this.PostText);
						}
						finally
						{
							this.IsPosting = false;
						}
					});
				});
			}
		}

		#endregion

		public IList<AccountTokenViewModel> Accounts
		{
			get
			{
				return GetValue(() => this.Accounts);
			}
			private set
			{
				SetValue(() => this.Accounts, value);
			}
		}

		public StatusCache StatusCache
		{
			get
			{
				return this.Model.StatusCache;
			}
		}

		public StatusesListBoxCommandHandler StatusesListBoxCommandHandler
		{
			get;
			private set;
		}

		public RateLimit RateLimit
		{
			get
			{
				return this.Model.RateLimits.Any() ? this.Model.RateLimits.FirstOrDefault() : default(RateLimit);
			}
		}

		public IList<RateLimitViewModel> RateLimits
		{
			get
			{
				return GetValue(() => this.RateLimits);
			}
			private set
			{
				SetValue(() => this.RateLimits, value);
			}
		}

		public bool IsPosting
		{
			get
			{
				return GetValue(() => this.IsPosting);
			}
			set
			{
				SetValue(() => this.IsPosting, value);
			}
		}

		public string PostText
		{
			get
			{
				return GetValue(() => this.PostText);
			}
			set
			{
				if (this.PostText != value)
				{
					SetValue(() => this.PostText, value);
					RequestFocusEndPostTextBox.DispatchEvent(this, EventArgs.Empty);
				}
				else
					OnPropertyChanged("PostText");
			}
		}

		public AccountTokenViewModel PostAccount
		{
			get
			{
				return this.Accounts.FirstOrDefault(_ => _.IsSelected)
					?? this.Accounts.FirstOrDefault();
			}
			set
			{
				this.Accounts.Run(_ => _.IsSelected = true);
				this.Accounts.Run(_ => _.IsSelected = _ == value);
				OnPropertyChanged("PostAccount");
			}
		}

		public string DirectMessageDestination
		{
			get
			{
				return GetValue(() => this.DirectMessageDestination);
			}
			set
			{
				SetValue(() => this.DirectMessageDestination, value);
			}
		}

		public IEnumerable<string> FollowerNames
		{
			get
			{
				return GetValue(() => this.FollowerNames);
			}
			private set
			{
				SetValue(() => this.FollowerNames, value);
			}
		}

		public bool IsDirectMessage
		{
			get
			{
				return GetValue(() => this.IsDirectMessage);
			}
			private set
			{
				SetValue(() => this.IsDirectMessage, value);

				if (value)
				{
					this.InReplyToStatus = null;

					Task.Factory.StartNew(() =>
					{
						try
						{
							using (new ReduceAuthenticatedQueryScope())
							using (var client = new TwitterClient(this.PostAccount.InternalAccount, this.Model.StatusCache))
								this.FollowerNames = client.Statuses.Followers(this.PostAccount.InternalAccount.Name).SelectMany().Select(_ => _.UserName).Freeze();
						}
						catch (Exception ex)
						{
							this.FollowerNames = new[] { ex.Message, };
							App.Log(ex);
						}
					});
				}
				else
					this.DirectMessageDestination = null;
			}
		}

		public bool IsUserStreamEnabled
		{
			get
			{
				return Settings.Default.Connection.EnableUserStreams;
			}
			set
			{
				Settings.Default.Connection.EnableUserStreams = value;
				OnPropertyChanged("IsUserStreamEnabled");
			}
		}

		public bool IsUserStreamRunning
		{
			get
			{
				return !this.Model.BypassStreams;
			}
			set
			{
				this.Model.BypassStreams = !value;
				OnPropertyChanged("IsUserStreamRunning");
			}
		}

		public Status InReplyToStatus
		{
			get
			{
				return GetValue(() => this.InReplyToStatus);
			}
			private set
			{
				SetValue(() => this.InReplyToStatus, value);

				if (value == null)
					this.PostText = null;
				else
					this.PostText = "@" + value.UserName + " ";

				if (value != null)
					this.IsDirectMessage = false;
			}
		}

		public ProgressBlock CurrentProgress
		{
			get
			{
				return GetValue(() => ((IProgressHost)this).CurrentProgress);
			}
			set
			{
				SetValue(() => ((IProgressHost)this).CurrentProgress, value);
			}
		}

		public IEnumerable<AccountToken> AccountsExceptSingle
		{
			get
			{
				return GetValue(() => this.AccountsExceptSingle);
			}
			set
			{
				SetValue(() => this.AccountsExceptSingle, value);
			}
		}

		public bool IsMultiAccount
		{
			get
			{
				return this.Model.Accounts != null && this.Model.Accounts.Count > 1;
			}
		}

		ClientParameters CreateClientParameters()
		{
			return SetClientParametersEventHandler(new ClientParameters(this.Model.Accounts, this.Model.StatusCache, this.StatusesListBoxCommandHandler));
		}

		ClientParameters CreateClientParameters(AccountToken singleAccount)
		{
			return SetClientParametersEventHandler(new ClientParameters(EnumerableEx.Wrap(singleAccount), this.Model.StatusCache, this.StatusesListBoxCommandHandler));
		}

		ClientParameters SetClientParametersEventHandler(ClientParameters cp)
		{
			cp.RequestAddCategory += (sender, e) => this.Model.Groups.Last().Add(e.Value);

			return cp;
		}

		public void Run(bool isDesignMode)
		{
			this.Model.IsDesignMode = isDesignMode;
			this.Model.Run();
			this.PostAccount = this.Accounts.FirstOrDefault();
			this.AccountsExceptSingle = this.Model.Accounts.Count <= 1 ? null : this.Model.Accounts;
			OnPropertyChanged("IsMultiAccount");

			this.Model.Accounts.CollectionChanged += (sender, e) =>
			{
				this.AccountsExceptSingle = this.Model.Accounts.Count <= 1 ? null : this.Model.Accounts;

				if (this.Accounts.Contains(this.PostAccount))
					this.PostAccount = this.PostAccount;
				else
					this.PostAccount = this.Accounts.FirstOrDefault();

				OnPropertyChanged("IsMultiAccount");
			};
		}

		public void Closed()
		{
			this.Model.Dispose();
		}

		public void RequestNewPage(Category category)
		{
			if (!category.IsRequestingNewPage)
				this.Model.RequestNewPage(category);
		}

		public class AccountTokenViewModel : ViewModel<AccountToken>
		{
			readonly MainWindowViewModel viewModel;

			public bool IsSelected
			{
				get
				{
					return GetValue(() => this.IsSelected);
				}
				set
				{
					SetValue(() => this.IsSelected, value);

					if (viewModel.Accounts.Any() &&
						viewModel.Accounts.All(_ => !_.IsSelected))
						viewModel.Accounts.First().IsSelected = true;

					viewModel.OnPropertyChanged("PostAccount");
				}
			}

			public AccountToken InternalAccount
			{
				get
				{
					return this.Model;
				}
			}

			public AccountTokenViewModel(MainWindowViewModel viewModel, AccountToken account)
				: base(account)
			{
				this.viewModel = viewModel;
			}

			public override string ToString()
			{
				return this.Model.ToString();
			}
		}

		public class RateLimitViewModel : ViewModel<RateLimit>
		{
			public RateLimit InternalRateLimit
			{
				get
				{
					return this.Model;
				}
			}

			public bool IsStreaming
			{
				get
				{
					return GetValue(() => this.IsStreaming);
				}
				set
				{
					SetValue(() => this.IsStreaming, value);
				}
			}

			public RateLimitViewModel(RateLimit internalRateLimit)
				: base(internalRateLimit)
			{
			}
		}
	}

#pragma warning restore 1591
}
