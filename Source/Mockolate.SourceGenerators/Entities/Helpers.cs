using System.Collections.Immutable;
using System.Text;
using Microsoft.CodeAnalysis;
using Mockolate.SourceGenerators.Internals;

namespace Mockolate.SourceGenerators.Entities;

internal static class Helpers
{
	public static SpecialGenericType GetSpecialType(this ITypeSymbol typeSymbol)
	{
		if (typeSymbol.ContainingNamespace is { Name: "System", ContainingNamespace.IsGlobalNamespace: true, })
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
		         typeSymbol.ContainingNamespace.ContainingNamespace?.ContainingNamespace?.ContainingNamespace
			         ?.IsGlobalNamespace == true)
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

	public static StringBuilder AppendTypeOrWrapper(this StringBuilder sb, Type type)
	{
		if (type.SpecialGenericType == SpecialGenericType.Span)
		{
			sb.Append("SpanWrapper<").Append(type.GenericTypeParameters!.Value.First().Fullname).Append(">");
		}
		else if (type.SpecialGenericType == SpecialGenericType.ReadOnlySpan)
		{
			sb.Append("ReadOnlySpanWrapper<").Append(type.GenericTypeParameters!.Value.First().Fullname).Append(">");
		}
		else
		{
			sb.Append(type.Fullname);
		}

		return sb;
	}

	public static string ToNameOrWrapper(this MethodParameter parameter)
	{
		if (parameter.Type.SpecialGenericType == SpecialGenericType.Span)
		{
			return $"new SpanWrapper<{parameter.Type.GenericTypeParameters!.Value.First().Fullname}>({parameter.Name})";
		}

		if (parameter.Type.SpecialGenericType == SpecialGenericType.ReadOnlySpan)
		{
			return
				$"new ReadOnlySpanWrapper<{parameter.Type.GenericTypeParameters!.Value.First().Fullname}>({parameter.Name})";
		}

		return parameter.Name;
	}

	public static string ToNameOrNull(this MethodParameter parameter)
	{
		if (parameter.Type.SpecialGenericType is SpecialGenericType.Span or SpecialGenericType.ReadOnlySpan)
		{
			return "null";
		}

		return parameter.Name;
	}

	public static StringBuilder AppendVerifyParameter(this StringBuilder sb, MethodParameter parameter)
	{
		sb.Append((parameter.RefKind, parameter.Type.SpecialGenericType) switch
		{
			(RefKind.Ref, _) => "IVerifyRefParameter<",
			(RefKind.Out, _) => "IVerifyOutParameter<",
			(_, SpecialGenericType.Span) => "IVerifySpanParameter<",
			(_, SpecialGenericType.ReadOnlySpan) => "IVerifyReadOnlySpanParameter<",
			(_, _) => "IParameter<",
		});
		sb.Append(parameter.Type.SpecialGenericType switch
		{
			SpecialGenericType.Span => parameter.Type.GenericTypeParameters!.Value.First().Fullname,
			SpecialGenericType.ReadOnlySpan => parameter.Type.GenericTypeParameters!.Value.First().Fullname,
			_ => parameter.Type.Fullname,
		}).Append('>');

		return sb;
	}

	public static string ToParameter(this MethodParameter parameter)
	{
		return (parameter.RefKind, parameter.Type.SpecialGenericType) switch
		{
			(RefKind.Ref, _) => $"IRefParameter<{GetType(parameter)}>",
			(RefKind.Out, _) => $"IOutParameter<{GetType(parameter)}>",
			(RefKind.RefReadOnlyParameter, _) => $"IRefParameter<{GetType(parameter)}>",
			(_, SpecialGenericType.Span) => $"ISpanParameter<{GetType(parameter)}>",
			(_, SpecialGenericType.ReadOnlySpan) => $"IReadOnlySpanParameter<{GetType(parameter)}>",
			(_, _) => $"IParameter<{GetType(parameter)}>",
		};

		static string GetType(MethodParameter parameter)
		{
			return parameter.Type.SpecialGenericType switch
			{
				SpecialGenericType.Span => parameter.Type.GenericTypeParameters!.Value.First().Fullname,
				SpecialGenericType.ReadOnlySpan => parameter.Type.GenericTypeParameters!.Value.First().Fullname,
				_ => parameter.Type.Fullname,
			};
		}
	}

	public static bool IsNullable(this MethodParameter parameter)
		=> parameter.RefKind is not RefKind.Ref and not RefKind.Out and not RefKind.RefReadOnlyParameter &&
		   parameter.Type.SpecialGenericType is not (SpecialGenericType.Span or SpecialGenericType.ReadOnlySpan);


	public static StringBuilder AppendDefaultValueGeneratorFor(this StringBuilder sb, Type type,
		string defaultValueName, string suffix = "")
	{
		sb.Append(defaultValueName);
		sb.Append(".Generate(default(");

		if (type.SpecialGenericType == SpecialGenericType.Span)
		{
			sb.Append("SpanWrapper<").Append(type.GenericTypeParameters!.Value.First().Fullname).Append(">");
		}
		else if (type.SpecialGenericType == SpecialGenericType.ReadOnlySpan)
		{
			sb.Append("ReadOnlySpanWrapper<").Append(type.GenericTypeParameters!.Value.First().Fullname).Append(">");
		}
		else
		{
			sb.Append(type.Fullname);
		}

		sb.Append(")!");

		if (type.TupleTypes is not null)
		{
			foreach (Type? genericType in type.TupleTypes.Value)
			{
				sb.Append(", () => ").AppendDefaultValueGeneratorFor(genericType, defaultValueName);
			}
		}
		else if (type.SpecialGenericType != SpecialGenericType.None && type.GenericTypeParameters?.Count > 0)
		{
			foreach (Type? genericType in type.GenericTypeParameters.Value)
			{
				sb.Append(", () => ").AppendDefaultValueGeneratorFor(genericType, defaultValueName);
			}
		}

		sb.Append(suffix);
		sb.Append(")");
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
	///     Generates a unique local variable name that does not conflict with any parameter names.
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
