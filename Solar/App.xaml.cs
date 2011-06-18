using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;
using Ignition.Presentation;
using Solar.Models;

namespace Solar
{
	/// <summary>
	/// App.xaml の相互作用ロジック
	/// </summary>
	public partial class App : Application
	{
		[SuppressUnmanagedCodeSecurity, DllImport("user32")]
		static extern IntPtr SetForegroundWindow(IntPtr hWnd);

		/// <summary>
		/// 実行可能ファイルのあるディレクトリを取得します。
		/// </summary>
		public static string StartupPath
		{
			get
			{
				return AppDomain.CurrentDomain.BaseDirectory;
			}
		}

		/// <summary>
		/// 実行可能ファイルへのフルパスを取得します。
		/// </summary>
		public static string ExecutablePath
		{
			get
			{
				return Assembly.GetEntryAssembly().Location;
			}
		}

		/// <summary>
		/// スクリプト フォルダのフルパスを取得します。
		/// </summary>
		public static string ScriptsPath
		{
			get
			{
				return Path.Combine(StartupPath, "Scripts");
			}
		}

		/// <summary>
		/// 短縮 URL 展開スクリプト フォルダのフルパスを取得します。
		/// </summary>
		public static string UrlExpanderScriptsPath
		{
			get
			{
				return Path.Combine(ScriptsPath, "UrlExpander");
			}
		}

		/// <summary>
		/// URL 短縮スクリプト フォルダのフルパスを取得します。
		/// </summary>
		public static string UrlShortenerScriptsPath
		{
			get
			{
				return Path.Combine(ScriptsPath, "UrlShortener");
			}
		}

		/// <summary>
		/// フィルタ ソース スクリプト フォルダのフルパスを取得します。
		/// </summary>
		public static string FilterSourceScriptsPath
		{
			get
			{
				return Path.Combine(ScriptsPath, "FilterSource");
			}
		}

		/// <summary>
		/// フィルタ項目 スクリプト フォルダのフルパスを取得します。
		/// </summary>
		public static string FilterTermsScriptsPath
		{
			get
			{
				return Path.Combine(ScriptsPath, "FilterTerms");
			}
		}

		/// <summary>
		/// コンソール スクリプト フォルダのフルパスを取得します。
		/// </summary>
		public static string ConsoleScriptsPath
		{
			get
			{
				return Path.Combine(ScriptsPath, "Console");
			}
		}

		/// <summary>
		/// ピクセルシェーダ 3.0 がサポートされていてかつアイコン影を使うかどうかを取得します。
		/// </summary>
		public static bool IsEffectSupported
		{
			get
			{
				return RenderCapability.IsPixelShaderVersionSupported(3, 0)
					&& Settings.Default.Timeline.ShowIconShadowIfAvailable;
			}
		}

		/// <summary>
		/// アセンブリ バージョンを取得します。
		/// </summary>
		public static Version AssemblyVersion
		{
			get
			{
				return Assembly.GetExecutingAssembly().GetName().Version;
			}
		}

		/// <summary>
		/// 現在の App を取得します。
		/// </summary>
		public static new App Current
		{
			get
			{
				return (App)Application.Current;
			}
		}

		/// <summary>
		/// アプリケーションのメインウィンドウを取得します。
		/// </summary>
		public new MainWindow MainWindow
		{
			get
			{
				return (MainWindow)Application.Current.MainWindow;
			}
		}

		/// <summary>
		/// 指定した例外を Solar.slexc へ書きだします。
		/// </summary>
		/// <param name="ex">発生した例外。</param>
		/// <returns>指定した例外。</returns>
		public static Exception Log(Exception ex)
		{
#if DEBUG
            if (!ex.Message.Contains("OAuth") && !ex.Message.Contains("401") && !ex.Message.Contains("API 実行"))
                throw ex;
#endif
            lock (App.Current)
				File.AppendAllText(Path.Combine(StartupPath, "Solar.slexc"), AssemblyVersion + " " + DateTime.Now + "\r\n" + ex.ToString() + "\r\n----\r\n\r\n");

			return ex;
		}

		void Application_Startup(object sender, StartupEventArgs e)
		{
			ServicePointManager.Expect100Continue = false;
#if !DEBUG
			AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);
#endif
            VisualStylesEnabled.Initialize();

			var current = Process.GetCurrentProcess();
			var ps = Process.GetProcessesByName(current.ProcessName)
							.Where(_ => _.MainModule.FileName == current.MainModule.FileName && _.Id != current.Id)
							.SingleOrDefault();

			if (ps != null)
			{
				SetForegroundWindow(ps.MainWindowHandle);
				App.Current.Shutdown();
			}
			else if (Environment.GetCommandLineArgs().Contains("--create-update"))
			{
				new UpdateInfo().Save();
				App.Current.Shutdown();
			}
			else
				CheckUpdates(true);
		}

		internal static void CheckUpdates(bool isAutomatic)
		{
			Task.Factory.StartNew(() =>
			{
				try
				{
					if (!UpdateInfo.Load()
								   .Update(_ => MessageBoxEx.Show(App.Current.MainWindow, "Solar " + _.Version + " を取得しました。\r\nSolar を終了すると自動的に新しいバージョンが起動されます。\r\n今すぐ新しいバージョンを起動しますか？", "アップデート", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes) &&
						!isAutomatic)
						App.Current.Dispatcher.BeginInvoke((Action)(() => MessageBoxEx.Show(App.Current.MainWindow, "新しいバージョンは見つかりませんでした。", "アップデート", MessageBoxButton.OK, MessageBoxImage.Information)), DispatcherPriority.Background);
				}
				catch (WebException ex)
				{
					App.Log(ex);

					if (!isAutomatic)
						App.Current.Dispatcher.BeginInvoke((Action)(() => MessageBoxEx.Show(App.Current.MainWindow, ex.Message, ex.GetType().FullName, MessageBoxButton.OK, MessageBoxImage.Warning)), DispatcherPriority.Background);
				}
			});
		}

		void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
		{
			var ex = Log((Exception)e.ExceptionObject);

			Application.Current.Dispatcher.Invoke((Action)(() => MessageBoxEx.Show(ex.ToString(), ex.GetType().FullName, MessageBoxButton.OK, MessageBoxImage.Error)), DispatcherPriority.Background);
		}

		void Application_SessionEnding(object sender, SessionEndingCancelEventArgs e)
		{
			Client.Instance.Dispose();
		}
	}
}
