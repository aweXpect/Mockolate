using System.Collections.Immutable;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using Microsoft.CodeAnalysis;
using Mockolate.SourceGenerators.Internals;

namespace Mockolate.SourceGenerators.Entities;

[DebuggerDisplay("{DisplayString}")]
internal record Class
{
	private readonly IAssemblySymbol _sourceAssembly;
	private List<Event>? _allEvents;
	private List<Method>? _allMethods;
	private List<Property>? _allProperties;
	private string? _classNameWithoutDots;

	public Class(ITypeSymbol type,
		IAssemblySymbol sourceAssembly,
		List<Method>? alreadyDefinedMethods = null,
		List<Property>? alreadyDefinedProperties = null,
		List<Event>? alreadyDefinedEvents = null,
		List<Method>? exceptMethods = null,
		List<Property>? exceptProperties = null,
		List<Event>? exceptEvents = null)
	{
		_sourceAssembly = sourceAssembly;
		ClassFullName = type.ToDisplayString(Helpers.TypeDisplayFormat);
		ClassName = GetTypeName(type);
		DisplayString = GetTypeFullName(type);

		INamedTypeSymbol? containingType = type.ContainingType;
		if (containingType is not null)
		{
			while (containingType is not null)
			{
				ClassName = containingType.Name + "." + ClassName;
				containingType = containingType.ContainingType;
			}
		}

		IsInterface = type.TypeKind == TypeKind.Interface;
		ImmutableArray<ISymbol> members = type.GetMembers();
		List<Method> methods = ToListExcept(members.OfType<IMethodSymbol>()
			// Exclude getter/setter methods
			.Where(x => x.MethodKind is MethodKind.Ordinary)
			.Where(x => !x.IsSealed)
			.Where(x => IsInterface || x.IsVirtual || x.IsAbstract)
			.Where(ShouldIncludeMember)
			.Select(x => new Method(x, alreadyDefinedMethods, sourceAssembly))
			.Distinct(), exceptMethods, Method.ContainingTypeIndependentEqualityComparer);
		Methods = new EquatableArray<Method>(methods.ToArray());

		List<Property> properties = ToListExcept(members.OfType<IPropertySymbol>()
			.Where(x => !x.IsSealed)
			.Where(x => IsInterface || x.IsVirtual || x.IsAbstract)
			.Where(ShouldIncludeMember)
			.Select(x => new Property(x, alreadyDefinedProperties, sourceAssembly))
			.Distinct(), exceptProperties, Property.ContainingTypeIndependentEqualityComparer);
		Properties = new EquatableArray<Property>(properties.ToArray());

		List<Event> events = ToListExcept(members.OfType<IEventSymbol>()
			.Where(x => !x.IsSealed)
			.Where(x => IsInterface || x.IsVirtual || x.IsAbstract)
			.Where(ShouldIncludeMember)
			.Select(x => (x, x.Type as INamedTypeSymbol))
			.Where(x => x.Item2?.DelegateInvokeMethod is not null)
			.Select(x => new Event(x.x, x.Item2!.DelegateInvokeMethod!, alreadyDefinedEvents, sourceAssembly))
			.Distinct(), exceptEvents, Event.ContainingTypeIndependentEqualityComparer);
		Events = new EquatableArray<Event>(events.ToArray());

		exceptProperties ??= new List<Property>();
		exceptProperties.AddRange(members.OfType<IPropertySymbol>()
			.Where(x => x.IsSealed || HidesBaseOverridable(x, type))
			.Select(x => new Property(x, null, sourceAssembly))
			.Distinct());

		exceptMethods ??= new List<Method>();
		exceptMethods.AddRange(members.OfType<IMethodSymbol>()
			.Where(x => x.MethodKind is MethodKind.Ordinary)
			.Where(x => x.IsSealed || HidesBaseOverridable(x, type))
			.Select(x => new Method(x, null, sourceAssembly))
			.Distinct());

		exceptEvents ??= new List<Event>();
		exceptEvents.AddRange(members.OfType<IEventSymbol>()
			.Where(x => x.IsSealed || HidesBaseOverridable(x, type))
			.Select(x => (x, x.Type as INamedTypeSymbol))
			.Where(x => x.Item2?.DelegateInvokeMethod is not null)
			.Select(x => new Event(x.x, x.Item2!.DelegateInvokeMethod!, null, sourceAssembly))
			.Distinct());

		InheritedTypes = new EquatableArray<Class>(
			GetInheritedTypes(type).Select(t
					=> new Class(t, sourceAssembly, methods, properties, events, exceptMethods, exceptProperties,
						exceptEvents))
				.ToArray());

		bool ShouldIncludeMember(ISymbol member)
		{
			if (IsInterface || member.IsAbstract)
			{
				return true;
			}

			return Helpers.IsOverridableFrom(member, _sourceAssembly);
		}
	}

