using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using IronPython.Runtime;
using Microsoft.Scripting;
using Microsoft.Scripting.Hosting;
using Microsoft.Scripting.Hosting.Providers;
using Microsoft.Scripting.Runtime;
using Solar.Models;
using Solar.Scripting;

namespace Solar
{
	/// <summary>
	/// ConsoleWindow.xaml の相互作用ロジック
	/// </summary>
	partial class ConsoleWindow : Window
	{
		int currentHistory = -1;
		StringBuilder output;
		bool currentSetToHistory;
		readonly StringBuilder code = new StringBuilder();
		readonly List<string> history = new List<string>();
		readonly ScriptScope scope = Client.Runtime.CreateScope();
		readonly ScriptEngine engine = Client.Runtime.GetEngineByFileExtension("py");
		readonly PythonContext context;
		readonly ErrorSink sink = ErrorSink.Null;
		readonly Regex indentRegex = new Regex(@"^\s*", RegexOptions.Compiled);
		readonly Regex charRegex = new Regex(@"\\u[0-9A-Fa-f]{1,4}", RegexOptions.Compiled);
		readonly ScriptWatcher scripts;
		readonly TextBoxConsole consoleFile;

		/// <summary>
		/// テキストボックスを取得します。
		/// </summary>
		public TextBox TextBox
		{
			get
			{
				return textBox;
			}
		}

		/// <summary>
		/// 現在の入力が継続入力であるかどうかを取得します。
		/// </summary>
		public bool IsContinueInput
		{
			get;
			private set;
		}

		/// <summary>
		/// タブキーから入力できるインデント文字列を取得または設定します。
		/// </summary>
		public string Indent
		{
			get;
			set;
		}

		/// <summary>
		/// 通常時のプロンプト文字列を取得または設定します。
		/// </summary>
		public string NormalPrompt
		{
			get;
			set;
		}

		/// <summary>
		/// 継続入力時のプロンプト文字列を取得または設定します。
		/// </summary>
		public string ContinuePrompt
		{
			get;
			set;
		}

		/// <summary>
		/// ユーザ入力文字列の開始位置を取得または設定します。
		/// </summary>
		public int InputStart
		{
			get;
			set;
		}

		/// <summary>
		/// ユーザ入力文字列の長さを取得します。
		/// </summary>
		public int InputLength
		{
			get
			{
				return textBox.Text.Length - this.InputStart;
			}
		}

		/// <summary>
		/// ユーザ入力文字列を取得または設定します。
		/// </summary>
		public string InputText
		{
			get
			{
				return textBox.Text.Substring(this.InputStart);
			}
			set
			{
				textBox.Select(this.InputStart, this.InputLength);
				textBox.SelectedText = value;
				textBox.Select(textBox.Text.Length, 0);
				textBox.ScrollToEnd();
			}
		}

		/// <summary>
		/// ConsoleWindow の新しいインスタンスを初期化します。
		/// </summary>
		public ConsoleWindow()
		{
			InitializeComponent();

			this.Indent = "    ";
			this.NormalPrompt = ">>> ";
			this.ContinuePrompt = "... ";

			consoleFile = new TextBoxConsole(this);
			history.Add("from System import *; from System.Collections.Generic import *; from System.Windows import *; from System.Windows.Controls import *; from System.Windows.Input import *; from Solar import *; from Solar.Models import *; from Lunar import *");
			context = (PythonContext)HostingHelpers.GetLanguageContext(engine);
			textBox.Text = @"IronPython ${ipy} on Solar ${sol}
Hit UP to set the input for importing recommended modules.
Type ""dir(obj)"" or ""help(obj)"" to get more details of the obj.
".Replace("${ipy}", typeof(PythonContext).Assembly.GetName().Version.ToString())
 .Replace("${sol}", App.AssemblyVersion.ToString()) + this.NormalPrompt;
			this.InputStart = textBox.Text.Length;

			scope.SetVariable("console", this);
			context.SystemState.Get__dict__()["stdout"] = consoleFile;
			scripts = new ScriptWatcher(Client.Instance, App.ConsoleScriptsPath, () => scope);
			scripts.Changing += (sender, e) => context.SystemState.Get__dict__()["stdout"] = consoleFile;
		}

