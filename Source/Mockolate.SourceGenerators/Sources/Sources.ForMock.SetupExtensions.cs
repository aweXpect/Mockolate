using System.Text;
using Microsoft.CodeAnalysis;
using Mockolate.SourceGenerators.Entities;
using Type = Mockolate.SourceGenerators.Entities.Type;

namespace Mockolate.SourceGenerators.Internals;

#pragma warning disable S3776 // Cognitive Complexity of methods should not be too high
internal static partial class Sources
{
	public static string ForMockSetupExtensions(string name, Class @class)
	{
		StringBuilder sb = InitializeBuilder([
			"Mockolate.Events",
			"Mockolate.Setup",
			"Mockolate.Verify",
		]);

		sb.Append("""
		          namespace Mockolate;

		          #nullable enable

		          """);
		sb.AppendLine();
		sb.Append("internal static class SetupExtensionsFor").Append(name).AppendLine();
		sb.AppendLine("{");

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

		sb.AppendLine("}");
		sb.AppendLine("#nullable disable");
		return sb.ToString();
	}

	private static void AppendRaisesExtensions(StringBuilder sb, Class @class,
		bool isProtected = false)
	{
		Func<Event, bool> predicate = isProtected
			? new Func<Event, bool>(@event
				=> @event.Accessibility is Accessibility.Protected or Accessibility.ProtectedOrInternal)
			: new Func<Event, bool>(@event
				=> @event.Accessibility is not (Accessibility.Protected or Accessibility.ProtectedOrInternal));
		if (!@class.AllEvents().Any(predicate))
		{
			return;
		}

		sb.Append("\textension(").Append(isProtected ? "Protected" : "").Append("MockRaises<").Append(@class.ClassFullName).Append("> mock)").AppendLine();
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
			sb.Append("\t\t\t((IMockRaises)mock).Raise(").Append(@event.GetUniqueNameString()).Append(", ")
				.Append(string.Join(", ", @event.Delegate.Parameters.Select(p => p.Name))).Append(");").AppendLine();
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
				=> !property.IsIndexer && property.Accessibility is Accessibility.Protected or Accessibility.ProtectedOrInternal)
			: new Func<Property, bool>(property
				=> !property.IsIndexer &&
				   property.Accessibility is not (Accessibility.Protected or Accessibility.ProtectedOrInternal));

