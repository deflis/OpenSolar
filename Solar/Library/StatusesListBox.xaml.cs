using System;
using System.Linq;
using System.Linq.Dynamic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Ignition;
using Ignition.Presentation;
using Lunar;
using Solar.Models;

namespace Solar
{
#pragma warning disable 1591

	/// <summary>
	/// StatusesListBox.xaml の相互作用ロジック
	/// </summary>
	public partial class StatusesListBox : ListBox
	{
		TreeView subListBox;
		TextBox searchTextBox;
		ScrollViewer subScrollViewer;
		double verticalOffset;
		int count;
		readonly char[] searchPrefixes = new[] { ':', '/', '@' };
		public static readonly DependencyProperty CommandHandlerProperty = DependencyProperty.Register("CommandHandler", typeof(StatusesListBoxCommandHandler), typeof(StatusesListBox), new UIPropertyMetadata(null));
		public static readonly DependencyProperty CommandProperty = DependencyProperty.Register("Command", typeof(ICommand), typeof(StatusesListBox), new UIPropertyMetadata(null));
		public static readonly DependencyProperty NewCountProperty = DependencyProperty.Register("NewCount", typeof(int), typeof(StatusesListBox), new UIPropertyMetadata(0, NewCountChanged));
		public static readonly DependencyProperty SearchAvailableProperty = DependencyProperty.Register("SearchAvailable", typeof(bool), typeof(StatusesListBox), new UIPropertyMetadata(Settings.Default.Interface.UseQuickSearch));
		public static readonly RoutedEvent RequestNewPageEvent = EventManager.RegisterRoutedEvent("RequestNewPage", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(StatusesListBox));

		public event RoutedEventHandler RequestNewPage
		{
			add
			{
				AddHandler(RequestNewPageEvent, value);
			}
			remove
			{
				RemoveHandler(RequestNewPageEvent, value);
			}
		}

		public bool SearchAvailable
		{
			get
			{
				return (bool)GetValue(SearchAvailableProperty);
			}
			set
			{
				SetValue(SearchAvailableProperty, value);
			}
		}

		public int NewCount
		{
			get
			{
				return (int)GetValue(NewCountProperty);
			}
			set
			{
				SetValue(NewCountProperty, value);
			}
		}

		public StatusesListBoxCommandHandler CommandHandler
		{
			get
			{
				return (StatusesListBoxCommandHandler)GetValue(CommandHandlerProperty);
			}
			set
			{
				SetValue(CommandHandlerProperty, value);
			}
		}

		public ICommand Command
		{
			get
			{
				return (ICommand)GetValue(CommandProperty);
			}
			set
			{
				SetValue(CommandProperty, value);
			}
		}

		public ICommand SearchQuickCommand
		{
			get
			{
				return new RelayCommand(_ =>
				{
					if (searchTextBox != null)
						searchTextBox.Focus();
				});
			}
		}

		/// <summary>
		/// StatusesListBox の新しいインスタンスを初期化します。
		/// </summary>
		public StatusesListBox()
		{
			InitializeComponent();

			foreach (var i in typeof(KeySettings).GetProperties()
												 .Select(_ => new
												 {
													 _.Name,
													 Value = (KeyGesture)_.GetValue(Settings.Default.Key, null),
												 })
												 .Where(_ => this.Resources.Contains(_.Name + "Binding")))
				((KeyBinding)this.Resources[i.Name + "Binding"]).Gesture = i.Value ?? new KeyGesture(Key.None);

			foreach (var i in typeof(StatusesListBoxCommandHandler).GetProperties()
																   .Where(_ => _.PropertyType == typeof(ICommand))
																   .Select(_ => new
																   {
																	   _.Name,
																	   Redirect = new CommandRedirect(() => this.CommandHandler == null ? null : (ICommand)_.GetValue(this.CommandHandler, null)),
																   }))
				this.Resources[i.Name] = i.Redirect;
		}

		static void NewCountChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			var self = ((StatusesListBox)d).subListBox;

			if (self != null)
			{
				var l = Math.Min(Math.Max((int)e.OldValue, (int)e.NewValue), self.Items.Count);

				for (int i = 0; i < l; i++)
				{
					var item = (TreeViewItem)self.ItemContainerGenerator.ContainerFromIndex(i);

					if (item != null)
						item.Style = self.ItemContainerStyleSelector.SelectStyle(self.ItemContainerGenerator.ItemFromContainer(item), item);
				}
			}
		}

		T FindAncestor<T>(FrameworkElement element)
			where T : FrameworkElement
		{
			do
				element = (FrameworkElement)VisualTreeHelper.GetParent(element);
			while (element != null && !(element is T));

			return (T)element;
		}

