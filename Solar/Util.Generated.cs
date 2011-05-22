using System;

namespace Solar
{
	partial class Util
	{
		public static void TypeMatch<T1>(this object value, Action<T1> d1)
		{
			if (value == null)
			{
			}
			else if (value is T1)
				d1((T1)value);
		}

		public static void TypeMatch<T1>(this object value, Action<T1> d1, Action<object> unmatch)
		{
			if (value == null)
				unmatch(value);
			else if (value is T1)
				d1((T1)value);
			else if (unmatch != null)
				unmatch(value);
		}

		public static TReturn TypeMatch<T1, TReturn>(this object value, Func<T1, TReturn> d1)
		{
			if (value == null)
				return default(TReturn);
			else if (value is T1)
				return d1((T1)value);
			else
				return default(TReturn);
		}

		public static TReturn TypeMatch<T1, TReturn>(this object value, Func<T1, TReturn> d1, Func<object, TReturn> unmatch)
		{
			if (value == null)
				return unmatch(value);
			else if (value is T1)
				return d1((T1)value);
			else if (unmatch != null)
				return unmatch(value);
			else
				return default(TReturn);
		}

		public static void TypeMatch<T1, T2>(this object value, Action<T1> d1, Action<T2> d2)
		{
			if (value == null)
			{
			}
			else if (value is T1)
				d1((T1)value);
			else if (value is T2)
				d2((T2)value);
		}

		public static void TypeMatch<T1, T2>(this object value, Action<T1> d1, Action<T2> d2, Action<object> unmatch)
		{
			if (value == null)
				unmatch(value);
			else if (value is T1)
				d1((T1)value);
			else if (value is T2)
				d2((T2)value);
			else if (unmatch != null)
				unmatch(value);
		}

		public static TReturn TypeMatch<T1, T2, TReturn>(this object value, Func<T1, TReturn> d1, Func<T2, TReturn> d2)
		{
			if (value == null)
				return default(TReturn);
			else if (value is T1)
				return d1((T1)value);
			else if (value is T2)
				return d2((T2)value);
			else
				return default(TReturn);
		}

		public static TReturn TypeMatch<T1, T2, TReturn>(this object value, Func<T1, TReturn> d1, Func<T2, TReturn> d2, Func<object, TReturn> unmatch)
		{
			if (value == null)
				return unmatch(value);
			else if (value is T1)
				return d1((T1)value);
			else if (value is T2)
				return d2((T2)value);
			else if (unmatch != null)
				return unmatch(value);
			else
				return default(TReturn);
		}

		public static void TypeMatch<T1, T2, T3>(this object value, Action<T1> d1, Action<T2> d2, Action<T3> d3)
		{
			if (value == null)
			{
			}
			else if (value is T1)
				d1((T1)value);
			else if (value is T2)
				d2((T2)value);
			else if (value is T3)
				d3((T3)value);
		}

		public static void TypeMatch<T1, T2, T3>(this object value, Action<T1> d1, Action<T2> d2, Action<T3> d3, Action<object> unmatch)
		{
			if (value == null)
				unmatch(value);
			else if (value is T1)
				d1((T1)value);
			else if (value is T2)
				d2((T2)value);
			else if (value is T3)
				d3((T3)value);
			else if (unmatch != null)
				unmatch(value);
		}

		public static TReturn TypeMatch<T1, T2, T3, TReturn>(this object value, Func<T1, TReturn> d1, Func<T2, TReturn> d2, Func<T3, TReturn> d3)
		{
			if (value == null)
				return default(TReturn);
			else if (value is T1)
				return d1((T1)value);
			else if (value is T2)
				return d2((T2)value);
			else if (value is T3)
				return d3((T3)value);
			else
				return default(TReturn);
		}

		public static TReturn TypeMatch<T1, T2, T3, TReturn>(this object value, Func<T1, TReturn> d1, Func<T2, TReturn> d2, Func<T3, TReturn> d3, Func<object, TReturn> unmatch)
		{
			if (value == null)
				return unmatch(value);
			else if (value is T1)
				return d1((T1)value);
			else if (value is T2)
				return d2((T2)value);
			else if (value is T3)
				return d3((T3)value);
			else if (unmatch != null)
				return unmatch(value);
			else
				return default(TReturn);
		}

		public static void TypeMatch<T1, T2, T3, T4>(this object value, Action<T1> d1, Action<T2> d2, Action<T3> d3, Action<T4> d4)
		{
			if (value == null)
			{
			}
			else if (value is T1)
				d1((T1)value);
			else if (value is T2)
				d2((T2)value);
			else if (value is T3)
				d3((T3)value);
			else if (value is T4)
				d4((T4)value);
		}

