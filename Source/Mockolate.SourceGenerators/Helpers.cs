using System.Collections.Immutable;
using System.Text;
using Microsoft.CodeAnalysis;
using Mockolate.SourceGenerators.Entities;
using Mockolate.SourceGenerators.Internals;
using Attribute = Mockolate.SourceGenerators.Entities.Attribute;
using Type = Mockolate.SourceGenerators.Entities.Type;

namespace Mockolate.SourceGenerators;

internal static class Helpers
{
	public static SymbolDisplayFormat TypeDisplayFormat { get; } = new(
		miscellaneousOptions: SymbolDisplayMiscellaneousOptions.EscapeKeywordIdentifiers |
		                      SymbolDisplayMiscellaneousOptions.UseSpecialTypes |
		                      SymbolDisplayMiscellaneousOptions.IncludeNullableReferenceTypeModifier,
		globalNamespaceStyle: SymbolDisplayGlobalNamespaceStyle.Included,
		typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces,
		genericsOptions: SymbolDisplayGenericsOptions.IncludeTypeParameters);

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

	extension(ITypeSymbol typeSymbol)
	{
		public SpecialGenericType GetSpecialType()
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
	}

	extension(Class @class)
	{
		public string GetUniqueName(string name, params string[] alternatives)
		{
			if (!HasName(name))
			{
				return name;
			}

			foreach (var alternative in alternatives)
			{
				if (!HasName(alternative))
				{
					return alternative;
				}
			}

			int index = 1;
			while (HasName($"{name}_{index}"))
			{
				index++;
			}

			return $"{name}_{index}";

			bool HasName(string candidate)
				=> @class.AllProperties().Any(p => p.Name == candidate) ||
				   @class.AllMethods().Any(m => m.Name == candidate) ||
				   @class.AllEvents().Any(e => e.Name == candidate);
		}
	}

	extension(ImmutableArray<AttributeData> attributes)
	{
		public EquatableArray<Attribute>? ToAttributeArray()
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

