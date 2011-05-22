using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using Ignition;
using Ignition.Presentation;
using Ignition.Linq;
using Microsoft.Scripting;
using Microsoft.Scripting.Hosting;
using Solar.Models;

namespace Solar.Scripting
{
	/// <summary>
	/// スクリプトの変更を監視し動的に再読み込みする機構を提供します。
	/// </summary>
	public class ScriptWatcher : IDisposable
	{
		readonly FileSystemWatcher watcher;
		readonly FileSystemWatcher subWatcher;
		readonly SortedDictionary<string, Timer> timers = new SortedDictionary<string, Timer>();
		readonly Client client;
		readonly string subPath;
		readonly Func<ScriptScope> scope;

		/// <summary>
		/// スクリプトが変更されるときに発生します。
		/// </summary>
		public event EventHandler Changing;
		/// <summary>
		/// スクリプトが変更されたときに発生します。
		/// </summary>
		public event EventHandler Changed;

		/// <summary>
		/// スクリプトの一覧を取得します。
		/// </summary>
		public SortedDictionary<string, ScriptScope> Scripts
		{
			get;
			private set;
		}

		/// <summary>
		/// クライアントとディレクトリ、およびスコープの作成方法を指定し ScriptWatcher の新しいインスタンスを初期化します。
		/// </summary>
		/// <param name="client">クライアント。</param>
		/// <param name="path">ディレクトリ。</param>
		/// <param name="scope">スコープの作成方法。</param>
		public ScriptWatcher(Client client, string path, Func<ScriptScope> scope)
		{
			this.client = client;
			this.scope = scope;
			this.Scripts = new SortedDictionary<string, ScriptScope>();

			if (Directory.Exists(path))
			{
				watcher = new FileSystemWatcher(path, "*.py")
				{
					IncludeSubdirectories = false,
					EnableRaisingEvents = true,
					NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite,
				};
				watcher.Created += watcher_Created;
				watcher.Deleted += watcher_Deleted;
				watcher.Changed += watcher_Changed;
				watcher.Renamed += watcher_Renamed;

				Directory.EnumerateFiles(path, "*.py").Run(LoadScript);
			}

			if (Directory.Exists(subPath = GetUserPath(path)))
			{
				subWatcher = new FileSystemWatcher(subPath, "*.py")
				{
					IncludeSubdirectories = false,
					EnableRaisingEvents = true,
					NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite,
				};
				subWatcher.Created += watcher_Created;
				subWatcher.Deleted += watcher_Deleted;
				subWatcher.Changed += watcher_Changed;
				subWatcher.Renamed += watcher_Renamed;

				var exists = new HashSet<string>(Directory.Exists(path) ? Directory.EnumerateFiles(path, "*.py").Select(Path.GetFileName) : Enumerable.Empty<string>());

				Directory.EnumerateFiles(subPath, "*.py").Where(_ => !exists.Contains(Path.GetFileName(_))).Run(LoadScript);
			}
		}

		/// <summary>
		/// クライアントとディレクトリを指定し ScriptWatcher の新しいインスタンスを初期化します。
		/// </summary>
		/// <param name="client">クライアント。</param>
		/// <param name="path">ディレクトリ。</param>
		public ScriptWatcher(Client client, string path)
			: this(client, path, () => Client.EnsureScriptRuntime().CreateScope())
		{
		}

		/// <summary>
		/// ユーザスクリプトへのパスを取得します。
		/// </summary>
		/// <param name="path">元のスクリプトへのパス。</param>
		/// <returns>ユーザスクリプトへのパス。</returns>
		public static string GetUserPath(string path)
		{
			return path.Replace(App.ScriptsPath, Path.Combine(App.StartupPath, "UserScripts"));
		}

