using System.Text;
using Microsoft.CodeAnalysis;
using Mockolate.SourceGenerators.Entities;
using Type = Mockolate.SourceGenerators.Entities.Type;

namespace Mockolate.SourceGenerators.Sources;

#pragma warning disable S3776 // Cognitive Complexity of methods should not be too high
internal static partial class Sources
{
	public static string ForMockSetupExtensions(string name, Class @class)
	{
		StringBuilder sb = InitializeBuilder([
			"Mockolate.Exceptions",
			"Mockolate.Monitor",
			"Mockolate.Raise",
			"Mockolate.Setup",
			"Mockolate.Verify",
		]);

		sb.Append("""
		          namespace Mockolate;

		          #nullable enable

		          """);
		sb.Append("internal static class MockFor").Append(name).Append("Extensions").AppendLine();
		sb.AppendLine("{");
		sb.Append("\textension(").Append(@class.ClassFullName).AppendLine(" subject)");
		sb.AppendLine("\t{");
		sb.Append("\t\t/// <summary>").AppendLine();
		sb.Append("\t\t///     Sets up the mock for <see cref=\"").Append(@class.ClassFullName.EscapeForXmlDoc()).AppendLine("\" />.").AppendLine();
		sb.Append("\t\t/// </summary>").AppendLine();
		sb.Append("\t\tpublic IMockSetup<").Append(@class.ClassFullName).AppendLine("> SetupMock").AppendLine();
		sb.Append("\t\t\t=> GetMockOrThrow(subject);").AppendLine();
		sb.AppendLine();
		sb.Append("\t\t/// <summary>").AppendLine();
		sb.Append("\t\t///     Raise events on the mock for <see cref=\"").Append(@class.ClassFullName.EscapeForXmlDoc()).AppendLine("\" />.").AppendLine();
		sb.Append("\t\t/// </summary>").AppendLine();
		sb.Append("\t\tpublic IMockRaises<").Append(@class.ClassFullName).AppendLine("> RaiseOnMock").AppendLine();
		sb.Append("\t\t\t=> GetMockOrThrow(subject);").AppendLine();
		sb.AppendLine();
		sb.Append("\t\t/// <summary>").AppendLine();
		sb.Append("\t\t///     Verifies the interactions with the mock for <see cref=\"").Append(@class.ClassFullName.EscapeForXmlDoc()).AppendLine("\" />.").AppendLine();
		sb.Append("\t\t/// </summary>").AppendLine();
		sb.Append("\t\tpublic IMockVerify<").Append(@class.ClassFullName).AppendLine("> VerifyMock").AppendLine();
		sb.Append("\t\t\t=> GetMockOrThrow(subject);").AppendLine();
		sb.AppendLine();
		sb.Append("\t\t/// <summary>").AppendLine();
		sb.Append("\t\t///     Verifies the interactions with the mock for <see cref=\"").Append(@class.ClassFullName.EscapeForXmlDoc()).AppendLine("\" />.").AppendLine();
		sb.Append("\t\t/// </summary>").AppendLine();
		sb.Append("\t\tpublic IDisposable MonitorMock(out MockMonitor<").Append(@class.ClassFullName).AppendLine("> monitor)").AppendLine();
		sb.Append("\t\t{").AppendLine();
		sb.Append("\t\t\tmonitor = new MockMonitor<").Append(@class.ClassFullName).AppendLine(">(GetMockOrThrow(subject));").AppendLine();
		sb.Append("\t\t\treturn monitor.Run();").AppendLine();
		sb.Append("\t\t}").AppendLine();
		sb.AppendLine("\t}");
		sb.AppendLine();

		AppendRaisesExtensions(sb, @class);
		AppendPropertySetupExtensions(sb, @class);
		AppendIndexerSetupExtensions(sb, @class);
		AppendMethodSetupExtensions(sb, @class);

		if (@class.AllEvents().Any(@event
			    => @event.Accessibility is not (Accessibility.Protected or Accessibility.ProtectedOrInternal)) ||
		    @class.AllMethods().Any(method
			    => method.Accessibility is not (Accessibility.Protected or Accessibility.ProtectedOrInternal)) ||
		    @class.AllProperties().Any(property
			    => property.Accessibility is not (Accessibility.Protected or Accessibility.ProtectedOrInternal)))
		{
			AppendRaisesExtensions(sb, @class, true);
			AppendPropertySetupExtensions(sb, @class, true);
			AppendIndexerSetupExtensions(sb, @class, true);
			AppendMethodSetupExtensions(sb, @class, true);
		}

		var methods = @class.AllMethods().Where(m => m.ExplicitImplementation is null && m.Accessibility is not (Accessibility.Protected or Accessibility.ProtectedOrInternal)).ToList();
		bool hasToString = methods.Any(m => m.IsToString());
		bool hasGetHashCode = methods.Any(m => m.IsGetHashCode());
		bool hasEquals = methods.Any(m => m.IsEquals());
		AppendInvokedExtensions(sb, @class, hasToString, hasGetHashCode, hasEquals);
		AppendGotExtensions(sb, @class);
		AppendSetExtensions(sb, @class);
		AppendGotIndexerExtensions(sb, @class);
		AppendSetIndexerExtensions(sb, @class);
		AppendEventExtensions(sb, @class);

		if (AppendProtectedMock(sb, @class))
		{
			AppendInvokedExtensions(sb, @class, hasToString, hasGetHashCode, hasEquals, true);
			AppendGotExtensions(sb, @class, true);
			AppendSetExtensions(sb, @class, true);
			AppendGotIndexerExtensions(sb, @class, true);
			AppendSetIndexerExtensions(sb, @class, true);
			AppendEventExtensions(sb, @class, true);
		}
		sb.AppendLine("""
		              	private static Mock<T> CastToMockOrThrow<T>(IInteractiveMock<T> subject)
		              	{
		              		if (subject is Mock<T> mock)
		              		{
		              			return mock;
		              		}
		              	
		              		throw new MockException("The subject is no mock.");
		              	}
		              """);
		sb.AppendLine();
		sb.AppendLine("""
		              	private static Mock<T> GetMockOrThrow<T>(T subject)
		              	{
		              		if (subject is IMockSubject<T> mock)
		              		{
		              			return mock.Mock;
		              		}
		              
		              		if (subject is IHasMockRegistration hasMockRegistration)
		              		{
		              			return new Mock<T>(subject, hasMockRegistration.Registrations);
		              		}
		              	
		              		throw new MockException("The subject is no mock subject.");
		              	}
		              """);
		sb.AppendLine("}");
		sb.AppendLine("#nullable disable");
		return sb.ToString();
	}

