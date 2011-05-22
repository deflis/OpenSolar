using System;
using System.ComponentModel;
using System.IO;
using System.Net;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Xaml;
using System.Xml;
using Ignition;
using Ignition.Presentation;
using Lunar;
using Solar.Filtering;

namespace Solar.Models
{
	/// <summary>
	/// 設定を表します。
	/// </summary>
	public class Settings
	{
		/// <summary>
		/// 設定ファイルへのパス
		/// </summary>
		public static readonly string FileName = Path.Combine(App.StartupPath, "Solar.slconf");
		/// <summary>
		/// インスタンス
		/// </summary>
		public static readonly Settings Default;

		static Settings()
		{
			if (File.Exists(FileName))
				try
				{
					Default = (Settings)XamlServices.Load(FileName);
				}
				catch (XmlException ex)
				{
					App.Log(ex);
					MessageBoxEx.Show("設定ファイル Solar.slconf の読み込みに失敗しました: " + ex.Message, "Solar", MessageBoxButton.OK, MessageBoxImage.Exclamation);
					Default = new Settings();
				}
			else
				Default = new Settings();
		}

		/// <summary>
		/// ユーザコードからは使用しないでください。
		/// </summary>
		public Settings()
		{
			this.Accounts = new NotifyCollection<AccountToken>();
			this.CategoryGroups = new NotifyCollection<CategoryGroup>();
			this.GlobalFilterTerms = new NotifyCollection<FilterTerms>();
			this.Connection = new ConnectionSettings();
			this.Timeline = new TimelineSettings();
			this.Interface = new InterfaceSettings();
			this.Post = new PostSettings();
			this.Key = new KeySettings();
		}

		#region Old Properties
#pragma warning disable 1591

		[Obsolete]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool CollapsePostBox
		{
			get
			{
				return this.Interface.CollapsePostBox;
			}
			set
			{
				this.Interface.CollapsePostBox = value;
			}
		}

		[Obsolete]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public Rect Bounds
		{
			get
			{
				return this.Interface.Bounds;
			}
			set
			{
				this.Interface.Bounds = value;
			}
		}

		[Obsolete]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public TimelineStyle TimelineStyle
		{
			get
			{
				return this.Timeline.TimelineStyle;
			}
			set
			{
				this.Timeline.TimelineStyle = value;
			}
		}

		[Obsolete]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public WindowState WindowState
		{
			get
			{
				return this.Interface.WindowState;
			}
			set
			{
				this.Interface.WindowState = value;
			}
		}

		[Obsolete]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool PostByControlEnter
		{
			get
			{
				return this.Interface.PostByControlEnter;
			}
			set
			{
				this.Interface.PostByControlEnter = value;
			}
		}

		[Obsolete]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool AutoResetNewCount
		{
			get
			{
				return this.Timeline.AutoResetNewCount;
			}
			set
			{
				this.Timeline.AutoResetNewCount = value;
			}
		}

		[Obsolete]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public int MaxStatuses
		{
			get
			{
				return this.Timeline.MaxStatuses;
			}
			set
			{
				this.Timeline.MaxStatuses = value;
			}
		}

		[Obsolete]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShowProtectedIcon
		{
			get
			{
				return this.Timeline.ShowProtectedIcon;
			}
			set
			{
				this.Timeline.ShowProtectedIcon = value;
			}
		}

		[Obsolete]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShowOneWayFollowIcon
		{
			get
			{
				return this.Timeline.ShowOneWayFollowIcon;
			}
			set
			{
				this.Timeline.ShowOneWayFollowIcon = value;
			}
		}

		[Obsolete]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShowIconShadowIfAvailable
		{
			get
			{
				return this.Timeline.ShowIconShadowIfAvailable;
			}
			set
			{
				this.Timeline.ShowIconShadowIfAvailable = value;
			}
		}

		[Obsolete]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShowStatusShadow
		{
			get
			{
				return this.Timeline.ShowStatusShadow;
			}
			set
			{
				this.Timeline.ShowStatusShadow = value;
			}
		}

		[Obsolete]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public Color AlternateStatusColor
		{
			get
			{
				return this.Timeline.AlternateStatusColor;
			}
			set
			{
				this.Timeline.AlternateStatusColor = value;
			}
		}

