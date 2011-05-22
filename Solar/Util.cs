using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using Ignition.Presentation;
using Lunar;

namespace Solar
{
	static partial class Util
	{
		public static IEnumerable<IEntry> ReduceAuthenticatedQuery(this IEnumerable<IEntry> self)
		{
			using (new ReduceAuthenticatedQueryScope())
				foreach (var i in self)
					yield return i;
		}

		public static void ResizeInScreen(this Window self, double x, double y, double width, double height)
		{
			var scr = ScreenHelper.GetScreenFromWindow(self);

			if (width > scr.Width)
				width = scr.Width;

			if (height > scr.Height)
				height = scr.Height;

			if (x < scr.X)
				x = scr.X;
			else if (x + width > scr.X + scr.Width)
				x = scr.X + scr.Width - width;

			if (y < scr.Y)
				y = scr.Y;
			else if (y + height > scr.Y + scr.Height)
				y = scr.Y + scr.Height - height;

			self.Left = x;
			self.Top = y;
			self.Width = width;
			self.Height = height;
		}
	}
}
