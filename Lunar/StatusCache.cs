using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Ignition;
using Ignition.Linq;

namespace Lunar
{
	/// <summary>
	/// キャッシュストレージを表します。
	/// </summary>
	public class StatusCache
	{
		/// <summary>
		/// つぶやきの一覧を取得するときに発生します。(返却するつぶやきの一覧を設定)
		/// </summary>
		public event EventHandler<EventArgs<IEnumerable<Status>>> ResolveStatuses;
		/// <summary>
		/// ユーザの一覧を取得するときに発生します。(返却するユーザの一覧を設定)
		/// </summary>
		public event EventHandler<EventArgs<IEnumerable<User>>> ResolveUsers;
		/// <summary>
		/// つぶやきを取得するときに発生します。(取得する ID, 返却するつぶやきを設定)
		/// </summary>
		public event EventHandler<EventArgs<StatusID, Status>> ResolveStatus;
		/// <summary>
		/// ユーザ名からユーザを取得するときに発生します。(取得するユーザ名, 返却するユーザを設定)
		/// </summary>
		public event EventHandler<EventArgs<string, User>> ResolveUserByName;
		/// <summary>
		/// ID からユーザを取得するときに発生します。(取得する ID, 返却するユーザを設定)
		/// </summary>
		public event EventHandler<EventArgs<UserID, User>> ResolveUserByID;
		/// <summary>
		/// つぶやきをキャッシュするときに発生します。(キャッシュするつぶやき)
		/// </summary>
		public event EventHandler<EventArgs<Status>> StoreStatus;
		/// <summary>
		/// ユーザをキャッシュするときに発生します。(キャッシュするユーザ)
		/// </summary>
		public event EventHandler<EventArgs<User>> StoreUser;
		/// <summary>
		/// 不要なつぶやきを判断するときに発生します。(不要になったつぶやきを設定)
		/// </summary>
		public event EventHandler<EventArgs<IEnumerable<Status>>> ReleaseStatuses;
		/// <summary>
		/// 不要なユーザを判断するときに発生します。(不要になったユーザを設定)
		/// </summary>
		public event EventHandler<EventArgs<IEnumerable<User>>> ReleaseUsers;
		/// <summary>
		/// キャッシュをクリーンするときに発生します。
		/// </summary>
		public event EventHandler CleanStorage;
		/// <summary>
		/// キャッシュをクリアするときに発生します。
		/// </summary>
		public event EventHandler ClearStorage;

		readonly SortedList<StatusID, Status> statuses = new SortedList<StatusID, Status>();
		readonly SortedList<UserID, User> usersByID = new SortedList<UserID, User>();
		readonly SortedList<string, User> usersByName = new SortedList<string, User>();
		readonly ReaderWriterLock rwl = new ReaderWriterLock();

		/// <summary>
		/// キャッシュされているつぶやき数を取得します。
		/// </summary>
		public int StatusCount
		{
			get
			{
				return statuses.Count;
			}
		}

		/// <summary>
		/// キャッシュされているユーザ数を取得します。
		/// </summary>
		public int UserCount
		{
			get
			{
				return usersByID.Count;
			}
		}

		/// <summary>
		/// キャッシュされているつぶやきを取得します。
		/// </summary>
		/// <returns>キャッシュされているつぶやき。</returns>
		public IEnumerable<Status> GetStatuses()
		{
			using (rwl.AcquireReaderLock())
			{
				var e = new EventArgs<IEnumerable<Status>>(null);

				ResolveStatuses.RaiseEvent(this, e);

				if (e.Value != null)
					foreach (var i in e.Value)
						yield return i;
				else
					lock (statuses)
						foreach (var i in statuses.Values)
							yield return i;
			}
		}

		/// <summary>
		/// キャッシュされているユーザを取得します。
		/// </summary>
		/// <returns>キャッシュされているユーザ。</returns>
		public IEnumerable<User> GetUsers()
		{
			using (rwl.AcquireReaderLock())
			{
				var e = new EventArgs<IEnumerable<User>>(null);

				ResolveUsers.RaiseEvent(this, e);

				if (e.Value != null)
					foreach (var i in e.Value)
						yield return i;
				else
					foreach (var i in usersByID.Values)
						yield return i;
			}
		}

		/// <summary>
		/// 指定した ID に割り当てられたキャッシュを取得するか、指定された外部取得処理を実行しキャッシュします。
		/// </summary>
		/// <param name="id">探索または割り当てるキャッシュの ID。</param>
		/// <param name="body">指定した ID がキャッシュされていなかった場合実行される取得処理。</param>
		/// <returns>指定した ID に割り当てられたキャッシュが存在する場合はそのキャッシュ。無い場合は指定された外部取得処理の取得結果。</returns>
		public Status RetrieveStatus(StatusID id, Func<StatusID, Status> body)
		{
			Clean();

			using (rwl.AcquireReaderLock())
			{
				var e = new EventArgs<StatusID, Status>(id, null);

				ResolveStatus.RaiseEvent(this, e);

				if (e.Value2 != null)
					return e.Value2;

				lock (statuses)
					if (statuses.ContainsKey(id))
						return statuses[id];
			}

			return SetStatus(body(id));
		}