		void ListBoxItem_MouseDoubleClick(dynamic sender, MouseButtonEventArgs e)
		{
			Task.Factory.StartNew(() => this.Dispatcher.BeginInvoke((Action)delegate
			{
				if (this.Command != null && this.Command.CanExecute(this.SelectedItem))
					this.Command.Execute(this.SelectedItem);
			}));
		}

		void ListBox_ScrollChanged(object sender, ScrollChangedEventArgs e)
		{
			if (e.VerticalOffset != 0.0 &&
				e.VerticalOffset >= e.ExtentHeight - e.ViewportHeight)
				RaiseEvent(new RoutedEventArgs(RequestNewPageEvent, this));
		}

		void TreeView_Loaded(object sender, RoutedEventArgs e)
		{
			subListBox = ((TreeView)sender);
			subScrollViewer = subListBox.FindDescendant<ScrollViewer>().First();
			subListBox.ItemContainerGenerator.ItemsChanged += (sender2, e2) => UpdateStatusStyles(subListBox);

			Category.Updating += CategoryUpdating;
			Category.Updated += CategoryUpdated;
		}

		void ListBox_Unloaded(object sender, RoutedEventArgs e)
		{
			Category.Updating -= CategoryUpdating;
			Category.Updated -= CategoryUpdated;
		}

		void CategoryUpdating(object sender, EventArgs<Category> e)
		{
			if (e.Value.Statuses == this.ItemsSource &&
				subScrollViewer != null)
			{
				verticalOffset = subScrollViewer.VerticalOffset == 0 ? 0 : subScrollViewer.ExtentHeight - subScrollViewer.VerticalOffset;
				count = e.Value.Statuses.Count;
			}
		}

		void CategoryUpdated(object sender, EventArgs<Category> e)
		{
			if (e.Value.Statuses == this.ItemsSource &&
				subScrollViewer != null &&
				verticalOffset > 0)
				this.Dispatcher.BeginInvoke((Action)(() => subScrollViewer.ScrollToVerticalOffset(subScrollViewer.ExtentHeight - verticalOffset + Enumerable.Range(0, Math.Max(0, e.Value.Statuses.Count - count)).Select(_ => subListBox.ItemContainerGenerator.ContainerFromIndex(_ + 1) as TreeViewItem).Sum(_ => _ == null ? 16 : _.ActualHeight))));
		}

		void TreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
		{
			this.SelectedItem = ((TreeView)sender).SelectedItem;
			UpdateStatusStyles((TreeView)sender);
		}

		void UpdateStatusStyles(TreeView self)
		{
			lock (self)
				for (int i = 0; i < self.Items.Count; i++)
					try
					{
						var item = (TreeViewItem)self.ItemContainerGenerator.ContainerFromIndex(i);

						if (item != null)
							item.Style = self.ItemContainerStyleSelector.SelectStyle(self.ItemContainerGenerator.ItemFromContainer(item), item);
					}
					catch (NullReferenceException)
					{
					}
		}

		void TextBox_TextChanged(object sender, TextChangedEventArgs e)
		{
			if (Settings.Default.Interface.UseIncrementalQuickSearch && !searchPrefixes.Contains(((TextBox)sender).Text.FirstOrDefault()))
			{
				UpdateSearch(sender);
				e.Handled = true;
			}
		}

		void TextBox_PreviewKeyDown(object sender, KeyEventArgs e)
		{
			if (e.Key == Key.Enter)
			{
				var text = ((TextBox)sender).Text;

				if (e.KeyboardDevice.Modifiers != ModifierKeys.Shift)
				{
					if (!Settings.Default.Interface.UseIncrementalQuickSearch ||
						searchPrefixes.Contains(((TextBox)sender).Text.FirstOrDefault()))
					{
						if (text.StartsWith(":") || e.KeyboardDevice.Modifiers == ModifierKeys.Control)
							UpdateDynamicSearch(sender);
						else if (text.StartsWith("/"))
							UpdateRegexSearch(sender);
						else if (text.StartsWith("@"))
							UpdateUserSearch(sender);
						else
							UpdateSearch(sender);
					}

					e.Handled = true;
				}
			}
		}

		void TextBox_Loaded(dynamic sender, RoutedEventArgs e)
		{
			searchTextBox = sender;
		}