	private static void AppendRaisesExtensions(StringBuilder sb, Class @class,
		bool isProtected = false)
	{
		Func<Event, bool> predicate = isProtected
			? new Func<Event, bool>(@event
				=> @event.ExplicitImplementation is null &&
				   @event.Accessibility is Accessibility.Protected or Accessibility.ProtectedOrInternal)
			: new Func<Event, bool>(@event
				=> @event.ExplicitImplementation is null &&
				   @event.Accessibility is not (Accessibility.Protected or Accessibility.ProtectedOrInternal));
		if (!@class.AllEvents().Any(predicate))
		{
			return;
		}

		sb.Append("\textension(I").Append(isProtected ? "Protected" : "").Append("MockRaises<")
			.Append(@class.ClassFullName).Append("> mockRaises)").AppendLine();
		sb.AppendLine("\t{");
		int count = 0;
		foreach (Event @event in @class.AllEvents().Where(predicate))
		{
			if (count++ > 0)
			{
				sb.AppendLine();
			}

			sb.Append("\t\t/// <summary>").AppendLine();
			sb.Append("\t\t///     Raise the <see cref=\"").Append(@class.ClassFullName.EscapeForXmlDoc())
				.Append(".").Append(@event.Name.EscapeForXmlDoc())
				.Append("\"/> event.").AppendLine();
			sb.Append("\t\t/// </summary>").AppendLine();
			sb.Append("\t\tpublic void ").Append(@event.Name).Append("(").Append(string.Join(", ",
					@event.Delegate.Parameters.Select(p => p.Type.Fullname + " " + p.Name)))
				.Append(")").AppendLine();
			sb.AppendLine("\t\t{");
			sb.Append("\t\t\tCastToMockOrThrow(mockRaises).Registrations.Raise(").Append(@event.GetUniqueNameString()).Append(", ")
				.Append(string.Join(", ", @event.Delegate.Parameters.Select(p => p.Name))).Append(");").AppendLine();
			sb.AppendLine("\t\t}");
		}

		foreach (Event @event in @class.AllEvents()
					 .Where(predicate)
					 .GroupBy(m => m.Name)
					 .Where(g => g.Count() == 1)
					 .Select(g => g.Single())
					 .Where(m => m.Delegate.Parameters.Count > 0))
		{
			if (count++ > 0)
			{
				sb.AppendLine();
			}

			sb.Append("\t\t/// <summary>").AppendLine();
			sb.Append("\t\t///     Raise the <see cref=\"").Append(@class.ClassFullName.EscapeForXmlDoc())
				.Append(".").Append(@event.Name.EscapeForXmlDoc())
				.Append("\"/> event.").AppendLine();
			sb.Append("\t\t/// </summary>").AppendLine();
			sb.Append("\t\tpublic void ").Append(@event.Name).Append("(Match.IDefaultEventParameters parameters)").AppendLine();
			sb.AppendLine("\t\t{");
			sb.Append("\t\t\tMock<").Append(@class.ClassFullName).Append("> mock = CastToMockOrThrow(mockRaises);");
			sb.Append("\t\t\tMockBehavior mockBehavior = mock.Registrations.Behavior;").AppendLine();
			sb.Append("\t\t\tmock.Registrations.Raise(").Append(@event.GetUniqueNameString()).Append(", ")
				.Append(string.Join(", ", @event.Delegate.Parameters.Select(p => $"mockBehavior.DefaultValue.Generate<{p.Type.Fullname}>()"))).Append(");").AppendLine();
			sb.AppendLine("\t\t}");
		}

		sb.AppendLine("\t}");
		sb.AppendLine();
	}