		/// <summary>
		/// 指定したつぶやきをキャッシュします。
		/// すでに同じ ID のものが存在する場合、上書きされます。
		/// </summary>
		/// <param name="status">キャッシュするつぶやき。</param>
		/// <returns>キャッシュされたつぶやき。</returns>
		public Status SetStatus(Status status)
		{
			if (status == null)
				return null;

			if (status.User != null)
				SetUser(status.User);

			using (rwl.AcquireWriterLock())
			{
				StoreStatus.RaiseEvent(this, new EventArgs<Status>(status));

				lock (statuses)
					return statuses[status.StatusID] = status;
			}
		}

		/// <summary>
		/// 指定したユーザ名に割り当てられたキャッシュを取得するか、指定された外部取得処理を実行しキャッシュします。
		/// </summary>
		/// <param name="id">探索または割り当てるキャッシュのユーザ名。</param>
		/// <param name="body">指定したユーザ名がキャッシュされていなかった場合実行される取得処理。</param>
		/// <returns>指定したユーザ名に割り当てられたキャッシュが存在する場合はそのキャッシュ。無い場合は指定された外部取得処理の取得結果。</returns>
		public User RetrieveUser(string id, Func<string, User> body)
		{
			Clean();

			using (rwl.AcquireReaderLock())
			{
				var e = new EventArgs<string, User>(id, null);

				ResolveUserByName.RaiseEvent(this, e);

				if (e.Value2 != null)
					return e.Value2;

				if (usersByName.ContainsKey(id))
					return usersByName[id];
			}

			return SetUser(body(id));
		}

		/// <summary>
		/// 指定したユーザ ID に割り当てられたキャッシュを取得するか、指定された外部取得処理を実行しキャッシュします。
		/// </summary>
		/// <param name="id">探索または割り当てるキャッシュのユーザ ID。</param>
		/// <param name="body">指定したユーザ ID がキャッシュされていなかった場合実行される取得処理。</param>
		/// <returns>指定したユーザ ID に割り当てられたキャッシュが存在する場合はそのキャッシュ。無い場合は指定された外部取得処理の取得結果。</returns>
		public User RetrieveUser(UserID id, Func<UserID, User> body)
		{
			Clean();

			using (rwl.AcquireReaderLock())
			{
				var e = new EventArgs<UserID, User>(id, null);

				ResolveUserByID.RaiseEvent(this, e);

				if (e.Value2 != null)
					return e.Value2;

				if (usersByID.ContainsKey(id))
					return usersByID[id];
			}

			return SetUser(body(id));
		}

		/// <summary>
		/// 指定したユーザをキャッシュします。
		/// すでに同じ ID のものが存在する場合、上書きされます。
		/// </summary>
		/// <param name="user">キャッシュするユーザ。</param>
		/// <returns>キャッシュされたユーザ。</returns>
		public User SetUser(User user)
		{
			if (user == null)
				return null;

			using (rwl.AcquireWriterLock())
			{
				StoreUser.RaiseEvent(this, new EventArgs<User>(user));

				return usersByID[user.UserID] = usersByName[user.Name] = user;
			}
		}

		/// <summary>
		/// すでに存在しないキャッシュ要素へのリンクを削除します。
		/// </summary>
		public void Clean()
		{
			var es = new EventArgs<IEnumerable<Status>>(null);
			var eu = new EventArgs<IEnumerable<User>>(null);

			ReleaseStatuses.RaiseEvent(this, es);
			ReleaseUsers.RaiseEvent(this, eu);

			using (rwl.AcquireWriterLock())
			{
				CleanStorage.RaiseEvent(this, EventArgs.Empty);

				lock (statuses)
					if (es.Value != null)
						es.Value.Select(_ => _.StatusID)
								.RunWhile(statuses.Remove);

				if (eu.Value != null)
				{
					var ul = eu.Value.Freeze();

					ul.Select(_ => _.UserID)
					  .RunWhile(usersByID.Remove);
					ul.Select(_ => _.Name)
					  .RunWhile(usersByName.Remove);
				}
			}
		}

		/// <summary>
		/// キャッシュを空にします。
		/// </summary>
		public void Clear()
		{
			using (rwl.AcquireWriterLock())
			{
				ClearStorage.RaiseEvent(this, EventArgs.Empty);
				statuses.Clear();
				usersByID.Clear();
				usersByName.Clear();
			}
		}

		/// <summary>
		/// 指定した ID のつぶやきをキャッシュから削除します。
		/// </summary>
		/// <param name="id">つぶやき ID。</param>
		public void RemoveStatus(StatusID id)
		{
			statuses.Remove(id);
		}
	}
}
