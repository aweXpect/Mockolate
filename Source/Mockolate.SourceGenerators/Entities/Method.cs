using Microsoft.CodeAnalysis;
using Mockolate.SourceGenerators.Internals;

namespace Mockolate.SourceGenerators.Entities;

internal record Method
{
	public Method(IMethodSymbol methodSymbol, List<Method>? alreadyDefinedMethods)
	{
		Accessibility = methodSymbol.DeclaredAccessibility;
		UseOverride = methodSymbol.IsVirtual || methodSymbol.IsAbstract;
		IsAbstract = methodSymbol.IsAbstract;
		ReturnType = methodSymbol.ReturnsVoid ? Type.Void : new Type(methodSymbol.ReturnType);
		Name = methodSymbol.Name;
		ContainingType = methodSymbol.ContainingType.ToDisplayString();
		Parameters = new EquatableArray<MethodParameter>(
			methodSymbol.Parameters.Select(x => new MethodParameter(x)).ToArray());
		if (methodSymbol.IsGenericMethod)
		{
			GenericParameters = new EquatableArray<GenericParameter>(methodSymbol.TypeArguments
				.Select(x => new GenericParameter((ITypeParameterSymbol)x)).ToArray());
			Name += $"<{string.Join(", ", GenericParameters.Value.Select(x => x.Name))}>";
		}

		if (alreadyDefinedMethods is not null)
		{
			if (alreadyDefinedMethods.Any(m =>
				    m.Name == Name &&
				    m.Parameters.Count == Parameters.Count &&
				    m.Parameters.SequenceEqual(Parameters)))
			{
				ExplicitImplementation = ContainingType;
			}

			alreadyDefinedMethods.Add(this);
		}
	}

	public static IEqualityComparer<Method> EqualityComparer { get; } = new MethodEqualityComparer();
	public EquatableArray<GenericParameter>? GenericParameters { get; }

	public bool UseOverride { get; }
	public bool IsAbstract { get; }
	public Accessibility Accessibility { get; }
	public Type ReturnType { get; }
	public string Name { get; }
	public string ContainingType { get; }
	public EquatableArray<MethodParameter> Parameters { get; }
	public string? ExplicitImplementation { get; }

	internal string GetUniqueNameString()
	{
		if (GenericParameters != null)
		{
			string name = Name.Substring(0, Name.IndexOf('<'));
			string parameters = string.Join(", ",
				GenericParameters.Value.Select(genericParameter => $"{{typeof({genericParameter.Name})}}"));
			return $"$\"{ContainingType}.{name}<{parameters}>\"";
		}

		return $"\"{ContainingType}.{Name}\"";
	}

	private sealed class MethodEqualityComparer : IEqualityComparer<Method>
	{
		public bool Equals(Method x, Method y) => x.Name.Equals(y.Name) && x.ContainingType.Equals(y.ContainingType) &&
		                                          x.Parameters.Count == y.Parameters.Count &&
		                                          x.Parameters.SequenceEqual(y.Parameters);

		public int GetHashCode(Method obj) => obj.Name.GetHashCode();
	}
}
