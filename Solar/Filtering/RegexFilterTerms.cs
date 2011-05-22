using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text.RegularExpressions;
using Ignition;
using Lunar;

namespace Solar.Filtering
{
	/// <summary>
	/// 正規表現 フィルタ項目を表します。
	/// </summary>
	public class RegexFilterTerms : FilterTerms
	{
		IList<Regex> regex;

		/// <summary>
		/// パターンを取得または設定します。
		/// </summary>
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		public Collection<string> Patterns
		{
			get;
			set;
		}

		/// <summary>
		/// RegexFilterTerms の新しいインスタンスを初期化します。
		/// </summary>
		public RegexFilterTerms()
		{
			this.Patterns = new Collection<string>();
		}

		/// <summary>
		/// 条件に項目が一致するかどうかを判断します。
		/// </summary>
		/// <param name="entries">エントリ。</param>
		/// <returns>条件に一致するかどうか。</returns>
		public override IEnumerable<IEntry> FilterStatuses(IEnumerable<IEntry> entries)
		{
			regex = this.Patterns.Select(_ =>
			{
				try
				{
					return new Regex(_, RegexOptions.Compiled);
				}
				catch
				{
					return null;
				}
			})
			.Where(_ => _ != null)
			.Freeze();

			return base.FilterStatuses(entries);
		}

		/// <summary>
		/// フィルタ処理を実行します。
		/// </summary>
		/// <param name="entry">フィルタ対象。</param>
		/// <returns>フィルタ結果。</returns>
		public override bool FilterStatus(IEntry entry)
		{
			return regex.Any(_ => _.IsMatch(entry.Text));
		}
	}
}
