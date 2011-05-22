using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Markup;
using Ignition;
using Ignition.Linq;
using Ignition.Presentation;
using Lunar;
using Solar.Filtering;

namespace Solar.Models
{
	/// <summary>
	/// カテゴリを表します。これは一般的にタブです。
	/// </summary>
	[ContentProperty("Filter")]
	public class Category : NotifyObject, ICloneable
	{
		static readonly HashSet<WeakReference<Category>> instances = new HashSet<WeakReference<Category>>();
		internal static event EventHandler<EventArgs<Category>> OnGetStatuses;
		/// <summary>
		/// 更新前。
		/// </summary>
		public static event EventHandler<EventArgs<Category>> Updating;
		/// <summary>
		/// 更新後。
		/// </summary>
		public static event EventHandler<EventArgs<Category>> Updated;
		bool notifyUpdates;

		/// <summary>
		/// 現在のページを取得または設定します。
		/// </summary>
		public int Page
		{
			get;
			set;
		}

		static void CleanInstances()
		{
			lock (instances)
				instances.Where(_ => !_.IsAlive)
						 .Freeze()
						 .RunWhile(instances.Remove);
		}

		internal void RemoveFromInstanceList()
		{
			instances.RemoveWhere(_ => _.Target == this);
		}

		/// <summary>
		/// すべての Category インスタンスを取得します。
		/// </summary>
		/// <returns>すべての Category インスタンス。</returns>
		public static IEnumerable<Category> GetInstances()
		{
			lock (instances)
				return instances.Where(_ => _.IsAlive)
								.Select(_ => _.Target)
								.Freeze();
		}

		internal IDisposable NotifyScrollUpdates()
		{
			return FinallyBlock.Create(notifyUpdates = true, _ => notifyUpdates = false);
		}

		/// <summary>
		/// 次のページを取得中かどうかを取得します。
		/// </summary>
		public bool IsRequestingNewPage
		{
			get;
			private set;
		}

		/// <summary>
		/// Category の新しいインスタンスを初期化します。
		/// </summary>
		public Category()
		{
			this.Filter = new Filter();
			this.Interval = TimeSpan.FromMinutes(3);
			this.Statuses = new NotifyCollection<IEntry>();

			CleanInstances();
			instances.Add(new WeakReference<Category>(this));

			this.Statuses.CollectionChanging += (sender, e) =>
			{
				if (notifyUpdates)
					Updating.RaiseEvent(this, new EventArgs<Category>(this));
			};
			this.Statuses.CollectionChanged += (sender, e) =>
			{
				if (notifyUpdates)
					Updated.RaiseEvent(this, new EventArgs<Category>(this));
			};
		}

		/// <summary>
		/// カテゴリの名前とフィルタ ソースを指定し Category の新しいインスタンスを初期化します。
		/// </summary>
		/// <param name="name">名前。</param>
		/// <param name="sources">フィルタ ソース。</param>
		public Category(string name, params FilterSource[] sources)
			: this()
		{
			this.Name = name;
			this.Filter.Sources.Clear();
			this.NotifyUpdates = true;
			this.CheckUnreads = true;

			foreach (var i in sources)
				this.Filter.Sources.Add(i);
		}

		/// <summary>
		/// カテゴリの名前を取得または設定します。
		/// </summary>
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		[DefaultValue(null)]
		public string Name
		{
			get;
			set;
		}

		/// <summary>
		/// フィルタを取得または設定します。
		/// </summary>
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		public Filter Filter
		{
			get;
			set;
		}

		/// <summary>
		/// 新着を通知するかを取得または設定します。
		/// </summary>
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		public bool NotifyUpdates
		{
			get;
			set;
		}

		/// <summary>
		/// 新着数を表示するかを取得または設定します。
		/// </summary>
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		public bool CheckUnreads
		{
			get;
			set;
		}

		/// <summary>
		/// 更新間隔を取得または設定します。
		/// </summary>
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		public TimeSpan Interval
		{
			get;
			set;
		}

		/// <summary>
		/// 取得済みのエントリを取得します。
		/// </summary>
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public NotifyCollection<IEntry> Statuses
		{
			get;
			private set;
		}