	private static void AppendPropertySetupExtensions(StringBuilder sb, Class @class,
		bool isProtected = false)
	{
		Func<Property, bool> predicate = isProtected
			? new Func<Property, bool>(property
				=> property.ExplicitImplementation is null && !property.IsIndexer &&
				   property.Accessibility is Accessibility.Protected or Accessibility.ProtectedOrInternal)
			: new Func<Property, bool>(property
				=> property.ExplicitImplementation is null && !property.IsIndexer &&
				   property.Accessibility is not (Accessibility.Protected or Accessibility.ProtectedOrInternal));

		if (@class.AllProperties().Any(predicate))
		{
			sb.Append("\textension(I").Append(isProtected ? "Protected" : "").Append("MockSetup<")
				.Append(@class.ClassFullName).Append("> setup)").AppendLine();
			sb.AppendLine("\t{");
			sb.Append("\t\t/// <summary>").AppendLine();
			sb.Append("\t\t///     Sets up properties on the mock for <see cref=\"")
				.Append(@class.ClassFullName.EscapeForXmlDoc()).Append("\"/>.").AppendLine();
			sb.Append("\t\t/// </summary>").AppendLine();
			sb.Append("\t\tpublic I").Append(isProtected ? "Protected" : "").Append("MockPropertySetup<").Append(@class.ClassFullName).Append(">")
				.Append("Property").AppendLine();
			sb.Append("\t\t\t=> (I").Append(isProtected ? "Protected" : "").Append("MockPropertySetup<").Append(@class.ClassFullName).Append(">)setup;").AppendLine();
			sb.AppendLine("\t}");
			sb.AppendLine();

			sb.Append("\textension(I").Append(isProtected ? "Protected" : "").Append("MockPropertySetup<").Append(@class.ClassFullName).Append(">")
				.Append("setup)").AppendLine();
			sb.AppendLine("\t{");
			int count = 0;
			foreach (Property property in @class.AllProperties().Where(predicate))
			{
				if (count++ > 0)
				{
					sb.AppendLine();
				}

				sb.Append("\t\t/// <summary>").AppendLine();
				sb.Append("\t\t///     Setup for the property <see cref=\"")
					.Append(@class.ClassFullName.EscapeForXmlDoc()).Append(".")
					.Append(property.Name.EscapeForXmlDoc()).Append("\"/>.").AppendLine();
				sb.Append("\t\t/// </summary>").AppendLine();
				sb.Append("\t\tpublic PropertySetup<").Append(property.Type.Fullname).Append("> ")
					.Append(property.IndexerParameters is not null
						? property.Name.Replace("[]",
							$"[{string.Join(", ", property.IndexerParameters.Value.Select(p => $"Match.IParameter<{p.Type.Fullname}> {p.Name}"))}]")
						: property.Name).AppendLine();

				sb.AppendLine("\t\t{");
				sb.AppendLine("\t\t\tget");
				sb.AppendLine("\t\t\t{");
				sb.Append("\t\t\t\tvar propertySetup = new PropertySetup<").Append(property.Type.Fullname)
					.Append(">();").AppendLine();
				sb.AppendLine("\t\t\t\tCastToMockOrThrow(setup).Registrations.SetupProperty(").Append(property.GetUniqueNameString())
					.Append(", propertySetup);").AppendLine();
				sb.AppendLine("\t\t\t\treturn propertySetup;");
				sb.AppendLine("\t\t\t}");
				sb.AppendLine("\t\t}");
				sb.AppendLine();
			}

			sb.AppendLine("\t}");
			sb.AppendLine();
		}
	}

