namespace Dapper.CodeGen.Markers
{
	[System.AttributeUsage(System.AttributeTargets.Property)]
	public sealed class DapperColumnAttribute : System.Attribute
	{
		public string Name { get; set; }

		// Marker class, nothing to see here.
	}
}