using Microsoft.CodeAnalysis;
using Mockerade.SourceGenerators.Internals;

namespace Mockerade.SourceGenerators.Entities;

internal record MockClass : Class
{
	public MockClass(ITypeSymbol[] types) : base(types[0])
	{
		AdditionalImplementations = new EquatableArray<Class>(
			types.Skip(1).Select(x => new Class(x)).ToArray());

		if (!IsInterface && types[0] is INamedTypeSymbol namedTypeSymbol)
		{
			Constructors = new EquatableArray<Method>(namedTypeSymbol.Constructors.Select(x => new Method(x)).ToArray());
		}
	}

	public EquatableArray<Method>? Constructors { get; }

	public EquatableArray<Class> AdditionalImplementations { get; }

	public string[] GetAllNamespaces() => EnumerateAllNamespaces().Distinct().OrderBy(n => n).ToArray();

	private IEnumerable<string> EnumerateAllNamespaces()
	{
		foreach (string? @namespace in EnumerateNamespaces())
		{
			yield return @namespace;
		}

		foreach (Class? implementation in AdditionalImplementations)
		{
			foreach (string? @namespace in implementation.EnumerateNamespaces())
			{
				yield return @namespace;
			}
		}
	}
}
