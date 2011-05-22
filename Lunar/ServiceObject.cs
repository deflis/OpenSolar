using Codeplex.Data;
using Ignition;

namespace Lunar
{
	/// <summary>
	/// 外部サービスのオブジェクトを表します。
	/// </summary>
	public abstract class ServiceObject : NotifyObject
	{
		/// <summary>
		/// 基になる JSON オブジェクトを取得します。
		/// </summary>
		protected readonly dynamic json;

		/// <summary>
		/// 基になる JSON オブジェクトを指定し ServiceObject の新しいインスタンスを初期化します。
		/// </summary>
		/// <param name="json">基になる JSON オブジェクト。</param>
		protected ServiceObject(DynamicJson json)
		{
			this.json = json;
		}
	}
}