		/// <summary>
		/// ユーザスクリプトと元のスクリプトを比較し適切な方を取得します。
		/// </summary>
		/// <param name="path">元のスクリプトへのパス。</param>
		/// <returns>ユーザスクリプトが元のスクリプトより新しい場合はユーザスクリプトへのパス。そうでなければ元のスクリプトへのパス。</returns>
		public static string GetUserOrDefault(string path)
		{
			var user = GetUserPath(path = path.Replace(Path.Combine(App.StartupPath, "UserScripts"), App.ScriptsPath));

			if (File.Exists(user) &&
				(!File.Exists(path) || File.GetLastWriteTime(user) >= File.GetLastWriteTime(path)))
				return user;
			else
				return path;
		}

		void watcher_Created(object sender, FileSystemEventArgs e)
		{
			OnChanging();
			LoadScript(e.FullPath);
			OnChanged();
		}

		void LoadScript(string path)
		{
			var name = Path.GetFileName(path = GetUserOrDefault(path));

			try
			{
				var s = this.Scripts[name] = Client.EnsureScriptRuntime().GetEngineByFileExtension("py").ExecuteFile(path, scope());

				if (s.ContainsVariable("Load"))
					s.GetVariable("Load")(client);
			}
			catch (Exception ex)
			{
				if (Client.Instance == null)
				{
					var message = App.Log(ex).ToString();

					if (ex is SyntaxErrorException)
					{
						var exs = (SyntaxErrorException)ex;

						message = string.Format(@"{0}
{1} line {2}, {3}", ex.Message, exs.SourcePath, exs.Line, exs.Column);
					}

					MessageBoxEx.Show(Application.Current.MainWindow, message, name, MessageBoxButton.OK, MessageBoxImage.Warning);
				}
				else
					Client.Instance.OnThrowScriptError(new EventArgs<string, Exception>(name, ex));
			}
		}

		internal void SuspendScript(string name)
		{
			OnChanging();
			Unload(name);
			OnChanged();
		}

		void watcher_Deleted(object sender, FileSystemEventArgs e)
		{
			OnChanging();
			Unload(e.Name);
			OnChanged();
		}

		void Unload(string name)
		{
			if (this.Scripts.ContainsKey(name) &&
				this.Scripts[name].ContainsVariable("Unload"))
				this.Scripts[name].GetVariable("Unload")(client);

			this.Scripts.Remove(name);
		}

		void watcher_Changed(object sender, FileSystemEventArgs e)
		{
			const int margin = 10;

			lock (timers)
				if (timers.ContainsKey(e.Name))
					timers[e.Name].Change(margin, Timeout.Infinite);
				else
					timers[e.Name] = new Timer(_ =>
					{
						lock (timers)
						{
							if (this.Scripts.ContainsKey(e.Name) &&
							   this.Scripts[e.Name].ContainsVariable("Unload"))
								this.Scripts[e.Name].GetVariable("Unload")(client);

							OnChanging();
							LoadScript(e.FullPath);
							OnChanged();
							timers.Remove(e.Name);
						}
					}, null, margin, Timeout.Infinite);
		}

		void watcher_Renamed(object sender, RenamedEventArgs e)
		{
			if (this.Scripts.ContainsKey(e.OldName))
			{
				OnChanging();
				this.Scripts[e.Name] = this.Scripts[e.OldName];
				this.Scripts.Remove(e.OldName);
				OnChanged();
			}
		}

		void OnChanging()
		{
			Changing.RaiseEvent(this, EventArgs.Empty);
		}

		void OnChanged()
		{
			Changed.RaiseEvent(this, EventArgs.Empty);
		}

		/// <summary>
		/// すべてのスクリプトを開放します。
		/// </summary>
		public void Dispose()
		{
			OnChanging();

			foreach (var i in this.Scripts.Values.Where(_ => _.ContainsVariable("Unload")))
				i.GetVariable("Unload")(client);

			OnChanged();

			if (watcher != null)
				watcher.Dispose();

			if (subWatcher != null)
				subWatcher.Dispose();

			GC.SuppressFinalize(this);
		}

		/// <summary>
		/// dtor
		/// </summary>
		~ScriptWatcher()
		{
			Dispose();
		}
	}
}
