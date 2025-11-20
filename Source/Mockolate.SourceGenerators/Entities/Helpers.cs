using System.Collections.Immutable;
using System.Text;
using Microsoft.CodeAnalysis;
using Mockolate.SourceGenerators.Internals;

namespace Mockolate.SourceGenerators.Entities;

internal static class Helpers
{
	public static StringBuilder Append(this StringBuilder sb, EquatableArray<Attribute>? attributes, string prefix = "")
	{
		if (attributes is null)
		{
			return sb;
		}

		foreach (Attribute? attribute in attributes)
		{
			sb.Append(prefix).Append('[').Append(attribute.Name);
			if (attribute.Parameters != null)
			{
				sb.Append('(');
				sb.Append(string.Join(", ", attribute.Parameters.Value.Select(parameter => parameter.Value)));
				sb.Append(')');
			}

			sb.Append("]").AppendLine();
		}

		return sb;
	}

	public static EquatableArray<Attribute>? ToAttributeArray(this ImmutableArray<AttributeData> attributes)
	{
		Attribute[] consideredAttributes = attributes
			.Where(x => x.AttributeClass != null && !IsNullableAttribute(x.AttributeClass))
			.Select(attr => new Attribute(attr))
			.ToArray();
		if (consideredAttributes.Length > 0)
		{
			return new EquatableArray<Attribute>(consideredAttributes);
		}

		return null;
	}

	private static bool IsNullableAttribute(INamedTypeSymbol attribute)
		=> attribute.Name is "NullableContextAttribute" or "NullableAttribute" &&
		   attribute.ContainingNamespace.ToDisplayString() == "System.Runtime.CompilerServices";
}
