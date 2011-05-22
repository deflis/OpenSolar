using System.Collections.Generic;
using Ignition.Presentation;

namespace Solar.Models
{
	/// <summary>
	/// カテゴリのグループを表します。これは一般的にカラムです。
	/// </summary>
	public class CategoryGroup : NotifyCollection<Category>
	{
		/// <summary>
		/// CategoryGroup の新しいインスタンスを初期化します。
		/// </summary>
		public CategoryGroup()
		{
		}

		/// <summary>
		/// 指定したコレクションからコピーされる要素を格納した CategoryGroup の新しいインスタンスを初期化します。
		/// </summary>
		/// <param name="collection">元になるコレクション。</param>
		public CategoryGroup(IEnumerable<Category> collection)
			: base(collection)
		{
		}
	}
}