	private static void AppendIndexerSetupExtensions(StringBuilder sb, Class @class,
		bool isProtected = false)
	{
		Func<Property, bool> predicate = isProtected
			? new Func<Property, bool>(property
				=> property.ExplicitImplementation is null && property.IsIndexer &&
				   property.Accessibility is Accessibility.Protected or Accessibility.ProtectedOrInternal)
			: new Func<Property, bool>(property
				=> property.ExplicitImplementation is null && property.IsIndexer &&
				   property.Accessibility is not (Accessibility.Protected or Accessibility.ProtectedOrInternal));

		if (@class.AllProperties().Any(predicate))
		{
			sb.Append("\textension(I").Append(isProtected ? "Protected" : "").Append("MockSetup<")
				.Append(@class.ClassFullName).Append("> setup)").AppendLine();
			sb.AppendLine("\t{");
			int count = 0;
			foreach (Property indexer in @class.AllProperties().Where(predicate))
			{
				if (count++ > 0)
				{
					sb.AppendLine();
				}

				sb.Append("\t\t/// <summary>").AppendLine();
				sb.Append("\t\t///     Sets up the ").Append(indexer.Type.Fullname)
					.Append(" indexer on the mock for <see cref=\"").Append(@class.ClassFullName.EscapeForXmlDoc())
					.Append("\" />.")
					.AppendLine();
				sb.Append("\t\t/// </summary>").AppendLine();
				sb.Append("\t\tpublic IndexerSetup<").Append(indexer.Type.Fullname);
				foreach (MethodParameter parameter in indexer.IndexerParameters!)
				{
					sb.Append(", ").Append(parameter.Type.Fullname);
				}

				sb.Append("> Indexer").Append("(").Append(string.Join(", ",
					indexer.IndexerParameters.Value.Select((p, i)
						=> $"Match.IParameter<{p.Type.Fullname}> parameter{i + 1}"))).Append(")").AppendLine();
				sb.Append("\t\t{").AppendLine();
				sb.Append("\t\t\tvar indexerSetup = new IndexerSetup<").Append(indexer.Type.Fullname);
				foreach (MethodParameter parameter in indexer.IndexerParameters!)
				{
					sb.Append(", ").Append(parameter.Type.Fullname);
				}

				sb.Append(">(").Append(string.Join(", ",
						Enumerable.Range(1, indexer.IndexerParameters.Value.Count).Select(p => $"parameter{p}")))
					.Append(");").AppendLine();
				sb.Append("\t\t\tCastToMockOrThrow(setup).Registrations.SetupIndexer(indexerSetup);").AppendLine();
				sb.Append("\t\t\treturn indexerSetup;").AppendLine();
				sb.Append("\t\t}").AppendLine();
			}

			sb.AppendLine("\t}");
			sb.AppendLine();
		}
	}