		/// <summary>
		/// 通知音を取得または設定します。
		/// </summary>
		public string NotifySound
		{
			get;
			set;
		}

		/// <summary>
		/// 新着数を取得または設定します。
		/// </summary>
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public int Unreads
		{
			get
			{
				return GetValue(() => this.Unreads);
			}
			set
			{
				SetValue(() => this.Unreads, value);
				this.HasUnreads = this.Unreads > 0;
			}
		}

		/// <summary>
		/// 新着数があるかどうかを取得します。
		/// </summary>
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public bool HasUnreads
		{
			get
			{
				return GetValue(() => this.HasUnreads);
			}
			private set
			{
				SetValue(() => this.HasUnreads, value);
			}
		}

		/// <summary>
		/// 最終更新を取得または設定します。
		/// </summary>
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public DateTime LastUpdate
		{
			get
			{
				return GetValue(() => this.LastUpdate);
			}
			set
			{
				SetValue(() => this.LastUpdate, value);
			}
		}

		/// <summary>
		/// クライアントを指定してこのカテゴリのフィルタからエントリを取得します。
		/// </summary>
		/// <param name="client">クライアント。</param>
		/// <returns>取得したエントリ。</returns>
		public IEnumerable<IEntry> GetStatuses(TwitterClient client)
		{
			return GetStatuses(client, null);
		}

		/// <summary>
		/// クライアントおよび取得範囲を指定してこのカテゴリのフィルタからエントリを取得します。
		/// </summary>
		/// <param name="client">クライアント。</param>
		/// <param name="range">取得範囲。</param>
		/// <returns>取得したエントリ。</returns>
		public IList<IEntry> GetStatuses(TwitterClient client, StatusRange range)
		{
			return GetStatuses(client, range, _ => true);
		}

		/// <summary>
		/// クライアントと取得範囲および使用するフィルタ ソースの条件を指定してこのカテゴリのフィルタからエントリを取得します。
		/// </summary>
		/// <param name="client">クライアント。</param>
		/// <param name="range">取得範囲。</param>
		/// <param name="useSource">使用するフィルタ ソースの条件。</param>
		/// <returns>取得したエントリ。</returns>
		public IList<IEntry> GetStatuses(TwitterClient client, StatusRange range, Func<FilterSource, bool> useSource)
		{
			try
			{
				if (this.Filter == null)
					return new List<IEntry>();
				else
					return this.Filter.GetStatuses(client, range, useSource)
									  .ToList();
			}
			finally
			{
				OnGetStatuses.RaiseEvent(this, new EventArgs<Category>(this));
			}
		}

		/// <summary>
		/// クライアントおよび取得範囲を指定してこのカテゴリのフィルタからエントリを取得しこのカテゴリを更新します。
		/// </summary>
		/// <param name="client">クライアント。</param>
		/// <param name="range">取得範囲。</param>
		/// <returns>取得したエントリ。</returns>
		public IList<IEntry> Update(TwitterClient client, StatusRange range)
		{
			return Update(client, range, _ => true);
		}

		/// <summary>
		/// クライアントと取得範囲および使用するフィルタ ソースを指定してこのカテゴリのフィルタからエントリを取得しこのカテゴリを更新します。
		/// </summary>
		/// <param name="client">クライアント。</param>
		/// <param name="range">取得範囲。</param>
		/// <param name="useSource">使用するフィルタ ソースの条件。</param>
		/// <returns>取得したエントリ。</returns>
		public IList<IEntry> Update(TwitterClient client, StatusRange range, Func<FilterSource, bool> useSource)
		{
			using (NotifyScrollUpdates())
			{
				this.Page = 1;

				var rt = GetStatuses(client, range, useSource);

				if (rt.All(_ => _.UserName == client.Account.Name) &&
					this.Statuses.Take(rt.Count).SequenceEqual(rt, new InstantEqualityComparer<IEntry>(entry => entry.TypeMatch
					(
						(Status _) => _.StatusID.ToString(),
						(List _) => _.User.Name + "/" + _.Name,
						_ => null
					))))
					rt.Clear();

				var rt2 = rt.Except(this.Statuses).Freeze();

				if (rt.Any())
					using (this.Statuses.SusupendNotification(true))
					{
						this.Statuses.RemoveWhere(rt.Contains);
						this.Statuses.InsertRange(0, rt.OrderByDescending(_ => _.CreatedAt));

						if (this.Statuses.Count > Settings.Default.Timeline.MaxStatuses)
							for (int i = this.Statuses.Count - 1; i >= Settings.Default.Timeline.MaxStatuses; i--)
								this.Statuses.RemoveAt(i);
					}

				return rt2;
			}
		}

