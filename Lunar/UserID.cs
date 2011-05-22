using System;
using System.ComponentModel;
using System.Diagnostics;

namespace Lunar
{
	/// <summary>
	/// ユーザ ID を表します。
	/// </summary>
	[DebuggerDisplay("{value}")]
	[TypeConverter(typeof(IDConverter<UserID>))]
	public struct UserID : IID, IComparable<UserID>
	{
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		long value;

		UserID(long value)
		{
			this.value = value;
		}

		/// <summary>
		/// 指定された UserID を long に変換します。
		/// </summary>
		/// <param name="self">UserID。</param>
		/// <returns>long。</returns>
		public static implicit operator long(UserID self)
		{
			return self.value;
		}

		/// <summary>
		/// 指定された long を UserID に変換します。
		/// </summary>
		/// <param name="self">long。</param>
		/// <returns>UserID。</returns>
		public static implicit operator UserID(long self)
		{
			return new UserID(self);
		}

		/// <summary>
		/// 指定された double を UserID に変換します。
		/// </summary>
		/// <param name="self">double。</param>
		/// <returns>UserID。</returns>
		public static implicit operator UserID(double self)
		{
			return new UserID((long)self);
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
		/// <returns>obj が UserID のインスタンスで、このインスタンスの値に等しい場合は true。それ以外の場合は false。</returns>
		public override bool Equals(object obj)
		{
			if (obj is UserID)
				return value.Equals(((UserID)obj).value);

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
		/// 指定した UserID とこのインスタンスを比較し、これらの相対値を示す値を返します。
		/// </summary>
		/// <param name="other">比較対象の UserID。</param>
		/// <returns>このインスタンスと value の相対値を示す符号付き数値。</returns>
		public int CompareTo(UserID other)
		{
			return value.CompareTo(other.value);
		}
	}
}
