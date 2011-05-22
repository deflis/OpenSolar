using System;
using System.Collections.Generic;
using Ignition;
using Lunar;
using Solar.Models;

namespace Solar
{
#pragma warning disable 1591

	class ClientParameters
	{
		public event EventHandler<EventArgs<Category>> RequestAddCategory;

		public ClientParameters(IEnumerable<AccountToken> accounts, StatusCache statusCache, StatusesListBoxCommandHandler statusesListBoxCommandHandler)
		{
			this.StatusCache = statusCache;
			this.Accounts = accounts;
			this.StatusesListBoxCommandHandler = statusesListBoxCommandHandler;
		}

		public StatusCache StatusCache
		{
			get;
			private set;
		}

		public IEnumerable<AccountToken> Accounts
		{
			get;
			private set;
		}

		public StatusesListBoxCommandHandler StatusesListBoxCommandHandler
		{
			get;
			private set;
		}

		public void AddCategory(Category category)
		{
			RequestAddCategory.RaiseEvent(this, new EventArgs<Category>(category));
		}
	}

#pragma warning restore 1591
}
