using Mockolate.SourceGenerators.Internals;
using Microsoft.CodeAnalysis;

namespace Mockolate.SourceGenerators.Entities;

internal record struct Method
{
	public Method(IMethodSymbol methodSymbol)
	{
		Accessibility = methodSymbol.DeclaredAccessibility;
		UseOverride = methodSymbol.IsVirtual || methodSymbol.IsAbstract;
		ReturnType = methodSymbol.ReturnsVoid ? Type.Void : new Type(methodSymbol.ReturnType);
		Name = methodSymbol.Name;
		ContainingType = methodSymbol.ContainingType.Name;
		Parameters = new EquatableArray<MethodParameter>(
			methodSymbol.Parameters.Select(x => new MethodParameter(x)).ToArray());
	}

	public bool UseOverride { get; }

	public Accessibility Accessibility { get; }
	public Type ReturnType { get; }
	public string Name { get; }
	public string ContainingType { get; }
	public EquatableArray<MethodParameter> Parameters { get; }
	public string? ExplicitImplementation { get; set; }
}
