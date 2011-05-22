using System.Windows;
using System.Windows.Input;

namespace Solar
{
	/// <summary>
	/// StatusesListBox で使用するコマンド コレクションを表します。
	/// </summary>
	public class StatusesListBoxCommandHandler : DependencyObject
	{
		/// <summary>
		/// DoubleClickCommand プロパティ
		/// </summary>
		public static readonly DependencyProperty DoubleClickCommandProperty = DependencyProperty.Register("DoubleClickCommand", typeof(ICommand), typeof(StatusesListBox), new UIPropertyMetadata(null));
		/// <summary>
		/// DoubleClickStatusCommand プロパティ
		/// </summary>
		public static readonly DependencyProperty DoubleClickStatusCommandProperty = DependencyProperty.Register("DoubleClickStatusCommand", typeof(ICommand), typeof(StatusesListBox), new UIPropertyMetadata(null));
		/// <summary>
		/// CopyStatusCommand プロパティ
		/// </summary>
		public static readonly DependencyProperty CopyStatusCommandProperty = DependencyProperty.Register("CopyStatusCommand", typeof(ICommand), typeof(StatusesListBox), new UIPropertyMetadata(null));
		/// <summary>
		/// CopyStatusUriCommand プロパティ
		/// </summary>
		public static readonly DependencyProperty CopyStatusUriCommandProperty = DependencyProperty.Register("CopyStatusUriCommand", typeof(ICommand), typeof(StatusesListBox), new UIPropertyMetadata(null));
		/// <summary>
		/// CopyStatusUserCommand プロパティ
		/// </summary>
		public static readonly DependencyProperty CopyStatusUserCommandProperty = DependencyProperty.Register("CopyStatusUserCommand", typeof(ICommand), typeof(StatusesListBox), new UIPropertyMetadata(null));
		/// <summary>
		/// CopyStatusBodyCommand プロパティ
		/// </summary>
		public static readonly DependencyProperty CopyStatusBodyCommandProperty = DependencyProperty.Register("CopyStatusBodyCommand", typeof(ICommand), typeof(StatusesListBox), new UIPropertyMetadata(null));
		/// <summary>
		/// ReplyToStatusCommand プロパティ
		/// </summary>
		public static readonly DependencyProperty ReplyToStatusCommandProperty = DependencyProperty.Register("ReplyToStatusCommand", typeof(ICommand), typeof(StatusesListBox), new UIPropertyMetadata(null));
		/// <summary>
		/// RetweetStatusCommand プロパティ
		/// </summary>
		public static readonly DependencyProperty RetweetStatusCommandProperty = DependencyProperty.Register("RetweetStatusCommand", typeof(ICommand), typeof(StatusesListBox), new UIPropertyMetadata(null));
		/// <summary>
		/// QuoteStatusCommand プロパティ
		/// </summary>
		public static readonly DependencyProperty QuoteStatusCommandProperty = DependencyProperty.Register("QuoteStatusCommand", typeof(ICommand), typeof(StatusesListBox), new UIPropertyMetadata(null));
		/// <summary>
		/// FavoriteStatusCommand プロパティ
		/// </summary>
		public static readonly DependencyProperty FavoriteStatusCommandProperty = DependencyProperty.Register("FavoriteStatusCommand", typeof(ICommand), typeof(StatusesListBox), new UIPropertyMetadata(null));
		/// <summary>
		/// DeleteStatusCommand プロパティ
		/// </summary>
		public static readonly DependencyProperty DeleteStatusCommandProperty = DependencyProperty.Register("DeleteStatusCommand", typeof(ICommand), typeof(StatusesListBox), new UIPropertyMetadata(null));
		/// <summary>
		/// StatusDetailsCommand プロパティ
		/// </summary>
		public static readonly DependencyProperty StatusDetailsCommandProperty = DependencyProperty.Register("StatusDetailsCommand", typeof(ICommand), typeof(StatusesListBox), new UIPropertyMetadata(null));
		/// <summary>
		/// UserDetailsCommand プロパティ
		/// </summary>
		public static readonly DependencyProperty UserDetailsCommandProperty = DependencyProperty.Register("UserDetailsCommand", typeof(ICommand), typeof(StatusesListBox), new UIPropertyMetadata(null));
		/// <summary>
		/// ReplyToDetailsCommand プロパティ
		/// </summary>
		public static readonly DependencyProperty ReplyToDetailsCommandProperty = DependencyProperty.Register("ReplyToDetailsCommand", typeof(ICommand), typeof(StatusesListBox), new UIPropertyMetadata(null));
		/// <summary>
		///  OpenUriCommand プロパティ
		/// </summary>
		public static readonly DependencyProperty OpenUriCommandProperty = DependencyProperty.Register("OpenUriCommand", typeof(ICommand), typeof(StatusesListBox), new UIPropertyMetadata(null));
		/// <summary>
		/// SearchCommand プロパティ
		/// </summary>
		public static readonly DependencyProperty SearchCommandProperty = DependencyProperty.Register("SearchCommand", typeof(ICommand), typeof(StatusesListBox), new UIPropertyMetadata(null));
		/// <summary>
		/// DirectMessageCommand プロパティ
		/// </summary>
		public static readonly DependencyProperty DirectMessageCommandProperty = DependencyProperty.Register("DirectMessageCommand", typeof(ICommand), typeof(StatusesListBox), new UIPropertyMetadata(null));
		/// <summary>
		/// NearStatusesCommand プロパティ
		/// </summary>
		public static readonly DependencyProperty NearStatusesCommandProperty = DependencyProperty.Register("NearStatusesCommand", typeof(ICommand), typeof(StatusesListBox), new UIPropertyMetadata(null));
		/// <summary>
		/// OpenListCommand プロパティ
		/// </summary>
		public static readonly DependencyProperty OpenListCommandProperty = DependencyProperty.Register("OpenListCommand", typeof(ICommand), typeof(StatusesListBox), new UIPropertyMetadata(null));
		
