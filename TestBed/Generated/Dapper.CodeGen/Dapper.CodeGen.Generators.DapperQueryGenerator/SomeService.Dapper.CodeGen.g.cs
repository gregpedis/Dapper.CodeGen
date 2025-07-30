using System;
using Dapper;

namespace Database.Repositories.Dapper
{
	partial class SomeService
	{
		public Database.Repositories.Dapper.Product FindById(object id)
		{
			var sql = @"
			SELECT
				x.cool_name as Name,
				x.fixed_price as FixedPrice,
				x.my_id as Id
			FROM
				my_products as x
			WHERE
				x.my_id = @id
			";

			var result = GetConnection().QuerySingle<Database.Repositories.Dapper.Product>(sql, new { id });
			return result;
		}
	}
}

