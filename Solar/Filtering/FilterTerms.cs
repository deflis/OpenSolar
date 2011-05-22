using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Ignition;
using Lunar;

namespace Solar.Filtering
{
	/// <summary>
	/// フィルタ項目を表します。
	/// </summary>
	public abstract class FilterTerms
	{
		/// <summary>
		/// 抽出ではなく、除外するかどうかを取得または設定します。
		/// </summary>
		[DefaultValue(false)]
		public bool Except
		{
			get;
			set;
		}

		/// <summary>
		/// 条件に項目が一致するかどうかを判断します。
		/// </summary>
		/// <param name="entry">エントリ。</param>
		/// <returns>条件に一致するかどうか。</returns>
		public abstract bool FilterStatus(IEntry entry);

		/// <summary>
		/// フィルタ処理を実行します。
		/// </summary>
		/// <param name="entries">フィルタ対象。</param>
		/// <returns>フィルタ結果。</returns>
		public virtual IEnumerable<IEntry> FilterStatuses(IEnumerable<IEntry> entries)
		{
			return this.Except
				? entries.Where(_ => !FilterStatus(_))
				: entries.Where(FilterStatus);
		}
	}
}
