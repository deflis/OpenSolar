using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Threading;
using Ignition;

namespace Solar
{
	/// <summary>
	/// 自動入力補完を表します。
	/// </summary>
	/// <typeparam name="T">補完対象のデータ型。</typeparam>
	public class AutoCompletion<T> : NotifyObject, IDisposable
	{
		TextBox textBox;
		Popup popup;
		ListBox listBox;
		int start;
		int length;
		Func<TextBox, TextChange, bool> open;
		Func<T, string> selector;

		/// <summary>
		/// 補完候補のソースを取得または設定します。
		/// </summary>
		public IEnumerable<T> Source
		{
			get;
			set;
		}

		/// <summary>
		/// ポップアップの X オフセットを取得または設定します。
		/// </summary>
		public double OffsetX
		{
			get;
			set;
		}

		/// <summary>
		/// ポップアップが開いているかどうかを取得します。
		/// </summary>
		public bool IsOpen
		{
			get
			{
				return popup.IsOpen;
			}
		}

		/// <summary>
		/// テキストボックス、ポップアップ、リストボックス、プレフィックス、補完ソース、補完文字列セレクタを指定し AutoCompletion の新しいインスタンスを初期化します。
		/// </summary>
		/// <param name="textBox">補完を使用するテキストボックス。</param>
		/// <param name="popup">補完候補を表示するポップアップ。</param>
		/// <param name="listBox">補完候補を表示するリストボックス。</param>
		/// <param name="prefix">候補の表示を開始するプレフィックス。</param>
		/// <param name="source">補完候補のソース。</param>
		/// <param name="selector">実際に保管される文字列へ変換するためのセレクタ。</param>
		public AutoCompletion(TextBox textBox, Popup popup, ListBox listBox, char prefix, IEnumerable<T> source, Func<T, string> selector)
			: this(textBox, popup, listBox, (_, c) => _.Text[c.Offset] == prefix, source, selector)
		{
		}

		/// <summary>
		/// テキストボックス、ポップアップ、リストボックス、開始条件、補完ソース、補完文字列セレクタを指定し AutoCompletion の新しいインスタンスを初期化します。
		/// </summary>
		/// <param name="textBox">補完を使用するテキストボックス。</param>
		/// <param name="popup">補完候補を表示するポップアップ。</param>
		/// <param name="listBox">補完候補を表示するリストボックス。</param>
		/// <param name="open">候補の表示を開始する条件。</param>
		/// <param name="source">補完候補のソース。</param>
		/// <param name="selector">実際に保管される文字列へ変換するためのセレクタ。</param>
		public AutoCompletion(TextBox textBox, Popup popup, ListBox listBox, Func<TextBox, TextChange, bool> open, IEnumerable<T> source, Func<T, string> selector)
		{
			this.textBox = textBox;
			this.popup = popup;
			this.listBox = listBox;
			this.open = open;
			this.Source = source;
			this.selector = selector;
			this.OffsetX = -4;

			textBox.PreviewKeyDown += textBox_PreviewKeyDown;
			textBox.KeyUp += textBox_KeyUp;
			textBox.TextChanged += textBox_TextChanged;
			listBox.MouseDoubleClick += listBox_MouseDoubleClick;
			listBox.MouseUp += listBox_MouseUp;
		}

		void listBox_MouseUp(object sender, MouseButtonEventArgs e)
		{
			textBox.Focus();
			e.Handled = true;
		}

		void listBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
		{
			if (listBox.SelectedItem != null)
			{
				Commit();
				textBox.Focus();
				e.Handled = true;
			}
		}

		void textBox_KeyUp(object sender, KeyEventArgs e)
		{
			if (popup.IsOpen &&
				(textBox.CaretIndex <= start || textBox.CaretIndex > start + length))
				popup.IsOpen = false;
		}

