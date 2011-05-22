using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using Ignition;
using Ignition.Presentation;
using Lunar;

namespace Solar
{
#pragma warning disable 1591

	/// <summary>
	/// FollowManagerWindow.xaml の相互作用ロジック
	/// </summary>
	public partial class FollowManagerWindow : Window
	{
		FilterEventHandler oldTwoWayFilter;
		FilterEventHandler oldFollowingFilter;
		FilterEventHandler oldFollowersFilter;

		FollowManagerWindowViewModel ViewModel
		{
			get
			{
				return (FollowManagerWindowViewModel)this.DataContext;
			}
		}

		internal FollowManagerWindow(StatusesListBoxCommandHandler commandHandler, ClientParameters cp)
		{
			InitializeComponent();

			this.Title += " (" + cp.Accounts.Single() + ")";
			this.ViewModel.RequestMessageBox += (sender, e) => e.Value(this);
			this.ViewModel.Apply(commandHandler, cp);
		}

		void Button_Click(object sender, RoutedEventArgs e)
		{
			this.Close();
		}

		void TwoWayViewSource_Filter(object sender, FilterEventArgs e)
		{
			e.Accepted = ((FollowManagerWindowViewModel.Follow)e.Item).FollowState == FollowManagerWindowViewModel.FollowState.TwoWay;
		}

		void FollowersViewSource_Filter(object sender, FilterEventArgs e)
		{
			e.Accepted = ((FollowManagerWindowViewModel.Follow)e.Item).FollowState == FollowManagerWindowViewModel.FollowState.Follower;
		}

		void FollowingViewSource_Filter(object sender, FilterEventArgs e)
		{
			e.Accepted = ((FollowManagerWindowViewModel.Follow)e.Item).FollowState == FollowManagerWindowViewModel.FollowState.Following;
		}

		void twoWaySearchBox_TextChanged(object sender, TextChangedEventArgs e)
		{
			UpdateIncrementalSearch((TextBox)sender, (CollectionViewSource)this.Resources["TwoWayViewSource"], ref oldTwoWayFilter);
			e.Handled = true;
		}

		void followingSearchBox_TextChanged(object sender, TextChangedEventArgs e)
		{
			UpdateIncrementalSearch((TextBox)sender, (CollectionViewSource)this.Resources["FollowingViewSource"], ref oldFollowingFilter);
			e.Handled = true;
		}

		void followersSearchBox_TextChanged(object sender, TextChangedEventArgs e)
		{
			UpdateIncrementalSearch((TextBox)sender, (CollectionViewSource)this.Resources["FollowersViewSource"], ref oldFollowersFilter);
			e.Handled = true;
		}

		void UpdateIncrementalSearch(TextBox textBox, CollectionViewSource collectionViewSource, ref FilterEventHandler oldFilter)
		{
			var text = textBox.Text.Split(' ', ' ');

			using (collectionViewSource.DeferRefresh())
			{
				FilterEventHandler newFilter = (sender2, e2) =>
					e2.Accepted = e2.Accepted && (!text.Any() || e2.Item.TypeMatch
					(
						(FollowManagerWindowViewModel.Follow _) => new[] { _.User.Name, _.User.FullName, _.User.Description },
						(User _) => new[] { _.Name, _.FullName, _.Description },
						_ => Enumerable.Empty<string>()
					)
					.Where(_ => _ != null)
					.Any(_ => text.All(_.Contains)));

				collectionViewSource.Filter += newFilter;

				if (oldFilter != null)
					collectionViewSource.Filter -= oldFilter;

				oldFilter = newFilter;
			}
		}
	}

	public class FollowManagerWindowViewModel : NotifyObject
	{
		public event EventHandler<EventArgs<Action<Window>>> RequestMessageBox;

		ClientParameters cp;

		#region Commands

