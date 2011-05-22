using System.Collections;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using Ignition;
using Microsoft.Scripting.Runtime;

[assembly: ExtensionType(typeof(IEnumerable), typeof(Solar.Scripting.LinqExtensions))]
namespace Solar.Scripting
{
	public static class LinqExtensions
	{
		delegate dynamic DynamicInvokeDelegate(params dynamic[] args);

		// Generics の解決をする必要があるので保留

		[SpecialName]
		public static object GetCustomMember(object self, string name)
		{
			if (self is IEnumerable)
			{
				var m = typeof(Enumerable).GetMethod(name, BindingFlags.Static | BindingFlags.Public);

				if (m == null ||
					!m.GetCustomAttributes(typeof(ExtensionAttribute), false).Any())
					return OperationFailed.Value;
				else
					return (DynamicInvokeDelegate)(_ => m.Invoke(null, _.Prepend(((IEnumerable)self).Cast<object>()).ToArray()));
			}
			else
				return OperationFailed.Value;
		}
	}
}
