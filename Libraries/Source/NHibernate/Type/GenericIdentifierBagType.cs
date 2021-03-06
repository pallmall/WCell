using System;
using System.Collections;
using System.Collections.Generic;

using NHibernate.Collection;
using NHibernate.Collection.Generic;
using NHibernate.Engine;
using NHibernate.Persister.Collection;

namespace NHibernate.Type
{
	[Serializable]
	public class GenericIdentifierBagType<T> : IdentifierBagType
	{
		public GenericIdentifierBagType(string role, string propertyRef)
			: base(role, propertyRef, false)
		{
		}

		public override IPersistentCollection Instantiate(ISessionImplementor session, ICollectionPersister persister, object key)
		{
			return new PersistentIdentifierBag<T>(session);
		}

		public override IPersistentCollection Wrap(ISessionImplementor session, object collection)
		{
			return new PersistentIdentifierBag<T>(session, (ICollection) collection);
		}

		public override System.Type ReturnedClass
		{
			get { return typeof(IList<T>); }
		}

		public override object Instantiate(int anticipatedSize)
		{
			return new List<T>();
		}
	}
}
