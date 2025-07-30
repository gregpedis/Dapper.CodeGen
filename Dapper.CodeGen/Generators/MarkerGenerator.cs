using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using System.Text;

namespace Dapper.CodeGen.Generators;

[Generator]
public class MarkerGenerator : IIncrementalGenerator
{
	public void Initialize(IncrementalGeneratorInitializationContext context)
	{
		// IDapperRepository
		AddMarkerAttribute(context, nameof(IDapperRepository), IDapperRepository);
		AddMarkerAttribute(context, nameof(DapperOperationAttribute), DapperOperationAttribute);

		// DapperEntity
		AddMarkerAttribute(context, nameof(DapperEntityAttribute), DapperEntityAttribute);
		AddMarkerAttribute(context, nameof(DapperIdAttribute), DapperIdAttribute);
		AddMarkerAttribute(context, nameof(DapperColumnAttribute), DapperColumnAttribute);
	}

	private static void AddMarkerAttribute(
		IncrementalGeneratorInitializationContext context,
		string fileName,
		string sourceFrom)
	{
		var sourceText = SourceText.From(sourceFrom, Encoding.UTF8);
		context.RegisterPostInitializationOutput(c => c.AddSource($"{fileName}.g.cs", sourceText));
	}

	public const string IDapperRepository = $$"""
		namespace {{GeneratedNamespace}}
		{
			public interface IDapperRepository<T>
			{
				public System.Data.IDbConnection GetConnection();
			}
		}
		""";

	public const string DapperOperationAttribute = $$"""
		namespace {{GeneratedNamespace}}
		{
			[System.AttributeUsage(System.AttributeTargets.Method)]
			public sealed class {{nameof(DapperOperationAttribute)}} : System.Attribute
			{
				// Marker class, nothing to see here.
			}
		}
		""";

	public const string DapperEntityAttribute = $$"""
		namespace {{GeneratedNamespace}}
		{
			[System.AttributeUsage(System.AttributeTargets.Class | System.AttributeTargets.Struct)]
			public sealed class {{nameof(DapperEntityAttribute)}} : System.Attribute
			{
				public string Name { get; set; }

				// Marker class, nothing to see here.
			}
		}
		""";

	public const string DapperIdAttribute = $$"""
		namespace {{GeneratedNamespace}}
		{
			[System.AttributeUsage(System.AttributeTargets.Property)]
			public sealed class {{nameof(DapperIdAttribute)}} : System.Attribute
			{
				public string Name { get; set; }

				// Marker class, nothing to see here.
			}
		}
		""";

	public const string DapperColumnAttribute = $$"""
		namespace {{GeneratedNamespace}}
		{
			[System.AttributeUsage(System.AttributeTargets.Property)]
			public sealed class {{nameof(DapperColumnAttribute)}} : System.Attribute
			{
				public string Name { get; set; }

				// Marker class, nothing to see here.
			}
		}
		""";

	public const string GeneratedNamespace = "Dapper.CodeGen.Markers";
}
