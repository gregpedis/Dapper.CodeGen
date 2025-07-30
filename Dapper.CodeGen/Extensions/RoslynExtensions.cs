using Dapper.CodeGen.Generators;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

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
			.Where(x => x.GetAttributes().Any(x => x.AttributeClass.Is(TargetMethodAttributeName)))
			.ToArray();

		return methods;

		static bool IsPartial(IMethodSymbol method) =>
			method.DeclaringSyntaxReferences.FirstOrDefault()?.GetSyntax() is MethodDeclarationSyntax syntax
			&& RoslynExtensions.IsPartial(syntax);
	}

	public static string GetEntityTableName(this ITypeSymbol entity)
	{
		var thingy = entity
			.GetAttributes()
			.FirstOrDefault(x => x.AttributeClass.Is(DapperEntityAttributeName));

		// "DapperEntity(Name = "product")"
		var a = thingy.ApplicationSyntaxReference.GetSyntax().ToFullString();
		// use RegexExtensions

		return "";
	}

	public static bool Is(this ITypeSymbol type, string fullName) =>
		type.FullNameWithoutGenerics() == fullName;

	private const string TargetMethodAttributeName = $"{MarkerGenerator.GeneratedNamespace}.{nameof(MarkerGenerator.DapperOperationAttribute)}";

	private const string IDapperRepositoryFullName = $"{MarkerGenerator.GeneratedNamespace}.{nameof(MarkerGenerator.IDapperRepository)}";

	private const string DapperEntityAttributeName = $"{MarkerGenerator.GeneratedNamespace}.{nameof(MarkerGenerator.DapperEntityAttribute)}";

}
