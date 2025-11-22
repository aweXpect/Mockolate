using System.Text;
using Microsoft.CodeAnalysis;
using Mockolate.SourceGenerators.Entities;
using Type = Mockolate.SourceGenerators.Entities.Type;

namespace Mockolate.SourceGenerators.Sources;

#pragma warning disable S3776 // Cognitive Complexity of methods should not be too high
internal static partial class Sources
{
	public static string ForMock(string name, MockClass mockClass)
	{
		StringBuilder sb = InitializeBuilder([
			"System.Diagnostics",
			"Mockolate.Setup",
		]);

		sb.Append("""
		          namespace Mockolate.Generated;

		          #nullable enable annotations

		          """);
		if (mockClass.Delegate is not null)
		{
			sb.Append("/// <summary>").AppendLine();
			sb.Append("///     A mock implementing <see cref=\"")
				.Append(mockClass.ClassFullName.EscapeForXmlDoc())
				.Append("\" />");
			foreach (Class? additional in mockClass.DistinctAdditionalImplementations())
			{
				sb.Append(" and <see cref=\"").Append(additional.ClassFullName.EscapeForXmlDoc()).Append("\" />");
			}

			sb.AppendLine(".");
			sb.Append("/// </summary>").AppendLine();
			sb.Append("internal class MockFor").Append(name).Append(" : IMockSubject<").Append(mockClass.ClassFullName)
				.Append(">").AppendLine();
			sb.Append("{").AppendLine();
			sb.Append("\t/// <inheritdoc cref=\"IMockSubject{T}.Mock\" />").AppendLine();
			sb.Append("\tMock<").Append(mockClass.ClassFullName).Append("> IMockSubject<")
				.Append(mockClass.ClassFullName).Append(">.Mock => _mock;").AppendLine();
			sb.Append("\tprivate readonly Mock<").Append(mockClass.ClassFullName).Append("> _mock;").AppendLine();
			sb.Append("\t/// <inheritdoc cref=\"IHasMockRegistration.Registrations\" />").AppendLine();
			sb.Append("\t[DebuggerBrowsable(DebuggerBrowsableState.Never)]").AppendLine();
			sb.Append("\tMockRegistration IHasMockRegistration.Registrations => _mock.Registrations;").AppendLine();

			sb.Append("\t/// <inheritdoc cref=\"MockFor").Append(name).Append("\" />").AppendLine();
			sb.Append("\tpublic MockFor").Append(name).Append("(MockBehavior mockBehavior)").AppendLine();
			sb.Append("\t{").AppendLine();
			sb.Append("\t\t_mock = new Mock<").Append(mockClass.ClassFullName)
				.Append(">(Invoke, new MockRegistration(mockBehavior, \"").Append(mockClass.DisplayString)
				.Append("\"));").AppendLine();
			sb.Append("\t}").AppendLine();
			sb.AppendLine();
			sb.Append("\tpublic ").Append(mockClass.ClassFullName).Append(" Object => new(Invoke);").AppendLine();
			sb.Append("\tprivate ")
				.Append(mockClass.Delegate.ReturnType == Type.Void
					? "void"
					: mockClass.Delegate.ReturnType.Fullname)
				.Append(" Invoke(")
				.Append(string.Join(", ",
					mockClass.Delegate.Parameters.Select(p => $"{p.RefKind.GetString()}{p.Type.Fullname} {p.Name}")))
				.Append(')').AppendLine();
			sb.Append("\t{").AppendLine();
			if (mockClass.Delegate.ReturnType != Type.Void)
			{
				sb.Append("\t\tvar result = _mock.Registrations.InvokeMethod<")
					.Append(mockClass.Delegate.ReturnType.Fullname)
					.Append(">(").Append(mockClass.Delegate.GetUniqueNameString());
				foreach (MethodParameter p in mockClass.Delegate.Parameters)
				{
					sb.Append(", ").Append(p.RefKind == RefKind.Out ? "null" : p.Name);
				}

				sb.AppendLine(");");
			}
			else
			{
				sb.Append("\t\tvar result = _mock.Registrations.InvokeMethod(")
					.Append(mockClass.Delegate.GetUniqueNameString());
				foreach (MethodParameter p in mockClass.Delegate.Parameters)
				{
					sb.Append(", ").Append(p.RefKind == RefKind.Out ? "null" : p.Name);
				}

				sb.AppendLine(");");
			}

			foreach (MethodParameter parameter in mockClass.Delegate.Parameters)
			{
				if (parameter.RefKind == RefKind.Out)
				{
					sb.Append("\t\t").Append(parameter.Name).Append(" = result.SetOutParameter<")
						.Append(parameter.Type.Fullname).Append(">(\"").Append(parameter.Name)
						.Append("\");").AppendLine();
				}
				else if (parameter.RefKind == RefKind.Ref)
				{
					sb.Append("\t\t").Append(parameter.Name).Append(" = result.SetRefParameter<")
						.Append(parameter.Type.Fullname).Append(">(\"").Append(parameter.Name)
						.Append("\", ").Append(parameter.Name).Append(");").AppendLine();
				}
			}

			sb.Append("\t\tresult.TriggerCallbacks(")
				.Append(string.Join(", ", mockClass.Delegate.Parameters.Select(p => p.Name)))
				.Append(");").AppendLine();
			if (mockClass.Delegate.ReturnType != Type.Void)
			{
				sb.Append("\t\treturn result.Result;").AppendLine();
			}

			sb.Append("\t}").AppendLine();
			sb.Append("}").AppendLine();
		}
		else
		{
			sb.Append("/// <summary>").AppendLine();
			sb.Append("///     A mock implementing <see cref=\"")
				.Append(mockClass.ClassFullName.EscapeForXmlDoc())
				.Append("\" />");
			foreach (Class? additional in mockClass.DistinctAdditionalImplementations())
			{
				sb.Append(" and <see cref=\"").Append(additional.ClassFullName.EscapeForXmlDoc()).Append("\" />");
			}

			sb.AppendLine(".");
			sb.Append("/// </summary>").AppendLine();
			sb.Append("internal class MockFor").Append(name).Append(" : ").Append(mockClass.ClassFullName).Append(",")
				.AppendLine();
			foreach (Class? additional in mockClass.DistinctAdditionalImplementations())
			{
				sb.Append("\t").Append(additional.ClassFullName).Append(",").AppendLine();
			}

			sb.Append("\tIMockSubject<").Append(mockClass.ClassFullName).Append(">").AppendLine();
			sb.Append("{").AppendLine();
			sb.Append("\t/// <inheritdoc cref=\"IMockSubject{T}.Mock\" />").AppendLine();
			sb.Append("\tMock<").Append(mockClass.ClassFullName).Append("> IMockSubject<")
				.Append(mockClass.ClassFullName).Append(">.Mock => _mock;").AppendLine();
			sb.Append("\t[DebuggerBrowsable(DebuggerBrowsableState.Never)]").AppendLine();
			sb.Append("\tprivate readonly Mock<").Append(mockClass.ClassFullName).Append("> _mock;").AppendLine();
			sb.AppendLine();
			if (mockClass.Constructors?.Count > 0)
			{
				sb.Append(
						"\tinternal static readonly System.Threading.AsyncLocal<MockRegistration> MockRegistrationsProvider = new System.Threading.AsyncLocal<MockRegistration>();")
					.AppendLine().AppendLine();
				sb.Append("\t/// <inheritdoc cref=\"IHasMockRegistration.Registrations\" />").AppendLine();
				sb.Append("\t[DebuggerBrowsable(DebuggerBrowsableState.Never)]").AppendLine();
				sb.Append("\tMockRegistration IHasMockRegistration.Registrations => MockRegistrations;").AppendLine();
				sb.Append("\t[DebuggerBrowsable(DebuggerBrowsableState.Never)]").AppendLine();
				sb.Append("\tprivate MockRegistration MockRegistrations").AppendLine();
				sb.Append("\t{").AppendLine();
				sb.Append("\t\tget => _mockRegistrations ?? MockRegistrationsProvider.Value!;").AppendLine();
				sb.Append("\t\tset => _mockRegistrations = value;").AppendLine();
				sb.Append("\t}").AppendLine();
				sb.Append("\t[DebuggerBrowsable(DebuggerBrowsableState.Never)]").AppendLine();
				sb.Append("\tprivate MockRegistration? _mockRegistrations;").AppendLine();
			}
			else
			{
				sb.Append("\t/// <inheritdoc cref=\"IHasMockRegistration.Registrations\" />").AppendLine();
				sb.Append("\t[DebuggerBrowsable(DebuggerBrowsableState.Never)]").AppendLine();
				sb.Append("\tMockRegistration IHasMockRegistration.Registrations => _mock.Registrations;").AppendLine();
				sb.Append("\t[DebuggerBrowsable(DebuggerBrowsableState.Never)]").AppendLine();
				sb.Append("\tprivate MockRegistration MockRegistrations => _mock.Registrations;").AppendLine();
			}

			sb.AppendLine();

			if (mockClass.IsInterface)
			{
				sb.Append("\t/// <inheritdoc cref=\"MockFor").Append(name).Append("\" />").AppendLine();
				sb.Append("\tpublic MockFor").Append(name).Append("(MockBehavior mockBehavior)").AppendLine();
				sb.Append("\t{").AppendLine();
				sb.Append("\t\t_mock = new Mock<").Append(mockClass.ClassFullName)
					.Append(">(this, new MockRegistration(mockBehavior, \"").Append(mockClass.DisplayString)
					.Append("\"));").AppendLine();
				sb.Append("\t}").AppendLine();
				sb.AppendLine();
				sb.Append("\t/// <inheritdoc cref=\"MockFor").Append(name).Append("\" />").AppendLine();
				sb.Append("\tpublic MockFor").Append(name).Append("(MockRegistration mockRegistration)").AppendLine();
				sb.Append("\t{").AppendLine();
				sb.Append("\t\t_mock = new Mock<").Append(mockClass.ClassFullName)
					.Append(">(this, mockRegistration);").AppendLine();
				sb.Append("\t}").AppendLine();
				sb.AppendLine();
			}
			else if (mockClass.Constructors?.Count > 0)
			{
				foreach (Method constructor in mockClass.Constructors)
				{
					AppendMockSubject_BaseClassConstructor(sb, mockClass, name, constructor);
				}
			}

			AppendMockSubject_ImplementClass(sb, mockClass, null);
			foreach (Class? additional in mockClass.DistinctAdditionalImplementations())
			{
				sb.AppendLine();
				AppendMockSubject_ImplementClass(sb, additional, mockClass);
			}

			sb.AppendLine("}");
		}

		sb.AppendLine("#nullable disable annotations");
		return sb.ToString();
	}

