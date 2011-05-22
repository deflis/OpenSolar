using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Ignition;
using Ignition.Presentation;
using Lunar;
using Microsoft.Win32;
using Solar.Filtering;
using Solar.Models;

namespace Solar.Dialogs
{
#pragma warning disable 1591

	/// <summary>
	/// LayoutWindow.xaml の相互作用ロジック
	/// </summary>
	public partial class LayoutWindow : Window
	{
		ICommand dropCommand;
		Point lastDragPoint;

		LayoutWindowViewModel ViewModel
		{
			get
			{
				return (LayoutWindowViewModel)this.DataContext;
			}
		}

		public IList<CategoryGroup> CategoryGroups
		{
			get
			{
				return this.ViewModel.TabGroups.Select(_ => (CategoryGroup)_).Freeze();
			}
		}

		public IList<FilterTerms> GlobalTerms
		{
			get
			{
				return this.ViewModel.GlobalTerms.Select(_ => _.Value).Freeze();
			}
		}

		public LayoutWindow(IEnumerable<FilterTerms> globalTerms, IEnumerable<CategoryGroup> groups, IEnumerable<AccountToken> accounts)
		{
			InitializeComponent();

			dropCommand = this.ViewModel.DropCommand;
			this.ViewModel.RequestClose += (sender, e) => this.DialogResult = e.Value;
			this.ViewModel.Apply(globalTerms, groups, accounts);
		}

		public LayoutWindow(IEnumerable<FilterTerms> globalTerms, IEnumerable<CategoryGroup> groups, IEnumerable<AccountToken> accounts, Category selected)
			: this(globalTerms, groups, accounts)
		{
			if (selected != null)
				this.Loaded += (sender, e) =>
				{
					LayoutWindowViewModel.Tab tab = null;
					var tabGroup = this.ViewModel.TabGroups.Single(_ => _.Any(__ =>
					{
						if ((Category)__ == selected)
						{
							tab = __;

							return true;
						}
						else
							return false;
					}));
					var groupTreeViewItem = (TreeViewItem)tabsTreeView.ItemContainerGenerator.ContainerFromItem(tabGroup);
					var treeViewItem = (TreeViewItem)groupTreeViewItem.ItemContainerGenerator.ContainerFromItem(tab);

					treeViewItem.IsSelected = true;
				};
		}

		void tabsTreeView_PreviewMouseDown(object sender, MouseButtonEventArgs e)
		{
			var pos = e.GetPosition(tabsTreeView);

			if (pos.X < tabsTreeView.ActualWidth - SystemParameters.VerticalScrollBarWidth &&
				pos.Y < tabsTreeView.ActualHeight - SystemParameters.HorizontalScrollBarHeight)
				lastDragPoint = pos;
		}

		void tabsTreeView_MouseMove(object sender, MouseEventArgs e)
		{
			var pos = e.GetPosition(tabsTreeView);

			if (pos.X < tabsTreeView.ActualWidth - SystemParameters.VerticalScrollBarWidth &&
				pos.Y < tabsTreeView.ActualHeight - SystemParameters.HorizontalScrollBarHeight)
			{
				var diff = lastDragPoint - pos;

				if (e.LeftButton == MouseButtonState.Pressed &&
					Math.Abs(diff.X) > SystemParameters.MinimumHorizontalDragDistance / 2 &&
					Math.Abs(diff.Y) > SystemParameters.MinimumVerticalDragDistance / 2)
					DragDrop.DoDragDrop(tabsTreeView, tabsTreeView.SelectedItem, DragDropEffects.Move);
			}
		}

		void tabsTreeView_DragOver(object sender, DragEventArgs e)
		{
			e.Effects = dropCommand.CanExecute(new
			{
				e.Data,
				Target = GetItemAtLocation(e.GetPosition(tabsTreeView))
			}) ? DragDropEffects.Move : DragDropEffects.None;
			e.Handled = true;
		}

		void tabsTreeView_Drop(object sender, DragEventArgs e)
		{
			var p = new
			{
				e.Data,
				Target = GetItemAtLocation(e.GetPosition(tabsTreeView))
			};

			if (dropCommand.CanExecute(p))
			{
				dropCommand.Execute(p);
				e.Handled = true;
			}
		}

		object GetItemAtLocation(Point location)
		{
			var hitTestResults = VisualTreeHelper.HitTest(tabsTreeView, location);

			if (hitTestResults.VisualHit is FrameworkElement)
				return (hitTestResults.VisualHit as FrameworkElement).DataContext;

			return null;
		}

