using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using Lunar;

namespace Solar.Filtering
{
	/// <summary>
	/// ユーザ フィルタ項目を表します。
	/// </summary>
	public class UserFilterTerms : FilterTerms
	{
		/// <summary>
		/// ユーザを取得または設定します。
		/// </summary>
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		public ICollection<string> Users
		{
			get;
			set;
		}

		/// <summary>
		/// UserFilterTerms の新しいインスタンスを初期化します。
		/// </summary>
		public UserFilterTerms()
		{
			this.Users = new Collection<string>();
		}

		/// <summary>
		/// 条件に項目が一致するかどうかを判断します。
		/// </summary>
		/// <param name="entry">エントリ。</param>
		/// <returns>条件に一致するかどうか。</returns>
		public override bool FilterStatus(IEntry entry)
		{
			return this.Users.Contains(entry.UserName);
		}
	}
}
