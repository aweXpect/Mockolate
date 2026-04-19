using Microsoft.CodeAnalysis;
using Mockolate.SourceGenerators.Internals;

namespace Mockolate.SourceGenerators.Entities;

internal record Type
{
	private Type(string fullname)
	{
		Fullname = fullname;
		DisplayName = fullname;
	}

	internal Type(ITypeSymbol typeSymbol)
	{
		// Removes '*' from multi-dimensional array types
		Fullname = typeSymbol.ToDisplayString(Helpers.TypeDisplayFormat).Replace("*", "");
		DisplayName = typeSymbol.ToDisplayString(Helpers.TypeDisplayShortFormat).Replace("*", "");
		Namespace = typeSymbol.ContainingNamespace?.ToString();
		if (typeSymbol is INamedTypeSymbol namedTypeSymbol)
		{
			if (typeSymbol.IsTupleType)
			{
				TupleTypes = new EquatableArray<Type>(namedTypeSymbol.TupleElements
					.Select(x => new Type(x.Type))
					.ToArray());
			}

			GenericTypeParameters = new EquatableArray<Type>(namedTypeSymbol.TypeArguments
				.Select(x => new Type(x))
				.ToArray());
		}

		SpecialGenericType = typeSymbol.GetSpecialType();
		SpecialType = typeSymbol.SpecialType;
		CanBeNullable = typeSymbol.NullableAnnotation == NullableAnnotation.Annotated ||
		                typeSymbol is INamedTypeSymbol
		                {
			                OriginalDefinition.SpecialType: SpecialType.System_Nullable_T,
		                };
		IsFormattable = IsIFormattable(typeSymbol);
		// IsRefLikeType is true for ref structs (e.g. Span<T>, ReadOnlySpan<T>, Utf8JsonReader,
		// and any user-defined `ref struct`). Used by the generator to switch to the ref-struct-safe
		// setup pipeline for parameters that cannot flow through the regular IParameter<T> path
		// on TFMs predating C# 13's `allows ref struct` anti-constraint.
		IsRefStruct = typeSymbol.IsRefLikeType;
	}

	public bool IsFormattable { get; }
	public bool CanBeNullable { get; }
	public bool IsRefStruct { get; }
	public SpecialType SpecialType { get; }
	public SpecialGenericType SpecialGenericType { get; }
	public EquatableArray<Type>? TupleTypes { get; }
	public EquatableArray<Type>? GenericTypeParameters { get; }
	public string? Namespace { get; }

	internal static Type Void { get; } = new("void");

	public string Fullname { get; }

	public string DisplayName { get; }

	private static bool IsIFormattable(ITypeSymbol typeSymbol)
	{
		// Unwrap Nullable<T>: check if the underlying type implements IFormattable
		if (typeSymbol is INamedTypeSymbol { OriginalDefinition.SpecialType: SpecialType.System_Nullable_T, } nullable)
		{
			typeSymbol = nullable.TypeArguments[0];
		}

		// The type itself may be System.IFormattable (AllInterfaces excludes self)
		if (typeSymbol is INamedTypeSymbol named &&
		    named.ContainingNamespace?.ContainingNamespace?.IsGlobalNamespace == true &&
		    named.ContainingNamespace.Name == "System" &&
		    named.Name == "IFormattable")
		{
			return true;
		}

		return typeSymbol.AllInterfaces.Any(i => i.ContainingNamespace?.ContainingNamespace?.IsGlobalNamespace == true &&
		                                         i.ContainingNamespace.Name == "System" &&
		                                         i.Name == "IFormattable");
	}

	public override string ToString() => Fullname;
}