		void notifySoundButton_Click(object sender, RoutedEventArgs e)
		{
			var f = new OpenFileDialog
			{
				Filter = "サウンド ファイル (*.wav)|*.wav",
			};

			if (f.ShowDialog(this) == true)
			{
				notifySoundTextBox.Text = f.FileName;
				notifySoundTextBox.GetBindingExpression(TextBox.TextProperty).UpdateSource();
			}
		}
	}

	public class LayoutWindowViewModel : NotifyObject
	{
		public event EventHandler<EventArgs<bool?>> RequestClose;

		#region Commands

		public ICommand AcceptCommand
		{
			get
			{
				return new RelayCommand(_ => RequestClose.RaiseEvent(this, new EventArgs<bool?>(true)));
			}
		}

		public ICommand CancelCommand
		{
			get
			{
				return new RelayCommand(_ => RequestClose.RaiseEvent(this, new EventArgs<bool?>(false)));
			}
		}

		public ICommand AddGroupCommand
		{
			get
			{
				return new RelayCommand(value => value.TypeMatch
				(
					(TabGroup _) => this.TabGroups.Add(new TabGroup()),
					(Tab _) => FindGroup(_).Add(new Tab())
				));
			}
		}

		public ICommand RemoveGroupCommand
		{
			get
			{
				return new RelayCommand(value => value.TypeMatch
				(
					(TabGroup _) => this.TabGroups.Count > 1,
					(Tab _) => FindGroup(_).Count > 1
				),
				value => value.TypeMatch
				(
					(TabGroup _) => this.TabGroups.Remove(_),
					(Tab _) => FindGroup(_).Remove(_)
				));
			}
		}

		public ICommand AddGlobalTermsCommand
		{
			get
			{
				return new RelayCommand<Wrapper<FilterTerms>>(_ => this.GlobalTerms.Insert(this.GlobalTerms.IndexOf(_) + 1, new Wrapper<FilterTerms>(new ContainsFilterTerms())));
			}
		}

		public ICommand RemoveGlobalTermsCommand
		{
			get
			{
				return new RelayCommand<Wrapper<FilterTerms>>(_ => this.GlobalTerms.Count > 1, _ => this.GlobalTerms.Remove(_));
			}
		}

		public ICommand DropCommand
		{
			get
			{
				return new RelayCommand<dynamic>(_ => _.Data.GetDataPresent(typeof(Tab)) || _.Data.GetDataPresent(typeof(TabGroup)) && !(_.Target is Tab),
				_ =>
				{
					if (_.Data.GetDataPresent(typeof(Tab)))
					{
						Tab tab = _.Data.GetData(typeof(Tab));
						var target = _.Target;

						if (target is Tab &&
							target == tab)
							return;

						var lastGroup = FindGroup(tab);

						if (lastGroup.Count == 1)
							return;

						lastGroup.Remove(tab);

						if (target is TabGroup)
							target.Add(tab);
						else if (target is Tab)
						{
							Tab targetTab = target;
							var group = FindGroup(targetTab);

							group.Insert(group.IndexOf(targetTab), tab);
						}
						else
							lastGroup.Add(tab);
					}
					else if (_.Data.GetDataPresent(typeof(TabGroup)))
					{
						TabGroup tabGroup = _.Data.GetData(typeof(TabGroup));
						var target = _.Target;

						if (target == tabGroup)
							return;

						this.TabGroups.Remove(tabGroup);

						if (target is TabGroup)
							this.TabGroups.Insert(this.TabGroups.IndexOf(target), tabGroup);
						else
							this.TabGroups.Add(tabGroup);
					}
				});
			}
		}

		#endregion

		public NotifyCollection<TabGroup> TabGroups
		{
			get;
			private set;
		}

		public List<AccountToken> Accounts
		{
			get;
			private set;
		}

		public NotifyCollection<Wrapper<FilterTerms>> GlobalTerms
		{
			get;
			private set;
		}

		public LayoutWindowViewModel()
		{
			this.TabGroups = new NotifyCollection<TabGroup>();
			this.Accounts = new List<AccountToken>
			{
				new AccountToken(),
			};
			this.GlobalTerms = new NotifyCollection<Wrapper<FilterTerms>>();
		}

		public void Apply(IEnumerable<FilterTerms> globalTerms, IEnumerable<CategoryGroup> groups, IEnumerable<AccountToken> accounts)
		{
			this.TabGroups.AddRange(groups.Select(_ => new TabGroup(_)));
			this.Accounts.AddRange(accounts);
			this.GlobalTerms.AddRange(globalTerms.Select(_ => new Wrapper<FilterTerms>(_)));

			if (!this.GlobalTerms.Any())
				this.GlobalTerms.Add(new Wrapper<FilterTerms>(new NothingFilterTerms()));
		}

