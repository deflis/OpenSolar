using System;
using System.Collections.Generic;
using System.Linq;
using Ignition;

namespace Solar
{
	/// <summary>
	/// 処理のブロックを表します。
	/// </summary>
	public class ProgressBlock : NotifyObject, IDisposable
	{
		static LinkedList<ProgressBlock> progresses = new LinkedList<ProgressBlock>();

		internal static IProgressHost ProgressHost
		{
			get;
			set;
		}

		/// <summary>
		/// 進行状況を表示するかどうかを取得または設定します。
		/// </summary>
		public bool UseProgress
		{
			get
			{
				return GetValue(() => this.UseProgress);
			}
			set
			{
				SetValue(() => this.UseProgress, value);
			}
		}

		/// <summary>
		/// 進行状況の最大値を取得または設定します。
		/// </summary>
		public int Maximum
		{
			get
			{
				return GetValue(() => this.Maximum);
			}
			set
			{
				SetValue(() => this.Maximum, value);
			}
		}

		/// <summary>
		/// 進行状況の最小値を取得または設定します。
		/// </summary>
		public int Minimum
		{
			get
			{
				return GetValue(() => this.Minimum);
			}
			set
			{
				SetValue(() => this.Minimum, value);
			}
		}

		/// <summary>
		/// 進行状況の数値を取得または設定します。
		/// </summary>
		public int Value
		{
			get
			{
				return GetValue(() => this.Value);
			}
			set
			{
				SetValue(() => this.Value, value);
			}
		}

		/// <summary>
		/// 説明テキストを取得または設定します。
		/// </summary>
		public string Text
		{
			get
			{
				return GetValue(() => this.Text);
			}
			set
			{
				SetValue(() => this.Text, value);
			}
		}

		/// <summary>
		/// 進行状況ではなく報告であるかどうかを取得または設定します。
		/// </summary>
		public bool IsReport
		{
			get
			{
				return GetValue(() => this.IsReport);
			}
			set
			{
				SetValue(() => this.IsReport, value);
			}
		}

		/// <summary>
		/// テキストを指定し ProgressBlock の新しいインスタンスを初期化します。
		/// </summary>
		/// <param name="text">説明テキスト。</param>
		public ProgressBlock(string text)
		{
			this.Text = text;

			lock (progresses)
			{
				if (progresses.Any() &&
					progresses.First.Value.IsReport)
					progresses.RemoveFirst();

				progresses.AddFirst(this);
			}

			if (ProgressHost != null)
				ProgressHost.CurrentProgress = this;
		}

		/// <summary>
		/// テキストおよび報告かどうかを指定し ProgressBlock の新しいインスタンスを初期化します。
		/// </summary>
		/// <param name="text">説明テキスト。</param>
		/// <param name="isReport">進行状況ではなく報告であるかどうか。</param>
		public ProgressBlock(string text, bool isReport)
			: this(text)
		{
			this.IsReport = true;
		}

		/// <summary>
		/// テキストおよび進行状況の最大値を指定し PrgoressBlock の新しいインスタンスを初期化します。
		/// </summary>
		/// <param name="text">説明テキスト。</param>
		/// <param name="maximum">進行状況の最大値。</param>
		public ProgressBlock(string text, int maximum)
			: this(text)
		{
			this.UseProgress = true;
			this.Maximum = maximum;
		}

		/// <summary>
		/// 処理が終了したことを通知します。
		/// </summary>
		public void Dispose()
		{
			lock (progresses)
			{
				progresses.Remove(this);

				if (ProgressHost != null)
					ProgressHost.CurrentProgress = progresses.Any() ? progresses.First.Value : null;
			}
		}

		/// <summary>
		/// 処理を抜けたことを無条件で通知します。
		/// </summary>
		public static void Pop()
		{
			lock (progresses)
			{
				if (progresses.Any())
					progresses.RemoveFirst();

				if (ProgressHost != null)
					ProgressHost.CurrentProgress = progresses.Any() ? progresses.First.Value : null;
			}
		}

		/// <summary>
		/// すべての進行状況をクリアします。
		/// </summary>
		public static void Clear()
		{
			lock (progresses)
			{
				progresses.Clear();

				if (ProgressHost != null)
					ProgressHost.CurrentProgress = null;
			}
		}
	}
}
