namespace Solar
{
	/// <summary>
	/// 進行状況表示コンテナを表します。
	/// </summary>
	public interface IProgressHost
	{
		/// <summary>
		/// 現在の進行状況を取得または設定します。
		/// </summary>
		ProgressBlock CurrentProgress
		{
			get;
			set;
		}
	}
}