	public EquatableArray<Method> Methods { get; }
	public EquatableArray<Class> InheritedTypes { get; }
	public EquatableArray<Property> Properties { get; }
	public EquatableArray<Event> Events { get; }

	public bool IsInterface { get; }
	public string ClassFullName { get; }
	public string ClassName { get; }
	public string DisplayString { get; }

	public static IEqualityComparer<Class> EqualityComparer { get; } = new ClassEqualityComparer();

	private string GetTypeName(ITypeSymbol type)
	{
		if (type is INamedTypeSymbol namedType)
		{
			if (namedType.IsGenericType)
			{
				return namedType.Name + "<" + string.Join(",",
					namedType.TypeArguments.Select(GetTypeName)) + ">";
			}

			if (TryExtractSpecialName(namedType, out string? name))
			{
				return name;
			}
		}

		return type.Name;
	}

	private string GetTypeFullName(ITypeSymbol type)
	{
		string GetPrefix(ITypeSymbol s)
		{
			string p = "";
			INamedTypeSymbol? containingType = s.ContainingType;
			while (containingType is not null)
			{
				p = $"{containingType.Name}.{p}";
				containingType = containingType.ContainingType;
			}

			return $"{s.ContainingNamespace}.{p}";
		}

		if (type is INamedTypeSymbol namedType)
		{
			if (namedType.IsGenericType)
			{
				return GetPrefix(namedType) + namedType.Name + "<" + string.Join(",",
					namedType.TypeArguments.Select(t
						=> t.TypeKind == TypeKind.TypeParameter ? t.Name : GetTypeFullName(t))) + ">";
			}

			if (TryExtractSpecialName(namedType, out string? name))
			{
				return name;
			}
		}

		return GetPrefix(type) + type.Name;
	}

	private static bool TryExtractSpecialName(INamedTypeSymbol namedType, [NotNullWhen(true)] out string? specialName)
	{
		(specialName, bool hasSpecialType) = namedType.SpecialType switch
		{
			SpecialType.System_Object => ("object", true),
			SpecialType.System_Boolean => ("bool", true),
			SpecialType.System_String => ("string", true),
			SpecialType.System_Char => ("char", true),
			SpecialType.System_Byte => ("byte", true),
			SpecialType.System_SByte => ("sbyte", true),
			SpecialType.System_Int16 => ("short", true),
			SpecialType.System_UInt16 => ("ushort", true),
			SpecialType.System_Int32 => ("int", true),
			SpecialType.System_UInt32 => ("uint", true),
			SpecialType.System_Int64 => ("long", true),
			SpecialType.System_UInt64 => ("ulong", true),
			SpecialType.System_Single => ("float", true),
			SpecialType.System_Double => ("double", true),
			SpecialType.System_Decimal => ("decimal", true),
			_ => (null, false),
		};
		return hasSpecialType;
	}

	private static List<T> ToListExcept<T>(IEnumerable<T> source, IEnumerable<T>? except, IEqualityComparer<T> comparer)
	{
		if (except is null)
		{
			return source.ToList();
		}

		return source.Except(except, comparer).ToList();
	}

