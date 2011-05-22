using System;
using System.Windows.Input;

namespace Solar
{
	/// <summary>
	/// デリゲートによるコマンドを表します。
	/// </summary>
	public class RelayCommand : RelayCommand<object>
	{
		/// <summary>
		/// 実行内容を指定し RelayCommand の新しいインスタンスを初期化します。
		/// </summary>
		/// <param name="execute">実行内容。</param>
		public RelayCommand(Action<object> execute)
			: this(null, execute)
		{
		}

		/// <summary>
		/// 必要条件および実行内容を指定し RelayCommand の新しいインスタンスを初期化します。
		/// </summary>
		/// <param name="canExecute">必要条件。</param>
		/// <param name="execute">実行内容。</param>
		public RelayCommand(Func<object, bool> canExecute, Action<object> execute)
			: base(canExecute, execute)
		{
		}

		/// <summary>
		/// 実行内容を指定し RelayCommand の新しいインスタンスを初期化します。
		/// </summary>
		/// <param name="execute">実行内容。</param>
		public RelayCommand(Func<object, object> execute)
			: this((Action<object>)(_ => execute(_)))
		{
		}

		/// <summary>
		/// 必要条件および実行内容を指定し RelayCommand の新しいインスタンスを初期化します。
		/// </summary>
		/// <param name="canExecute">必要条件。</param>
		/// <param name="execute">実行内容。</param>
		public RelayCommand(Func<object, bool> canExecute, Func<object, object> execute)
			: this(_ => canExecute(_), (Action<object>)(_ => execute(_)))
		{
		}
	}

	/// <summary>
	/// 引数の型を指定したデリゲートによるコマンドを表します。
	/// </summary>
	/// <typeparam name="T">引数の型。</typeparam>
	public class RelayCommand<T> : ICommand
	{
		readonly Action<T> execute;
		readonly Func<T, bool> canExecute;

		/// <summary>
		/// 実行内容を指定し RelayCommand の新しいインスタンスを初期化します。
		/// </summary>
		/// <param name="execute">実行内容。</param>
		public RelayCommand(Action<T> execute)
			: this(null, execute)
		{
		}

		/// <summary>
		/// 必要条件および実行内容を指定し RelayCommand の新しいインスタンスを初期化します。
		/// </summary>
		/// <param name="canExecute">必要条件。</param>
		/// <param name="execute">実行内容。</param>
		public RelayCommand(Func<T, bool> canExecute, Action<T> execute)
		{
			if (execute == null)
				throw new ArgumentNullException("execute");

			this.execute = execute;
			this.canExecute = canExecute;
		}

		/// <summary>
		/// 実行内容を指定し RelayCommand の新しいインスタンスを初期化します。
		/// </summary>
		/// <param name="execute">実行内容。</param>
		public RelayCommand(Func<T, object> execute)
			: this((Action<T>)(_ => execute(_)))
		{
		}

		/// <summary>
		/// 必要条件および実行内容を指定し RelayCommand の新しいインスタンスを初期化します。
		/// </summary>
		/// <param name="canExecute">必要条件。</param>
		/// <param name="execute">実行内容。</param>
		public RelayCommand(Func<T, bool> canExecute, Func<T, object> execute)
			: this(_ => canExecute(_), (Action<T>)(_ => execute(_)))
		{
		}

		/// <summary>
		/// 指定した引数でこのコマンドを実行できるかどうかを判断します。
		/// </summary>
		/// <param name="parameter">コマンドの引数。引数を使用しない場合、null を指定することも可能です。</param>
		/// <returns>実行できるかどうか。</returns>
		public bool CanExecute(object parameter)
		{
			return canExecute == null ? true : canExecute((T)parameter);
		}

		/// <summary>
		/// コマンドを実行するかどうかに影響するような変更があった場合に発生します。
		/// </summary>
		public event EventHandler CanExecuteChanged
		{
			add
			{
				CommandManager.RequerySuggested += value;
			}
			remove
			{
				CommandManager.RequerySuggested -= value;
			}
		}

		/// <summary>
		/// 引数を指定しこのコマンドを実行します。
		/// </summary>
		/// <param name="parameter">コマンドの引数。引数を使用しない場合、null を指定することも可能です。</param>
		public void Execute(object parameter)
		{
			execute((T)parameter);
		}
	}
}
