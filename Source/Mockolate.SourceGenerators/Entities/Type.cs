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
		foreach (string? @namespace in namespaces.OrderByDescending(x => x.Length))
		{
			if (Fullname.StartsWith(@namespace + "."))
			{
				return Fullname.Substring(@namespace.Length + 1);
			}
		}

		return Fullname;
	}
}