		[Obsolete]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public Color NewStatusColor
		{
			get
			{
				return this.Timeline.NewStatusColor;
			}
			set
			{
				this.Timeline.NewStatusColor = value;
			}
		}

		[Obsolete]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool HighlightStatusRepliedBySelected
		{
			get
			{
				return this.Timeline.HighlightStatusRepliedBySelected;
			}
			set
			{
				this.Timeline.HighlightStatusRepliedBySelected = value;
			}
		}

		[Obsolete]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public Color HighlightStatusRepliedBySelectedColor
		{
			get
			{
				return this.Timeline.HighlightStatusRepliedBySelectedColor;
			}
			set
			{
				this.Timeline.HighlightStatusRepliedBySelectedColor = value;
			}
		}

		[Obsolete]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool HighlightStatusReplyingToSelected
		{
			get
			{
				return this.Timeline.HighlightStatusReplyingToSelected;
			}
			set
			{
				this.Timeline.HighlightStatusReplyingToSelected = value;
			}
		}

		[Obsolete]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public Color HighlightStatusReplyingToSelectedColor
		{
			get
			{
				return this.Timeline.HighlightStatusReplyingToSelectedColor;
			}
			set
			{
				this.Timeline.HighlightStatusReplyingToSelectedColor = value;
			}
		}

		[Obsolete]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool HighlightStatusHavingSelectedSender
		{
			get
			{
				return this.Timeline.HighlightStatusHavingSelectedSender;
			}
			set
			{
				this.Timeline.HighlightStatusHavingSelectedSender = value;
			}
		}

		[Obsolete]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public Color HighlightStatusHavingSelectedSenderColor
		{
			get
			{
				return this.Timeline.HighlightStatusHavingSelectedSenderColor;
			}
			set
			{
				this.Timeline.HighlightStatusHavingSelectedSenderColor = value;
			}
		}

		[Obsolete]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool UseQuickSearch
		{
			get
			{
				return this.Interface.UseQuickSearch;
			}
			set
			{
				this.Interface.UseQuickSearch = value;
			}
		}

		[Obsolete]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool UseIncrementalQuickSearch
		{
			get
			{
				return this.Interface.UseIncrementalQuickSearch;
			}
			set
			{
				this.Interface.UseIncrementalQuickSearch = value;
			}
		}

		[Obsolete]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool EnableUserStreams
		{
			get
			{
				return this.Connection.EnableUserStreams;
			}
			set
			{
				this.Connection.EnableUserStreams = true;
			}
		}

		[Obsolete]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool MinimizeToTray
		{
			get
			{
				return this.Interface.MinimizeToTray;
			}
			set
			{
				this.Interface.MinimizeToTray = value;
			}
		}

#pragma warning restore 1591
		#endregion

		/// <summary>
		/// アカウントを取得します。
		/// </summary>
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		public NotifyCollection<AccountToken> Accounts
		{
			get;
			private set;
		}

		/// <summary>
		/// カテゴリ グループを取得します。
		/// </summary>
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		public NotifyCollection<CategoryGroup> CategoryGroups
		{
			get;
			private set;
		}

		/// <summary>
		/// 全体フィルタ項目を取得します。
		/// </summary>
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		public NotifyCollection<FilterTerms> GlobalFilterTerms
		{
			get;
			private set;
		}

		/// <summary>
		/// 接続設定を取得します。
		/// </summary>
		public ConnectionSettings Connection
		{
			get;
			set;
		}

		/// <summary>
		/// タイムライン設定を取得します。
		/// </summary>
		public TimelineSettings Timeline
		{
			get;
			set;
		}

		/// <summary>
		/// インターフェイス設定を取得します。
		/// </summary>
		public InterfaceSettings Interface
		{
			get;
			set;
		}

		/// <summary>
		/// 投稿設定を取得します。
		/// </summary>
		public PostSettings Post
		{
			get;
			set;
		}

		/// <summary>
		/// キー設定を取得します。
		/// </summary>
		public KeySettings Key
		{
			get;
			set;
		}

