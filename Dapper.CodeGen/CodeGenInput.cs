using Microsoft.CodeAnalysis;
using System.Collections.Immutable;
using System.Reflection;

namespace Dapper.CodeGen;

public readonly record struct CodeGenInput
{
	public readonly string NamespaceName;
	public readonly string ClassName;
	public readonly MethodCodeGenInput[] MethodInputs;
	public readonly EntityCodeGenInput EntityInput;

	public CodeGenInput(
		string namespaceName,
		string className,
		MethodCodeGenInput[] methods,
		EntityCodeGenInput entityInput)
	{
		NamespaceName = namespaceName;
		ClassName = className;
		this.MethodInputs = methods;
		EntityInput = entityInput;
	}

	public bool IsValid =>
		!string.IsNullOrWhiteSpace(ClassName)
		&& !string.IsNullOrWhiteSpace(EntityInput.TypeName)
		&& !string.IsNullOrWhiteSpace(EntityInput.TableName)
		&& !string.IsNullOrWhiteSpace(EntityInput.IdColumnName)
		&& !EntityInput.PropertiesToColumns.IsEmpty;
}

public readonly record struct MethodCodeGenInput
{
	public readonly string MethodName;
	public readonly ImmutableArray<string> MethodArgumentNames;
	public readonly ITypeSymbol MethodReturnType;

	public MethodCodeGenInput(IMethodSymbol method)
	{
		MethodName = method.Name;
		MethodArgumentNames = ImmutableArray.CreateRange(method.Parameters.Select(x => x.Name));
		MethodReturnType = method.ReturnType;
	}
}
public readonly record struct EntityCodeGenInput
{
	public readonly string TypeName;
	public readonly string TableName;
	public readonly string IdColumnName;
	public readonly ImmutableDictionary<string, string> PropertiesToColumns;

	public EntityCodeGenInput(
		string typeName,
		string tableName,
		string idColumnName,
		ImmutableDictionary<string, string> propertiesToColumns)
	{
		TypeName = typeName;
		TableName = tableName;
		IdColumnName = idColumnName;
		PropertiesToColumns = propertiesToColumns;
	}
}
