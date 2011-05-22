using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Dynamic;
using Ignition;
using Lunar;
using Microsoft.Scripting.Hosting;
using Solar.Models;

namespace Solar.Filtering
{
	/// <summary>
	/// 拡張 フィルタ項目を表します。
	/// </summary>
	public class ExtensionFilterTerms : FilterTerms
	{
		/// <summary>
		/// ExtensionFilterTerms の新しいインスタンスを初期化します。
		/// </summary>
		public ExtensionFilterTerms()
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
		/// 条件に項目が一致するかどうかを判断します。
		/// </summary>
		/// <param name="entries">エントリ。</param>
		/// <returns>条件に一致するかどうか。</returns>
		public override IEnumerable<IEntry> FilterStatuses(IEnumerable<IEntry> entries)
		{
			try
			{
				var s = GetScope();

				if (s != null &&
					s.ContainsVariable("FilterStatuses"))
					return s.GetVariable("FilterStatuses")(entries);
			}
			catch (Exception ex)
			{
				Client.Instance.OnThrowScriptError(new EventArgs<string, Exception>(this.Name, ex));
				Client.Instance.FilterTermsScriptWatcher.SuspendScript(this.Name);
			}

			return base.FilterStatuses(entries);
		}

		/// <summary>
		/// フィルタ処理を実行します。
		/// </summary>
		/// <param name="entry">フィルタ対象。</param>
		/// <returns>フィルタ結果。</returns>
		public override bool FilterStatus(IEntry entry)
		{
			try
			{
				var s = GetScope();

				if (s != null &&
					s.ContainsVariable("FilterStatus"))
					return s.GetVariable("FilterStatus")(entry);
			}
			catch (Exception ex)
			{
				Client.Instance.OnThrowScriptError(new EventArgs<string, Exception>(this.Name, ex));
				Client.Instance.FilterTermsScriptWatcher.SuspendScript(this.Name);
			}

			return true;
		}

		ScriptScope GetScope()
		{
			return this.Name != null && Client.Instance.FilterTermsScriptWatcher.Scripts.ContainsKey(this.Name)
				? Client.Instance.FilterTermsScriptWatcher.Scripts[this.Name].Apply(_ => _.SetVariable("LocalData", this.LocalData))
				: null;
		}
	}
}
