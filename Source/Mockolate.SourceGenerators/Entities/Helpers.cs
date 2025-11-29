using System.Collections.Immutable;
using System.Text;
using Microsoft.CodeAnalysis;
using Mockolate.SourceGenerators.Internals;

namespace Mockolate.SourceGenerators.Entities;

internal enum SpecialGenericType
{
	None,
	Span,
	ReadOnlySpan,
	Tuple,
	Task,
	ValueTask,
}

internal static class Helpers
{
	// TODO: Replace with specialgenerictype
	public static bool IsSpanOrReadOnlySpan(this ITypeSymbol typeSymbol, out bool isSpan, out bool isReadOnlySpan,
		out Type? spanType)
	{
		if (typeSymbol.ContainingNamespace is { Name: "System", ContainingNamespace.IsGlobalNamespace: true })
		{
			isSpan = typeSymbol.Name == "Span";
			isReadOnlySpan = typeSymbol.Name == "ReadOnlySpan";
			if ((isSpan || isReadOnlySpan) && typeSymbol is INamedTypeSymbol
			    {
				    TypeArguments.Length: 1,
			    } namedTypeSymbol)
			{
				spanType = new Type(namedTypeSymbol.TypeArguments[0]);
				return true;
			}
		}

		isSpan = false;
		isReadOnlySpan = false;
		spanType = null;
		return false;
	}

	public static SpecialGenericType GetSpecialType(this ITypeSymbol typeSymbol)
	{
		if (typeSymbol.ContainingNamespace is { Name: "System", ContainingNamespace.IsGlobalNamespace: true })
		{
			if (typeSymbol.Name == "Span")
			{
				return SpecialGenericType.Span;
			}

			if (typeSymbol.Name == "ReadOnlySpan")
			{
				return SpecialGenericType.ReadOnlySpan;
			}

			if (typeSymbol.Name == "ValueTuple")
			{
				return SpecialGenericType.Tuple;
			}
		}
		else if (typeSymbol.ContainingNamespace is { Name: "Tasks", ContainingNamespace.Name: "Threading", } &&
		         typeSymbol.ContainingNamespace.ContainingNamespace?.ContainingNamespace.Name == "System" && 
		         typeSymbol.ContainingNamespace.ContainingNamespace?.ContainingNamespace?.ContainingNamespace?.IsGlobalNamespace == true)
		{
			if (typeSymbol.Name == "Task")
			{
				return SpecialGenericType.Task;
			}

			if (typeSymbol.Name == "ValueTask")
			{
				return SpecialGenericType.ValueTask;
			}
		}
		
		return SpecialGenericType.None;
	}

	public static StringBuilder Append(this StringBuilder sb, EquatableArray<Attribute>? attributes, string prefix = "")
	{
		if (attributes is null)
		{
			return sb;
		}

		foreach (Attribute? attribute in attributes)
		{
			sb.Append(prefix).Append('[').Append(attribute.Name);
			if (attribute.Parameters != null || attribute.NamedArguments != null)
			{
				sb.Append('(');
				if (attribute.Parameters != null)
				{
					sb.Append(string.Join(", ", attribute.Parameters.Value.Select(parameter => parameter.Value)));
				}

				if (attribute.NamedArguments != null)
				{
					if (attribute.Parameters != null)
					{
						sb.Append(", ");
					}

					sb.Append(string.Join(", ", attribute.NamedArguments.Value
						.Select(item => $"{item.Name} = {item.Parameter.Value}")));
				}

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

	/// <summary>
	/// Generates a unique local variable name that does not conflict with any parameter names.
	/// </summary>
	/// <param name="baseName">The base name for the variable (e.g., "result", "methodExecution")</param>
	/// <param name="parameters">The parameters to check against for conflicts</param>
	/// <returns>A unique variable name that does not conflict with any parameter names</returns>
	public static string GetUniqueLocalVariableName(string baseName, EquatableArray<MethodParameter> parameters)
	{
		string candidateName = baseName;
		int suffix = 0;

		while (parameters.Any(p => p.Name == candidateName))
		{
			suffix++;
			candidateName = $"{baseName}{suffix}";
		}

		return candidateName;
	}
}
