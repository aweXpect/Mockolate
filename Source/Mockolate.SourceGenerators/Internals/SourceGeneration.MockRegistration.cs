using System.Security.Claims;
using System.Text;
using Mockolate.SourceGenerators.Entities;

namespace Mockolate.SourceGenerators.Internals;

internal static partial class SourceGeneration
{
	public static string MockRegistration(ICollection<(string Name, MockClass MockClass)> mocks)
	{
		List<string> namespaces =
		[
			.. mocks.SelectMany(m => m.MockClass.GetAllNamespaces()),
			"System",
		];
		if (mocks.Any())
		{
			namespaces.Add("Mockolate.Generated");
		}
		StringBuilder sb = new();
		sb.AppendLine(Header);
		foreach (string @namespace in namespaces.Distinct().OrderBy(n => n))
		{
			sb.Append("using ").Append(@namespace).AppendLine(";");
		}

		sb.AppendLine();
		sb.AppendLine("namespace Mockolate;");
		sb.AppendLine();
		sb.AppendLine("#nullable enable");
		sb.AppendLine("internal static partial class Mock");
		sb.AppendLine("{");
		sb.AppendLine("\tprivate partial class MockGenerator");
		sb.AppendLine("\t{");
		sb.AppendLine(
			"\t\tpartial void Generate(BaseClass.ConstructorParameters? constructorParameters, MockBehavior mockBehavior, params Type[] types)");
		sb.AppendLine("\t\t{");
		int index = 0;
		foreach ((string Name, MockClass MockClass) mock in mocks)
		{
			string prefix;
			if (index++ > 0)
			{
				sb.Append("\t\t\telse ");
				prefix = "\t\t\t         ";
			}
			else
			{
				sb.Append("\t\t\t");
				prefix = "\t\t\t    ";
			}

			sb.Append("if (types.Length == ").Append(mock.MockClass.AdditionalImplementations.Count + 1).Append(" &&")
				.AppendLine();
			sb.Append(prefix).Append("types[0] == typeof(").Append(mock.MockClass.ClassName).Append(")");
			int idx = 1;
			foreach (Class? item in mock.MockClass.AdditionalImplementations)
			{
				sb.AppendLine(" &&");
				sb.Append(prefix).Append("types[").Append(idx++).Append("] == typeof(").Append(item.ClassName)
					.Append(")");
			}

			sb.AppendLine(")");
			sb.Append("\t\t\t{").AppendLine();
			sb.Append("\t\t\t\t_value = new For").Append(mock.Name)
				.Append(".Mock(constructorParameters, mockBehavior);").AppendLine();
			sb.Append("\t\t\t}").AppendLine();
		}

		sb.AppendLine("\t\t}");
		sb.AppendLine("\t}");
		sb.AppendLine("}");
		sb.AppendLine("#nullable disable");
		return sb.ToString();
	}
}
