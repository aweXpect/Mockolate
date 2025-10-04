using System.Text;
using Mockolate.SourceGenerators.Entities;

namespace Mockolate.SourceGenerators.Internals;

internal static partial class SourceGeneration
{
	public static string RegisterMocks(ICollection<(string Name, MockClass MockClass)> mocks)
	{
		var sb = new StringBuilder();
		sb.AppendLine(Header);
		sb.AppendLine("using System;");
		foreach (string? @namespace in mocks.Select(x => x.MockClass.Namespace).Where(x => x != "System").Distinct().OrderBy(x => x))
		{
			sb.Append("using ").Append(@namespace).AppendLine(";");
		}

		sb.AppendLine();
		sb.AppendLine("namespace Mockolate;");
		sb.AppendLine();
		sb.AppendLine("#nullable enable");
		sb.AppendLine("public static partial class Mock");
		sb.AppendLine("{");
		sb.AppendLine("\tprivate partial class MockGenerator");
		sb.AppendLine("\t{");
		sb.AppendLine("\t\tpartial void Generate(BaseClass.ConstructorParameters? constructorParameters, MockBehavior mockBehavior, params Type[] types)");
		sb.AppendLine("\t\t{");
		int index = 0;
		foreach (var mock in mocks)
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
			foreach (var item in mock.MockClass.AdditionalImplementations)
			{
				sb.AppendLine(" &&");
				sb.Append(prefix).Append("types[").Append(idx++).Append("] == typeof(").Append(item.ClassName).Append(")");
			}
			sb.AppendLine(")");
			sb.Append("\t\t\t{").AppendLine();
			sb.Append("\t\t\t\t_value = new For").Append(mock.Name).Append(".Mock(constructorParameters, mockBehavior);").AppendLine();
			sb.Append("\t\t\t}").AppendLine();
		}

		sb.AppendLine("\t\t}");
		sb.AppendLine("\t}");
		sb.AppendLine("}");
		sb.AppendLine("#nullable disable");
		return sb.ToString();
	}
}
