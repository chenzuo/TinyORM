namespace TinyORM
{
	#region LINQ

	//internal class Query<T> : IQuery<T>, IOrderedQuery<T>
	//{
	//   private const string SortOrderAscending = "asc";
	//   private const string SortOrderDescending = "desc";

	//   private readonly TypeMapper _typeMapper;
	//   private readonly List<string> _sortBy;

	//   public Query(TypeMapper typeMapper)
	//   {
	//      _typeMapper = typeMapper;
	//      _sortBy = new List<string>();
	//   }

	//   public IEnumerator<T> GetEnumerator()
	//   {
	//      var builder = new StringBuilder();
	//      builder.AppendLine("select");
	//      foreach (var propertyMapper in _typeMapper.PropertyMappers)
	//      builder.AppendFormat("")
	//      throw new NotImplementedException();
	//   }

	//   IEnumerator IEnumerable.GetEnumerator()
	//   {
	//      return GetEnumerator();
	//   }

	//   public IQuery<T> Where(Expression<Func<T, bool>> expression)
	//   {
	//      throw new NotImplementedException();
	//   }

	//   public IOrderedQuery<T> OrderBy<TKey>(Expression<Func<T, TKey>> keySelector)
	//   {
	//      return OrderBy(keySelector, SortOrderAscending);
	//   }

	//   public IOrderedQuery<T> OrderByDescending<TKey>(Expression<Func<T, TKey>> keySelector)
	//   {
	//      return OrderBy(keySelector, SortOrderDescending);
	//   }

	//   public IOrderedQuery<T> ThenBy<TKey>(Expression<Func<T, TKey>> keySelector)
	//   {
	//      return OrderBy(keySelector, SortOrderAscending);
	//   }

	//   public IOrderedQuery<T> ThenByDescending<TKey>(Expression<Func<T, TKey>> keySelector)
	//   {
	//      return OrderBy(keySelector, SortOrderDescending);
	//   }

	//   private IOrderedQuery<T> OrderBy<TKey>(Expression<Func<T, TKey>> keySelector, string order)
	//   {
	//      _sortBy.Add(string.Format("{0} {1}", "TODO", order));
	//      return this;
	//   }
	//}

	//public interface IQuery<T> : IEnumerable<T>
	//{
	//   IQuery<T> Where(Expression<Func<T, bool>> expression);
	//   IOrderedQuery<T> OrderBy<TKey>(Expression<Func<T, TKey>> keySelector);
	//   IOrderedQuery<T> OrderByDescending<TKey>(Expression<Func<T, TKey>> keySelector);
	//}

	//public interface IOrderedQuery<T> : IEnumerable<T>
	//{
	//   IOrderedQuery<T> ThenBy<TKey>(Expression<Func<T, TKey>> keySelector);
	//   IOrderedQuery<T> ThenByDescending<TKey>(Expression<Func<T, TKey>> keySelector);
	//}

	#endregion
}