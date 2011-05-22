using System;
using System.ComponentModel;
using System.Diagnostics;

namespace Lunar
{
	/// <summary>
	/// つぶやき ID を表します。
	/// </summary>
	[DebuggerDisplay("{value}")]
	[TypeConverter(typeof(IDConverter<StatusID>))]
	public struct StatusID : IID, IComparable<StatusID>
	{
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		long value;

		StatusID(long value)
		{
			this.value = value;
		}

		/// <summary>
		/// 指定された StatusID を long に変換します。
		/// </summary>
		/// <param name="self">StatusID。</param>
		/// <returns>long。</returns>
		public static implicit operator long(StatusID self)
		{
			return self.value;
		}

		/// <summary>
		/// 指定された long を StatusID に変換します。
		/// </summary>
		/// <param name="self">long。</param>
		/// <returns>StatusID。</returns>
		public static implicit operator StatusID(long self)
		{
			return new StatusID(self);
		}

		/// <summary>
		/// 指定された double を StatusID に変換します。
		/// </summary>
		/// <param name="self">double。</param>
		/// <returns>StatusID。</returns>
		public static implicit operator StatusID(double self)
		{
			return new StatusID((long)self);
		}

		/// <summary>
		/// このインスタンスの数値を、それと等価な文字列形式に変換します。
		/// </summary>
		/// <returns>文字列形式。</returns>
		public override string ToString()
		{
			return value == 0 ? null : value.ToString();
		}

		/// <summary>
		/// このインスタンスのハッシュ コードを返します。
		/// </summary>
		/// <returns>このインスタンスのハッシュ コード。</returns>
		public override int GetHashCode()
		{
			return value.GetHashCode();
		}

		/// <summary>
		/// 対象のインスタンスが、指定したオブジェクトに等しいかどうかを示す値を返します。
		/// </summary>
		/// <param name="obj">このインスタンスと比較するオブジェクト。</param>
		/// <returns>obj が StatusID のインスタンスで、このインスタンスの値に等しい場合は true。それ以外の場合は false。</returns>
		public override bool Equals(object obj)
		{
			if (obj is StatusID)
				return value.Equals(((StatusID)obj).value);

			return value.Equals(obj);
		}

		long IID.Value
		{
			get
			{
				return this.value;
			}
			set
			{
				this.value = value;
			}
		}

		/// <summary>
		/// 指定した StatusID とこのインスタンスを比較し、これらの相対値を示す値を返します。
		/// </summary>
		/// <param name="other">比較対象の StatusID。</param>
		/// <returns>このインスタンスと value の相対値を示す符号付き数値。</returns>
		public int CompareTo(StatusID other)
		{
			return value.CompareTo(other.value);
		}
	}
}