			static bool IsNullableAttribute(INamedTypeSymbol attribute)
			{
				return attribute.Name is "NullableContextAttribute" or "NullableAttribute" &&
				       attribute.ContainingNamespace.ToDisplayString() == "System.Runtime.CompilerServices";
			}
		}
	}

	extension(StringBuilder sb)
	{
		public StringBuilder AppendDefaultValueGeneratorFor(Type type,
			string defaultValueName, string suffix = "")
		{
			sb.Append(defaultValueName);
			sb.Append(".Generate(default(");

			if (type.SpecialGenericType == SpecialGenericType.Span)
			{
				sb.Append("global::Mockolate.Setup.SpanWrapper<").Append(type.GenericTypeParameters!.Value.First().Fullname).Append(">");
			}
			else if (type.SpecialGenericType == SpecialGenericType.ReadOnlySpan)
			{
				sb.Append("global::Mockolate.Setup.ReadOnlySpanWrapper<").Append(type.GenericTypeParameters!.Value.First().Fullname).Append(">");
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

		public StringBuilder Append(EquatableArray<Attribute>? attributes, string prefix = "")
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

		public StringBuilder AppendTypeOrWrapper(Type type)
		{
			if (type.SpecialGenericType == SpecialGenericType.Span)
			{
				sb.Append("global::Mockolate.Setup.SpanWrapper<").Append(type.GenericTypeParameters!.Value.First().Fullname).Append(">");
			}
			else if (type.SpecialGenericType == SpecialGenericType.ReadOnlySpan)
			{
				sb.Append("global::Mockolate.Setup.ReadOnlySpanWrapper<").Append(type.GenericTypeParameters!.Value.First().Fullname).Append(">");
			}
			else
			{
				sb.Append(type.Fullname);
			}

			return sb;
		}

		public StringBuilder AppendVerifyParameter(MethodParameter parameter)
		{
			sb.Append((parameter.RefKind, parameter.Type.SpecialGenericType) switch
			{
				(RefKind.Ref, _) => "global::Mockolate.Parameters.IVerifyRefParameter<",
				(RefKind.Out, _) => "global::Mockolate.Parameters.IVerifyOutParameter<",
				(_, SpecialGenericType.Span) => "global::Mockolate.Parameters.IVerifySpanParameter<",
				(_, SpecialGenericType.ReadOnlySpan) => "global::Mockolate.Parameters.IVerifyReadOnlySpanParameter<",
				(_, _) => "global::Mockolate.Parameters.IParameter<",
			});
			sb.Append((parameter.Type.SpecialGenericType,
					parameter.IsNullableAnnotated && !parameter.Type.Fullname.EndsWith("?")) switch
				{
					(SpecialGenericType.Span, _) => parameter.Type.GenericTypeParameters!.Value.First().Fullname,
					(SpecialGenericType.ReadOnlySpan, _) => parameter.Type.GenericTypeParameters!.Value.First().Fullname,
					(_, false) => parameter.Type.Fullname,
					(_, true) => $"{parameter.Type.Fullname}?",
				}).Append('>');

			return sb;
		}
	}

	extension(MethodParameter parameter)
	{
		public string ToNameOrWrapper()
		{
			if (parameter.Type.SpecialGenericType == SpecialGenericType.Span)
			{
				return $"new global::Mockolate.Setup.SpanWrapper<{parameter.Type.GenericTypeParameters!.Value.First().Fullname}>({parameter.Name})";
			}

			if (parameter.Type.SpecialGenericType == SpecialGenericType.ReadOnlySpan)
			{
				return
					$"new global::Mockolate.Setup.ReadOnlySpanWrapper<{parameter.Type.GenericTypeParameters!.Value.First().Fullname}>({parameter.Name})";
			}

			return parameter.Name;
		}

		public string ToNameOrNull()
		{
			if (parameter.Type.SpecialGenericType is SpecialGenericType.Span or SpecialGenericType.ReadOnlySpan)
			{
				return "null";
			}

			return parameter.Name;
		}

		public string ToParameter()
		{
			return (parameter.RefKind, parameter.Type.SpecialGenericType) switch
			{
				(RefKind.Ref, _) => $"global::Mockolate.Parameters.IRefParameter<{GetMethodParameterType(parameter)}>",
				(RefKind.Out, _) => $"global::Mockolate.Parameters.IOutParameter<{GetMethodParameterType(parameter)}>",
				(RefKind.RefReadOnlyParameter, _) => $"global::Mockolate.Parameters.IRefParameter<{GetMethodParameterType(parameter)}>",
				(_, SpecialGenericType.Span) => $"global::Mockolate.Parameters.ISpanParameter<{GetMethodParameterType(parameter)}>",
				(_, SpecialGenericType.ReadOnlySpan) =>
					$"global::Mockolate.Parameters.IReadOnlySpanParameter<{GetMethodParameterType(parameter)}>",
				(_, _) => $"global::Mockolate.Parameters.IParameter<{GetMethodParameterType(parameter)}>",
			};

			static string GetMethodParameterType(MethodParameter parameter)
			{
				return (parameter.Type.SpecialGenericType,
						parameter.IsNullableAnnotated && !parameter.Type.Fullname.EndsWith("?")) switch
					{
						(SpecialGenericType.Span, _) => parameter.Type.GenericTypeParameters!.Value.First().Fullname,
						(SpecialGenericType.ReadOnlySpan, _) =>
							parameter.Type.GenericTypeParameters!.Value.First().Fullname,
						(_, false) => parameter.Type.Fullname,
						(_, true) => $"{parameter.Type.Fullname}?",
					};
			}
		}

		public string ToNullableType()
		{
			if (parameter.IsNullableAnnotated && !parameter.Type.Fullname.EndsWith("?"))
			{
				return $"{parameter.Type.Fullname}?";
			}

			return parameter.Type.Fullname;
		}

		public bool CanBeNullable()
			=> parameter.RefKind is not RefKind.Ref and not RefKind.Out and not RefKind.RefReadOnlyParameter &&
			   parameter.Type.SpecialGenericType is not (SpecialGenericType.Span or SpecialGenericType.ReadOnlySpan);
	}
}
