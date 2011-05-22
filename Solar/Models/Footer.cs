namespace Solar.Models
{
	/// <summary>
	/// フッタ
	/// </summary>
	public class Footer
	{
		/// <summary>
		/// Footer の新しいインスタンスを初期化します。
		/// </summary>
		public Footer()
		{
		}

		/// <summary>
		/// フッタ文字列を指定し Footer の新しいインスタンスを初期化します。
		/// </summary>
		/// <param name="text">フッタ文字列。</param>
		public Footer(string text)
		{
			this.Text = text;
		}

		/// <summary>
		/// フッタ文字列
		/// </summary>
		public string Text
		{
			get;
			set;
		}

		/// <summary>
		/// フッタとして使用するかどうか
		/// </summary>
		public bool Use
		{
			get;
			set;
		}

		/// <summary>
		/// フッタ文字列を取得します。
		/// </summary>
		/// <returns>フッタ文字列</returns>
		public override string ToString()
		{
			return this.Text;
		}
	}
}
