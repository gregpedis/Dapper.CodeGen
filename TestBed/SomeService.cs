using Dapper;
using Dapper.CodeGen.Markers;
using System.Data;

namespace Database.Repositories.Dapper;

public partial class SomeService : IDapperRepository<Product>
{
	public IDbConnection GetConnection() => null;
}

[DapperEntity(Name = "my_products")]
public class Product
{
	[DapperId]
	[DapperColumn(Name = "my_id")]
	public int Id { get; set; }

	[DapperColumn(Name = "cool_name")]
	public string Name { get; set; }

	[DapperColumn(Name = "great_price")]
	public float Price { get; set; }

	public int IRRELEVANT_COLUMN_LOOOOOOOL { get; set; }
}


































































[DapperEntity]
public struct Customer
{
	public int Id { get; set; }
	public string FirstName { get; set; }
	public string LastName { get; set; }
}



public partial class SomeService
{
	[DapperOperation]
	public partial List<Product> GetByName(string name);

	public List<Product> GetByNamedWithoutCodeGeneration(string name)
	{
		var sql = "SELECT * FROM Product as p WHERE p.name = @name";
		var result = GetConnection().Query<Product>(sql, new { name });
		return result.ToList();
	}

	public partial List<Product> GetByName(string name)
	{
		return null;
	}
}