		TabGroup FindGroup(Tab tab)
		{
			return this.TabGroups.Single(_ => _.Contains(tab));
		}

		public class TabGroup : ObservableCollection<Tab>
		{
			public TabGroup()
			{
				this.Add(new Tab());
			}

			public TabGroup(CategoryGroup group)
				: base(group.Select(_ => new Tab(_)))
			{
			}

			public static explicit operator CategoryGroup(TabGroup self)
			{
				return new CategoryGroup(self.Select(_ => (Category)_));
			}
		}

		public class Wrapper<T> : NotifyObject
		{
			public Type Type
			{
				get
				{
					return this.Value.GetType();
				}
				set
				{
					if (this.Value.GetType() == value ||
						!value.IsSubclassOf(typeof(T)))
						return;

					this.Value = (T)Activator.CreateInstance(value);
				}
			}

			public T Value
			{
				get
				{
					return GetValue(() => this.Value);
				}
				private set
				{
					SetValue(() => this.Value, value);
				}
			}

			public Wrapper(T value)
			{
				this.Value = value;
			}
		}

		public class Tab : NotifyObject
		{
			Category category;

			#region Commands

			public ICommand AddSourceCommand
			{
				get
				{
					return new RelayCommand<Wrapper<FilterSource>>(_ => this.Sources.Insert(this.Sources.IndexOf(_) + 1, new Wrapper<FilterSource>(new HomeFilterSource())));
				}
			}

			public ICommand RemoveSourceCommand
			{
				get
				{
					return new RelayCommand<Wrapper<FilterSource>>(_ => this.Sources.Count > 1, _ => this.Sources.Remove(_));
				}
			}

			public ICommand AddTermsCommand
			{
				get
				{
					return new RelayCommand<Wrapper<FilterTerms>>(_ => this.Terms.Insert(this.Terms.IndexOf(_) + 1, new Wrapper<FilterTerms>(new ContainsFilterTerms())));
				}
			}

			public ICommand RemoveTermsCommand
			{
				get
				{
					return new RelayCommand<Wrapper<FilterTerms>>(_ => this.Terms.Count > 1, _ => this.Terms.Remove(_));
				}
			}

			#endregion

			public ObservableCollection<Wrapper<FilterSource>> Sources
			{
				get;
				private set;
			}

			public ObservableCollection<Wrapper<FilterTerms>> Terms
			{
				get;
				private set;
			}

			public string Name
			{
				get
				{
					return category.Name;
				}
				set
				{
					category.Name = value;
					OnPropertyChanged("Name");
				}
			}

			public bool NotifyUpdates
			{
				get
				{
					return category.NotifyUpdates;
				}
				set
				{
					category.NotifyUpdates = value;
					OnPropertyChanged("NotifyUpdates");
				}
			}

			public string NotifySound
			{
				get
				{
					return category.NotifySound;
				}
				set
				{
					category.NotifySound = value;
					OnPropertyChanged("NotifySound");
				}
			}

			public bool CheckUnreads
			{
				get
				{
					return category.CheckUnreads;
				}
				set
				{
					category.CheckUnreads = value;
					OnPropertyChanged("CheckUnreads");
				}
			}

			public double Interval
			{
				get
				{
					return category.Interval.TotalMinutes;
				}
				set
				{
					category.Interval = TimeSpan.FromMinutes(value);
					OnPropertyChanged("Interval");
				}
			}

			public Tab()
				: this(new Category
				{
					Name = "名称未設定",
					NotifyUpdates = true,
					CheckUnreads = true,
					NotifySound = null,
					Filter =
					{
						Sources =
						{
							new HomeFilterSource(),
						},
						Terms =
						{
							new NothingFilterTerms(),
						},
					},
				})
			{
			}

			public Tab(Category category)
			{
				this.category = category;
				this.Sources = new ObservableCollection<Wrapper<FilterSource>>(category.Filter.Sources.Select(_ => new Wrapper<FilterSource>(_)));
				this.Terms = new ObservableCollection<Wrapper<FilterTerms>>(category.Filter.Terms.Select(_ => new Wrapper<FilterTerms>(_)));

				if (!this.Sources.Any())
					this.Sources.Add(new Wrapper<FilterSource>(new HomeFilterSource()));

				if (!this.Terms.Any())
					this.Terms.Add(new Wrapper<FilterTerms>(new NothingFilterTerms()));
			}

			public static explicit operator Category(Tab self)
			{
				self.category.Filter.Sources = new NotifyCollection<FilterSource>(self.Sources.Select(_ => _.Value));
				self.category.Filter.Terms = new NotifyCollection<FilterTerms>(self.Terms.Select(_ => _.Value));

				return self.category;
			}
		}
	}

#pragma warning restore 1591
}
