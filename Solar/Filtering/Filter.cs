using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Markup;
using System.Xaml;
using Ignition;
using Ignition.Presentation;
using Lunar;
using Solar.Models;

namespace Solar.Filtering
{
	/// <summary>
	/// エントリのフィルタリング機能を提供します。
	/// </summary>
	[ContentProperty("Sources")]
	public class Filter : ICloneable
	{
		/// <summary>
		/// Filter の新しいインスタンスを初期化します。
		/// </summary>
		public Filter()
		{
			this.Sources = new NotifyCollection<FilterSource>();
			this.Terms = new NotifyCollection<FilterTerms>();
		}

		/// <summary>
		/// フィルタ ソースを取得または設定します。
		/// </summary>
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		public NotifyCollection<FilterSource> Sources
		{
			get;
			set;
		}

		/// <summary>
		/// フィルタ項目を取得または設定します。
		/// </summary>
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		public NotifyCollection<FilterTerms> Terms
		{
			get;
			set;
		}

		/// <summary>
		/// クライアントと取得範囲および使用するフィルタ ソースを指定してこのフィルタからエントリを取得しこのカテゴリを更新します。
		/// </summary>
		/// <param name="client">クライアント。</param>
		/// <param name="range">取得範囲。</param>
		/// <param name="useSource">使用するフィルタ ソースの条件。</param>
		/// <returns>取得したエントリ。</returns>
		public IEnumerable<IEntry> GetStatuses(TwitterClient client, StatusRange range, Func<FilterSource, bool> useSource)
		{
			if (range == null)
				range = new StatusRange();

			return FilterStatuses(this.Sources.Where(useSource).SelectMany(_ => _.GetStatusesFromSource(client, range)));
		}

		/// <summary>
		/// 指定したエントリをフィルタ項目に従ってフィルタします。
		/// </summary>
		/// <param name="statuses">エントリ。</param>
		/// <returns>フィルタされたエントリ。</returns>
		public IEnumerable<IEntry> FilterStatuses(IEnumerable<IEntry> statuses)
		{
			return Settings.Default.GlobalFilterTerms.Concat(this.Terms).Aggregate(statuses, (from, i) => i.FilterStatuses(from)).Distinct();
		}

		/// <summary>
		/// 現在のインスタンスのコピーである新しいオブジェクトを作成します。
		/// </summary>
		/// <returns>このインスタンスのコピーである新しいオブジェクト。</returns>
		public Filter Clone()
		{
			return (Filter)XamlServices.Parse(XamlServices.Save(this));
		}

		object ICloneable.Clone()
		{
			return Clone();
		}
	}
}