		/// <summary>
		/// 設定を保存します。
		/// </summary>
		public void Save()
		{
			try
			{
				var tmp = Path.GetTempFileName();

				if (File.Exists(FileName))
					File.Copy(FileName, tmp, true);

				try
				{
					using (var xw = XmlWriter.Create(FileName, new XmlWriterSettings
					{
						Indent = true,
						IndentChars = "\t",
						NewLineOnAttributes = true,
					}))
						XamlServices.Save(xw, Settings.Default);
				}
				catch (Exception)
				{
					if (File.Exists(tmp))
						File.Copy(tmp, FileName, true);

					throw;
				}
				finally
				{
					File.Delete(tmp);
				}
			}
			catch
			{
			}
		}
	}

	/// <summary>
	/// 接続設定を表します。
	/// </summary>
	public class ConnectionSettings
	{
		/// <summary>
		/// ConnectionSettings の新しいインスタンスを初期化します。
		/// </summary>
		public ConnectionSettings()
		{
			this.EnableUserStreams = true;
			this.UseHttps = true;
			this.WebProxyPort = 8080;
		}

		/// <summary>
		/// User Streams を使うかどうかを取得または設定します。
		/// </summary>
		public bool EnableUserStreams
		{
			get;
			set;
		}

		/// <summary>
		/// HTTPS を使うかどうかを取得または設定します。
		/// </summary>
		public bool UseHttps
		{
			get
			{
				return TwitterUriBuilder.UseHttps;
			}
			set
			{
				TwitterUriBuilder.UseHttps = value;
			}
		}

		/// <summary>
		/// プロキシを取得または設定します。
		/// </summary>
		[Obsolete]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public IWebProxy WebProxy
		{
			get
			{
				return GetWebProxy();
			}
			set
			{
				if (value == null)
				{
					this.WebProxyPort = 8080;
					this.WebProxyHost =
						this.WebProxyUser =
						this.WebProxyPassword = null;
				}
				else if (value is WebProxy)
				{
					var px = (WebProxy)value;

					this.WebProxyHost = px.Address.Host;
					this.WebProxyPort = px.Address.Port;

					if (px.Credentials is NetworkCredential)
					{
						var cred = (NetworkCredential)px.Credentials;

						this.WebProxyUser = cred.UserName;
						this.WebProxyPassword = cred.Password;
					}
					else
					{
						this.WebProxyUser =
							this.WebProxyPassword = null;
					}
				}
			}
		}

		/// <summary>
		/// プロキシのインスタンスを生成します。
		/// </summary>
		/// <returns>プロキシのインスタンス。無い場合は null。</returns>
		public IWebProxy GetWebProxy()
		{
			if (!string.IsNullOrEmpty(this.WebProxyHost))
				return new WebProxy(new Uri("http://" + this.WebProxyHost + ":" + this.WebProxyPort))
				{
					Credentials = !string.IsNullOrEmpty(this.WebProxyUser) ? new NetworkCredential(this.WebProxyUser, this.WebProxyPassword) : null,
				};
			else
				return null;
		}

		/// <summary>
		/// プロキシのホスト名を取得または設定します。
		/// </summary>
		public string WebProxyHost
		{
			get;
			set;
		}

		/// <summary>
		/// プロキシのポートを取得または設定します。
		/// </summary>
		public int WebProxyPort
		{
			get;
			set;
		}

		/// <summary>
		/// プロキシのユーザ名を取得または設定します。
		/// </summary>
		public string WebProxyUser
		{
			get;
			set;
		}

		/// <summary>
		/// プロキのパスワードを取得または設定します。
		/// </summary>
		public string WebProxyPassword
		{
			get;
			set;
		}
	}

	/// <summary>
	/// タイムライン設定を表します。
	/// </summary>
	public class TimelineSettings : NotifyObject
	{
		/// <summary>
		/// TimelineSettings の新しいインスタンスを初期化します。
		/// </summary>
		public TimelineSettings()
		{
			this.BorderThickness = new Thickness(0, 0, 0, 1);
			this.MaxStatuses = 1000;
			this.AutoResetNewCount = true;
			this.ShowProtectedIcon = true;
			this.ShowOneWayFollowIcon = true;
			this.ShowIconShadowIfAvailable = true;
			this.ShowStatusShadow = true;
			this.AlternateStatusColor = Color.FromArgb(0xFF, 0xEF, 0xEF, 0xEF);
			this.NewStatusColor = Colors.PaleTurquoise;
			this.HighlightStatusRepliedBySelected = true;
			this.HighlightStatusRepliedBySelectedColor = Colors.LightPink;
			this.HighlightStatusReplyingToSelected = true;
			this.HighlightStatusReplyingToSelectedColor = Colors.PaleGoldenrod;
			this.HighlightStatusHavingSelectedSender = false;
			this.HighlightStatusHavingSelectedSenderColor = Colors.PaleGreen;
			this.HighlightFavorited = true;
			this.HighlightFavoritedColor = Color.FromArgb(255, 255, 255, 150);
			this.DoubleClickAction = ClickAction.ReplyTo;
		}

