using Microsoft.CodeAnalysis;
using System.Collections.Immutable;
using System.Reflection;

namespace Dapper.CodeGen;

public readonly record struct CodeGenInput
{
	// Needed from class.
	public readonly string ClassName;
	public readonly MethodCodeGenInput[] MethodInputs;
	public readonly DtoCodeGenInput DtoInput;

	public CodeGenInput(string className, MethodCodeGenInput[] methods, DtoCodeGenInput dtoInput)
	{
		ClassName = className;
		this.MethodInputs = methods;
		DtoInput = dtoInput;
	}

	public bool IsValid =>
		!string.IsNullOrWhiteSpace(ClassName)
		&& !string.IsNullOrWhiteSpace(DtoInput.TableName)
		&& !string.IsNullOrWhiteSpace(DtoInput.IdColumnName)
		&& !DtoInput.TableColumnNames.IsEmpty;
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
public readonly record struct DtoCodeGenInput
{
	public readonly string TableName;
	public readonly string IdColumnName;
	public readonly ImmutableArray<string> TableColumnNames;

	public DtoCodeGenInput(
		string tableName,
		string idColumnName,
		ImmutableArray<string> tableColumnNames)
	{
		TableName = tableName;
		IdColumnName = idColumnName;
		TableColumnNames = tableColumnNames;
	}
}
