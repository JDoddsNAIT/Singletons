using System;

namespace Singletons.Editor
{
	internal interface ISearchQueryHandler<in T>
	{
		Predicate<T> GetMatchDelegate(string query);
	}

	internal readonly struct ContainsHandler : ISearchQueryHandler<Type>
	{
		public Predicate<Type> GetMatchDelegate(string query)
		{
			return type => {
				return type.FullName.Contains(query, StringComparison.InvariantCultureIgnoreCase);
			};
		}
	}
}
