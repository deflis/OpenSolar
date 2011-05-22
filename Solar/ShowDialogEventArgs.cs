using System;
using System.Windows;

namespace Solar
{
	class ShowDialogEventArgs : EventArgs
	{
		Func<Window, Window> createWindow;

		public Window Window
		{
			get;
			private set;
		}

		public bool SetOwner
		{
			get;
			private set;
		}

		public bool? DialogResult
		{
			get;
			set;
		}

		public ShowDialogEventArgs(Window window)
			: this(_ => window, true)
		{
		}

		public ShowDialogEventArgs(Func<Window, Window> createWindow, bool setOwner)
		{
			this.createWindow = createWindow;
			this.SetOwner = setOwner;
		}

		public void Show(Window owner)
		{
			if (createWindow != null)
				this.Window = createWindow(owner);

			if (this.SetOwner)
				this.Window.Owner = owner;

			if (this.Window.Icon == null)
				this.Window.Icon = owner.Icon;

			this.Window.Show();
		}

		public void ShowDialog(Window owner)
		{
			if (createWindow != null)
				this.Window = createWindow(owner);

			if (this.SetOwner)
				this.Window.Owner = owner;

			if (this.Window.Icon == null)
				this.Window.Icon = owner.Icon;

			this.DialogResult = this.Window.ShowDialog();
		}
	}
}
