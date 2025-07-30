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
			classSymbol.ContainingNamespace.ToDisplayString(),
			classSymbol.Name,
			methodInputs,
			GetEntityInput(dapperEntity, context.SemanticModel));
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

	private static EntityCodeGenInput GetEntityInput(ITypeSymbol entity, SemanticModel model)
	{
		var columnProperties = entity.GetColumnProperties();

		var propertiesToColumns = columnProperties
			.ToImmutableDictionary(
			x => x.Name,
			x => x.GetColumnName(model));

		return new EntityCodeGenInput(
			entity.FullNameWithoutGenerics(),
			entity.GetEntityTableName(model),
			GetIdColumnName(columnProperties, propertiesToColumns),
			propertiesToColumns);
	}

	private static string GetIdColumnName(
		IPropertySymbol[] columnProperties,
		ImmutableDictionary<string, string> propertiesToColumns)
	{
		// Try to find via the [DapperId] attribute the correct property/column.
		if (Array.Find(columnProperties, x => x.IsIdColumn()) is { }  idColumn)
		{
			if (propertiesToColumns.TryGetValue(idColumn.Name, out var found))
			{
				return found;
			}
		}
		// Otherwise, just try to find via the [DapperColumn] attribute a property called id.
		else if (propertiesToColumns.Keys.FirstOrDefault(x => x.ToLower() == "id") is { } found)
		{
			return propertiesToColumns[found];
		}

		return string.Empty;
	}
}