	private static void AppendMockSubject_BaseClassConstructor(StringBuilder sb, MockClass mockClass, string name,
		Method constructor)
	{
		sb.Append("\t/// <inheritdoc cref=\"MockFor").Append(name).Append("\" />").AppendLine();
		sb.Append(constructor.Attributes, "\t");
		sb.Append("\tpublic MockFor").Append(name).Append("(");
		foreach (MethodParameter parameter in constructor.Parameters)
		{
			sb.Append(parameter.Type.Fullname).Append(' ').Append(parameter.Name);
			sb.Append(", ");
		}

		sb.Append("MockRegistration mockRegistration)").AppendLine();
		sb.Append("\t\t\t: base(");
		int index = 0;
		foreach (MethodParameter parameter in constructor.Parameters)
		{
			if (index++ > 0)
			{
				sb.Append(", ");
			}

			sb.Append(parameter.Name);
		}

		sb.Append(')').AppendLine();
		sb.Append("\t{").AppendLine();
		sb.Append("\t\t_mock = new Mock<").Append(mockClass.ClassFullName).Append(">(this, mockRegistration);")
			.AppendLine();
		sb.Append("\t\t_mockRegistrations = mockRegistration;").AppendLine();
		sb.Append("\t}").AppendLine();
		sb.AppendLine();
	}