		void textBox_PreviewKeyDown(object sender, KeyEventArgs e)
		{
			if (!e.Handled)
				switch (e.Key)
				{
					case Key.Enter:
						if (e.KeyboardDevice.Modifiers == ModifierKeys.None)
							Process(this.InputText);

						e.Handled = true;

						break;
					case Key.Home:
						if (textBox.SelectionStart >= this.InputStart)
							textBox.Select(InputStart, e.KeyboardDevice.Modifiers == ModifierKeys.Shift
								? textBox.SelectionStart - this.InputStart
								: 0);
						else if (e.KeyboardDevice.Modifiers == ModifierKeys.Shift)
							textBox.Select(textBox.SelectionStart, this.InputStart - textBox.SelectionStart);
						else
							textBox.Select(InputStart, 0);

						e.Handled = true;

						break;
					case Key.Up:
						if (!currentSetToHistory && !string.IsNullOrEmpty(this.InputText))
						{
							currentSetToHistory = true;
							history.Add(this.InputText);
						}
						else if (currentSetToHistory && currentHistory == history.Count - 1)
							history[history.Count - 1] = this.InputText;

						if (currentHistory == -1)
							this.InputText = history[currentHistory = history.Count - (currentSetToHistory ? 2 : 1)];
						else if (currentHistory > 0)
							this.InputText = history[--currentHistory];

						e.Handled = true;

						break;
					case Key.Down:
						if (!currentSetToHistory && !string.IsNullOrEmpty(this.InputText))
						{
							currentSetToHistory = true;
							history.Add(this.InputText);
						}
						else if (currentSetToHistory && currentHistory == history.Count - 1)
							history[history.Count - 1] = this.InputText;

						if (currentHistory == -1)
							this.InputText = history[currentHistory = history.Count - (currentSetToHistory ? 2 : 1)];
						else if (currentHistory + 1 < history.Count)
							this.InputText = history[++currentHistory];

						e.Handled = true;

						break;
					case Key.Left:
						e.Handled = textBox.SelectionStart == this.InputStart;

						break;
					case Key.Tab:
						if (e.KeyboardDevice.Modifiers == ModifierKeys.None)
						{
							if (textBox.SelectionStart < this.InputStart)
								textBox.Select(InputStart, 0);

							textBox.SelectedText = this.Indent;
							textBox.SelectionLength = 0;
							textBox.SelectionStart += this.Indent.Length;
							e.Handled = true;
						}

						break;
					case Key.Back:
						if (textBox.SelectionStart > this.InputStart &&
							textBox.SelectionLength == 0 &&
							textBox.Text.Substring(this.InputStart, textBox.SelectionStart - this.InputStart).EndsWith(Indent))
						{
							textBox.SelectionStart -= this.Indent.Length;
							textBox.SelectionLength = this.Indent.Length;
							textBox.SelectedText = "";
							e.Handled = true;
						}
						else
							e.Handled = textBox.SelectionLength > 0 ? textBox.SelectionStart < this.InputStart : textBox.SelectionStart <= this.InputStart;

						break;
					case Key.Delete:
						e.Handled = textBox.SelectionStart < this.InputStart;

						break;
					case Key.Escape:
						this.InputText = "";
						e.Handled = true;

						break;
					case Key.F1:
						Surround("help(", ")");
						e.Handled = true;

						break;
					case Key.F2:
						Surround("dir(", ")");
						e.Handled = true;

						break;
					default:
						if ((e.KeyboardDevice.Modifiers == ModifierKeys.None ||
							e.KeyboardDevice.Modifiers == ModifierKeys.Shift ||
							e.KeyboardDevice.Modifiers == ModifierKeys.Control && (e.Key == Key.V || e.Key == Key.X || e.Key == Key.Back || e.Key == Key.Delete)) &&
							textBox.SelectionStart < this.InputStart)
							textBox.Select(InputStart, 0);

						break;
				}
		}

		/// <summary>
		/// 選択されているテキストを囲みます。すでに囲まれている場合は、囲みを解除します。選択されていない場合は、囲みの間にキャレットを置きます。
		/// </summary>
		/// <param name="start">囲みの開始。</param>
		/// <param name="end">囲みの終了。</param>
		public void Surround(string start, string end)
		{
			if (textBox.SelectionStart >= this.InputStart)
				if (textBox.SelectionLength > 0)
					if (textBox.SelectedText.StartsWith(start) &&
						textBox.SelectedText.EndsWith(end))
						textBox.SelectedText = textBox.SelectedText.Substring(start.Length, textBox.SelectionLength - end.Length - start.Length);
					else
						textBox.SelectedText = start + textBox.SelectedText + end;
				else
				{
					textBox.SelectedText = start + end;
					textBox.Select(textBox.SelectionStart + start.Length, 0);
				}
		}

