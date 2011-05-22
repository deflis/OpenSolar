using System;

namespace Lunar
{
	/// <summary>
	/// 非認証状態での取得の活用を指示します。
	/// </summary>
	public class ReduceAuthenticatedQueryScope : IDisposable
	{
		/// <summary>
		/// 現在の ReduceAuthenticatedQueryScope コンテキストを取得します。
		/// </summary>
		public static ReduceAuthenticatedQueryScope Current
		{
			get;
			private set;
		}

		/// <summary>
		/// ReduceAuthenticatedQueryScope の新しいインスタンスを初期化します。
		/// </summary>
		public ReduceAuthenticatedQueryScope()
		{
			Current = this;
		}

		/// <summary>
		/// 非認証状態での取得の活用を終了します。
		/// </summary>
		public void Dispose()
		{
			Current = null;
			GC.SuppressFinalize(this);
		}

		/// <summary>
		/// dtor
		/// </summary>
		~ReduceAuthenticatedQueryScope()
		{
			Dispose();
		}
	}
}
