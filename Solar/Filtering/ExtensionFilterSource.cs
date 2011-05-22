using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Dynamic;
using Ignition;
using Lunar;
using Microsoft.Scripting.Hosting;
using Solar.Models;
using System.Collections;
using System.Linq;

namespace Solar.Filtering
{
	/// <summary>
	/// 拡張 フィルタソースを表します。
	/// </summary>
	public class ExtensionFilterSource : FilterSource
	{
		/// <summary>
		/// ExtensionFilterSource の新しいインスタンスを初期化します。
		/// </summary>
		public ExtensionFilterSource()
		{
			this.LocalData = new ExpandoObject();
		}

		/// <summary>
		/// スクリプト名を取得または設定します。
		/// </summary>
		public string Name
		{
			get;
			set;
		}

		/// <summary>
		/// 設定を取得します。
		/// </summary>
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		public ExpandoObject LocalData
		{
			get;
			private set;
		}

		/// <summary>
		/// ページに分けて取得することが可能かどうかを取得します。
		/// </summary>
		public override bool Pagable
		{
			get
			{
				try
				{
					var s = GetScope();

					if (s != null &&
						s.ContainsVariable("Pagable"))
						return s.GetVariable("Pagable");
					else
						return base.Pagable;
				}
				catch (Exception ex)
				{
					Client.Instance.OnThrowScriptError(new EventArgs<string, Exception>(this.Name, ex));
					Client.Instance.FilterSourceScriptWatcher.SuspendScript(this.Name);

					return base.Pagable;
				}
			}
		}

		/// <summary>
		/// 保存可能かどうかを取得します。
		/// </summary>
		public override bool Serializable
		{
			get
			{
				try
				{
					var s = GetScope();

					if (s != null &&
						s.ContainsVariable("Serializable"))
						return s.GetVariable("Serializable");
					else
						return base.Serializable;
				}
				catch (Exception ex)
				{
					Client.Instance.OnThrowScriptError(new EventArgs<string, Exception>(this.Name, ex));
					Client.Instance.FilterSourceScriptWatcher.SuspendScript(this.Name);

					return base.Serializable;
				}
			}
		}

		/// <summary>
		/// クライアントと取得範囲を指定しソースからエントリを取得します。
		/// </summary>
		/// <param name="client">クライアント。</param>
		/// <param name="range">取得範囲。</param>
		/// <returns>取得したエントリ。</returns>
		protected override IEnumerable<IEntry> GetStatuses(TwitterClient client, StatusRange range)
		{
			try
			{
				var s = GetScope();

				if (s != null)
				{
					var rt = s.GetVariable("GetStatuses")(client, range);

					if (rt is IEnumerable<IEntry>)
						return rt;
					else if (rt is IEnumerable)
						return ((IEnumerable)rt).Cast<IEntry>();
				}
			}
			catch (Exception ex)
			{
				Client.Instance.OnThrowScriptError(new EventArgs<string, Exception>(this.Name, ex));
				Client.Instance.FilterSourceScriptWatcher.SuspendScript(this.Name);
			}

			return null;
		}

		/// <summary>
		/// 指定したエントリがこのソースから取得できるエントリとして扱うかどうかを取得します。
		/// </summary>
		/// <param name="entry">判定するエントリ。</param>
		/// <returns>このソースから取得できるエントリとして扱うかどうか。</returns>
		protected override bool StreamEntryMatches(IEntry entry)
		{
			try
			{
				var s = GetScope();

				if (s != null &&
					s.ContainsVariable("StreamEntryMatches"))
					return s.GetVariable("StreamEntryMatches")(entry);
				else
					return false;
			}
			catch (Exception ex)
			{
				Client.Instance.OnThrowScriptError(new EventArgs<string, Exception>(this.Name, ex));
				Client.Instance.FilterSourceScriptWatcher.SuspendScript(this.Name);
			}

			return false;
		}

		ScriptScope GetScope()
		{
			return this.Name != null && Client.Instance.FilterSourceScriptWatcher.Scripts.ContainsKey(this.Name)
				? Client.Instance.FilterSourceScriptWatcher.Scripts[this.Name].Apply(_ => _.SetVariable("LocalData", this.LocalData))
				: null;
		}
	}
}
