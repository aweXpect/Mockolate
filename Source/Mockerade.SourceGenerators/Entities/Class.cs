using Mockerade.SourceGenerators.Internals;
using Microsoft.CodeAnalysis;

namespace Mockerade.SourceGenerators.Entities;

internal record Class
{
	private string GetTypeName(ITypeSymbol type, List<string> additionalNamespaces)
	{
		if (type is INamedTypeSymbol namedType)
		{
			if (namedType.IsGenericType)
			{
				additionalNamespaces.AddRange(namedType.TypeArguments
					.Select(t => t.ContainingNamespace.ToString()));
				return namedType.Name + "<" + string.Join(",", namedType.TypeArguments.Select(t => GetTypeName(t, additionalNamespaces))) + ">";
			}
			return namedType.SpecialType switch
			{
				SpecialType.System_Int32 => "int",
				SpecialType.System_Int64 => "long",
				SpecialType.System_Int16 => "short",
				SpecialType.System_UInt32 => "uint",
				SpecialType.System_UInt64 => "ulong",
				SpecialType.System_UInt16 => "ushort",
				_ => type.Name
			};
		}
		else
		{
			return type.Name;
		}
	}
	public static IEnumerable<ITypeSymbol> GetBaseTypesAndThis(ITypeSymbol type)
	{
		var current = type;
		while (current != null)
		{
			yield return current;
			if (current.TypeKind == TypeKind.Interface)
			{
				foreach (var @interface in current.Interfaces)
				{
					yield return @interface;
				}
			}
			current = current.BaseType;
		}
	}

	public Class(ITypeSymbol type)
	{
		List<string> additionalNamespaces = [];
		Namespace = type.ContainingNamespace.ToString();
		ClassName = GetTypeName(type, additionalNamespaces);

		if (type.ContainingType is not null)
		{
			ContainingType = new Type(type.ContainingType);
			ClassName = type.ContainingType.Name + "." + ClassName;
		}

		IsInterface = type.TypeKind == TypeKind.Interface;
		var y = GetBaseTypesAndThis(type).SelectMany(t => t.GetMembers().OfType<IMethodSymbol>())
				// Exclude getter/setter methods
				.Where(x => x.AssociatedSymbol is null && !x.IsSealed)
				.Where(x => IsInterface || x.IsVirtual || x.IsAbstract).ToList();
		var methods = GetBaseTypesAndThis(type).SelectMany(t => t.GetMembers().OfType<IMethodSymbol>())
				// Exclude getter/setter methods
				.Where(x => x.AssociatedSymbol is null && !x.IsSealed)
				.Where(x => IsInterface || x.IsVirtual || x.IsAbstract)
				.Select(x => new Method(x))
				.Distinct()
				.ToList();
		for (int i = 0; i < methods.Count; i++)
		{
			var method = methods[i];
			if (methods.Take(i)
				.Any(m => 
					m.Name == method.Name &&
					m.Parameters.Count == method.Parameters.Count &&
					m.Parameters.SequenceEqual(method.Parameters)))
			{
				methods[i] = method with { ExplicitImplementation = method.ContainingType };
			}
		}
		Methods = new EquatableArray<Method>(methods.ToArray());
		Properties = new EquatableArray<Property>(
			GetBaseTypesAndThis(type).SelectMany(t => t.GetMembers().OfType<IPropertySymbol>())
				.Where(x => !x.IsSealed)
				.Where(x => IsInterface || x.IsVirtual || x.IsAbstract)
				.Select(x => new Property(x))
				.Distinct()
				.ToArray());
		Events = new EquatableArray<Event>(
			GetBaseTypesAndThis(type).SelectMany(t => t.GetMembers().OfType<IEventSymbol>())
				.Where(x => !x.IsSealed)
				.Where(x => IsInterface || x.IsVirtual || x.IsAbstract)
				.Select(x => (x, (x.Type as INamedTypeSymbol)?.DelegateInvokeMethod))
				.Where(x => x.DelegateInvokeMethod is not null)
				.Select(x => new Event(x.x, x.DelegateInvokeMethod!))
				.Distinct()
				.ToArray());
		AdditionalNamespaces = new EquatableArray<string>(additionalNamespaces.Distinct().ToArray());
	}

	public Type? ContainingType { get; }

	public EquatableArray<Method> Methods { get; }
	public EquatableArray<string> AdditionalNamespaces { get; }

	public EquatableArray<Property> Properties { get; }

	public EquatableArray<Event> Events { get; }

	public bool IsInterface { get; }
	public string Namespace { get; }
	public string ClassName { get; }

	public string GetClassNameWithoutDots()
		=> ClassName
		.Replace(".", "")
		.Replace("<", "")
		.Replace(">", "");

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

	internal string GetFullName(string name)
	{
		return $"{Namespace}.{ClassName}.{name}";
	}
}
