using System.Collections.Immutable;
using System.Diagnostics;
using System.Text;
using Microsoft.CodeAnalysis;

namespace Mockolate.SourceGenerators.Entities;

[DebuggerDisplay("{DisplayString}")]
internal class Class : IEquatable<Class>
{
	private readonly IAssemblySymbol _sourceAssembly;
	private readonly int _surfaceHash;
	private List<Event>? _allEvents;
	private List<Method>? _allMethods;
	private List<Property>? _allProperties;
	private string? _classNameWithoutDots;

#pragma warning disable S107 // Methods should not have too many parameters
	public Class(ITypeSymbol type,
		IAssemblySymbol sourceAssembly,
		List<Method>? alreadyDefinedMethods = null,
		List<Property>? alreadyDefinedProperties = null,
		List<Event>? alreadyDefinedEvents = null,
		List<Method>? exceptMethods = null,
		List<Property>? exceptProperties = null,
		List<Event>? exceptEvents = null)
#pragma warning restore S107
	{
		_sourceAssembly = sourceAssembly;
		ClassFullName = type.ToDisplayString(Helpers.TypeDisplayFormat);
		ClassName = GetTypeName(type);
		DisplayString = GetTypeFullName(type);

		INamedTypeSymbol? containingType = type.ContainingType;
		if (containingType is not null)
		{
			List<string> ancestors = new();
			while (containingType is not null)
			{
				ancestors.Add(containingType.Name);
				containingType = containingType.ContainingType;
			}

			StringBuilder nameBuilder = new();
			for (int i = ancestors.Count - 1; i >= 0; i--)
			{
				nameBuilder.Append(ancestors[i]).Append('.');
			}

			nameBuilder.Append(ClassName);
			ClassName = nameBuilder.ToString();
		}

		IsInterface = type.TypeKind == TypeKind.Interface;
		HasRequiredMembers = ComputeHasRequiredMembers(type);
		ImmutableArray<ISymbol> members = type.GetMembers();

		List<Method> methodIncludes = new();
		List<Property> propertyIncludes = new();
		List<Event> eventIncludes = new();
		List<Method> methodExceptCandidates = new();
		List<Property> propertyExceptCandidates = new();
		List<Event> eventExceptCandidates = new();

		foreach (ISymbol member in members)
		{
			switch (member)
			{
				case IMethodSymbol methodSymbol when methodSymbol.MethodKind is MethodKind.Ordinary:
					{
						if (!methodSymbol.IsSealed && (IsInterface || methodSymbol.IsVirtual || methodSymbol.IsAbstract) &&
						    ShouldIncludeMember(methodSymbol))
						{
							methodIncludes.Add(new Method(methodSymbol, alreadyDefinedMethods, sourceAssembly));
						}

						if (methodSymbol.IsSealed || HidesBaseOverridable(methodSymbol, type))
						{
							methodExceptCandidates.Add(new Method(methodSymbol, null, sourceAssembly));
						}

						break;
					}

				case IPropertySymbol propertySymbol:
					{
						if (!propertySymbol.IsSealed && (IsInterface || propertySymbol.IsVirtual || propertySymbol.IsAbstract) &&
						    ShouldIncludeMember(propertySymbol))
						{
							propertyIncludes.Add(new Property(propertySymbol, alreadyDefinedProperties, sourceAssembly));
						}

						if (propertySymbol.IsSealed || HidesBaseOverridable(propertySymbol, type))
						{
							propertyExceptCandidates.Add(new Property(propertySymbol, null, sourceAssembly));
						}

						break;
					}

				case IEventSymbol eventSymbol:
					{
						IMethodSymbol? invoke = (eventSymbol.Type as INamedTypeSymbol)?.DelegateInvokeMethod;
						if (invoke is null)
						{
							break;
						}

						if (!eventSymbol.IsSealed && (IsInterface || eventSymbol.IsVirtual || eventSymbol.IsAbstract) &&
						    ShouldIncludeMember(eventSymbol))
						{
							eventIncludes.Add(new Event(eventSymbol, invoke, alreadyDefinedEvents, sourceAssembly));
						}

						if (eventSymbol.IsSealed || HidesBaseOverridable(eventSymbol, type))
						{
							eventExceptCandidates.Add(new Event(eventSymbol, invoke, null, sourceAssembly));
						}

						break;
					}
			}
		}

		List<Method> methods = ToListExcept(DistinctList(methodIncludes), exceptMethods, Method.ContainingTypeIndependentEqualityComparer);
		Methods = new EquatableArray<Method>(methods.ToArray());

		List<Property> properties = ToListExcept(DistinctList(propertyIncludes), exceptProperties, Property.ContainingTypeIndependentEqualityComparer);
		Properties = new EquatableArray<Property>(properties.ToArray());

		List<Event> events = ToListExcept(DistinctList(eventIncludes), exceptEvents, Event.ContainingTypeIndependentEqualityComparer);
		Events = new EquatableArray<Event>(events.ToArray());

		exceptProperties ??= new List<Property>();
		exceptProperties.AddRange(DistinctList(propertyExceptCandidates));

		exceptMethods ??= new List<Method>();
		exceptMethods.AddRange(DistinctList(methodExceptCandidates));

		exceptEvents ??= new List<Event>();
		exceptEvents.AddRange(DistinctList(eventExceptCandidates));

		InheritedTypes = new EquatableArray<Class>(
			GetInheritedTypes(type).Select(t
					=> new Class(t, sourceAssembly, methods, properties, events, exceptMethods, exceptProperties,
						exceptEvents))
				.ToArray());

		ReservedNames = ComputeReservedNames(type);

		_surfaceHash = ComputeSurfaceHash();

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
	public EquatableArray<string> ReservedNames { get; }

	public bool IsInterface { get; }
	public bool HasRequiredMembers { get; }
	public string ClassFullName { get; }
	public string ClassName { get; }
	public string DisplayString { get; }

	/// <summary>
	///     Folds the member surface (methods, properties, events, recursive base/interface chain,
	///     reserved names, kind, required-member flag) into a single content-derived integer.
	///     Roslyn's incremental cache uses <see cref="Equals(Class?)" /> to decide whether a
	///     downstream stage's input changed; if equality only checked the type's name, an edit
	///     that altered the member surface but not the name would let stale generated source
	///     persist. Folding members into a hash keeps the comparison O(1) on the cache hot path
	///     while still invalidating on any surface change.
	/// </summary>
	private int ComputeSurfaceHash()
	{
		int hash = ClassFullName.GetHashCode();
		hash = unchecked((hash * 17) + Methods.GetHashCode());
		hash = unchecked((hash * 17) + Properties.GetHashCode());
		hash = unchecked((hash * 17) + Events.GetHashCode());
		hash = unchecked((hash * 17) + InheritedTypes.GetHashCode());
		hash = unchecked((hash * 17) + ReservedNames.GetHashCode());
		hash = unchecked((hash * 17) + (IsInterface ? 1 : 0));
		hash = unchecked((hash * 17) + (HasRequiredMembers ? 1 : 0));
		return hash;
	}

	/// <summary>
	///     Identifiers that the mock class shares its scope with but that aren't surfaced through
	///     Methods/Properties/Events: generic type parameters of the type itself, nested types, and
	///     fields declared on the type. A generated member colliding with any of these would either
	///     fail to compile (CS0102 / type-parameter shadowing) or hide an inherited field (CS0108).
	/// </summary>
	private static EquatableArray<string> ComputeReservedNames(ITypeSymbol type)
	{
		HashSet<string> names = new();
		if (type is INamedTypeSymbol namedType)
		{
			foreach (ITypeParameterSymbol typeParameter in namedType.TypeParameters)
			{
				names.Add(typeParameter.Name);
			}
		}

		foreach (INamedTypeSymbol nested in type.GetTypeMembers())
		{
			names.Add(nested.Name);
		}

		foreach (IFieldSymbol field in type.GetMembers().OfType<IFieldSymbol>())
		{
			if (field.IsImplicitlyDeclared)
			{
				continue;
			}

			names.Add(field.Name);
		}

		return new EquatableArray<string>(names.ToArray());
	}

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
				return name!;
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
				return name!;
			}
		}

		return GetPrefix(type) + type.Name;
	}

	private static bool TryExtractSpecialName(INamedTypeSymbol namedType, out string? specialName)
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

	/// <summary>
	///     In-place deduplication of the <paramref name="list" /> that preserves insertion order and uses default equality.
	/// </summary>
	private static List<T> DistinctList<T>(List<T> list) where T : notnull
	{
		if (list.Count <= 1)
		{
			return list;
		}

		HashSet<T> seen = new();
		List<T> result = new(list.Count);
		foreach (T item in list)
		{
			if (seen.Add(item))
			{
				result.Add(item);
			}
		}

		return result;
	}

	/// <summary>
	///     True when `member` (declared on `thisType`) hides an overridable member of the same
	///     signature on a base class. The hidden base cannot be overridden from a class deriving from
	///     `thisType` — the compiler resolves the override target to the hiding member first and fails
	///     with CS0506.<br />
	///     Overrides are not hiding: they continue the virtual slot.
	/// </summary>
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

	private static bool ComputeHasRequiredMembers(ITypeSymbol type)
	{
		for (ITypeSymbol? current = type;
		     current is not null && current.SpecialType != SpecialType.System_Object;
		     current = current.BaseType)
		{
			foreach (ISymbol member in current.GetMembers())
			{
				if (member is IPropertySymbol { IsRequired: true, } or IFieldSymbol { IsRequired: true, })
				{
					return true;
				}
			}
		}

		return false;
	}

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

	/// <summary>
	///     Equality is keyed on <see cref="ClassFullName" /> plus a content-derived hash of the
	///     member surface (<see cref="ComputeSurfaceHash" />). The full name alone is necessary
	///     but not sufficient as a Roslyn incremental cache key: across edits, a target type can
	///     keep its name while its members change, and a name-only comparison would let Roslyn
	///     skip downstream stages and persist stale generated source. Folding the surface into a
	///     precomputed hash keeps the comparison O(1) on the cache hot path while still
	///     invalidating on any change to the emitted member set. Hash collisions are theoretically
	///     possible but the leaf entities (<see cref="Method" />, <see cref="Property" />, and
	///     <see cref="Event" />) are records with content-based hashes that propagate through
	///     <see cref="EquatableArray{T}" />, so different surfaces almost always hash apart.
	/// </summary>
	public virtual bool Equals(Class? other)
		=> ReferenceEquals(this, other) ||
		   (other is not null &&
		    GetType() == other.GetType() &&
		    _surfaceHash == other._surfaceHash &&
		    ClassFullName == other.ClassFullName);

	public override bool Equals(object? obj) => Equals(obj as Class);

	public override int GetHashCode() => _surfaceHash;

	public static bool operator ==(Class? left, Class? right)
	{
		return left?.Equals(right) ?? right is null;
	}

	public static bool operator !=(Class? left, Class? right)
	{
		return !(left == right);
	}
}
