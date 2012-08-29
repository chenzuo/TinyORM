namespace TinyORM
{
	public interface ISqlBuilder
	{
		string GetSelect(ITableMap tableMap, SqlBuilderColumns columns, string whereStatement);
		string GetInsert(ITableMap tableMap);
		string GetUpdate(ITableMap tableMap);
		string GetDelete(ITableMap tableMap);
	}
}