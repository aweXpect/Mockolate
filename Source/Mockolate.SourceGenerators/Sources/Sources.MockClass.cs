using System.Text;
using Microsoft.CodeAnalysis;
using Mockolate.SourceGenerators.Entities;
using Mockolate.SourceGenerators.Internals;
using GenericParameter = Mockolate.SourceGenerators.Entities.GenericParameter;
using Type = Mockolate.SourceGenerators.Entities.Type;

namespace Mockolate.SourceGenerators.Sources;

#pragma warning disable S3776 // Cognitive Complexity of methods should not be too high
internal static partial class Sources
{
	private const int MaxExplicitParameters = 4;

	public static string MockClass(string name, Class @class, bool hasOverloadResolutionPriority = false)
	{
		EquatableArray<Method>? constructors = (@class as MockClass)?.Constructors;
		bool hasParameterizedConstructor = !@class.IsInterface &&
		                                   constructors?.Any(m => m.Parameters.Count > 0) == true;
		string escapedClassName = @class.ClassFullName.EscapeForXmlDoc();
		bool hasEvents = @class.AllEvents().Any(x => !x.IsStatic);
		bool hasStaticEvents = @class.IsInterface &&
		                       @class.AllEvents().Any(@event => @event.IsStatic);
		bool hasStaticMembers = @class.IsInterface &&
		                        (@class.AllMethods().Any(method => method.IsStatic) ||
		                         @class.AllProperties().Any(property => property.IsStatic));
		bool hasProtectedEvents = !@class.IsInterface && @class.AllEvents().Any(@event => @event.IsProtected);
		bool hasProtectedMembers = !@class.IsInterface &&
		                           (@class.AllMethods().Any(method => method.IsProtected)
		                            || @class.AllProperties().Any(property => property.IsProtected));
		string setupType = hasProtectedMembers
			? $"IMockSetupInitializationFor{name}"
			: $"global::Mockolate.Mock.IMockSetupFor{name}";
		string mockRegistryName = @class.GetUniqueName("MockRegistry", "MockolateMockRegistry");
		StringBuilder sb = InitializeBuilder();

		sb.Append("#nullable enable annotations").AppendLine();
		sb.Append("namespace Mockolate;").AppendLine();
		sb.AppendLine();

		#region MockForXXXExtensions

		sb.AppendXmlSummary($"Mock extensions for <see cref=\"{escapedClassName}\" />.", "");
#if !DEBUG
		sb.Append("[global::System.Diagnostics.DebuggerNonUserCode]").AppendLine();
#endif
		sb.Append("[global::System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]").AppendLine();
		sb.Append("internal static partial class MockExtensionsFor").Append(name).AppendLine();
		sb.Append("{").AppendLine();

		#region Type extensions

		sb.Append("\t/// <inheritdoc cref=\"MockExtensionsFor").Append(name).Append("\" />").AppendLine();
		sb.Append("\textension(").Append(@class.ClassFullName).Append(" mock)").AppendLine();
		sb.Append("\t{").AppendLine();

		#region Mock Property

		string mockPropertyName = CreateUniquePropertyName(@class, "Mock");

		List<string> mockPropertyRemarks = new()
		{
			$"The accessor is the bridge between the strongly-typed instance of <see cref=\"{escapedClassName}\" /> returned by <c>CreateMock(...)</c> and the underlying mock registry where setups and recorded interactions live.",
			"Through it you can:",
			"<list type=\"bullet\">",
			"  <item><description><c>Setup</c> - configure how members respond when invoked (<c>Returns</c>, <c>Throws</c>, <c>Do</c>, <c>InitializeWith</c>, ...).</description></item>",
			"  <item><description><c>Verify</c> - assert how often (and in which order) members were invoked.</description></item>",
		};
		if (hasEvents)
		{
			mockPropertyRemarks.Add("  <item><description><c>Raise</c> - trigger events declared on the mocked type.</description></item>");
		}
		if (hasProtectedMembers || hasProtectedEvents)
		{
			mockPropertyRemarks.Add("  <item><description><c>SetupProtected</c> / <c>VerifyProtected</c> / <c>RaiseProtected</c> - target <see langword=\"protected\" /> members on class mocks.</description></item>");
		}
		if (hasStaticMembers || hasStaticEvents)
		{
			mockPropertyRemarks.Add("  <item><description><c>SetupStatic</c> / <c>VerifyStatic</c> / <c>RaiseStatic</c> - target <see langword=\"static\" /> members on interface mocks.</description></item>");
		}
		mockPropertyRemarks.Add("  <item><description><c>InScenario</c> / <c>TransitionTo</c> - scope setups and behavior to a named scenario and switch between scenarios.</description></item>");
		mockPropertyRemarks.Add("  <item><description><c>Monitor</c>, <c>ClearAllInteractions</c>, <c>VerifyThatAllInteractionsAreVerified</c>, <c>VerifyThatAllSetupsAreUsed</c> - manage recorded interactions.</description></item>");
		mockPropertyRemarks.Add("  <item><description><c>VerifySetup</c> - verify how often a specific setup matched.</description></item>");
		mockPropertyRemarks.Add("</list>");

		sb.AppendXmlSummary($"Gets the mock accessor for <see cref=\"{escapedClassName}\" /> - the entry point for configuring setups, verifying interactions and raising events.");
		sb.AppendXmlRemarks(mockPropertyRemarks.ToArray());
		sb.AppendXmlException("global::Mockolate.Exceptions.MockException",
			$"The instance is not a Mockolate-generated mock of <see cref=\"{escapedClassName}\" />.");
		sb.Append("\t\tpublic global::Mockolate.Mock.IMockFor").Append(name).Append(' ').Append(mockPropertyName)
			.AppendLine();
		sb.Append("\t\t{").AppendLine();
		sb.Append("\t\t\tget").AppendLine();
		sb.Append("\t\t\t{").AppendLine();
		sb.Append("\t\t\t\tif (mock is global::Mockolate.Mock.IMockFor").Append(name).Append(" mockInterface)")
			.AppendLine();
		sb.Append("\t\t\t\t{").AppendLine();
		sb.Append("\t\t\t\t\treturn mockInterface;").AppendLine();
		sb.Append("\t\t\t\t}").AppendLine();
		sb.Append("\t\t\t\tthrow new global::Mockolate.Exceptions.MockException(\"The subject is no mock.\");")
			.AppendLine();
		sb.Append("\t\t\t}").AppendLine();
		sb.Append("\t\t}").AppendLine();
		sb.AppendLine();

		#endregion Mock Property

		#region CreateMock

		string createMockReturns =
			$"A new mock instance of <see cref=\"{escapedClassName}\" />.";

		List<string> createMockRemarks = new()
		{
			$"The returned instance is a strongly-typed mock generated at compile time - it implements <see cref=\"{escapedClassName}\" /> and exposes the Mockolate surface through <c>.Mock</c>:",
			"<list type=\"bullet\">",
			"  <item><description><c>.Mock.Setup</c> configures how members respond (<c>Returns</c>, <c>Throws</c>, <c>Do</c>, <c>InitializeWith</c>, sequences, callbacks).</description></item>",
			"  <item><description><c>.Mock.Verify</c> asserts how often and in which order members were invoked.</description></item>",
		};
		if (hasEvents)
		{
			createMockRemarks.Add("  <item><description><c>.Mock.Raise</c> triggers events declared on the mocked type.</description></item>");
		}
		createMockRemarks.Add("</list>");
		createMockRemarks.Add("With the default behavior, un-configured members return <c>default</c> values (empty collections / strings, completed tasks, <see langword=\"null\" /> otherwise) and base-class implementations are invoked for class mocks. Use one of the overloads that accepts a <see cref=\"global::Mockolate.MockBehavior\" /> to customize this (for example to make un-configured calls throw or to skip the base class).");
		createMockRemarks.Add("Overloads allow you to additionally pass constructor parameters (for class mocks), apply an initial <c>setup</c> callback before the instance is returned, or combine both.");

		sb.AppendXmlSummary(
			$"Creates a new mock of <see cref=\"{escapedClassName}\" /> with the default <see cref=\"global::Mockolate.MockBehavior\" />.");
		sb.AppendXmlRemarks(createMockRemarks.ToArray());
		sb.AppendXmlReturns(createMockReturns);
		sb.Append("\t\tpublic static ").Append(@class.ClassFullName).Append(" CreateMock()").AppendLine();
		sb.Append("\t\t\t=> CreateMock(null, null, (object?[]?)null);").AppendLine();
		sb.AppendLine();

		sb.AppendXmlSummary(
			$"Creates a new mock of <see cref=\"{escapedClassName}\" /> with the default <see cref=\"global::Mockolate.MockBehavior\" />, applying the given <paramref name=\"setup\" /> immediately.");
		sb.AppendXmlRemarks("The provided <paramref name=\"setup\" /> is immediately applied to the mock. Use this overload when you want setups to cover virtual interactions triggered inside the constructor.");
		sb.AppendXmlParam("setup", "Callback that receives the mock's setup surface and registers initial setups before the mock is returned.");
		sb.AppendXmlReturns(createMockReturns);
		sb.Append("\t\tpublic static ").Append(@class.ClassFullName).Append(" CreateMock(global::System.Action<")
			.Append(setupType).Append("> setup)").AppendLine();
		sb.Append("\t\t\t=> CreateMock(null, setup, (object?[]?)null);").AppendLine();
		sb.AppendLine();

		sb.AppendXmlSummary(
			$"Creates a new mock of <see cref=\"{escapedClassName}\" /> with the given <paramref name=\"mockBehavior\" />.");
		sb.AppendXmlParam("mockBehavior", "Controls how the mock responds when members are invoked without a matching setup; see <see cref=\"global::Mockolate.MockBehavior\" />.");
		sb.AppendXmlReturns(createMockReturns);
		sb.Append("\t\tpublic static ").Append(@class.ClassFullName)
			.Append(" CreateMock(global::Mockolate.MockBehavior mockBehavior)").AppendLine();
		sb.Append("\t\t\t=> CreateMock(mockBehavior, null, (object?[]?)null);").AppendLine();
		sb.AppendLine();

		sb.AppendXmlSummary(
			$"Creates a new mock of <see cref=\"{escapedClassName}\" /> with the given <paramref name=\"mockBehavior\" />, applying the given <paramref name=\"setup\" /> immediately.");
		sb.AppendXmlRemarks("The provided <paramref name=\"setup\" /> is immediately applied to the mock. Use this overload when you want setups to cover virtual interactions triggered inside the constructor.");
		sb.AppendXmlParam("mockBehavior", "Controls how the mock responds when members are invoked without a matching setup; see <see cref=\"global::Mockolate.MockBehavior\" />.");
		sb.AppendXmlParam("setup", "Callback that receives the mock's setup surface and registers initial setups before the mock is returned.");
		sb.AppendXmlReturns(createMockReturns);
		sb.Append("\t\tpublic static ").Append(@class.ClassFullName)
			.Append(" CreateMock(global::Mockolate.MockBehavior mockBehavior, global::System.Action<").Append(setupType)
			.Append("> setup)").AppendLine();
		sb.Append("\t\t\t=> CreateMock(mockBehavior, setup, (object?[]?)null);").AppendLine();
		sb.AppendLine();

		if (hasParameterizedConstructor)
		{
			sb.AppendXmlSummary(
				$"Creates a new mock of <see cref=\"{escapedClassName}\" /> using the given <paramref name=\"constructorParameters\" /> to invoke the base-class constructor.");
			sb.AppendXmlParam("constructorParameters", "Values forwarded to a matching base-class constructor. Required when no parameterless constructor exists.");
			sb.AppendXmlReturns(createMockReturns);
			sb.Append("\t\tpublic static ").Append(@class.ClassFullName)
				.Append(" CreateMock(object?[] constructorParameters)").AppendLine();
			sb.Append("\t\t\t=> CreateMock(null, null, constructorParameters);").AppendLine();
			sb.AppendLine();

			sb.AppendXmlSummary(
				$"Creates a new mock of <see cref=\"{escapedClassName}\" /> using the given <paramref name=\"mockBehavior\" /> and <paramref name=\"constructorParameters\" />.");
			sb.AppendXmlParam("mockBehavior", "Controls how the mock responds when members are invoked without a matching setup; see <see cref=\"global::Mockolate.MockBehavior\" />.");
			sb.AppendXmlParam("constructorParameters", "Values forwarded to a matching base-class constructor. Required when no parameterless constructor exists.");
			sb.AppendXmlReturns(createMockReturns);
			sb.Append("\t\tpublic static ").Append(@class.ClassFullName)
				.Append(" CreateMock(global::Mockolate.MockBehavior mockBehavior, object?[] constructorParameters)")
				.AppendLine();
			sb.Append("\t\t\t=> CreateMock(mockBehavior, null, constructorParameters);").AppendLine();
			sb.AppendLine();

			sb.AppendXmlSummary(
				$"Creates a new mock of <see cref=\"{escapedClassName}\" /> applying the given <paramref name=\"setup\" /> immediately, using the given <paramref name=\"constructorParameters\" />.");
			sb.AppendXmlRemarks("The provided <paramref name=\"setup\" /> is immediately applied to the mock. Use this overload when you want setups to cover virtual interactions triggered inside the constructor.");
			sb.AppendXmlParam("setup", "Callback that receives the mock's setup surface and registers initial setups before the mock is returned.");
			sb.AppendXmlParam("constructorParameters", "Values forwarded to a matching base-class constructor. Required when no parameterless constructor exists.");
			sb.AppendXmlReturns(createMockReturns);
			sb.Append("\t\tpublic static ").Append(@class.ClassFullName)
				.Append(" CreateMock(global::System.Action<").Append(setupType)
				.Append("> setup, object?[] constructorParameters)").AppendLine();
			sb.Append("\t\t\t=> CreateMock(null, setup, constructorParameters);").AppendLine();
			sb.AppendLine();

			AppendTypedCreateMockOverloads(sb, @class, constructors, setupType, escapedClassName, createMockReturns);
		}

		sb.AppendXmlSummary(
			$"Creates a new mock of <see cref=\"{escapedClassName}\" /> using the given <paramref name=\"mockBehavior\" />, applying the given <paramref name=\"setup\" /> immediately, using the given <paramref name=\"constructorParameters\" />.");
		sb.AppendXmlRemarks("The provided <paramref name=\"setup\" /> is immediately applied to the mock. Use this overload when you want setups to cover virtual interactions triggered inside the constructor.");
		sb.AppendXmlParam("mockBehavior", "Controls how the mock responds when members are invoked without a matching setup, or <see langword=\"null\" /> for <c>MockBehavior.Default</c>.");
		sb.AppendXmlParam("setup", "Callback that receives the mock's setup surface and registers initial setups before the mock is returned, or <see langword=\"null\" /> to skip.");
		sb.AppendXmlParam("constructorParameters", "Values forwarded to a matching base-class constructor, or <see langword=\"null\" /> to use the parameterless constructor.");
		sb.AppendXmlReturns(createMockReturns);
		sb.Append("\t\t").Append(hasParameterizedConstructor ? "public" : "private").Append(" static ")
			.Append(@class.ClassFullName)
			.Append(" CreateMock(global::Mockolate.MockBehavior? mockBehavior, global::System.Action<")
			.Append(setupType).Append(">? setup, object?[]? constructorParameters)").AppendLine();
		sb.Append("\t\t{").AppendLine();
		sb.Append("\t\t\tif (mockBehavior is not null)").AppendLine();
		sb.Append("\t\t\t{").AppendLine();
		sb.Append(
				"\t\t\t\tIMockBehaviorAccess mockBehaviorAccess = (global::Mockolate.IMockBehaviorAccess)mockBehavior;")
			.AppendLine();
		sb.Append("\t\t\t\tif (mockBehaviorAccess.TryGet<global::System.Action<").Append(setupType)
			.Append(">?>(out var additionalSetup))").AppendLine();
		sb.Append("\t\t\t\t{").AppendLine();
		sb.Append("\t\t\t\t\tif (setup is null)").AppendLine();
		sb.Append("\t\t\t\t\t{").AppendLine();
		sb.Append("\t\t\t\t\t\tsetup = additionalSetup;").AppendLine();
		sb.Append("\t\t\t\t\t}").AppendLine();
		sb.Append("\t\t\t\t\telse").AppendLine();
		sb.Append("\t\t\t\t\t{").AppendLine();
		sb.Append("\t\t\t\t\t\tvar originalSetup = setup;").AppendLine();
		sb.Append("\t\t\t\t\t\tsetup = s => { additionalSetup.Invoke(s); originalSetup.Invoke(s); };").AppendLine();
		sb.Append("\t\t\t\t\t}").AppendLine();
		sb.Append("\t\t\t\t}").AppendLine();
		if (!@class.IsInterface && !hasStaticMembers)
		{
			sb.Append("\t\t\t\tif (constructorParameters is null && mockBehaviorAccess.TryGetConstructorParameters<")
				.Append(@class.ClassFullName).Append(">(out object?[]? parameters))").AppendLine();
			sb.Append("\t\t\t\t{").AppendLine();
			sb.Append("\t\t\t\t\tconstructorParameters = parameters;").AppendLine();
			sb.Append("\t\t\t\t}").AppendLine();
		}

		sb.Append("\t\t\t}").AppendLine();
		sb.AppendLine();

		if (@class is { ClassFullName: "global::System.Net.Http.HttpClient", })
		{
			sb.Append(
					"\t\t\tglobal::Mockolate.MockRegistry mockRegistry = new global::Mockolate.MockRegistry(mockBehavior ?? global::Mockolate.MockBehavior.Default, constructorParameters);")
				.AppendLine();
			sb.Append("\t\t\tif (constructorParameters is null)").AppendLine();
			sb.Append("\t\t\t{").AppendLine();
			sb.Append("\t\t\t\tconstructorParameters = [new global::Mockolate.Mock.HttpMessageHandler(mockRegistry),];")
				.AppendLine();
			sb.Append("\t\t\t\tmockRegistry = new global::Mockolate.MockRegistry(mockRegistry, constructorParameters);")
				.AppendLine();
			sb.Append("\t\t\t}").AppendLine();
			sb.Append(
					"\t\t\telse if (constructorParameters.Length > 0 && constructorParameters[0] is global::Mockolate.Mock.HttpMessageHandler && constructorParameters[0] is global::Mockolate.IMock httpMessageHandlerMock)")
				.AppendLine();
			sb.Append("\t\t\t{").AppendLine();
			sb.Append(
					"\t\t\t\tif (mockBehavior is not null && httpMessageHandlerMock.MockRegistry.Behavior != mockBehavior)")
				.AppendLine();
			sb.Append("\t\t\t\t{").AppendLine();
			sb.Append(
					"\t\t\t\t\tthrow new global::Mockolate.Exceptions.MockException($\"Mock of type 'System.Net.Http.HttpClient' cannot be created with behavior '{mockBehavior}' because it shares its mock registry with a mock of type 'System.Net.Http.HttpMessageHandler' that has behavior '{httpMessageHandlerMock.MockRegistry.Behavior}'.\");")
				.AppendLine();
			sb.Append("\t\t\t\t}").AppendLine();
			sb.Append(
					"\t\t\t\tmockRegistry = new global::Mockolate.MockRegistry(httpMessageHandlerMock.MockRegistry, constructorParameters);")
				.AppendLine();
			sb.Append("\t\t\t}").AppendLine();
			sb.Append("\t\t\tmockBehavior ??= global::Mockolate.MockBehavior.Default;").AppendLine();
		}
		else
		{
			sb.Append("\t\t\tmockBehavior ??= global::Mockolate.MockBehavior.Default;").AppendLine();
			sb.Append(
					"\t\t\tglobal::Mockolate.MockRegistry mockRegistry = new global::Mockolate.MockRegistry(mockBehavior, constructorParameters);")
				.AppendLine();
		}

		sb.Append("\t\t\treturn CreateMockInstance(mockRegistry, constructorParameters, setup);").AppendLine();
		sb.Append("\t\t}").AppendLine();

		sb.AppendLine();
		sb.Append("\t\tprivate static ").Append(@class.ClassFullName)
			.Append(
				" CreateMockInstance(global::Mockolate.MockRegistry mockRegistry, object?[]? constructorParameters, global::System.Action<")
			.Append(setupType).Append(">? setup)").AppendLine();
		sb.Append("\t\t{").AppendLine();
		if (!@class.IsInterface && constructors?.Count > 0)
		{
			sb.Append("\t\t\tif (constructorParameters is null || constructorParameters.Length == 0)").AppendLine();
			sb.Append("\t\t\t{").AppendLine();
			if (constructors.Value.Any(m => m.Parameters.Count == 0))
			{
				sb.Append("\t\t\t\tglobal::Mockolate.Mock.").Append(name)
					.Append(".MockRegistryProvider.Value = mockRegistry;").AppendLine();
				sb.Append("\t\t\t\tglobal::Mockolate.MockExtensionsFor").Append(name)
					.Append(".MockSetup? setupTarget = null;").AppendLine();
				sb.Append("\t\t\t\tif (setup is not null)").AppendLine();
				sb.Append("\t\t\t\t{").AppendLine();
				sb.Append("\t\t\t\t\tsetupTarget ??= new(mockRegistry);").AppendLine();
				sb.Append("\t\t\t\t\tsetup.Invoke(setupTarget);").AppendLine();
				sb.Append("\t\t\t\t}").AppendLine();
				sb.Append("\t\t\t\treturn new global::Mockolate.Mock.").Append(name).Append("(mockRegistry);")
					.AppendLine();
			}
			else
			{
				sb.Append(
						"\t\t\t\tthrow new global::Mockolate.Exceptions.MockException(\"No parameterless constructor found for '")
					.Append(@class.DisplayString).Append("'. Please provide constructor parameters.\");").AppendLine();
			}

			sb.Append("\t\t\t}").AppendLine();
			int constructorIndex = 0;
			bool useTryCast = false;
			bool useTryCastWithDefaultValue = false;
			foreach (EquatableArray<MethodParameter> constructorParameters in constructors.Value
				         .Select(constructor => constructor.Parameters))
			{
				constructorIndex++;
				int requiredParameters = constructorParameters.Count(c => !c.HasExplicitDefaultValue);
				if (requiredParameters < constructorParameters.Count)
				{
					sb.Append("\t\t\telse if (constructorParameters.Length >= ")
						.Append(requiredParameters).Append(" && constructorParameters.Length <= ")
						.Append(constructorParameters.Count);
				}
				else
				{
					sb.Append("\t\t\telse if (constructorParameters.Length == ")
						.Append(constructorParameters.Count);
				}

				int constructorParameterIndex = 0;
				foreach (MethodParameter parameter in constructorParameters)
				{
					useTryCast = useTryCast || !parameter.HasExplicitDefaultValue;
					useTryCastWithDefaultValue = useTryCastWithDefaultValue || parameter.HasExplicitDefaultValue;
					sb.AppendLine().Append("\t\t\t    && ")
						.Append(parameter.HasExplicitDefaultValue ? "TryCastWithDefaultValue" : "TryCast")
						.Append("(constructorParameters, ")
						.Append(constructorParameterIndex++)
						.Append(parameter.HasExplicitDefaultValue ? $", {parameter.ExplicitDefaultValue}" : "")
						.Append(", mockRegistry.Behavior, out ").Append(parameter.Type.Fullname).Append(" c")
						.Append(constructorIndex)
						.Append('p')
						.Append(constructorParameterIndex).Append(")");
				}

				sb.Append(")").AppendLine();
				sb.Append("\t\t\t{").AppendLine();
				sb.Append("\t\t\t\tglobal::Mockolate.Mock.").Append(name)
					.Append(".MockRegistryProvider.Value = mockRegistry;").AppendLine();
				sb.Append("\t\t\t\tglobal::Mockolate.MockExtensionsFor").Append(name)
					.Append(".MockSetup? setupTarget = null;").AppendLine();
				sb.Append("\t\t\t\tif (setup is not null)").AppendLine();
				sb.Append("\t\t\t\t{").AppendLine();
				sb.Append("\t\t\t\t\tsetupTarget ??= new(mockRegistry);").AppendLine();
				sb.Append("\t\t\t\t\tsetup.Invoke(setupTarget);").AppendLine();
				sb.Append("\t\t\t\t}").AppendLine();
				sb.Append("\t\t\t\treturn new global::Mockolate.Mock.").Append(name)
					.Append("(mockRegistry");
				for (int i = 1; i <= constructorParameters.Count; i++)
				{
					sb.Append(", ").Append('c').Append(constructorIndex).Append('p').Append(i);
				}

				sb.Append(");").AppendLine();
				sb.Append("\t\t\t}").AppendLine();
			}

			sb.Append("\t\t\telse").AppendLine();
			sb.Append("\t\t\t{").AppendLine();
			sb.Append(
					"\t\t\t\tthrow new global::Mockolate.Exceptions.MockException($\"Could not find any constructor for '")
				.Append(@class.DisplayString)
				.Append(
					"' that matches the {constructorParameters.Length} given parameters ({string.Join(\", \", constructorParameters)}).\");")
				.AppendLine();
			sb.Append("\t\t\t}").AppendLine();
			if (useTryCast)
			{
				sb.Append("""
				          			static bool TryCast<TValue>(object?[] values, int index, global::Mockolate.MockBehavior behavior, out TValue result)
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
				          """).AppendLine();
			}

			if (useTryCastWithDefaultValue)
			{
				sb.Append("""
				          			static bool TryCastWithDefaultValue<TValue>(object?[] values, int index, TValue defaultValue, global::Mockolate.MockBehavior behavior, out TValue result)
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
			}
		}
		else
		{
			sb.Append("\t\t\tvar value = new global::Mockolate.Mock.").Append(name).Append("(mockRegistry);")
				.AppendLine();
			sb.Append("\t\t\tif (setup is not null)").AppendLine();
			sb.Append("\t\t\t{").AppendLine();
			sb.Append("\t\t\t\tsetup.Invoke(value);").AppendLine();
			sb.Append("\t\t\t}").AppendLine();
			sb.Append("\t\t\treturn value;").AppendLine();
		}

		sb.Append("\t\t}").AppendLine();

		#endregion CreateMock

		sb.AppendXmlSummary("Creates a mock that wraps the given <paramref name=\"instance\" />.");
		sb.AppendXmlRemarks("Public members on the mock forward to <paramref name=\"instance\" /> unless overridden by a setup; protected members still go through the base-class implementation. All forwarded interactions are recorded and can be verified the same as on a plain mock.");
		sb.AppendXmlParam("instance", "The real object whose calls should be forwarded. Must not be <see langword=\"null\" />.");
		sb.AppendXmlReturns($"A new mock of <see cref=\"{escapedClassName}\" /> that delegates to <paramref name=\"instance\" />.");
		sb.Append("\t\tpublic ").Append(@class.ClassFullName).Append(" Wrapping(").Append(@class.ClassFullName)
			.Append(" instance)").AppendLine();
		sb.Append("\t\t{").AppendLine();
		sb.Append("\t\t\tif (mock is global::Mockolate.IMock mockInterface)").AppendLine();
		sb.Append("\t\t\t{").AppendLine();
		sb.Append(
				"\t\t\t\treturn CreateMockInstance(new global::Mockolate.MockRegistry(mockInterface.MockRegistry, instance), mockInterface.MockRegistry.ConstructorParameters, null);")
			.AppendLine();
		sb.Append("\t\t\t}").AppendLine();
		sb.Append("\t\t\tthrow new global::Mockolate.Exceptions.MockException(\"The subject is no mock.\");")
			.AppendLine();
		sb.Append("\t\t}").AppendLine();
		sb.AppendLine();

		sb.Append("\t}").AppendLine();

		#endregion Type extensions

		sb.AppendLine();

		#region MockBehavior extensions

		sb.Append("\t/// <inheritdoc cref=\"MockExtensionsFor").Append(name).Append("\" />").AppendLine();
		sb.Append("\textension(global::Mockolate.MockBehavior behavior)").AppendLine();
		sb.Append("\t{").AppendLine();

		sb.AppendXmlSummary(
			"Initializes mocks of type <typeparamref name=\"T\" /> with the given <paramref name=\"setup\" />.");
		sb.AppendXmlRemarks(
			"The <paramref name=\"setup\" /> is applied to the mock before the constructor is executed. Calling <c>Initialize</c> again overlays additional setups on top of any previously registered ones.");
		sb.AppendXmlTypeParam("T", $"The mockable type derived from <see cref=\"{escapedClassName}\" /> that this setup should apply to.");
		sb.AppendXmlParam("setup", $"Callback invoked when a new mock of <typeparamref name=\"T\" /> is created.");
		sb.AppendXmlReturns("A new <see cref=\"global::Mockolate.MockBehavior\" /> with the registered initializer. The original instance is unchanged.");
		sb.Append("\t\tpublic global::Mockolate.MockBehavior Initialize<T>(global::System.Action<").Append(setupType)
			.Append("> setup)").AppendLine();
		sb.Append("\t\t\twhere T : ").Append(@class.ClassFullName).AppendLine();
		sb.Append("\t\t{").AppendLine();
		sb.Append("\t\t\tvar behaviorAccess = (global::Mockolate.IMockBehaviorAccess)behavior;").AppendLine();
		sb.Append("\t\t\treturn behaviorAccess.Set(setup);").AppendLine();
		sb.Append("\t\t}").AppendLine();

		sb.Append("\t}").AppendLine();

		#endregion MockBehavior extensions

		#region Setup helpers

		if (!@class.IsInterface && constructors?.Count > 0)
		{
			string protectedName = @class.GetUniqueName("Protected", "SetupProtected");
			if (hasProtectedMembers)
			{
				sb.Append("\tinternal interface IMockSetupInitializationFor").Append(name)
					.Append(" : global::Mockolate.Mock.IMockSetupFor").Append(name).AppendLine();
				sb.Append("\t{").AppendLine();
				sb.AppendXmlSummary("Setup protected members");
				sb.Append("\t\tglobal::Mockolate.Mock.IMockProtectedSetupFor").Append(name).Append(' ')
					.Append(protectedName).Append(" { get; }").AppendLine();
				sb.Append("\t}").AppendLine();
			}

			sb.AppendLine();
#if !DEBUG
			sb.Append("\t[global::System.Diagnostics.DebuggerNonUserCode]").AppendLine();
#endif
			sb.Append("\t[global::System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]").AppendLine();
			sb.Append(
					"\tinternal sealed class MockSetup(global::Mockolate.MockRegistry mockRegistry) : global::Mockolate.Mock.IMockSetupFor")
				.Append(name);
			if (hasProtectedMembers)
			{
				sb.Append(", global::Mockolate.Mock.IMockProtectedSetupFor").Append(name)
					.Append(", IMockSetupInitializationFor").Append(name);
			}

			sb.AppendLine();
			sb.Append("\t{").AppendLine();
			if (hasProtectedMembers)
			{
				sb.Append("\t\t/// <inheritdoc />").AppendLine();
				sb.Append("\t\tglobal::Mockolate.Mock.IMockProtectedSetupFor").Append(name)
					.Append(" IMockSetupInitializationFor").Append(name).Append('.').Append(protectedName)
					.Append(" => this;").AppendLine();
			}

			sb.Append("\t\tprivate global::Mockolate.MockRegistry ").Append(mockRegistryName)
				.Append(" { get; } = mockRegistry;").AppendLine();
			sb.AppendLine();
			sb.Append("\t\t#region IMockSetupFor").Append(name).AppendLine();
			sb.AppendLine();
			ImplementSetupInterface(sb, @class, mockRegistryName, $"IMockSetupFor{name}", MemberType.Public);
			sb.Append("\t\t#endregion IMockSetupFor").Append(name).AppendLine();
			if (hasProtectedMembers)
			{
				sb.AppendLine();
				sb.Append("\t\t#region IMockProtectedSetupFor").Append(name).AppendLine();
				sb.AppendLine();
				ImplementSetupInterface(sb, @class, mockRegistryName, $"IMockProtectedSetupFor{name}",
					MemberType.Protected);
				sb.Append("\t\t#endregion IMockProtectedSetupFor").Append(name).AppendLine();
			}

			sb.Append("\t}").AppendLine();
		}

		#endregion Setup helpers

		AppendNestedCovariantParameterAdapter(sb);
		sb.Append("}").AppendLine();

		#endregion MockForXXXExtensions

		sb.AppendLine();

		#region MockForXXX

		sb.Append("internal static partial class Mock").AppendLine();
		sb.Append("{").AppendLine();
		sb.AppendXmlSummary($"A mock implementation for <see cref=\"{escapedClassName}\" />.", "\t");
		sb.Append(
				"\t[global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)]")
			.AppendLine();
#if !DEBUG
		sb.Append("\t[global::System.Diagnostics.DebuggerNonUserCode]").AppendLine();
#endif
		sb.Append("\t[global::System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]").AppendLine();
		sb.Append("\tinternal class ").Append(name).Append(" :").AppendLine();
		sb.Append("\t\t").Append(@class.ClassFullName);
		sb.Append(", IMockFor").Append(name).Append(", IMockSetupFor").Append(name);
		if (hasProtectedMembers)
		{
			sb.Append(", IMockProtectedSetupFor").Append(name);
			sb.Append(", global::Mockolate.MockExtensionsFor").Append(name).Append(".IMockSetupInitializationFor")
				.Append(name);
		}

		if (hasStaticMembers)
		{
			sb.Append(", IMockStaticSetupFor").Append(name);
		}

		if (hasEvents)
		{
			sb.Append(", IMockRaiseOn").Append(name);
		}

		if (hasProtectedEvents)
		{
			sb.Append(", IMockProtectedRaiseOn").Append(name);
		}

		if (hasStaticEvents)
		{
			sb.Append(", IMockStaticRaiseOn").Append(name);
		}

		sb.Append(", IMockVerifyFor").Append(name);

		if (hasProtectedMembers || hasProtectedEvents)
		{
			sb.Append(", IMockProtectedVerifyFor").Append(name);
		}

		if (hasStaticMembers || hasStaticEvents)
		{
			sb.Append(", IMockStaticVerifyFor").Append(name);
		}

		sb.Append(',').AppendLine();

		sb.Append("\t\tglobal::Mockolate.IMock").AppendLine();
		sb.Append("\t{").AppendLine();

		sb.Append("\t\t/// <inheritdoc />").AppendLine();
		sb.Append(
				"\t\t[global::System.Diagnostics.DebuggerBrowsable(global::System.Diagnostics.DebuggerBrowsableState.Never)]")
			.AppendLine();
		sb.Append("\t\tglobal::Mockolate.MockRegistry global::Mockolate.IMock.MockRegistry => this.")
			.Append(mockRegistryName).Append(';').AppendLine();
		if (constructors?.Count > 0)
		{
			sb.Append("\t\tprivate global::Mockolate.MockRegistry ").Append(mockRegistryName).AppendLine();
			sb.Append("\t\t{").AppendLine();
			sb.Append("\t\t\tget => field ?? MockRegistryProvider.Value;").AppendLine();
			sb.Append("\t\t\tset;").AppendLine();
			sb.Append("\t\t}").AppendLine();
			sb.Append(
					"\t\t[global::System.Diagnostics.DebuggerBrowsable(global::System.Diagnostics.DebuggerBrowsableState.Never)]")
				.AppendLine();
			sb.Append(
					"\t\tinternal static readonly global::System.Threading.AsyncLocal<global::Mockolate.MockRegistry> MockRegistryProvider = new global::System.Threading.AsyncLocal<global::Mockolate.MockRegistry>();")
				.AppendLine();
		}
		else
		{
			sb.Append("\t\tprivate global::Mockolate.MockRegistry ").Append(mockRegistryName).Append(" { get; }")
				.AppendLine();
			if (hasStaticMembers)
			{
				sb.Append(
						"\t\t[global::System.Diagnostics.DebuggerBrowsable(global::System.Diagnostics.DebuggerBrowsableState.Never)]")
					.AppendLine();
				sb.Append(
						"\t\tinternal static readonly global::System.Threading.AsyncLocal<global::Mockolate.MockRegistry> MockRegistryProvider = new global::System.Threading.AsyncLocal<global::Mockolate.MockRegistry>();")
					.AppendLine();
			}
		}

		sb.AppendLine();
		ImplementMockForInterface(sb, mockRegistryName, name, hasEvents, hasProtectedMembers, hasProtectedEvents,
			hasStaticMembers, hasStaticEvents);

		sb.Append("\t\t/// <inheritdoc />").AppendLine();
		sb.Append("\t\tstring global::Mockolate.IMock.ToString()").AppendLine();
		sb.Append("\t\t\t=> \"").Append(@class.DisplayString).Append(" mock\";").AppendLine();
		sb.AppendLine();

		if (@class.IsInterface)
		{
			sb.Append("\t\t/// <inheritdoc cref=\"").Append(name).Append("\" />").AppendLine();
			sb.Append("\t\tpublic ").Append(name).Append("(global::Mockolate.MockRegistry mockRegistry)").AppendLine();
			sb.Append("\t\t{").AppendLine();
			sb.Append("\t\t\tthis.").Append(mockRegistryName).Append(" = mockRegistry;").AppendLine();
			if (hasStaticMembers)
			{
				sb.Append("\t\t\tMockRegistryProvider.Value = mockRegistry;").AppendLine();
			}

			sb.Append("\t\t}").AppendLine();
			sb.AppendLine();
		}
		else if (constructors is not null)
		{
			foreach (Method constructor in constructors)
			{
				AppendMockSubject_BaseClassConstructor(sb, mockRegistryName, name, constructor,
					@class.HasRequiredMembers);
			}
		}
		else
		{
			sb.Append("\t\t/// <inheritdoc cref=\"").Append(name).Append("\" />").AppendLine();
			sb.Append("\t\tpublic ").Append(name).Append("(global::Mockolate.MockRegistry mockRegistry)").AppendLine();
			sb.Append("\t\t{").AppendLine();
			sb.Append("\t\t\tthis.").Append(mockRegistryName).Append(" = mockRegistry;").AppendLine();
			sb.Append("\t\t}").AppendLine();
			sb.AppendLine();
		}

		AppendMockSubject_ImplementClass(sb, @class, mockRegistryName, null);
		sb.AppendLine();

		#region IMockSetupForXXX

		sb.Append("\t\t#region IMockSetupFor").Append(name).AppendLine();
		sb.AppendLine();
		ImplementSetupInterface(sb, @class, mockRegistryName, $"IMockSetupFor{name}", MemberType.Public);
		sb.Append("\t\t#endregion IMockSetupFor").Append(name).AppendLine();

		if (hasProtectedMembers)
		{
			sb.AppendLine();
			sb.Append("\t\t#region IMockProtectedSetupFor").Append(name).AppendLine();
			sb.AppendLine();
			ImplementSetupInterface(sb, @class, mockRegistryName, $"IMockProtectedSetupFor{name}",
				MemberType.Protected);
			sb.Append("\t\t#endregion IMockProtectedSetupFor").Append(name).AppendLine();
		}

		if (hasStaticMembers)
		{
			sb.AppendLine();
			sb.Append("\t\t#region IMockStaticSetupFor").Append(name).AppendLine();
			sb.AppendLine();
			ImplementSetupInterface(sb, @class, mockRegistryName, $"IMockStaticSetupFor{name}", MemberType.Static);
			sb.Append("\t\t#endregion IMockStaticSetupFor").Append(name).AppendLine();
		}

		#endregion IMockSetupForXXX

		if (hasEvents)
		{
			#region IMockRaiseOnXXX

			sb.AppendLine();
			sb.Append("\t\t#region IMockRaiseOn").Append(name).AppendLine();
			sb.AppendLine();
			ImplementRaiseInterface(sb, @class, mockRegistryName, $"IMockRaiseOn{name}", MemberType.Public);
			sb.Append("\t\t#endregion IMockRaiseOn").Append(name).AppendLine();

			#endregion IMockRaiseOnXXX
		}

		if (hasProtectedEvents)
		{
			#region IMockProtectedRaiseOnXXX

			sb.AppendLine();
			sb.Append("\t\t#region IMockProtectedRaiseOn").Append(name).AppendLine();
			sb.AppendLine();
			ImplementRaiseInterface(sb, @class, mockRegistryName, $"IMockProtectedRaiseOn{name}", MemberType.Protected);
			sb.Append("\t\t#endregion IMockProtectedRaiseOn").Append(name).AppendLine();

			#endregion IMockProtectedRaiseOnXXX
		}

		if (hasStaticEvents)
		{
			#region IMockStaticRaiseOnXXX

			sb.AppendLine();
			sb.Append("\t\t#region IMockStaticRaiseOn").Append(name).AppendLine();
			sb.AppendLine();
			ImplementRaiseInterface(sb, @class, mockRegistryName, $"IMockStaticRaiseOn{name}", MemberType.Static);
			sb.Append("\t\t#endregion IMockStaticRaiseOn").Append(name).AppendLine();

			#endregion IMockStaticRaiseOnXXX
		}

		#region IMockVerifyForXXX

		sb.AppendLine();
		sb.Append("\t\t#region IMockVerifyFor").Append(name).AppendLine();
		sb.AppendLine();
		ImplementVerifyInterface(sb, @class, mockRegistryName, $"IMockVerifyFor{name}", MemberType.Public);
		sb.Append("\t\t#endregion IMockVerifyFor").Append(name).AppendLine();

		if (hasProtectedMembers || hasProtectedEvents)
		{
			sb.AppendLine();
			sb.Append("\t\t#region IMockProtectedVerifyFor").Append(name).AppendLine();
			sb.AppendLine();
			ImplementVerifyInterface(sb, @class, mockRegistryName, $"IMockProtectedVerifyFor{name}",
				MemberType.Protected);
			sb.Append("\t\t#endregion IMockProtectedVerifyFor").Append(name).AppendLine();
		}

		if (hasStaticMembers || hasStaticEvents)
		{
			sb.AppendLine();
			sb.Append("\t\t#region IMockStaticVerifyFor").Append(name).AppendLine();
			sb.AppendLine();
			ImplementVerifyInterface(sb, @class, mockRegistryName, $"IMockStaticVerifyFor{name}", MemberType.Static);
			sb.Append("\t\t#endregion IMockStaticVerifyFor").Append(name).AppendLine();
		}

		#endregion IMockVerifyForXXX

		sb.AppendLine("\t}");

		sb.AppendLine();
#if !DEBUG
		sb.Append("\t[global::System.Diagnostics.DebuggerNonUserCode]").AppendLine();
#endif
		sb.Append("\t[global::System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]").AppendLine();
		sb.Append("\tprivate sealed class VerifyMonitor").Append(name)
			.Append("(global::Mockolate.MockRegistry mockRegistry) : global::Mockolate.Mock.IMockVerifyFor")
			.Append(name).AppendLine();
		sb.Append("\t{").AppendLine();
		sb.Append("\t\tprivate global::Mockolate.MockRegistry ").Append(mockRegistryName)
			.Append(" { get; } = mockRegistry;").AppendLine();
		sb.AppendLine();
		sb.Append("\t\t#region IMockVerifyFor").Append(name).AppendLine();
		sb.AppendLine();
		ImplementVerifyInterface(sb, @class, mockRegistryName, $"IMockVerifyFor{name}", MemberType.Public);
		sb.Append("\t\t#endregion IMockVerifyFor").Append(name).AppendLine();
		sb.Append("\t}").AppendLine();

		sb.AppendLine();
#if !DEBUG
		sb.Append("\t[global::System.Diagnostics.DebuggerNonUserCode]").AppendLine();
#endif
		sb.Append("\t[global::System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]").AppendLine();
		sb.Append("\tprivate sealed class MockInScenarioFor").Append(name)
			.Append(" : global::Mockolate.Mock.IMockInScenarioFor").Append(name)
			.Append(", global::Mockolate.Mock.IMockSetupFor").Append(name);
		if (hasProtectedMembers)
		{
			sb.Append(", global::Mockolate.Mock.IMockProtectedSetupFor").Append(name);
		}

		sb.AppendLine();
		sb.Append("\t{").AppendLine();
		sb.Append("\t\tprivate global::Mockolate.MockRegistry ").Append(mockRegistryName).Append(" { get; }").AppendLine();
		sb.Append("\t\tprivate string _scenarioName;").AppendLine();
		sb.AppendLine();
		sb.Append("\t\tpublic MockInScenarioFor").Append(name)
			.Append("(global::Mockolate.MockRegistry mockRegistry, string scenario)").AppendLine();
		sb.Append("\t\t{").AppendLine();
		sb.Append("\t\t\tthis.").Append(mockRegistryName).Append(" = mockRegistry;").AppendLine();
		sb.Append("\t\t\t_scenarioName = scenario;").AppendLine();
		sb.Append("\t\t}").AppendLine();
		sb.AppendLine();
		sb.Append("\t\t/// <inheritdoc />").AppendLine();
		sb.Append("\t\tglobal::Mockolate.Mock.IMockSetupFor").Append(name)
			.Append(" global::Mockolate.Mock.IMockInScenarioFor").Append(name).Append(".Setup").AppendLine();
		sb.Append("\t\t\t=> this;").AppendLine();
		sb.AppendLine();
		if (hasProtectedMembers)
		{
			sb.Append("\t\t/// <inheritdoc />").AppendLine();
			sb.Append("\t\tglobal::Mockolate.Mock.IMockProtectedSetupFor").Append(name)
				.Append(" global::Mockolate.Mock.IMockInScenarioFor").Append(name).Append(".SetupProtected").AppendLine();
			sb.Append("\t\t\t=> this;").AppendLine();
			sb.AppendLine();
		}

		sb.Append("\t\t#region IMockSetupFor").Append(name).AppendLine();
		sb.AppendLine();
		ImplementSetupInterface(sb, @class, mockRegistryName, $"IMockSetupFor{name}", MemberType.Public, "_scenarioName");
		sb.Append("\t\t#endregion IMockSetupFor").Append(name).AppendLine();
		if (hasProtectedMembers)
		{
			sb.AppendLine();
			sb.Append("\t\t#region IMockProtectedSetupFor").Append(name).AppendLine();
			sb.AppendLine();
			ImplementSetupInterface(sb, @class, mockRegistryName, $"IMockProtectedSetupFor{name}", MemberType.Protected, "_scenarioName");
			sb.Append("\t\t#endregion IMockProtectedSetupFor").Append(name).AppendLine();
		}

		sb.Append("\t}").AppendLine();

		#endregion MockForXXX

		sb.AppendLine();

		#region IMockForXXX

		sb.AppendXmlSummary($"The Mockolate accessor for a mock of <see cref=\"{escapedClassName}\" />, reached through <c>.Mock</c> on the mocked instance.", "\t");
		sb.AppendXmlRemarks([
			"Groups every operation that acts on the mock rather than on the mocked subject: setups, verifications, event raising, scenarios and monitoring.",
		], "\t");
		sb.Append("\tinternal interface IMockFor").Append(name).AppendLine();
		sb.Append("\t{").AppendLine();
		sb.AppendXmlSummary($"Configures how members of the mock of <see cref=\"{escapedClassName}\" /> respond when invoked.");
		sb.AppendXmlRemarks([
			"Each mocked member is available as a strongly-typed entry on this surface. Chain <c>Returns</c>, <c>ReturnsAsync</c>, <c>Throws</c>, <c>ThrowsAsync</c> or <c>Do</c> to control the response; chain <c>InitializeWith</c>/<c>Register</c> to initialize properties and indexers; chain multiple returns/throws to define a sequence; use <c>.For(n)</c>, <c>.Only(n)</c>, <c>.Forever()</c>, <c>.When(predicate)</c> to control when a callback runs.",
			"When two setups overlap, the most recently defined one wins.",
		]);
		sb.Append("\t\tIMockSetupFor").Append(name).Append(" Setup { get; }").AppendLine();
		sb.AppendLine();
		if (hasProtectedMembers)
		{
			sb.AppendXmlSummary($"Configures how <see langword=\"protected\" /> virtual members of the mock of <see cref=\"{escapedClassName}\" /> respond when invoked.");
			sb.AppendXmlRemarks([
				"Only members declared as <see langword=\"protected\" /> (or <see langword=\"protected\" /> <see langword=\"internal\" />) on the mocked class appear here. All setup chain operators (<c>Returns</c>, <c>Throws</c>, <c>Do</c>, sequences, <c>.For</c>/<c>.Only</c>/<c>.Forever</c>, ...) work identically to <see cref=\"Setup\" />.",
			]);
			sb.Append("\t\tIMockProtectedSetupFor").Append(name).Append(" SetupProtected { get; }").AppendLine();
			sb.AppendLine();
		}

		if (hasStaticMembers)
		{
			sb.AppendXmlSummary($"Configures how <see langword=\"static\" /> members declared on <see cref=\"{escapedClassName}\" /> respond when invoked.");
			sb.AppendXmlRemarks([
				"Static members are scoped per async/execution flow while the mock is alive; invocations from other flows are not intercepted.",
			]);
			sb.Append("\t\tIMockStaticSetupFor").Append(name).Append(" SetupStatic { get; }").AppendLine();
			sb.AppendLine();
		}

		sb.AppendXmlSummary($"Opens a named scenario scope on the mock of <see cref=\"{escapedClassName}\" /> so that additional setups can be registered for that scenario.");
		sb.AppendXmlRemarks([
			"Scenarios let you define per-state behavior. Setups registered inside the returned <c>IMockInScenarioFor...</c> scope only apply while the mock's current scenario matches <paramref name=\"scenario\" />; switch scenarios with <see cref=\"TransitionTo\" />.",
		]);
		sb.AppendXmlParam("scenario", "Name of the scenario to enter. Any non-null string acts as a key; the mock starts in an unnamed default scenario.");
		sb.AppendXmlReturns("A scoped accessor whose <c>Setup</c> (and <c>SetupProtected</c>, where applicable) register scenario-specific setups.");
		sb.Append("\t\tIMockInScenarioFor").Append(name).Append(" InScenario(string scenario);").AppendLine();
		sb.AppendLine();

		sb.AppendXmlSummary($"Opens a named scenario scope on the mock of <see cref=\"{escapedClassName}\" /> and immediately invokes <paramref name=\"setup\" /> to register scenario-specific setups.");
		sb.AppendXmlRemarks([
			"Equivalent to <c>InScenario(scenario)</c> followed by the setup callback, but returns the original <c>IMockFor...</c> accessor so it chains nicely at mock-creation time.",
		]);
		sb.AppendXmlParam("scenario", "Name of the scenario to enter.");
		sb.AppendXmlParam("setup", "Callback that receives the scenario-scoped setup surface and registers scenario-specific setups.");
		sb.AppendXmlReturns("This accessor, to allow chaining.");
		sb.Append("\t\tIMockFor").Append(name).Append(" InScenario(string scenario, global::System.Action<IMockInScenarioFor")
			.Append(name).Append("> setup);").AppendLine();
		sb.AppendLine();

		sb.AppendXmlSummary($"Switches the active scenario of the mock of <see cref=\"{escapedClassName}\" /> to <paramref name=\"scenario\" />.");
		sb.AppendXmlRemarks([
			"After the transition, setups registered via <see cref=\"InScenario(string)\" /> under that scenario take effect. Scenarios that have no matching setup for a given member fall back to the default (un-scoped) setups.",
		]);
		sb.AppendXmlParam("scenario", "Name of the scenario to transition to.");
		sb.AppendXmlReturns("This accessor, to allow chaining.");
		sb.Append("\t\tIMockFor").Append(name).Append(" TransitionTo(string scenario);").AppendLine();
		sb.AppendLine();

		if (hasEvents)
		{
			sb.AppendXmlSummary($"Triggers events declared on <see cref=\"{escapedClassName}\" /> so that currently subscribed handlers are invoked.");
			sb.AppendXmlRemarks([
				"One entry per event is generated; the signature matches the event's delegate. Only handlers that are subscribed at the moment of the <c>Raise</c> call are invoked - handlers subscribed later (or already removed) are skipped.",
			]);
			sb.Append("\t\tIMockRaiseOn").Append(name).Append(" Raise { get; }").AppendLine();
			sb.AppendLine();
		}

		if (hasProtectedEvents)
		{
			sb.AppendXmlSummary($"Triggers <see langword=\"protected\" /> events declared on <see cref=\"{escapedClassName}\" /> so that currently subscribed handlers are invoked.");
			sb.AppendXmlRemarks([
				"Same semantics as <see cref=\"Raise\" /> but for events whose accessibility prevents external subscription from outside the class. Useful when testing code that subclasses the mocked type.",
			]);
			sb.Append("\t\tIMockProtectedRaiseOn").Append(name).Append(" RaiseProtected { get; }").AppendLine();
			sb.AppendLine();
		}

		if (hasStaticEvents)
		{
			sb.AppendXmlSummary($"Triggers <see langword=\"static\" /> events declared on <see cref=\"{escapedClassName}\" /> so that currently subscribed handlers are invoked.");
			sb.AppendXmlRemarks([
				"Static events are scoped per async/execution flow while the mock is alive.",
			]);
			sb.Append("\t\tIMockStaticRaiseOn").Append(name).Append(" RaiseStatic { get; }").AppendLine();
			sb.AppendLine();
		}

		sb.AppendXmlSummary($"Asserts how often, and in which order, members of the mock of <see cref=\"{escapedClassName}\" /> were invoked.");
		sb.AppendXmlRemarks([
			"Each call to a member here returns a <c>VerificationResult</c> that you terminate with a count assertion: <c>Never()</c>, <c>Once()</c>, <c>Twice()</c>, <c>Exactly(n)</c>, <c>AtLeast(n)</c>/<c>AtLeastOnce()</c>/<c>AtLeastTwice()</c>, <c>AtMost(n)</c>/<c>AtMostOnce()</c>/<c>AtMostTwice()</c>, <c>Between(min, max)</c> or <c>Times(predicate)</c>.",
			"Use <c>Within(TimeSpan)</c> / <c>WithCancellation(CancellationToken)</c> before the terminator to wait for expected interactions that happen on background threads.",
			"Chain <c>Then(...)</c> to assert an ordered sequence of calls. A failing assertion throws a <see cref=\"global::Mockolate.Exceptions.MockVerificationException\" />.",
		]);
		sb.Append("\t\tIMockVerifyFor").Append(name).Append(" Verify { get; }").AppendLine();
		sb.AppendLine();
		if (hasProtectedMembers || hasProtectedEvents)
		{
			sb.AppendXmlSummary($"Asserts how often, and in which order, <see langword=\"protected\" /> members of the mock of <see cref=\"{escapedClassName}\" /> were invoked.");
			sb.AppendXmlRemarks([
				"Same terminators and modifiers as <see cref=\"Verify\" /> (<c>Once()</c>, <c>Exactly(n)</c>, <c>Within(...)</c>, <c>Then(...)</c>, ...); applies to <see langword=\"protected\" /> members and events instead of public ones.",
			]);
			sb.Append("\t\tIMockProtectedVerifyFor").Append(name).Append(" VerifyProtected { get; }").AppendLine();
			sb.AppendLine();
		}

		if (hasStaticMembers || hasStaticEvents)
		{
			sb.AppendXmlSummary($"Asserts how often, and in which order, <see langword=\"static\" /> members declared on <see cref=\"{escapedClassName}\" /> were invoked.");
			sb.AppendXmlRemarks([
				"Same terminators and modifiers as <see cref=\"Verify\" />; scoped per async/execution flow in the same way as <see cref=\"SetupStatic\" />.",
			]);
			sb.Append("\t\tIMockStaticVerifyFor").Append(name).Append(" VerifyStatic { get; }").AppendLine();
			sb.AppendLine();
		}

		sb.AppendXmlSummary("Verifies how often a specific method setup was matched by actual invocations.");
		sb.AppendXmlRemarks([
			"Useful when you want to verify &quot;this <em>particular</em> setup was hit N times&quot; without re-stating the matchers. Chain the usual count terminators (<c>Once()</c>, <c>AtLeastOnce()</c>, <c>Exactly(n)</c>, ...) on the returned result.",
		]);
		sb.AppendXmlParam("setup", "The setup previously registered through <see cref=\"Setup\" /> (typically returned from a <c>Returns(...)</c>/<c>Throws(...)</c> call).");
		sb.AppendXmlReturns("A <c>VerificationResult</c> that counts invocations matching the given setup.");
		sb.Append("\t\tglobal::Mockolate.Verify.VerificationResult<IMockVerifyFor").Append(name)
			.Append("> VerifySetup(global::Mockolate.Setup.IMethodSetup setup);").AppendLine();
		sb.AppendLine();
		sb.AppendXmlSummary("Checks whether every recorded interaction on this mock has been observed by at least one <c>Verify</c> call.");
		sb.AppendXmlRemarks([
			"Useful in test teardown to catch unexpected interactions (&quot;strict verification&quot;): if any recorded call has never been matched by a verification, the method returns <see langword=\"false\" />.",
		]);
		sb.AppendXmlReturns("<see langword=\"true\" /> if every recorded interaction was verified at least once; otherwise <see langword=\"false\" />.");
		sb.Append("\t\tbool VerifyThatAllInteractionsAreVerified();").AppendLine();
		sb.AppendLine();
		sb.AppendXmlSummary("Checks whether every registered setup on this mock was matched by at least one actual invocation.");
		sb.AppendXmlRemarks([
			"Useful to catch unused setups that silently rot as the test subject evolves.",
		]);
		sb.AppendXmlReturns("<see langword=\"true\" /> if every registered setup was used at least once; otherwise <see langword=\"false\" />.");
		sb.Append("\t\tbool VerifyThatAllSetupsAreUsed();").AppendLine();
		sb.AppendLine();
		sb.AppendXmlSummary("Removes every recorded interaction from this mock while keeping all registered setups intact.");
		sb.AppendXmlRemarks([
			"Handy when a single test exercises multiple logical phases and you only want to verify the interactions of the latest phase.",
		]);
		sb.Append("\t\tvoid ClearAllInteractions();").AppendLine();
		sb.AppendLine();
		sb.AppendXmlSummary(
			"Creates a monitor whose <c>Verify</c> surface is scoped to interactions produced between <c>monitor.Run()</c> and the disposal of its <see cref=\"global::System.IDisposable\" /> scope.");
		sb.AppendXmlRemarks([
			"The underlying mock keeps recording all interactions as usual - only the monitor's <c>Verify</c> view is scoped. Useful to verify only the interactions produced by a specific block of test code without resetting the mock.",
		]);
		sb.AppendXmlReturns("A <see cref=\"global::Mockolate.Monitor.MockMonitor{T}\" /> that exposes <c>Verify</c> over the monitored interactions and a <c>Run()</c> method that opens the recording scope.");
		sb.Append("\t\tglobal::Mockolate.Monitor.MockMonitor<IMockVerifyFor").Append(name).Append("> Monitor();")
			.AppendLine();
		sb.Append("\t}").AppendLine();

		#endregion IMockForXXX

		sb.AppendLine();

		#region IMockInScenarioForXXX

		sb.AppendXmlSummary($"Scoped access to setups for a scenario on the mock of <see cref=\"{escapedClassName}\" />.", "\t");
		sb.Append("\tinternal interface IMockInScenarioFor").Append(name).AppendLine();
		sb.Append("\t{").AppendLine();
		sb.AppendXmlSummary($"Set up the mock of <see cref=\"{escapedClassName}\" /> within the scenario scope.");
		sb.Append("\t\tIMockSetupFor").Append(name).Append(" Setup { get; }").AppendLine();
		if (hasProtectedMembers)
		{
			sb.AppendLine();
			sb.AppendXmlSummary($"Set up protected members of the mock of <see cref=\"{escapedClassName}\" /> within the scenario scope.");
			sb.Append("\t\tIMockProtectedSetupFor").Append(name).Append(" SetupProtected { get; }").AppendLine();
		}

		sb.Append("\t}").AppendLine();

		#endregion IMockInScenarioForXXX

		sb.AppendLine();

		#region IMockSetupForXXX

		sb.AppendXmlSummary($"Set up the mock of <see cref=\"{escapedClassName}\" />.", "\t");
		sb.Append("\tinternal interface IMockSetupFor").Append(name);
		if (!hasStaticMembers)
		{
			sb.Append(" : global::Mockolate.Setup.IMockSetup<").Append(@class.ClassFullName).Append(">").AppendLine();
		}
		else
		{
			sb.AppendLine();
		}

		sb.Append("\t{").AppendLine();
		DefineSetupInterface(sb, @class, MemberType.Public, hasOverloadResolutionPriority);
		sb.Append("\t}").AppendLine();
		sb.AppendLine();

		if (hasProtectedMembers)
		{
			sb.AppendXmlSummary($"Set up protected members for the mock of <see cref=\"{escapedClassName}\" />.", "\t");
			sb.Append("\tinternal interface IMockProtectedSetupFor").Append(name).AppendLine();
			sb.Append("\t{").AppendLine();
			DefineSetupInterface(sb, @class, MemberType.Protected, hasOverloadResolutionPriority);
			sb.Append("\t}").AppendLine();
			sb.AppendLine();
		}

		if (hasStaticMembers)
		{
			sb.AppendXmlSummary($"Set up static members for the mock of <see cref=\"{escapedClassName}\" />.", "\t");
			sb.Append("\tinternal interface IMockStaticSetupFor").Append(name).AppendLine();
			sb.Append("\t{").AppendLine();
			DefineSetupInterface(sb, @class, MemberType.Static, hasOverloadResolutionPriority);
			sb.Append("\t}").AppendLine();
			sb.AppendLine();
		}

		#endregion IMockSetupForXXX

		if (hasEvents)
		{
			#region IMockRaiseOnXXX

			sb.AppendXmlSummary($"Raise events on the mock of <see cref=\"{escapedClassName}\" />.", "\t");
			sb.Append("\tinternal interface IMockRaiseOn").Append(name).AppendLine();
			sb.Append("\t{").AppendLine();
			DefineRaiseInterface(sb, @class, MemberType.Public);
			sb.Append("\t}").AppendLine();
			sb.AppendLine();

			#endregion IMockRaiseOnXXX
		}

		if (hasProtectedEvents)
		{
			#region IMockProtectedRaiseOnXXX

			sb.AppendXmlSummary($"Raise protected events on the mock of <see cref=\"{escapedClassName}\" />.", "\t");
			sb.Append("\tinternal interface IMockProtectedRaiseOn").Append(name).AppendLine();
			sb.Append("\t{").AppendLine();
			DefineRaiseInterface(sb, @class, MemberType.Protected);
			sb.Append("\t}").AppendLine();
			sb.AppendLine();

			#endregion IMockProtectedRaiseOnXXX
		}

		if (hasStaticEvents)
		{
			#region IMockStaticRaiseOnXXX

			sb.AppendXmlSummary($"Raise static events on the mock of <see cref=\"{escapedClassName}\" />.", "\t");
			sb.Append("\tinternal interface IMockStaticRaiseOn").Append(name).AppendLine();
			sb.Append("\t{").AppendLine();
			DefineRaiseInterface(sb, @class, MemberType.Static);
			sb.Append("\t}").AppendLine();
			sb.AppendLine();

			#endregion IMockStaticRaiseOnXXX
		}

		#region IMockVerifyForXXX

		sb.AppendXmlSummary($"Verify interactions with the mock of <see cref=\"{escapedClassName}\" />.", "\t");
		sb.Append("\tinternal interface IMockVerifyFor").Append(name);
		if (!hasStaticMembers)
		{
			sb.Append(" : global::Mockolate.Verify.IMockVerify<").Append(@class.ClassFullName).Append(">").AppendLine();
		}
		else
		{
			sb.AppendLine();
		}

		sb.Append("\t{").AppendLine();
		DefineVerifyInterface(sb, @class, $"IMockVerifyFor{name}", MemberType.Public, hasOverloadResolutionPriority);
		sb.Append("\t}").AppendLine();

		if (hasProtectedMembers || hasProtectedEvents)
		{
			sb.AppendLine();
			sb.AppendXmlSummary($"Verify protected interactions with the mock of <see cref=\"{escapedClassName}\" />.",
				"\t");
			sb.Append("\tinternal interface IMockProtectedVerifyFor").Append(name).AppendLine();
			sb.Append("\t{").AppendLine();
			DefineVerifyInterface(sb, @class, $"IMockProtectedVerifyFor{name}", MemberType.Protected,
				hasOverloadResolutionPriority);
			sb.Append("\t}").AppendLine();
		}

		if (hasStaticMembers || hasStaticEvents)
		{
			sb.AppendLine();
			sb.AppendXmlSummary($"Verify static interactions with the mock of <see cref=\"{escapedClassName}\" />.",
				"\t");
			sb.Append("\tinternal interface IMockStaticVerifyFor").Append(name).AppendLine();
			sb.Append("\t{").AppendLine();
			DefineVerifyInterface(sb, @class, $"IMockStaticVerifyFor{name}", MemberType.Static,
				hasOverloadResolutionPriority);
			sb.Append("\t}").AppendLine();
		}

		#endregion IMockVerifyForXXX

		sb.Append("}").AppendLine();
		sb.AppendLine();
		sb.AppendLine("#nullable disable annotations");
		return sb.ToString();
	}

#pragma warning disable S107 // Methods should not have too many parameters
	private static void ImplementMockForInterface(StringBuilder sb, string mockRegistryName, string name,
		bool hasEvents, bool hasProtectedMembers, bool hasProtectedEvents, bool hasStaticMembers, bool hasStaticEvents)
	{
		sb.Append("\t\t/// <inheritdoc />").AppendLine();
		sb.Append(
				"\t\t[global::System.Diagnostics.DebuggerBrowsable(global::System.Diagnostics.DebuggerBrowsableState.Never)]")
			.AppendLine();
		sb.Append("\t\tIMockSetupFor").Append(name).Append(" IMockFor").Append(name).Append(".Setup").AppendLine();
		sb.Append("\t\t\t=> this;").AppendLine();

		if (hasProtectedMembers)
		{
			sb.Append("\t\t/// <inheritdoc />").AppendLine();
			sb.Append(
					"\t\t[global::System.Diagnostics.DebuggerBrowsable(global::System.Diagnostics.DebuggerBrowsableState.Never)]")
				.AppendLine();
			sb.Append("\t\tIMockProtectedSetupFor").Append(name).Append(" IMockFor").Append(name)
				.Append(".SetupProtected").AppendLine();
			sb.Append("\t\t\t=> this;").AppendLine();

			sb.Append("\t\t/// <inheritdoc />").AppendLine();
			sb.Append(
					"\t\t[global::System.Diagnostics.DebuggerBrowsable(global::System.Diagnostics.DebuggerBrowsableState.Never)]")
				.AppendLine();
			sb.Append("\t\tIMockProtectedSetupFor").Append(name).Append(" global::Mockolate.MockExtensionsFor")
				.Append(name).Append(".IMockSetupInitializationFor").Append(name).Append(".Protected").AppendLine();
			sb.Append("\t\t\t=> this;").AppendLine();
		}

		if (hasStaticMembers)
		{
			sb.Append("\t\t/// <inheritdoc />").AppendLine();
			sb.Append(
					"\t\t[global::System.Diagnostics.DebuggerBrowsable(global::System.Diagnostics.DebuggerBrowsableState.Never)]")
				.AppendLine();
			sb.Append("\t\tIMockStaticSetupFor").Append(name).Append(" IMockFor").Append(name).Append(".SetupStatic")
				.AppendLine();
			sb.Append("\t\t\t=> this;").AppendLine();
		}

		sb.Append("\t\t/// <inheritdoc />").AppendLine();
		sb.Append("\t\tIMockInScenarioFor").Append(name).Append(" IMockFor").Append(name)
			.Append(".InScenario(string scenario)").AppendLine();
		sb.Append("\t\t\t=> new MockInScenarioFor").Append(name).Append("(this.").Append(mockRegistryName)
			.Append(", scenario);").AppendLine();
		sb.AppendLine();

		sb.Append("\t\t/// <inheritdoc />").AppendLine();
		sb.Append("\t\tIMockFor").Append(name).Append(" IMockFor").Append(name)
			.Append(".InScenario(string scenario, global::System.Action<IMockInScenarioFor").Append(name)
			.Append("> setup)").AppendLine();
		sb.Append("\t\t{").AppendLine();
		sb.Append("\t\t\tsetup.Invoke(new MockInScenarioFor").Append(name).Append("(this.").Append(mockRegistryName)
			.Append(", scenario));").AppendLine();
		sb.Append("\t\t\treturn this;").AppendLine();
		sb.Append("\t\t}").AppendLine();
		sb.AppendLine();

		sb.Append("\t\t/// <inheritdoc />").AppendLine();
		sb.Append("\t\tIMockFor").Append(name).Append(" IMockFor").Append(name)
			.Append(".TransitionTo(string scenario)").AppendLine();
		sb.Append("\t\t{").AppendLine();
		sb.Append("\t\t\tthis.").Append(mockRegistryName).Append(".TransitionTo(scenario);").AppendLine();
		sb.Append("\t\t\treturn this;").AppendLine();
		sb.Append("\t\t}").AppendLine();

		if (hasEvents)
		{
			sb.Append("\t\t/// <inheritdoc />").AppendLine();
			sb.Append(
					"\t\t[global::System.Diagnostics.DebuggerBrowsable(global::System.Diagnostics.DebuggerBrowsableState.Never)]")
				.AppendLine();
			sb.Append("\t\tIMockRaiseOn").Append(name).Append(" IMockFor").Append(name).Append(".Raise").AppendLine();
			sb.Append("\t\t\t=> this;").AppendLine();
		}

		if (hasProtectedEvents)
		{
			sb.Append("\t\t/// <inheritdoc />").AppendLine();
			sb.Append(
					"\t\t[global::System.Diagnostics.DebuggerBrowsable(global::System.Diagnostics.DebuggerBrowsableState.Never)]")
				.AppendLine();
			sb.Append("\t\tIMockProtectedRaiseOn").Append(name).Append(" IMockFor").Append(name)
				.Append(".RaiseProtected").AppendLine();
			sb.Append("\t\t\t=> this;").AppendLine();
		}

		if (hasStaticEvents)
		{
			sb.Append("\t\t/// <inheritdoc />").AppendLine();
			sb.Append(
					"\t\t[global::System.Diagnostics.DebuggerBrowsable(global::System.Diagnostics.DebuggerBrowsableState.Never)]")
				.AppendLine();
			sb.Append("\t\tIMockStaticRaiseOn").Append(name).Append(" IMockFor").Append(name).Append(".RaiseStatic")
				.AppendLine();
			sb.Append("\t\t\t=> this;").AppendLine();
		}

		sb.Append("\t\t/// <inheritdoc />").AppendLine();
		sb.Append(
				"\t\t[global::System.Diagnostics.DebuggerBrowsable(global::System.Diagnostics.DebuggerBrowsableState.Never)]")
			.AppendLine();
		sb.Append("\t\tIMockVerifyFor").Append(name).Append(" IMockFor").Append(name).Append(".Verify").AppendLine();
		sb.Append("\t\t\t=> this;").AppendLine();

		if (hasProtectedMembers || hasProtectedEvents)
		{
			sb.Append("\t\t/// <inheritdoc />").AppendLine();
			sb.Append(
					"\t\t[global::System.Diagnostics.DebuggerBrowsable(global::System.Diagnostics.DebuggerBrowsableState.Never)]")
				.AppendLine();
			sb.Append("\t\tIMockProtectedVerifyFor").Append(name).Append(" IMockFor").Append(name)
				.Append(".VerifyProtected").AppendLine();
			sb.Append("\t\t\t=> this;").AppendLine();
		}

		if (hasStaticMembers || hasStaticEvents)
		{
			sb.Append("\t\t/// <inheritdoc />").AppendLine();
			sb.Append(
					"\t\t[global::System.Diagnostics.DebuggerBrowsable(global::System.Diagnostics.DebuggerBrowsableState.Never)]")
				.AppendLine();
			sb.Append("\t\tIMockStaticVerifyFor").Append(name).Append(" IMockFor").Append(name).Append(".VerifyStatic")
				.AppendLine();
			sb.Append("\t\t\t=> this;").AppendLine();
		}

		sb.Append("\t\t/// <inheritdoc />").AppendLine();
		sb.Append("\t\tglobal::Mockolate.Verify.VerificationResult<IMockVerifyFor").Append(name).Append("> IMockFor")
			.Append(name).Append(".VerifySetup(global::Mockolate.Setup.IMethodSetup setup)").AppendLine();
		sb.Append("\t\t\t=> this.").Append(mockRegistryName).Append(".Method<IMockVerifyFor").Append(name)
			.Append(">(this, setup);").AppendLine();

		sb.Append("\t\t/// <inheritdoc />").AppendLine();
		sb.Append("\t\tbool IMockFor").Append(name).Append(".VerifyThatAllInteractionsAreVerified()").AppendLine();
		sb.Append("\t\t\t=> this.").Append(mockRegistryName)
			.Append(".Interactions.GetUnverifiedInteractions().Count == 0;").AppendLine();

		sb.Append("\t\t/// <inheritdoc />").AppendLine();
		sb.Append("\t\tbool IMockFor").Append(name).Append(".VerifyThatAllSetupsAreUsed()").AppendLine();
		sb.Append("\t\t\t=> this.").Append(mockRegistryName).Append(".GetUnusedSetups(this.").Append(mockRegistryName)
			.Append(".Interactions).Count == 0;").AppendLine();

		sb.Append("\t\t/// <inheritdoc />").AppendLine();
		sb.Append("\t\tvoid IMockFor").Append(name).Append(".ClearAllInteractions()").AppendLine();
		sb.Append("\t\t\t=> this.").Append(mockRegistryName).Append(".ClearAllInteractions();").AppendLine();

		sb.Append("\t\t/// <inheritdoc />").AppendLine();
		sb.Append("\t\tglobal::Mockolate.Monitor.MockMonitor<IMockVerifyFor").Append(name).Append("> IMockFor")
			.Append(name).Append(".Monitor()").AppendLine();
		sb.Append("\t\t\t=> new global::Mockolate.Monitor.MockMonitor<IMockVerifyFor").Append(name).Append(">(this.")
			.Append(mockRegistryName).Append(".Interactions, interactions => new VerifyMonitor").Append(name)
			.Append("(new global::Mockolate.MockRegistry(this.").Append(mockRegistryName).Append(", interactions)));")
			.AppendLine();
		sb.AppendLine();
	}
#pragma warning restore S107 // Methods should not have too many parameters

	private static void AppendTypedCreateMockOverloads(StringBuilder sb, Class @class,
		EquatableArray<Method>? constructors, string setupType, string escapedClassName, string createMockReturns)
	{
		if (constructors is null)
		{
			return;
		}

		// Seeded signatures track the hand-written CreateMock overloads so typed overloads that
		// would collide with them are skipped. The key order mirrors the emitted C# signature:
		// "mockBehavior? | setup? | ctor-param-types...".
		HashSet<string> emittedSignatures = new(StringComparer.Ordinal)
		{
			string.Empty,
			"global::Mockolate.MockBehavior",
			$"global::System.Action<{setupType}>",
			$"global::Mockolate.MockBehavior|global::System.Action<{setupType}>",
			"object?[]",
			"global::Mockolate.MockBehavior|object?[]",
			$"global::System.Action<{setupType}>|object?[]",
			$"global::Mockolate.MockBehavior|global::System.Action<{setupType}>|object?[]",
		};

		foreach (Method constructor in constructors.Value)
		{
			if (constructor.Parameters.Count == 0)
			{
				continue;
			}

			if (constructor.Parameters.Any(p => p.RefKind != RefKind.None || p.IsParams))
			{
				continue;
			}

			string mockBehaviorName = CreateUniqueParameterName(constructor.Parameters, "mockBehavior");
			string setupName = CreateUniqueParameterName(constructor.Parameters, "setup");
			string baseSig = string.Join("|",
				constructor.Parameters.Select(p => p.Type.Fullname));

			TryEmitTypedCreateMockOverload(sb, @class, constructor, setupType, escapedClassName, createMockReturns,
				includeMockBehavior: false, includeSetup: false, mockBehaviorName, setupName, baseSig,
				emittedSignatures);
			TryEmitTypedCreateMockOverload(sb, @class, constructor, setupType, escapedClassName, createMockReturns,
				includeMockBehavior: true, includeSetup: false, mockBehaviorName, setupName, baseSig,
				emittedSignatures);
			TryEmitTypedCreateMockOverload(sb, @class, constructor, setupType, escapedClassName, createMockReturns,
				includeMockBehavior: false, includeSetup: true, mockBehaviorName, setupName, baseSig,
				emittedSignatures);
			TryEmitTypedCreateMockOverload(sb, @class, constructor, setupType, escapedClassName, createMockReturns,
				includeMockBehavior: true, includeSetup: true, mockBehaviorName, setupName, baseSig,
				emittedSignatures);
		}
	}

	/// <summary>
	///     Builds an XML-doc cref string and a matching short display text for the given
	///     <paramref name="constructor" /> on <paramref name="class" />. The cref has the form
	///     <c>{class-cref}.{simple-name}({fully-qualified-param-types})</c>; the display has the
	///     form <c>{simple-name}({short-param-types})</c>, intended as the inner text of
	///     <c>&lt;see cref="..."&gt;...&lt;/see&gt;</c> so the rendered prose reads
	///     <c>the MyClass(int) constructor</c> rather than <c>the MyClass.MyClass(int) constructor</c>.
	///     Returns <see langword="null" /> when no valid cref can be produced.
	/// </summary>
	/// <remarks>
	///     Generic classes are skipped because the cref type-parameter-list syntax (e.g. <c>{T}</c>)
	///     expects identifier tokens, not the concrete type arguments that closed generics carry —
	///     emitting <c>MyClass{int}.MyClass(int)</c> would surface CS1584/CS1658 on the consumer side.
	/// </remarks>
	private static (string Cref, string Display)? BuildConstructorCref(Class @class, Method constructor)
	{
		string fullName = @class.ClassFullName;

		if (fullName.IndexOf('<') >= 0)
		{
			return null;
		}

		int lastDot = fullName.LastIndexOf('.');
		string simpleName = lastDot >= 0 ? fullName.Substring(lastDot + 1) : fullName;

		StringBuilder cref = new();
		StringBuilder display = new();
		cref.Append(fullName).Append('.').Append(simpleName).Append('(');
		display.Append(simpleName).Append('(');
		bool first = true;
		foreach (MethodParameter parameter in constructor.Parameters)
		{
			if (!first)
			{
				cref.Append(", ");
				display.Append(", ");
			}

			first = false;
			cref.Append(parameter.Type.Fullname.EscapeForXmlDoc());
			// Inner text of <see> is XML content, so escape '<'/'>' as entities
			// (unlike cref attributes, which use the '{...}' shorthand).
			display.Append(parameter.Type.DisplayName.Replace("<", "&lt;").Replace(">", "&gt;"));
		}

		cref.Append(')');
		display.Append(')');
		return (cref.ToString(), display.ToString());
	}

#pragma warning disable S107 // Methods should not have too many parameters
	private static void TryEmitTypedCreateMockOverload(StringBuilder sb, Class @class, Method constructor,
		string setupType, string escapedClassName, string createMockReturns,
		bool includeMockBehavior, bool includeSetup, string mockBehaviorName, string setupName, string baseSig,
		HashSet<string> emittedSignatures)
	{
		// Build the signature key in the same order as the emitted method signature
		// (mockBehavior, setup, then ctor parameters) so it correctly detects collisions against
		// other typed overloads and against the hand-written seeded overloads.
		string sig = baseSig;
		if (includeSetup)
		{
			sig = $"global::System.Action<{setupType}>|{sig}";
		}

		if (includeMockBehavior)
		{
			sig = $"global::Mockolate.MockBehavior|{sig}";
		}

		if (!emittedSignatures.Add(sig))
		{
			return;
		}

		(string Cref, string Display)? constructorCref = BuildConstructorCref(@class, constructor);
		string ctorPhrase = constructorCref is null
			? "the base-class constructor"
			: $"the <see cref=\"{constructorCref.Value.Cref}\">{constructorCref.Value.Display}</see> constructor";

		if (includeMockBehavior && includeSetup)
		{
			sb.AppendXmlSummary(
				$"Creates a new mock of <see cref=\"{escapedClassName}\" /> using the given <paramref name=\"{mockBehaviorName}\" />, applying the given <paramref name=\"{setupName}\" /> immediately, using the given constructor parameters to invoke {ctorPhrase}.");
			sb.AppendXmlRemarks(
				$"The provided <paramref name=\"{setupName}\" /> is immediately applied to the mock. Use this overload when you want setups to cover virtual interactions triggered inside the constructor.");
		}
		else if (includeMockBehavior)
		{
			sb.AppendXmlSummary(
				$"Creates a new mock of <see cref=\"{escapedClassName}\" /> using the given <paramref name=\"{mockBehaviorName}\" /> and the given constructor parameters to invoke {ctorPhrase}.");
		}
		else if (includeSetup)
		{
			sb.AppendXmlSummary(
				$"Creates a new mock of <see cref=\"{escapedClassName}\" /> applying the given <paramref name=\"{setupName}\" /> immediately, using the given constructor parameters to invoke {ctorPhrase}.");
			sb.AppendXmlRemarks(
				$"The provided <paramref name=\"{setupName}\" /> is immediately applied to the mock. Use this overload when you want setups to cover virtual interactions triggered inside the constructor.");
		}
		else
		{
			sb.AppendXmlSummary(
				$"Creates a new mock of <see cref=\"{escapedClassName}\" /> using the given constructor parameters to invoke {ctorPhrase}.");
		}

		if (includeMockBehavior)
		{
			sb.AppendXmlParam(mockBehaviorName,
				"Controls how the mock responds when members are invoked without a matching setup; see <see cref=\"global::Mockolate.MockBehavior\" />.");
		}

		if (includeSetup)
		{
			sb.AppendXmlParam(setupName,
				"Callback that receives the mock's setup surface and registers initial setups before the mock is returned.");
		}

		foreach (MethodParameter parameter in constructor.Parameters)
		{
			sb.AppendXmlParam(parameter.Name, "Value forwarded to the base-class constructor.");
		}

		sb.AppendXmlReturns(createMockReturns);
		sb.Append("\t\tpublic static ").Append(@class.ClassFullName).Append(" CreateMock(");
		bool needsLeadingComma = false;
		if (includeMockBehavior)
		{
			sb.Append("global::Mockolate.MockBehavior ").Append(mockBehaviorName);
			needsLeadingComma = true;
		}

		if (includeSetup)
		{
			if (needsLeadingComma)
			{
				sb.Append(", ");
			}

			sb.Append("global::System.Action<").Append(setupType).Append("> ").Append(setupName);
			needsLeadingComma = true;
		}

		foreach (MethodParameter parameter in constructor.Parameters)
		{
			if (needsLeadingComma)
			{
				sb.Append(", ");
			}

			needsLeadingComma = true;
			sb.Append(parameter.Type.Fullname).Append(' ').Append(parameter.Name);
			if (parameter.HasExplicitDefaultValue)
			{
				sb.Append(" = ").Append(parameter.ExplicitDefaultValue);
			}
		}

		sb.Append(")").AppendLine();
		sb.Append("\t\t\t=> CreateMock(").Append(includeMockBehavior ? mockBehaviorName : "null").Append(", ")
			.Append(includeSetup ? setupName : "null").Append(", new object?[] { ");
		int argIndex = 0;
		foreach (MethodParameter parameter in constructor.Parameters)
		{
			if (argIndex++ > 0)
			{
				sb.Append(", ");
			}

			sb.Append(parameter.Name);
		}

		sb.Append(" });").AppendLine();
		sb.AppendLine();
	}
#pragma warning restore S107 // Methods should not have too many parameters

	#region Mock Helpers

	private static void AppendMockSubject_BaseClassConstructor(StringBuilder sb, string mockRegistryName, string name,
		Method constructor, bool hasRequiredMembers)
	{
		string mockRegistry = CreateUniqueParameterName(constructor.Parameters, "mockRegistry");
		sb.Append("\t\t/// <inheritdoc cref=\"").Append(name).Append("\" />").AppendLine();
		sb.Append(constructor.Attributes, "\t\t");
		if (hasRequiredMembers && constructor.Attributes?.Any(a => a.Name == "global::System.Diagnostics.CodeAnalysis.SetsRequiredMembers") != true)
		{
			sb.Append("\t\t[global::System.Diagnostics.CodeAnalysis.SetsRequiredMembers]").AppendLine();
		}

		sb.Append("\t\tpublic ").Append(name).Append("(global::Mockolate.MockRegistry ").Append(mockRegistry);
		foreach (MethodParameter parameter in constructor.Parameters)
		{
			sb.Append(", ");
			if (parameter.IsParams)
			{
				sb.Append("params ");
			}

			sb.Append(parameter.Type.Fullname).Append(' ').Append(parameter.Name);
			if (parameter.HasExplicitDefaultValue)
			{
				sb.Append(" = ").Append(parameter.ExplicitDefaultValue);
			}
		}

		sb.Append(")").AppendLine();
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
		sb.Append("\t\t{").AppendLine();
		sb.Append("\t\t\tthis.").Append(mockRegistryName).Append(" = ").Append(mockRegistry).Append(';').AppendLine();
		sb.Append("\t\t}").AppendLine();
		sb.AppendLine();
	}

	private static void AppendMockSubject_ImplementClass(StringBuilder sb, Class @class, string mockRegistryName,
		MockClass? mockClass, Dictionary<string, int>? signatureIndicesOverride = null,
		int[]? nextSignatureIndexRef = null)
	{
		string className = @class.ClassFullName;
		sb.Append("\t\t#region ").Append(@class.DisplayString).AppendLine();
		sb.AppendLine();

		List<Event>? mockEvents = mockClass?.AllEvents().ToList();
		foreach (Event @event in @class.AllEvents())
		{
			if (mockEvents?.All(e => !Event.EqualityComparer.Equals(@event, e)) != false)
			{
				AppendMockSubject_ImplementClass_AddEvent(sb, @event, mockRegistryName, className,
					mockClass is not null,
					@class.IsInterface);
				sb.AppendLine();
			}
		}

		List<Property>? mockProperties = mockClass?.AllProperties().ToList();
		Dictionary<string, int> signatureIndices = signatureIndicesOverride ?? new Dictionary<string, int>();
		int[] nextSignatureIndex = nextSignatureIndexRef ?? [0];
		foreach (Property property in @class.AllProperties())
		{
			if (mockProperties?.All(p => !Property.EqualityComparer.Equals(property, p)) != false)
			{
				int signatureIndex = -1;
				if (property is { IsIndexer: true, IndexerParameters: not null, })
				{
					string signatureKey = property.ContainingType + "::" +
						(property.ExplicitImplementation ?? "") + "::" +
						property.Type.Fullname + "->|" +
						string.Join("|",
							property.IndexerParameters.Value.Select(p => p.RefKind + " " + p.Type.Fullname));
					if (!signatureIndices.TryGetValue(signatureKey, out signatureIndex))
					{
						signatureIndex = nextSignatureIndex[0]++;
						signatureIndices[signatureKey] = signatureIndex;
					}
				}

				AppendMockSubject_ImplementClass_AddProperty(sb, property, mockRegistryName, className,
					mockClass is not null,
					@class.IsInterface, signatureIndex);
				sb.AppendLine();
			}
		}

		List<Method>? mockMethods = mockClass?.AllMethods().ToList();
		foreach (Method method in @class.AllMethods())
		{
			if (mockMethods?.All(m => !Method.EqualityComparer.Equals(method, m)) != false)
			{
				AppendMockSubject_ImplementClass_AddMethod(sb, method, mockRegistryName, className,
					mockClass is not null,
					@class.IsInterface, @class);
				sb.AppendLine();
			}
		}

		sb.Append("\t\t#endregion ").Append(@class.DisplayString).AppendLine();
	}

	private static void AppendMockSubject_ImplementClass_AddEvent(StringBuilder sb, Event @event,
		string mockRegistryName, string className,
		bool explicitInterfaceImplementation, bool isClassInterface)
	{
		string mockRegistry = @event.IsStatic ? "MockRegistryProvider.Value" : $"this.{mockRegistryName}";
		string backingFieldName = @event.GetBackingFieldName();
		string backingFieldAccess;
		if (@event.IsStatic)
		{
			sb.Append("\t\tprivate static readonly global::System.Threading.AsyncLocal<")
				.Append(@event.Type.Fullname.TrimEnd('?'))
				.Append("?> ").Append(backingFieldName).Append(" = new global::System.Threading.AsyncLocal<")
				.Append(@event.Type.Fullname.TrimEnd('?')).Append("?>();").AppendLine();
			backingFieldAccess = $"{backingFieldName}.Value";
		}
		else
		{
			sb.Append("\t\tprivate ").Append(@event.Type.Fullname.TrimEnd('?'))
				.Append("? ").Append(backingFieldName).Append(';').AppendLine();
			backingFieldAccess = $"this.{backingFieldName}";
		}

		sb.Append("\t\t/// <inheritdoc cref=\"").Append(@event.ContainingType.EscapeForXmlDoc()).Append('.')
			.Append(@event.Name).AppendLine("\" />");
		sb.Append(@event.Attributes, "\t\t");
		if (explicitInterfaceImplementation)
		{
			sb.Append(@event.IsStatic ? "\t\tstatic event " : "\t\tevent ").Append(@event.Type.Fullname).Append(' ')
				.Append(className).Append('.').Append(@event.Name).AppendLine();
		}
		else
		{
			if (@event.ExplicitImplementation is null)
			{
				sb.Append("\t\t").Append(@event.Accessibility.ToVisibilityString()).Append(' ');
				if (@event.IsStatic)
				{
					sb.Append("static ");
				}

				if (!isClassInterface && @event.UseOverride)
				{
					sb.Append("override ");
				}

				sb.Append("event ").Append(@event.Type.Fullname).Append(' ');
			}
			else
			{
				sb.Append(@event.IsStatic ? "\t\tstatic event " : "\t\tevent ").Append(@event.Type.Fullname).Append(' ')
					.Append(@event.ExplicitImplementation).Append('.');
			}

			sb.Append(@event.Name).AppendLine();
		}

		sb.AppendLine("\t\t{");
		bool supportsWrapping = @event is { IsStatic: false, IsProtected: false, ExplicitImplementation: null, } &&
		                        !explicitInterfaceImplementation;
		bool supportsBaseForwarding = supportsWrapping && !isClassInterface && @event.UseOverride && !@event.IsAbstract;
		if (supportsWrapping)
		{
			sb.Append("\t\t\tadd").AppendLine();
			sb.Append("\t\t\t{").AppendLine();
			sb.Append("\t\t\t\t").Append(mockRegistry).Append(".AddEvent(").Append(@event.GetUniqueNameString())
				.Append(", value?.Target, value?.Method);").AppendLine();
			sb.Append("\t\t\t\t").Append(backingFieldAccess).Append(" += value;").AppendLine();
			sb.Append("\t\t\t\tif (").Append(mockRegistry).Append(".Wraps is ").Append(className).Append(" wraps)")
				.AppendLine();
			sb.Append("\t\t\t\t{").AppendLine();
			sb.Append("\t\t\t\t\twraps.").Append(@event.Name).Append(" += value;").AppendLine();
			sb.Append("\t\t\t\t}").AppendLine();
			if (supportsBaseForwarding)
			{
				sb.Append("\t\t\t\tif (!").Append(mockRegistry).Append(".Behavior.SkipBaseClass)").AppendLine();
				sb.Append("\t\t\t\t{").AppendLine();
				sb.Append("\t\t\t\t\tbase.").Append(@event.Name).Append(" += value;").AppendLine();
				sb.Append("\t\t\t\t}").AppendLine();
			}

			sb.Append("\t\t\t}").AppendLine();
			sb.Append("\t\t\tremove").AppendLine();
			sb.Append("\t\t\t{").AppendLine();
			sb.Append("\t\t\t\t").Append(mockRegistry).Append(".RemoveEvent(").Append(@event.GetUniqueNameString())
				.Append(", value?.Target, value?.Method);").AppendLine();
			sb.Append("\t\t\t\t").Append(backingFieldAccess).Append(" -= value;").AppendLine();
			sb.Append("\t\t\t\tif (").Append(mockRegistry).Append(".Wraps is ").Append(className).Append(" wraps)")
				.AppendLine();
			sb.Append("\t\t\t\t{").AppendLine();
			sb.Append("\t\t\t\t\twraps.").Append(@event.Name).Append(" -= value;").AppendLine();
			sb.Append("\t\t\t\t}").AppendLine();
			if (supportsBaseForwarding)
			{
				sb.Append("\t\t\t\tif (!").Append(mockRegistry).Append(".Behavior.SkipBaseClass)").AppendLine();
				sb.Append("\t\t\t\t{").AppendLine();
				sb.Append("\t\t\t\t\tbase.").Append(@event.Name).Append(" -= value;").AppendLine();
				sb.Append("\t\t\t\t}").AppendLine();
			}

			sb.Append("\t\t\t}").AppendLine();
		}
		else
		{
			sb.Append("\t\t\tadd").AppendLine();
			sb.Append("\t\t\t{").AppendLine();
			sb.Append("\t\t\t\t").Append(mockRegistry).Append(".AddEvent(").Append(@event.GetUniqueNameString())
				.Append(", value?.Target, value?.Method);").AppendLine();
			sb.Append("\t\t\t\t").Append(backingFieldAccess).Append(" += value;").AppendLine();
			sb.Append("\t\t\t}").AppendLine();
			sb.Append("\t\t\tremove").AppendLine();
			sb.Append("\t\t\t{").AppendLine();
			sb.Append("\t\t\t\t").Append(mockRegistry).Append(".RemoveEvent(").Append(@event.GetUniqueNameString())
				.Append(", value?.Target, value?.Method);").AppendLine();
			sb.Append("\t\t\t\t").Append(backingFieldAccess).Append(" -= value;").AppendLine();
			sb.Append("\t\t\t}").AppendLine();
		}

		sb.AppendLine("\t\t}");
	}

	private static void AppendMockSubject_ImplementClass_AddProperty(StringBuilder sb, Property property,
		string mockRegistryName,
		string className, bool explicitInterfaceImplementation, bool isClassInterface, int signatureIndex)
	{
		string mockRegistry = property.IsStatic ? "MockRegistryProvider.Value" : $"this.{mockRegistryName}";
		sb.Append("\t\t/// <inheritdoc cref=\"").Append(property.ContainingType.EscapeForXmlDoc()).Append('.').Append(
				property.IndexerParameters is not null
					? property.Name.Replace("[]",
						$"[{string.Join(", ", property.IndexerParameters.Value.Select(p => $"{p.Type.Fullname.EscapeForXmlDoc()}"))}]")
					: property.Name)
			.AppendLine("\" />");
		sb.Append(property.Attributes, "\t\t");
		if (explicitInterfaceImplementation)
		{
			sb.Append(property.IsStatic ? "\t\tstatic " : "\t\t").Append(property.Type.Fullname)
				.Append(" ").Append(className).Append('.').Append(property.IndexerParameters is not null
					? property.Name.Replace("[]",
						$"[{string.Join(", ", property.IndexerParameters.Value.Select(p => $"{p.Type.Fullname} {p.Name}"))}]")
					: property.Name).AppendLine();
		}
		else
		{
			if (property.ExplicitImplementation is null)
			{
				sb.Append("\t\t").Append(property.Accessibility.ToVisibilityString()).Append(' ');
				if (property.IsStatic)
				{
					sb.Append("static ");
				}

				if (!isClassInterface && property.UseOverride)
				{
					sb.Append("override ");
				}

				sb.Append(property.Type.Fullname).Append(" ");
			}
			else
			{
				sb.Append("\t\t");

				if (property.IsStatic)
				{
					sb.Append("static ");
				}

				sb.Append(property.Type.Fullname).Append(" ").Append(property.ExplicitImplementation)
					.Append('.');
			}

			sb.Append(property.IndexerParameters is not null
				? property.Name.Replace("[]",
					$"[{string.Join(", ", property.IndexerParameters.Value.Select(p => $"{p.Type.Fullname} {p.Name}"))}]")
				: property.Name).AppendLine();
		}

		// Ref-struct-keyed indexer (getter AND setter): the full access pipeline requires the
		// key value to flow through IndexerGetterAccess / IndexerSetterAccess (classes with
		// field slots), which is illegal for a ref-struct key. Emit a runtime
		// NotSupportedException from both accessor bodies; the analyzer flags the declaration
		// at compile time. Full getter wiring to RefStructIndexerGetterSetup is future work.
		bool isRefStructIndexer = property is { IsIndexer: true, IndexerParameters: not null, } &&
		                          property.IndexerParameters.Value.Any(p => p.NeedsRefStructPipeline());

		sb.AppendLine("\t\t{");
		if (property.Getter != null)
		{
			sb.Append("\t\t\t");
			if (property.Getter.Accessibility != property.Accessibility)
			{
				sb.Append(property.Getter.Accessibility.ToVisibilityString()).Append(' ');
			}

			sb.AppendLine("get");
			sb.AppendLine("\t\t\t{");

			if (isRefStructIndexer)
			{
				AppendRefStructIndexerGetterBody(sb, property, mockRegistry);
			}
			else if (isClassInterface && !explicitInterfaceImplementation && property.ExplicitImplementation is null)
			{
				if (property is { IsIndexer: true, IndexerParameters: not null, })
				{
					string accessVarName =
						Helpers.GetUniqueLocalVariableName("access", property.IndexerParameters.Value);
					string setupVarName =
						Helpers.GetUniqueLocalVariableName("setup", property.IndexerParameters.Value);
					string baseResultVarName =
						Helpers.GetUniqueLocalVariableName("baseResult", property.IndexerParameters.Value);

					EmitIndexerGetterAccessAndSetup(sb, "\t\t\t\t", mockRegistry, accessVarName, setupVarName,
						property.Type, property.IndexerParameters.Value);
					sb.Append("\t\t\t\tif (").Append(mockRegistry).Append(".Wraps is not ").Append(className)
						.Append(" wraps)").AppendLine();
					sb.Append("\t\t\t\t{").AppendLine();
					sb.Append("\t\t\t\t\treturn ").Append(setupVarName).Append(" is null")
						.AppendLine();
					sb.Append("\t\t\t\t\t\t? ").Append(mockRegistry).Append(".GetIndexerFallback<")
						.AppendTypeOrWrapper(property.Type).Append(">(").Append(accessVarName).Append(", ")
						.Append(signatureIndex).Append(")")
						.AppendLine();
					sb.Append("\t\t\t\t\t\t: ").Append(mockRegistry).Append(".ApplyIndexerSetup<")
						.AppendTypeOrWrapper(property.Type).Append(">(").Append(accessVarName).Append(", ")
						.Append(setupVarName).Append(", ").Append(signatureIndex).Append(");").AppendLine();
					sb.Append("\t\t\t\t}").AppendLine();
					sb.Append("\t\t\t\t").AppendTypeOrWrapper(property.Type).Append(' ').Append(baseResultVarName)
						.Append(" = wraps[")
						.Append(FormatIndexerParametersAsNames(property.IndexerParameters.Value)).Append("];")
						.AppendLine();
					sb.Append("\t\t\t\treturn ").Append(mockRegistry).Append(".ApplyIndexerGetter(")
						.Append(accessVarName).Append(", ").Append(setupVarName).Append(", ")
						.Append(baseResultVarName).Append(", ").Append(signatureIndex).Append(");").AppendLine();
				}
				else
				{
					sb.Append("\t\t\t\treturn ").Append(mockRegistry).Append(".GetProperty<")
						.AppendTypeOrWrapper(property.Type).Append(">(")
						.Append(property.GetUniqueNameString()).Append(", () => ")
						.AppendDefaultValueGeneratorFor(property.Type, $"{mockRegistry}.Behavior.DefaultValue");
					if (!property.IsStatic)
					{
						sb.Append(", ").Append(mockRegistry).Append(".Wraps is not ").Append(className)
							.Append(" wraps ? null : () => wraps.").Append(property.Name);
					}
					else
					{
						sb.Append(", null");
					}

					sb.Append(");").AppendLine();
				}
			}
			else if (!isClassInterface && !property.IsAbstract)
			{
				if (property is { IsIndexer: true, IndexerParameters: not null, })
				{
					string accessVarName =
						Helpers.GetUniqueLocalVariableName("access", property.IndexerParameters.Value);
					string setupVarName =
						Helpers.GetUniqueLocalVariableName("setup", property.IndexerParameters.Value);
					string baseResultVarName =
						Helpers.GetUniqueLocalVariableName("baseResult", property.IndexerParameters.Value);

					EmitIndexerGetterAccessAndSetup(sb, "\t\t\t\t", mockRegistry, accessVarName, setupVarName,
						property.Type, property.IndexerParameters.Value);
					sb.Append("\t\t\t\tif (!(").Append(setupVarName).Append("?.SkipBaseClass() ?? ")
						.Append(mockRegistry).Append(".Behavior.SkipBaseClass))").AppendLine();
					sb.Append("\t\t\t\t{").AppendLine();
					if (property.Getter?.IsProtected != true)
					{
						sb.Append("\t\t\t\t\t").AppendTypeOrWrapper(property.Type).Append(' ')
							.Append(baseResultVarName).Append(" = this.")
							.Append(mockRegistryName)
							.Append(".Wraps is ").Append(className).Append(" wraps ? wraps[")
							.Append(FormatIndexerParametersAsNames(property.IndexerParameters.Value))
							.Append("] : base[")
							.Append(FormatIndexerParametersAsNames(property.IndexerParameters.Value)).Append("];")
							.AppendLine();
					}
					else
					{
						sb.Append("\t\t\t\t\t").AppendTypeOrWrapper(property.Type).Append(' ')
							.Append(baseResultVarName).Append(" = base[")
							.Append(FormatIndexerParametersAsNames(property.IndexerParameters.Value)).Append("];")
							.AppendLine();
					}

					sb.Append("\t\t\t\t\treturn ").Append(mockRegistry).Append(".ApplyIndexerGetter(")
						.Append(accessVarName).Append(", ").Append(setupVarName).Append(", ")
						.Append(baseResultVarName).Append(", ").Append(signatureIndex).Append(");").AppendLine();
					sb.Append("\t\t\t\t}").AppendLine();
					sb.Append("\t\t\t\treturn ").Append(setupVarName).Append(" is null").AppendLine();
					sb.Append("\t\t\t\t\t? ").Append(mockRegistry).Append(".GetIndexerFallback<")
						.AppendTypeOrWrapper(property.Type).Append(">(").Append(accessVarName).Append(", ")
						.Append(signatureIndex).Append(")")
						.AppendLine();
					sb.Append("\t\t\t\t\t: ").Append(mockRegistry).Append(".ApplyIndexerSetup<")
						.AppendTypeOrWrapper(property.Type).Append(">(").Append(accessVarName).Append(", ")
						.Append(setupVarName).Append(", ").Append(signatureIndex).Append(");").AppendLine();
				}
				else
				{
					sb.Append("\t\t\t\treturn ").Append(mockRegistry).Append(".GetProperty<")
						.AppendTypeOrWrapper(property.Type).Append(">(")
						.Append(property.GetUniqueNameString()).Append(", () => ")
						.AppendDefaultValueGeneratorFor(property.Type, $"{mockRegistry}.Behavior.DefaultValue");
					if (property is { IsStatic: false, } && property.Getter?.IsProtected != true)
					{
						sb.Append(", ").Append(mockRegistry).Append(".Wraps is ").Append(className)
							.Append(" wraps ? () => wraps.").Append(property.Name).Append(" : () => base.")
							.Append(property.Name);
					}
					else
					{
						sb.Append(", () => base.").Append(property.Name);
					}

					sb.Append(");").AppendLine();
				}
			}
			else if (property is { IsIndexer: true, IndexerParameters: not null, })
			{
				string accessVarName =
					Helpers.GetUniqueLocalVariableName("access", property.IndexerParameters.Value);
				string setupVarName =
					Helpers.GetUniqueLocalVariableName("setup", property.IndexerParameters.Value);

				EmitIndexerGetterAccessAndSetup(sb, "\t\t\t\t", mockRegistry, accessVarName, setupVarName,
					property.Type, property.IndexerParameters.Value);
				sb.Append("\t\t\t\treturn ").Append(setupVarName).Append(" is null").AppendLine();
				sb.Append("\t\t\t\t\t? ").Append(mockRegistry).Append(".GetIndexerFallback<")
					.AppendTypeOrWrapper(property.Type).Append(">(").Append(accessVarName).Append(", ")
					.Append(signatureIndex).Append(")")
					.AppendLine();
				sb.Append("\t\t\t\t\t: ").Append(mockRegistry).Append(".ApplyIndexerSetup<")
					.AppendTypeOrWrapper(property.Type).Append(">(").Append(accessVarName).Append(", ")
					.Append(setupVarName).Append(", ").Append(signatureIndex).Append(");").AppendLine();
			}
			else
			{
				sb.Append("\t\t\t\treturn ").Append(mockRegistry).Append(".GetProperty<")
					.AppendTypeOrWrapper(property.Type).Append(">(").Append(property.GetUniqueNameString())
					.Append(", () => ")
					.AppendDefaultValueGeneratorFor(property.Type, $"{mockRegistry}.Behavior.DefaultValue")
					.Append(", null);").AppendLine();
			}

			sb.AppendLine("\t\t\t}");
		}

		if (property.Setter != null)
		{
			sb.Append("\t\t\t");
			if (property.Setter.Accessibility != property.Accessibility)
			{
				sb.Append(property.Setter.Accessibility.ToVisibilityString()).Append(' ');
			}

			sb.AppendLine("set");
			sb.AppendLine("\t\t\t{");

			// Ref-struct-keyed indexer setter: dispatches through
			// RefStructIndexerSetterSetup<TValue, T1..Tn>. Mirrors the getter pipeline.
			if (isRefStructIndexer)
			{
				AppendRefStructIndexerSetterBody(sb, property, mockRegistry);
			}
			else if (isClassInterface && !explicitInterfaceImplementation && property.ExplicitImplementation is null)
			{
				if (property is { IsIndexer: true, IndexerParameters: not null, })
				{
					string accessVarName =
						Helpers.GetUniqueLocalVariableName("access", property.IndexerParameters.Value);
					string setupVarName =
						Helpers.GetUniqueLocalVariableName("setup", property.IndexerParameters.Value);

					EmitIndexerSetterAccessAndSetup(sb, "\t\t\t\t", mockRegistry, accessVarName, setupVarName,
						property.Type, property.IndexerParameters.Value);
					sb.Append("\t\t\t\t").Append(mockRegistry).Append(".ApplyIndexerSetter(")
						.Append(accessVarName).Append(", ").Append(setupVarName).Append(", value, ")
						.Append(signatureIndex).Append(");")
						.AppendLine();

					sb.Append("\t\t\t\tif (").Append(mockRegistry).Append(".Wraps is ").Append(className)
						.Append(" wraps)").AppendLine();
					sb.Append("\t\t\t\t{").AppendLine();
					sb.Append("\t\t\t\t\twraps[")
						.Append(FormatIndexerParametersAsNames(property.IndexerParameters.Value))
						.AppendLine("] = value;");
					sb.Append("\t\t\t\t}").AppendLine();
				}
				else
				{
					sb.Append("\t\t\t\t").Append(mockRegistry).Append(".SetProperty<")
						.AppendTypeOrWrapper(property.Type).Append(">(").Append(property.GetUniqueNameString())
						.Append(", value);").AppendLine();
					if (!property.IsStatic)
					{
						sb.Append("\t\t\t\tif (").Append(mockRegistry).Append(".Wraps is ").Append(className)
							.Append(" wraps)").AppendLine();
						sb.Append("\t\t\t\t{").AppendLine();
						sb.Append("\t\t\t\t\twraps.").Append(property.Name).Append(" = value;").AppendLine();
						sb.Append("\t\t\t\t}").AppendLine();
					}
				}
			}
			else if (property is { IsIndexer: true, IndexerParameters: not null, })
			{
				string accessVarName =
					Helpers.GetUniqueLocalVariableName("access", property.IndexerParameters.Value);
				string setupVarName =
					Helpers.GetUniqueLocalVariableName("setup", property.IndexerParameters.Value);

				if (!isClassInterface && !property.IsAbstract)
				{
					EmitIndexerSetterAccessAndSetup(sb, "\t\t\t\t", mockRegistry, accessVarName, setupVarName,
						property.Type, property.IndexerParameters.Value);
					sb.Append("\t\t\t\tif (!").Append(mockRegistry).Append(".ApplyIndexerSetter(")
						.Append(accessVarName).Append(", ").Append(setupVarName).Append(", value, ")
						.Append(signatureIndex).Append("))").AppendLine();
					sb.Append("\t\t\t\t{").AppendLine();
					if (property.Setter?.IsProtected != true)
					{
						sb.Append("\t\t\t\t\tif (this.").Append(mockRegistryName).Append(".Wraps is ").Append(className)
							.Append(" wraps)").AppendLine();
						sb.Append("\t\t\t\t\t{").AppendLine();
						sb.Append("\t\t\t\t\t\twraps[")
							.Append(FormatIndexerParametersAsNames(property.IndexerParameters.Value))
							.AppendLine("] = value;");
						sb.Append("\t\t\t\t\t}").AppendLine();
						sb.Append("\t\t\t\t\telse").AppendLine();
						sb.Append("\t\t\t\t\t{").AppendLine();
						sb.Append("\t\t\t\t\t\tbase[")
							.Append(FormatIndexerParametersAsNames(property.IndexerParameters.Value))
							.AppendLine("] = value;");
						sb.Append("\t\t\t\t\t}").AppendLine();
					}
					else
					{
						sb.Append("\t\t\t\t\tbase[")
							.Append(FormatIndexerParametersAsNames(property.IndexerParameters.Value))
							.AppendLine("] = value;");
					}

					sb.Append("\t\t\t\t}").AppendLine();
				}
				else
				{
					EmitIndexerSetterAccessAndSetup(sb, "\t\t\t\t", mockRegistry, accessVarName, setupVarName,
						property.Type, property.IndexerParameters.Value);
					sb.Append("\t\t\t\t").Append(mockRegistry).Append(".ApplyIndexerSetter(")
						.Append(accessVarName).Append(", ").Append(setupVarName).Append(", value, ")
						.Append(signatureIndex).Append(");").AppendLine();
				}
			}
			else
			{
				if (!isClassInterface && !property.IsAbstract)
				{
					sb.Append("\t\t\t\tif (!").Append(mockRegistry).Append(".SetProperty<")
						.AppendTypeOrWrapper(property.Type).Append(">(").Append(property.GetUniqueNameString())
						.Append(", value))").AppendLine();
					sb.Append("\t\t\t\t{").AppendLine();
					if (property is { IsStatic: false, } && property.Setter?.IsProtected != true)
					{
						sb.Append("\t\t\t\t\tif (").Append(mockRegistry).Append(".Wraps is ").Append(className)
							.Append(" wraps)").AppendLine();
						sb.Append("\t\t\t\t\t{").AppendLine();
						sb.Append("\t\t\t\t\t\twraps.").Append(property.Name).Append(" = value;").AppendLine();
						sb.Append("\t\t\t\t\t}").AppendLine();
						sb.Append("\t\t\t\t\telse").AppendLine();
						sb.Append("\t\t\t\t\t{").AppendLine();
						sb.Append("\t\t\t\t\t\tbase.").Append(property.Name).Append(" = value;").AppendLine();
						sb.Append("\t\t\t\t\t}").AppendLine();
					}
					else
					{
						sb.Append("\t\t\t\t\tbase.").Append(property.Name).Append(" = value;").AppendLine();
					}

					sb.Append("\t\t\t\t}").AppendLine();
				}
				else
				{
					sb.Append("\t\t\t\t").Append(mockRegistry).Append(".SetProperty<")
						.AppendTypeOrWrapper(property.Type).Append(">(").Append(property.GetUniqueNameString())
						.AppendLine(", value);");
				}
			}

			sb.AppendLine("\t\t\t}");
		}

		sb.AppendLine("\t\t}");
	}

	private static void AppendMockSubject_ImplementClass_AddMethod(StringBuilder sb, Method method,
		string mockRegistryName, string className,
		bool explicitInterfaceImplementation, bool isClassInterface, Class @class)
	{
		string mockRegistry = method.IsStatic ? "MockRegistryProvider.Value" : $"this.{mockRegistryName}";
		sb.Append("\t\t/// <inheritdoc cref=\"").Append(method.ContainingType.EscapeForXmlDoc()).Append('.')
			.Append(method.Name.EscapeForXmlDoc()).Append('(')
			.Append(string.Join(", ", method.Parameters.Select(p => p.RefKind.GetString() + p.Type.Fullname))
				.EscapeForXmlDoc()).AppendLine(")\" />");
		sb.Append(method.Attributes, "\t\t");
		if (explicitInterfaceImplementation)
		{
			sb.Append("\t\t");
			if (method.IsStatic)
			{
				sb.Append("static ");
			}

			sb.Append(method.ReturnType.Fullname).Append(' ').Append(className).Append('.').Append(method.Name)
				.Append('(');
		}
		else
		{
			sb.Append("\t\t");
			if (method.ExplicitImplementation is null)
			{
				sb.Append(method.Accessibility.ToVisibilityString()).Append(' ');
				if (method.IsStatic)
				{
					sb.Append("static ");
				}

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
				if (method.IsStatic)
				{
					sb.Append("static ");
				}

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
			if (parameter.IsParams)
			{
				sb.Append("params ");
			}

			sb.Append(parameter.Type.Fullname).Append(' ').Append(parameter.Name);
			if (!explicitInterfaceImplementation && parameter.HasExplicitDefaultValue)
			{
				sb.Append(" = ").Append(parameter.ExplicitDefaultValue);
			}
		}

		sb.Append(')');
		if (method.GenericParameters is not null && method.GenericParameters.Value.Count > 0)
		{
			bool isOverride = !isClassInterface && method.UseOverride;
			bool isExplicitImplementation = explicitInterfaceImplementation || method.ExplicitImplementation is not null;
			bool inheritsConstraints = isExplicitImplementation || isOverride || method.IsEquals() ||
			                           method.IsGetHashCode() || method.IsToString();
			foreach (GenericParameter gp in method.GenericParameters.Value)
			{
				gp.AppendWhereConstraint(sb, "\t\t\t", inheritsConstraints, isExplicitImplementation);
			}
		}

		sb.AppendLine();
		sb.AppendLine("\t\t{");

		// Methods with at least one ref-struct parameter (outside the Span/ReadOnlySpan wrapper
		// carve-out) route through the ref-struct setup pipeline. The ref-struct value cannot
		// be captured in a closure, so we emit a synchronous, stack-bound match/invoke loop.
		if (method.Parameters.Any(p => p.NeedsRefStructPipeline()))
		{
			AppendMockSubject_ImplementClass_AddRefStructMethodBody(sb, method, mockRegistry);
			sb.AppendLine("\t\t}");
			return;
		}

		string methodSetup = Helpers.GetUniqueLocalVariableName("methodSetup", method.Parameters);
		string methodSetupType = (method.ReturnType == Type.Void, method.Parameters.Count) switch
		{
			(true, 0) => "global::Mockolate.Setup.VoidMethodSetup",
			(true, _) => $"global::Mockolate.Setup.VoidMethodSetup<{string.Join(", ", method.Parameters.Select(p => p.ToTypeOrWrapper()))}>",
			(_, 0) => $"global::Mockolate.Setup.ReturnMethodSetup<{method.ReturnType.ToTypeOrWrapper()}>",
			(_, _) => $"global::Mockolate.Setup.ReturnMethodSetup<{method.ReturnType.ToTypeOrWrapper()}, {string.Join(", ", method.Parameters.Select(p => p.ToTypeOrWrapper()))}>",
		};
		bool hasOutParams = method.Parameters.Any(p => p.RefKind is RefKind.Out);
		bool hasRefParams = method.Parameters.Any(p => p.RefKind is RefKind.Ref);
		string hasWrappedResult = Helpers.GetUniqueLocalVariableName("hasWrappedResult", method.Parameters);
		string wrappedResult = Helpers.GetUniqueLocalVariableName("wrappedResult", method.Parameters);
		string wpc = Helpers.GetUniqueLocalVariableName("wpc", method.Parameters);
		bool supportsWrapping = !explicitInterfaceImplementation && method is { IsStatic: false, IsProtected: false, };
		bool isAbstractOrInterface = isClassInterface || method.IsAbstract;

		StringBuilder sb2 = new();
		int i = 0;
		foreach (MethodParameter p in method.Parameters)
		{
			if (i++ > 0)
			{
				sb2.Append(", ");
			}

			if (p.RefKind == RefKind.Ref || p.RefKind == RefKind.In || p.RefKind == RefKind.RefReadOnlyParameter)
			{
				string paramRef = Helpers.GetUniqueLocalVariableName($"ref_{p.Name}", method.Parameters);

				sb.Append("\t\t\tvar ").Append(paramRef).Append(" = ").Append(p.Name).Append(';').AppendLine();
				sb2.Append(paramRef);
			}
			else if (p.Type.SpecialGenericType == SpecialGenericType.Span ||
			         p.Type.SpecialGenericType == SpecialGenericType.ReadOnlySpan)
			{
				string paramRef = Helpers.GetUniqueLocalVariableName($"ref_{p.Name}", method.Parameters);

				sb.Append("\t\t\tvar ").Append(paramRef).Append(" = ").Append(p.ToNameOrWrapper()).Append(';')
					.AppendLine();
				sb2.Append(paramRef);
			}
			else
			{
				sb2.Append(
					p.RefKind switch
					{
						RefKind.Out => "default",
						_ => p.ToNameOrWrapper(),
					});
			}
		}

		sb.Append("\t\t\tvar ").Append(methodSetup)
			.Append(" = ").Append(mockRegistry).Append(".GetMethodSetup<").Append(methodSetupType).Append(">(")
			.Append(method.GetUniqueNameString()).Append(", m => m.Matches(");
		sb.Append(sb2);
		sb.AppendLine("));");
		sb.Append("\t\t\tbool ").Append(hasWrappedResult).Append(" = false;").AppendLine();
		if (method.ReturnType != Type.Void)
		{
			sb.Append("\t\t\t").Append(method.ReturnType.Fullname).Append(" ").Append(wrappedResult)
				.Append(" = default!;")
				.AppendLine();
		}

		if (hasOutParams)
		{
			foreach (MethodParameter parameter in method.Parameters.Where(p => p.RefKind == RefKind.Out))
			{
				sb.Append("\t\t\t").Append(parameter.Name).Append(" = default!;").AppendLine();
			}
		}

		sb.Append("\t\t\tif (").Append(mockRegistry).Append(".Behavior.SkipInteractionRecording == false)").AppendLine();
		sb.Append("\t\t\t{").AppendLine();
		sb.Append("\t\t\t\t").Append(mockRegistry)
			.Append(".RegisterInteraction(new global::Mockolate.Interactions.MethodInvocation");
		if (method.Parameters.Count > 0)
		{
			sb.Append('<').Append(string.Join(", ", method.Parameters.Select(p => p.ToTypeOrWrapper()))).Append('>');
		}

		sb.Append("(").Append(method.GetUniqueNameString());
		if (method.Parameters.Count > 0)
		{
			sb.Append(", ").Append(string.Join(", ", method.Parameters.Select(p => $"\"{p.Name}\", {p.ToNameOrWrapper()}")));
		}

		sb.Append("));").AppendLine();
		sb.Append("\t\t\t}").AppendLine();

		sb.Append("\t\t\ttry").AppendLine();
		sb.Append("\t\t\t{").AppendLine();

		if (supportsWrapping)
		{
			sb.Append("\t\t\t\tif (").Append(mockRegistry).Append(".Wraps is ").Append(className)
				.Append(" wraps)").AppendLine();
			sb.Append("\t\t\t\t{").AppendLine();
			if (method.ReturnType != Type.Void)
			{
				sb.Append("\t\t\t\t\t").Append(wrappedResult).Append(" = wraps").Append(".")
					.Append(method.Name).Append('(')
					.Append(FormatMethodParametersWithRefKind(method.Parameters))
					.Append(");").AppendLine();
				sb.Append("\t\t\t\t\t").Append(hasWrappedResult).Append(" = true;").AppendLine();
			}
			else
			{
				sb.Append("\t\t\t\t\twraps").Append(".")
					.Append(method.Name).Append('(')
					.Append(FormatMethodParametersWithRefKind(method.Parameters))
					.Append(");").AppendLine();
				sb.Append("\t\t\t\t\t").Append(hasWrappedResult).Append(" = true;").AppendLine();
			}

			sb.Append("\t\t\t\t}").AppendLine();
		}

		if (!isAbstractOrInterface)
		{
			if (method.Name.StartsWith("Send", StringComparison.Ordinal) &&
			    @class is { ClassFullName: "global::System.Net.Http.HttpClient", })
			{
				sb.Append("\t\t\t\t#if NETFRAMEWORK").AppendLine();
				sb.Append(
						"\t\t\t\t// Persist the HttpContent, because it gets automatically disposed on .NET Framework")
					.AppendLine();
				sb.Append("\t\t\t\tif (request.Content != null)").AppendLine();
				sb.Append("\t\t\t\t{").AppendLine();
				sb.Append(
						"\t\t\t\t\tvar stream = request.Content.ReadAsStreamAsync().ConfigureAwait(false).GetAwaiter().GetResult();")
					.AppendLine();
				sb.Append("\t\t\t\t\tusing global::System.IO.MemoryStream ms = new();").AppendLine();
				sb.Append("\t\t\t\t\tstream.CopyTo(ms);").AppendLine();
				sb.Append("\t\t\t\t\tbyte[] bytes = ms.ToArray();").AppendLine();
				sb.Append("\t\t\t\t\tstream.Position = 0L;").AppendLine();
				sb.Append("\t\t\t\t\trequest.Properties.Add(\"Mockolate:HttpContent\", bytes);").AppendLine();
				sb.Append("\t\t\t\t}").AppendLine();
				sb.Append("\t\t\t\t#endif").AppendLine();
			}

			sb.Append("\t\t\t\tif (!(").Append(methodSetup).Append("?.SkipBaseClass(").Append(mockRegistry)
				.Append(".Behavior) ?? ").Append(mockRegistry).Append(".Behavior.SkipBaseClass)");
			if (supportsWrapping)
			{
				sb.Append(" && !").Append(hasWrappedResult);
			}

			sb.Append(')').AppendLine();
			sb.Append("\t\t\t\t{").AppendLine();
			sb.Append("\t\t\t\t\t");
			if (method.ReturnType != Type.Void)
			{
				sb.Append(wrappedResult).Append(" = ");
			}

			sb.Append("base.").Append(method.Name).Append('(')
				.Append(FormatMethodParametersWithRefKind(method.Parameters))
				.Append(");").AppendLine();
			sb.Append("\t\t\t\t\t").Append(hasWrappedResult).Append(" = true;").AppendLine();
			sb.Append("\t\t\t\t}").AppendLine();
		}

		if (hasOutParams || hasRefParams)
		{
			sb.Append("\t\t\t\tif (!").Append(hasWrappedResult).Append(" || ").Append(methodSetup).Append(" is ").Append(methodSetupType)
				.Append(".WithParameterCollection)")
				.AppendLine();
			sb.Append("\t\t\t\t{").AppendLine();
			sb.Append("\t\t\t\t\tif (").Append(methodSetup).Append(" is ").Append(methodSetupType)
				.Append(".WithParameterCollection ").Append(wpc).Append(')').AppendLine();
			sb.Append("\t\t\t\t\t{").AppendLine();
			int parameterIndex = 0;
			foreach (MethodParameter parameter in method.Parameters)
			{
				parameterIndex++;
				if (parameter.RefKind == RefKind.Out)
				{
					sb.Append("\t\t\t\t\t\tif (").Append(wpc).Append(".Parameter").Append(parameterIndex)
						.Append(" is not global::Mockolate.Parameters.IOutParameter<")
						.Append(parameter.Type.ToTypeOrWrapper()).Append("> outParam").Append(parameterIndex)
						.Append(" || !outParam").Append(parameterIndex).Append(".TryGetValue(out ")
						.Append(parameter.Name).Append("))").AppendLine();
					sb.Append("\t\t\t\t\t\t{").AppendLine();
					sb.Append("\t\t\t\t\t\t\t").Append(parameter.Name).Append(" = ")
						.AppendDefaultValueGeneratorFor(parameter.Type, $"{mockRegistry}.Behavior.DefaultValue")
						.Append(';').AppendLine();
					sb.Append("\t\t\t\t\t\t}").AppendLine();
				}
				else if (parameter.RefKind == RefKind.Ref)
				{
					sb.Append("\t\t\t\t\t\tif (").Append(wpc).Append(".Parameter").Append(parameterIndex)
						.Append(" is global::Mockolate.Parameters.IRefParameter<")
						.Append(parameter.Type.ToTypeOrWrapper()).Append("> refParam").Append(parameterIndex)
						.Append(")").AppendLine();
					sb.Append("\t\t\t\t\t\t{").AppendLine();
					sb.Append("\t\t\t\t\t\t\t").Append(parameter.Name).Append(" = refParam").Append(parameterIndex)
						.Append(".GetValue(").Append(parameter.Name).Append(");").AppendLine();
					sb.Append("\t\t\t\t\t\t}").AppendLine();
				}
			}

			sb.Append("\t\t\t\t\t}").AppendLine();
			sb.Append("\t\t\t\t\telse").AppendLine();
			sb.Append("\t\t\t\t\t{").AppendLine();
			foreach (MethodParameter parameter in method.Parameters.Where(p => p.RefKind == RefKind.Out))
			{
				sb.Append("\t\t\t\t\t\t").Append(parameter.Name).Append(" = ")
					.AppendDefaultValueGeneratorFor(parameter.Type, $"{mockRegistry}.Behavior.DefaultValue").Append(';')
					.AppendLine();
			}

			sb.Append("\t\t\t\t\t}").AppendLine();
			sb.Append("\t\t\t\t}").AppendLine();
		}

		sb.Append("\t\t\t}").AppendLine();
		sb.Append("\t\t\tfinally").AppendLine();
		sb.Append("\t\t\t{").AppendLine();
		AppendTriggerCallbacks(sb, "\t\t\t\t", methodSetup, method.Parameters);
		sb.Append("\t\t\t}").AppendLine();

		string displayMethodName = $"{method.ContainingType}.{method.Name}({string.Join(", ", method.Parameters.Select(p => p.Type.DisplayName))})";
		sb.Append("\t\t\tif (").Append(methodSetup).Append(" is null && !").Append(hasWrappedResult).Append(" && ").Append(mockRegistry).Append(".Behavior.ThrowWhenNotSetup)").AppendLine();
		sb.Append("\t\t\t{").AppendLine();
		sb.Append("\t\t\t\tthrow new global::Mockolate.Exceptions.MockNotSetupException(\"The method '").Append(displayMethodName).Append("' was invoked without prior setup.\");").AppendLine();
		sb.Append("\t\t\t}").AppendLine();

		if (method.ReturnType != Type.Void)
		{
			sb.Append("\t\t\tif (").Append(methodSetup).Append("?.HasReturnCallbacks != true && ").Append(hasWrappedResult).Append(")").AppendLine();
			sb.Append("\t\t\t{").AppendLine();
			sb.Append("\t\t\t\treturn ").Append(wrappedResult).Append(";").AppendLine();
			sb.Append("\t\t\t}").AppendLine();
			sb.Append("\t\t\treturn ").Append(methodSetup).Append("?.TryGetReturnValue(");
			string defaultValueGeneratorSuffix = "";
			if (method.Parameters.Count > 0)
			{
				sb.Append(string.Join(", ", method.Parameters.Select(p => p.ToNameOrWrapper()))).Append(", ");
				defaultValueGeneratorSuffix = string.Join(", ", method.Parameters.Select(p => p.ToNameOrWrapper()));
			}

			sb.Append("out var returnValue) == true ? returnValue : ")
				.AppendDefaultValueGeneratorFor(method.ReturnType, $"{mockRegistry}.Behavior.DefaultValue", defaultValueGeneratorSuffix)
				.Append(';').AppendLine();
		}

		sb.AppendLine("\t\t}");
	}

	/// <summary>
	///     Emits the body of a mock method whose signature contains at least one ref-struct
	///     parameter (outside the Span/ReadOnlySpan wrapper carve-out). Uses
	///     <c>RefStructMethodInvocation</c> for recording and
	///     <c>RefStruct{Void,Return}MethodSetup&lt;T1..Tn&gt;</c> for dispatch. All matching runs
	///     synchronously on the caller's stack — no closure capture.
	/// </summary>
	/// <remarks>
	///     <para>
	///         The full setup-builder pipeline (<c>Do(Action&lt;T&gt;)</c>, <c>Callbacks&lt;T&gt;</c>
	///         sequencing, <c>TransitionTo</c>, <c>When</c>/<c>Only</c>, etc.) is intentionally NOT
	///         supported here: all of it requires <c>T</c> to flow through delegate type parameters,
	///         which is illegal for a ref-struct <c>T</c>. Only the narrow <c>Throws</c> /
	///         <c>Returns(value)</c> / <c>DoesNotThrow</c> / <c>SkippingBaseClass</c> surface is
	///         available — see <c>IRefStructVoidMethodSetup</c> / <c>IRefStructReturnMethodSetup</c>.
	///     </para>
	///     <para>
	///         The emitted code is guarded by <c>#if NET9_0_OR_GREATER</c>. Older TFMs get a
	///         <c>#error</c> because the <c>IParameter&lt;T&gt;</c> anti-constraint is a C# 13
	///         feature and the ref-struct setup types only compile on net9.0+.
	///     </para>
	///     <para>
	///         Out of scope (throws <c>NotSupportedException</c> at mock-invocation time, but the
	///         mock class still compiles so the rest of the interface can be mocked):
	///         arity above 4; out/ref ref-struct parameters; ref-struct return types that aren't
	///         <c>Span&lt;T&gt;</c>/<c>ReadOnlySpan&lt;T&gt;</c>. Proper analyzer-level diagnostics
	///         for these live in <c>MockabilityAnalyzer</c>.
	///     </para>
	/// </remarks>
	private static void AppendMockSubject_ImplementClass_AddRefStructMethodBody(
		StringBuilder sb, Method method, string mockRegistry)
	{
		sb.Append("#if NET9_0_OR_GREATER").AppendLine();

		bool hasUnsupportedParameter =
			method.Parameters.Any(p =>
				(p.RefKind == RefKind.Out || p.RefKind == RefKind.Ref ||
				 p.RefKind == RefKind.RefReadOnlyParameter) && p.NeedsRefStructPipeline());
		bool returnsUnsupportedRefStruct = method.ReturnType.IsRefStruct &&
		                                   method.ReturnType.SpecialGenericType is not
			                                   (SpecialGenericType.Span or SpecialGenericType.ReadOnlySpan);

		if (hasUnsupportedParameter || returnsUnsupportedRefStruct)
		{
			string reason = returnsUnsupportedRefStruct
				? "methods returning a non-span ref struct are not supported"
				: "out/ref ref-struct parameters are not supported";
			sb.Append("\t\t\tthrow new global::System.NotSupportedException(\"Mockolate: ")
				.Append(reason).Append(". Method '").Append(method.ContainingType).Append('.')
				.Append(method.Name).Append("'.\");").AppendLine();
			sb.Append("#else").AppendLine();
			sb.Append(
					"#error Mockolate: methods with ref-struct parameters require .NET 9 or later (uses the 'allows ref struct' anti-constraint).")
				.AppendLine();
			sb.Append("\t\t\tthrow new global::System.NotSupportedException();").AppendLine();
			sb.Append("#endif").AppendLine();
			return;
		}

		string typeParams = string.Join(", ", method.Parameters.Select(p => p.Type.Fullname));
		string setupType = method.ReturnType == Type.Void
			? $"global::Mockolate.Setup.RefStructVoidMethodSetup<{typeParams}>"
			: $"global::Mockolate.Setup.RefStructReturnMethodSetup<{method.ReturnType.Fullname}, {typeParams}>";

		// Record the invocation — names only, no values. The ref-struct values stay on the stack.
		sb.Append("\t\t\t").Append(mockRegistry)
			.Append(".RegisterInteraction(new global::Mockolate.Interactions.RefStructMethodInvocation(")
			.Append(method.GetUniqueNameString());
		foreach (MethodParameter p in method.Parameters)
		{
			sb.Append(", \"").Append(p.Name).Append('"');
		}

		sb.Append("));").AppendLine();

		// Iterate setups in latest-registered-first order (scenario-scoped first, default-scope
		// after — GetMethodSetups<T> preserves that ordering). Stop on the first matcher that
		// accepts every positional argument. The matching runs synchronously on the stack so
		// ref-struct values are not captured in a closure.
		string paramNames = string.Join(", ", method.Parameters.Select(p => p.Name));

		sb.Append("\t\t\tbool __matched = false;").AppendLine();
		sb.Append("\t\t\tforeach (").Append(setupType).Append(" __setup in ").Append(mockRegistry)
			.Append(".GetMethodSetups<").Append(setupType).Append(">(").Append(method.GetUniqueNameString())
			.Append("))").AppendLine();
		sb.Append("\t\t\t{").AppendLine();
		sb.Append("\t\t\t\tif (!__setup.Matches(").Append(paramNames).Append("))").AppendLine();
		sb.Append("\t\t\t\t{").AppendLine();
		sb.Append("\t\t\t\t\tcontinue;").AppendLine();
		sb.Append("\t\t\t\t}").AppendLine();
		sb.AppendLine();
		sb.Append("\t\t\t\t__matched = true;").AppendLine();

		if (method.ReturnType == Type.Void)
		{
			sb.Append("\t\t\t\t__setup.Invoke(").Append(paramNames).Append(");").AppendLine();
			sb.Append("\t\t\t\treturn;").AppendLine();
		}
		else
		{
			// Return-side: the Invoke overload takes an optional defaultFactory for missing
			// return configuration. When the setup has a return configured it wins; otherwise
			// we still run Invoke for any Throws/DoesNotThrow side effect then break out of the
			// search (a matching-but-unconfigured setup shadows later setups).
			string comma = method.Parameters.Count > 0 ? ", " : "";
			sb.Append("\t\t\t\tif (__setup.HasReturnValue)").AppendLine();
			sb.Append("\t\t\t\t{").AppendLine();
			sb.Append("\t\t\t\t\treturn __setup.Invoke(").Append(paramNames).Append(comma)
				.Append("() => default!);").AppendLine();
			sb.Append("\t\t\t\t}").AppendLine();
			sb.AppendLine();
			sb.Append("\t\t\t\t__setup.Invoke(").Append(paramNames).Append(comma)
				.Append("() => default!);").AppendLine();
			sb.Append("\t\t\t\tbreak;").AppendLine();
		}

		sb.Append("\t\t\t}").AppendLine();

		// Not-matched path: respect Behavior.ThrowWhenNotSetup, else return the framework default.
		string displayMethodName =
			$"{method.ContainingType}.{method.Name}({string.Join(", ", method.Parameters.Select(p => p.Type.DisplayName))})";

		sb.Append("\t\t\tif (!__matched && ").Append(mockRegistry).Append(".Behavior.ThrowWhenNotSetup)")
			.AppendLine();
		sb.Append("\t\t\t{").AppendLine();
		sb.Append("\t\t\t\tthrow new global::Mockolate.Exceptions.MockNotSetupException(\"The method '")
			.Append(displayMethodName).Append("' was invoked without prior setup.\");").AppendLine();
		sb.Append("\t\t\t}").AppendLine();

		if (method.ReturnType != Type.Void)
		{
			// Either the setup matched but had no return value configured, or no setup matched and
			// the behavior doesn't throw. Either way, surface the framework default for this return
			// type — matches the non-ref-struct pipeline's behavior when there is no wrapped base
			// and no configured return.
			sb.Append("\t\t\treturn ")
				.AppendDefaultValueGeneratorFor(method.ReturnType, $"{mockRegistry}.Behavior.DefaultValue")
				.Append(';').AppendLine();
		}

		sb.Append("#else").AppendLine();
		sb.Append(
				"#error Mockolate: methods with ref-struct parameters require .NET 9 or later (uses the 'allows ref struct' anti-constraint).")
			.AppendLine();
		sb.Append("\t\t\tthrow new global::System.NotSupportedException();").AppendLine();
		sb.Append("#endif").AppendLine();
	}

	/// <summary>
	///     Emits the body of an indexer getter whose key list contains at least one ref-struct
	///     parameter (outside the Span/ReadOnlySpan wrapper carve-out). Mirrors
	///     <see cref="AppendMockSubject_ImplementClass_AddRefStructMethodBody" /> but uses
	///     <c>RefStructIndexerGetterSetup&lt;TValue, T1..Tn&gt;</c> and the CLR accessor name
	///     <c>get_Item</c>.
	/// </summary>
	/// <remarks>
	///     Semantically equivalent to a method <c>TValue get_Item(T1 k1, ..., Tn kn)</c> — the
	///     setup dispatch, match-then-invoke loop, and fallthrough-to-default behavior all follow
	///     the return-method pipeline. No value is captured on the recorded interaction (ref-struct
	///     keys stay on the stack); verify-side matching over keys is not available, same rationale
	///     as the method-side pipeline.
	/// </remarks>
	private static void AppendRefStructIndexerGetterBody(StringBuilder sb, Property property, string mockRegistry)
	{
		sb.Append("#if NET9_0_OR_GREATER").AppendLine();

		string typeParams = string.Join(", ",
			property.IndexerParameters!.Value.Select(p => p.Type.Fullname));
		string setupType =
			$"global::Mockolate.Setup.RefStructIndexerGetterSetup<{property.Type.Fullname}, {typeParams}>";
		string displayName =
			$"{property.ContainingType}.this[{string.Join(", ", property.IndexerParameters!.Value.Select(p => p.Type.DisplayName))}]";
		string indexerName = $"\"{property.ContainingType}.get_Item\"";

		// Record the invocation — names only, no values. The ref-struct keys stay on the stack.
		sb.Append("\t\t\t\t").Append(mockRegistry)
			.Append(".RegisterInteraction(new global::Mockolate.Interactions.RefStructMethodInvocation(")
			.Append(indexerName);
		foreach (MethodParameter p in property.IndexerParameters.Value)
		{
			sb.Append(", \"").Append(p.Name).Append('"');
		}

		sb.Append("));").AppendLine();

		// Iterate setups in latest-registered-first order; stop on the first matching setup.
		string paramNames = string.Join(", ", property.IndexerParameters.Value.Select(p => p.Name));
		sb.Append("\t\t\t\tforeach (").Append(setupType).Append(" __setup in ").Append(mockRegistry)
			.Append(".GetMethodSetups<").Append(setupType).Append(">(").Append(indexerName).Append("))")
			.AppendLine();
		sb.Append("\t\t\t\t{").AppendLine();
		sb.Append("\t\t\t\t\tif (!__setup.Matches(").Append(paramNames).Append("))").AppendLine();
		sb.Append("\t\t\t\t\t{").AppendLine();
		sb.Append("\t\t\t\t\t\tcontinue;").AppendLine();
		sb.Append("\t\t\t\t\t}").AppendLine();
		sb.AppendLine();

		// Build a comma-separated raw-key list for arity 2+ that pre-boxes non-ref-struct slots
		// (ref-struct slots receive null and use their projection). At arity 1 no raw keys are
		// passed — the arity-1 setup owns its own projection dispatch via the matcher.
		string rawKeysArgs = property.IndexerParameters!.Value.Count >= 2
			? ", " + string.Join(", ",
				property.IndexerParameters!.Value.Select(p =>
					p.NeedsRefStructPipeline() ? "null" : $"(object){p.Name}"))
			: string.Empty;

		sb.Append("\t\t\t\t\tif (__setup.HasReturnValue)").AppendLine();
		sb.Append("\t\t\t\t\t{").AppendLine();
		sb.Append("\t\t\t\t\t\treturn __setup.Invoke(").Append(paramNames).Append(rawKeysArgs)
			.Append(", () => default!);").AppendLine();
		sb.Append("\t\t\t\t\t}").AppendLine();
		sb.AppendLine();

		// Matching-but-unconfigured setup still invokes (for Throws side effects) and then shadows
		// later setups in the iteration.
		sb.Append("\t\t\t\t\t__setup.Invoke(").Append(paramNames).Append(rawKeysArgs)
			.Append(", () => default!);").AppendLine();
		sb.Append("\t\t\t\t\tbreak;").AppendLine();
		sb.Append("\t\t\t\t}").AppendLine();

		// Not-matched path: respect Behavior.ThrowWhenNotSetup, else the framework default.
		sb.Append("\t\t\t\tif (").Append(mockRegistry).Append(".Behavior.ThrowWhenNotSetup)").AppendLine();
		sb.Append("\t\t\t\t{").AppendLine();
		sb.Append(
				"\t\t\t\t\tthrow new global::Mockolate.Exceptions.MockNotSetupException(\"The indexer '")
			.Append(displayName).Append("' was invoked without prior setup.\");").AppendLine();
		sb.Append("\t\t\t\t}").AppendLine();

		sb.Append("\t\t\t\treturn ")
			.AppendDefaultValueGeneratorFor(property.Type, $"{mockRegistry}.Behavior.DefaultValue")
			.Append(';').AppendLine();

		sb.Append("#else").AppendLine();
		sb.Append(
				"#error Mockolate: indexer getters with ref-struct keys require .NET 9 or later (uses the 'allows ref struct' anti-constraint).")
			.AppendLine();
		sb.Append("\t\t\t\tthrow new global::System.NotSupportedException();").AppendLine();
		sb.Append("#endif").AppendLine();
	}

	/// <summary>
	///     Emits the body of an indexer setter whose key list contains at least one ref-struct
	///     parameter. Mirrors <see cref="AppendRefStructIndexerGetterBody" /> but uses
	///     <c>RefStructIndexerSetterSetup&lt;TValue, T1..Tn&gt;</c> and the CLR accessor name
	///     <c>set_Item</c>. Records the value parameter's name alongside the ref-struct keys on
	///     <c>RefStructMethodInvocation</c> but does not carry the value.
	/// </summary>
	private static void AppendRefStructIndexerSetterBody(StringBuilder sb, Property property, string mockRegistry)
	{
		sb.Append("#if NET9_0_OR_GREATER").AppendLine();

		string typeParams = string.Join(", ",
			property.IndexerParameters!.Value.Select(p => p.Type.Fullname));
		string setupType =
			$"global::Mockolate.Setup.RefStructIndexerSetterSetup<{property.Type.Fullname}, {typeParams}>";
		string indexerName = $"\"{property.ContainingType}.set_Item\"";

		// Record the invocation — names only, no ref-struct key values (stays on the stack). The
		// "value" parameter name is appended for parity with the getter's recorded parameter names.
		sb.Append("\t\t\t\t").Append(mockRegistry)
			.Append(".RegisterInteraction(new global::Mockolate.Interactions.RefStructMethodInvocation(")
			.Append(indexerName);
		foreach (MethodParameter p in property.IndexerParameters.Value)
		{
			sb.Append(", \"").Append(p.Name).Append('"');
		}

		sb.Append(", \"value\"));").AppendLine();

		// Iterate setter setups latest-first; first match wins. The setter's Invoke handles the
		// OnSet callback and any configured throw, plus projection-keyed storage forwarding to
		// the companion getter (when both accessors form a combined setup).
		string keyNames = string.Join(", ", property.IndexerParameters.Value.Select(p => p.Name));

		// Build a comma-separated raw-key list for arity 2+ that pre-boxes non-ref-struct slots.
		// At arity 1 no raw keys are passed — the arity-1 setup owns its own projection dispatch.
		string rawKeysArgs = property.IndexerParameters.Value.Count >= 2
			? ", " + string.Join(", ",
				property.IndexerParameters.Value.Select(p =>
					p.NeedsRefStructPipeline() ? "null" : $"(object){p.Name}"))
			: string.Empty;

		sb.Append("\t\t\t\tforeach (").Append(setupType).Append(" __setup in ").Append(mockRegistry)
			.Append(".GetMethodSetups<").Append(setupType).Append(">(").Append(indexerName).Append("))")
			.AppendLine();
		sb.Append("\t\t\t\t{").AppendLine();
		sb.Append("\t\t\t\t\tif (!__setup.Matches(").Append(keyNames).Append("))").AppendLine();
		sb.Append("\t\t\t\t\t{").AppendLine();
		sb.Append("\t\t\t\t\t\tcontinue;").AppendLine();
		sb.Append("\t\t\t\t\t}").AppendLine();
		sb.AppendLine();
		sb.Append("\t\t\t\t\t__setup.Invoke(").Append(keyNames).Append(", value").Append(rawKeysArgs).Append(");").AppendLine();
		sb.Append("\t\t\t\t\treturn;").AppendLine();
		sb.Append("\t\t\t\t}").AppendLine();

		sb.Append("#else").AppendLine();
		sb.Append(
				"#error Mockolate: indexer setters with ref-struct keys require .NET 9 or later (uses the 'allows ref struct' anti-constraint).")
			.AppendLine();
		sb.Append("\t\t\t\tthrow new global::System.NotSupportedException();").AppendLine();
		sb.Append("#endif").AppendLine();
	}

	#endregion Mock Helpers

	#region Setup Helpers

	/// <summary>
	///     Emits a private nested <c>CovariantParameterAdapter&lt;T&gt;</c> class, used so that covariant widening of
	///     <c>IParameter&lt;T&gt;</c> parameters can be dispatched at setup/verify time without an
	///     <c>IParameterMatch&lt;T&gt;</c> cast failure.
	/// </summary>
	private static void AppendNestedCovariantParameterAdapter(StringBuilder sb)
	{
		sb.AppendLine();
		sb.Append("\t[global::System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]").AppendLine();
		sb.Append("\tprivate sealed class CovariantParameterAdapter<T>(global::Mockolate.Parameters.IParameter inner) : global::Mockolate.Parameters.IParameterMatch<T>").AppendLine();
		sb.Append("\t{").AppendLine();
		sb.Append("\t\tpublic bool Matches(T value) => inner.Matches(value);").AppendLine();
		sb.Append("\t\tpublic void InvokeCallbacks(T value) => inner.InvokeCallbacks(value);").AppendLine();
		sb.Append("\t\tpublic override string? ToString() => inner.ToString();").AppendLine();
		sb.AppendLine();
		sb.Append("\t\tpublic static global::Mockolate.Parameters.IParameterMatch<T> Wrap(global::Mockolate.Parameters.IParameter<T> parameter)").AppendLine();
		sb.Append("\t\t\t=> parameter is global::Mockolate.Parameters.IParameterMatch<T> direct").AppendLine();
		sb.Append("\t\t\t\t? direct").AppendLine();
		sb.Append("\t\t\t\t: new CovariantParameterAdapter<T>(parameter);").AppendLine();
		sb.Append("\t}").AppendLine();
	}

	private static IEnumerable<bool[]> GenerateValueFlagCombinations(EquatableArray<MethodParameter> parameters)
	{
		int[] valueableIndices = parameters
			.Select((p, i) => (p, i))
			.Where(x => x.p.CanUseNullableParameterOverload())
			.Select(x => x.i)
			.ToArray();
		int valueableCount = valueableIndices.Length;
		int totalCombos = 1 << valueableCount;
		for (int combo = 1; combo < totalCombos; combo++)
		{
			bool[] flags = new bool[parameters.Count];
			for (int bit = 0; bit < valueableCount; bit++)
			{
				if ((combo & (1 << bit)) != 0)
				{
					flags[valueableIndices[bit]] = true;
				}
			}

			yield return flags;
		}
	}

	/// <summary>
	///     Computes which parameters should receive a <c>= null</c> default in the generated signature.
	///     Only non-value parameters in the uninterrupted trailing suffix of optional parameters get a default;
	///     a value parameter (explicit-value overload) breaks the suffix, preventing earlier parameters from
	///     becoming optional and causing a CS1737 error.
	/// </summary>
	private static bool[] ComputeTrailingDefaults(ReadOnlySpan<MethodParameter> parameters, bool[]? valueFlags)
	{
		bool[] result = new bool[parameters.Length];
		bool inTrailingSuffix = true;
		for (int k = parameters.Length - 1; k >= 0; k--)
		{
			bool isValueParam = valueFlags?[k] == true;
			if (!isValueParam && parameters[k].HasExplicitDefaultValue && inTrailingSuffix)
			{
				result[k] = true;
			}
			else
			{
				inTrailingSuffix = false;
			}
		}

		return result;
	}

	private static void DefineSetupInterface(StringBuilder sb, Class @class, MemberType memberType,
		bool hasOverloadResolutionPriority)
	{
		#region Properties

		Func<Property, bool> propertyPredicate = property
			=> property.ExplicitImplementation is null && property is { IsIndexer: false, } &&
			   property.MemberType == memberType;
		foreach (Property property in @class.AllProperties().Where(propertyPredicate))
		{
			sb.AppendXmlSummary(
				$"Setup for the {property.Type.Fullname.EscapeForXmlDoc()} property <see cref=\"{property.ContainingType.EscapeForXmlDoc()}.{property.Name}\" />.");
			sb.Append("\t\tglobal::Mockolate.Setup.PropertySetup<").Append(property.Type.Fullname).Append("> ")
				.Append(property.Name).Append(" { get; }").AppendLine();
			sb.AppendLine();
		}

		#endregion

		#region Events

		Func<Event, bool> eventPredicate = @event
			=> @event.ExplicitImplementation is null && @event.MemberType == memberType;
		foreach (Event @event in @class.AllEvents().Where(eventPredicate))
		{
			sb.AppendXmlSummary(
				$"Setup for the event <see cref=\"{@event.ContainingType.EscapeForXmlDoc()}.{@event.Name}\" />.");
			sb.Append("\t\tglobal::Mockolate.Setup.EventSetup ").Append(@event.Name).Append(" { get; }").AppendLine();
			sb.AppendLine();
		}

		#endregion

		#region Indexers

		Func<Property, bool> indexerPredicate =
			indexer => indexer.ExplicitImplementation is null && indexer is { IsIndexer: true, } &&
			           indexer.MemberType == memberType;
		foreach (Property indexer in @class.AllProperties().Where(indexerPredicate))
		{
			AppendIndexerSetupDefinition(sb, indexer, hasOverloadResolutionPriority: hasOverloadResolutionPriority);
			if (indexer.IndexerParameters!.Value.Count <= MaxExplicitParameters)
			{
				foreach (bool[] valueFlags in GenerateValueFlagCombinations(indexer.IndexerParameters.Value))
				{
					AppendIndexerSetupDefinition(sb, indexer, valueFlags, hasOverloadResolutionPriority);
				}
			}
			else
			{
				bool[] allValueFlags = indexer.IndexerParameters.Value.Select(p => p.CanUseNullableParameterOverload())
					.ToArray();
				if (allValueFlags.Any(f => f))
				{
					AppendIndexerSetupDefinition(sb, indexer, allValueFlags, hasOverloadResolutionPriority);
				}
			}
		}

		#endregion

		#region Methods

		bool MethodPredicate(Method method)
		{
			return method.ExplicitImplementation is null && method.MemberType == memberType;
		}

		List<IGrouping<string, Method>> methodGroups =
			@class.AllMethods().Where((Func<Method, bool>)MethodPredicate).GroupBy(m => m.Name).ToList();
		foreach (IGrouping<string, Method>? methodGroup in methodGroups)
		{
			if (methodGroup.Count() == 1)
			{
				Method? method = methodGroup.Single();
				if (method.Parameters.Count > 0)
				{
					AppendMethodSetupDefinition(sb, @class, method, true,
						hasOverloadResolutionPriority: hasOverloadResolutionPriority);
				}
			}

			foreach (Method? method in methodGroup)
			{
				if (method.Parameters.Count == 0)
				{
					AppendMethodSetupDefinition(sb, @class, method, false,
						hasOverloadResolutionPriority: hasOverloadResolutionPriority);
				}
				else
				{
					AppendMethodSetupDefinition(sb, @class, method, false,
						hasOverloadResolutionPriority: hasOverloadResolutionPriority);
					if (method.Parameters.Count <= MaxExplicitParameters)
					{
						foreach (bool[] valueFlags in GenerateValueFlagCombinations(method.Parameters))
						{
							AppendMethodSetupDefinition(sb, @class, method, false, valueFlags: valueFlags,
								hasOverloadResolutionPriority: hasOverloadResolutionPriority);
						}
					}
					else
					{
						bool[] allValueFlags = method.Parameters.Select(p => p.CanUseNullableParameterOverload())
							.ToArray();
						if (allValueFlags.Any(f => f))
						{
							AppendMethodSetupDefinition(sb, @class, method, false, valueFlags: allValueFlags,
								hasOverloadResolutionPriority: hasOverloadResolutionPriority);
						}
					}
				}
			}
		}

		#endregion
	}

	private static void AppendOverloadDifferentiatorRemark(StringBuilder sb,
		IReadOnlyList<string> parameterNames, bool useParameters, bool[]? valueFlags,
		bool isVerify = false)
	{
		if (parameterNames.Count == 0)
		{
			return;
		}

		string text;
		if (useParameters)
		{
			text = isVerify
				? "This overload matches invocations via a custom <see cref=\"global::Mockolate.Match\" /> predicate (for example <see cref=\"global::Mockolate.Match.AnyParameters()\" /> or <see cref=\"global::Mockolate.Match.Parameters(global::System.Func{object?[], bool}, string)\" />) rather than per-parameter matchers."
				: "This overload configures the setup via a custom <see cref=\"global::Mockolate.Match\" /> predicate (for example <see cref=\"global::Mockolate.Match.AnyParameters()\" /> or <see cref=\"global::Mockolate.Match.Parameters(global::System.Func{object?[], bool}, string)\" />) rather than per-parameter matchers.";
		}
		else if (valueFlags is null)
		{
			text = "This overload takes <see cref=\"global::Mockolate.It\" /> argument matchers (e.g. <c>It.IsAny&lt;T&gt;()</c>, <c>It.Is&lt;T&gt;(value)</c>) for every parameter.";
		}
		else if (valueFlags.All(x => x))
		{
			text = isVerify
				? "This overload accepts direct values for every parameter and returns a <see cref=\"global::Mockolate.Verify.VerificationResult{TVerify}.IgnoreParameters\" /> whose <see cref=\"global::Mockolate.Verify.VerificationResult{TVerify}.IgnoreParameters.AnyParameters()\" /> drops per-parameter matching entirely."
				: "This overload accepts direct values for every parameter; each is treated as <c>It.Is&lt;T&gt;(value)</c>.";
		}
		else
		{
			List<string> valueParams = [];
			List<string> matcherParams = [];
			for (int i = 0; i < parameterNames.Count && i < valueFlags.Length; i++)
			{
				if (valueFlags[i])
				{
					valueParams.Add($"<paramref name=\"{parameterNames[i]}\" />");
				}
				else
				{
					matcherParams.Add($"<paramref name=\"{parameterNames[i]}\" />");
				}
			}

			string directPart = string.Join(", ", valueParams);
			string matcherPart = string.Join(", ", matcherParams);
			text =
				$"This overload accepts a direct value for {directPart} (equivalent to <c>It.Is&lt;T&gt;(value)</c>) and an <see cref=\"global::Mockolate.It\" /> matcher for {matcherPart}.";
		}

		sb.AppendXmlRemarks(text);
	}

	private static void AppendMethodSetupDefinition(StringBuilder sb, Class @class, Method method,
		bool useParameters, string? methodNameOverride = null, bool[]? valueFlags = null,
		bool hasOverloadResolutionPriority = false)
	{
		// Ref-struct pipeline: emit only the narrow IRefStruct*Setup declaration. We skip the
		// value-flag overloads entirely because an explicit ref-struct value cannot be captured
		// via `It.Is<T>(value)` (the static value would need to live in a class field). We also
		// skip the useParameters=true variant (IParameters overload) — ref-struct values cannot
		// flow through the existing IParameters collection. Users pass matchers via
		// `It.IsAnyRefStruct<T>()` / `It.IsRefStruct<T>(predicate)` through the single surviving
		// overload.
		if (method.Parameters.Any(p => p.NeedsRefStructPipeline()))
		{
			if (useParameters || valueFlags is not null)
			{
				return;
			}

			AppendRefStructMethodSetupDefinition(sb, method, methodNameOverride);
			return;
		}

		string methodName = methodNameOverride ?? method.Name;
		sb.Append("\t\t/// <summary>").AppendLine();
		if (methodNameOverride is null)
		{
			sb.Append("\t\t///     Setup for the method <see cref=\"")
				.Append(method.ContainingType.EscapeForXmlDoc()).Append(".")
				.Append(method.Name.EscapeForXmlDoc()).Append("(")
				.Append(string.Join(", ",
					method.Parameters.Select(p => p.RefKind.GetString() + p.Type.Fullname.EscapeForXmlDoc())))
				.Append(")\"/>");
		}
		else
		{
			sb.Append("\t\t///     Setup for the delegate <see cref=\"")
				.Append(@class.ClassFullName.EscapeForXmlDoc())
				.Append("\"/>");
		}

		if (useParameters)
		{
			sb.Append(" with the given <paramref name=\"parameters\" />");
		}
		else if (method.Parameters.Any())
		{
			sb.Append(" with the given ")
				.Append(string.Join(", ", method.Parameters.Select(p => $"<paramref name=\"{p.Name}\"/>")));
		}

		sb.Append(".").AppendLine();
		sb.Append("\t\t/// </summary>").AppendLine();
		AppendOverloadDifferentiatorRemark(sb, method.Parameters.Select(p => p.Name).ToArray(), useParameters, valueFlags);
		if (method.ReturnType != Type.Void)
		{
			if (valueFlags?.All(x => x) == true)
			{
				sb.Append("\t\tglobal::Mockolate.Setup.IReturnMethodSetupParameterIgnorer");
			}
			else
			{
				if (hasOverloadResolutionPriority)
				{
					sb.Append("\t\t[global::System.Runtime.CompilerServices.OverloadResolutionPriority(")
						.Append(valueFlags?.Count(x => !x).ToString() ?? "int.MaxValue").Append(")]").AppendLine();
				}

				sb.Append(method.Parameters.Count > 0
					? "\t\tglobal::Mockolate.Setup.IReturnMethodSetupWithCallback"
					: "\t\tglobal::Mockolate.Setup.IReturnMethodSetup");
			}

			sb.Append('<').AppendTypeOrWrapper(method.ReturnType);
			foreach (MethodParameter parameter in method.Parameters)
			{
				sb.Append(", ").AppendTypeOrWrapper(parameter.Type);
			}

			sb.Append("> ");
			sb.Append(methodName).Append("(");
		}
		else
		{
			if (valueFlags?.All(x => x) == true)
			{
				sb.Append("\t\tglobal::Mockolate.Setup.IVoidMethodSetupParameterIgnorer");
			}
			else
			{
				if (hasOverloadResolutionPriority)
				{
					sb.Append("\t\t[global::System.Runtime.CompilerServices.OverloadResolutionPriority(")
						.Append(valueFlags?.Count(x => !x).ToString() ?? "int.MaxValue").Append(")]").AppendLine();
				}

				sb.Append(method.Parameters.Count > 0
					? "\t\tglobal::Mockolate.Setup.IVoidMethodSetupWithCallback"
					: "\t\tglobal::Mockolate.Setup.IVoidMethodSetup");
			}

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

					sb.AppendTypeOrWrapper(parameter.Type);
				}

				sb.Append('>');
			}

			sb.Append(' ').Append(methodName).Append("(");
		}

		if (useParameters)
		{
			sb.Append("global::Mockolate.Parameters.IParameters parameters)");
		}
		else
		{
			ReadOnlySpan<MethodParameter> paramSpan = method.Parameters.AsSpan();
			bool[] hasTrailingDefault = ComputeTrailingDefaults(paramSpan, valueFlags);

			int i = 0;
			foreach (MethodParameter parameter in method.Parameters)
			{
				if (i > 0)
				{
					sb.Append(", ");
				}

				bool isValueParam = valueFlags?[i] == true;
				if (isValueParam)
				{
					sb.Append(parameter.ToNullableType()).Append(' ').Append(parameter.Name);
				}
				else
				{
					sb.Append(parameter.ToParameter());
					if (parameter.CanUseNullableParameterOverload())
					{
						sb.Append('?');
					}

					sb.Append(' ').Append(parameter.Name);
					if (hasTrailingDefault[i])
					{
						sb.Append(" = null");
					}
				}

				i++;
			}

			sb.Append(")");
		}

		if (method.GenericParameters is not null && method.GenericParameters.Value.Count > 0)
		{
			foreach (GenericParameter gp in method.GenericParameters.Value)
			{
				gp.AppendWhereConstraint(sb, "\t\t\t");
			}
		}

		sb.Append(";").AppendLine();
		sb.AppendLine();
	}

	private static void ImplementSetupInterface(StringBuilder sb, Class @class, string mockRegistryName,
		string setupName, MemberType memberType, string? scopeExpression = null)
	{
		string scopePrefix = scopeExpression is null ? "" : scopeExpression + ", ";

		#region Properties

		Func<Property, bool> propertyPredicate = property
			=> property.ExplicitImplementation is null && property is { IsIndexer: false, } &&
			   property.MemberType == memberType;
		foreach (Property property in @class.AllProperties().Where(propertyPredicate))
		{
			sb.Append("\t\t/// <inheritdoc />").AppendLine();
			sb.Append(
					"\t\t[global::System.Diagnostics.DebuggerBrowsable(global::System.Diagnostics.DebuggerBrowsableState.Never)]")
				.AppendLine();
			sb.Append("\t\tglobal::Mockolate.Setup.PropertySetup<").Append(property.Type.Fullname)
				.Append("> global::Mockolate.Mock.").Append(setupName).Append('.').Append(property.Name).AppendLine();
			sb.Append("\t\t{").AppendLine();
			sb.Append("\t\t\tget").AppendLine();
			sb.Append("\t\t\t{").AppendLine();
			sb.Append("\t\t\t\tvar propertySetup = new global::Mockolate.Setup.PropertySetup<")
				.Append(property.Type.Fullname).Append(">(").Append(mockRegistryName).Append(", ")
				.Append(property.GetUniqueNameString()).Append(");")
				.AppendLine();
			sb.Append("\t\t\t\tthis.").Append(mockRegistryName).Append(".SetupProperty(").Append(scopePrefix).Append("propertySetup);").AppendLine();
			sb.Append("\t\t\t\treturn propertySetup;").AppendLine();
			sb.Append("\t\t\t}").AppendLine();
			sb.Append("\t\t}").AppendLine();
			sb.AppendLine();
		}

		#endregion

		#region Events

		Func<Event, bool> eventSetupPredicate = @event
			=> @event.ExplicitImplementation is null && @event.MemberType == memberType;
		foreach (Event @event in @class.AllEvents().Where(eventSetupPredicate))
		{
			sb.Append("\t\t/// <inheritdoc />").AppendLine();
			sb.Append(
					"\t\t[global::System.Diagnostics.DebuggerBrowsable(global::System.Diagnostics.DebuggerBrowsableState.Never)]")
				.AppendLine();
			sb.Append("\t\tglobal::Mockolate.Setup.EventSetup global::Mockolate.Mock.")
				.Append(setupName).Append('.').Append(@event.Name).AppendLine();
			sb.Append("\t\t{").AppendLine();
			sb.Append("\t\t\tget").AppendLine();
			sb.Append("\t\t\t{").AppendLine();
			sb.Append("\t\t\t\tglobal::Mockolate.Setup.EventSetup eventSetup = new global::Mockolate.Setup.EventSetup(")
				.Append(mockRegistryName).Append(", ").Append(@event.GetUniqueNameString()).Append(");").AppendLine();
			sb.Append("\t\t\t\tthis.").Append(mockRegistryName).Append(".SetupEvent(").Append(scopePrefix).Append("eventSetup);").AppendLine();
			sb.Append("\t\t\t\treturn eventSetup;").AppendLine();
			sb.Append("\t\t\t}").AppendLine();
			sb.Append("\t\t}").AppendLine();
			sb.AppendLine();
		}

		#endregion

		#region Indexers

		Func<Property, bool> indexerPredicate =
			indexer => indexer.ExplicitImplementation is null && indexer is { IsIndexer: true, } &&
			           indexer.MemberType == memberType;
		foreach (Property indexer in @class.AllProperties().Where(indexerPredicate))
		{
			AppendIndexerSetupImplementation(sb, indexer, mockRegistryName, setupName, scopeExpression: scopeExpression);
			if (indexer.IndexerParameters!.Value.Count <= MaxExplicitParameters)
			{
				foreach (bool[] valueFlags in GenerateValueFlagCombinations(indexer.IndexerParameters.Value))
				{
					AppendIndexerSetupImplementation(sb, indexer, mockRegistryName, setupName, valueFlags, scopeExpression);
				}
			}
			else
			{
				bool[] allValueFlags = indexer.IndexerParameters.Value.Select(p => p.CanUseNullableParameterOverload())
					.ToArray();
				if (allValueFlags.Any(f => f))
				{
					AppendIndexerSetupImplementation(sb, indexer, mockRegistryName, setupName, allValueFlags, scopeExpression);
				}
			}
		}

		#endregion

		#region Methods

		bool MethodPredicate(Method method)
		{
			return method.ExplicitImplementation is null && method.MemberType == memberType;
		}

		List<IGrouping<string, Method>> methodGroups =
			@class.AllMethods().Where((Func<Method, bool>)MethodPredicate).GroupBy(m => m.Name).ToList();
		foreach (IGrouping<string, Method>? methodGroup in methodGroups)
		{
			if (methodGroup.Count() == 1)
			{
				Method? method = methodGroup.Single();
				if (method.Parameters.Count > 0)
				{
					AppendMethodSetupImplementation(sb, method, mockRegistryName, setupName, true,
						scopeExpression: scopeExpression);
				}
			}

			foreach (Method? method in methodGroup)
			{
				if (method.Parameters.Count == 0)
				{
					AppendMethodSetupImplementation(sb, method, mockRegistryName, setupName, false,
						scopeExpression: scopeExpression);
				}
				else
				{
					AppendMethodSetupImplementation(sb, method, mockRegistryName, setupName, false,
						scopeExpression: scopeExpression);
					if (method.Parameters.Count <= MaxExplicitParameters)
					{
						foreach (bool[] valueFlags in GenerateValueFlagCombinations(method.Parameters))
						{
							AppendMethodSetupImplementation(sb, method, mockRegistryName, setupName, false,
								valueFlags: valueFlags, scopeExpression: scopeExpression);
						}
					}
					else
					{
						bool[] allValueFlags = method.Parameters.Select(p => p.CanUseNullableParameterOverload())
							.ToArray();
						if (allValueFlags.Any(f => f))
						{
							AppendMethodSetupImplementation(sb, method, mockRegistryName, setupName, false,
								valueFlags: allValueFlags, scopeExpression: scopeExpression);
						}
					}
				}
			}
		}

		#endregion
	}
#pragma warning disable S107 // Methods should not have too many parameters
	private static void AppendMethodSetupImplementation(StringBuilder sb, Method method, string mockRegistryName,
		string setupName,
		bool useParameters, string? methodNameOverride = null, bool[]? valueFlags = null,
		string? scopeExpression = null)
	{
		if (method.Parameters.Any(p => p.NeedsRefStructPipeline()))
		{
			// Emit exactly once: skip the useParameters=true variant (IParameters collection
			// overload is not offered for ref-struct methods) and every value-flag variant
			// (explicit-value overloads are incompatible with ref-struct T).
			if (useParameters || valueFlags is not null)
			{
				return;
			}

			AppendRefStructMethodSetupImplementation(sb, method, mockRegistryName, setupName, methodNameOverride,
				scopeExpression);
			return;
		}

		string methodName = methodNameOverride ?? method.Name;
		string scopePrefix = scopeExpression is null ? "" : scopeExpression + ", ";
		sb.Append("\t\t/// <inheritdoc />").AppendLine();
		if (method.ReturnType != Type.Void)
		{
			if (valueFlags?.All(x => x) == true)
			{
				sb.Append("\t\tglobal::Mockolate.Setup.IReturnMethodSetupParameterIgnorer");
			}
			else
			{
				sb.Append(method.Parameters.Count > 0
					? "\t\tglobal::Mockolate.Setup.IReturnMethodSetupWithCallback"
					: "\t\tglobal::Mockolate.Setup.IReturnMethodSetup");
			}

			sb.Append('<').AppendTypeOrWrapper(method.ReturnType);
			foreach (MethodParameter parameter in method.Parameters)
			{
				sb.Append(", ").AppendTypeOrWrapper(parameter.Type);
			}

			sb.Append("> global::Mockolate.Mock.").Append(setupName).Append('.');
			sb.Append(methodName).Append("(");
		}
		else
		{
			if (valueFlags?.All(x => x) == true)
			{
				sb.Append("\t\tglobal::Mockolate.Setup.IVoidMethodSetupParameterIgnorer");
			}
			else
			{
				sb.Append(method.Parameters.Count > 0
					? "\t\tglobal::Mockolate.Setup.IVoidMethodSetupWithCallback"
					: "\t\tglobal::Mockolate.Setup.IVoidMethodSetup");
			}

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

					sb.AppendTypeOrWrapper(parameter.Type);
				}

				sb.Append('>');
			}

			sb.Append(" global::Mockolate.Mock.").Append(setupName).Append('.').Append(methodName).Append("(");
		}

		if (useParameters)
		{
			sb.Append("global::Mockolate.Parameters.IParameters parameters)");
		}
		else
		{
			int i = 0;
			foreach (MethodParameter parameter in method.Parameters)
			{
				if (i > 0)
				{
					sb.Append(", ");
				}

				bool isValueParam = valueFlags?[i] == true;
				if (isValueParam)
				{
					sb.Append(parameter.ToNullableType()).Append(' ').Append(parameter.Name);
				}
				else
				{
					sb.Append(parameter.ToParameter());
					if (parameter.CanUseNullableParameterOverload())
					{
						sb.Append('?');
					}

					sb.Append(' ').Append(parameter.Name);
				}

				i++;
			}

			sb.Append(")");
		}

		if (method.GenericParameters is not null && method.GenericParameters.Value.Count > 0)
		{
			foreach (GenericParameter gp in method.GenericParameters.Value)
			{
				gp.AppendWhereConstraint(sb, "\t\t\t", true, true);
			}
		}

		sb.AppendLine();
		sb.AppendLine("\t\t{");
		if (method.ReturnType != Type.Void)
		{
			sb.Append("\t\t\tvar methodSetup = new global::Mockolate.Setup.ReturnMethodSetup<")
				.AppendTypeOrWrapper(method.ReturnType);

			foreach (MethodParameter parameter in method.Parameters)
			{
				sb.Append(", ").AppendTypeOrWrapper(parameter.Type);
			}

			sb.Append(">");
		}
		else
		{
			sb.Append("\t\t\tvar methodSetup = new global::Mockolate.Setup.VoidMethodSetup");

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

					sb.AppendTypeOrWrapper(parameter.Type);
				}

				sb.Append('>');
			}
		}

		if (useParameters)
		{
			sb.Append(".WithParameters(").Append(mockRegistryName).Append(", ").Append(method.GetUniqueNameString())
				.Append(", parameters");
			foreach (MethodParameter parameter in method.Parameters)
			{
				sb.Append(", \"").Append(parameter.Name).Append('"');
			}

			sb.Append(");").AppendLine();
			sb.Append("\t\t\tthis.").Append(mockRegistryName).Append(".SetupMethod(").Append(scopePrefix).Append("methodSetup);").AppendLine();
			sb.Append("\t\t\treturn methodSetup;").AppendLine();
		}
		else
		{
			sb.Append(".WithParameterCollection(").Append(mockRegistryName).Append(", ")
				.Append(method.GetUniqueNameString());
			int j = 0;
			foreach (MethodParameter parameter in method.Parameters)
			{
				sb.Append(", ");
				if (valueFlags?[j] == true)
				{
					AppendNamedValueParameter(sb, parameter);
				}
				else
				{
					AppendNamedParameter(sb, parameter);
				}

				j++;
			}

			sb.Append(");").AppendLine();
			sb.Append("\t\t\tthis.").Append(mockRegistryName).Append(".SetupMethod(").Append(scopePrefix).Append("methodSetup);").AppendLine();
			sb.Append("\t\t\treturn methodSetup;").AppendLine();
		}

		sb.AppendLine("\t\t}");
		sb.AppendLine();
	}
#pragma warning restore S107 // Methods should not have too many parameters

	/// <summary>
	///     Emits the setup-interface declaration for a method that contains at least one
	///     ref-struct parameter. Gated on <c>NET9_0_OR_GREATER</c> because the declaration
	///     references <c>IRefStructVoidMethodSetup</c> / <c>IRefStructReturnMethodSetup</c>
	///     which themselves only compile on net9.0+.
	/// </summary>
	private static void AppendRefStructMethodSetupDefinition(StringBuilder sb, Method method,
		string? methodNameOverride)
	{
		bool unsupported = method.Parameters.Any(p =>
			                   p.RefKind == RefKind.Out || p.RefKind == RefKind.Ref ||
			                   p.RefKind == RefKind.RefReadOnlyParameter) ||
		                   (method.ReturnType.IsRefStruct && method.ReturnType.SpecialGenericType is not
			                   (SpecialGenericType.Span or SpecialGenericType.ReadOnlySpan));
		if (unsupported)
		{
			// No setup surface — the mock method body throws NotSupportedException at runtime and
			// the analyzer flags the signature at build time. Skipping the declaration entirely
			// keeps the setup interface clean.
			return;
		}

		string methodName = methodNameOverride ?? method.Name;
		string typeParams = string.Join(", ", method.Parameters.Select(p => p.Type.Fullname));
		string iface = method.ReturnType == Type.Void
			? $"global::Mockolate.Setup.IRefStructVoidMethodSetup<{typeParams}>"
			: $"global::Mockolate.Setup.IRefStructReturnMethodSetup<{method.ReturnType.Fullname}, {typeParams}>";

		sb.Append("#if NET9_0_OR_GREATER").AppendLine();
		sb.AppendXmlSummary(
			$"Setup for the method <see cref=\"{method.ContainingType.EscapeForXmlDoc()}.{method.Name.EscapeForXmlDoc()}({string.Join(", ", method.Parameters.Select(p => p.Type.Fullname.EscapeForXmlDoc()))})\"/>" +
			" — ref-struct parameter pipeline (narrow setup surface).");
		sb.Append("\t\t").Append(iface).Append(' ').Append(methodName).Append("(");
		int i = 0;
		foreach (MethodParameter parameter in method.Parameters)
		{
			if (i++ > 0)
			{
				sb.Append(", ");
			}

			sb.Append("global::Mockolate.Parameters.IParameter<").Append(parameter.Type.Fullname)
				.Append(">? ").Append(parameter.Name);
		}

		sb.Append(");").AppendLine();
		sb.Append("#endif").AppendLine();
		sb.AppendLine();
	}

	/// <summary>
	///     Emits the setup-interface implementation for a method that contains at least one
	///     ref-struct parameter. Constructs the concrete
	///     <c>RefStruct{Void,Return}MethodSetup&lt;T1..Tn&gt;</c>, registers it via
	///     <c>SetupMethod</c>, and returns it as its narrow interface.
	/// </summary>
	private static void AppendRefStructMethodSetupImplementation(StringBuilder sb, Method method,
		string mockRegistryName, string setupName, string? methodNameOverride, string? scopeExpression)
	{
		bool unsupported = method.Parameters.Any(p =>
			                   p.RefKind == RefKind.Out || p.RefKind == RefKind.Ref ||
			                   p.RefKind == RefKind.RefReadOnlyParameter) ||
		                   (method.ReturnType.IsRefStruct && method.ReturnType.SpecialGenericType is not
			                   (SpecialGenericType.Span or SpecialGenericType.ReadOnlySpan));
		if (unsupported)
		{
			return;
		}

		string methodName = methodNameOverride ?? method.Name;
		string scopePrefix = scopeExpression is null ? "" : scopeExpression + ", ";
		string typeParams = string.Join(", ", method.Parameters.Select(p => p.Type.Fullname));
		string iface = method.ReturnType == Type.Void
			? $"global::Mockolate.Setup.IRefStructVoidMethodSetup<{typeParams}>"
			: $"global::Mockolate.Setup.IRefStructReturnMethodSetup<{method.ReturnType.Fullname}, {typeParams}>";
		string concrete = method.ReturnType == Type.Void
			? $"global::Mockolate.Setup.RefStructVoidMethodSetup<{typeParams}>"
			: $"global::Mockolate.Setup.RefStructReturnMethodSetup<{method.ReturnType.Fullname}, {typeParams}>";

		sb.Append("#if NET9_0_OR_GREATER").AppendLine();
		sb.Append("\t\t/// <inheritdoc />").AppendLine();
		sb.Append("\t\t").Append(iface).Append(" global::Mockolate.Mock.").Append(setupName).Append('.')
			.Append(methodName).Append("(");
		int i = 0;
		foreach (MethodParameter parameter in method.Parameters)
		{
			if (i++ > 0)
			{
				sb.Append(", ");
			}

			sb.Append("global::Mockolate.Parameters.IParameter<").Append(parameter.Type.Fullname)
				.Append(">? ").Append(parameter.Name);
		}

		sb.Append(")").AppendLine();
		sb.Append("\t\t{").AppendLine();
		// The concrete setup constructor takes (string name, IParameterMatch<T1>? matcher1, ...).
		// All framework-provided matchers (It.IsAnyRefStruct<T>, It.IsRefStruct<T>, It.IsAny<T>, ...)
		// implement both IParameter<T> and IParameterMatch<T>, so a direct cast succeeds.
		sb.Append("\t\t\tvar methodSetup = new ").Append(concrete).Append('(')
			.Append(method.GetUniqueNameString());
		foreach (MethodParameter parameter in method.Parameters)
		{
			sb.Append(", (global::Mockolate.Parameters.IParameterMatch<").Append(parameter.Type.Fullname)
				.Append(">?)").Append(parameter.Name);
		}

		sb.Append(");").AppendLine();
		sb.Append("\t\t\tthis.").Append(mockRegistryName).Append(".SetupMethod(").Append(scopePrefix)
			.Append("methodSetup);").AppendLine();
		sb.Append("\t\t\treturn methodSetup;").AppendLine();
		sb.Append("\t\t}").AppendLine();
		sb.Append("#endif").AppendLine();
		sb.AppendLine();
	}

	private static void AppendIndexerSetupDefinition(StringBuilder sb, Property indexer, bool[]? valueFlags = null,
		bool hasOverloadResolutionPriority = false)
	{
		// Ref-struct-keyed indexers go through the narrow RefStruct pipeline. Based on the
		// getter/setter combination we expose the appropriate facade type:
		//   - getter only  -> IRefStructIndexerGetterSetup<TValue, T1..Tn>
		//   - setter only  -> IRefStructIndexerSetterSetup<TValue, T1..Tn>
		//   - get + set    -> IRefStructIndexerSetup<TValue, T1..Tn> (combined)
		// Ref-struct setups take IParameter<T>? for every key (no by-value overloads), so only
		// the valueFlags==null caller emits; subsequent variant-flag calls are skipped.
		if (indexer.IndexerParameters!.Value.Any(p => p.NeedsRefStructPipeline()))
		{
			if (valueFlags is not null)
			{
				return;
			}

			if (indexer.Getter is not null && indexer.Setter is null)
			{
				AppendRefStructIndexerGetterSetupDefinition(sb, indexer);
			}
			else if (indexer.Setter is not null && indexer.Getter is null)
			{
				AppendRefStructIndexerSetterSetupDefinition(sb, indexer);
			}
			else if (indexer.Getter is not null && indexer.Setter is not null)
			{
				AppendRefStructIndexerSetupDefinition(sb, indexer);
			}

			return;
		}

		sb.AppendXmlSummary(
			$"Setup for the {indexer.Type.Fullname.EscapeForXmlDoc()} indexer <see cref=\"{indexer.ContainingType.EscapeForXmlDoc()}.this[{string.Join(", ", indexer.IndexerParameters!.Value.Select(p => p.RefKind.GetString() + p.Type.Fullname.EscapeForXmlDoc()))}]\" />");
		string[] indexerNames = Enumerable.Range(1, indexer.IndexerParameters!.Value.Count)
			.Select(i => $"parameter{i}").ToArray();
		AppendOverloadDifferentiatorRemark(sb, indexerNames, useParameters: false, valueFlags);
		if (hasOverloadResolutionPriority)
		{
			sb.Append("\t\t[global::System.Runtime.CompilerServices.OverloadResolutionPriority(")
				.Append(valueFlags?.Count(x => !x).ToString() ?? "int.MaxValue").Append(")]").AppendLine();
		}

		sb.Append("\t\tglobal::Mockolate.Setup.IndexerSetup<").AppendTypeOrWrapper(indexer.Type);
		foreach (MethodParameter parameter in indexer.IndexerParameters!)
		{
			sb.Append(", ").AppendTypeOrWrapper(parameter.Type);
		}

		sb.Append("> this[");
		int i = 0;
		foreach (MethodParameter parameter in indexer.IndexerParameters.Value)
		{
			if (i > 0)
			{
				sb.Append(", ");
			}

			bool isValueParam = valueFlags?[i] == true;
			if (isValueParam)
			{
				sb.Append(parameter.ToNullableType()).Append($" parameter{i + 1}");
			}
			else
			{
				sb.Append(parameter.ToParameter());
				if (parameter.CanUseNullableParameterOverload())
				{
					sb.Append('?');
				}

				sb.Append($" parameter{i + 1}");
			}

			i++;
		}

		sb.Append("] { get; }").AppendLine();
		sb.AppendLine();
	}

	private static void AppendIndexerSetupImplementation(StringBuilder sb, Property indexer, string mockRegistryName,
		string setupName, bool[]? valueFlags = null, string? scopeExpression = null)
	{
		// Mirror AppendIndexerSetupDefinition: dispatch to the appropriate ref-struct facade
		// implementation depending on whether the indexer has a getter, a setter, or both.
		if (indexer.IndexerParameters!.Value.Any(p => p.NeedsRefStructPipeline()))
		{
			if (valueFlags is not null)
			{
				return;
			}

			if (indexer.Getter is not null && indexer.Setter is null)
			{
				AppendRefStructIndexerGetterSetupImplementation(sb, indexer, mockRegistryName, setupName,
					scopeExpression);
			}
			else if (indexer.Setter is not null && indexer.Getter is null)
			{
				AppendRefStructIndexerSetterSetupImplementation(sb, indexer, mockRegistryName, setupName,
					scopeExpression);
			}
			else if (indexer.Getter is not null && indexer.Setter is not null)
			{
				AppendRefStructIndexerSetupImplementation(sb, indexer, mockRegistryName, setupName, scopeExpression);
			}

			return;
		}

		string scopePrefix = scopeExpression is null ? "" : scopeExpression + ", ";
		sb.Append("\t\t/// <inheritdoc />").AppendLine();
		sb.Append(
				"\t\t[global::System.Diagnostics.DebuggerBrowsable(global::System.Diagnostics.DebuggerBrowsableState.Never)]")
			.AppendLine();
		sb.Append("\t\tglobal::Mockolate.Setup.IndexerSetup<").AppendTypeOrWrapper(indexer.Type);
		foreach (MethodParameter parameter in indexer.IndexerParameters!)
		{
			sb.Append(", ").AppendTypeOrWrapper(parameter.Type);
		}

		sb.Append("> global::Mockolate.Mock.").Append(setupName).Append(".this[");
		int i = 0;
		foreach (MethodParameter parameter in indexer.IndexerParameters.Value)
		{
			if (i > 0)
			{
				sb.Append(", ");
			}

			bool isValueParam = valueFlags?[i] == true;
			if (isValueParam)
			{
				sb.Append(parameter.ToNullableType()).Append($" parameter{i + 1}");
			}
			else
			{
				sb.Append(parameter.ToParameter());
				if (parameter.CanUseNullableParameterOverload())
				{
					sb.Append('?');
				}

				sb.Append($" parameter{i + 1}");
			}

			i++;
		}

		sb.Append("]").AppendLine();
		sb.Append("\t\t{").AppendLine();
		sb.Append("\t\t\tget").AppendLine();
		sb.Append("\t\t\t{").AppendLine();
		sb.Append("\t\t\t\tvar indexerSetup = new global::Mockolate.Setup.IndexerSetup<")
			.AppendTypeOrWrapper(indexer.Type);
		foreach (MethodParameter parameter in indexer.IndexerParameters!)
		{
			sb.Append(", ").AppendTypeOrWrapper(parameter.Type);
		}

		sb.Append(">(").Append(mockRegistryName);
		int j = 0;
		foreach (MethodParameter parameter in indexer.IndexerParameters.Value)
		{
			sb.Append(", ");

			bool isValueParam = valueFlags?[j] == true;
			string paramRef = $"parameter{j + 1}";
			if (isValueParam)
			{
				AppendNamedValueParameter(sb, parameter, paramRef);
			}
			else
			{
				sb.Append("CovariantParameterAdapter<")
					.AppendTypeOrWrapper(parameter.Type)
					.Append(">.Wrap(")
					.Append(paramRef);
				if (parameter.CanUseNullableParameterOverload())
				{
					sb.Append($" ?? global::Mockolate.It.IsNull<{parameter.ToNullableType()}>(\"null\")");
				}

				sb.Append(")");
			}

			j++;
		}

		sb.Append(");").AppendLine();
		sb.Append("\t\t\t\tthis.").Append(mockRegistryName).Append(".SetupIndexer(").Append(scopePrefix).Append("indexerSetup);").AppendLine();
		sb.Append("\t\t\t\treturn indexerSetup;").AppendLine();
		sb.Append("\t\t\t}").AppendLine();
		sb.Append("\t\t}").AppendLine();
		sb.AppendLine();
	}

	/// <summary>
	///     Emits the setup-interface declaration for a ref-struct-keyed getter-only indexer.
	///     Shape: <c>IRefStructIndexerGetterSetup&lt;TValue, T1..Tn&gt; this[IParameter&lt;T1&gt;?, ...] { get; }</c>.
	/// </summary>
	private static void AppendRefStructIndexerGetterSetupDefinition(StringBuilder sb, Property indexer)
	{
		sb.Append("#if NET9_0_OR_GREATER").AppendLine();
		sb.AppendXmlSummary(
			$"Setup for the ref-struct-keyed getter-only indexer <see cref=\"{indexer.ContainingType.EscapeForXmlDoc()}.this[{string.Join(", ", indexer.IndexerParameters!.Value.Select(p => p.Type.Fullname.EscapeForXmlDoc()))}]\" />" +
			" — narrow setup surface (Returns / Throws / SkippingBaseClass).");
		sb.Append("\t\tglobal::Mockolate.Setup.IRefStructIndexerGetterSetup<")
			.Append(indexer.Type.Fullname);
		foreach (MethodParameter p in indexer.IndexerParameters!.Value)
		{
			sb.Append(", ").Append(p.Type.Fullname);
		}

		sb.Append("> this[");
		int i = 0;
		foreach (MethodParameter parameter in indexer.IndexerParameters.Value)
		{
			if (i++ > 0)
			{
				sb.Append(", ");
			}

			sb.Append("global::Mockolate.Parameters.IParameter<").Append(parameter.Type.Fullname)
				.Append(">? ").Append($"parameter{i}");
		}

		sb.Append("] { get; }").AppendLine();
		sb.Append("#endif").AppendLine();
		sb.AppendLine();
	}

	/// <summary>
	///     Emits the setup-interface implementation for a ref-struct-keyed getter-only indexer.
	///     Constructs the concrete <c>RefStructIndexerGetterSetup&lt;TValue, T1..Tn&gt;</c>,
	///     registers it via <c>SetupMethod</c>, and returns it as its narrow interface.
	/// </summary>
	private static void AppendRefStructIndexerGetterSetupImplementation(StringBuilder sb, Property indexer,
		string mockRegistryName, string setupName, string? scopeExpression)
	{
		string scopePrefix = scopeExpression is null ? "" : scopeExpression + ", ";
		string typeParams = string.Join(", ", indexer.IndexerParameters!.Value.Select(p => p.Type.Fullname));
		string iface =
			$"global::Mockolate.Setup.IRefStructIndexerGetterSetup<{indexer.Type.Fullname}, {typeParams}>";
		string concrete =
			$"global::Mockolate.Setup.RefStructIndexerGetterSetup<{indexer.Type.Fullname}, {typeParams}>";
		string indexerName = $"\"{indexer.ContainingType}.get_Item\"";

		sb.Append("#if NET9_0_OR_GREATER").AppendLine();
		sb.Append("\t\t/// <inheritdoc />").AppendLine();
		sb.Append("\t\t").Append(iface).Append(" global::Mockolate.Mock.").Append(setupName).Append(".this[");
		int i = 0;
		foreach (MethodParameter parameter in indexer.IndexerParameters.Value)
		{
			if (i++ > 0)
			{
				sb.Append(", ");
			}

			sb.Append("global::Mockolate.Parameters.IParameter<").Append(parameter.Type.Fullname)
				.Append(">? parameter").Append(i);
		}

		sb.Append(']').AppendLine();
		sb.Append("\t\t{").AppendLine();
		sb.Append("\t\t\tget").AppendLine();
		sb.Append("\t\t\t{").AppendLine();
		sb.Append("\t\t\t\tvar indexerSetup = new ").Append(concrete).Append('(').Append(indexerName);
		int k = 0;
		foreach (MethodParameter keyParam in indexer.IndexerParameters.Value)
		{
			k++;
			sb.Append(", (global::Mockolate.Parameters.IParameterMatch<")
				.Append(keyParam.Type.Fullname).Append(">?)parameter").Append(k);
		}

		sb.Append(");").AppendLine();
		// Re-use the generic SetupMethod slot. The setup's MatchesInteraction filter on its name
		// ensures it only participates in get_Item lookups.
		sb.Append("\t\t\t\tthis.").Append(mockRegistryName).Append(".SetupMethod(").Append(scopePrefix)
			.Append("indexerSetup);").AppendLine();
		sb.Append("\t\t\t\treturn indexerSetup;").AppendLine();
		sb.Append("\t\t\t}").AppendLine();
		sb.Append("\t\t}").AppendLine();
		sb.Append("#endif").AppendLine();
		sb.AppendLine();
	}

	/// <summary>
	///     Emits the setup-interface declaration for a ref-struct-keyed setter-only indexer.
	///     Shape: <c>IRefStructIndexerSetterSetup&lt;TValue, T1..Tn&gt; this[IParameter&lt;T1&gt;?, ...] { get; }</c>.
	/// </summary>
	private static void AppendRefStructIndexerSetterSetupDefinition(StringBuilder sb, Property indexer)
	{
		sb.Append("#if NET9_0_OR_GREATER").AppendLine();
		sb.AppendXmlSummary(
			$"Setup for the ref-struct-keyed setter-only indexer <see cref=\"{indexer.ContainingType.EscapeForXmlDoc()}.this[{string.Join(", ", indexer.IndexerParameters!.Value.Select(p => p.Type.Fullname.EscapeForXmlDoc()))}]\" />" +
			" — narrow setter surface (OnSet / Throws / SkippingBaseClass).");
		sb.Append("\t\tglobal::Mockolate.Setup.IRefStructIndexerSetterSetup<")
			.Append(indexer.Type.Fullname);
		foreach (MethodParameter p in indexer.IndexerParameters!.Value)
		{
			sb.Append(", ").Append(p.Type.Fullname);
		}

		sb.Append("> this[");
		int i = 0;
		foreach (MethodParameter parameter in indexer.IndexerParameters.Value)
		{
			if (i++ > 0)
			{
				sb.Append(", ");
			}

			sb.Append("global::Mockolate.Parameters.IParameter<").Append(parameter.Type.Fullname)
				.Append(">? parameter").Append(i);
		}

		sb.Append("] { get; }").AppendLine();
		sb.Append("#endif").AppendLine();
		sb.AppendLine();
	}

	private static void AppendRefStructIndexerSetterSetupImplementation(StringBuilder sb, Property indexer,
		string mockRegistryName, string setupName, string? scopeExpression)
	{
		string scopePrefix = scopeExpression is null ? "" : scopeExpression + ", ";
		string typeParams = string.Join(", ", indexer.IndexerParameters!.Value.Select(p => p.Type.Fullname));
		string iface =
			$"global::Mockolate.Setup.IRefStructIndexerSetterSetup<{indexer.Type.Fullname}, {typeParams}>";
		string concrete =
			$"global::Mockolate.Setup.RefStructIndexerSetterSetup<{indexer.Type.Fullname}, {typeParams}>";
		string indexerName = $"\"{indexer.ContainingType}.set_Item\"";

		sb.Append("#if NET9_0_OR_GREATER").AppendLine();
		sb.Append("\t\t/// <inheritdoc />").AppendLine();
		sb.Append("\t\t").Append(iface).Append(" global::Mockolate.Mock.").Append(setupName).Append(".this[");
		int i = 0;
		foreach (MethodParameter parameter in indexer.IndexerParameters.Value)
		{
			if (i++ > 0)
			{
				sb.Append(", ");
			}

			sb.Append("global::Mockolate.Parameters.IParameter<").Append(parameter.Type.Fullname)
				.Append(">? parameter").Append(i);
		}

		sb.Append(']').AppendLine();
		sb.Append("\t\t{").AppendLine();
		sb.Append("\t\t\tget").AppendLine();
		sb.Append("\t\t\t{").AppendLine();
		sb.Append("\t\t\t\tvar indexerSetup = new ").Append(concrete).Append('(').Append(indexerName);
		int k = 0;
		foreach (MethodParameter keyParam in indexer.IndexerParameters.Value)
		{
			k++;
			sb.Append(", (global::Mockolate.Parameters.IParameterMatch<")
				.Append(keyParam.Type.Fullname).Append(">?)parameter").Append(k);
		}

		sb.Append(");").AppendLine();
		sb.Append("\t\t\t\tthis.").Append(mockRegistryName).Append(".SetupMethod(").Append(scopePrefix)
			.Append("indexerSetup);").AppendLine();
		sb.Append("\t\t\t\treturn indexerSetup;").AppendLine();
		sb.Append("\t\t\t}").AppendLine();
		sb.Append("\t\t}").AppendLine();
		sb.Append("#endif").AppendLine();
		sb.AppendLine();
	}

	/// <summary>
	///     Emits the setup-interface declaration for a ref-struct-keyed get+set indexer.
	///     Shape: <c>IRefStructIndexerSetup&lt;TValue, T1..Tn&gt; this[IParameter&lt;T1&gt;?, ...] { get; }</c>.
	/// </summary>
	private static void AppendRefStructIndexerSetupDefinition(StringBuilder sb, Property indexer)
	{
		sb.Append("#if NET9_0_OR_GREATER").AppendLine();
		sb.AppendXmlSummary(
			$"Setup for the ref-struct-keyed get+set indexer <see cref=\"{indexer.ContainingType.EscapeForXmlDoc()}.this[{string.Join(", ", indexer.IndexerParameters!.Value.Select(p => p.Type.Fullname.EscapeForXmlDoc()))}]\" />" +
			" — combined getter/setter facade.");
		sb.Append("\t\tglobal::Mockolate.Setup.IRefStructIndexerSetup<")
			.Append(indexer.Type.Fullname);
		foreach (MethodParameter p in indexer.IndexerParameters!.Value)
		{
			sb.Append(", ").Append(p.Type.Fullname);
		}

		sb.Append("> this[");
		int i = 0;
		foreach (MethodParameter parameter in indexer.IndexerParameters.Value)
		{
			if (i++ > 0)
			{
				sb.Append(", ");
			}

			sb.Append("global::Mockolate.Parameters.IParameter<").Append(parameter.Type.Fullname)
				.Append(">? parameter").Append(i);
		}

		sb.Append("] { get; }").AppendLine();
		sb.Append("#endif").AppendLine();
		sb.AppendLine();
	}

	private static void AppendRefStructIndexerSetupImplementation(StringBuilder sb, Property indexer,
		string mockRegistryName, string setupName, string? scopeExpression)
	{
		string scopePrefix = scopeExpression is null ? "" : scopeExpression + ", ";
		string typeParams = string.Join(", ", indexer.IndexerParameters!.Value.Select(p => p.Type.Fullname));
		string iface =
			$"global::Mockolate.Setup.IRefStructIndexerSetup<{indexer.Type.Fullname}, {typeParams}>";
		string concrete =
			$"global::Mockolate.Setup.RefStructIndexerSetup<{indexer.Type.Fullname}, {typeParams}>";
		string getterName = $"\"{indexer.ContainingType}.get_Item\"";
		string setterName = $"\"{indexer.ContainingType}.set_Item\"";

		sb.Append("#if NET9_0_OR_GREATER").AppendLine();
		sb.Append("\t\t/// <inheritdoc />").AppendLine();
		sb.Append("\t\t").Append(iface).Append(" global::Mockolate.Mock.").Append(setupName).Append(".this[");
		int i = 0;
		foreach (MethodParameter parameter in indexer.IndexerParameters.Value)
		{
			if (i++ > 0)
			{
				sb.Append(", ");
			}

			sb.Append("global::Mockolate.Parameters.IParameter<").Append(parameter.Type.Fullname)
				.Append(">? parameter").Append(i);
		}

		sb.Append(']').AppendLine();
		sb.Append("\t\t{").AppendLine();
		sb.Append("\t\t\tget").AppendLine();
		sb.Append("\t\t\t{").AppendLine();
		sb.Append("\t\t\t\tvar indexerSetup = new ").Append(concrete).Append('(').Append(getterName)
			.Append(", ").Append(setterName);
		int k = 0;
		foreach (MethodParameter keyParam in indexer.IndexerParameters.Value)
		{
			k++;
			sb.Append(", (global::Mockolate.Parameters.IParameterMatch<")
				.Append(keyParam.Type.Fullname).Append(">?)parameter").Append(k);
		}

		sb.Append(");").AppendLine();
		// Register both the inner getter and setter. Each has its own MatchesInteraction name
		// filter so they participate only in their own accessor's dispatch loop.
		sb.Append("\t\t\t\tthis.").Append(mockRegistryName).Append(".SetupMethod(").Append(scopePrefix)
			.Append("indexerSetup.Getter);").AppendLine();
		sb.Append("\t\t\t\tthis.").Append(mockRegistryName).Append(".SetupMethod(").Append(scopePrefix)
			.Append("indexerSetup.Setter);").AppendLine();
		sb.Append("\t\t\t\treturn indexerSetup;").AppendLine();
		sb.Append("\t\t\t}").AppendLine();
		sb.Append("\t\t}").AppendLine();
		sb.Append("#endif").AppendLine();
		sb.AppendLine();
	}

	private static void AppendIndexerVerifyDefinition(StringBuilder sb, Property indexer, string verifyName,
		bool[]? valueFlags = null, bool hasOverloadResolutionPriority = false)
	{
		// Ref-struct-keyed indexers: no Verify surface — same rationale as the setup path.
		if (indexer.IndexerParameters!.Value.Any(p => p.NeedsRefStructPipeline()))
		{
			return;
		}

		sb.AppendXmlSummary(
			$"Verify interactions with the {indexer.Type.Fullname.EscapeForXmlDoc()} indexer <see cref=\"{indexer.ContainingType.EscapeForXmlDoc()}.this[{string.Join(", ", indexer.IndexerParameters!.Value.Select(p => p.RefKind.GetString() + p.Type.Fullname.EscapeForXmlDoc()))}]\" />.");
		AppendOverloadDifferentiatorRemark(sb,
			indexer.IndexerParameters!.Value.Select(p => p.Name).ToArray(),
			useParameters: false, valueFlags, isVerify: true);
		if (hasOverloadResolutionPriority)
		{
			sb.Append("\t\t[global::System.Runtime.CompilerServices.OverloadResolutionPriority(")
				.Append(valueFlags?.Count(x => !x).ToString() ?? "int.MaxValue").Append(")]").AppendLine();
		}

		sb.Append("\t\tglobal::Mockolate.Verify.VerificationIndexerResult<").Append(verifyName).Append(", ")
			.AppendTypeOrWrapper(indexer.Type).Append("> this[");
		int i = 0;
		foreach (MethodParameter parameter in indexer.IndexerParameters!.Value)
		{
			if (i > 0)
			{
				sb.Append(", ");
			}

			bool isValueParam = valueFlags?[i] == true;
			if (isValueParam)
			{
				sb.Append(parameter.ToNullableType()).Append(' ').Append(parameter.Name);
			}
			else
			{
				sb.Append(parameter.ToParameter());
				if (parameter.CanUseNullableParameterOverload())
				{
					sb.Append('?');
				}

				sb.Append(' ').Append(parameter.Name);
			}

			i++;
		}

		sb.Append("] { get; }").AppendLine();
		sb.AppendLine();
	}

	private static void AppendIndexerVerifyImplementation(StringBuilder sb, Property indexer, string mockRegistryName,
		string verifyName, bool[]? valueFlags = null)
	{
		if (indexer.IndexerParameters!.Value.Any(p => p.NeedsRefStructPipeline()))
		{
			return;
		}

		sb.Append("\t\t/// <inheritdoc />").AppendLine();
		sb.Append(
				"\t\t[global::System.Diagnostics.DebuggerBrowsable(global::System.Diagnostics.DebuggerBrowsableState.Never)]")
			.AppendLine();
		sb.Append("\t\tglobal::Mockolate.Verify.VerificationIndexerResult<").Append(verifyName).Append(", ")
			.AppendTypeOrWrapper(indexer.Type).Append("> ").Append(verifyName).Append(".this[");
		int i = 0;
		foreach (MethodParameter parameter in indexer.IndexerParameters!.Value)
		{
			if (i > 0)
			{
				sb.Append(", ");
			}

			bool isValueParam = valueFlags?[i] == true;
			if (isValueParam)
			{
				sb.Append(parameter.ToNullableType()).Append(' ').Append(parameter.Name);
			}
			else
			{
				sb.Append(parameter.ToParameter());
				if (parameter.CanUseNullableParameterOverload())
				{
					sb.Append('?');
				}

				sb.Append(' ').Append(parameter.Name);
			}

			i++;
		}

		sb.Append("]").AppendLine();
		sb.Append("\t\t{").AppendLine();
		sb.Append("\t\t\tget").AppendLine();
		sb.Append("\t\t\t{").AppendLine();
		sb.Append("\t\t\t\treturn new global::Mockolate.Verify.VerificationIndexerResult<").Append(verifyName)
			.Append(", ").AppendTypeOrWrapper(indexer.Type).Append(">(this, this.").Append(mockRegistryName)
			.Append(",").AppendLine();

		sb.Append("\t\t\t\t\tinteraction => interaction is global::Mockolate.Interactions.IndexerGetterAccess<");
		int ti = 0;
		foreach (MethodParameter parameter in indexer.IndexerParameters.Value)
		{
			if (ti > 0)
			{
				sb.Append(", ");
			}

			sb.AppendTypeOrWrapper(parameter.Type);
			ti++;
		}

		sb.Append("> g");
		AppendIndexerVerifyParameterMatches(sb, indexer.IndexerParameters.Value, valueFlags, "g");
		sb.Append(",").AppendLine();

		sb.Append("\t\t\t\t\t(interaction, value) => interaction is global::Mockolate.Interactions.IndexerSetterAccess<");
		ti = 0;
		foreach (MethodParameter parameter in indexer.IndexerParameters.Value)
		{
			sb.AppendTypeOrWrapper(parameter.Type).Append(", ");
			ti++;
		}

		sb.AppendTypeOrWrapper(indexer.Type).Append("> s");
		AppendIndexerVerifyParameterMatches(sb, indexer.IndexerParameters.Value, valueFlags, "s");
		sb.Append(" && value.Matches(s.TypedValue),").AppendLine();

		sb.Append("\t\t\t\t\t() => global::System.String.Format(\"[");

		for (int k = 0; k < indexer.IndexerParameters.Value.Count; k++)
		{
			if (k > 0)
			{
				sb.Append(", ");
			}

			sb.Append("{").Append(k).Append("}");
		}

		sb.Append("]\"");
		int kk = 0;
		foreach (string? parameterName in indexer.IndexerParameters.Value.Select(p => p.Name))
		{
			sb.Append(", ");
			bool isValueParam = valueFlags?[kk] == true;
			if (isValueParam)
			{
				sb.Append("(object?)").Append(parameterName);
			}
			else
			{
				sb.Append("(object?)").Append(parameterName).Append(" ?? \"null\"");
			}

			kk++;
		}

		sb.Append("));").AppendLine();
		sb.Append("\t\t\t}").AppendLine();
		sb.Append("\t\t}").AppendLine();
		sb.AppendLine();
	}

	private static void AppendIndexerVerifyParameterMatches(StringBuilder sb,
		EquatableArray<MethodParameter> parameters, bool[]? valueFlags, string interactionVar)
	{
		int j = 0;
		foreach (MethodParameter parameter in parameters)
		{
			sb.Append(" && ");
			bool isValueParam = valueFlags?[j] == true;
			if (isValueParam)
			{
				sb.Append("global::System.Collections.Generic.EqualityComparer<")
					.AppendTypeOrWrapper(parameter.Type).Append(">.Default.Equals(").Append(parameter.Name)
					.Append(", ").Append(interactionVar).Append(".Parameter").Append(j + 1).Append(")");
			}
			else
			{
				sb.Append("CovariantParameterAdapter<").AppendTypeOrWrapper(parameter.Type)
					.Append(">.Wrap(").Append(parameter.Name);
				if (parameter.CanUseNullableParameterOverload())
				{
					sb.Append(" ?? global::Mockolate.It.IsNull<").Append(parameter.ToNullableType())
						.Append(">(\"null\")");
				}

				sb.Append(").Matches(").Append(interactionVar).Append(".Parameter").Append(j + 1).Append(")");
			}

			j++;
		}
	}

	#endregion Setup Helpers

	#region Raise Helpers

	private static void DefineRaiseInterface(StringBuilder sb, Class @class, MemberType memberType)
	{
		Func<Event, bool> predicate = @event => @event.ExplicitImplementation is null &&
		                                        @event.MemberType == memberType;
		foreach (Event @event in @class.AllEvents().Where(predicate))
		{
			sb.AppendXmlSummary(
				$"Raise the <see cref=\"{@event.ContainingType.EscapeForXmlDoc()}.{@event.Name}\"/> event.");
			sb.Append("\t\tvoid ").Append(@event.Name).Append("(")
				.Append(FormatParametersWithTypeAndName(@event.Delegate.Parameters)).Append(");").AppendLine();
			sb.AppendLine();
		}

		foreach (Event @event in @class.AllEvents()
			         .Where(predicate)
			         .GroupBy(m => m.Name)
			         .Where(g => g.Count() == 1)
			         .Select(g => g.Single())
			         .Where(m => m.Delegate.Parameters.Count > 0))
		{
			sb.AppendXmlSummary(
				$"Raise the <see cref=\"{@event.ContainingType.EscapeForXmlDoc()}.{@event.Name}\"/> event.");
			sb.Append("\t\tvoid ").Append(@event.Name)
				.Append("(global::Mockolate.Parameters.IDefaultEventParameters parameters);").AppendLine();
			sb.AppendLine();
		}
	}

	private static void ImplementRaiseInterface(StringBuilder sb, Class @class, string mockRegistryName,
		string raiseOnName, MemberType memberType)
	{
		string mockRegistry =
			memberType == MemberType.Static ? "MockRegistryProvider.Value" : $"this.{mockRegistryName}";
		Func<Event, bool> predicate = @event => @event.ExplicitImplementation is null &&
		                                        @event.MemberType == memberType;
		foreach (Event @event in @class.AllEvents().Where(predicate))
		{
			string backingFieldAccess = @event.IsStatic
				? $"{@event.GetBackingFieldName()}.Value"
				: $"this.{@event.GetBackingFieldName()}";
			sb.Append("\t\t/// <inheritdoc />").AppendLine();
			sb.Append("\t\tvoid ").Append(raiseOnName).Append('.').Append(@event.Name).Append("(")
				.Append(FormatParametersWithTypeAndName(@event.Delegate.Parameters))
				.Append(")").AppendLine();
			sb.AppendLine("\t\t{");
			sb.Append("\t\t\t").Append(backingFieldAccess).Append("?.Invoke(");
			if (@event.Delegate.Parameters.Count > 0)
			{
				sb.Append(FormatParametersAsNames(@event.Delegate.Parameters));
			}

			sb.Append(");").AppendLine();
			sb.AppendLine("\t\t}");
			sb.AppendLine();
		}

		foreach (Event @event in @class.AllEvents()
			         .Where(predicate)
			         .GroupBy(m => m.Name)
			         .Where(g => g.Count() == 1)
			         .Select(g => g.Single())
			         .Where(m => m.Delegate.Parameters.Count > 0))
		{
			string backingFieldAccess = @event.IsStatic
				? $"{@event.GetBackingFieldName()}.Value"
				: $"this.{@event.GetBackingFieldName()}";
			sb.Append("\t\t/// <inheritdoc />").AppendLine();
			sb.Append("\t\tvoid ").Append(raiseOnName).Append('.').Append(@event.Name)
				.Append("(global::Mockolate.Parameters.IDefaultEventParameters parameters)")
				.AppendLine();
			sb.AppendLine("\t\t{");
			sb.Append("\t\t\tglobal::Mockolate.MockBehavior mockBehavior = ").Append(mockRegistry).Append(".Behavior;")
				.AppendLine();
			sb.Append("\t\t\t").Append(backingFieldAccess).Append("?.Invoke(");

			if (@event.Delegate.Parameters.Count > 0)
			{
				sb.Append(string.Join(", ",
					@event.Delegate.Parameters.Select(p
						=> $"mockBehavior.DefaultValue.Generate(default({p.Type.Fullname.TrimEnd('?')}))")));
			}

			sb.Append(");").AppendLine();
			sb.AppendLine("\t\t}");
			sb.AppendLine();
		}
	}

	#endregion Raise Helpers

	#region Verify Helpers

	private static void DefineVerifyInterface(StringBuilder sb, Class @class, string verifyName, MemberType memberType,
		bool hasOverloadResolutionPriority)
	{
		#region Properties

		Func<Property, bool> propertyPredicate = property
			=> property.ExplicitImplementation is null && property is { IsIndexer: false, } &&
			   property.MemberType == memberType;
		foreach (Property property in @class.AllProperties().Where(propertyPredicate))
		{
			sb.AppendXmlSummary(
				$"Verify interactions with the {property.Type.Fullname.EscapeForXmlDoc()} property <see cref=\"{property.ContainingType.EscapeForXmlDoc()}.{property.Name}\" />.");
			sb.Append("\t\tglobal::Mockolate.Verify.VerificationPropertyResult<").Append(verifyName).Append(", ")
				.Append(property.Type.Fullname).Append("> ").Append(property.Name).Append(" { get; }").AppendLine();
			sb.AppendLine();
		}

		#endregion

		#region Indexers

		Func<Property, bool> indexerPredicate =
			indexer => indexer.ExplicitImplementation is null && indexer is { IsIndexer: true, } &&
			           indexer.MemberType == memberType;
		foreach (Property indexer in @class.AllProperties().Where(indexerPredicate))
		{
			AppendIndexerVerifyDefinition(sb, indexer, verifyName,
				hasOverloadResolutionPriority: hasOverloadResolutionPriority);
			if (indexer.IndexerParameters!.Value.Count <= MaxExplicitParameters)
			{
				foreach (bool[] valueFlags in GenerateValueFlagCombinations(indexer.IndexerParameters.Value))
				{
					AppendIndexerVerifyDefinition(sb, indexer, verifyName, valueFlags, hasOverloadResolutionPriority);
				}
			}
			else
			{
				bool[] allValueFlags = indexer.IndexerParameters.Value.Select(p => p.CanUseNullableParameterOverload())
					.ToArray();
				if (allValueFlags.Any(f => f))
				{
					AppendIndexerVerifyDefinition(sb, indexer, verifyName, allValueFlags,
						hasOverloadResolutionPriority);
				}
			}
		}

		#endregion

		#region Methods

		bool MethodPredicate(Method method)
		{
			return method.ExplicitImplementation is null && method.MemberType == memberType;
		}

		List<IGrouping<string, Method>> methodGroups =
			@class.AllMethods().Where((Func<Method, bool>)MethodPredicate).GroupBy(m => m.Name).ToList();
		foreach (IGrouping<string, Method>? methodGroup in methodGroups)
		{
			if (methodGroup.Count() == 1)
			{
				Method? method = methodGroup.Single();
				if (method.Parameters.Count > 0)
				{
					AppendMethodVerifyDefinition(sb, method, verifyName, true,
						hasOverloadResolutionPriority: hasOverloadResolutionPriority);
				}
			}

			foreach (Method? method in methodGroup)
			{
				if (method.Parameters.Count == 0)
				{
					AppendMethodVerifyDefinition(sb, method, verifyName, false,
						hasOverloadResolutionPriority: hasOverloadResolutionPriority);
				}
				else
				{
					AppendMethodVerifyDefinition(sb, method, verifyName, false,
						hasOverloadResolutionPriority: hasOverloadResolutionPriority);
					if (method.Parameters.Count <= MaxExplicitParameters)
					{
						foreach (bool[] valueFlags in GenerateValueFlagCombinations(method.Parameters))
						{
							AppendMethodVerifyDefinition(sb, method, verifyName, false, valueFlags: valueFlags,
								hasOverloadResolutionPriority: hasOverloadResolutionPriority);
						}
					}
					else
					{
						bool[] allValueFlags = method.Parameters.Select(p => p.CanUseNullableParameterOverload())
							.ToArray();
						if (allValueFlags.Any(f => f))
						{
							AppendMethodVerifyDefinition(sb, method, verifyName, false, valueFlags: allValueFlags,
								hasOverloadResolutionPriority: hasOverloadResolutionPriority);
						}
					}
				}
			}
		}

		#endregion

		#region Events

		Func<Event, bool> eventPredicate = @event => @event.ExplicitImplementation is null &&
		                                             @event.MemberType == memberType;
		foreach (Event @event in @class.AllEvents().Where(eventPredicate))
		{
			sb.AppendXmlSummary(
				$"Verify subscriptions on the {@event.Name} event of <see cref=\"{@event.ContainingType.EscapeForXmlDoc()}.{@event.Name}\" />.");
			sb.Append("\t\tglobal::Mockolate.Verify.VerificationEventResult<").Append(verifyName).Append("> ")
				.Append(@event.Name).Append(" { get; }").AppendLine();
			sb.AppendLine();
		}

		#endregion
	}

	private static void AppendMethodVerifyDefinition(StringBuilder sb, Method method, string verifyName,
		bool useParameters, string? methodNameOverride = null, bool[]? valueFlags = null,
		bool hasOverloadResolutionPriority = false)
	{
		// For methods with ref-struct parameters, skip Verify emission entirely. The
		// VerificationResult pipeline takes IParameter<T>? matchers that then feed into
		// closures over the captured call values — incompatible with a ref-struct T. Callers
		// fall back to reading `mock.Mock.Registry.Interactions.OfType<RefStructMethodInvocation>()`
		// for count-based verification. See brief's "verify count" guidance.
		if (method.Parameters.Any(p => p.NeedsRefStructPipeline()))
		{
			return;
		}

		string methodName = methodNameOverride ?? method.Name;
		sb.Append("\t\t/// <summary>").AppendLine();
		if (methodNameOverride is null)
		{
			sb.Append("\t\t///     Verify invocations for the method <see cref=\"")
				.Append(method.ContainingType.EscapeForXmlDoc())
				.Append(".").Append(methodName.EscapeForXmlDoc()).Append("(");
			sb.Append(string.Join(", ",
				method.Parameters.Select(p => p.RefKind.GetString() + p.Type.Fullname.EscapeForXmlDoc())));
			sb.Append(")\"/>");
		}
		else
		{
			sb.Append("\t\t///     Verify invocations for the delegate <see cref=\"")
				.Append(method.ContainingType.EscapeForXmlDoc()).Append("\"/>");
		}

		sb.Append(method.Parameters.Count > 0 ? " with the given " : "");
		if (useParameters)
		{
			sb.Append("<paramref name=\"parameters\"/>");
		}
		else
		{
			sb.Append(string.Join(", ", method.Parameters.Select(p => $"<paramref name=\"{p.Name}\"/>")));
		}

		sb.Append(".").AppendLine();
		sb.Append("\t\t/// </summary>").AppendLine();
		AppendOverloadDifferentiatorRemark(sb, method.Parameters.Select(p => p.Name).ToArray(), useParameters, valueFlags, isVerify: true);
		if (valueFlags?.All(x => x) == true)
		{
			sb.Append("\t\tglobal::Mockolate.Verify.VerificationResult<").Append(verifyName)
				.Append(">.IgnoreParameters ").Append(methodName).Append("(");
		}
		else
		{
			if (hasOverloadResolutionPriority)
			{
				sb.Append("\t\t[global::System.Runtime.CompilerServices.OverloadResolutionPriority(")
					.Append(valueFlags?.Count(x => !x).ToString() ?? "int.MaxValue").Append(")]").AppendLine();
			}

			sb.Append("\t\tglobal::Mockolate.Verify.VerificationResult<").Append(verifyName).Append("> ")
				.Append(methodName).Append("(");
		}

		if (useParameters)
		{
			sb.Append("global::Mockolate.Parameters.IParameters parameters");
		}
		else
		{
			ReadOnlySpan<MethodParameter> paramSpan = method.Parameters.AsSpan();
			bool[] hasTrailingDefault = ComputeTrailingDefaults(paramSpan, valueFlags);

			int i = 0;
			foreach (MethodParameter parameter in method.Parameters)
			{
				if (i > 0)
				{
					sb.Append(", ");
				}

				bool isValueParam = valueFlags?[i] == true;
				if (isValueParam)
				{
					sb.Append(parameter.ToNullableType()).Append(' ').Append(parameter.Name);
				}
				else
				{
					sb.AppendVerifyParameter(parameter);
					if (parameter.CanUseNullableParameterOverload())
					{
						sb.Append('?');
					}

					sb.Append(' ').Append(parameter.Name);
					if (hasTrailingDefault[i])
					{
						sb.Append(" = null");
					}
				}

				i++;
			}
		}

		sb.Append(")");
		if (method.GenericParameters is not null && method.GenericParameters.Value.Count > 0)
		{
			foreach (GenericParameter gp in method.GenericParameters.Value)
			{
				gp.AppendWhereConstraint(sb, "\t\t\t");
			}
		}

		sb.Append(";").AppendLine();
		sb.AppendLine();
	}

	private static void ImplementVerifyInterface(StringBuilder sb, Class @class, string mockRegistryName,
		string verifyName, MemberType memberType)
	{
		#region Properties

		Func<Property, bool> propertyPredicate = property
			=> property.ExplicitImplementation is null && property is { IsIndexer: false, } &&
			   property.MemberType == memberType;
		foreach (Property property in @class.AllProperties().Where(propertyPredicate))
		{
			sb.Append("\t\t/// <inheritdoc />").AppendLine();
			sb.Append(
					"\t\t[global::System.Diagnostics.DebuggerBrowsable(global::System.Diagnostics.DebuggerBrowsableState.Never)]")
				.AppendLine();
			sb.Append("\t\tglobal::Mockolate.Verify.VerificationPropertyResult<").Append(verifyName).Append(", ")
				.Append(property.Type.Fullname).Append("> ").Append(verifyName).Append('.').Append(property.Name)
				.AppendLine();
			sb.Append("\t\t{").AppendLine();
			sb.Append("\t\t\tget").AppendLine();
			sb.Append("\t\t\t{").AppendLine();
			sb.Append("\t\t\t\treturn new global::Mockolate.Verify.VerificationPropertyResult<").Append(verifyName)
				.Append(", ").Append(property.Type.Fullname).Append(">(this, this.").Append(mockRegistryName)
				.Append(", ").Append(property.GetUniqueNameString()).Append(");").AppendLine();
			sb.Append("\t\t\t}").AppendLine();
			sb.Append("\t\t}").AppendLine();
			sb.AppendLine();
		}

		#endregion

		#region Indexers

		Func<Property, bool> indexerPredicate = indexer
			=> indexer.ExplicitImplementation is null && indexer is { IsIndexer: true, IndexerParameters: not null, } &&
			   indexer.MemberType == memberType;
		foreach (Property indexer in @class.AllProperties().Where(indexerPredicate))
		{
			AppendIndexerVerifyImplementation(sb, indexer, mockRegistryName, verifyName);
			if (indexer.IndexerParameters!.Value.Count <= MaxExplicitParameters)
			{
				foreach (bool[] valueFlags in GenerateValueFlagCombinations(indexer.IndexerParameters.Value))
				{
					AppendIndexerVerifyImplementation(sb, indexer, mockRegistryName, verifyName, valueFlags);
				}
			}
			else
			{
				bool[] allValueFlags = indexer.IndexerParameters.Value.Select(p => p.CanUseNullableParameterOverload())
					.ToArray();
				if (allValueFlags.Any(f => f))
				{
					AppendIndexerVerifyImplementation(sb, indexer, mockRegistryName, verifyName, allValueFlags);
				}
			}
		}

		#endregion

		#region Methods

		bool MethodPredicate(Method method)
		{
			return method.ExplicitImplementation is null && method.MemberType == memberType;
		}

		List<IGrouping<string, Method>> methodGroups =
			@class.AllMethods().Where((Func<Method, bool>)MethodPredicate).GroupBy(m => m.Name).ToList();
		foreach (IGrouping<string, Method>? methodGroup in methodGroups)
		{
			if (methodGroup.Count() == 1)
			{
				Method? method = methodGroup.Single();
				if (method.Parameters.Count > 0)
				{
					AppendMethodVerifyImplementation(sb, method, mockRegistryName, verifyName, true);
				}
			}

			foreach (Method? method in methodGroup)
			{
				if (method.Parameters.Count == 0)
				{
					AppendMethodVerifyImplementation(sb, method, mockRegistryName, verifyName, false);
				}
				else
				{
					AppendMethodVerifyImplementation(sb, method, mockRegistryName, verifyName, false);
					if (method.Parameters.Count <= MaxExplicitParameters)
					{
						foreach (bool[] valueFlags in GenerateValueFlagCombinations(method.Parameters))
						{
							AppendMethodVerifyImplementation(sb, method, mockRegistryName, verifyName, false,
								valueFlags: valueFlags);
						}
					}
					else
					{
						bool[] allValueFlags = method.Parameters.Select(p => p.CanUseNullableParameterOverload())
							.ToArray();
						if (allValueFlags.Any(f => f))
						{
							AppendMethodVerifyImplementation(sb, method, mockRegistryName, verifyName, false,
								valueFlags: allValueFlags);
						}
					}
				}
			}
		}

		#endregion

		#region Events

		Func<Event, bool> eventPredicate = @event => @event.ExplicitImplementation is null &&
		                                             @event.MemberType == memberType;
		foreach (Event @event in @class.AllEvents().Where(eventPredicate))
		{
			sb.AppendXmlSummary(
				$"Verify subscriptions on the {@event.Name} event <see cref=\"{@event.ContainingType.EscapeForXmlDoc()}.{@event.Name}\" />.");
			sb.Append(
					"\t\t[global::System.Diagnostics.DebuggerBrowsable(global::System.Diagnostics.DebuggerBrowsableState.Never)]")
				.AppendLine();
			sb.Append("\t\tglobal::Mockolate.Verify.VerificationEventResult<").Append(verifyName).Append("> ")
				.Append(verifyName).Append('.').Append(@event.Name).AppendLine();
			sb.Append("\t\t{").AppendLine();
			sb.Append("\t\t\tget").AppendLine();
			sb.Append("\t\t\t{").AppendLine();
			sb.Append("\t\t\t\treturn new global::Mockolate.Verify.VerificationEventResult<").Append(verifyName)
				.Append(">(this, this.").Append(mockRegistryName).Append(", ").Append(@event.GetUniqueNameString())
				.Append(");").AppendLine();
			sb.Append("\t\t\t}").AppendLine();
			sb.Append("\t\t}").AppendLine();
			sb.AppendLine();
		}

		#endregion
	}

	private static void AppendMethodVerifyImplementation(StringBuilder sb, Method method, string mockRegistryName,
		string verifyName,
		bool useParameters, string? methodNameOverride = null, bool[]? valueFlags = null)
	{
		// Mirror the AppendMethodVerifyDefinition short-circuit for ref-struct signatures.
		if (method.Parameters.Any(p => p.NeedsRefStructPipeline()))
		{
			return;
		}

		string methodName = methodNameOverride ?? method.Name;
		sb.Append("\t\t/// <inheritdoc />").AppendLine();
		sb.Append("\t\tglobal::Mockolate.Verify.VerificationResult<");
		sb.Append(verifyName).Append('>');
		if (valueFlags?.All(x => x) == true)
		{
			sb.Append(".IgnoreParameters");
		}

		sb.Append(' ').Append(verifyName).Append('.').Append(methodName).Append("(");
		if (useParameters)
		{
			sb.Append("global::Mockolate.Parameters.IParameters parameters");
		}
		else
		{
			int i = 0;
			foreach (MethodParameter parameter in method.Parameters)
			{
				if (i > 0)
				{
					sb.Append(", ");
				}

				bool isValueParam = valueFlags?[i] == true;
				if (isValueParam)
				{
					sb.Append(parameter.ToNullableType()).Append(' ').Append(parameter.Name);
				}
				else
				{
					sb.AppendVerifyParameter(parameter);
					if (parameter.CanUseNullableParameterOverload())
					{
						sb.Append('?');
					}

					sb.Append(' ').Append(parameter.Name);
				}

				i++;
			}
		}

		sb.Append(")");
		if (method.GenericParameters is not null && method.GenericParameters.Value.Count > 0)
		{
			foreach (GenericParameter gp in method.GenericParameters.Value)
			{
				gp.AppendWhereConstraint(sb, "\t\t\t", true, true);
			}
		}

		sb.AppendLine();
		sb.Append("\t\t\t=> this.").Append(mockRegistryName).Append(".VerifyMethod<").Append(verifyName)
			.Append(", global::Mockolate.Interactions.MethodInvocation");
		if (method.Parameters.Count > 0)
		{
			sb.Append("<").Append(string.Join(", ", method.Parameters.Select(p => p.ToTypeOrWrapper()))).Append(">");
		}

		sb.Append(">(this, ").Append(method.GetUniqueNameString());
		if (useParameters)
		{
			sb.Append(", i => parameters switch").AppendLine();
			sb.Append("\t\t\t\t{").AppendLine();
			sb.Append("\t\t\t\t\tglobal::Mockolate.Parameters.IParametersMatch m => m.Matches([").Append(string.Join(", ", Enumerable.Range(1, method.Parameters.Count).Select(i => $"i.Parameter{i}"))).Append("]),").AppendLine();
			sb.Append("\t\t\t\t\tglobal::Mockolate.Parameters.INamedParametersMatch m => m.Matches([").Append(string.Join(", ", method.Parameters.Select((p, i) => $"(\"{p.Name}\", i.Parameter{i + 1})"))).Append("]),").AppendLine();
			sb.Append("\t\t\t\t\t_ => true").AppendLine();
			sb.Append("\t\t\t\t}");
		}
		else if (method.Parameters.Count == 0)
		{
			sb.Append(", i => true");
		}
		else
		{
			sb.Append(", i => ");

			int i = 0;
			foreach (MethodParameter parameter in method.Parameters)
			{
				if (i > 0)
				{
					sb.Append(" && ");
				}

				sb.AppendLine().Append("\t\t\t\t");

				bool isValueParam = valueFlags?[i] == true;
				if (isValueParam)
				{
					sb.Append($"(global::System.Collections.Generic.EqualityComparer<{parameter.ToTypeOrWrapper()}>.Default.Equals({parameter.Name}, i.Parameter{i + 1}))");
				}
				else if (parameter.RefKind == RefKind.Out || parameter.RefKind == RefKind.Ref || parameter.RefKind == RefKind.RefReadOnlyParameter)
				{
					// out/ref verify parameters use IVerifyOutParameter<T> / IVerifyRefParameter<T>, which don't inherit
					// from IParameter<T> — covariance isn't applicable, so keep the direct IParameterMatch<T> check.
					sb.Append($"({parameter.Name} is global::Mockolate.Parameters.IParameterMatch<{parameter.ToTypeOrWrapper()}> {parameter.Name}Match ? {parameter.Name}Match.Matches(i.Parameter{i + 1}) : global::System.Collections.Generic.EqualityComparer<{parameter.ToTypeOrWrapper()}>.Default.Equals(i.Parameter{i + 1}, default({parameter.ToTypeOrWrapper()})))");
				}
				else
				{
					sb.Append($"({parameter.Name} is not null ? CovariantParameterAdapter<{parameter.ToTypeOrWrapper()}>.Wrap({parameter.Name}).Matches(i.Parameter{i + 1}) : global::System.Collections.Generic.EqualityComparer<{parameter.ToTypeOrWrapper()}>.Default.Equals(i.Parameter{i + 1}, default({parameter.ToTypeOrWrapper()})))");
				}

				i++;
			}
		}

		sb.Append(", () => $\"").Append(method.Name).Append("(").Append(useParameters ? "{parameters}" : string.Join(", ", method.Parameters.Select(p => $"{{{p.Name}}}"))).Append(")\");").AppendLine();
	}

	#endregion Verify Helpers
}
#pragma warning restore S3776 // Cognitive Complexity of methods should not be too high
