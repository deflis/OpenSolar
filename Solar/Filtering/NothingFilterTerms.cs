using System.Collections.Generic;
using System.Linq;
using Ignition;
using Lunar;

namespace Solar.Filtering
{
	/// <summary>
	/// なし フィルタ項目を表します。
	/// </summary>
	public class NothingFilterTerms : FilterTerms
	{
		/// <summary>
		/// なにもしません処理を実行します。
		/// </summary>
		/// <param name="entries">フィルタ対象。</param>
		/// <returns>フィルタ結果。実際のところ、引数をそのまま返すだけです。</returns>
		public override IEnumerable<IEntry> FilterStatuses(IEnumerable<IEntry> entries)
		{
			return entries;
		}

		/// <summary>
		/// なにもしない条件に項目が一致するかどうかを判断しなにもしませんます。
		/// </summary>
		/// <param name="entry">エントリ。</param>
		/// <returns>条件に一致するかどうか。実際のところ、いつでも true です。</returns>
		public override bool FilterStatus(IEntry entry)
		{
			return true;
		}
	}
}