		void textBox_PreviewKeyDown(object sender, KeyEventArgs e)
		{
			if (popup.IsOpen)
			{
				switch (e.Key)
				{
					case Key.Up:
						if (listBox.SelectedIndex <= 0)
							listBox.SelectedIndex = listBox.Items.Count - 1;
						else
							listBox.SelectedIndex--;

						listBox.ScrollIntoView(listBox.Items[listBox.SelectedIndex]);
						e.Handled = true;

						break;
					case Key.Down:
						listBox.SelectedIndex = (listBox.SelectedIndex + 1) % listBox.Items.Count;
						listBox.ScrollIntoView(listBox.Items[listBox.SelectedIndex]);
						e.Handled = true;

						break;
					case Key.Escape:
						popup.IsOpen = false;
						e.Handled = true;

						break;
					case Key.Return:
					case Key.Tab:
						Commit();
						e.Handled = true;

						break;
					case Key.LeftShift:
					case Key.LeftAlt:
					case Key.LeftCtrl:
					case Key.RightShift:
					case Key.RightAlt:
					case Key.RightCtrl:
					case Key.LWin:
					case Key.RWin:
					case Key.Left:
					case Key.Right:
					case Key.Back:
						break;
					case Key.Oem1:
					case Key.Oem2:
					case Key.Oem3:
					case Key.Oem4:
					case Key.Oem5:
					case Key.Oem6:
					case Key.Oem7:
					case Key.Oem8:
						Commit();

						break;
					default:
						var c = (char)KeyInterop.VirtualKeyFromKey(e.Key);

						if (!char.IsLetterOrDigit(c) &&
							c != '_')
							Commit();

						break;
				}
			}
		}

		void Commit()
		{
			if (listBox.SelectedIndex != -1)
			{
				textBox.Select(start, length);
				textBox.SelectedText = selector((T)listBox.Items[listBox.SelectedIndex]);
				textBox.Select(textBox.SelectionStart + textBox.SelectionLength, 0);
			}

			popup.IsOpen = false;
		}

		void textBox_TextChanged(object sender, TextChangedEventArgs e)
		{
			var c = e.Changes.FirstOrDefault();

			if (c != null &&
				(InputMethod.Current == null || InputMethod.Current.ImeState == InputMethodState.Off))
				if (popup.IsOpen)
				{
					if ((length += c.AddedLength - c.RemovedLength) < 0)
					{
						popup.IsOpen = false;

						return;
					}

					var str = textBox.Text.Substring(start, length);

					if (string.IsNullOrEmpty(str))
					{
						Task.Factory.StartNew(() =>
						{
							try
							{
								var src = this.Source.Freeze();

								textBox.Dispatcher.BeginInvoke((Action)(() =>
								{
									listBox.ItemsSource = src;
									listBox.SelectedIndex = -1;
								}), DispatcherPriority.Background);
							}
							catch (Exception ex)
							{
								App.Log(ex);
							}
						});
					}
					else
					{
						Task.Factory.StartNew(() =>
						{
							try
							{
								var src = this.Source.Select(_ => new
								{
									Value = _,
									Index = selector(_).IndexOf(str, StringComparison.OrdinalIgnoreCase),
								}).Where(_ => _.Index != -1).OrderBy(_ => _.Index).Select(_ => _.Value).Freeze();

								textBox.Dispatcher.BeginInvoke((Action)(() =>
								{
									if (src.Any())
									{
										listBox.ItemsSource = src;
										listBox.SelectedIndex = 0;
										listBox.ScrollIntoView(listBox.Items[listBox.SelectedIndex]);
									}
									else
										listBox.SelectedIndex = -1;
								}), DispatcherPriority.Background);
							}
							catch (Exception ex)
							{
								App.Log(ex);
							}
						});
					}
				}
				else if (c.AddedLength > 0)
				{
					start = textBox.CaretIndex - 1;
					length = 1;

					if (open(textBox, c) &&
						start >= 0 &&
						!char.IsWhiteSpace(textBox.Text.Substring(c.Offset, c.AddedLength).Last()))
					{
						Task.Factory.StartNew(() =>
						{
							try
							{
								var src = this.Source.Freeze();

								if (!src.Any())
									return;

								textBox.Dispatcher.BeginInvoke((Action)(() =>
								{
									var p = textBox.GetRectFromCharacterIndex(start);

									listBox.SelectedIndex = -1;
									listBox.ItemsSource = src;
									popup.PlacementTarget = textBox;
									popup.PlacementRectangle = new Rect(p.X + this.OffsetX, p.Y, p.Width, p.Height);
									popup.IsOpen = true;
								}), DispatcherPriority.Background);
							}
							catch (Exception ex)
							{
								App.Log(ex);
							}
						});
					}
				}
		}

		/// <summary>
		/// 渡されたコントロールにアタッチされたイベントをデタッチします。
		/// </summary>
		public void Dispose()
		{
			textBox.PreviewKeyDown -= textBox_PreviewKeyDown;
			textBox.KeyUp -= textBox_KeyUp;
			textBox.TextChanged -= textBox_TextChanged;
			listBox.MouseDoubleClick -= listBox_MouseDoubleClick;
			listBox.MouseUp -= listBox_MouseUp;
		}
	}
}