		/// <summary>
		/// 項目の枠の幅を取得または設定します。
		/// </summary>
		public Thickness BorderThickness
		{
			get;
			set;
		}

		/// <summary>
		/// 新着数表示を新着受信時に数え直すかを取得または設定します。
		/// </summary>
		public bool AutoResetNewCount
		{
			get;
			set;
		}

		/// <summary>
		/// タブを開いたときに新着数表示をリセットするかどうかを取得または設定します。
		/// </summary>
		public bool ResetNewCountOnTabShow
		{
			get;
			set;
		}

		/// <summary>
		/// タブを切り替えたときに新着数表示をリセットするかどうかを取得または設定します。
		/// </summary>
		public bool ResetNewCountOnTabHide
		{
			get;
			set;
		}

		/// <summary>
		///	エントリの最大保持数を取得または設定します。
		/// </summary>
		public int MaxStatuses
		{
			get;
			set;
		}

		/// <summary>
		/// protected アイコンを表示するかどうかを取得または設定します。
		/// </summary>
		public bool ShowProtectedIcon
		{
			get;
			set;
		}

		/// <summary>
		/// 未実装です。
		/// </summary>
		public bool ShowOneWayFollowIcon
		{
			get;
			set;
		}

		/// <summary>
		/// 可能な場合、アイコンの影を表示するかどうかを取得または設定します。
		/// </summary>
		public bool ShowIconShadowIfAvailable
		{
			get;
			set;
		}

		/// <summary>
		/// 項目の影を表示するかどうかを取得または設定します。
		/// </summary>
		public bool ShowStatusShadow
		{
			get;
			set;
		}

		/// <summary>
		/// 偶数項目の背景色を取得または設定します。
		/// </summary>
		public Color AlternateStatusColor
		{
			get;
			set;
		}

		/// <summary>
		/// 新着項目の背景色を取得または設定します。
		/// </summary>
		public Color NewStatusColor
		{
			get;
			set;
		}

		/// <summary>
		/// 選択した項目の返信先をハイライトするかどうかを取得または設定します。
		/// </summary>
		public bool HighlightStatusRepliedBySelected
		{
			get;
			set;
		}

		/// <summary>
		/// 選択した項目の返信先のハイライト色を取得または設定します。
		/// </summary>
		public Color HighlightStatusRepliedBySelectedColor
		{
			get;
			set;
		}

		/// <summary>
		/// 選択した項目に返信している項目をハイライトするかどうかを取得または設定します。
		/// </summary>
		public bool HighlightStatusReplyingToSelected
		{
			get;
			set;
		}

		/// <summary>
		/// 選択した項目に返信している項目のハイライト色を取得または設定します。
		/// </summary>
		public Color HighlightStatusReplyingToSelectedColor
		{
			get;
			set;
		}

		/// <summary>
		/// 選択した項目と同じ送信者の項目をハイライトするかどうかを取得または設定します。
		/// </summary>
		public bool HighlightStatusHavingSelectedSender
		{
			get;
			set;
		}

		/// <summary>
		/// 選択した項目と同じ送信者の項目のハイライト色を取得または設定します。
		/// </summary>
		public Color HighlightStatusHavingSelectedSenderColor
		{
			get;
			set;
		}

		/// <summary>
		/// お気に入り項目をハイライトするかどうかを取得または設定します。
		/// </summary>
		public bool HighlightFavorited
		{
			get;
			set;
		}

		/// <summary>
		/// お気に入り項目と同じ送信者の項目のハイライト色を取得または設定します。
		/// </summary>
		public Color HighlightFavoritedColor
		{
			get;
			set;
		}

