using System.Text;
using Mockolate.SourceGenerators.Entities;
using Mockolate.SourceGenerators.Internals;

namespace Mockolate.SourceGenerators.Sources;

#pragma warning disable S3776 // Cognitive Complexity of methods should not be too high
internal static partial class Sources
{
	public static string MockRegistration(ICollection<(string Name, MockClass MockClass)> mocks, bool hasAnyMocks)
	{
		StringBuilder sb = InitializeBuilder(hasAnyMocks
			?
			[
				"System",
				"System.Linq",
				"System.Threading",
				"Mockolate.Exceptions",
				"Mockolate.Generated",
				"Mockolate.Setup",
			]
			:
			[
				"System",
				"System.Linq",
				"System.Threading",
			]);

		sb.Append("""
		          namespace Mockolate;

		          #nullable enable

		          """);
		sb.AppendLine("internal static partial class Mock");
		sb.AppendLine("{");
		if (mocks.Any())
		{
			sb.AppendLine("\tprivate partial class MockGenerator");
			sb.AppendLine("\t{");
			sb.AppendLine(
				"\t\tpartial void Generate<T>(BaseClass.ConstructorParameters? constructorParameters, MockBehavior mockBehavior, Action<IMockSetup<T>>[] setups, params Type[] types)");
			sb.Append("\t\t{").AppendLine();
			sb.Append("\t\t\tif (mockBehavior.TryInitialize<T>(out Action<IMockSetup<T>>[]? additionalSetups))")
				.AppendLine();
			sb.Append("\t\t\t{").AppendLine();
			sb.Append("\t\t\t\tif (setups.Length > 0)").AppendLine();
			sb.Append("\t\t\t\t{").AppendLine();
			sb.Append(
					"\t\t\t\t\tAction<IMockSetup<T>>[] concatenatedSetups = new Action<IMockSetup<T>>[additionalSetups.Length + setups.Length];")
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
					sb.Append("\t\t\t\tthrow new MockException($\"The mock declaration has ")
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
					sb.Append("\t\t\t\tMockFor").Append(mock.Name).Append(" mockTarget = new MockFor").Append(mock.Name)
						.Append("(mockBehavior);")
						.AppendLine();
					sb.Append("\t\t\t\tif (setups.Length > 0)").AppendLine();
					sb.Append("\t\t\t\t{").AppendLine();
					sb.Append("\t\t\t\t\tIMockSetup<").Append(mock.MockClass.ClassFullName)
						.Append("> setupTarget = ((IMockSubject<").Append(mock.MockClass.ClassFullName)
						.Append(">)mockTarget).Mock;").AppendLine();
					sb.Append("\t\t\t\t\tforeach (Action<IMockSetup<").Append(mock.MockClass.ClassFullName)
						.Append(">> setup in setups)").AppendLine();
					sb.Append("\t\t\t\t\t{").AppendLine();
					sb.Append("\t\t\t\t\t\tsetup.Invoke(setupTarget);").AppendLine();
					sb.Append("\t\t\t\t\t}").AppendLine();
					sb.Append("\t\t\t\t}").AppendLine();
					sb.Append("\t\t\t\t_value = mockTarget.Object;").AppendLine();
				}
				else if (mock.MockClass.IsInterface)
				{
					sb.Append("\t\t\t\t_value = new MockFor").Append(mock.Name).Append("(mockBehavior);").AppendLine();
					sb.Append("\t\t\t\tif (setups.Length > 0)").AppendLine();
					sb.Append("\t\t\t\t{").AppendLine();
					sb.Append("\t\t\t\t\tIMockSetup<").Append(mock.MockClass.ClassFullName)
						.Append("> setupTarget = ((IMockSubject<").Append(mock.MockClass.ClassFullName)
						.Append(">)_value).Mock;").AppendLine();
					sb.Append("\t\t\t\t\tforeach (Action<IMockSetup<").Append(mock.MockClass.ClassFullName)
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
						sb.Append("\t\t\t\t\tMockRegistration mockRegistration = new MockRegistration(mockBehavior, \"")
							.Append(mock.MockClass.DisplayString).Append("\");").AppendLine();
						sb.Append("\t\t\t\t\tMockFor").Append(mock.Name)
							.Append(".MockRegistrationsProvider.Value = mockRegistration;").AppendLine();
						sb.Append("\t\t\t\t\tif (setups.Length > 0)").AppendLine();
						sb.Append("\t\t\t\t\t{").AppendLine();
						sb.Append("\t\t\t\t\t\t#pragma warning disable CS0618").AppendLine();
						sb.Append("\t\t\t\t\t\tIMockSetup<").Append(mock.MockClass.ClassFullName)
							.Append("> setupTarget = new MockSetup<").Append(mock.MockClass.ClassFullName)
							.Append(">(mockRegistration);").AppendLine();
						sb.Append("\t\t\t\t\t\t#pragma warning restore CS0618").AppendLine();
						sb.Append("\t\t\t\t\t\tforeach (Action<IMockSetup<").Append(mock.MockClass.ClassFullName)
							.Append(">> setup in setups)").AppendLine();
						sb.Append("\t\t\t\t\t\t{").AppendLine();
						sb.Append("\t\t\t\t\t\t\tsetup.Invoke(setupTarget);").AppendLine();
						sb.Append("\t\t\t\t\t\t}").AppendLine();
						sb.Append("\t\t\t\t\t}").AppendLine();
						sb.Append("\t\t\t\t\t_value = new MockFor").Append(mock.Name).Append("(mockRegistration);")
							.AppendLine();
					}
					else
					{
						sb.Append("\t\t\t\t\tthrow new MockException(\"No parameterless constructor found for '")
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
						sb.Append("\t\t\t\telse if (constructorParameters.Parameters.Length == ")
							.Append(constructorParameters.Count);
						int constructorParameterIndex = 0;
						foreach (MethodParameter parameter in constructorParameters)
						{
							sb.AppendLine().Append("\t\t\t\t    && TryCast(constructorParameters.Parameters[")
								.Append(constructorParameterIndex++)
								.Append("], mockBehavior, out ").Append(parameter.Type.Fullname).Append(" c")
								.Append(constructorIndex)
								.Append('p')
								.Append(constructorParameterIndex).Append(")");
						}

						sb.Append(")").AppendLine();
						sb.Append("\t\t\t\t{").AppendLine();
						sb.Append("\t\t\t\t\tMockRegistration mockRegistration = new MockRegistration(mockBehavior, \"")
							.Append(mock.MockClass.DisplayString).Append("\");").AppendLine();
						sb.Append("\t\t\t\t\tMockFor").Append(mock.Name)
							.Append(".MockRegistrationsProvider.Value = mockRegistration;").AppendLine();
						sb.Append("\t\t\t\t\tif (setups.Length > 0)").AppendLine();
						sb.Append("\t\t\t\t\t{").AppendLine();
						sb.Append("\t\t\t\t\t\t#pragma warning disable CS0618").AppendLine();
						sb.Append("\t\t\t\t\t\tIMockSetup<").Append(mock.MockClass.ClassFullName)
							.Append("> setupTarget = new MockSetup<").Append(mock.MockClass.ClassFullName)
							.Append(">(mockRegistration);").AppendLine();
						sb.Append("\t\t\t\t\t\t#pragma warning restore CS0618").AppendLine();
						sb.Append("\t\t\t\t\t\tforeach (Action<IMockSetup<").Append(mock.MockClass.ClassFullName)
							.Append(">> setup in setups)").AppendLine();
						sb.Append("\t\t\t\t\t\t{").AppendLine();
						sb.Append("\t\t\t\t\t\t\tsetup.Invoke(setupTarget);").AppendLine();
						sb.Append("\t\t\t\t\t\t}").AppendLine();
						sb.Append("\t\t\t\t\t}").AppendLine();
						sb.Append("\t\t\t\t\t_value = new MockFor").Append(mock.Name).Append("(");
						for (int i = 1; i <= constructorParameters.Count; i++)
						{
							sb.Append('c').Append(constructorIndex).Append('p').Append(i).Append(", ");
						}

						sb.Append("mockRegistration);").AppendLine();
						sb.Append("\t\t\t\t}").AppendLine();
					}

					sb.Append("\t\t\t\telse").AppendLine();
					sb.Append("\t\t\t\t{").AppendLine();
					sb.Append("\t\t\t\t\tthrow new MockException($\"Could not find any constructor for '")
						.Append(mock.MockClass.ClassFullName)
						.Append(
							"' that matches the {constructorParameters.Parameters.Length} given parameters ({string.Join(\", \", constructorParameters.Parameters)}).\");")
						.AppendLine();
					sb.Append("\t\t\t\t}").AppendLine();
				}
				else
				{
					sb.Append(
							"\t\t\t\tthrow new MockException(\"Could not find any constructor at all for the base type '")
						.Append(mock.MockClass.ClassFullName)
						.Append("'. Therefore mocking is not supported!\");")
						.AppendLine();
				}

