using System;

namespace Lunar
{
	/// <summary>
	/// 各種エントリで実装する、エントリの基本情報を定義します。
	/// </summary>
	public interface IEntry : IEquatable<IEntry>
	{
		/// <summary>
		/// ユーザ名を取得します。
		/// </summary>
		string UserName
		{
			get;
		}

		/// <summary>
		/// 本文を取得します。
		/// </summary>
		string Text
		{
			get;
		}

		/// <summary>
		/// 生成日時を取得します。
		/// </summary>
		DateTime CreatedAt
		{
			get;
		}

		/// <summary>
		/// 取得したアカウントを取得します。
		/// </summary>
		AccountToken Account
		{
			get;
		}

		/// <summary>
		/// ID を取得します。
		/// </summary>
		long ID
		{
			get;
		}
	}
}