	private static void AppendMethodSetupExtensions(StringBuilder sb, Class @class,
		bool isProtected = false)
	{
		Func<Method, bool> predicate = isProtected
			? new Func<Method, bool>(method
				=> method.ExplicitImplementation is null &&
				   method.Accessibility is Accessibility.Protected or Accessibility.ProtectedOrInternal)
			: new Func<Method, bool>(method
				=> method.ExplicitImplementation is null &&
				   method.Accessibility is not (Accessibility.Protected or Accessibility.ProtectedOrInternal));
		var methods = @class.AllMethods().Where(predicate).ToList();
		if (methods.Any())
		{
			bool hasToString = !isProtected && methods.Any(m => m.IsToString());
			bool hasGetHashCode = !isProtected && methods.Any(m => m.IsGetHashCode());
			bool hasEquals = !isProtected && methods.Any(m => m.IsEquals());
			sb.Append("\textension(I").Append(isProtected ? "Protected" : "").Append("MockSetup<")
				.Append(@class.ClassFullName).Append("> setup)").AppendLine();
			sb.AppendLine("\t{");
			sb.Append("\t\t/// <summary>").AppendLine();
			sb.Append("\t\t///     Sets up methods on the mock for <see cref=\"")
				.Append(@class.ClassFullName.EscapeForXmlDoc()).Append("\"/>.").AppendLine();
			sb.Append("\t\t/// </summary>").AppendLine();
			sb.Append("\t\tpublic I").Append(isProtected ? "Protected" : "").Append("MockMethodSetup").Append(hasToString ? "WithToString" : "").Append(hasEquals ? "WithEquals" : "").Append(hasGetHashCode ? "WithGetHashCode" : "").Append("<").Append(@class.ClassFullName).Append(">")
				.Append(" Method").AppendLine();
			sb.Append("\t\t\t=> (I").Append(isProtected ? "Protected" : "").Append("MockMethodSetup").Append(hasToString ? "WithToString" : "").Append(hasEquals ? "WithEquals" : "").Append(hasGetHashCode ? "WithGetHashCode" : "").Append("<").Append(@class.ClassFullName).Append(">)setup")
				.Append(";").AppendLine();
			sb.AppendLine("\t}");
			sb.AppendLine();

			sb.Append("\textension(I").Append(isProtected ? "Protected" : "").Append("MockMethodSetup<").Append(@class.ClassFullName).Append("> setup)").AppendLine();
			sb.AppendLine("\t{");
			int count = 0;
			foreach (Method method in methods)
			{
				if (method.IsToString() || method.IsGetHashCode() || method.IsEquals())
				{
					continue;
				}

				if (count++ > 0)
				{
					sb.AppendLine();
				}

				sb.Append("\t\t/// <summary>").AppendLine();
				sb.Append("\t\t///     Setup for the method <see cref=\"")
					.Append(@class.ClassFullName.EscapeForXmlDoc()).Append(".")
					.Append(method.Name.EscapeForXmlDoc()).Append("(")
					.Append(string.Join(", ",
						method.Parameters.Select(p => p.RefKind.GetString() + p.Type.Fullname)))
					.Append(")\"/>");
				if (method.Parameters.Any())
				{
					sb.Append(" with the given ")
						.Append(string.Join(", ", method.Parameters.Select(p => $"<paramref name=\"{p.Name}\"/>")));
				}

				sb.Append(".").AppendLine();
				sb.Append("\t\t/// </summary>").AppendLine();
				if (method.ReturnType != Type.Void)
				{
					sb.Append("\t\tpublic ReturnMethodSetup<")
						.Append(method.ReturnType.Fullname);
					foreach (MethodParameter parameter in method.Parameters)
					{
						sb.Append(", ").Append(parameter.Type.Fullname);
					}

					sb.Append("> ");
					sb.Append(method.Name).Append("(");
				}
				else
				{
					sb.Append("\t\tpublic VoidMethodSetup");
					if (method.Parameters.Count > 0)
					{
						sb.Append('<');
						int index = 0;
						foreach (MethodParameter parameter in method.Parameters)
						{
							if (index++ > 0)
							{
								sb.Append(", ");
							}

							sb.Append(parameter.Type.Fullname);
						}

						sb.Append('>');
					}

					sb.Append(' ').Append(method.Name).Append("(");
				}

				int i = 0;
				foreach (MethodParameter parameter in method.Parameters)
				{
					if (i++ > 0)
					{
						sb.Append(", ");
					}

					sb.Append(parameter.RefKind switch
						{
							RefKind.Ref => "Match.IRefParameter<",
							RefKind.Out => "Match.IOutParameter<",
							_ => "Match.IParameter<",
						}).Append(parameter.Type.Fullname)
						.Append('>');
					if (parameter.RefKind is not RefKind.Ref and not RefKind.Out)
					{
						sb.Append('?');
					}

					sb.Append(' ').Append(parameter.Name);
				}

				sb.Append(")");
				if (method.GenericParameters is not null && method.GenericParameters.Value.Count > 0)
				{
					foreach (GenericParameter gp in method.GenericParameters.Value)
					{
						gp.AppendWhereConstraint(sb, "\t\t\t");
					}
				}
				sb.AppendLine();
				sb.AppendLine("\t\t{");

				if (method.ReturnType != Type.Void)
				{
					sb.Append("\t\t\tvar methodSetup = new ReturnMethodSetup<")
						.Append(method.ReturnType.Fullname);
					foreach (MethodParameter parameter in method.Parameters)
					{
						sb.Append(", ").Append(parameter.Type.Fullname);
					}

					sb.Append(">");
				}
				else
				{
					sb.Append("\t\t\tvar methodSetup = new VoidMethodSetup");

					if (method.Parameters.Count > 0)
					{
						sb.Append('<');
						int index = 0;
						foreach (MethodParameter parameter in method.Parameters)
						{
							if (index++ > 0)
							{
								sb.Append(", ");
							}

							sb.Append(parameter.Type.Fullname);
						}

						sb.Append('>');
					}
				}

				sb.Append("(").Append(method.GetUniqueNameString());
				foreach (MethodParameter parameter in method.Parameters)
				{
					sb.Append(", new Match.NamedParameter(\"").Append(parameter.Name).Append("\", ")
						.Append(parameter.Name);
					if (parameter.RefKind is not RefKind.Ref and not RefKind.Out)
					{
						sb.Append(" ?? Match.Null<").Append(parameter.Type.Fullname)
							.Append(">()");
					}

					sb.Append(")");
				}

				sb.Append(");").AppendLine();
				sb.AppendLine("\t\t\tCastToMockOrThrow(setup).Registrations.SetupMethod(methodSetup);");
				sb.AppendLine("\t\t\treturn methodSetup;");
				sb.AppendLine("\t\t}");
			}

			foreach (Method method in @class.AllMethods()
				         .Where(predicate)
				         .GroupBy(m => m.Name)
				         .Where(g => g.Count() == 1)
				         .Select(g => g.Single())
				         .Where(m => m.Parameters.Count > 0))
			{
				if (count++ > 0)
				{
					sb.AppendLine();
				}

				sb.Append("\t\t/// <summary>").AppendLine();
				sb.Append("\t\t///     Setup for the method <see cref=\"")
					.Append(@class.ClassFullName.EscapeForXmlDoc()).Append(".")
					.Append(method.Name.EscapeForXmlDoc()).Append("(")
					.Append(string.Join(", ",
						method.Parameters.Select(p => p.RefKind.GetString() + p.Type.Fullname)))
					.Append(")\"/>");
				sb.Append(" with the given <paramref name=\"parameters\" />.").AppendLine();
				sb.Append("\t\t/// </summary>").AppendLine();
				if (method.ReturnType != Type.Void)
				{
					sb.Append("\t\tpublic ReturnMethodSetup<").Append(method.ReturnType.Fullname);
					foreach (MethodParameter parameter in method.Parameters)
					{
						sb.Append(", ").Append(parameter.Type.Fullname);
					}
					sb.Append("> ").Append(method.Name).Append("(");
				}
				else
				{
					sb.Append("\t\tpublic VoidMethodSetup<")
						.Append(string.Join(", ", method.Parameters.Select(parameter => parameter.Type.Fullname)))
						.Append("> ").Append(method.Name).Append("(");
				}

				sb.Append("Match.IParameters parameters)");
				if (method.GenericParameters is not null && method.GenericParameters.Value.Count > 0)
				{
					foreach (GenericParameter gp in method.GenericParameters.Value)
					{
						gp.AppendWhereConstraint(sb, "\t\t\t");
					}
				}
				sb.AppendLine();

				sb.AppendLine("\t\t{");

				if (method.ReturnType != Type.Void)
				{
					sb.Append("\t\t\tvar methodSetup = new ReturnMethodSetup<")
						.Append(method.ReturnType.Fullname);
					foreach (MethodParameter parameter in method.Parameters)
					{
						sb.Append(", ").Append(parameter.Type.Fullname);
					}
					sb.Append(">(");
				}
				else
				{
					sb.Append("\t\t\tvar methodSetup = new VoidMethodSetup<")
						.Append(string.Join(", ", method.Parameters.Select(parameter => parameter.Type.Fullname)))
						.Append(">(");
				}

				sb.Append(method.GetUniqueNameString()).Append(", parameters);").AppendLine();
				sb.AppendLine("\t\t\tCastToMockOrThrow(setup).Registrations.SetupMethod(methodSetup);");
				sb.AppendLine("\t\t\treturn methodSetup;");
				sb.AppendLine("\t\t}");
			}

			sb.AppendLine("\t}");
			sb.AppendLine();
		}
	}
}
#pragma warning restore S3776 // Cognitive Complexity of methods should not be too high