		public static void TypeMatch<T1, T2, T3, T4>(this object value, Action<T1> d1, Action<T2> d2, Action<T3> d3, Action<T4> d4, Action<object> unmatch)
		{
			if (value == null)
				unmatch(value);
			else if (value is T1)
				d1((T1)value);
			else if (value is T2)
				d2((T2)value);
			else if (value is T3)
				d3((T3)value);
			else if (value is T4)
				d4((T4)value);
			else if (unmatch != null)
				unmatch(value);
		}

		public static TReturn TypeMatch<T1, T2, T3, T4, TReturn>(this object value, Func<T1, TReturn> d1, Func<T2, TReturn> d2, Func<T3, TReturn> d3, Func<T4, TReturn> d4)
		{
			if (value == null)
				return default(TReturn);
			else if (value is T1)
				return d1((T1)value);
			else if (value is T2)
				return d2((T2)value);
			else if (value is T3)
				return d3((T3)value);
			else if (value is T4)
				return d4((T4)value);
			else
				return default(TReturn);
		}

		public static TReturn TypeMatch<T1, T2, T3, T4, TReturn>(this object value, Func<T1, TReturn> d1, Func<T2, TReturn> d2, Func<T3, TReturn> d3, Func<T4, TReturn> d4, Func<object, TReturn> unmatch)
		{
			if (value == null)
				return unmatch(value);
			else if (value is T1)
				return d1((T1)value);
			else if (value is T2)
				return d2((T2)value);
			else if (value is T3)
				return d3((T3)value);
			else if (value is T4)
				return d4((T4)value);
			else if (unmatch != null)
				return unmatch(value);
			else
				return default(TReturn);
		}

		public static void TypeMatch<T1, T2, T3, T4, T5>(this object value, Action<T1> d1, Action<T2> d2, Action<T3> d3, Action<T4> d4, Action<T5> d5)
		{
			if (value == null)
			{
			}
			else if (value is T1)
				d1((T1)value);
			else if (value is T2)
				d2((T2)value);
			else if (value is T3)
				d3((T3)value);
			else if (value is T4)
				d4((T4)value);
			else if (value is T5)
				d5((T5)value);
		}

		public static void TypeMatch<T1, T2, T3, T4, T5>(this object value, Action<T1> d1, Action<T2> d2, Action<T3> d3, Action<T4> d4, Action<T5> d5, Action<object> unmatch)
		{
			if (value == null)
				unmatch(value);
			else if (value is T1)
				d1((T1)value);
			else if (value is T2)
				d2((T2)value);
			else if (value is T3)
				d3((T3)value);
			else if (value is T4)
				d4((T4)value);
			else if (value is T5)
				d5((T5)value);
			else if (unmatch != null)
				unmatch(value);
		}

		public static TReturn TypeMatch<T1, T2, T3, T4, T5, TReturn>(this object value, Func<T1, TReturn> d1, Func<T2, TReturn> d2, Func<T3, TReturn> d3, Func<T4, TReturn> d4, Func<T5, TReturn> d5)
		{
			if (value == null)
				return default(TReturn);
			else if (value is T1)
				return d1((T1)value);
			else if (value is T2)
				return d2((T2)value);
			else if (value is T3)
				return d3((T3)value);
			else if (value is T4)
				return d4((T4)value);
			else if (value is T5)
				return d5((T5)value);
			else
				return default(TReturn);
		}

		public static TReturn TypeMatch<T1, T2, T3, T4, T5, TReturn>(this object value, Func<T1, TReturn> d1, Func<T2, TReturn> d2, Func<T3, TReturn> d3, Func<T4, TReturn> d4, Func<T5, TReturn> d5, Func<object, TReturn> unmatch)
		{
			if (value == null)
				return unmatch(value);
			else if (value is T1)
				return d1((T1)value);
			else if (value is T2)
				return d2((T2)value);
			else if (value is T3)
				return d3((T3)value);
			else if (value is T4)
				return d4((T4)value);
			else if (value is T5)
				return d5((T5)value);
			else if (unmatch != null)
				return unmatch(value);
			else
				return default(TReturn);
		}

		public static void TypeMatch<T1, T2, T3, T4, T5, T6>(this object value, Action<T1> d1, Action<T2> d2, Action<T3> d3, Action<T4> d4, Action<T5> d5, Action<T6> d6)
		{
			if (value == null)
			{
			}
			else if (value is T1)
				d1((T1)value);
			else if (value is T2)
				d2((T2)value);
			else if (value is T3)
				d3((T3)value);
			else if (value is T4)
				d4((T4)value);
			else if (value is T5)
				d5((T5)value);
			else if (value is T6)
				d6((T6)value);
		}

