using Dapper.CodeGen.Generators;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System.Data;
using System.Reflection;

namespace GeneratorTests.Tests
{
	[TestClass]
	public class DapperQueryGeneratorTests
	{
		[TestMethod]
		public void DapperQueryGenerator_HappyPath()
		{
			var sourceText = """
				using System.Data;
				using System.Collections.Generic;
				using Dapper.CodeGen.Markers;

				public partial class SomeService : IDapperRepository<Product>
				{
					public IDbConnection GetConnection() => null;

					[DapperOperation]
					public partial List<Product> GetByName(string name);

					[DapperOperation]
					public partial List<Product> GetByPrice(float price);
				}

				[DapperEntity(Name = "product")]
				public struct Product
				{
					[DapperId]
					public int Id { get; set; }
					public string Name { get; set; }
					public float Price { get; set; }
				}
				""";

			GeneratorDriver driver = CSharpGeneratorDriver.Create(
				new MarkerGenerator(),
				new DapperQueryGenerator());

			driver = driver.RunGeneratorsAndUpdateCompilation(
				CreateCompilation(sourceText),
				out var outputCompilation,
				out var diagnostics);

			var realDiagnostics = outputCompilation.GetDiagnostics();

			Assert.IsTrue(diagnostics.IsEmpty);
			Assert.IsTrue(realDiagnostics.IsEmpty);
			Assert.AreEqual(7, outputCompilation.SyntaxTrees.Count());

			var runResult = driver.GetRunResult();
			Assert.AreEqual(6, runResult.GeneratedTrees.Length);
			// TODO: Assert the trees exactly.
		}

		private static CSharpCompilation CreateCompilation(string source) =>
			CSharpCompilation.Create(
				"irrelevant",
				[CSharpSyntaxTree.ParseText(source, new CSharpParseOptions(LanguageVersion.Latest))],
				[
					AssemblyFromType<Dapper.DbString>(),
					AssemblyFromType<IDbConnection>(),
					AssemblyFromType<List<int>>(),
					AssemblyFromType<string>(),
				],
				new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

		private static PortableExecutableReference AssemblyFromType<T>() =>
			MetadataReference.CreateFromFile(typeof(T).GetTypeInfo().Assembly.Location);
	}
}

