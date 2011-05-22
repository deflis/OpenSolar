namespace Solar.Models
{
	/// <summary>
	/// クリックアクション
	/// </summary>
	public enum ClickAction
	{
		/// <summary>
		/// なし
		/// </summary>
		None,
		/// <summary>
		/// コピー
		/// </summary>
		Copy,
		/// <summary>
		/// 返信
		/// </summary>
		ReplyTo,
		/// <summary>
		/// 引用
		/// </summary>
		Quote,
		/// <summary>
		/// お気に入りトグル
		/// </summary>
		Favorite,
		/// <summary>
		/// ブラウザで開く
		/// </summary>
		StatusDetails,
		/// <summary>
		/// ユーザ情報を開く
		/// </summary>
		UserDetails,
		/// <summary>
		/// 付近のつぶやきを取得
		/// </summary>
		NearStatuses,
	}
}
