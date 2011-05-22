using Codeplex.Data;

namespace Lunar
{
	/// <summary>
	/// ユーザの関係を表します。
	/// </summary>
	public class Relationship : ServiceObject
	{
		/// <summary>
		/// 基になる JSON オブジェクトを指定し Relationship の新しいインスタンスを初期化します。
		/// </summary>
		/// <param name="json">基になる JSON オブジェクト。</param>
		public Relationship(DynamicJson json)
			: base(json)
		{
		}

		/// <summary>
		/// ソースのユーザ ID を取得します。
		/// </summary>
		public UserID SourceUserID
		{
			get
			{
				return long.Parse(json.source.id_str);
			}
		}

		/// <summary>
		/// ソースのユーザ名を取得します。
		/// </summary>
		public string SourceUserName
		{
			get
			{
				return json.source.screen_name;
			}
		}

		/// <summary>
		/// ソースが対象をフォローしているかどうかを取得します。
		/// </summary>
		public bool SourceFollowingTarget
		{
			get
			{
				return json.source.following;
			}
		}

		/// <summary>
		/// ソースが対象をブロックしているかどうかを取得します。
		/// </summary>
		public bool SourceBlockingTarget
		{
			get
			{
				return json.source.blocking() ? json.source.blocking : false;
			}
		}

		/// <summary>
		/// 対象がソースをフォローしているかどうかを取得します。
		/// </summary>
		public bool TargetFollowingSource
		{
			get
			{
				return json.source.followed_by;
			}
		}

		/// <summary>
		/// 対象のユーザ ID を取得します。
		/// </summary>
		public UserID TargetUserID
		{
			get
			{
				return long.Parse(json.target.id_str);
			}
		}

		/// <summary>
		/// 対象のユーザ名を取得します。
		/// </summary>
		public string TargetUserName
		{
			get
			{
				return json.target.screen_name;
			}
		}
	}
}