		public ICommand FollowCommand
		{
			get
			{
				return new RelayCommand<Follow>(_ =>
				{
					this.Follows.Remove(_);

					if (_.FollowState == FollowState.Follower)
						this.Follows.Add(new Follow(FollowState.TwoWay, _.User));
					else
						this.Follows.Add(new Follow(FollowState.Following, _.User));

					Task.Factory.StartNew(() =>
					{
						try
						{
							using (var client = new TwitterClient(cp.Accounts.Single(), cp.StatusCache))
								client.Friendships.Create(_.User.UserID);
						}
						catch (ContentedWebException ex)
						{
							RequestMessageBox.DispatchEvent(this, new EventArgs<Action<Window>>(window => MessageBoxEx.Show(window, ex.Message, "送信中にエラーが発生しました", MessageBoxButton.OK, MessageBoxImage.Warning)));
						}
					});
				});
			}
		}

		public ICommand UnfollowCommand
		{
			get
			{
				return new RelayCommand<Follow>(_ =>
				{
					this.Follows.Remove(_);

					if (_.FollowState == FollowState.TwoWay)
						this.Follows.Add(new Follow(FollowState.Follower, _.User));

					Task.Factory.StartNew(() =>
					{
						try
						{
							using (var client = new TwitterClient(cp.Accounts.Single(), cp.StatusCache))
								client.Friendships.Destroy(_.User.UserID);
						}
						catch (ContentedWebException ex)
						{
							RequestMessageBox.DispatchEvent(this, new EventArgs<Action<Window>>(window => MessageBoxEx.Show(window, ex.Message, "送信中にエラーが発生しました", MessageBoxButton.OK, MessageBoxImage.Warning)));
						}
					});
				});
			}
		}

		public ICommand BlockCommand
		{
			get
			{
				return new RelayCommand<Follow>(_ =>
				{
					this.Follows.Remove(_);
					this.Blocks.Add(_.User);

					Task.Factory.StartNew(() =>
					{
						try
						{
							using (var client = new TwitterClient(cp.Accounts.Single(), cp.StatusCache))
								client.Blocks.Create(_.User.UserID);
						}
						catch (ContentedWebException ex)
						{
							RequestMessageBox.DispatchEvent(this, new EventArgs<Action<Window>>(window => MessageBoxEx.Show(window, ex.Message, "送信中にエラーが発生しました", MessageBoxButton.OK, MessageBoxImage.Warning)));
						}
					});
				});
			}
		}

		public ICommand UnblockCommand
		{
			get
			{
				return new RelayCommand<User>(_ =>
				{
					this.Blocks.Remove(_);

					Task.Factory.StartNew(() =>
					{
						try
						{
							using (var client = new TwitterClient(cp.Accounts.Single(), cp.StatusCache))
								client.Blocks.Destroy(_.UserID);
						}
						catch (ContentedWebException ex)
						{
							RequestMessageBox.DispatchEvent(this, new EventArgs<Action<Window>>(window => MessageBoxEx.Show(window, ex.Message, "送信中にエラーが発生しました", MessageBoxButton.OK, MessageBoxImage.Warning)));
						}
					});
				});
			}
		}

		public ICommand RefreshFollows
		{
			get
			{
				return new RelayCommand(_ => BeginRefreshFollows());
			}
		}

		public ICommand RefreshBlocks
		{
			get
			{
				return new RelayCommand(_ => BeginRefreshBlocks());
			}
		}

		#endregion

		public StatusesListBoxCommandHandler CommandHandler
		{
			get;
			private set;
		}

		public ObservableCollection<Follow> Follows
		{
			get;
			private set;
		}

		public ObservableCollection<User> Blocks
		{
			get;
			private set;
		}

		public bool IsLoadingFollows
		{
			get
			{
				return GetValue(() => this.IsLoadingFollows);
			}
			private set
			{
				SetValue(() => this.IsLoadingFollows, value);
			}
		}