		void UpdateUserSearch(object sender)
		{
			if (subListBox == null)
				return;

			try
			{
				var text = ((TextBox)sender).Text.TrimStart(searchPrefixes);

				subListBox.Items.Filter = value => text.Any() ? value.TypeMatch
				(
					(IEntry _) => _.UserName != null && _.UserName.StartsWith(text)
							   || _.Text != null && text.Contains("@" + text),
					_ => true
				) : true;
			}
			catch (Exception ex)
			{
				App.Log(ex);
			}
		}

		void UpdateRegexSearch(object sender)
		{
			if (subListBox == null)
				return;

			try
			{
				var text = ((TextBox)sender).Text.Trim('/').Split(new[] { '　', ' ', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries).Select(_ => new Regex(_, RegexOptions.Compiled)).Freeze();

				subListBox.Items.Filter = value => text.Any() ? value.TypeMatch
				(
					(Status _) => _.UserName != null && text.All(r => r.IsMatch(_.UserName))
							   || _.FullUserName != null && text.All(r => r.IsMatch(_.FullUserName))
							   || _.Text != null && text.All(r => r.IsMatch(_.Text)),
					(IEntry _) => _.UserName != null && text.All(r => r.IsMatch(_.UserName))
							   || _.Text != null && text.All(r => r.IsMatch(_.Text)),
					_ => true
				) : true;
			}
			catch (Exception ex)
			{
				App.Log(ex);
			}
		}

		void UpdateSearch(object sender)
		{
			if (subListBox == null)
				return;

			var text = ((TextBox)sender).Text.Split(new[] { '　', ' ', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

			subListBox.Items.Filter = value => text.Any() ? value.TypeMatch
			(
				(Status _) => _.UserName != null && text.All(_.UserName.Contains)
						   || _.FullUserName != null && text.All(_.FullUserName.Contains)
						   || _.Text != null && text.All(_.Text.Contains),
				(IEntry _) => _.UserName != null && text.All(_.UserName.Contains)
						   || _.Text != null && text.All(_.Text.Contains),
				_ => true
			) : true;
		}

		void UpdateDynamicSearch(object sender)
		{
			if (subListBox == null)
				return;

			try
			{
				var text = ((TextBox)sender).Text.TrimStart(searchPrefixes);
				var predicate = DynamicExpression.ParseLambda<Status, bool>(text).Compile();

				subListBox.Items.Filter = value => string.IsNullOrEmpty(text) ? true : value.TypeMatch
				(
					predicate,
					_ => false
				);
			}
			catch (Exception ex)
			{
				MessageBoxEx.Show(Application.Current.MainWindow, ex.Message, ex.GetType().FullName, MessageBoxButton.OK, MessageBoxImage.Warning);
			}
		}

		class CommandRedirect : ICommand
		{
			Func<ICommand> getCommand;

			public CommandRedirect(Func<ICommand> getCommand)
			{
				this.getCommand = getCommand;
			}

			public bool CanExecute(object parameter)
			{
				var command = getCommand();

				if (command != null)
					return command.CanExecute(parameter);
				else
					return true;
			}

			public event EventHandler CanExecuteChanged
			{
				add
				{
					var command = getCommand();

					if (command != null)
						command.CanExecuteChanged += value;
				}
				remove
				{
					var command = getCommand();

					if (command != null)
						command.CanExecuteChanged -= value;
				}
			}

			public void Execute(object parameter)
			{
				var command = getCommand();

				if (command != null)
					command.Execute(parameter);
			}
		}
	}

	class StatusesListBoxStyleSelector : StyleSelector
	{
		public override Style SelectStyle(object item, DependencyObject container)
		{
			var obj = container;

			while (!(obj is StatusesListBox) && obj != null)
				obj = VisualTreeHelper.GetParent(obj);

			var listBox = (StatusesListBox)obj;
			var status = item as Status;
			var selected = listBox.SelectedItem as Status;

			var str = listBox.Items.IndexOf(item) < listBox.NewCount
					? "NewItemStyle"
					: "ItemStyle";

			if (status != null)
			{
				if (Settings.Default.Timeline.HighlightFavorited && status.Favorited)
					str = "FavoritedItemStyle";

				if (selected != null)
					if (Settings.Default.Timeline.HighlightStatusRepliedBySelected && status.StatusID == selected.InReplyToStatusID)
						str = "StatusRepliedBySelectedItemStyle";
					else if (Settings.Default.Timeline.HighlightStatusReplyingToSelected && status.InReplyToStatusID == selected.StatusID)
						str = "StatusReplyingToSelectedItemStyle";
					else if (Settings.Default.Timeline.HighlightStatusHavingSelectedSender && status.UserName == selected.UserName)
						str = "StatusHavingSelectedSenderItemStyle";
			}

			return (Style)listBox.Resources[str];
		}
	}

#pragma warning restore 1591
}
