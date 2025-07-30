namespace Dapper.CodeGen.Markers
{
	[System.AttributeUsage(System.AttributeTargets.Class | System.AttributeTargets.Struct)]
	public sealed class DapperEntityAttribute : System.Attribute
	{
		public string Name { get; set; }

		// Marker class, nothing to see here.
	}
}