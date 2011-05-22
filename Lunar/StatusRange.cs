namespace Lunar
{
	/// <summary>
	/// 取得範囲を表します。
	/// </summary>
	public class StatusRange
	{
		/// <summary>
		/// StatusRange の新しいインスタンスを初期化します。
		/// </summary>
		public StatusRange()
		{
			this.Page = 1;
			this.Count = 50;
		}

		/// <summary>
		/// 各種パラメータを指定し StatusRange の新しいインスタンスを初期化します。
		/// </summary>
		/// <param name="sinceID">since_id 指定子。</param>
		/// <param name="maxID">max_id 指定子。</param>
		/// <param name="count">count 指定子。</param>
		/// <param name="page">page 指定子。</param>
		public StatusRange(StatusID sinceID = default(StatusID), StatusID maxID = default(StatusID), int count = 50, int page = 1)
			: this()
		{
			this.SinceID = sinceID;
			this.MaxID = maxID;
			this.Count = count;
			this.Page = page;
		}

		/// <summary>
		/// since_id 指定子を取得または設定します。
		/// </summary>
		public StatusID SinceID
		{
			get;
			set;
		}

		/// <summary>
		/// max_id 指定子を取得または設定します。
		/// </summary>
		public StatusID MaxID
		{
			get;
			set;
		}

		/// <summary>
		/// count 指定子を取得または設定します。
		/// </summary>
		public int Count
		{
			get;
			set;
		}

		/// <summary>
		/// page 指定子を取得または設定します。
		/// </summary>
		public int Page
		{
			get;
			set;
		}
	}
}