	private static void AppendMockSubject_ImplementClass(StringBuilder sb, Class @class,
		MockClass? mockClass)
	{
		string className = @class.ClassFullName;
		sb.Append("\t#region ").Append(className).AppendLine();
		int count = 0;
		List<Event>? mockEvents = mockClass?.AllEvents().ToList();
		foreach (Event @event in @class.AllEvents())
		{
			if (count++ > 0)
			{
				sb.AppendLine();
			}

			if (mockEvents?.All(e => !Event.EqualityComparer.Equals(@event, e)) != false)
			{
				AppendMockSubject_ImplementClass_AddEvent(sb, @event, className, mockClass is not null,
					@class.IsInterface);
			}
		}

		List<Property>? mockProperties = mockClass?.AllProperties().ToList();
		foreach (Property property in @class.AllProperties())
		{
			if (count++ > 0)
			{
				sb.AppendLine();
			}

			if (mockProperties?.All(p => !Property.EqualityComparer.Equals(property, p)) != false)
			{
				AppendMockSubject_ImplementClass_AddProperty(sb, property, className, mockClass is not null,
					@class.IsInterface);
			}
		}

		List<Method>? mockMethods = mockClass?.AllMethods().ToList();
		foreach (Method method in @class.AllMethods())
		{
			if (count++ > 0)
			{
				sb.AppendLine();
			}


			if (mockMethods?.All(m => !Method.EqualityComparer.Equals(method, m)) != false)
			{
				AppendMockSubject_ImplementClass_AddMethod(sb, method, className, mockClass is not null,
					@class.IsInterface);
			}
		}

		sb.Append("\t#endregion ").Append(className).AppendLine();
	}