		public static void TypeMatch<T1, T2, T3, T4, T5, T6>(this object value, Action<T1> d1, Action<T2> d2, Action<T3> d3, Action<T4> d4, Action<T5> d5, Action<T6> d6, Action<object> unmatch)
		{
			if (value == null)
				unmatch(value);
			else if (value is T1)
				d1((T1)value);
			else if (value is T2)
				d2((T2)value);
			else if (value is T3)
				d3((T3)value);
			else if (value is T4)
				d4((T4)value);
			else if (value is T5)
				d5((T5)value);
			else if (value is T6)
				d6((T6)value);
			else if (unmatch != null)
				unmatch(value);
		}

		public static TReturn TypeMatch<T1, T2, T3, T4, T5, T6, TReturn>(this object value, Func<T1, TReturn> d1, Func<T2, TReturn> d2, Func<T3, TReturn> d3, Func<T4, TReturn> d4, Func<T5, TReturn> d5, Func<T6, TReturn> d6)
		{
			if (value == null)
				return default(TReturn);
			else if (value is T1)
				return d1((T1)value);
			else if (value is T2)
				return d2((T2)value);
			else if (value is T3)
				return d3((T3)value);
			else if (value is T4)
				return d4((T4)value);
			else if (value is T5)
				return d5((T5)value);
			else if (value is T6)
				return d6((T6)value);
			else
				return default(TReturn);
		}

		public static TReturn TypeMatch<T1, T2, T3, T4, T5, T6, TReturn>(this object value, Func<T1, TReturn> d1, Func<T2, TReturn> d2, Func<T3, TReturn> d3, Func<T4, TReturn> d4, Func<T5, TReturn> d5, Func<T6, TReturn> d6, Func<object, TReturn> unmatch)
		{
			if (value == null)
				return unmatch(value);
			else if (value is T1)
				return d1((T1)value);
			else if (value is T2)
				return d2((T2)value);
			else if (value is T3)
				return d3((T3)value);
			else if (value is T4)
				return d4((T4)value);
			else if (value is T5)
				return d5((T5)value);
			else if (value is T6)
				return d6((T6)value);
			else if (unmatch != null)
				return unmatch(value);
			else
				return default(TReturn);
		}

		public static void TypeMatch<T1, T2, T3, T4, T5, T6, T7>(this object value, Action<T1> d1, Action<T2> d2, Action<T3> d3, Action<T4> d4, Action<T5> d5, Action<T6> d6, Action<T7> d7)
		{
			if (value == null)
			{
			}
			else if (value is T1)
				d1((T1)value);
			else if (value is T2)
				d2((T2)value);
			else if (value is T3)
				d3((T3)value);
			else if (value is T4)
				d4((T4)value);
			else if (value is T5)
				d5((T5)value);
			else if (value is T6)
				d6((T6)value);
			else if (value is T7)
				d7((T7)value);
		}

		public static void TypeMatch<T1, T2, T3, T4, T5, T6, T7>(this object value, Action<T1> d1, Action<T2> d2, Action<T3> d3, Action<T4> d4, Action<T5> d5, Action<T6> d6, Action<T7> d7, Action<object> unmatch)
		{
			if (value == null)
				unmatch(value);
			else if (value is T1)
				d1((T1)value);
			else if (value is T2)
				d2((T2)value);
			else if (value is T3)
				d3((T3)value);
			else if (value is T4)
				d4((T4)value);
			else if (value is T5)
				d5((T5)value);
			else if (value is T6)
				d6((T6)value);
			else if (value is T7)
				d7((T7)value);
			else if (unmatch != null)
				unmatch(value);
		}

		public static TReturn TypeMatch<T1, T2, T3, T4, T5, T6, T7, TReturn>(this object value, Func<T1, TReturn> d1, Func<T2, TReturn> d2, Func<T3, TReturn> d3, Func<T4, TReturn> d4, Func<T5, TReturn> d5, Func<T6, TReturn> d6, Func<T7, TReturn> d7)
		{
			if (value == null)
				return default(TReturn);
			else if (value is T1)
				return d1((T1)value);
			else if (value is T2)
				return d2((T2)value);
			else if (value is T3)
				return d3((T3)value);
			else if (value is T4)
				return d4((T4)value);
			else if (value is T5)
				return d5((T5)value);
			else if (value is T6)
				return d6((T6)value);
			else if (value is T7)
				return d7((T7)value);
			else
				return default(TReturn);
		}

