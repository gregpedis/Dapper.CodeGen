using System;
using System.Collections.Generic;
using System.Text;

namespace Dapper.CodeGen.Extensions;

public static class StringExtensions
{
	// convert Thingy -> thingy
	// convert SomeThingy -> some_thingy
	public static string ToSqlName(this string value)
	{
		if (string.IsNullOrWhiteSpace(value))
		{
			return value;
		}

		var result = new StringBuilder();
		result.Append(char.ToLower(value[0]));

		foreach (var c in value.Skip(1))
		{
			if (char.IsUpper(c))
			{
				result.Append('_');
				result.Append(char.ToLower(c));
			}
			else
			{
				result.Append(c);
			}

		}

		return result.ToString();
	}
}
