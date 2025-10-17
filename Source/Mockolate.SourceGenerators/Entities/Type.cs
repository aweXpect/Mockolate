using Microsoft.CodeAnalysis;

namespace Mockolate.SourceGenerators.Entities;

internal readonly record struct Type
{
	private Type(string fullname)
	{
		Fullname = fullname;
	}

	internal Type(ITypeSymbol typeSymbol)
	{
		Fullname = typeSymbol.ToDisplayString();
		Namespace = typeSymbol.ContainingNamespace?.ToString();
	}

	public string? Namespace { get; }

	internal static Type Void { get; } = new("void");

	public string Fullname { get; }

	public override string ToString() => Fullname;

	public string GetMinimizedString(string[] namespaces)
	{
		return Fullname;
	}
}
