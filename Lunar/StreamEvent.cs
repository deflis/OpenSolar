using System;

namespace Lunar
{
	/// <summary>
	/// User Streams イベントを表します。
	/// </summary>
	public class StreamEvent : IEntry
	{
		TwitterStreamEventArgs e;

		/// <summary>
		/// イベント データを指定し StreamEvent の新しいインスタンスを初期化します。
		/// </summary>
		/// <param name="e">イベント データ。</param>
		public StreamEvent(TwitterStreamEventArgs e)
		{
			this.e = e;
		}

		/// <summary>
		/// ユーザ名を取得します。
		/// </summary>
		public string UserName
		{
			get
			{
				return e.Source.Name;
			}
		}

		/// <summary>
		/// 本文を取得します。
		/// </summary>
		public string Text
		{
			get
			{
				return "@" + e.Source.Name + " " + e.Type + " @" + e.Target.Name
					+ (e.TargetStatus == null ? null : e.TargetStatus.Text);
			}
		}

		/// <summary>
		/// 発生日時を取得します。
		/// </summary>
		public DateTime CreatedAt
		{
			get
			{
				return e.CreatedAt;
			}
		}

		/// <summary>
		/// 取得したアカウントを取得します。
		/// </summary>
		public AccountToken Account
		{
			get
			{
				return e.Account;
			}
		}

		/// <summary>
		/// ID を取得します。
		/// </summary>
		public long ID
		{
			get
			{
				return (long)(e.CreatedAt - DateTime.MinValue).TotalMilliseconds;
			}
		}

		/// <summary>
		/// 指定した IEntry が現在のインスタンスと等しいかどうか判断します。
		/// </summary>
		/// <param name="other">現在の StreamEvent と比較する IEntry。</param>
		/// <returns>等しいかどうか。</returns>
		public bool Equals(IEntry other)
		{
			return other is StreamEvent
				&& other.ID == this.ID;
		}
	}
}
