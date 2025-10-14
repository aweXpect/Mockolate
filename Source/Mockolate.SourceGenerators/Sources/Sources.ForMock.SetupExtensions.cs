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
		string[] namespaces =
		[
			..GlobalUsings,
			.. @class.GetClassNamespaces(),
			"Mockolate.Events",
			"Mockolate.Protected",
			"Mockolate.Setup",
			"Mockolate.Verify",
		];
		StringBuilder sb = new();
		sb.AppendLine(Header);
		foreach (string @namespace in namespaces.Distinct().OrderBy(n => n))
		{
			sb.Append("using ").Append(@namespace).AppendLine(";");
		}

		sb.Append("""

		          namespace Mockolate;

		          #nullable enable

		          """);
		sb.AppendLine();
		sb.Append("internal static class SetupExtensionsFor").Append(name).AppendLine();
		sb.AppendLine("{");

		AppendRaisesExtensions(sb, @class, namespaces);
		AppendPropertySetupExtensions(sb, @class, namespaces);
		AppendIndexerSetupExtensions(sb, @class, namespaces);
		AppendMethodSetupExtensions(sb, @class, namespaces);

		if (@class.Events.Any(@event
			    => @event.Accessibility is not (Accessibility.Protected or Accessibility.ProtectedOrInternal)) ||
		    @class.Methods.Any(method
			    => method.Accessibility is not (Accessibility.Protected or Accessibility.ProtectedOrInternal)) ||
		    @class.Properties.Any(property
			    => property.Accessibility is not (Accessibility.Protected or Accessibility.ProtectedOrInternal)))
		{
			AppendRaisesExtensions(sb, @class, namespaces, true);
			AppendPropertySetupExtensions(sb, @class, namespaces, true);
			AppendIndexerSetupExtensions(sb, @class, namespaces, true);
			AppendMethodSetupExtensions(sb, @class, namespaces, true);
		}

		sb.AppendLine("}");
		sb.AppendLine("#nullable disable");
		return sb.ToString();
	}

	private static void AppendRaisesExtensions(StringBuilder sb, Class @class, string[] namespaces,
		bool isProtected = false)
	{
		Func<Event, bool> predicate = isProtected
			? new Func<Event, bool>(e
				=> e.Accessibility is Accessibility.Protected or Accessibility.ProtectedOrInternal)
			: new Func<Event, bool>(e
				=> e.Accessibility is not (Accessibility.Protected or Accessibility.ProtectedOrInternal));
		if (!@class.Events.Any(predicate))
		{
			return;
		}

		sb.Append("\textension(MockRaises<").Append(@class.ClassName).Append(">")
			.Append(isProtected ? ".Protected" : "").Append(" mock)").AppendLine();
		sb.AppendLine("\t{");
		int count = 0;
		foreach (Event @event in @class.Events.Where(predicate))
		{
			if (count++ > 0)
			{
				sb.AppendLine();
			}

			sb.Append("\t\t/// <summary>").AppendLine();
			sb.Append("\t\t///     Raise the <see cref=\"").Append(@class.ClassName.EscapeForXmlDoc())
				.Append(".").Append(@event.Name.EscapeForXmlDoc())
				.Append("\"/> event.").AppendLine();
			sb.Append("\t\t/// </summary>").AppendLine();
			sb.Append("\t\tpublic void ").Append(@event.Name).Append("(").Append(string.Join(", ",
					@event.Delegate.Parameters.Select(p => p.Type.GetMinimizedString(namespaces) + " " + p.Name)))
				.Append(")").AppendLine();
			sb.AppendLine("\t\t{");
			sb.Append("\t\t\t((IMockRaises)mock).Raise(\"").Append(@class.GetFullName(@event.Name)).Append("\", ")
				.Append(string.Join(", ", @event.Delegate.Parameters.Select(p => p.Name))).Append(");").AppendLine();
			sb.AppendLine("\t\t}");
		}

		sb.AppendLine("\t}");
		sb.AppendLine();
	}

	private static void AppendPropertySetupExtensions(StringBuilder sb, Class @class, string[] namespaces,
		bool isProtected = false)
	{
		Func<Property, bool> predicate = isProtected
			? new Func<Property, bool>(e
				=> !e.IsIndexer && e.Accessibility is Accessibility.Protected or Accessibility.ProtectedOrInternal)
			: new Func<Property, bool>(e
				=> !e.IsIndexer &&
				   e.Accessibility is not (Accessibility.Protected or Accessibility.ProtectedOrInternal));

		if (@class.Properties.Any(predicate))
		{
			sb.Append("\textension(MockSetup<").Append(@class.ClassName).Append(">")
				.Append(isProtected ? ".Protected" : "").Append(" setup)").AppendLine();
			sb.AppendLine("\t{");
			sb.Append("\t\t/// <summary>").AppendLine();
			sb.Append("\t\t///     Sets up properties on the mock for <see cref=\"").Append(@class.ClassName.EscapeForXmlDoc()).Append("\"/>.").AppendLine();
			sb.Append("\t\t/// </summary>").AppendLine();
			sb.Append("\t\tpublic MockSetup<").Append(@class.ClassName).Append(">")
				.Append(isProtected ? ".Protected" : ".").Append("Properties Property").AppendLine();
			sb.Append("\t\t\t=> new MockSetup<").Append(@class.ClassName).Append(">")
				.Append(isProtected ? ".Protected" : ".").Append("Properties(setup);").AppendLine();
			sb.AppendLine("\t}");
			sb.AppendLine();

			sb.Append("\textension(MockSetup<").Append(@class.ClassName).Append(">")
				.Append(isProtected ? ".Protected" : ".").Append("Properties setup)").AppendLine();
			sb.AppendLine("\t{");
			int count = 0;
			foreach (Property property in @class.Properties.Where(predicate))
			{
				if (count++ > 0)
				{
					sb.AppendLine();
				}

				sb.Append("\t\t/// <summary>").AppendLine();
				sb.Append("\t\t///     Setup for the property <see cref=\"").Append(@class.ClassName.EscapeForXmlDoc()).Append(".")
					.Append(property.Name.EscapeForXmlDoc()).Append("\"/>.").AppendLine();
				sb.Append("\t\t/// </summary>").AppendLine();
				sb.Append("\t\tpublic PropertySetup<").Append(property.Type.GetMinimizedString(namespaces)).Append("> ")
					.Append(property.IndexerParameters is not null
						? property.Name.Replace("[]",
							$"[{string.Join(", ", property.IndexerParameters.Value.Select(p => $"With.Parameter<{p.Type.GetMinimizedString(namespaces)}> {p.Name}"))}]")
						: property.Name).AppendLine();

				sb.AppendLine("\t\t{");
				sb.AppendLine("\t\t\tget");
				sb.AppendLine("\t\t\t{");
				sb.Append("\t\t\t\tvar propertySetup = new PropertySetup<").Append(property.Type.GetMinimizedString(namespaces))
					.Append(">();").AppendLine();
				sb.AppendLine("\t\t\t\tif (setup is IMockSetup mockSetup)");
				sb.AppendLine("\t\t\t\t{");
				sb.Append("\t\t\t\t\tmockSetup.RegisterProperty(\"").Append(@class.GetFullName(property.Name))
					.Append("\", propertySetup);").AppendLine();
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

	private static void AppendIndexerSetupExtensions(StringBuilder sb, Class @class, string[] namespaces,
		bool isProtected = false)
	{
		Func<Property, bool> predicate = isProtected
			? new Func<Property, bool>(e
				=> e.IsIndexer && e.Accessibility is Accessibility.Protected or Accessibility.ProtectedOrInternal)
			: new Func<Property, bool>(e
				=> e.IsIndexer && e.Accessibility is not (Accessibility.Protected or Accessibility.ProtectedOrInternal));

		if (@class.Properties.Any(predicate))
		{
			sb.Append("\textension(MockSetup<").Append(@class.ClassName).Append(">")
				.Append(isProtected ? ".Protected" : "").Append(" setup)").AppendLine();
			sb.AppendLine("\t{");
			int count = 0;
			foreach (Property indexer in @class.Properties.Where(predicate))
			{
				if (count++ > 0)
				{
					sb.AppendLine();
				}

				sb.Append("\t\t/// <summary>").AppendLine();
				sb.Append("\t\t///     Sets up the ").Append(indexer.Type.GetMinimizedString(namespaces)).Append(" indexer on the mock for <see cref=\"").Append(@class.ClassName.EscapeForXmlDoc()).Append("\" />.")
					.AppendLine();
				sb.Append("\t\t/// </summary>").AppendLine();
				sb.Append("\t\tpublic IndexerSetup<").Append(indexer.Type.GetMinimizedString(namespaces));
				foreach (var parameter in indexer.IndexerParameters!)
				{
					sb.Append(", ").Append(parameter.Type.GetMinimizedString(namespaces));
				}
				sb.Append("> Indexer").Append("(").Append(string.Join(", ", indexer.IndexerParameters.Value.Select((p, i) => $"With.Parameter<{p.Type.GetMinimizedString(namespaces)}> parameter{i + 1}"))).Append(")").AppendLine();
				sb.Append("\t\t{").AppendLine();
				sb.Append("\t\t\tvar indexerSetup = new IndexerSetup<").Append(indexer.Type.GetMinimizedString(namespaces));
				foreach (var parameter in indexer.IndexerParameters!)
				{
					sb.Append(", ").Append(parameter.Type.GetMinimizedString(namespaces));
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

	private static void AppendMethodSetupExtensions(StringBuilder sb, Class @class, string[] namespaces,
		bool isProtected = false)
	{
		Func<Method, bool> predicate = isProtected
			? new Func<Method, bool>(e
				=> e.ExplicitImplementation is null &&
				   e.Accessibility is Accessibility.Protected or Accessibility.ProtectedOrInternal)
			: new Func<Method, bool>(e
				=> e.ExplicitImplementation is null &&
				   e.Accessibility is not (Accessibility.Protected or Accessibility.ProtectedOrInternal));

		if (@class.Methods.Any(predicate))
		{
			sb.Append("\textension(MockSetup<").Append(@class.ClassName).Append(">")
				.Append(isProtected ? ".Protected" : "").Append(" setup)").AppendLine();
			sb.AppendLine("\t{");
			sb.Append("\t\t/// <summary>").AppendLine();
			sb.Append("\t\t///     Sets up methods on the mock for <see cref=\"").Append(@class.ClassName.EscapeForXmlDoc()).Append("\"/>.").AppendLine();
			sb.Append("\t\t/// </summary>").AppendLine();
			sb.Append("\t\tpublic MockSetup<").Append(@class.ClassName).Append(">")
				.Append(isProtected ? ".Protected" : ".").Append("Methods Method").AppendLine();
			sb.Append("\t\t\t=> new MockSetup<").Append(@class.ClassName).Append(">")
				.Append(isProtected ? ".Protected" : ".").Append("Methods(setup);").AppendLine();
			sb.AppendLine("\t}");
			sb.AppendLine();

			sb.Append("\textension(MockSetup<").Append(@class.ClassName).Append(">")
				.Append(isProtected ? ".Protected" : ".").Append("Methods setup)").AppendLine();
			sb.AppendLine("\t{");
			int count = 0;
			foreach (Method method in @class.Methods.Where(predicate))
			{
				if (count++ > 0)
				{
					sb.AppendLine();
				}

				sb.Append("\t\t/// <summary>").AppendLine();
				sb.Append("\t\t///     Setup for the method <see cref=\"").Append(@class.ClassName.EscapeForXmlDoc()).Append(".")
					.Append(method.Name.EscapeForXmlDoc()).Append("(")
					.Append(string.Join(", ",
						method.Parameters.Select(p => p.RefKind.GetString() + p.Type.GetMinimizedString(namespaces))))
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
						.Append(method.ReturnType.GetMinimizedString(namespaces));
					foreach (MethodParameter parameter in method.Parameters)
					{
						sb.Append(", ").Append(parameter.Type.GetMinimizedString(namespaces));
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

							sb.Append(parameter.Type.GetMinimizedString(namespaces));
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
					}).Append(parameter.Type.GetMinimizedString(namespaces))
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
						sb.AppendLine();
						sb.Append("\t\t\t");
						gp.AppendWhereConstraint(sb, namespaces);
					}
				}
				sb.AppendLine("\t\t{");

				if (method.ReturnType != Type.Void)
				{
					sb.Append("\t\t\tvar methodSetup = new ReturnMethodSetup<")
						.Append(method.ReturnType.GetMinimizedString(namespaces));
					foreach (MethodParameter parameter in method.Parameters)
					{
						sb.Append(", ").Append(parameter.Type.GetMinimizedString(namespaces));
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

							sb.Append(parameter.Type.GetMinimizedString(namespaces));
						}

						sb.Append('>');
					}
				}

				sb.Append("(\"").Append(@class.GetFullName(method.Name)).Append("\"");
				foreach (var parameter in method.Parameters)
				{
					sb.Append(", new With.NamedParameter(\"").Append(parameter.Name).Append("\", ").Append(parameter.Name);
					if (parameter.RefKind is not RefKind.Ref and not RefKind.Out)
					{
						sb.Append(" ?? With.Null<").Append(parameter.Type.GetMinimizedString(namespaces))
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