		public bool IsLoadingBlocks
		{
			get
			{
				return GetValue(() => this.IsLoadingBlocks);
			}
			private set
			{
				SetValue(() => this.IsLoadingBlocks, value);
			}
		}

		public bool HasFollowsError
		{
			get
			{
				return GetValue(() => this.HasFollowsError);
			}
			private set
			{
				SetValue(() => this.HasFollowsError, value);
			}
		}

		public Exception FollowsError
		{
			get
			{
				return GetValue(() => this.FollowsError);
			}
			private set
			{
				SetValue(() => this.FollowsError, value);
			}
		}

		public bool HasBlocksError
		{
			get
			{
				return GetValue(() => this.HasBlocksError);
			}
			private set
			{
				SetValue(() => this.HasBlocksError, value);
			}
		}

		public Exception BlocksError
		{
			get
			{
				return GetValue(() => this.BlocksError);
			}
			private set
			{
				SetValue(() => this.BlocksError, value);
			}
		}

		public FollowManagerWindowViewModel()
		{
			this.Follows = new ObservableCollection<Follow>();
			this.Blocks = new ObservableCollection<User>();
		}

		internal void Apply(StatusesListBoxCommandHandler commandHandler, ClientParameters cp)
		{
			this.CommandHandler = commandHandler;
			this.cp = cp;

			BeginRefreshFollows();
			BeginRefreshBlocks();
		}

		Task BeginRefreshBlocks()
		{
			return Task.Factory.StartNew(() =>
			{
				this.IsLoadingBlocks = true;
				this.HasBlocksError = false;

				try
				{
					using (new ReduceAuthenticatedQueryScope())
						foreach (var i in cp.Accounts)
							using (var client = new TwitterClient(i, cp.StatusCache))
							{
								var blocks = client.Blocks.Blocking().Freeze();

								this.Blocks.AddRangeAsync(blocks.Select(_ => _.User));
							}
				}
				catch (Exception ex)
				{
					this.HasBlocksError = false;
					this.BlocksError = ex;
				}

				this.IsLoadingBlocks = false;
			});
		}

		Task BeginRefreshFollows()
		{
			return Task.Factory.StartNew(() =>
			{
				this.IsLoadingFollows = true;
				this.HasFollowsError = false;

				try
				{
					using (new ReduceAuthenticatedQueryScope())
						foreach (var i in cp.Accounts)
							using (var client = new TwitterClient(i, cp.StatusCache))
							{
								var followers = client.Statuses.Followers().SelectMany().Freeze();
								var friends = client.Statuses.Friends().SelectMany().Freeze();
								var eq = new InstantEqualityComparer<Status>(_ => _.UserID);

								this.Follows.AddRangeAsync(followers.Intersect(friends, eq).Select(_ => new Follow(FollowState.TwoWay, _.User)));
								this.Follows.AddRangeAsync(friends.Except(followers, eq).Select(_ => new Follow(FollowState.Following, _.User)));
								this.Follows.AddRangeAsync(followers.Except(friends, eq).Select(_ => new Follow(FollowState.Follower, _.User)));
							}

				}
				catch (Exception ex)
				{
					this.HasFollowsError = true;
					this.FollowsError = ex;
				}

				this.IsLoadingFollows = false;
			});
		}

		public class Follow
		{
			public FollowState FollowState
			{
				get;
				private set;
			}

			public bool IsFollowing
			{
				get
				{
					return this.FollowState == FollowState.TwoWay
						|| this.FollowState == FollowState.Following;
				}
			}

			public bool IsNotFollowing
			{
				get
				{
					return this.FollowState == FollowState.Follower;
				}
			}

			public User User
			{
				get;
				private set;
			}

			public Follow(FollowState followState, User user)
			{
				this.FollowState = followState;
				this.User = user;
			}
		}

		public enum FollowState
		{
			TwoWay,
			Following,
			Follower,
		}
	}
#pragma warning restore 1591
}
