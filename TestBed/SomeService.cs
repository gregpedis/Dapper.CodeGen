using Dapper;
using Dapper.CodeGen.Markers;
using Microsoft.Data.SqlClient;
using System.Data;

namespace TestBed;

public partial class SomeService : IDapperRepository<Product>
{
	private static readonly SqlConnection _connection = new("connectionString");

	public IDbConnection GetConnection()
		=> _connection;

	[DapperOperation]
	public partial List<Product> GetByName(string name);

	public List<Product> GetByNamedWithoutCodeGeneration(string name)
	{
		var sql = "SELECT * FROM Product as p WHERE p.name = @name";
		var result = _connection.Query<Product>(sql, new { name });
		return result.ToList();
	}
}

public partial class SomeService
{
	public partial List<Product> GetByName(string name)
	{
		return null;
	}
}

[DapperEntity(Name = "product")]
public struct Product
{
	public int Id { get; set; }
	public string Name { get; set; }
	public float Price { get; set; }
}

[DapperEntity]
public struct Customer
{
	public int Id { get; set; }
	public string FirstName { get; set; }
	public string LastName { get; set; }
}