		public static TReturn TypeMatch<T1, T2, T3, T4, T5, T6, T7, TReturn>(this object value, Func<T1, TReturn> d1, Func<T2, TReturn> d2, Func<T3, TReturn> d3, Func<T4, TReturn> d4, Func<T5, TReturn> d5, Func<T6, TReturn> d6, Func<T7, TReturn> d7, Func<object, TReturn> unmatch)
		{
			if (value == null)
				return unmatch(value);
			else if (value is T1)
				return d1((T1)value);
			else if (value is T2)
				return d2((T2)value);
			else if (value is T3)
				return d3((T3)value);
			else if (value is T4)
				return d4((T4)value);
			else if (value is T5)
				return d5((T5)value);
			else if (value is T6)
				return d6((T6)value);
			else if (value is T7)
				return d7((T7)value);
			else if (unmatch != null)
				return unmatch(value);
			else
				return default(TReturn);
		}

		public static void TypeMatch<T1, T2, T3, T4, T5, T6, T7, T8>(this object value, Action<T1> d1, Action<T2> d2, Action<T3> d3, Action<T4> d4, Action<T5> d5, Action<T6> d6, Action<T7> d7, Action<T8> d8)
		{
			if (value == null)
			{
			}
			else if (value is T1)
				d1((T1)value);
			else if (value is T2)
				d2((T2)value);
			else if (value is T3)
				d3((T3)value);
			else if (value is T4)
				d4((T4)value);
			else if (value is T5)
				d5((T5)value);
			else if (value is T6)
				d6((T6)value);
			else if (value is T7)
				d7((T7)value);
			else if (value is T8)
				d8((T8)value);
		}

		public static void TypeMatch<T1, T2, T3, T4, T5, T6, T7, T8>(this object value, Action<T1> d1, Action<T2> d2, Action<T3> d3, Action<T4> d4, Action<T5> d5, Action<T6> d6, Action<T7> d7, Action<T8> d8, Action<object> unmatch)
		{
			if (value == null)
				unmatch(value);
			else if (value is T1)
				d1((T1)value);
			else if (value is T2)
				d2((T2)value);
			else if (value is T3)
				d3((T3)value);
			else if (value is T4)
				d4((T4)value);
			else if (value is T5)
				d5((T5)value);
			else if (value is T6)
				d6((T6)value);
			else if (value is T7)
				d7((T7)value);
			else if (value is T8)
				d8((T8)value);
			else if (unmatch != null)
				unmatch(value);
		}

		public static TReturn TypeMatch<T1, T2, T3, T4, T5, T6, T7, T8, TReturn>(this object value, Func<T1, TReturn> d1, Func<T2, TReturn> d2, Func<T3, TReturn> d3, Func<T4, TReturn> d4, Func<T5, TReturn> d5, Func<T6, TReturn> d6, Func<T7, TReturn> d7, Func<T8, TReturn> d8)
		{
			if (value == null)
				return default(TReturn);
			else if (value is T1)
				return d1((T1)value);
			else if (value is T2)
				return d2((T2)value);
			else if (value is T3)
				return d3((T3)value);
			else if (value is T4)
				return d4((T4)value);
			else if (value is T5)
				return d5((T5)value);
			else if (value is T6)
				return d6((T6)value);
			else if (value is T7)
				return d7((T7)value);
			else if (value is T8)
				return d8((T8)value);
			else
				return default(TReturn);
		}

		public static TReturn TypeMatch<T1, T2, T3, T4, T5, T6, T7, T8, TReturn>(this object value, Func<T1, TReturn> d1, Func<T2, TReturn> d2, Func<T3, TReturn> d3, Func<T4, TReturn> d4, Func<T5, TReturn> d5, Func<T6, TReturn> d6, Func<T7, TReturn> d7, Func<T8, TReturn> d8, Func<object, TReturn> unmatch)
		{
			if (value == null)
				return unmatch(value);
			else if (value is T1)
				return d1((T1)value);
			else if (value is T2)
				return d2((T2)value);
			else if (value is T3)
				return d3((T3)value);
			else if (value is T4)
				return d4((T4)value);
			else if (value is T5)
				return d5((T5)value);
			else if (value is T6)
				return d6((T6)value);
			else if (value is T7)
				return d7((T7)value);
			else if (value is T8)
				return d8((T8)value);
			else if (unmatch != null)
				return unmatch(value);
			else
				return default(TReturn);
		}

	}
}