		/// <summary>
		/// 指定されたエントリをこのカテゴリに追加します。
		/// </summary>
		/// <param name="entries">追加するエントリ一覧。</param>
		public void AppendStatuses(IList<IEntry> entries)
		{
			AppendStatuses(entries, null, null);
		}

		internal void AppendStatuses(IList<IEntry> entries, Action pre, Action post)
		{
			using (this.NotifyScrollUpdates())
				if (entries.Any())
				{
					if (this.CheckUnreads && Settings.Default.Timeline.AutoResetNewCount)
						this.ClearUnreads();

					if (pre != null)
						pre();

					try
					{
						this.Statuses.InsertRange(0, entries.Except(this.Statuses));

						if (this.Statuses.Count > Settings.Default.Timeline.MaxStatuses)
							for (int i = this.Statuses.Count - 1; i >= Settings.Default.Timeline.MaxStatuses; i--)
								this.Statuses.RemoveAt(i);
					}
					finally
					{
						if (post != null)
							post();
					}

					if (this.CheckUnreads)
						if (Settings.Default.Timeline.AutoResetNewCount)
							this.Unreads = entries.Count;
						else
							this.Unreads += entries.Count;
				}
		}

		/// <summary>
		/// 取得済みのエントリをクリアします。
		/// </summary>
		public void ClearStatuses()
		{
			this.Statuses.Clear();
			ClearUnreads();
		}

		/// <summary>
		/// 新着数をクリアします。
		/// </summary>
		public void ClearUnreads()
		{
			this.Unreads = 0;
		}

		/// <summary>
		/// 取得済みのエントリにフィルタを再度適用します。
		/// </summary>
		public void ApplyFilter()
		{
			var rt = this.Filter.Terms.Aggregate((IEnumerable<IEntry>)this.Statuses, (from, _) => _.FilterStatuses(from))
									  .Freeze();

			using (NotifyScrollUpdates())
			{
				this.Statuses.Clear();
				this.Statuses.AddRange(rt);
			}
		}

		/// <summary>
		/// クライアントを指定して次のページを取得します。
		/// </summary>
		/// <param name="client">クライアント。</param>
		public void RequestNewPage(TwitterClient client)
		{
			if (this.Statuses.Any() && this.Statuses.Count < Settings.Default.Timeline.MaxStatuses && !this.IsRequestingNewPage)
				using (FinallyBlock.Create(this.IsRequestingNewPage = true, _ => this.IsRequestingNewPage = false))
					try
					{
						var rt = GetStatuses(client, new StatusRange(maxID: this.Statuses.OfType<Status>().Last().StatusID, page: ++this.Page)).OrderByDescending(_ => _.CreatedAt);

						using (this.Statuses.SusupendNotification(true))
							this.Statuses.AddRange(rt.Except(this.Statuses));
					}
					catch (Exception ex)
					{
						this.Page--;

						throw new Exception(ex.Message, ex);
					}
		}

		/// <summary>
		/// 現在のインスタンスのコピーである新しいオブジェクトを作成します。
		/// </summary>
		/// <returns>このインスタンスのコピーである新しいオブジェクト。</returns>
		public Category Clone()
		{
			return new Category
			{
				Name = this.Name,
				Statuses = new NotifyCollection<IEntry>(this.Statuses),
				Unreads = this.Unreads,
				CheckUnreads = this.CheckUnreads,
				NotifyUpdates = this.NotifyUpdates,
				Interval = this.Interval,
				LastUpdate = this.LastUpdate,
				Filter = this.Filter.Clone(),
			};
		}

		object ICloneable.Clone()
		{
			return Clone();
		}
	}
}
