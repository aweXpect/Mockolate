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
#pragma warning disable S3267 // Loops should be simplified with "LINQ" expressions
		foreach (string? @namespace in namespaces.OrderByDescending(x => x.Length))
		{
			if (Fullname.StartsWith(@namespace + "."))
			{
				return Fullname.Substring(@namespace.Length + 1);
			}
		}
#pragma warning restore S3267

		return Fullname;
	}
}