		void Process(string text)
		{
			try
			{
				code.Append(text);
				AppendText("\r\n");
				output = new StringBuilder();

				var str = code.ToString();
				var stat = engine.CreateScriptSourceFromString(str, SourceCodeKind.InteractiveCode).GetCodeProperties();

				if (string.IsNullOrEmpty(text) ||
					SourceCodePropertiesUtils.IsCompleteOrInvalid(stat, string.IsNullOrEmpty(text)))
				{
					context.SystemState.Get__dict__()["stdout"] = consoleFile;

					var rt = engine.Execute<object>(str, scope);

					if (output.Length > 0)
						AppendText(output.ToString());

					if (rt != null)
					{
						var result = context.FormatObject(new DynamicOperations(context), rt);

						AppendText(charRegex.Replace(result, _ => _.Index == 0 || result.Substring(_.Index - 1, 1) != "\\" ? Convert.ToChar(int.Parse(_.Value.Substring(2), NumberStyles.HexNumber)).ToString() : _.Value) + "\r\n" + NormalPrompt);
					}
					else
						AppendText(this.NormalPrompt);

					this.IsContinueInput = false;
					code.Clear();
				}
				else
				{
					code.AppendLine();
					AppendText(this.ContinuePrompt);

					if (this.IsContinueInput)
						this.InputText = indentRegex.Match(text).Value + (text.EndsWith(":") ? this.Indent : null);
					else if (!text.EndsWith("\"\"\""))
						this.InputText = this.Indent;

					this.IsContinueInput = true;
				}

				output = null;
			}
			catch (Exception ex)
			{
				AppendText(context.FormatException(ex) + "\r\n" + NormalPrompt);
				code.Clear();
			}

			if (!string.IsNullOrWhiteSpace(text))
				history.Add(text);

			currentSetToHistory = false;
		}

		void AppendText(string text)
		{
			textBox.Text += text;
			this.InputStart = textBox.Text.Length;
			textBox.Select(textBox.Text.Length, 0);
			textBox.ScrollToEnd();
			currentHistory = -1;
		}

		void Window_Loaded(object sender, RoutedEventArgs e)
		{
			this.InputText = "";
			textBox.Focus();
		}

		void Window_Unloaded(object sender, RoutedEventArgs e)
		{
			scripts.Dispose();
		}

		void cutMenuItem_Click(object sender, RoutedEventArgs e)
		{
			if (textBox.SelectionLength == 0 ||
				textBox.SelectionStart < this.InputStart)
				return;

			textBox.Cut();
		}

		void pasteMenuItem_Click(object sender, RoutedEventArgs e)
		{
			if (textBox.SelectionStart < this.InputStart)
				textBox.Select(this.InputStart, 0);

			textBox.Paste();
		}

		void clearMenuItem_Click(object sender, RoutedEventArgs e)
		{
			Clear();
		}

		/// <summary>
		/// テキストボックスをクリアします。
		/// </summary>
		public void Clear()
		{
			textBox.Clear();
			textBox.Text = this.IsContinueInput ? this.ContinuePrompt : this.NormalPrompt;
			this.InputStart = textBox.Text.Length;
			textBox.Select(this.InputStart, 0);
			textBox.ScrollToEnd();
		}

		/// <summary>
		/// 指定された文字列を表示します。
		/// </summary>
		/// <param name="s">表示する文字列。</param>
		public void Write(string s)
		{
			if (output == null)
			{
				var st = textBox.SelectionStart;
				var ln = textBox.SelectionLength;
				var ov = textBox.SelectionStart >= this.InputStart;

				textBox.Text = textBox.Text.Insert(this.InputStart - (this.IsContinueInput ? this.ContinuePrompt : this.NormalPrompt).Length - 2, s);

				this.InputStart += s.Length;

				if (ov)
					textBox.Select(st + s.Length, ln);
				else
					textBox.Select(st, ln);

				textBox.ScrollToEnd();
			}
			else
				output.Append(s);
		}

		/// <summary>
		/// 指定された文字列を表示します。
		/// </summary>
		/// <param name="s">表示する文字列。</param>
		public void WriteLine(string s)
		{
			Write(s + "\r\n");
		}

#pragma warning disable 1591
		public class TextBoxConsole
		{
			readonly ConsoleWindow consoleWindow;
			readonly StringBuilder sb = new StringBuilder();

			public TextBoxConsole(ConsoleWindow consoleWindow)
			{
				this.consoleWindow = consoleWindow;
			}

			public void write(string s)
			{
				consoleWindow.Write(s);
			}
		}
#pragma warning restore 1591
	}
}
