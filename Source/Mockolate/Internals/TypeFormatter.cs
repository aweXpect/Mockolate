using System;
using System.Collections.Generic;
using System.Text;

namespace Mockolate.Internals;

internal static class TypeFormatter
{
	private static readonly Dictionary<Type, string> Aliases = new()
	{
		{
			typeof(byte), "byte"
		},
		{
			typeof(sbyte), "sbyte"
		},
		{
			typeof(short), "short"
		},
		{
			typeof(ushort), "ushort"
		},
		{
			typeof(int), "int"
		},
		{
			typeof(uint), "uint"
		},
		{
			typeof(long), "long"
		},
		{
			typeof(ulong), "ulong"
		},
		{
			typeof(float), "float"
		},
		{
			typeof(double), "double"
		},
		{
			typeof(decimal), "decimal"
		},
		{
			typeof(object), "object"
		},
		{
			typeof(bool), "bool"
		},
		{
			typeof(char), "char"
		},
		{
			typeof(string), "string"
		},
		{
			typeof(void), "void"
		},
		{
			typeof(nint), "nint"
		},
		{
			typeof(nuint), "nuint"
		},
	};

	internal static string FormatType(this Type value)
	{
		StringBuilder stringBuilder = new();
		FormatType(value, stringBuilder);
		return stringBuilder.ToString();
	}

	private static void FormatType(
		Type value,
		StringBuilder stringBuilder)
	{
		if (value.IsGenericParameter)
		{
			stringBuilder.Append(value.Name);
		}
		else if (value.IsArray)
		{
			FormatType(value.GetElementType()!, stringBuilder);
			stringBuilder.Append("[]");
		}
		else if (!AppendedPrimitiveAlias(value, stringBuilder))
		{
			// Handle nested types
			if (value.DeclaringType is not null)
			{
				FormatType(value.DeclaringType, stringBuilder);
				stringBuilder.Append('.');
			}

			if (value.IsGenericType)
			{
				Type genericTypeDefinition = value.GetGenericTypeDefinition();
				stringBuilder.Append(genericTypeDefinition.Name.SubstringUntilFirst('`'));
				stringBuilder.Append('<');
				bool isFirstArgument = true;
				foreach (Type argument in value.GetGenericArguments())
				{
					if (!isFirstArgument)
					{
						stringBuilder.Append(", ");
					}

					isFirstArgument = false;
					if (!argument.ContainsGenericParameters)
					{
						FormatType(argument, stringBuilder);
					}
				}

				stringBuilder.Append('>');
			}
			else
			{
				stringBuilder.Append(value.Name);
			}
		}
	}

	private static bool AppendedPrimitiveAlias(Type value, StringBuilder stringBuilder)
	{
		if (Aliases.TryGetValue(value, out string? typeAlias))
		{
			stringBuilder.Append(typeAlias);
			return true;
		}

		Type? underlyingType = Nullable.GetUnderlyingType(value);

		if (underlyingType != null)
		{
			if (Aliases.TryGetValue(underlyingType, out string? underlyingAlias))
			{
				stringBuilder.Append(underlyingAlias).Append('?');
				return true;
			}

			FormatType(underlyingType, stringBuilder);
			stringBuilder.Append('?');
			return true;
		}

		return false;
	}
}
