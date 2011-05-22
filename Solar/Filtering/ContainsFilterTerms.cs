using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using Lunar;

namespace Solar.Filtering
{
	/// <summary>
	/// キーワード フィルタ項目を表します。
	/// </summary>
	public class ContainsFilterTerms : FilterTerms
	{
		/// <summary>
		/// キーワードを取得または設定します。
		/// </summary>
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		public Collection<string> Keywords
		{
			get;
			set;
		}

		/// <summary>
		/// ContainsFilterTerms の新しいインスタンスを初期化します。
		/// </summary>
		public ContainsFilterTerms()
		{
			this.Keywords = new Collection<string>();
		}

		/// <summary>
		/// 条件に項目が一致するかどうかを判断します。
		/// </summary>
		/// <param name="entry">エントリ。</param>
		/// <returns>条件に一致するかどうか。</returns>
		public override bool FilterStatus(IEntry entry)
		{
			return entry.Text != null
				&& this.Keywords.Any(entry.Text.Contains);
		}
	}
}