		/// <summary>
		/// 項目がダブルクリックされたときのコマンドを取得または設定します。
		/// </summary>
		public ICommand DoubleClickCommand
		{
			get
			{
				return (ICommand)GetValue(DoubleClickCommandProperty);
			}
			set
			{
				SetValue(DoubleClickCommandProperty, value);
			}
		}
		/// <summary>
		/// Status がダブルクリックされたときのコマンドを取得または設定します。
		/// </summary>
		public ICommand DoubleClickStatusCommand
		{
			get
			{
				return (ICommand)GetValue(DoubleClickStatusCommandProperty);
			}
			set
			{
				SetValue(DoubleClickStatusCommandProperty, value);
			}
		}
		/// <summary>
		/// コピー コマンドを取得または設定します。
		/// </summary>
		public ICommand CopyStatusCommand
		{
			get
			{
				return (ICommand)GetValue(CopyStatusCommandProperty);
			}
			set
			{
				SetValue(CopyStatusCommandProperty, value);
			}
		}
		/// <summary>
		/// URL コピー コマンドを取得または設定します。
		/// </summary>
		public ICommand CopyStatusUriCommand
		{
			get
			{
				return (ICommand)GetValue(CopyStatusUriCommandProperty);
			}
			set
			{
				SetValue(CopyStatusUriCommandProperty, value);
			}
		}
		/// <summary>
		/// ユーザ コピー コマンドを取得または設定します。
		/// </summary>
		public ICommand CopyStatusUserCommand
		{
			get
			{
				return (ICommand)GetValue(CopyStatusUserCommandProperty);
			}
			set
			{
				SetValue(CopyStatusUserCommandProperty, value);
			}
		}
		/// <summary>
		/// 本文 コピー コマンドを取得または設定します。
		/// </summary>
		public ICommand CopyStatusBodyCommand
		{
			get
			{
				return (ICommand)GetValue(CopyStatusBodyCommandProperty);
			}
			set
			{
				SetValue(CopyStatusBodyCommandProperty, value);
			}
		}
		/// <summary>
		/// 返信 コマンドを取得または設定します。
		/// </summary>
		public ICommand ReplyToStatusCommand
		{
			get
			{
				return (ICommand)GetValue(ReplyToStatusCommandProperty);
			}
			set
			{
				SetValue(ReplyToStatusCommandProperty, value);
			}
		}
		/// <summary>
		/// Retweet コマンドを取得または設定します。
		/// </summary>
		public ICommand RetweetStatusCommand
		{
			get
			{
				return (ICommand)GetValue(RetweetStatusCommandProperty);
			}
			set
			{
				SetValue(RetweetStatusCommandProperty, value);
			}
		}
		/// <summary>
		/// 引用 コマンドを取得または設定します。
		/// </summary>
		public ICommand QuoteStatusCommand
		{
			get
			{
				return (ICommand)GetValue(QuoteStatusCommandProperty);
			}
			set
			{
				SetValue(QuoteStatusCommandProperty, value);
			}
		}
		/// <summary>
		/// お気に入りトグル コマンドを取得または設定します。
		/// </summary>
		public ICommand FavoriteStatusCommand
		{
			get
			{
				return (ICommand)GetValue(FavoriteStatusCommandProperty);
			}
			set
			{
				SetValue(FavoriteStatusCommandProperty, value);
			}
		}
		/// <summary>
		/// 削除 コマンドを取得または設定します。
		/// </summary>
		public ICommand DeleteStatusCommand
		{
			get
			{
				return (ICommand)GetValue(DeleteStatusCommandProperty);
			}
			set
			{
				SetValue(DeleteStatusCommandProperty, value);
			}
		}
		/// <summary>
		/// ブラウザで開く コマンドを取得または設定します。
		/// </summary>
		public ICommand StatusDetailsCommand
		{
			get
			{
				return (ICommand)GetValue(StatusDetailsCommandProperty);
			}
			set
			{
				SetValue(StatusDetailsCommandProperty, value);
			}
		}
		/// <summary>
		/// ユーザ情報を開く コマンドを取得または設定します。
		/// </summary>
		public ICommand UserDetailsCommand
		{
			get
			{
				return (ICommand)GetValue(UserDetailsCommandProperty);
			}
			set
			{
				SetValue(UserDetailsCommandProperty, value);
			}
		}
		/// <summary>
		/// 返信履歴 コマンドを取得または設定します。
		/// </summary>
		public ICommand ReplyToDetailsCommand
		{
			get
			{
				return (ICommand)GetValue(ReplyToDetailsCommandProperty);
			}
			set
			{
				SetValue(ReplyToDetailsCommandProperty, value);
			}
		}
		/// <summary>
		/// URL を開く コマンドを取得または設定します。
		/// </summary>
		public ICommand OpenUriCommand
		{
			get
			{
				return (ICommand)GetValue(OpenUriCommandProperty);
			}
			set
			{
				SetValue(OpenUriCommandProperty, value);
			}
		}
		/// <summary>
		/// 指定した文字列を広域検索 コマンドを取得または設定します。
		/// </summary>
		public ICommand SearchCommand
		{
			get
			{
				return (ICommand)GetValue(SearchCommandProperty);
			}
			set
			{
				SetValue(SearchCommandProperty, value);
			}
		}
		/// <summary>
		/// ダイレクトメッセージを送信 コマンドを取得または設定します。
		/// </summary>
		public ICommand DirectMessageCommand
		{
			get
			{
				return (ICommand)GetValue(DirectMessageCommandProperty);
			}
			set
			{
				SetValue(DirectMessageCommandProperty, value);
			}
		}
		/// <summary>
		/// 付近のつぶやきを取得 コマンドを取得または設定します。
		/// </summary>
		public ICommand NearStatusesCommand
		{
			get
			{
				return (ICommand)GetValue(NearStatusesCommandProperty);
			}
			set
			{
				SetValue(NearStatusesCommandProperty, value);
			}
		}
		/// <summary>
		/// リストを開く コマンドを取得または設定します。
		/// </summary>
		public ICommand OpenListCommand
		{
			get
			{
				return (ICommand)GetValue(OpenListCommandProperty);
			}
			set
			{
				SetValue(OpenListCommandProperty, value);
			}
		}
	}
}