	private static void AppendMockSubject_ImplementClass_AddEvent(StringBuilder sb, Event @event, string className,
		bool explicitInterfaceImplementation, bool isClassInterface)
	{
		sb.Append("\t/// <inheritdoc cref=\"").Append(@event.ContainingType.EscapeForXmlDoc()).Append('.')
			.Append(@event.Name.EscapeForXmlDoc())
			.AppendLine("\" />");
		sb.Append(@event.Attributes, "\t");
		if (explicitInterfaceImplementation)
		{
			sb.Append("\tevent ").Append(@event.Type.Fullname.TrimEnd('?'))
				.Append("? ").Append(className).Append('.').Append(@event.Name).AppendLine();
		}
		else
		{
			if (@event.ExplicitImplementation is null)
			{
				sb.Append("\t").Append(@event.Accessibility.ToVisibilityString()).Append(' ');
				if (!isClassInterface && @event.UseOverride)
				{
					sb.Append("override ");
				}

				sb.Append("event ").Append(@event.Type.Fullname.TrimEnd('?')).Append("? ");
			}
			else
			{
				sb.Append("\t").Append("event ").Append(@event.Type.Fullname.TrimEnd('?')).Append("? ")
					.Append(@event.ExplicitImplementation).Append('.');
			}

			sb.Append(@event.Name).AppendLine();
		}

		sb.AppendLine("\t{");
		sb.Append("\t\tadd => MockRegistrations.AddEvent(").Append(@event.GetUniqueNameString())
			.Append(", value?.Target, value?.Method);").AppendLine();
		sb.Append("\t\tremove => MockRegistrations.RemoveEvent(")
			.Append(@event.GetUniqueNameString()).Append(", value?.Target, value?.Method);").AppendLine();
		sb.AppendLine("\t}");
	}

