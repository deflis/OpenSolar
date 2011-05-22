using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Dynamic;
using Lunar;

namespace Solar.Filtering
{
	/// <summary>
	/// WHERE フィルタ項目を表します。
	/// </summary>
	public class WhereFilterTerms : FilterTerms
	{
		/// <summary>
		/// フィルタ条件を取得または設定します。
		/// </summary>
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		public string Predicate
		{
			get;
			set;
		}

		/// <summary>
		/// 条件に項目が一致するかどうかを判断します。
		/// </summary>
		/// <param name="entries">エントリ。</param>
		/// <returns>条件に一致するかどうか。</returns>
		public override IEnumerable<IEntry> FilterStatuses(IEnumerable<IEntry> entries)
		{
			return entries.OfType<Status>()
						  .AsQueryable()
						  .Where(this.Except ? string.Format("not ({0})", this.Predicate) : this.Predicate)
						  .Concat(entries.Where(_ => !(_ is Status)));
		}

		/// <summary>
		/// フィルタ処理を実行します。
		/// </summary>
		/// <param name="entry">フィルタ対象。</param>
		/// <returns>フィルタ結果。</returns>
		public override bool FilterStatus(IEntry entry)
		{
			return true;
		}
	}
}
