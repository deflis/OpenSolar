using System;
using System.ComponentModel;
using System.Globalization;

namespace Lunar
{
	/// <summary>
	/// ID 型と string, int, long 型の間の変換を提供します。
	/// </summary>
	/// <typeparam name="T">ID 型。</typeparam>
	public class IDConverter<T> : TypeConverter
		where T : struct, IID
	{
		/// <summary>
		/// 指定したコンテキストを使用して、コンバーターが特定の型のオブジェクトをコンバーターの型に変換できるかどうかを示す値を返します。
		/// </summary>
		/// <param name="context">書式指定コンテキストを提供する System.ComponentModel.ITypeDescriptorContext。</param>
		/// <param name="sourceType">変換前の型を表す System.Type。</param>
		/// <returns>コンバーターが変換を実行できる場合は true。それ以外の場合は false。</returns>
		public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
		{
			return sourceType == typeof(int)
				|| sourceType == typeof(long)
				|| sourceType == typeof(string)
				|| base.CanConvertFrom(context, sourceType);
		}

		/// <summary>
		/// コンバーターが、指定したコンテキストを使用して、指定した型にオブジェクトを変換できるかどうかを示す値を返します。
		/// </summary>
		/// <param name="context">書式指定コンテキストを提供する System.ComponentModel.ITypeDescriptorContext。</param>
		/// <param name="destinationType">変換後の型を表す System.Type。</param>
		/// <returns>コンバーターが変換を実行できる場合は true。それ以外の場合は false</returns>
		public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
		{
			return destinationType == typeof(int)
				|| destinationType == typeof(long)
				|| destinationType == typeof(string)
				|| base.CanConvertTo(context, destinationType);
		}

		/// <summary>
		/// 指定したコンテキストとカルチャ情報を使用して、指定したオブジェクトをコンバーターの型に変換します。
		/// </summary>
		/// <param name="context">書式指定コンテキストを提供する System.ComponentModel.ITypeDescriptorContext。</param>
		/// <param name="culture">現在のカルチャとして使用する System.Globalization.CultureInfo。</param>
		/// <param name="value">変換対象の System.Object。</param>
		/// <returns>変換後の値を表す System.Object。</returns>
		public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
		{
			if (value is int)
				return new T
				{
					Value = (int)value,
				};
			else if (value is long)
				return new T
				{
					Value = (long)value,
				};
			else if (value is string)
				return new T
				{
					Value = long.Parse(value.ToString()),
				};

			return base.ConvertFrom(context, culture, value);
		}

		/// <summary>
		/// 指定したコンテキストとカルチャ情報を使用して、指定した値オブジェクトを、指定した型に変換します。
		/// </summary>
		/// <param name="context">書式指定コンテキストを提供する System.ComponentModel.ITypeDescriptorContext。</param>
		/// <param name="culture">System.Globalization.CultureInfo。null が渡された場合は、現在のカルチャが使用されます。</param>
		/// <param name="value">変換対象の System.Object。</param>
		/// <param name="destinationType">value パラメーターの変換後の System.Type。</param>
		/// <returns>変換後の値を表す System.Object。</returns>
		public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
		{
			if (destinationType == typeof(int))
				return (int)((IID)value).Value;
			else if (destinationType == typeof(long))
				return ((IID)value).Value;
			else if (destinationType == typeof(string))
				return ((IID)value).Value.ToString();

			return base.ConvertTo(context, culture, value, destinationType);
		}
	}
}
