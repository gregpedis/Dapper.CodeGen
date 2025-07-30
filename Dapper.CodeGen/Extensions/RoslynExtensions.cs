using Dapper.CodeGen.Generators;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Immutable;

namespace Dapper.CodeGen.Extensions;

public static class RoslynExtensions
{
	public static string FullNameWithoutGenerics(this ITypeSymbol type)
	{
		var fullname = type.ToDisplayString();
		var genericStart = fullname.IndexOf('<');
		return genericStart >= 0 ? fullname.Substring(0, genericStart) : fullname;
	}

	// TODO: Check if Dapper is referenced in the compilation.
	public static bool IsDapperRepository(this ClassDeclarationSyntax @class, SemanticModel model) =>
		@class.BaseList.Types
			.Select(type => model.GetTypeInfo(type.Type).Type)
			.Where(type => type is not null)
			.Any(type => type.Is(IDapperRepositoryFullName));

	public static bool IsPartial(MemberDeclarationSyntax member) =>
		member.Modifiers.Any(x => x.IsKind(SyntaxKind.PartialKeyword));

	public static ITypeSymbol GetDapperEntity(this ClassDeclarationSyntax @class, SemanticModel model) =>
		@class.BaseList.Types
			.Select(x => model.GetTypeInfo(x.Type).Type)
			.Where(x => x is INamedTypeSymbol { TypeKind: TypeKind.Interface })
			.Cast<INamedTypeSymbol>()
			.FirstOrDefault(x => x.Is(IDapperRepositoryFullName) && x.TypeArguments.Length == 1)?
			.TypeArguments[0];

	public static IMethodSymbol[] GetMethodsForCodeGeneration(this INamedTypeSymbol type)
	{
		var methods = type.GetMembers()
			.Where(x => x.Kind == SymbolKind.Method)
			.Cast<IMethodSymbol>()
			.Where(IsPartial)
			.Where(x => x.GetAttributes().Any(x => x.AttributeClass.Is(DapperOperationAttributeName)))
			.ToArray();

		return methods;

		static bool IsPartial(IMethodSymbol method) =>
			method.DeclaringSyntaxReferences.FirstOrDefault()?.GetSyntax() is MethodDeclarationSyntax syntax
			&& RoslynExtensions.IsPartial(syntax);
	}

	public static IPropertySymbol[] GetColumnProperties(this ITypeSymbol entity)
	{
		return entity.GetMembers()
			.Where(x => x.Kind == SymbolKind.Property)
			.Cast<IPropertySymbol>()
			.Where(x => x.GetAttributes().Any(x => x.AttributeClass.Is(DapperColumnAttributeName)))
			.ToArray();
	}

	public static string GetEntityTableName(this ITypeSymbol entity, SemanticModel model) =>
		entity.GetSqlName(DapperEntityAttributeName, model);

	public static string GetColumnName(this IPropertySymbol column, SemanticModel model) =>
		column.GetSqlName(DapperColumnAttributeName, model);

	public static bool IsIdColumn(this IPropertySymbol id) =>
		id.GetAttributes().Any(x => x.Equals(DapperIdAttributeName));

	private static string GetSqlName(this ISymbol appliedTo, string attributeName, SemanticModel model)
	{
		var entityAttribute = appliedTo
			.GetAttributes()
			.FirstOrDefault(x => x.AttributeClass.Is(attributeName));
		var syntax = entityAttribute.ApplicationSyntaxReference.GetSyntax() as AttributeSyntax;

		return syntax.GetNameArgumentConstantValue(model) ?? appliedTo.Name.ToSqlName();
	}

	private static string GetNameArgumentConstantValue(this AttributeSyntax attribute, SemanticModel model)
	{
		if (attribute.ArgumentList is { } list
			&& list.Arguments.Count > 0)
		{
			var maybeValue = model.GetConstantValue(list.Arguments[0].Expression);
			return maybeValue.HasValue ? (string)maybeValue.Value : null;
		}
		return null;
	}

	private static bool Is(this ITypeSymbol type, string fullName) =>
		type.FullNameWithoutGenerics() == fullName;

	private const string DapperOperationAttributeName = $"{MarkerGenerator.GeneratedNamespace}.{nameof(MarkerGenerator.DapperOperationAttribute)}";

	private const string IDapperRepositoryFullName = $"{MarkerGenerator.GeneratedNamespace}.{nameof(MarkerGenerator.IDapperRepository)}";

	private const string DapperEntityAttributeName = $"{MarkerGenerator.GeneratedNamespace}.{nameof(MarkerGenerator.DapperEntityAttribute)}";

	private const string DapperIdAttributeName = $"{MarkerGenerator.GeneratedNamespace}.{nameof(MarkerGenerator.DapperIdAttribute)}";

	private const string DapperColumnAttributeName = $"{MarkerGenerator.GeneratedNamespace}.{nameof(MarkerGenerator.DapperColumnAttribute)}";
}