	private static void AppendMockSubject_ImplementClass_AddProperty(StringBuilder sb, Property property,
		string className, bool explicitInterfaceImplementation, bool isClassInterface)
	{
		sb.Append("\t/// <inheritdoc cref=\"").Append(property.ContainingType.EscapeForXmlDoc()).Append('.').Append(
				property.IndexerParameters is not null
					? property.Name.Replace("[]",
							$"[{string.Join(", ", property.IndexerParameters.Value.Select(p => $"{p.Type.Fullname}"))}]")
						.EscapeForXmlDoc()
					: property.Name.EscapeForXmlDoc())
			.AppendLine("\" />");
		sb.Append(property.Attributes, "\t");
		if (explicitInterfaceImplementation)
		{
			sb.Append("\t").Append(property.Type.Fullname)
				.Append(" ").Append(className).Append('.').Append(property.IndexerParameters is not null
					? property.Name.Replace("[]",
						$"[{string.Join(", ", property.IndexerParameters.Value.Select(p => $"{p.Type.Fullname} {p.Name}"))}]")
					: property.Name).AppendLine();
		}
		else
		{
			if (property.ExplicitImplementation is null)
			{
				sb.Append("\t").Append(property.Accessibility.ToVisibilityString()).Append(' ');
				if (!isClassInterface && property.UseOverride)
				{
					sb.Append("override ");
				}

				sb.Append(property.Type.Fullname).Append(" ");
			}
			else
			{
				sb.Append("\t").Append(property.Type.Fullname).Append(" ").Append(property.ExplicitImplementation)
					.Append('.');
			}

			sb.Append(property.IndexerParameters is not null
				? property.Name.Replace("[]",
					$"[{string.Join(", ", property.IndexerParameters.Value.Select(p => $"{p.Type.Fullname} {p.Name}"))}]")
				: property.Name).AppendLine();
		}

		sb.AppendLine("\t{");
		if (property.Getter != null && property.Getter.Accessibility != Accessibility.Private)
		{
			sb.Append("\t\t");
			if (property.Getter.Accessibility != property.Accessibility)
			{
				sb.Append(property.Getter.Accessibility.ToVisibilityString()).Append(' ');
			}

			sb.AppendLine("get");
			sb.AppendLine("\t\t{");
			if (!isClassInterface && !property.IsAbstract)
			{
				if (property is { IsIndexer: true, IndexerParameters: not null, })
				{
					sb.Append("\t\t\tvar indexerResult = MockRegistrations.GetIndexer<").Append(property.Type.Fullname)
						.Append(">(")
						.Append(string.Join(", ", property.IndexerParameters.Value.Select(p
							=> (p.IsSpan, p.IsReadOnlySpan) switch
							{
								(true, false) => $"new SpanWrapper<{p.SpanType!.Fullname}>({p.Name})",
								(false, true) => $"new ReadOnlySpanWrapper<{p.SpanType!.Fullname}>({p.Name})",
								(_, _) => p.Name,
							})))
						.AppendLine(");");
					sb.Append(
							"\t\t\tif (indexerResult.CallBaseClass)")
						.AppendLine();
					sb.Append("\t\t\t{").AppendLine();
					sb.Append("\t\t\t\tvar baseResult = base[")
						.Append(string.Join(", ", property.IndexerParameters.Value.Select(p => p.Name)))
						.Append("];").AppendLine();
					sb.Append("\t\t\t\treturn indexerResult.GetResult(baseResult);").AppendLine();
					sb.Append("\t\t\t}").AppendLine();
					sb.Append("\t\t\treturn indexerResult.GetResult();").AppendLine();
				}
				else
				{
					sb.Append(
							"\t\t\treturn MockRegistrations.GetProperty<").Append(property.Type.Fullname).Append(">(")
						.Append(property.GetUniqueNameString()).Append(", () => base.").Append(property.Name)
						.Append(");").AppendLine();
				}
			}
			else if (property is { IsIndexer: true, IndexerParameters: not null, })
			{
				sb.Append("\t\t\treturn MockRegistrations.GetIndexer<")
					.Append(property.Type.Fullname)
					.Append(">(").Append(string.Join(", ", property.IndexerParameters.Value.Select(p
						=> (p.IsSpan, p.IsReadOnlySpan) switch
						{
							(true, false) => $"new SpanWrapper<{p.SpanType!.Fullname}>({p.Name})",
							(false, true) => $"new ReadOnlySpanWrapper<{p.SpanType!.Fullname}>({p.Name})",
							(_, _) => p.Name,
						})))
					.AppendLine(").GetResult();");
			}
			else
			{
				sb.Append("\t\t\treturn MockRegistrations.GetProperty<")
					.Append(property.Type.Fullname)
					.Append(">(").Append(property.GetUniqueNameString()).Append(");").AppendLine();
			}

			sb.AppendLine("\t\t}");
		}

		if (property.Setter != null && property.Setter.Accessibility != Accessibility.Private)
		{
			sb.Append("\t\t");
			if (property.Setter.Accessibility != property.Accessibility)
			{
				sb.Append(property.Setter.Accessibility.ToVisibilityString()).Append(' ');
			}

			sb.AppendLine("set");
			sb.AppendLine("\t\t{");

			if (property is { IsIndexer: true, IndexerParameters: not null, })
			{
				if (!isClassInterface && !property.IsAbstract)
				{
					sb.Append(
							"\t\t\tif (MockRegistrations.SetIndexer<")
						.Append(property.Type.Fullname)
						.Append(">(value, ").Append(string.Join(", ", property.IndexerParameters.Value.Select(p
							=> (p.IsSpan, p.IsReadOnlySpan) switch
							{
								(true, false) => $"new SpanWrapper<{p.SpanType!.Fullname}>({p.Name})",
								(false, true) => $"new ReadOnlySpanWrapper<{p.SpanType!.Fullname}>({p.Name})",
								(_, _) => p.Name,
							})))
						.Append("))").AppendLine();
					sb.Append("\t\t\t{").AppendLine();
					sb.Append("\t\t\t\tbase[")
						.Append(string.Join(", ", property.IndexerParameters.Value.Select(p => p.Name)))
						.AppendLine("] = value;");
					sb.Append("\t\t\t}").AppendLine();
				}
				else
				{
					sb.Append("\t\t\tMockRegistrations.SetIndexer<")
						.Append(property.Type.Fullname)
						.Append(">(value, ").Append(string.Join(", ", property.IndexerParameters.Value.Select(p
							=> (p.IsSpan, p.IsReadOnlySpan) switch
							{
								(true, false) => $"new SpanWrapper<{p.SpanType!.Fullname}>({p.Name})",
								(false, true) => $"new ReadOnlySpanWrapper<{p.SpanType!.Fullname}>({p.Name})",
								(_, _) => p.Name,
							})))
						.AppendLine(");");
				}
			}
			else
			{
				if (!isClassInterface && !property.IsAbstract)
				{
					sb.Append(
							"\t\t\tif (MockRegistrations.SetProperty(").Append(property.GetUniqueNameString())
						.Append(", value))").AppendLine();
					sb.Append("\t\t\t{").AppendLine();
					sb.Append("\t\t\t\tbase.").Append(property.Name).Append(" = value;").AppendLine();
					sb.Append("\t\t\t}").AppendLine();
				}
				else
				{
					sb.Append("\t\t\tMockRegistrations.SetProperty(").Append(property.GetUniqueNameString())
						.AppendLine(", value);");
				}
			}

			sb.AppendLine("\t\t}");
		}

		sb.AppendLine("\t}");
	}

