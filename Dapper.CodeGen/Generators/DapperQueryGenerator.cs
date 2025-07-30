using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System.Text;
using Dapper.CodeGen.Extensions;
using System.Collections.Immutable;

namespace Dapper.CodeGen.Generators;

[Generator]
public class DapperQueryGenerator : IIncrementalGenerator
{
	public void Initialize(IncrementalGeneratorInitializationContext context)
	{
		var codeGenInputs = context.SyntaxProvider
			.CreateSyntaxProvider(
				predicate: static (node, _) => ShouldRun(node),
				transform: static (context, _) => GetCodeGenInput(context));

		context.RegisterSourceOutput(
			codeGenInputs,
			static (c, input) => Execute(c, input));
	}

	private static bool ShouldRun(SyntaxNode node) =>
		node is ClassDeclarationSyntax @class
		&& @class.BaseList?.Types.Count > 0
		&& RoslynExtensions.IsPartial(@class);

	private static CodeGenInput? GetCodeGenInput(GeneratorSyntaxContext context)
	{
		var @class = (ClassDeclarationSyntax)context.Node;
		if (!@class.IsDapperRepository(context.SemanticModel))
		{
			return null;
		}
		if (@class.GetDapperEntity(context.SemanticModel) is not { } dapperEntity)
		{
			return null;
		}

		var classSymbol = context.SemanticModel.GetDeclaredSymbol(@class);
		var methodInputs = classSymbol.GetMethodsForCodeGeneration()
			.Select(methodSymbol => new MethodCodeGenInput(methodSymbol))
			.ToArray();

		return new(
			classSymbol.FullNameWithoutGenerics(),
			methodInputs,
			GetDtoInput(dapperEntity));
	}

	private static void Execute(SourceProductionContext context, CodeGenInput? codeGenInput)
	{
		if (codeGenInput is { IsValid: true } input)
		{
			context.AddSource(
				$"{input.ClassName}.Dapper.CodeGen.g.cs",
				SourceText.From(input.GeneratePartialClass(), Encoding.UTF8));
		}
	}

	// From DTO, get:
	// 	TableName
	// IdColumnName
	// TableColumnNames
	private static DtoCodeGenInput GetDtoInput(ITypeSymbol entity)
	{
		// TODO: Should look for the entity attribute and extract the name
		// TODO: Should look for a property marked with the id attribute OR called "ID". (Add attribute)
		// TODO: Should look for all the property names and see if they are marked. (Add attribute)
		return new DtoCodeGenInput(
			entity.GetEntityTableName(),
			"id",
			ImmutableArray.Create("foo"));
	}

	public const string GeneratedNamespace = "Dapper.CodeGen.Repositories";
}
