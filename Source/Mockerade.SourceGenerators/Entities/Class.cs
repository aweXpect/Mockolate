using Mockerade.SourceGenerators.Internals;
using Microsoft.CodeAnalysis;

namespace Mockerade.SourceGenerators.Entities;

internal record Class
{
	public Class(ITypeSymbol type)
	{
		Namespace = type.ContainingNamespace.ToString();
		ClassName = type.Name;

		IsInterface = type.TypeKind == TypeKind.Interface;
		Methods = new EquatableArray<Method>(
			type.GetMembers().OfType<IMethodSymbol>()
				// Exclude getter/setter methods
				.Where(x => x.AssociatedSymbol is null)
				.Where(x => IsInterface || x.IsVirtual)
				.Select(x => new Method(x))
				.ToArray());
		Properties = new EquatableArray<Property>(
			type.GetMembers().OfType<IPropertySymbol>()
				.Where(x => IsInterface || x.IsVirtual)
				.Select(x => new Property(x))
				.ToArray());
		Events = new EquatableArray<Event>(
			type.GetMembers().OfType<IEventSymbol>()
				.Where(x => IsInterface || x.IsVirtual)
				.Select(x => (x, (x.Type as INamedTypeSymbol)?.DelegateInvokeMethod))
				.Where(x => x.DelegateInvokeMethod is not null)
				.Select(x => new Event(x.x, x.DelegateInvokeMethod!))
				.ToArray());
	}

	public EquatableArray<Method> Methods { get; }

	public EquatableArray<Property> Properties { get; }

	public EquatableArray<Event> Events { get; }

	public bool IsInterface { get; }
	public string Namespace { get; }
	public string ClassName { get; }

	public string[] GetClassNamespaces() => EnumerateNamespaces().Distinct().OrderBy(n => n).ToArray();
	internal IEnumerable<string> EnumerateNamespaces()
	{
		yield return Namespace;
		foreach (Method method in Methods)
		{
			if (method.ReturnType.Namespace != null)
			{
				yield return method.ReturnType.Namespace;
			}

			foreach (string? @namespace in method.Parameters
				         .Select(parameter => parameter.Type.Namespace)
				         .Where(n => n is not null))
			{
				yield return @namespace!;
			}
		}
		foreach (Property property in Properties)
		{
			if (property.Type.Namespace != null)
			{
				yield return property.Type.Namespace;
			}
		}
		foreach (Event @event in Events)
		{
			if (@event.Type.Namespace != null)
			{
				yield return @event.Type.Namespace;
			}
		}
	}
}
