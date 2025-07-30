using Dapper.CodeGen.Generators;

namespace Dapper.CodeGen.Extensions;

public static class SourceGenerationExtensions
{
	public static string GeneratePartialClass(this CodeGenInput input)
	{
		var res = $$"""
		using Dapper;

		namespace {{DapperQueryGenerator.GeneratedNamespace}}
		{
			partial class {{input.ClassName}}
			{
				private void Testing ()
				{

				}
			}
		}
		""";

		return res;
	}
}

