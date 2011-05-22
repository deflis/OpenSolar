using System;
using System.Collections.Generic;

namespace Lunar
{
	/// <summary>
	/// 同一要素へのリクエストのキャッシングを指示します。
	/// </summary>
	public class CacheScope : IDisposable
	{
		Dictionary<object, string> cache = new Dictionary<object, string>();

		/// <summary>
		/// 現在の CacheScope のコンテキストを取得します。
		/// </summary>
		public static CacheScope Current
		{
			get;
			private set;
		}

		/// <summary>
		/// CacheScope の新しいインスタンスを初期化します。
		/// </summary>
		public CacheScope()
		{
			Current = this;
		}

		/// <summary>
		/// この CacheScope を使用し指定したキー要素に割り当てられたキャッシュを取得するか、指定された外部取得処理を実行しキャッシュします。
		/// </summary>
		/// <typeparam name="T">キー要素の型。</typeparam>
		/// <param name="key">探索または割り当てるキャッシュのキー要素。</param>
		/// <param name="body">指定したキー要素がキャッシュされていなかった場合実行される取得処理。</param>
		/// <returns>指定したキー要素に割り当てられたキャッシュが存在する場合はそのキャッシュ。無い場合は指定された外部取得処理の取得結果。</returns>
		public string ReadWithCaching<T>(T key, Func<T, string> body)
		{
			lock (this)
				return cache.ContainsKey(key) ? cache[key] : cache[key] = body(key);
		}

		/// <summary>
		/// 同一要素へのリクエストのキャッシングを終了します。
		/// </summary>
		public void Dispose()
		{
			Current = null;
			GC.SuppressFinalize(this);
		}

		/// <summary>
		/// dtor
		/// </summary>
		~CacheScope()
		{
			Dispose();
		}
	}
}
