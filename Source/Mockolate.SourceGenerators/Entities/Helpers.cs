using System.Collections.Immutable;
using System.Net.Mime;
using System.Text;
using Microsoft.CodeAnalysis;

namespace Mockolate.SourceGenerators.Entities;

internal static class Helpers
{
	public static StringBuilder Append(this StringBuilder sb, ObsoleteAttribute? obsolete, string prefix = "")
	{
		if (obsolete is null)
		{
			return sb;
		}
		
		sb.Append(prefix).Append("[System.Obsolete(");
		if (obsolete.Text is not null)
		{
			sb.Append("\"").Append(obsolete.Text.Replace("\"", "\\\"")).Append("\"");
		}

		sb.Append(")]").AppendLine();
		return sb;
	}
	public static ObsoleteAttribute? GetObsoleteAttribute(this ImmutableArray<AttributeData> attributes)
	{
		AttributeData? obsoleteAttribute = attributes
			.FirstOrDefault(attribute => attribute.AttributeClass is
			{
				ContainingNamespace: { Name: "System", ContainingNamespace.IsGlobalNamespace: true, },
				Name: "ObsoleteAttribute",
			});
		if (obsoleteAttribute is not null)
		{
			return new ObsoleteAttribute(
				obsoleteAttribute.ConstructorArguments.Length > 0 &&
				obsoleteAttribute.ConstructorArguments[0].Type?.SpecialType == SpecialType.System_String
					? obsoleteAttribute.ConstructorArguments[0].Value as string
					: null);
		}

		return null;
	}
}
