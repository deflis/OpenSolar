using Ignition.Aspect;

namespace Solar
{
	public class CachedValueAttribute : AspectAttribute
	{
		bool cached;
		object cache = null;

		public override dynamic Invoke(AspectCall call)
		{
			if (cached)
				return cache;
			else
			{
				cached = true;

				return cache = base.Invoke(call);
			}
		}
	}
}
