using Microsoft.CodeAnalysis;
using Mockolate.SourceGenerators.Internals;

namespace Mockolate.SourceGenerators.Entities;

internal readonly record struct Type
{
	private Type(string fullname)
	{
		Fullname = fullname;
	}

	internal Type(ITypeSymbol typeSymbol)
	{
		// Removes '*' from multi-dimensional array types
		Fullname = typeSymbol.ToDisplayString().Replace("*", "");
		Namespace = typeSymbol.ContainingNamespace?.ToString();
		IsArray = typeSymbol.TypeKind == TypeKind.Array;
		if (typeSymbol is INamedTypeSymbol namedTypeSymbol &&
			typeSymbol.IsTupleType)
		{
			TupleTypes = new EquatableArray<Type>(namedTypeSymbol.TupleElements
				.Select(x => new Type(x.Type))
				.ToArray());
		}
	}

	public bool IsArray { get; }

	public EquatableArray<Type>? TupleTypes { get; }

	public string? Namespace { get; }

	internal static Type Void { get; } = new("void");

	public string Fullname { get; }

	public override string ToString() => Fullname;
}