		if (@class.AllProperties().Any(predicate))
		{
			sb.Append("\textension(").Append(isProtected ? "Protected" : "").Append("MockSetup<").Append(@class.ClassFullName).Append("> setup)").AppendLine();
			sb.AppendLine("\t{");
			sb.Append("\t\t/// <summary>").AppendLine();
			sb.Append("\t\t///     Sets up properties on the mock for <see cref=\"").Append(@class.ClassFullName.EscapeForXmlDoc()).Append("\"/>.").AppendLine();
			sb.Append("\t\t/// </summary>").AppendLine();
			sb.Append("\t\tpublic MockSetup<").Append(@class.ClassFullName).Append(">")
				.Append(isProtected ? ".Protected" : ".").Append("Properties Property").AppendLine();
			sb.Append("\t\t\t=> new MockSetup<").Append(@class.ClassFullName).Append(">")
				.Append(isProtected ? ".Protected" : ".").Append("Properties(setup);").AppendLine();
			sb.AppendLine("\t}");
			sb.AppendLine();

			sb.Append("\textension(MockSetup<").Append(@class.ClassFullName).Append(">")
				.Append(isProtected ? ".Protected" : ".").Append("Properties setup)").AppendLine();
			sb.AppendLine("\t{");
			int count = 0;
			foreach (Property property in @class.AllProperties().Where(predicate))
			{
				if (count++ > 0)
				{
					sb.AppendLine();
				}

				sb.Append("\t\t/// <summary>").AppendLine();
				sb.Append("\t\t///     Setup for the property <see cref=\"").Append(@class.ClassFullName.EscapeForXmlDoc()).Append(".")
					.Append(property.Name.EscapeForXmlDoc()).Append("\"/>.").AppendLine();
				sb.Append("\t\t/// </summary>").AppendLine();
				sb.Append("\t\tpublic PropertySetup<").Append(property.Type.Fullname).Append("> ")
					.Append(property.IndexerParameters is not null
						? property.Name.Replace("[]",
							$"[{string.Join(", ", property.IndexerParameters.Value.Select(p => $"With.Parameter<{p.Type.Fullname}> {p.Name}"))}]")
						: property.Name).AppendLine();

				sb.AppendLine("\t\t{");
				sb.AppendLine("\t\t\tget");
				sb.AppendLine("\t\t\t{");
				sb.Append("\t\t\t\tvar propertySetup = new PropertySetup<").Append(property.Type.Fullname)
					.Append(">();").AppendLine();
				sb.AppendLine("\t\t\t\tif (setup is IMockSetup mockSetup)");
				sb.AppendLine("\t\t\t\t{");
				sb.Append("\t\t\t\t\tmockSetup.RegisterProperty(").Append(property.GetUniqueNameString()).Append(", propertySetup);").AppendLine();
				sb.AppendLine("\t\t\t\t}");
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
				=> property.IsIndexer && property.Accessibility is Accessibility.Protected or Accessibility.ProtectedOrInternal)
			: new Func<Property, bool>(property
				=> property.IsIndexer && property.Accessibility is not (Accessibility.Protected or Accessibility.ProtectedOrInternal));

		if (@class.AllProperties().Any(predicate))
		{
			sb.Append("\textension(").Append(isProtected ? "Protected" : "").Append("MockSetup<").Append(@class.ClassFullName).Append("> setup)").AppendLine();
			sb.AppendLine("\t{");
			int count = 0;
			foreach (Property indexer in @class.AllProperties().Where(predicate))
			{
				if (count++ > 0)
				{
					sb.AppendLine();
				}

				sb.Append("\t\t/// <summary>").AppendLine();
				sb.Append("\t\t///     Sets up the ").Append(indexer.Type.Fullname).Append(" indexer on the mock for <see cref=\"").Append(@class.ClassFullName.EscapeForXmlDoc()).Append("\" />.")
					.AppendLine();
				sb.Append("\t\t/// </summary>").AppendLine();
				sb.Append("\t\tpublic IndexerSetup<").Append(indexer.Type.Fullname);
				foreach (var parameter in indexer.IndexerParameters!)
				{
					sb.Append(", ").Append(parameter.Type.Fullname);
				}
				sb.Append("> Indexer").Append("(").Append(string.Join(", ", indexer.IndexerParameters.Value.Select((p, i) => $"With.Parameter<{p.Type.Fullname}> parameter{i + 1}"))).Append(")").AppendLine();
				sb.Append("\t\t{").AppendLine();
				sb.Append("\t\t\tvar indexerSetup = new IndexerSetup<").Append(indexer.Type.Fullname);
				foreach (var parameter in indexer.IndexerParameters!)
				{
					sb.Append(", ").Append(parameter.Type.Fullname);
				}
				sb.Append(">(").Append(string.Join(", ", Enumerable.Range(1, indexer.IndexerParameters.Value.Count).Select(p => $"parameter{p}"))).Append(");").AppendLine();
				sb.Append("\t\t\t((IMockSetup)setup).RegisterIndexer(indexerSetup);").AppendLine();
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

		if (@class.AllMethods().Any(predicate))
		{
			sb.Append("\textension(").Append(isProtected ? "Protected" : "").Append("MockSetup<").Append(@class.ClassFullName).Append("> setup)").AppendLine();
			sb.AppendLine("\t{");
			sb.Append("\t\t/// <summary>").AppendLine();
			sb.Append("\t\t///     Sets up methods on the mock for <see cref=\"").Append(@class.ClassFullName.EscapeForXmlDoc()).Append("\"/>.").AppendLine();
			sb.Append("\t\t/// </summary>").AppendLine();
			sb.Append("\t\tpublic MockSetup<").Append(@class.ClassFullName).Append(">")
				.Append(isProtected ? ".Protected" : ".").Append("Methods Method").AppendLine();
			sb.Append("\t\t\t=> new MockSetup<").Append(@class.ClassFullName).Append(">")
				.Append(isProtected ? ".Protected" : ".").Append("Methods(setup);").AppendLine();
			sb.AppendLine("\t}");
			sb.AppendLine();

			sb.Append("\textension(MockSetup<").Append(@class.ClassFullName).Append(">")
				.Append(isProtected ? ".Protected" : ".").Append("Methods setup)").AppendLine();
			sb.AppendLine("\t{");
			int count = 0;
			foreach (Method method in @class.AllMethods().Where(predicate))
			{
				if (count++ > 0)
				{
					sb.AppendLine();
				}

				sb.Append("\t\t/// <summary>").AppendLine();
				sb.Append("\t\t///     Setup for the method <see cref=\"").Append(@class.ClassFullName.EscapeForXmlDoc()).Append(".")
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
						RefKind.Ref => "With.RefParameter<",
						RefKind.Out => "With.OutParameter<",
						_ => "With.Parameter<",
					}).Append(parameter.Type.Fullname)
						.Append('>');
					if (parameter.RefKind is not RefKind.Ref and not RefKind.Out)
					{
						sb.Append('?');
					}
					sb.Append(' ').Append(parameter.Name);
				}

				sb.Append(")").AppendLine();
				if (method.GenericParameters is not null && method.GenericParameters.Value.Count > 0)
				{
					foreach (GenericParameter gp in method.GenericParameters.Value)
					{
						gp.AppendWhereConstraint(sb, "\t\t\t");
					}
				}
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
				foreach (var parameter in method.Parameters)
				{
					sb.Append(", new With.NamedParameter(\"").Append(parameter.Name).Append("\", ").Append(parameter.Name);
					if (parameter.RefKind is not RefKind.Ref and not RefKind.Out)
					{
						sb.Append(" ?? With.Null<").Append(parameter.Type.Fullname)
						.Append(">()");
					}
					sb.Append(")");
				}

				sb.Append(");").AppendLine();
				sb.AppendLine("\t\t\tif (setup is IMockSetup mockSetup)");
				sb.AppendLine("\t\t\t{");
				sb.AppendLine("\t\t\t\tmockSetup.RegisterMethod(methodSetup);");
				sb.AppendLine("\t\t\t}");
				sb.AppendLine("\t\t\treturn methodSetup;");
				sb.AppendLine("\t\t}");
			}

			sb.AppendLine("\t}");
			sb.AppendLine();
		}
	}
}
#pragma warning restore S3776 // Cognitive Complexity of methods should not be too high