	// True when `member` (declared on `thisType`) hides an overridable member of the same
	// signature on a base class. The hidden base cannot be overridden from a class deriving from
	// `thisType` — the compiler resolves the override target to the hiding member first and fails
	// with CS0506. Overrides are not hiding: they continue the virtual slot.
	private static bool HidesBaseOverridable(ISymbol member, ITypeSymbol thisType)
	{
		if (member is IMethodSymbol { IsOverride: true, } or IPropertySymbol { IsOverride: true, } or IEventSymbol { IsOverride: true, })
		{
			return false;
		}

		for (INamedTypeSymbol? b = thisType.BaseType; b is not null; b = b.BaseType)
		{
			foreach (ISymbol candidate in b.GetMembers(member.Name))
			{
				if (candidate.Kind != member.Kind || candidate.IsStatic != member.IsStatic)
				{
					continue;
				}

				if (!(candidate.IsVirtual || candidate.IsAbstract || candidate.IsOverride) || candidate.IsSealed)
				{
					continue;
				}

				if (SignatureMatches(member, candidate))
				{
					return true;
				}
			}
		}

		return false;
	}

	private static bool SignatureMatches(ISymbol a, ISymbol b)
		=> (a, b) switch
		{
			(IMethodSymbol ma, IMethodSymbol mb) => ParametersMatch(ma.Parameters, mb.Parameters) &&
			                                        ma.TypeParameters.Length == mb.TypeParameters.Length,
			(IPropertySymbol pa, IPropertySymbol pb) => ParametersMatch(pa.Parameters, pb.Parameters),
			(IEventSymbol, IEventSymbol) => true,
			_ => false,
		};

	private static bool ParametersMatch(ImmutableArray<IParameterSymbol> a, ImmutableArray<IParameterSymbol> b)
	{
		if (a.Length != b.Length)
		{
			return false;
		}

		for (int i = 0; i < a.Length; i++)
		{
			if (a[i].RefKind != b[i].RefKind ||
			    !SymbolEqualityComparer.Default.Equals(a[i].Type, b[i].Type))
			{
				return false;
			}
		}

		return true;
	}

#pragma warning disable S3776 // Cognitive Complexity of methods should not be too high
	public static IEnumerable<ITypeSymbol> GetInheritedTypes(ITypeSymbol type)
	{
		ITypeSymbol? current = type;
		while (current != null && current.SpecialType != SpecialType.System_Object)
		{
			if (!SymbolEqualityComparer.Default.Equals(current, type))
			{
				yield return current;
			}

			if (current.TypeKind == TypeKind.Interface)
			{
				foreach (INamedTypeSymbol? @interface in current.AllInterfaces)
				{
					yield return @interface;
				}
			}

			current = current.BaseType;
		}
	}
#pragma warning restore S3776 // Cognitive Complexity of methods should not be too high

	public IEnumerable<Property> AllProperties()
	{
		_allProperties ??= AllClasses().SelectMany(c => c.Properties).Distinct(Property.EqualityComparer).ToList();
		return _allProperties;
	}

	public IEnumerable<Method> AllMethods()
	{
		_allMethods ??= AllClasses().SelectMany(c => c.Methods).Distinct(Method.EqualityComparer).ToList();
		return _allMethods;
	}

	public IEnumerable<Event> AllEvents()
	{
		_allEvents ??= AllClasses().SelectMany(c => c.Events).Distinct(Event.EqualityComparer).ToList();
		return _allEvents;
	}

	private IEnumerable<Class> AllClasses()
	{
		yield return this;
		foreach (Class inherited in InheritedTypes)
		{
			yield return inherited;
		}
	}

	public string GetClassNameWithoutDots()
	{
		if (_classNameWithoutDots is not null)
		{
			return _classNameWithoutDots;
		}

		StringBuilder sb = new(ClassName.Length);
		foreach (char c in ClassName)
		{
			switch (c)
			{
				case ',':
				case '.':
				case '<':
					sb.Append('_');
					break;
				case '>':
				case ' ':
					break;
				default:
					sb.Append(c);
					break;
			}
		}

		return _classNameWithoutDots = sb.ToString();
	}

	private sealed class ClassEqualityComparer : IEqualityComparer<Class>
	{
		public bool Equals(Class? x, Class? y)
			=> (x is null && y is null) ||
			   (x is not null && y is not null &&
			    x.ClassFullName == y.ClassFullName);

		public int GetHashCode(Class obj) => obj.ClassFullName.GetHashCode();
	}
}
