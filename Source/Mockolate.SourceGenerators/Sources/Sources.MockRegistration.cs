using System.Text;
using Mockolate.SourceGenerators.Entities;
using Mockolate.SourceGenerators.Internals;

namespace Mockolate.SourceGenerators.Sources;

#pragma warning disable S3776 // Cognitive Complexity of methods should not be too high
internal static partial class Sources
{
	public static string ForMockRegistration(ICollection<(string Name, MockClass MockClass)> mocks)
	{
		StringBuilder sb = InitializeBuilder();
		sb.Append("""
		          namespace Mockolate;

		          #nullable enable

		          internal static partial class MockCreationExtensions
		          {
		          	extension(Mock _)
		          	{
		          """);

		foreach (var mock in mocks.GroupBy(m => m.MockClass.ClassFullName))
		{
			var mockClass = mock.First().MockClass;
			var name = mock.First().Name;
			sb.AppendXmlSummary($"Create a new mock for <see cref=\"{mockClass.ClassFullName}\" /> with the default <see cref=\"global::Mockolate.MockBehavior\" />.");
			sb.Append("\t\tpublic static T Create<T>(global::Mockolate.MockBehavior? mockBehavior = null) where T : ")
				.AppendLine(mockClass.ClassFullName);
			sb.Append("\t\t{").AppendLine();
			sb.Append("\t\t\tvar value = new global::Mockolate.Generated.MockFor").Append(name).Append("(mockBehavior ?? global::Mockolate.MockBehavior.Default);").AppendLine();
			sb.Append("\t\t\tif (value is T mock)").AppendLine();
			sb.Append("\t\t\t{").AppendLine();
			sb.Append("\t\t\t\treturn mock;").AppendLine();
			sb.Append("\t\t\t}").AppendLine();
			sb.Append("\t\t\tthrow new global::Mockolate.Exceptions.MockException(\"Could not generate Mock<T>. Did the source generator run correctly?\");").AppendLine();
			sb.Append("\t\t}").AppendLine();
			sb.AppendLine();
		}
		/*
		if (mocks.Any())
		{
			sb.AppendLine("\tprivate partial class MockGenerator");
			sb.AppendLine("\t{");
			sb.AppendLine(
				"\t\tpartial void Generate<T>(global::Mockolate.BaseClass.ConstructorParameters? constructorParameters, global::Mockolate.MockBehavior mockBehavior, global::System.Action<global::Mockolate.Setup.IMockSetup<T>>[] setups, params global::System.Type[] types)");
			sb.Append("\t\t{").AppendLine();
			sb.Append("\t\t\tIMockBehaviorAccess mockBehaviorAccess = (global::Mockolate.IMockBehaviorAccess)mockBehavior;").AppendLine();
			sb.Append("\t\t\tif (mockBehaviorAccess.TryInitialize<T>(out global::System.Action<global::Mockolate.Setup.IMockSetup<T>>[]? additionalSetups))")
				.AppendLine();
			sb.Append("\t\t\t{").AppendLine();
			sb.Append("\t\t\t\tif (setups.Length > 0)").AppendLine();
			sb.Append("\t\t\t\t{").AppendLine();
			sb.Append(
					"\t\t\t\t\tglobal::System.Action<global::Mockolate.Setup.IMockSetup<T>>[] concatenatedSetups = new global::System.Action<global::Mockolate.Setup.IMockSetup<T>>[additionalSetups.Length + setups.Length];")
				.AppendLine();
			sb.Append("\t\t\t\t\tadditionalSetups.CopyTo(concatenatedSetups, 0);").AppendLine();
			sb.Append("\t\t\t\t\tsetups.CopyTo(concatenatedSetups, additionalSetups.Length);").AppendLine();
			sb.Append("\t\t\t\t\tsetups = concatenatedSetups;").AppendLine();
			sb.Append("\t\t\t\t}").AppendLine();
			sb.Append("\t\t\t\telse").AppendLine();
			sb.Append("\t\t\t\t{").AppendLine();
			sb.Append("\t\t\t\t\tsetups = additionalSetups;").AppendLine();
			sb.Append("\t\t\t\t}").AppendLine();
			sb.Append("\t\t\t}").AppendLine();
			sb.AppendLine();
			sb.Append(
					"\t\t\tif (constructorParameters is null && mockBehaviorAccess.TryGetConstructorParameters<T>(out object?[]? parameters))")
				.AppendLine();
			sb.Append("\t\t\t{").AppendLine();
			sb.Append("\t\t\t\tconstructorParameters = new BaseClass.ConstructorParameters(parameters);").AppendLine();
			sb.Append("\t\t\t}").AppendLine();
			sb.AppendLine();
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

				sb.Append("if (types.Length == ").Append(mock.MockClass.AdditionalImplementations.Count + 1)
					.Append(" &&")
					.AppendLine();
				sb.Append(prefix).Append("types[0] == typeof(").Append(mock.MockClass.ClassFullName).Append(")");
				int idx = 1;
				foreach (Class? item in mock.MockClass.AdditionalImplementations)
				{
					sb.AppendLine(" &&");
					sb.Append(prefix).Append("types[").Append(idx++).Append("] == typeof(").Append(item.ClassFullName)
						.Append(")");
				}

				sb.AppendLine(")");
				sb.Append("\t\t\t{").AppendLine();
				if (mock.MockClass.AdditionalImplementations.Any(x => !x.IsInterface))
				{
					List<Class> incorrectDeclarations = mock.MockClass.AdditionalImplementations
						.Where(x => !x.IsInterface).ToList();
					sb.Append("\t\t\t\tthrow new global::Mockolate.Exceptions.MockException($\"The mock declaration has ")
						.Append(incorrectDeclarations.Count)
						.Append(" additional ")
						.Append(incorrectDeclarations.Count == 1
							? "implementation that is not an interface: "
							: "implementations that are not interfaces: ")
						.Append(string.Join(", ", incorrectDeclarations.Select(x => x.ClassFullName)))
						.Append("\");")
						.AppendLine();
				}
				else if (mock.MockClass.Delegate != null)
				{
					sb.Append("\t\t\t\tglobal::Mockolate.Generated.MockFor").Append(mock.Name).Append(" mockTarget = new global::Mockolate.Generated.MockFor").Append(mock.Name)
						.Append("(mockBehavior);")
						.AppendLine();
					sb.Append("\t\t\t\tif (setups.Length > 0)").AppendLine();
					sb.Append("\t\t\t\t{").AppendLine();
					sb.Append("\t\t\t\t\tglobal::Mockolate.Setup.IMockSetup<").Append(mock.MockClass.ClassFullName)
						.Append("> setupTarget = ((global::Mockolate.IMockSubject<").Append(mock.MockClass.ClassFullName)
						.Append(">)mockTarget).Mock;").AppendLine();
					sb.Append("\t\t\t\t\tforeach (global::System.Action<global::Mockolate.Setup.IMockSetup<").Append(mock.MockClass.ClassFullName)
						.Append(">> setup in setups)").AppendLine();
					sb.Append("\t\t\t\t\t{").AppendLine();
					sb.Append("\t\t\t\t\t\tsetup.Invoke(setupTarget);").AppendLine();
					sb.Append("\t\t\t\t\t}").AppendLine();
					sb.Append("\t\t\t\t}").AppendLine();
					sb.Append("\t\t\t\t_value = mockTarget.Object;").AppendLine();
				}
				else if (mock.MockClass.IsInterface)
				{
					sb.Append("\t\t\t\t_value = new global::Mockolate.Generated.MockFor").Append(mock.Name).Append("(mockBehavior);").AppendLine();
					sb.Append("\t\t\t\tif (setups.Length > 0)").AppendLine();
					sb.Append("\t\t\t\t{").AppendLine();
					sb.Append("\t\t\t\t\tglobal::Mockolate.Setup.IMockSetup<").Append(mock.MockClass.ClassFullName)
						.Append("> setupTarget = ((global::Mockolate.IMockSubject<").Append(mock.MockClass.ClassFullName)
						.Append(">)_value).Mock;").AppendLine();
					sb.Append("\t\t\t\t\tforeach (global::System.Action<global::Mockolate.Setup.IMockSetup<").Append(mock.MockClass.ClassFullName)
						.Append(">> setup in setups)").AppendLine();
					sb.Append("\t\t\t\t\t{").AppendLine();
					sb.Append("\t\t\t\t\t\tsetup.Invoke(setupTarget);").AppendLine();
					sb.Append("\t\t\t\t\t}").AppendLine();
					sb.Append("\t\t\t\t}").AppendLine();
				}
				else if (mock.MockClass.Constructors?.Count > 0)
				{
					sb.Append(
							"\t\t\t\tif (constructorParameters is null || constructorParameters.Parameters.Length == 0)")
						.AppendLine();
					sb.Append("\t\t\t\t{").AppendLine();
					if (mock.MockClass.Constructors.Value.Any(mockClass => mockClass.Parameters.Count == 0))
					{
						sb.Append("\t\t\t\t\tglobal::Mockolate.MockRegistration mockRegistration = new global::Mockolate.MockRegistration(mockBehavior, \"")
							.Append(mock.MockClass.DisplayString).Append("\");").AppendLine();
						sb.Append("\t\t\t\t\tglobal::Mockolate.Generated.MockFor").Append(mock.Name)
							.Append(".MockRegistrationsProvider.Value = mockRegistration;").AppendLine();
						sb.Append("\t\t\t\t\tif (setups.Length > 0)").AppendLine();
						sb.Append("\t\t\t\t\t{").AppendLine();
						sb.Append("\t\t\t\t\t\t#pragma warning disable CS0618").AppendLine();
						sb.Append("\t\t\t\t\t\tglobal::Mockolate.Setup.IMockSetup<").Append(mock.MockClass.ClassFullName)
							.Append("> setupTarget = new global::Mockolate.MockSetup<").Append(mock.MockClass.ClassFullName)
							.Append(">(mockRegistration);").AppendLine();
						sb.Append("\t\t\t\t\t\t#pragma warning restore CS0618").AppendLine();
						sb.Append("\t\t\t\t\t\tforeach (global::System.Action<global::Mockolate.Setup.IMockSetup<").Append(mock.MockClass.ClassFullName)
							.Append(">> setup in setups)").AppendLine();
						sb.Append("\t\t\t\t\t\t{").AppendLine();
						sb.Append("\t\t\t\t\t\t\tsetup.Invoke(setupTarget);").AppendLine();
						sb.Append("\t\t\t\t\t\t}").AppendLine();
						sb.Append("\t\t\t\t\t}").AppendLine();
						sb.Append("\t\t\t\t\t_value = new global::Mockolate.Generated.MockFor").Append(mock.Name).Append("(mockRegistration);")
							.AppendLine();
					}
					else
					{
						sb.Append("\t\t\t\t\tthrow new global::Mockolate.Exceptions.MockException(\"No parameterless constructor found for '")
							.Append(mock.MockClass.ClassFullName)
							.Append("'. Please provide constructor parameters.\");")
							.AppendLine();
					}

					sb.Append("\t\t\t\t}").AppendLine();
					int constructorIndex = 0;
					foreach (EquatableArray<MethodParameter> constructorParameters in mock.MockClass.Constructors.Value
						         .Select(constructor => constructor.Parameters))
					{
						constructorIndex++;
						int requiredParameters = constructorParameters.Count(c => !c.HasExplicitDefaultValue);
						if (requiredParameters < constructorParameters.Count)
						{
							sb.Append("\t\t\t\telse if (constructorParameters.Parameters.Length >= ")
								.Append(requiredParameters).Append(" && constructorParameters.Parameters.Length <= ")
								.Append(constructorParameters.Count);
						}
						else
						{
							sb.Append("\t\t\t\telse if (constructorParameters.Parameters.Length == ")
								.Append(constructorParameters.Count);
						}

						int constructorParameterIndex = 0;
						foreach (MethodParameter parameter in constructorParameters)
						{
							sb.AppendLine().Append("\t\t\t\t    && TryCast(constructorParameters.Parameters, ")
								.Append(constructorParameterIndex++)
								.Append(parameter.HasExplicitDefaultValue ? $", {parameter.ExplicitDefaultValue}" : "")
								.Append(", mockBehavior, out ").Append(parameter.Type.Fullname).Append(" c")
								.Append(constructorIndex)
								.Append('p')
								.Append(constructorParameterIndex).Append(")");
						}

						sb.Append(")").AppendLine();
						sb.Append("\t\t\t\t{").AppendLine();
						sb.Append("\t\t\t\t\tglobal::Mockolate.MockRegistration mockRegistration = new global::Mockolate.MockRegistration(mockBehavior, \"")
							.Append(mock.MockClass.DisplayString).Append("\");").AppendLine();
						sb.Append("\t\t\t\t\tglobal::Mockolate.Generated.MockFor").Append(mock.Name)
							.Append(".MockRegistrationsProvider.Value = mockRegistration;").AppendLine();
						sb.Append("\t\t\t\t\tif (setups.Length > 0)").AppendLine();
						sb.Append("\t\t\t\t\t{").AppendLine();
						sb.Append("\t\t\t\t\t\t#pragma warning disable CS0618").AppendLine();
						sb.Append("\t\t\t\t\t\tglobal::Mockolate.Setup.IMockSetup<").Append(mock.MockClass.ClassFullName)
							.Append("> setupTarget = new global::Mockolate.MockSetup<").Append(mock.MockClass.ClassFullName)
							.Append(">(mockRegistration);").AppendLine();
						sb.Append("\t\t\t\t\t\t#pragma warning restore CS0618").AppendLine();
						sb.Append("\t\t\t\t\t\tforeach (global::System.Action<global::Mockolate.Setup.IMockSetup<").Append(mock.MockClass.ClassFullName)
							.Append(">> setup in setups)").AppendLine();
						sb.Append("\t\t\t\t\t\t{").AppendLine();
						sb.Append("\t\t\t\t\t\t\tsetup.Invoke(setupTarget);").AppendLine();
						sb.Append("\t\t\t\t\t\t}").AppendLine();
						sb.Append("\t\t\t\t\t}").AppendLine();
						sb.Append("\t\t\t\t\t_value = new global::Mockolate.Generated.MockFor").Append(mock.Name).Append("(mockRegistration");
						for (int i = 1; i <= constructorParameters.Count; i++)
						{
							sb.Append(", ").Append('c').Append(constructorIndex).Append('p').Append(i);
						}

						sb.Append(");").AppendLine();
						sb.Append("\t\t\t\t}").AppendLine();
					}

					sb.Append("\t\t\t\telse").AppendLine();
					sb.Append("\t\t\t\t{").AppendLine();
					sb.Append("\t\t\t\t\tthrow new global::Mockolate.Exceptions.MockException($\"Could not find any constructor for '")
						.Append(mock.MockClass.ClassFullName)
						.Append(
							"' that matches the {constructorParameters.Parameters.Length} given parameters ({string.Join(\", \", constructorParameters.Parameters)}).\");")
						.AppendLine();
					sb.Append("\t\t\t\t}").AppendLine();
				}
				else
				{
					sb.Append(
							"\t\t\t\tthrow new global::Mockolate.Exceptions.MockException(\"Could not find any constructor at all for the base type '")
						.Append(mock.MockClass.ClassFullName)
						.Append("'. Therefore mocking is not supported!\");")
						.AppendLine();
				}

				sb.Append("\t\t\t}").AppendLine();
			}

			sb.AppendLine("\t\t}");

			sb.AppendLine();
			sb.AppendLine(
				"\t\tpartial void GenerateWrapped<T>(T instance, global::Mockolate.MockBehavior mockBehavior, global::System.Action<global::Mockolate.Setup.IMockSetup<T>>[] setups)");
			sb.Append("\t\t{").AppendLine();
			index = 0;
			foreach ((string Name, MockClass MockClass) mock in mocks.Where(m
				         => m.MockClass.AdditionalImplementations.Count == 0))
			{
				if (index++ > 0)
				{
					sb.Append("\t\t\telse ");
				}
				else
				{
					sb.Append("\t\t\t");
				}

				sb.Append("if (typeof(T) == typeof(").Append(mock.MockClass.ClassFullName).Append("))").AppendLine();
				sb.Append("\t\t\t{").AppendLine();

				sb.Append("\t\t\t\tglobal::Mockolate.MockRegistration mockRegistration = new global::Mockolate.MockRegistration(mockBehavior, \"")
					.Append(mock.MockClass.DisplayString).Append("\");").AppendLine();

				if (mock.MockClass.IsInterface)
				{
					sb.Append("\t\t\t\t_value = new global::Mockolate.Generated.MockFor").Append(mock.Name).Append("(mockBehavior, instance as ")
						.Append(mock.MockClass.ClassFullName).Append(");").AppendLine();
					sb.Append("\t\t\t\tif (setups.Length > 0)").AppendLine();
					sb.Append("\t\t\t\t{").AppendLine();
					sb.Append("\t\t\t\t\tglobal::Mockolate.Setup.IMockSetup<").Append(mock.MockClass.ClassFullName)
						.Append("> setupTarget = ((global::Mockolate.IMockSubject<").Append(mock.MockClass.ClassFullName)
						.Append(">)_value).Mock;").AppendLine();
					sb.Append("\t\t\t\t\tforeach (global::System.Action<global::Mockolate.Setup.IMockSetup<").Append(mock.MockClass.ClassFullName)
						.Append(">> setup in setups)").AppendLine();
					sb.Append("\t\t\t\t\t{").AppendLine();
					sb.Append("\t\t\t\t\t\tsetup.Invoke(setupTarget);").AppendLine();
					sb.Append("\t\t\t\t\t}").AppendLine();
					sb.Append("\t\t\t\t}").AppendLine();
				}

				sb.Append("\t\t\t}").AppendLine();
			}

			sb.AppendLine("\t\t}");

			sb.AppendLine("\t}");
			sb.AppendLine();
		}
		*/

		sb.Append("""
		          	}

		          	private static bool TryCast<TValue>(object?[] values, int index, global::Mockolate.MockBehavior behavior, out TValue result)
		          	{
		          	    var value = values[index];
		          		if (value is TValue typedValue)
		          		{
		          			result = typedValue;
		          			return true;
		          		}
		          		
		          		result = default!;
		          		return value is null;
		          	}
		          	private static bool TryCast<TValue>(object?[] values, int index, TValue defaultValue, global::Mockolate.MockBehavior behavior, out TValue result)
		          	{
		          		if (values.Length > index && values[index] is TValue typedValue)
		          		{
		          			result = typedValue;
		          			return true;
		          		}
		          		
		          		result = defaultValue;
		          		return true;
		          	}
		          """).AppendLine();
		sb.AppendLine("}");
		sb.AppendLine("#nullable disable");
		return sb.ToString();
	}
}
#pragma warning restore S3776 // Cognitive Complexity of methods should not be too high
