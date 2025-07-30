using Dapper.CodeGen.Generators;
using System.Text;

namespace Dapper.CodeGen.Extensions;

public static class SourceGenerationExtensions
{
	// TODO:
	// ExistsById
	// FindById

	// DeleteById
	// DeleteAll

	// Insert
	// Update

	// FIX FORMATTING
	public static string GeneratePartialClass(this CodeGenInput input)
	{
		var res = $$"""
		partial class {{input.ClassName}}
		{
			public {{input.EntityInput.TypeName}} FindById(object id)
			{
				var sql = @"
				SELECT
					{{SelectColumnList(input)}}
				FROM
					{{input.EntityInput.TableName}} as x
				WHERE
					x.{{input.EntityInput.IdColumnName}} = @id
				";

				var result = GetConnection().QuerySingle<{{input.EntityInput.TypeName}}>(sql, new { id });
				return result;
			}
		}
		""";

		return WrapInUsings(
			WrapInNamespace(res, input.NamespaceName));
	}

	private static string WrapInNamespace(string inner, string namespaceName)
	{
		if (string.IsNullOrWhiteSpace(namespaceName))
		{
			return inner;
		}
		return $$"""
		namespace {{namespaceName}}
		{
			{{inner}}
		}
		""";
	}

	private static string WrapInUsings(string inner) =>
		$$"""
		using System;
		using Dapper;

		{{inner}}
		""";

	private static string SelectColumnList(CodeGenInput input)
	{
		var sb = new StringBuilder();

		foreach (var kv in input.EntityInput.PropertiesToColumns)
		{
			sb.AppendLine($"x.{kv.Value} as {kv.Key},");
		}

		var result = sb.ToString();
		return result.Substring(0, result.Length - 3); // remove the last ',' and '\n'
	}
}