		/// <summary>
		/// タイムラインの表示形式を取得または設定します。
		/// </summary>
		public TimelineStyle TimelineStyle
		{
			get
			{
				return GetValue(() => this.TimelineStyle);
			}
			set
			{
				SetValue(() => this.TimelineStyle, value);
			}
		}

		/// <summary>
		/// つぶやきのダブルクリック時の動作を取得または設定します。
		/// </summary>
		public ClickAction DoubleClickAction
		{
			get;
			set;
		}
	}

	/// <summary>
	/// インターフェイス設定を表します。
	/// </summary>
	public class InterfaceSettings
	{
		/// <summary>
		/// InterfaceSettings の新しいインスタンスを初期化します。
		/// </summary>
		public InterfaceSettings()
		{
			this.UseQuickSearch = true;
			this.UseIncrementalQuickSearch = true;
		}

		/// <summary>
		/// 投稿ボックスを非フォーカス時に畳むかどうかを取得または設定します。
		/// </summary>
		public bool CollapsePostBox
		{
			get;
			set;
		}

		/// <summary>
		/// Enter の代わりに Ctrl+Enter で投稿するかどうかを取得または設定します。
		/// </summary>
		public bool PostByControlEnter
		{
			get;
			set;
		}

		/// <summary>
		/// タスクトレイに最小化するかを取得または設定します。
		/// </summary>
		public bool MinimizeToTray
		{
			get;
			set;
		}

		/// <summary>
		/// タスクトレイに閉じるかどうかを取得または設定します。
		/// </summary>
		public bool CloseToTray
		{
			get;
			set;
		}

		/// <summary>
		/// クイック検索ボックスを使用するかどうかを取得または設定します。
		/// </summary>
		public bool UseQuickSearch
		{
			get;
			set;
		}

		/// <summary>
		/// クイック検索ボックスでインクリメンタル検索を使用するかどうかを取得または設定します。
		/// </summary>
		public bool UseIncrementalQuickSearch
		{
			get;
			set;
		}

		/// <summary>
		/// 逐次スクロール処理を省略するかどうかを取得または設定します。
		/// </summary>
		public bool UseDeferredScrolling
		{
			get;
			set;
		}

		/// <summary>
		/// アカウントボックスを左側に配置するかどうかを取得または設定します。
		/// </summary>
		[Obsolete]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public bool SwapAccountComboBoxPosition
		{
			get
			{
				return this.AccountBoxStyle == AccountBoxStyle.DropDownLeft;
			}
			set
			{
				this.AccountBoxStyle = value ? AccountBoxStyle.DropDownLeft : AccountBoxStyle.DropDownRight;
			}
		}

		/// <summary>
		/// アカウントボックスの形式を取得または設定します。
		/// </summary>
		public AccountBoxStyle AccountBoxStyle
		{
			get;
			set;
		}

		/// <summary>
		/// メインウィンドウの位置および大きさを取得または設定します。
		/// </summary>
		public Rect Bounds
		{
			get;
			set;
		}

		/// <summary>
		/// メインウィンドウの状態を取得または設定します。
		/// </summary>
		public WindowState WindowState
		{
			get;
			set;
		}

		/// <summary>
		/// 新着通知の位置を取得または設定します。
		/// </summary>
		public NotifyLocation NotifyLocation
		{
			get;
			set;
		}
	}

	/// <summary>
	/// 投稿設定を表します。
	/// </summary>
	public class PostSettings
	{
		/// <summary>
		/// PostSettings の新しいインスタンスを初期化します。
		/// </summary>
		public PostSettings()
		{
			this.Footers = new NotifyCollection<Footer>();
			this.AutoSwitchAccount = true;
		}

		/// <summary>
		/// フッタを取得します。
		/// </summary>
		public NotifyCollection<Footer> Footers
		{
			get;
			private set;
		}

		/// <summary>
		/// 未使用です。
		/// </summary>
		[Obsolete]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public bool CheckLastAccount
		{
			get;
			set;
		}

		/// <summary>
		/// 投稿時に自動的に URL を短縮するためのスクリプト名を取得または設定します。
		/// </summary>
		public string AutoShortenUrl
		{
			get;
			set;
		}

		/// <summary>
		/// 返信や Retweet 時などに自動的につぶやきを取得したアカウントに切り替えるかどうかを取得または設定します。
		/// </summary>
		public bool AutoSwitchAccount
		{
			get;
			set;
		}
	}

