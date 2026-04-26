using System.Collections.Immutable;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
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

	public static SymbolDisplayFormat TypeDisplayShortFormat { get; } = new(
		miscellaneousOptions: SymbolDisplayMiscellaneousOptions.EscapeKeywordIdentifiers |
		                      SymbolDisplayMiscellaneousOptions.UseSpecialTypes |
		                      SymbolDisplayMiscellaneousOptions.IncludeNullableReferenceTypeModifier,
		globalNamespaceStyle: SymbolDisplayGlobalNamespaceStyle.Omitted,
		typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameAndContainingTypes,
		genericsOptions: SymbolDisplayGenericsOptions.IncludeTypeParameters);

	public static string EscapeIfKeyword(string name)
		=> SyntaxFacts.GetKeywordKind(name) != SyntaxKind.None ? "@" + name : name;

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

	/// <summary>
	///     Generates a unique base name such that <c>{base}1</c>..<c>{base}N</c> do not conflict with
	///     any parameter names. Used for numerically suffixed cast variables like <c>outParam1</c>.
	/// </summary>
	public static string GetUniqueIndexedLocalVariableBase(string baseName, EquatableArray<MethodParameter> parameters)
	{
		string candidateName = baseName;
		int suffix = 0;

		while (parameters.Any(p => HasIndexedConflict(candidateName, p.Name, parameters.Count)))
		{
			suffix++;
			candidateName = $"{baseName}_{suffix}";
		}

		return candidateName;

		static bool HasIndexedConflict(string @base, string parameterName, int count)
		{
			if (!parameterName.StartsWith(@base, StringComparison.Ordinal))
			{
				return false;
			}

			string tail = parameterName.Substring(@base.Length);
			return tail.Length > 0 && int.TryParse(tail, out int index) && index >= 1 && index <= count;
		}
	}

	// A member (or accessor) declared in another assembly is overridable only if the overriding
	// assembly can actually see it. `internal` and `private protected` are invisible across assembly
	// boundaries unless the declaring assembly grants InternalsVisibleTo. `protected internal`
	// (= protected OR internal) is always reachable via the protected half from a derived class.
	public static bool IsOverridableFrom(ISymbol member, IAssemblySymbol? sourceAssembly)
	{
		if (sourceAssembly is null ||
		    member.DeclaredAccessibility is not (Accessibility.Internal or Accessibility.ProtectedAndInternal))
		{
			return true;
		}

		IAssemblySymbol containingAssembly = member.ContainingAssembly;
		if (SymbolEqualityComparer.Default.Equals(containingAssembly, sourceAssembly))
		{
			return true;
		}

		return containingAssembly.GivesAccessTo(sourceAssembly);
	}

	public static string ToTypeOrWrapper(this Type type)
	{
		if (type.SpecialGenericType == SpecialGenericType.Span)
		{
			return $"global::Mockolate.Setup.SpanWrapper<{type.GenericTypeParameters!.Value.First().Fullname}>";
		}

		if (type.SpecialGenericType == SpecialGenericType.ReadOnlySpan)
		{
			return $"global::Mockolate.Setup.ReadOnlySpanWrapper<{type.GenericTypeParameters!.Value.First().Fullname}>";
		}

		return type.Fullname;
	}

	/// <summary>
	///     Returns true if the given type needs the ref-struct setup pipeline: it is a ref-like
	///     type AND the existing Span/ReadOnlySpan wrapper fallback doesn't apply.
	/// </summary>
	/// <remarks>
	///     <c>System.Span&lt;T&gt;</c> and <c>System.ReadOnlySpan&lt;T&gt;</c> are themselves
	///     ref-like, but the generator already boxes them into <c>SpanWrapper&lt;T&gt;</c> /
	///     <c>ReadOnlySpanWrapper&lt;T&gt;</c> (a plain class), so their setup flows through the
	///     regular <c>VoidMethodSetup</c> hierarchy with a non-ref-struct <c>T</c>. Only types
	///     outside that wrapping need the <c>RefStructVoidMethodSetup</c> /
	///     <c>RefStructReturnMethodSetup</c> / <c>RefStructIndexerGetterSetup</c> path.
	/// </remarks>
	public static bool NeedsRefStructPipeline(this Type type)
		=> type.IsRefStruct
		   && type.SpecialGenericType is not (SpecialGenericType.Span or SpecialGenericType.ReadOnlySpan);

	public static bool NeedsRefStructPipeline(this MethodParameter parameter)
		=> parameter.Type.NeedsRefStructPipeline();

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
			if (!@class.HasReservedName(name))
			{
				return name;
			}

			string? alternative = alternatives.FirstOrDefault(a => !@class.HasReservedName(a));
			if (alternative is not null)
			{
				return alternative;
			}

			int index = 1;
			while (@class.HasReservedName($"{name}_{index}"))
			{
				index++;
			}

			return $"{name}_{index}";
		}

		public bool HasReservedName(string candidate)
			=> @class.AllProperties().Any(p => p.Name == candidate) ||
			   @class.AllMethods().Any(m => m.Name == candidate) ||
			   @class.AllEvents().Any(e => e.Name == candidate) ||
			   @class.ReservedNames.Any(r => r == candidate);
	}

	extension(ImmutableArray<AttributeData> attributes)
	{
		public EquatableArray<Attribute>? ToAttributeArray(IAssemblySymbol? sourceAssembly = null)
		{
			Attribute[] consideredAttributes = attributes
				.Where(x => x.AttributeClass is not null
				            && !IsCompilerEmittedAttribute(x.AttributeClass)
				            && IsAccessibleFrom(x.AttributeClass, sourceAssembly))
				.Select(attr => new Attribute(attr))
				.ToArray();
			if (consideredAttributes.Length > 0)
			{
				return new EquatableArray<Attribute>(consideredAttributes);
			}

			return null;

			// Compiler-emitted attributes describe the shape of the original method body (nullability,
			// iterator/async state machine types) and MUST NOT be copied onto the generated override.
			// The state-machine attributes reference private nested compiler-generated types that are
			// invisible outside the declaring assembly (CS0103), and the override has its own,
			// synchronous body that is not a state machine anyway.
			static bool IsCompilerEmittedAttribute(INamedTypeSymbol attribute)
			{
				if (attribute.ContainingNamespace is not { Name: "CompilerServices", ContainingNamespace: { Name: "Runtime", ContainingNamespace: { Name: "System", ContainingNamespace.IsGlobalNamespace: true, }, }, })
				{
					return false;
				}

				return attribute.Name is "NullableContextAttribute" or "NullableAttribute"
					or "AsyncStateMachineAttribute" or "IteratorStateMachineAttribute"
					or "AsyncIteratorStateMachineAttribute";
			}

			// The attribute name is emitted verbatim into the generated code (e.g.
			// `[global::Azure.Core.CallerShouldAudit(...)]`). If the attribute class — or any of its
			// containing types — is not visible to the generated mock assembly, referencing it causes
			// CS0122. Drop the attribute instead of producing uncompilable output.
			//
			// Conservative rule: a type is accessible only if its whole containing chain is either
			// Public, or Internal/ProtectedOrInternal with InternalsVisibleTo granted (or the
			// same assembly). Private/Protected/ProtectedAndInternal nested types and
			// ProtectedOrInternal across assemblies without IVT are treated as inaccessible — the
			// `protected` half would require knowing the derivation relationship to the declaring
			// type, which we don't verify here.
			static bool IsAccessibleFrom(INamedTypeSymbol attribute, IAssemblySymbol? sourceAssembly)
			{
				for (INamedTypeSymbol? t = attribute; t is not null; t = t.ContainingType)
				{
					switch (t.DeclaredAccessibility)
					{
						case Accessibility.Public:
							continue;
						case Accessibility.Internal:
						case Accessibility.ProtectedOrInternal:
							if (sourceAssembly is null ||
							    SymbolEqualityComparer.Default.Equals(t.ContainingAssembly, sourceAssembly) ||
							    t.ContainingAssembly.GivesAccessTo(sourceAssembly))
							{
								continue;
							}

							return false;
						default:
							return false;
					}
				}

				return true;
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
					sb.Append(", ").Append("() => ")
						.AppendDefaultValueGeneratorFor(genericType, defaultValueName);
				}

				if (!string.IsNullOrWhiteSpace(suffix))
				{
					sb.Append(", ").Append(suffix);
				}

				sb.Append(")");
				return sb;
			}

			if (type.SpecialGenericType != SpecialGenericType.None && type.GenericTypeParameters?.Count > 0)
			{
				foreach (Type? genericType in type.GenericTypeParameters.Value)
				{
					sb.Append(", ").Append("() => ")
						.AppendDefaultValueGeneratorFor(genericType, defaultValueName);
				}
			}

			if (!string.IsNullOrWhiteSpace(suffix))
			{
				sb.Append(", ").Append(suffix);
			}

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

		public string ToTypeOrWrapper()
		{
			if (parameter.Type.SpecialGenericType == SpecialGenericType.Span)
			{
				return $"global::Mockolate.Setup.SpanWrapper<{parameter.Type.GenericTypeParameters!.Value.First().Fullname}>";
			}

			if (parameter.Type.SpecialGenericType == SpecialGenericType.ReadOnlySpan)
			{
				return
					$"global::Mockolate.Setup.ReadOnlySpanWrapper<{parameter.Type.GenericTypeParameters!.Value.First().Fullname}>";
			}

			return parameter.Type.Fullname;
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

		/// <summary>
		///     Returns true if the parameter can be used as an explicit value in a T overload.
		///     Ambiguity with the <c>IParameter&lt;T&gt;?</c> overload when <c>null</c> is passed is
		///     resolved via the OverloadResolutionPriorityAttribute.
		/// </summary>
		public bool CanUseNullableParameterOverload()
			=> parameter.RefKind is not RefKind.Ref and not RefKind.Out and not RefKind.RefReadOnlyParameter &&
			   parameter.Type.SpecialGenericType is not (SpecialGenericType.Span or SpecialGenericType.ReadOnlySpan);
	}
}
