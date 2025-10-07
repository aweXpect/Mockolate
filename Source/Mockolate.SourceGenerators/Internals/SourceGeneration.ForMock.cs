using System.Text;
using Microsoft.CodeAnalysis;
using Mockolate.SourceGenerators.Entities;
using Type = Mockolate.SourceGenerators.Entities.Type;

namespace Mockolate.SourceGenerators.Internals;

#pragma warning disable S3776 // Cognitive Complexity of methods should not be too high
internal static partial class SourceGeneration
{
	public static string ForMock(string name, MockClass mockClass)
	{
		string[] namespaces =
		[
			.. mockClass.GetAllNamespaces(),
			"Mockolate.Checks",
			"Mockolate.Events",
			"Mockolate.Protected",
			"Mockolate.Setup",
		];
		StringBuilder sb = new();
		sb.AppendLine(Header);
		foreach (string @namespace in namespaces.Distinct().OrderBy(n => n))
		{
			sb.Append("using ").Append(@namespace).AppendLine(";");
		}

		sb.Append("""

		          namespace Mockolate.Generated;

		          #nullable enable

		          """);
		sb.Append("public static class For").Append(name).AppendLine();
		sb.AppendLine("{");

		AppendMock(sb, mockClass);
		sb.AppendLine();

		AppendMockObject(sb, mockClass, namespaces);
		sb.AppendLine("}");
		sb.AppendLine("#nullable disable");
		return sb.ToString();
	}

	private static void AppendMock(StringBuilder sb, MockClass mockClass)
	{
		sb.Append("\t/// <summary>").AppendLine();
		sb.Append("\t///     The mock class for <see cref=\"").Append(mockClass.ClassName).Append("\" />");
		foreach (Class? additional in mockClass.AdditionalImplementations)
		{
			sb.Append(" and <see cref=\"").Append(additional.ClassName).Append("\" />");
		}

		sb.AppendLine(".");
		sb.Append("\t/// </summary>").AppendLine();
		sb.Append("\tpublic class Mock : Mock<").Append(mockClass.ClassName);
		foreach (Class? item in mockClass.AdditionalImplementations)
		{
			sb.Append(", ").Append(item.ClassName);
		}

		sb.AppendLine(">");
		sb.AppendLine("\t{");
		sb.Append("\t\t/// <inheritdoc cref=\"Mock\" />").AppendLine();
		sb.Append(
				"\t\tpublic Mock(BaseClass.ConstructorParameters? constructorParameters, MockBehavior mockBehavior) : base(mockBehavior)")
			.AppendLine();
		sb.AppendLine("\t\t{");
		sb.Append("\t\t\tObject = Create<MockObject>(constructorParameters);").AppendLine();
		sb.AppendLine("\t\t}");
		sb.AppendLine();
		sb.Append("\t\t/// <inheritdoc cref=\"Mock{").Append(mockClass.ClassName)
			.Append(string.Join(", ", mockClass.AdditionalImplementations.Select(x => x.ClassName)))
			.AppendLine("}.Object\" />");
		sb.Append("\t\tpublic override ").Append(mockClass.ClassName).AppendLine(" Object { get; }");
		sb.AppendLine("\t}");
	}

	private static void AppendMockObject(StringBuilder sb, MockClass mockClass, string[] namespaces)
	{
		sb.Append("\t/// <summary>").AppendLine();
		sb.Append("\t///     The actual mock object implementing <see cref=\"").Append(mockClass.ClassName)
			.Append("\" />");
		foreach (Class? additional in mockClass.AdditionalImplementations)
		{
			sb.Append(" and <see cref=\"").Append(additional.ClassName).Append("\" />");
		}

		sb.AppendLine(".");
		sb.Append("\t/// </summary>").AppendLine();
		sb.Append("\tpublic partial class MockObject : ").Append(mockClass.ClassName);
		foreach (Class? additional in mockClass.AdditionalImplementations)
		{
			sb.Append(",").AppendLine();
			sb.Append("\t\t").Append(additional.ClassName);
		}

		sb.AppendLine().AppendLine("\t{");
		sb.AppendLine("\t\tprivate IMock _mock;");
		sb.AppendLine();
		sb.Append("\t\t/// <inheritdoc cref=\"MockObject\" />").AppendLine();
		if (mockClass.IsInterface || mockClass.Constructors?.All(m => m.Parameters.Count == 0) != false)
		{
			sb.AppendLine("\t\tpublic MockObject(IMock mock)");
			sb.AppendLine("\t\t{");
			sb.AppendLine("\t\t\t_mock = mock;");
			sb.AppendLine("\t\t}");
		}
		else
		{
			foreach (Method constructor in mockClass.Constructors)
			{
				sb.Append("\t\tpublic MockObject(IMock mock");
				foreach (MethodParameter parameter in constructor.Parameters)
				{
					sb.Append(", ");
					sb.Append(parameter.Type.GetMinimizedString(namespaces)).Append(' ').Append(parameter.Name);
				}

				sb.AppendLine(")");
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

				sb.AppendLine(")");
				sb.AppendLine("\t\t{");
				sb.AppendLine("\t\t\t_mock = mock;");
				sb.AppendLine("\t\t}");
			}
		}

		sb.AppendLine();
		ImplementClass(sb, mockClass, namespaces, false);
		foreach (Class? additional in mockClass.AdditionalImplementations)
		{
			sb.AppendLine();
			ImplementClass(sb, additional, namespaces, true);
		}

		sb.AppendLine("\t}");
	}
}
#pragma warning restore S3776 // Cognitive Complexity of methods should not be too high
