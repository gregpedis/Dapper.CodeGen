namespace Dapper.CodeGen.Markers
{
	public interface IDapperRepository<T>
	{
		public System.Data.IDbConnection GetConnection();
	}
}