				sb.Append("\t\t\t}").AppendLine();
			}

			sb.AppendLine("\t\t}");
			sb.AppendLine("\t}");
			sb.AppendLine();
		}

		sb.Append("""
		          	private static bool TryCast<TValue>(object? value, MockBehavior behavior, out TValue result)
		          	{
		          		if (value is TValue typedValue)
		          		{
		          			result = typedValue;
		          			return true;
		          		}
		          		
		          		if (behavior.DefaultValue.Generate(typeof(TValue)) is TValue defaultValue)
		          		{
		          			result = defaultValue;
		          			return true;
		          		}
		          		
		          		result = default!;
		          		return value is null;
		          	}
		          """);
		sb.AppendLine();
		sb.AppendLine();
		sb.AppendLine("\tprivate class TypedDefaultValueFactory<T>(T value) : ExtensionsOnMockBehavior.IDefaultValueFactory");
		sb.AppendLine("\t{");
		sb.AppendLine("\t\t/// <inheritdoc cref=\"ExtensionsOnMockBehavior.IDefaultValueFactory.IsMatch(Type)\" />");
		sb.AppendLine("\t\tpublic bool IsMatch(Type type)");
		sb.AppendLine("\t\t\t=> type == typeof(T);");
		sb.AppendLine();
		sb.AppendLine("\t\t/// <inheritdoc cref=\"ExtensionsOnMockBehavior.IDefaultValueFactory.Create(Type, IDefaultValueGenerator, object?[])\" />");
		sb.AppendLine("\t\tpublic object? Create(Type type, IDefaultValueGenerator defaultValueGenerator, object?[] parameters)");
		sb.AppendLine("\t\t\t=> value;");
		sb.AppendLine("\t}");

		sb.AppendLine();
		sb.AppendLine(
			"\tprivate class CallbackDefaultValueFactory<T>(Func<IDefaultValueGenerator, T> callback, Func<Type, bool>? isMatch = null) : ExtensionsOnMockBehavior.IDefaultValueFactory");
		sb.AppendLine("\t{");
		sb.AppendLine("\t\t/// <inheritdoc cref=\"ExtensionsOnMockBehavior.IDefaultValueFactory.IsMatch(Type)\" />");
		sb.AppendLine("\t\tpublic bool IsMatch(Type type)");
		sb.AppendLine("\t\t\t=> isMatch?.Invoke(type) ?? type == typeof(T);");
		sb.AppendLine();
		sb.AppendLine("\t\t/// <inheritdoc cref=\"ExtensionsOnMockBehavior.IDefaultValueFactory.Create(Type, IDefaultValueGenerator, object?[])\" />");
		sb.AppendLine("\t\tpublic object? Create(Type type, IDefaultValueGenerator defaultValueGenerator, object?[] parameters)");
		sb.AppendLine("\t\t\t=> callback(defaultValueGenerator);");
		sb.AppendLine("\t}");

		sb.AppendLine();
		sb.AppendLine(
			"\tprivate class ParametrizedCallbackDefaultValueFactory<T>(Func<IDefaultValueGenerator, object?[], T> callbackWithParams, Func<Type, bool>? isMatch = null) : ExtensionsOnMockBehavior.IDefaultValueFactory");
		sb.AppendLine("\t{");
		sb.AppendLine("\t\t/// <inheritdoc cref=\"ExtensionsOnMockBehavior.IDefaultValueFactory.IsMatch(Type)\" />");
		sb.AppendLine("\t\tpublic bool IsMatch(Type type)");
		sb.AppendLine("\t\t\t=> isMatch?.Invoke(type) ?? type == typeof(T);");
		sb.AppendLine();
		sb.AppendLine("\t\t/// <inheritdoc cref=\"ExtensionsOnMockBehavior.IDefaultValueFactory.Create(Type, IDefaultValueGenerator, object?[])\" />");
		sb.AppendLine("\t\tpublic object? Create(Type type, IDefaultValueGenerator defaultValueGenerator, params object?[] parameters)");
		sb.AppendLine("\t\t\t=> callbackWithParams(defaultValueGenerator, parameters);");
		sb.AppendLine("\t}");

		sb.AppendLine();
		sb.AppendLine(
			"\tprivate class RecursiveMockValueFactory() : ExtensionsOnMockBehavior.IDefaultValueFactory");
		sb.AppendLine("\t{");
		sb.AppendLine("\t\t/// <inheritdoc cref=\"ExtensionsOnMockBehavior.IDefaultValueFactory.IsMatch(Type)\" />");
		sb.AppendLine("\t\tpublic bool IsMatch(Type type)");
		sb.AppendLine("\t\t\t=> true;");
		sb.AppendLine();
		sb.AppendLine("\t\t/// <inheritdoc cref=\"ExtensionsOnMockBehavior.IDefaultValueFactory.Create(Type, IDefaultValueGenerator, object?[])\" />");
		sb.AppendLine("\t\tpublic object? Create(Type type, IDefaultValueGenerator defaultValueGenerator, params object?[] parameters)");
		sb.AppendLine("\t\t{");
		sb.AppendLine("\t\t\treturn new MockGenerator().Get(null, MockBehavior.Default, type);");
		sb.AppendLine("\t\t}");
		sb.AppendLine("\t}");
		sb.AppendLine("}");
		sb.AppendLine("#nullable disable");
		return sb.ToString();
	}
}
#pragma warning restore S3776 // Cognitive Complexity of methods should not be too high
