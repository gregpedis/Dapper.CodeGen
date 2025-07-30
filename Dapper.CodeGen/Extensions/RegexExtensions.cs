using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Dapper.CodeGen.Extensions
{
	// Name\s*=\s*""(?<value>[^""]+)""

	public static class RegexExtensions
	{

		public void Foo()
		{
			var x = new Regex("Name\\s*=\\s*\"\"(?<value>[^\"\"]+)\"\"");
		}
	}
}