	private static void AppendMockSubject_ImplementClass_AddMethod(StringBuilder sb, Method method, string className,
		bool explicitInterfaceImplementation, bool isClassInterface)
	{
		sb.Append("\t/// <inheritdoc cref=\"").Append(method.ContainingType.EscapeForXmlDoc()).Append('.')
			.Append(method.Name.EscapeForXmlDoc())
			.Append('(').Append(string.Join(", ",
				method.Parameters.Select(p => p.RefKind.GetString() + p.Type.Fullname)).EscapeForXmlDoc())
			.AppendLine(")\" />");
		sb.Append(method.Attributes, "\t");
		if (explicitInterfaceImplementation)
		{
			sb.Append("\t");
			sb.Append(method.ReturnType.Fullname).Append(' ')
				.Append(className).Append('.').Append(method.Name).Append('(');
		}
		else
		{
			sb.Append("\t");
			if (method.ExplicitImplementation is null)
			{
				sb.Append(method.Accessibility.ToVisibilityString()).Append(' ');
				if ((!isClassInterface && method.UseOverride) || method.IsEquals() || method.IsGetHashCode() ||
				    method.IsToString())
				{
					sb.Append("override ");
				}

				sb.Append(method.ReturnType.Fullname).Append(' ')
					.Append(method.Name).Append('(');
			}
			else
			{
				sb.Append(method.ReturnType.Fullname).Append(' ')
					.Append(method.ExplicitImplementation).Append('.').Append(method.Name).Append('(');
			}
		}

		int index = 0;
		foreach (MethodParameter parameter in method.Parameters)
		{
			if (index++ > 0)
			{
				sb.Append(", ");
			}

			sb.Append(parameter.RefKind.GetString());
			sb.Append(parameter.Type.Fullname).Append(' ').Append(parameter.Name);
		}

		sb.Append(')');
		if (method.GenericParameters is not null && method.GenericParameters.Value.Count > 0)
		{
			foreach (GenericParameter gp in method.GenericParameters.Value)
			{
				gp.AppendWhereConstraint(sb, "\t\t");
			}
		}

		sb.AppendLine();
		sb.AppendLine("\t{");
		if (method.ReturnType != Type.Void)
		{
			sb.Append("\t\tMethodSetupResult<").Append(method.ReturnType.Fullname)
				.Append("> methodExecution = MockRegistrations.InvokeMethod<")
				.Append(method.ReturnType.Fullname).Append(">(").Append(method.GetUniqueNameString());
		}
		else
		{
			sb.Append("\t\tMethodSetupResult methodExecution = MockRegistrations.InvokeMethod(")
				.Append(method.GetUniqueNameString());
		}

		foreach (MethodParameter p in method.Parameters)
		{
			sb.Append(", ").Append((p.RefKind, p.IsSpan, p.IsReadOnlySpan) switch
			{
				(RefKind.Out, _, _) => "null",
				(_, true, false) => $"new SpanWrapper<{p.SpanType!.Fullname}>({p.Name})",
				(_, false, true) => $"new ReadOnlySpanWrapper<{p.SpanType!.Fullname}>({p.Name})",
				(_, _, _) => p.Name,
			});
		}

		sb.AppendLine(");");

		if (isClassInterface || method.IsAbstract)
		{
			foreach (MethodParameter parameter in method.Parameters)
			{
				if (parameter.RefKind == RefKind.Out)
				{
					sb.Append("\t\t").Append(parameter.Name).Append(" = methodExecution.SetOutParameter<")
						.Append(parameter.Type.Fullname).Append(">(\"").Append(parameter.Name).AppendLine("\");");
				}
				else if (parameter.RefKind == RefKind.Ref)
				{
					sb.Append("\t\t").Append(parameter.Name).Append(" = methodExecution.SetRefParameter<")
						.Append(parameter.Type.Fullname).Append(">(\"").Append(parameter.Name).Append("\", ")
						.Append(parameter.Name).Append(");").AppendLine();
				}
			}

			sb.Append("\t\tmethodExecution.TriggerCallbacks(")
				.Append(
					string.Join(", ", method.Parameters.Select(p => p.IsSpan || p.IsReadOnlySpan ? "null" : p.Name)))
				.Append(");").AppendLine();
			if (method.ReturnType != Type.Void)
			{
				sb.Append("\t\treturn methodExecution.Result;").AppendLine();
			}
		}
		else if (method.ReturnType != Type.Void)
		{
			sb.Append(
					"\t\tif (methodExecution.CallBaseClass)")
				.AppendLine();
			sb.Append("\t\t{").AppendLine();
			sb.Append("\t\t\tvar baseResult = base.").Append(method.Name).Append('(')
				.Append(string.Join(", ", method.Parameters.Select(p => $"{p.RefKind.GetString()}{p.Name}")))
				.Append(");").AppendLine();
			foreach (MethodParameter parameter in method.Parameters)
			{
				if (parameter.RefKind == RefKind.Out)
				{
					sb.Append(
							"\t\t\tif (methodExecution.HasSetupResult == true)")
						.AppendLine();
					sb.Append("\t\t\t{").AppendLine();
					sb.Append("\t\t\t\t").Append(parameter.Name).Append(" = methodExecution.SetOutParameter<")
						.Append(parameter.Type.Fullname).Append(">(\"").Append(parameter.Name).AppendLine("\");");
					sb.Append("\t\t\t}").AppendLine().AppendLine();
				}
				else if (parameter.RefKind == RefKind.Ref)
				{
					sb.Append(
							"\t\t\tif (methodExecution.HasSetupResult == true)")
						.AppendLine();
					sb.Append("\t\t\t{").AppendLine();
					sb.Append("\t\t\t\t").Append(parameter.Name).Append(" = methodExecution.SetRefParameter<")
						.Append(parameter.Type.Fullname).Append(">(\"").Append(parameter.Name).Append("\", ")
						.Append(parameter.Name).Append(");").AppendLine();
					sb.Append("\t\t\t}").AppendLine().AppendLine();
				}
			}

			sb.Append(
					"\t\t\tif (!methodExecution.HasSetupResult)")
				.AppendLine();
			sb.Append("\t\t\t{").AppendLine();
			sb.Append("\t\t\t\tmethodExecution.TriggerCallbacks(")
				.Append(
					string.Join(", ", method.Parameters.Select(p => p.IsSpan || p.IsReadOnlySpan ? "null" : p.Name)))
				.Append(");").AppendLine();
			sb.Append("\t\t\t\treturn baseResult;").AppendLine();
			sb.Append("\t\t\t}").AppendLine();
			sb.Append("\t\t}").AppendLine();
			if (method.Parameters.Any(p => p.RefKind == RefKind.Ref || p.RefKind == RefKind.Out))
			{
				sb.Append("\t\telse").AppendLine();
				sb.Append("\t\t{").AppendLine();
				foreach (MethodParameter parameter in method.Parameters)
				{
					if (parameter.RefKind == RefKind.Out)
					{
						sb.Append("\t\t\t").Append(parameter.Name).Append(" = methodExecution.SetOutParameter<")
							.Append(parameter.Type.Fullname).Append(">(\"").Append(parameter.Name).AppendLine("\");");
					}
					else if (parameter.RefKind == RefKind.Ref)
					{
						sb.Append("\t\t\t").Append(parameter.Name).Append(" = methodExecution.SetRefParameter<")
							.Append(parameter.Type.Fullname).Append(">(\"").Append(parameter.Name).Append("\", ")
							.Append(parameter.Name).Append(");").AppendLine();
					}
				}

				sb.Append("\t\t}").AppendLine();
			}

			sb.AppendLine();
			sb.Append("\t\tmethodExecution.TriggerCallbacks(")
				.Append(
					string.Join(", ", method.Parameters.Select(p => p.IsSpan || p.IsReadOnlySpan ? "null" : p.Name)))
				.Append(");").AppendLine();
			sb.Append("\t\treturn methodExecution.Result;").AppendLine();
		}
		else
		{
			sb.Append(
					"\t\tif (methodExecution.CallBaseClass)")
				.AppendLine();
			sb.Append("\t\t{").AppendLine();
			sb.Append("\t\t\tbase.").Append(method.Name).Append('(')
				.Append(string.Join(", ", method.Parameters.Select(p => $"{p.RefKind.GetString()}{p.Name}")))
				.Append(");").AppendLine();
			sb.Append("\t\t}").AppendLine();
			foreach (MethodParameter parameter in method.Parameters)
			{
				if (parameter.RefKind == RefKind.Out)
				{
					sb.AppendLine();
					sb.Append("\t\t").Append(parameter.Name).Append(" = methodExecution.SetOutParameter<")
						.Append(parameter.Type.Fullname).Append(">(\"").Append(parameter.Name).AppendLine("\");");
				}
				else if (parameter.RefKind == RefKind.Ref)
				{
					sb.AppendLine();
					sb.Append("\t\t").Append(parameter.Name).Append(" = methodExecution.SetRefParameter<")
						.Append(parameter.Type.Fullname).Append(">(\"").Append(parameter.Name).Append("\", ")
						.Append(parameter.Name).Append(");").AppendLine();
				}

				sb.Append("\t\tmethodExecution.TriggerCallbacks(")
					.Append(string.Join(", ",
						method.Parameters.Select(p => p.IsSpan || p.IsReadOnlySpan ? "null" : p.Name)))
					.Append(");").AppendLine();
			}
		}

		sb.AppendLine("\t}");
	}
}
#pragma warning restore S3776 // Cognitive Complexity of methods should not be too high