	/// <summary>
	/// キー設定を表します。
	/// </summary>
	public class KeySettings
	{
		/// <summary>
		/// KeySettings の新しいインスタンスを初期化します。
		/// </summary>
		public KeySettings()
		{
			this.CopyStatus = new KeyGesture(Key.C, ModifierKeys.Control);
			this.CopyStatusUri = new KeyGesture(Key.C, ModifierKeys.Control | ModifierKeys.Shift);
			this.CopyStatusUser = new KeyGesture(Key.C, ModifierKeys.Alt | ModifierKeys.Shift);
			this.DirectMessage = new KeyGesture(Key.D, ModifierKeys.Control);
			this.ReplyToStatus = new KeyGesture(Key.R, ModifierKeys.Control);
			this.QuoteStatus = new KeyGesture(Key.Q, ModifierKeys.Control);
			this.SearchStatuses = new KeyGesture(Key.F, ModifierKeys.Control);
			this.SearchQuick = new KeyGesture(Key.F, ModifierKeys.Control | ModifierKeys.Shift);
			this.DeleteStatus = new KeyGesture(Key.Delete);
			this.Refresh = new KeyGesture(Key.F5);
		}

		/// <summary>
		/// コピーのキーバインドを取得または設定します。
		/// </summary>
		public KeyGesture CopyStatus
		{
			get;
			set;
		}

		/// <summary>
		/// URL をコピーのキーバインドを取得または設定します。
		/// </summary>
		public KeyGesture CopyStatusUri
		{
			get;
			set;
		}

		/// <summary>
		/// ユーザ名をコピーのキーバインドを取得または設定します。
		/// </summary>
		public KeyGesture CopyStatusUser
		{
			get;
			set;
		}

		/// <summary>
		/// 本文をコピーのキーバインドを取得または設定します。
		/// </summary>
		public KeyGesture CopyStatusBody
		{
			get;
			set;
		}

		/// <summary>
		/// ダイレクトメッセージを送るのキーバインドを取得または設定します。
		/// </summary>
		public KeyGesture DirectMessage
		{
			get;
			set;
		}

		/// <summary>
		/// 削除のキーバインドを取得または設定します。
		/// </summary>
		public KeyGesture DeleteStatus
		{
			get;
			set;
		}

		/// <summary>
		/// 返信のキーバインドを取得または設定します。
		/// </summary>
		public KeyGesture ReplyToStatus
		{
			get;
			set;
		}

		/// <summary>
		/// Retweet のキーバインドを取得または設定します。
		/// </summary>
		public KeyGesture RetweetStatus
		{
			get;
			set;
		}

		/// <summary>
		/// 引用のキーバインドを取得または設定します。
		/// </summary>
		public KeyGesture QuoteStatus
		{
			get;
			set;
		}

		/// <summary>
		/// 返信履歴のキーバインドを取得または設定します。
		/// </summary>
		public KeyGesture ReplyToDetails
		{
			get;
			set;
		}

		/// <summary>
		/// お気に入りのキーバインドを取得または設定します。
		/// </summary>
		public KeyGesture FavoriteStatus
		{
			get;
			set;
		}

		/// <summary>
		/// 更新のキーバインドを取得または設定します。
		/// </summary>
		public KeyGesture Refresh
		{
			get;
			set;
		}

		/// <summary>
		/// つぶやきの検索のキーバインドを取得または設定します。
		/// </summary>
		public KeyGesture SearchStatuses
		{
			get;
			set;
		}

		/// <summary>
		/// ユーザの検索のキーバインドを取得または設定します。
		/// </summary>
		public KeyGesture SearchUsers
		{
			get;
			set;
		}

		/// <summary>
		/// キャッシュの検索のキーバインドを取得または設定します。
		/// </summary>
		public KeyGesture SearchCache
		{
			get;
			set;
		}

		/// <summary>
		/// キャッシュへクエリのキーバインドを取得または設定します。
		/// </summary>
		public KeyGesture QueryCache
		{
			get;
			set;
		}

		/// <summary>
		/// クイック検索のキーバインドを取得または設定します。
		/// </summary>
		public KeyGesture SearchQuick
		{
			get;
			set;
		}
	}
}
