using System.Collections.ObjectModel;
using System.ComponentModel;
using Ignition;
using Lunar;

namespace Solar.Filtering
{
	/// <summary>
	/// ソース フィルタ項目を表します。
	/// </summary>
	public class SourceFilterTerms : FilterTerms
	{
		/// <summary>
		/// ソースを取得または設定します。
		/// </summary>
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		public Collection<string> Sources
		{
			get;
			set;
		}

		/// <summary>
		/// SourceFilterTerms の新しいインスタンスを初期化します。
		/// </summary>
		public SourceFilterTerms()
		{
			this.Sources = new Collection<string>();
		}

		/// <summary>
		/// 条件に項目が一致するかどうかを判断します。
		/// </summary>
		/// <param name="entry">エントリ。</param>
		/// <returns>条件に一致するかどうか。</returns>
		public override bool FilterStatus(IEntry entry)
		{
			return entry.TypeMatch
			(
				(Status _) => this.Sources.Contains(_.SourceName)
						   || _.SourceUri != null && this.Sources.Contains(_.SourceUri.AbsoluteUri),
				_ => false
			);
		}
	}
}
