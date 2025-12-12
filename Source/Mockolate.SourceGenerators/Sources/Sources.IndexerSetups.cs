using System.Text;

namespace Mockolate.SourceGenerators.Sources;

internal static partial class Sources
{
	public static string IndexerSetups(HashSet<int> indexerSetups)
	{
		StringBuilder sb = InitializeBuilder([
			"System",
			"System.Collections.Generic",
			"System.Diagnostics.CodeAnalysis",
			"System.Threading",
			"Mockolate.Exceptions",
			"Mockolate.Interactions",
			"Mockolate.Parameters",
		]);

		sb.Append("""
		          #nullable enable

		          namespace Mockolate.Setup
		          {
		          """);
		foreach (int item in indexerSetups)
		{
			sb.AppendLine();
			AppendIndexerSetup(sb, item);
		}

		sb.AppendLine();
		sb.Append("""
		          }

		          namespace Mockolate
		          {
		          	internal static class IndexerSetupExtensions
		          	{
		          """).AppendLine();
		foreach (int item in indexerSetups)
		{
			string types = GetGenericTypeParameters(item);
			sb.Append($$"""
			            		/// <summary>
			            		///     Extensions for indexer callback setups with {{item}} parameters.
			            		/// </summary>
			            		extension<TValue, {{types}}>(Mockolate.Setup.IIndexerSetupCallbackWhenBuilder<TValue, {{types}}> setup)
			            		{
			            			/// <summary>
			            			///     Executes the callback only once.
			            			/// </summary>
			            			public Mockolate.Setup.IIndexerSetup<TValue, {{types}}> OnlyOnce()
			            				=> setup.Only(1);
			            		}
			            		
			            		/// <summary>
			            		///     Extensions for indexer setups with {{item}} parameters.
			            		/// </summary>
			            		extension<TValue, {{types}}>(Mockolate.Setup.IIndexerSetupReturnWhenBuilder<TValue, {{types}}> setup)
			            		{
			            			/// <summary>
			            			///     Returns/throws forever.
			            			/// </summary>
			            			public void Forever()
			            			{
			            				setup.For(int.MaxValue);
			            			}

			            			/// <summary>
			            			///     Uses the return value only once.
			            			/// </summary>
			            			public Mockolate.Setup.IIndexerSetup<TValue, {{types}}> OnlyOnce()
			            				=> setup.Only(1);
			            		}
			            """).AppendLine();
		}

		sb.Append("""
		          	}
		          }
		          """).AppendLine();
		sb.AppendLine();

		sb.AppendLine("#nullable disable");
		return sb.ToString();
	}

	private static void AppendIndexerSetup(StringBuilder sb, int numberOfParameters)
	{
		string typeParams = GetGenericTypeParameters(numberOfParameters);
		string outTypeParams = GetOutGenericTypeParameters(numberOfParameters);
		string parameters = string.Join(", ", Enumerable.Range(1, numberOfParameters).Select(i => $"p{i}"));
		string discards = string.Join(", ", Enumerable.Range(1, numberOfParameters).Select(_ => "_"));

		sb.AppendXmlSummary($"Sets up a <typeparamref name=\"TValue\"/> indexer getter for {GetTypeParametersDescription(numberOfParameters)}.", "\t");
		sb.Append("\tinternal interface IIndexerGetterSetup<TValue, ").Append(outTypeParams).Append(">").AppendLine();
		sb.Append("\t{").AppendLine();
		sb.AppendXmlSummary("Registers a <paramref name=\"callback\"/> to be invoked whenever the indexer's getter is accessed.");
		sb.Append("\t\tIIndexerSetupCallbackBuilder<TValue, ").Append(typeParams).Append("> Do(Action callback);")
			.AppendLine();
		sb.AppendLine();

		sb.AppendXmlSummary("Registers a <paramref name=\"callback\"/> to be invoked whenever the indexer's getter is accessed.");
		sb.AppendXmlRemarks("The callback receives the parameters of the indexer.");
		sb.Append("\t\tIIndexerSetupCallbackBuilder<TValue, ").Append(typeParams).Append("> Do(Action<")
			.Append(typeParams)
			.Append("> callback);").AppendLine();
		sb.AppendLine();

		sb.AppendXmlSummary("Registers a <paramref name=\"callback\"/> to be invoked whenever the indexer's getter is accessed.");
		sb.AppendXmlRemarks("The callback receives the parameters of the indexer and the value of the indexer as last parameter.");
		sb.Append("\t\tIIndexerSetupCallbackBuilder<TValue, ").Append(typeParams).Append("> Do(Action<")
			.Append(typeParams)
			.Append(", TValue> callback);").AppendLine();
		sb.AppendLine();

		sb.AppendXmlSummary("Registers a <paramref name=\"callback\"/> to be invoked whenever the indexer's getter is accessed.");
		sb.AppendXmlRemarks("The callback receives an incrementing access counter as first parameter, the parameters of the indexer and the value of the indexer as last parameter.");
		sb.Append("\t\tIIndexerSetupCallbackBuilder<TValue, ").Append(typeParams).Append("> Do(Action<int, ")
			.Append(typeParams)
			.Append(", TValue> callback);").AppendLine();
		sb.Append("\t}").AppendLine();
		sb.AppendLine();

		sb.AppendXmlSummary($"Sets up a <typeparamref name=\"TValue\"/> indexer setter for {GetTypeParametersDescription(numberOfParameters)}.", "\t");
		sb.Append("\tinternal interface IIndexerSetterSetup<TValue, ").Append(outTypeParams).Append(">").AppendLine();
		sb.Append("\t{").AppendLine();
		sb.AppendXmlSummary("Registers a <paramref name=\"callback\"/> to be invoked whenever the indexer's setter is accessed.");
		sb.Append("\t\tIIndexerSetupCallbackBuilder<TValue, ").Append(typeParams).Append("> Do(Action callback);")
			.AppendLine();
		sb.AppendLine();

		sb.AppendXmlSummary("Registers a <paramref name=\"callback\"/> to be invoked whenever the indexer's setter is accessed.");
		sb.AppendXmlRemarks("The callback receives the value the indexer is set to as single parameter.");
		sb.Append("\t\tIIndexerSetupCallbackBuilder<TValue, ").Append(typeParams)
			.Append("> Do(Action<TValue> callback);").AppendLine();
		sb.AppendLine();

		sb.AppendXmlSummary("Registers a <paramref name=\"callback\"/> to be invoked whenever the indexer's setter is accessed.");
		sb.AppendXmlRemarks("The callback receives the parameters of the indexer and the value the indexer is set to as last parameter.");
		sb.Append("\t\tIIndexerSetupCallbackBuilder<TValue, ").Append(typeParams).Append("> Do(Action<")
			.Append(typeParams).Append(", TValue> callback);").AppendLine();
		sb.AppendLine();

		sb.AppendXmlSummary("Registers a <paramref name=\"callback\"/> to be invoked whenever the indexer's setter is accessed.");
		sb.AppendXmlRemarks("The callback receives an incrementing access counter as first parameter, the parameters of the indexer and the value the indexer is set to as last parameter.");
		sb.Append("\t\tIIndexerSetupCallbackBuilder<TValue, ").Append(typeParams).Append("> Do(Action<int, ")
			.Append(typeParams).Append(", TValue> callback);").AppendLine();
		sb.Append("\t}").AppendLine();
		sb.AppendLine();

		sb.AppendXmlSummary($"Sets up a <typeparamref name=\"TValue\"/> indexer for {GetTypeParametersDescription(numberOfParameters)}.", "\t");
		sb.Append("\tinternal interface IIndexerSetup<TValue, ").Append(outTypeParams).Append(">").AppendLine();
		sb.Append("\t{").AppendLine();
		sb.AppendXmlSummary("Sets up callbacks on the getter.");
		sb.Append("\t\tIIndexerGetterSetup<TValue, ").Append(typeParams)
			.Append("> OnGet { get; }").AppendLine();
		sb.AppendLine();

		sb.AppendXmlSummary("Sets up callbacks on the setter.");
		sb.Append("\t\tIIndexerSetterSetup<TValue, ").Append(typeParams)
			.Append("> OnSet { get; }").AppendLine();
		sb.AppendLine();

		sb.AppendXmlSummary("Flag indicating if the base class implementation should be called, and its return values used as default values.");
		sb.AppendXmlRemarks("If not specified, use <see cref=\"MockBehavior.CallBaseClass\" />.");
		sb.Append("\t\tIIndexerSetup<TValue, ").Append(typeParams)
			.Append("> CallingBaseClass(bool callBaseClass = true);").AppendLine();
		sb.AppendLine();

		sb.AppendXmlSummary("Initializes the indexer with the given <paramref name=\"value\" />.");
		sb.Append("\t\tIIndexerSetup<TValue, ").Append(typeParams).Append("> InitializeWith(TValue value);")
			.AppendLine();
		sb.AppendLine();

		sb.AppendXmlSummary("Initializes the indexer according to the given <paramref name=\"valueGenerator\" />.");
		sb.Append("\t\tIIndexerSetup<TValue, ").Append(typeParams).Append("> InitializeWith(Func<")
			.Append(typeParams).Append(", TValue> valueGenerator);").AppendLine();
		sb.AppendLine();

		sb.AppendXmlSummary("Registers the <paramref name=\"returnValue\" /> for this indexer.");
		sb.Append("\t\tIIndexerSetupReturnBuilder<TValue, ").Append(typeParams).Append("> Returns(TValue returnValue);")
			.AppendLine();
		sb.AppendLine();

		sb.AppendXmlSummary("Registers a <paramref name=\"callback\" /> to setup the return value for this indexer.");
		sb.Append("\t\tIIndexerSetupReturnBuilder<TValue, ").Append(typeParams)
			.Append("> Returns(Func<TValue> callback);")
			.AppendLine();
		sb.AppendLine();

		sb.AppendXmlSummary("Registers a <paramref name=\"callback\" /> to setup the return value for this indexer.");
		sb.AppendXmlRemarks("The callback receives the parameters of the indexer.");
		sb.Append("\t\tIIndexerSetupReturnBuilder<TValue, ").Append(typeParams).Append("> Returns(Func<")
			.Append(typeParams)
			.Append(", TValue> callback);").AppendLine();
		sb.AppendLine();

		sb.AppendXmlSummary("Registers a <paramref name=\"callback\" /> to setup the return value for this indexer.");
		sb.AppendXmlRemarks("The callback receives the parameters of the indexer and the value of the indexer as last parameter.");
		sb.Append("\t\tIIndexerSetupReturnBuilder<TValue, ").Append(typeParams).Append("> Returns(Func<")
			.Append(typeParams).Append(", TValue, TValue> callback);").AppendLine();
		sb.AppendLine();

		sb.AppendXmlSummary("Registers an <typeparamref name=\"TException\" /> to throw when the indexer is read.");
		sb.Append("\t\tIIndexerSetupReturnBuilder<TValue, ").Append(typeParams).Append("> Throws<TException>()")
			.AppendLine();
		sb.Append("\t\t\twhere TException : Exception, new();").AppendLine();
		sb.AppendLine();

		sb.AppendXmlSummary("Registers an <paramref name=\"exception\" /> to throw when the indexer is read.");
		sb.Append("\t\tIIndexerSetupReturnBuilder<TValue, ").Append(typeParams).Append("> Throws(Exception exception);")
			.AppendLine();
		sb.AppendLine();

		sb.AppendXmlSummary("Registers a <paramref name=\"callback\" /> that will calculate the exception to throw when the indexer is read.");
		sb.Append("\t\tIIndexerSetupReturnBuilder<TValue, ").Append(typeParams)
			.Append("> Throws(Func<Exception> callback);")
			.AppendLine();
		sb.AppendLine();

		sb.AppendXmlSummary("Registers a <paramref name=\"callback\" /> that will calculate the exception to throw when the indexer is read.");
		sb.AppendXmlRemarks("The callback receives the parameters of the indexer.");
		sb.Append("\t\tIIndexerSetupReturnBuilder<TValue, ").Append(typeParams).Append("> Throws(Func<")
			.Append(typeParams)
			.Append(", Exception> callback);").AppendLine();
		sb.AppendLine();

		sb.AppendXmlSummary("Registers a <paramref name=\"callback\" /> that will calculate the exception to throw when the indexer is read.");
		sb.AppendXmlRemarks("The callback receives the parameters of the indexer and the value of the indexer as last parameter.");
		sb.Append("\t\tIIndexerSetupReturnBuilder<TValue, ").Append(typeParams).Append("> Throws(Func<")
			.Append(typeParams).Append(", TValue, Exception> callback);").AppendLine();

		sb.Append("\t}").AppendLine();
		sb.AppendLine();

		sb.AppendXmlSummary($"Sets up a callback for a <typeparamref name=\"TValue\"/> indexer for {GetTypeParametersDescription(numberOfParameters)}.", "\t");
		sb.Append("\tinternal interface IIndexerSetupCallbackBuilder<TValue, ").Append(outTypeParams)
			.Append("> : IIndexerSetupCallbackWhenBuilder<TValue, ").Append(typeParams).Append(">").AppendLine();
		sb.Append("\t{").AppendLine();
		sb.AppendXmlSummary("Runs the callback in parallel to the other callbacks.");
		sb.Append("\t\tIIndexerSetupCallbackBuilder<TValue, ").Append(typeParams).Append("> InParallel();")
			.AppendLine();
		sb.AppendLine();
		sb.AppendXmlSummary("Limits the callback to only execute for indexer accesses where the predicate returns true.");
		sb.AppendXmlRemarks("Provides a zero-based counter indicating how many times the indexer has been accessed so far.");
		sb.Append("\t\tIIndexerSetupCallbackWhenBuilder<TValue, ").Append(typeParams)
			.Append("> When(Func<int, bool> predicate);").AppendLine();
		sb.Append("\t}").AppendLine();
		sb.AppendLine();

		sb.AppendXmlSummary($"Sets up a when callback for a <typeparamref name=\"TValue\"/> indexer for {GetTypeParametersDescription(numberOfParameters)}.", "\t");
		sb.Append("\tinternal interface IIndexerSetupCallbackWhenBuilder<TValue, ").Append(outTypeParams)
			.Append("> : IIndexerSetup<TValue, ").Append(typeParams).Append(">").AppendLine();
		sb.Append("\t{").AppendLine();
		
		sb.AppendXmlSummary("Repeats the callback for the given number of <paramref name=\"times\" />.");
		sb.AppendXmlRemarks($"The number of times is only counted for actual executions (<see cref=\"IIndexerSetupCallbackBuilder{{TValue, {typeParams}}}.When(Func{{int, bool}})\" /> evaluates to <see langword=\"true\" />).");
		sb.Append("\t\tIIndexerSetupCallbackWhenBuilder<TValue, ").Append(typeParams).Append("> For(int times);")
			.AppendLine();
		sb.AppendLine();
		sb.AppendXmlSummary("Deactivates the callback after the given number of <paramref name=\"times\" />.");
		sb.AppendXmlRemarks($"The number of times is only counted for actual executions (<see cref=\"IIndexerSetupCallbackBuilder{{TValue, {typeParams}}}.When(Func{{int, bool}})\" /> evaluates to <see langword=\"true\" />).");
		sb.Append("\t\tIIndexerSetup<TValue, ").Append(typeParams).Append("> Only(int times);").AppendLine();
		sb.Append("\t}").AppendLine();
		sb.AppendLine();

		sb.AppendXmlSummary($"Sets up a return/throw callback for a <typeparamref name=\"TValue\"/> indexer for {GetTypeParametersDescription(numberOfParameters)}.", "\t");
		sb.Append("\tinternal interface IIndexerSetupReturnBuilder<TValue, ").Append(outTypeParams)
			.Append("> : IIndexerSetupReturnWhenBuilder<TValue, ").Append(typeParams).Append(">").AppendLine();
		sb.Append("\t{").AppendLine();
		sb.AppendXmlSummary("Limits the return/throw callback to only execute for indexer accesses where the predicate returns true.");
		sb.AppendXmlRemarks("Provides a zero-based counter indicating how many times the indexer has been accessed so far.");
		sb.Append("\t\tIIndexerSetupReturnWhenBuilder<TValue, ").Append(typeParams)
			.Append("> When(Func<int, bool> predicate);").AppendLine();
		sb.Append("\t}").AppendLine();
		sb.AppendLine();

		sb.AppendXmlSummary($"Sets up a when return/throw callback for a <typeparamref name=\"TValue\"/> indexer for {GetTypeParametersDescription(numberOfParameters)}.", "\t");
		sb.Append("\tinternal interface IIndexerSetupReturnWhenBuilder<TValue, ").Append(outTypeParams)
			.Append("> : IIndexerSetup<TValue, ").Append(typeParams).Append(">").AppendLine();
		sb.Append("\t{").AppendLine();
		sb.AppendXmlSummary("Repeats the return/throw callback for the given number of <paramref name=\"times\" />.");
		sb.AppendXmlRemarks($"The number of times is only counted for actual executions (<see cref=\"IIndexerSetupReturnBuilder{{TValue, {typeParams}}}.When(Func{{int, bool}})\" /> evaluates to <see langword=\"true\" />).");
		sb.Append("\t\tIIndexerSetupReturnWhenBuilder<TValue, ").Append(typeParams).Append("> For(int times);")
			.AppendLine();
		sb.AppendLine();
		sb.AppendXmlSummary("Deactivates the return/throw after the given number of <paramref name=\"times\" />.");
		sb.AppendXmlRemarks($"The number of times is only counted for actual executions (<see cref=\"IIndexerSetupReturnBuilder{{TValue, {typeParams}}}.When(Func{{int, bool}})\" /> evaluates to <see langword=\"true\" />).");
		sb.Append("\t\tIIndexerSetup<TValue, ").Append(typeParams).Append("> Only(int times);").AppendLine();
		sb.Append("\t}").AppendLine();
		sb.AppendLine();

		sb.AppendXmlSummary($"Sets up a <typeparamref name=\"TValue\"/> indexer for {GetTypeParametersDescription(numberOfParameters)}.", "\t");
		sb.Append("\tinternal class IndexerSetup<TValue, ").Append(typeParams).Append(">(")
			.Append(
				string.Join(", ", Enumerable.Range(1, numberOfParameters).Select(i => $"IParameter match{i}")))
			.Append(") : IndexerSetup,")
			.AppendLine();
		sb.Append("\t\tIIndexerSetupCallbackBuilder<TValue, ").Append(typeParams).Append(">,").AppendLine();
		sb.Append("\t\tIIndexerSetupReturnBuilder<TValue, ").Append(typeParams).Append(">,").AppendLine();
		sb.Append("\t\tIIndexerGetterSetup<TValue, ").Append(typeParams).Append(">,").AppendLine();
		sb.Append("\t\tIIndexerSetterSetup<TValue, ").Append(typeParams).Append(">").AppendLine();
		sb.Append("\t{").AppendLine();
		sb.Append("\t\tprivate readonly List<Callback<Action<int, ").Append(typeParams)
			.Append(", TValue>>> _getterCallbacks = [];")
			.AppendLine();
		sb.Append("\t\tprivate readonly List<Callback<Action<int, ").Append(typeParams)
			.Append(", TValue>>> _setterCallbacks = [];")
			.AppendLine();
		sb.Append("\t\tprivate readonly List<Callback<Func<int, ").Append(typeParams)
			.Append(", TValue, TValue>>> _returnCallbacks = [];")
			.AppendLine();
		sb.Append("\t\tprivate bool? _callBaseClass;").AppendLine();
		sb.Append("\t\tprivate Callback? _currentCallback;").AppendLine();
		sb.Append("\t\tprivate Callback? _currentReturnCallback;").AppendLine();
		sb.Append("\t\tprivate int _currentGetterCallbacksIndex;").AppendLine();
		sb.Append("\t\tprivate int _currentReturnCallbackIndex;").AppendLine();
		sb.Append("\t\tprivate int _currentSetterCallbacksIndex;").AppendLine();
		sb.Append("\t\tprivate Func<").Append(typeParams).Append(", TValue>? _initialization;").AppendLine();
		sb.AppendLine();

		sb.Append("\t\t/// <inheritdoc cref=\"IIndexerSetup{TValue, ").Append(typeParams)
			.Append("}.CallingBaseClass(bool)\" />").AppendLine();
		sb.Append("\t\tpublic IIndexerSetup<TValue, ").Append(typeParams)
			.Append("> CallingBaseClass(bool callBaseClass = true)")
			.AppendLine();
		sb.Append("\t\t{").AppendLine();
		sb.Append("\t\t\t_callBaseClass = callBaseClass;").AppendLine();
		sb.Append("\t\t\treturn this;").AppendLine();
		sb.Append("\t\t}").AppendLine();
		sb.AppendLine();

		sb.Append("\t\t/// <inheritdoc cref=\"IIndexerSetup{TValue, ").Append(typeParams)
			.Append("}.InitializeWith(TValue)\" />").AppendLine();
		sb.Append("\t\tpublic IIndexerSetup<TValue, ").Append(typeParams).Append("> InitializeWith(TValue value)")
			.AppendLine();
		sb.Append("\t\t{").AppendLine();
		sb.Append("\t\t\tif (_initialization is not null)").AppendLine();
		sb.Append("\t\t\t{").AppendLine();
		sb.Append(
				"\t\t\t\tthrow new MockException(\"The indexer is already initialized. You cannot initialize it twice.\");")
			.AppendLine();
		sb.Append("\t\t\t}").AppendLine();
		sb.AppendLine();
		sb.Append("\t\t\t_initialization = (").Append(discards).Append(") => value;").AppendLine();
		sb.Append("\t\t\treturn this;").AppendLine();
		sb.Append("\t\t}").AppendLine();
		sb.AppendLine();

		sb.Append("\t\t/// <inheritdoc cref=\"IIndexerSetup{TValue, ").Append(typeParams)
			.Append("}.InitializeWith(Func{").Append(typeParams).Append(", TValue})\" />").AppendLine();
		sb.Append("\t\tpublic IIndexerSetup<TValue, ").Append(typeParams).Append("> InitializeWith(Func<")
			.Append(typeParams).Append(", TValue> valueGenerator)").AppendLine();
		sb.Append("\t\t{").AppendLine();
		sb.Append("\t\t\tif (_initialization is not null)").AppendLine();
		sb.Append("\t\t\t{").AppendLine();
		sb.Append(
				"\t\t\t\tthrow new MockException(\"The indexer is already initialized. You cannot initialize it twice.\");")
			.AppendLine();
		sb.Append("\t\t\t}").AppendLine();
		sb.AppendLine();
		sb.Append("\t\t\t_initialization = valueGenerator;").AppendLine();
		sb.Append("\t\t\treturn this;").AppendLine();
		sb.Append("\t\t}").AppendLine();
		sb.AppendLine();

		sb.Append("\t\t/// <inheritdoc cref=\"IIndexerSetup{TValue, ").Append(typeParams).Append("}.OnGet\" />")
			.AppendLine();
		sb.Append("\t\tpublic IIndexerGetterSetup<TValue, ").Append(typeParams)
			.Append("> OnGet").AppendLine();
		sb.Append("\t\t\t=> this;").AppendLine();
		sb.AppendLine();

		sb.Append("\t\t/// <inheritdoc cref=\"IIndexerGetterSetup{TValue, ").Append(typeParams)
			.Append("}.Do(Action)\" />").AppendLine();
		sb.Append("\t\tIIndexerSetupCallbackBuilder<TValue, ").Append(typeParams)
			.Append("> IIndexerGetterSetup<TValue, ").Append(typeParams)
			.Append(">.Do(Action callback)").AppendLine();
		sb.Append("\t\t{").AppendLine();
		sb.Append("\t\t\tCallback<Action<int, ").Append(typeParams).Append(", TValue>>? currentCallback = new((_, ")
			.Append(discards).Append(", _) => callback());").AppendLine();
		sb.Append("\t\t\t_currentCallback = currentCallback;").AppendLine();
		sb.Append("\t\t\t_getterCallbacks.Add(currentCallback);").AppendLine();
		sb.Append("\t\t\treturn this;").AppendLine();
		sb.Append("\t\t}").AppendLine();
		sb.AppendLine();

		sb.Append("\t\t/// <inheritdoc cref=\"IIndexerGetterSetup{TValue, ").Append(typeParams).Append("}.Do(Action{")
			.Append(typeParams).Append("})\" />").AppendLine();
		sb.Append("\t\tIIndexerSetupCallbackBuilder<TValue, ").Append(typeParams)
			.Append("> IIndexerGetterSetup<TValue, ").Append(typeParams)
			.Append(">.Do(Action<")
			.Append(typeParams)
			.Append("> callback)").AppendLine();
		sb.Append("\t\t{").AppendLine();
		sb.Append("\t\t\tCallback<Action<int, ").Append(typeParams).Append(", TValue>>? currentCallback = new((_, ")
			.Append(parameters).Append(", _) => callback(").Append(parameters).Append("));").AppendLine();
		sb.Append("\t\t\t_currentCallback = currentCallback;").AppendLine();
		sb.Append("\t\t\t_getterCallbacks.Add(currentCallback);").AppendLine();
		sb.Append("\t\t\treturn this;").AppendLine();
		sb.Append("\t\t}").AppendLine();
		sb.AppendLine();

		sb.Append("\t\t/// <inheritdoc cref=\"IIndexerGetterSetup{TValue, ").Append(typeParams).Append("}.Do(Action{")
			.Append(typeParams).Append(", TValue})\" />").AppendLine();
		sb.Append("\t\tIIndexerSetupCallbackBuilder<TValue, ").Append(typeParams)
			.Append("> IIndexerGetterSetup<TValue, ").Append(typeParams)
			.Append(">.Do(Action<")
			.Append(typeParams)
			.Append(", TValue> callback)").AppendLine();
		sb.Append("\t\t{").AppendLine();
		sb.Append("\t\t\tCallback<Action<int, ").Append(typeParams).Append(", TValue>>? currentCallback = new((_, ")
			.Append(parameters).Append(", v) => callback(").Append(parameters).Append(", v));").AppendLine();
		sb.Append("\t\t\t_currentCallback = currentCallback;").AppendLine();
		sb.Append("\t\t\t_getterCallbacks.Add(currentCallback);").AppendLine();
		sb.Append("\t\t\treturn this;").AppendLine();
		sb.Append("\t\t}").AppendLine();
		sb.AppendLine();

		sb.Append("\t\t/// <inheritdoc cref=\"IIndexerGetterSetup{TValue, ").Append(typeParams)
			.Append("}.Do(Action{int, ").Append(typeParams).Append(", TValue})\" />").AppendLine();
		sb.Append("\t\tIIndexerSetupCallbackBuilder<TValue, ").Append(typeParams)
			.Append("> IIndexerGetterSetup<TValue, ").Append(typeParams)
			.Append(">.Do(Action<int, ")
			.Append(typeParams)
			.Append(", TValue> callback)").AppendLine();
		sb.Append("\t\t{").AppendLine();
		sb.Append("\t\t\tCallback<Action<int, ").Append(typeParams)
			.Append(", TValue>>? currentCallback = new(callback);")
			.AppendLine();
		sb.Append("\t\t\t_currentCallback = currentCallback;").AppendLine();
		sb.Append("\t\t\t_getterCallbacks.Add(currentCallback);").AppendLine();
		sb.Append("\t\t\treturn this;").AppendLine();
		sb.Append("\t\t}").AppendLine();
		sb.AppendLine();

		sb.Append("\t\t/// <inheritdoc cref=\"IIndexerSetup{TValue, ").Append(typeParams).Append("}.OnSet\" />")
			.AppendLine();
		sb.Append("\t\tpublic IIndexerSetterSetup<TValue, ").Append(typeParams)
			.Append("> OnSet").AppendLine();
		sb.Append("\t\t\t=> this;").AppendLine();
		sb.AppendLine();

		sb.Append("\t\t/// <inheritdoc cref=\"IIndexerSetterSetup{TValue, ").Append(typeParams)
			.Append("}.Do(Action)\" />").AppendLine();
		sb.Append("\t\tIIndexerSetupCallbackBuilder<TValue, ").Append(typeParams)
			.Append("> IIndexerSetterSetup<TValue, ").Append(typeParams)
			.Append(">.Do(Action callback)").AppendLine();
		sb.Append("\t\t{").AppendLine();
		sb.Append("\t\t\tCallback<Action<int, ").Append(typeParams).Append(", TValue>>? currentCallback = new((_, _, ")
			.Append(discards).Append(") => callback());").AppendLine();
		sb.Append("\t\t\t_currentCallback = currentCallback;").AppendLine();
		sb.Append("\t\t\t_setterCallbacks.Add(currentCallback);").AppendLine();
		sb.Append("\t\t\treturn this;").AppendLine();
		sb.Append("\t\t}").AppendLine();
		sb.AppendLine();

		sb.Append("\t\t/// <inheritdoc cref=\"IIndexerSetterSetup{TValue, ").Append(typeParams)
			.Append("}.Do(Action{TValue})\" />").AppendLine();
		sb.Append("\t\tIIndexerSetupCallbackBuilder<TValue, ").Append(typeParams)
			.Append("> IIndexerSetterSetup<TValue, ").Append(typeParams)
			.Append(">.Do(Action<TValue> callback)")
			.AppendLine();
		sb.Append("\t\t{").AppendLine();
		sb.Append("\t\t\tCallback<Action<int, ").Append(typeParams).Append(", TValue>>? currentCallback = new((_, ")
			.Append(discards).Append(", v) => callback(v));").AppendLine();
		sb.Append("\t\t\t_currentCallback = currentCallback;").AppendLine();
		sb.Append("\t\t\t_setterCallbacks.Add(currentCallback);").AppendLine();
		sb.Append("\t\t\treturn this;").AppendLine();
		sb.Append("\t\t}").AppendLine();
		sb.AppendLine();

		sb.Append("\t\t/// <inheritdoc cref=\"IIndexerSetterSetup{TValue, ").Append(typeParams).Append("}.Do(Action{")
			.Append(typeParams).Append(", TValue})\" />").AppendLine();
		sb.Append("\t\tIIndexerSetupCallbackBuilder<TValue, ").Append(typeParams)
			.Append("> IIndexerSetterSetup<TValue, ").Append(typeParams)
			.Append(">.Do(Action<")
			.Append(typeParams).Append(", TValue> callback)").AppendLine();
		sb.Append("\t\t{").AppendLine();
		sb.Append("\t\t\tCallback<Action<int, ").Append(typeParams).Append(", TValue>>? currentCallback = new((_, ")
			.Append(parameters).Append(", v) => callback(").Append(parameters).Append(", v));").AppendLine();
		sb.Append("\t\t\t_currentCallback = currentCallback;").AppendLine();
		sb.Append("\t\t\t_setterCallbacks.Add(currentCallback);").AppendLine();
		sb.Append("\t\t\treturn this;").AppendLine();
		sb.Append("\t\t}").AppendLine();
		sb.AppendLine();

		sb.Append("\t\t/// <inheritdoc cref=\"IIndexerSetterSetup{TValue, ").Append(typeParams)
			.Append("}.Do(Action{int, ").Append(typeParams).Append(", TValue})\" />").AppendLine();
		sb.Append("\t\tIIndexerSetupCallbackBuilder<TValue, ").Append(typeParams)
			.Append("> IIndexerSetterSetup<TValue, ").Append(typeParams)
			.Append(">.Do(Action<int, ")
			.Append(typeParams).Append(", TValue> callback)").AppendLine();
		sb.Append("\t\t{").AppendLine();
		sb.Append("\t\t\tCallback<Action<int, ").Append(typeParams)
			.Append(", TValue>>? currentCallback = new(callback);")
			.AppendLine();
		sb.Append("\t\t\t_currentCallback = currentCallback;").AppendLine();
		sb.Append("\t\t\t_setterCallbacks.Add(currentCallback);").AppendLine();
		sb.Append("\t\t\treturn this;").AppendLine();
		sb.Append("\t\t}").AppendLine();
		sb.AppendLine();

		sb.Append("\t\t/// <inheritdoc cref=\"IIndexerSetup{TValue, ").Append(typeParams)
			.Append("}.Returns(TValue)\" />").AppendLine();
		sb.Append("\t\tpublic IIndexerSetupReturnBuilder<TValue, ").Append(typeParams)
			.Append("> Returns(TValue returnValue)")
			.AppendLine();
		sb.Append("\t\t{").AppendLine();
		sb.Append("\t\t\tvar currentCallback = new Callback<Func<int, ").Append(typeParams)
			.Append(", TValue, TValue>>((_, ").Append(discards).Append(", _) => returnValue);").AppendLine();
		sb.Append("\t\t\t_currentReturnCallback = currentCallback;").AppendLine();
		sb.Append("\t\t\t_returnCallbacks.Add(currentCallback);").AppendLine();
		sb.Append("\t\t\treturn this;").AppendLine();
		sb.Append("\t\t}").AppendLine();
		sb.AppendLine();

		sb.Append("\t\t/// <inheritdoc cref=\"IIndexerSetup{TValue, ").Append(typeParams)
			.Append("}.Returns(Func{TValue})\" />").AppendLine();
		sb.Append("\t\tpublic IIndexerSetupReturnBuilder<TValue, ").Append(typeParams)
			.Append("> Returns(Func<TValue> callback)")
			.AppendLine();
		sb.Append("\t\t{").AppendLine();
		sb.Append("\t\t\tvar currentCallback = new Callback<Func<int, ").Append(typeParams)
			.Append(", TValue, TValue>>((_, ").Append(discards).Append(", _) => callback());").AppendLine();
		sb.Append("\t\t\t_currentReturnCallback = currentCallback;").AppendLine();
		sb.Append("\t\t\t_returnCallbacks.Add(currentCallback);").AppendLine();
		sb.Append("\t\t\treturn this;").AppendLine();
		sb.Append("\t\t}").AppendLine();
		sb.AppendLine();

		sb.Append("\t\t/// <inheritdoc cref=\"IIndexerSetup{TValue, ").Append(typeParams).Append("}.Returns(Func{")
			.Append(typeParams).Append(", TValue})\" />").AppendLine();
		sb.Append("\t\tpublic IIndexerSetupReturnBuilder<TValue, ").Append(typeParams).Append("> Returns(Func<")
			.Append(typeParams)
			.Append(", TValue> callback)").AppendLine();
		sb.Append("\t\t{").AppendLine();
		sb.Append("\t\t\tvar currentCallback = new Callback<Func<int, ").Append(typeParams)
			.Append(", TValue, TValue>>((_, ").Append(parameters).Append(", _) => callback(").Append(parameters)
			.Append("));")
			.AppendLine();
		sb.Append("\t\t\t_currentReturnCallback = currentCallback;").AppendLine();
		sb.Append("\t\t\t_returnCallbacks.Add(currentCallback);").AppendLine();
		sb.Append("\t\t\treturn this;").AppendLine();
		sb.Append("\t\t}").AppendLine();
		sb.AppendLine();

		sb.Append("\t\t/// <inheritdoc cref=\"IIndexerSetup{TValue, ").Append(typeParams).Append("}.Returns(Func{")
			.Append(typeParams).Append(", TValue, TValue})\" />").AppendLine();
		sb.Append("\t\tpublic IIndexerSetupReturnBuilder<TValue, ").Append(typeParams).Append("> Returns(Func<")
			.Append(typeParams).Append(", TValue, TValue> callback)").AppendLine();
		sb.Append("\t\t{").AppendLine();
		sb.Append("\t\t\tvar currentCallback = new Callback<Func<int, ").Append(typeParams)
			.Append(", TValue, TValue>>((_, v, ").Append(parameters).Append(") => callback(v, ").Append(parameters)
			.Append("));").AppendLine();
		sb.Append("\t\t\t_currentReturnCallback = currentCallback;").AppendLine();
		sb.Append("\t\t\t_returnCallbacks.Add(currentCallback);").AppendLine();
		sb.Append("\t\t\treturn this;").AppendLine();
		sb.Append("\t\t}").AppendLine();
		sb.AppendLine();

		sb.Append("\t\t/// <inheritdoc cref=\"IIndexerSetup{TValue, ").Append(typeParams)
			.Append("}.Throws{TException}()\" />").AppendLine();
		sb.Append("\t\tpublic IIndexerSetupReturnBuilder<TValue, ").Append(typeParams).Append("> Throws<TException>()")
			.AppendLine();
		sb.Append("\t\t\twhere TException : Exception, new()").AppendLine();
		sb.Append("\t\t{").AppendLine();
		sb.Append("\t\t\tvar currentCallback = new Callback<Func<int, ").Append(typeParams)
			.Append(", TValue, TValue>>((_, ").Append(discards).Append(", _) => throw new TException());").AppendLine();
		sb.Append("\t\t\t_currentReturnCallback = currentCallback;").AppendLine();
		sb.Append("\t\t\t_returnCallbacks.Add(currentCallback);").AppendLine();
		sb.Append("\t\t\treturn this;").AppendLine();
		sb.Append("\t\t}").AppendLine();
		sb.AppendLine();

		sb.Append("\t\t/// <inheritdoc cref=\"IIndexerSetup{TValue, ").Append(typeParams)
			.Append("}.Throws(Exception)\" />").AppendLine();
		sb.Append("\t\tpublic IIndexerSetupReturnBuilder<TValue, ").Append(typeParams)
			.Append("> Throws(Exception exception)")
			.AppendLine();
		sb.Append("\t\t{").AppendLine();
		sb.Append("\t\t\tvar currentCallback = new Callback<Func<int, ").Append(typeParams)
			.Append(", TValue, TValue>>((_, ").Append(discards).Append(", _) => throw exception);").AppendLine();
		sb.Append("\t\t\t_currentReturnCallback = currentCallback;").AppendLine();
		sb.Append("\t\t\t_returnCallbacks.Add(currentCallback);").AppendLine();
		sb.Append("\t\t\treturn this;").AppendLine();
		sb.Append("\t\t}").AppendLine();
		sb.AppendLine();

		sb.Append("\t\t/// <inheritdoc cref=\"IIndexerSetup{TValue, ").Append(typeParams)
			.Append("}.Throws(Func{Exception})\" />").AppendLine();
		sb.Append("\t\tpublic IIndexerSetupReturnBuilder<TValue, ").Append(typeParams)
			.Append("> Throws(Func<Exception> callback)")
			.AppendLine();
		sb.Append("\t\t{").AppendLine();
		sb.Append("\t\t\tvar currentCallback = new Callback<Func<int, ").Append(typeParams)
			.Append(", TValue, TValue>>((_, ").Append(discards).Append(", _) => throw callback());").AppendLine();
		sb.Append("\t\t\t_currentReturnCallback = currentCallback;").AppendLine();
		sb.Append("\t\t\t_returnCallbacks.Add(currentCallback);").AppendLine();
		sb.Append("\t\t\treturn this;").AppendLine();
		sb.Append("\t\t}").AppendLine();
		sb.AppendLine();

		sb.Append("\t\t/// <inheritdoc cref=\"IIndexerSetup{TValue, ").Append(typeParams).Append("}.Throws(Func{")
			.Append(typeParams).Append(", Exception})\" />").AppendLine();
		sb.Append("\t\tpublic IIndexerSetupReturnBuilder<TValue, ").Append(typeParams).Append("> Throws(Func<")
			.Append(typeParams)
			.Append(", Exception> callback)").AppendLine();
		sb.Append("\t\t{").AppendLine();
		sb.Append("\t\t\tvar currentCallback = new Callback<Func<int, ").Append(typeParams)
			.Append(", TValue, TValue>>((_, ").Append(parameters).Append(", _) => throw callback(").Append(parameters)
			.Append("));").AppendLine();
		sb.Append("\t\t\t_currentReturnCallback = currentCallback;").AppendLine();
		sb.Append("\t\t\t_returnCallbacks.Add(currentCallback);").AppendLine();
		sb.Append("\t\t\treturn this;").AppendLine();
		sb.Append("\t\t}").AppendLine();
		sb.AppendLine();

		sb.Append("\t\t/// <inheritdoc cref=\"IIndexerSetup{TValue, ").Append(typeParams).Append("}.Throws(Func{")
			.Append(typeParams).Append(", TValue, Exception})\" />").AppendLine();
		sb.Append("\t\tpublic IIndexerSetupReturnBuilder<TValue, ").Append(typeParams).Append("> Throws(Func<")
			.Append(typeParams).Append(", TValue, Exception> callback)").AppendLine();
		sb.Append("\t\t{").AppendLine();
		sb.Append("\t\t\tvar currentCallback = new Callback<Func<int, ").Append(typeParams)
			.Append(", TValue, TValue>>((_, ").Append(parameters).Append(", v) => throw callback(").Append(parameters)
			.Append(", v));").AppendLine();
		sb.Append("\t\t\t_currentReturnCallback = currentCallback;").AppendLine();
		sb.Append("\t\t\t_returnCallbacks.Add(currentCallback);").AppendLine();
		sb.Append("\t\t\treturn this;").AppendLine();
		sb.Append("\t\t}").AppendLine();
		sb.AppendLine();

		sb.Append("\t\t/// <inheritdoc cref=\"IIndexerSetupCallbackBuilder{TValue, ").Append(typeParams)
			.Append("}.When(Func{int, bool})\" />").AppendLine();
		sb.Append("\t\tIIndexerSetupCallbackWhenBuilder<TValue, ").Append(typeParams)
			.Append("> IIndexerSetupCallbackBuilder<TValue, ").Append(typeParams)
			.Append(">.When(Func<int, bool> predicate)").AppendLine();
		sb.Append("\t\t{").AppendLine();
		sb.Append("\t\t\t_currentCallback?.When(predicate);").AppendLine();
		sb.Append("\t\t\treturn this;").AppendLine();
		sb.Append("\t\t}").AppendLine();
		sb.AppendLine();

		sb.Append("\t\t/// <inheritdoc cref=\"IIndexerSetupCallbackBuilder{TValue, ").Append(typeParams)
			.Append("}.InParallel()\" />").AppendLine();
		sb.Append("\t\tIIndexerSetupCallbackBuilder<TValue, ").Append(typeParams)
			.Append("> IIndexerSetupCallbackBuilder<TValue, ").Append(typeParams)
			.Append(">.InParallel()").AppendLine();
		sb.Append("\t\t{").AppendLine();
		sb.Append("\t\t\t_currentCallback?.InParallel();").AppendLine();
		sb.Append("\t\t\treturn this;").AppendLine();
		sb.Append("\t\t}").AppendLine();
		sb.AppendLine();

		sb.Append("\t\t/// <inheritdoc cref=\"IIndexerSetupCallbackWhenBuilder{TValue, ").Append(typeParams)
			.Append("}.For(int)\" />").AppendLine();
		sb.Append("\t\tIIndexerSetupCallbackWhenBuilder<TValue, ").Append(typeParams)
			.Append("> IIndexerSetupCallbackWhenBuilder<TValue, ")
			.Append(typeParams)
			.Append(">.For(int times)").AppendLine();
		sb.Append("\t\t{").AppendLine();
		sb.Append("\t\t\t_currentCallback?.For(times);").AppendLine();
		sb.Append("\t\t\treturn this;").AppendLine();
		sb.Append("\t\t}").AppendLine();
		sb.AppendLine();

		sb.Append("\t\t/// <inheritdoc cref=\"IIndexerSetupCallbackWhenBuilder{TValue, ").Append(typeParams)
			.Append("}.Only(int)\" />").AppendLine();
		sb.Append("\t\tIIndexerSetup<TValue, ").Append(typeParams).Append("> IIndexerSetupCallbackWhenBuilder<TValue, ")
			.Append(typeParams)
			.Append(">.Only(int times)").AppendLine();
		sb.Append("\t\t{").AppendLine();
		sb.Append("\t\t\t_currentCallback?.Only(times);").AppendLine();
		sb.Append("\t\t\treturn this;").AppendLine();
		sb.Append("\t\t}").AppendLine();
		sb.AppendLine();

		sb.Append("\t\t/// <inheritdoc cref=\"IIndexerSetupReturnBuilder{TValue, ").Append(typeParams)
			.Append("}.When(Func{int, bool})\" />").AppendLine();
		sb.Append("\t\tIIndexerSetupReturnWhenBuilder<TValue, ").Append(typeParams)
			.Append("> IIndexerSetupReturnBuilder<TValue, ").Append(typeParams)
			.Append(">.When(Func<int, bool> predicate)").AppendLine();
		sb.Append("\t\t{").AppendLine();
		sb.Append("\t\t\t_currentReturnCallback?.When(predicate);").AppendLine();
		sb.Append("\t\t\treturn this;").AppendLine();
		sb.Append("\t\t}").AppendLine();
		sb.AppendLine();

		sb.Append("\t\t/// <inheritdoc cref=\"IIndexerSetupReturnWhenBuilder{TValue, ").Append(typeParams)
			.Append("}.For(int)\" />").AppendLine();
		sb.Append("\t\tIIndexerSetupReturnWhenBuilder<TValue, ").Append(typeParams)
			.Append("> IIndexerSetupReturnWhenBuilder<TValue, ")
			.Append(typeParams)
			.Append(">.For(int times)").AppendLine();
		sb.Append("\t\t{").AppendLine();
		sb.Append("\t\t\t_currentReturnCallback?.For(times);").AppendLine();
		sb.Append("\t\t\treturn this;").AppendLine();
		sb.Append("\t\t}").AppendLine();
		sb.AppendLine();

		sb.Append("\t\t/// <inheritdoc cref=\"IIndexerSetupReturnWhenBuilder{TValue, ").Append(typeParams)
			.Append("}.Only(int)\" />").AppendLine();
		sb.Append("\t\tIIndexerSetup<TValue, ").Append(typeParams).Append("> IIndexerSetupReturnWhenBuilder<TValue, ")
			.Append(typeParams)
			.Append(">.Only(int times)").AppendLine();
		sb.Append("\t\t{").AppendLine();
		sb.Append("\t\t\t_currentReturnCallback?.Only(times);").AppendLine();
		sb.Append("\t\t\treturn this;").AppendLine();
		sb.Append("\t\t}").AppendLine();
		sb.AppendLine();

		sb.Append(
				"\t\t/// <inheritdoc cref=\"ExecuteGetterCallback{TValue}(IndexerGetterAccess, TValue, MockBehavior)\" />")
			.AppendLine();
		sb.Append(
				"\t\tprotected override T ExecuteGetterCallback<T>(IndexerGetterAccess indexerGetterAccess, T value, MockBehavior behavior)")
			.AppendLine();
		sb.Append("\t\t{").AppendLine();
		sb.Append("\t\t\tif (TryCast(value, out TValue resultValue, behavior) &&").AppendLine();
		sb.Append("\t\t\t    indexerGetterAccess.Parameters.Length == ").Append(numberOfParameters);
		for (int i = 1; i <= numberOfParameters; i++)
		{
			sb.Append(" &&").AppendLine();
			sb.Append("\t\t\t    TryCast(indexerGetterAccess.Parameters[").Append(i - 1).Append("], out T").Append(i)
				.Append(" p")
				.Append(i).Append(", behavior)");
		}

		sb.Append(")").AppendLine();
		sb.Append("\t\t\t{").AppendLine();
		sb.Append("\t\t\t\tbool wasInvoked = false;").AppendLine();
		sb.Append("\t\t\t\tint currentGetterCallbacksIndex = _currentGetterCallbacksIndex;").AppendLine();
		sb.Append("\t\t\t\tfor (int i = 0; i < _getterCallbacks.Count; i++)").AppendLine();
		sb.Append("\t\t\t\t{").AppendLine();
		sb.Append("\t\t\t\t\tCallback<Action<int, ").Append(typeParams).Append(", TValue>> getterCallback =")
			.AppendLine();
		sb.Append("\t\t\t\t\t\t_getterCallbacks[(currentGetterCallbacksIndex + i) % _getterCallbacks.Count];")
			.AppendLine();
		sb.Append(
				"\t\t\t\t\tif (getterCallback.Invoke(wasInvoked, ref _currentGetterCallbacksIndex, (invocationCount, @delegate)")
			.AppendLine();
		sb.Append("\t\t\t\t\t\t=> @delegate(invocationCount, ").Append(parameters).Append(", resultValue)))")
			.AppendLine();
		sb.Append("\t\t\t\t\t{").AppendLine();
		sb.Append("\t\t\t\t\t\twasInvoked = true;").AppendLine();
		sb.Append("\t\t\t\t\t}").AppendLine();
		sb.Append("\t\t\t\t}").AppendLine();
		sb.AppendLine();
		sb.Append("\t\t\t\tforeach (var _ in _returnCallbacks)").AppendLine();
		sb.Append("\t\t\t\t{").AppendLine();
		sb.Append("\t\t\t\t\tCallback<Func<int, ").Append(typeParams)
			.Append(
				", TValue, TValue>> returnCallback = _returnCallbacks[_currentReturnCallbackIndex % _returnCallbacks.Count];")
			.AppendLine();
		sb.Append(
				"\t\t\t\t\tif (returnCallback.Invoke<TValue>(ref _currentReturnCallbackIndex, (invocationCount, @delegate)")
			.AppendLine();
		sb.Append("\t\t\t\t\t\t\t=> @delegate(invocationCount, ").Append(parameters)
			.Append(", resultValue), out TValue? newValue) &&").AppendLine();
		sb.Append("\t\t\t\t\t\tTryCast(newValue, out T returnValue, behavior))").AppendLine();
		sb.Append("\t\t\t\t\t{").AppendLine();
		sb.Append("\t\t\t\t\t\treturn returnValue;").AppendLine();
		sb.Append("\t\t\t\t\t}").AppendLine();
		sb.Append("\t\t\t\t}").AppendLine();
		sb.Append("\t\t\t}").AppendLine();
		sb.AppendLine();
		sb.Append("\t\t\treturn value;").AppendLine();
		sb.Append("\t\t}").AppendLine();
		sb.AppendLine();

		sb.Append("\t\t/// <inheritdoc cref=\"IndexerSetup.GetCallBaseClass()\" />").AppendLine();
		sb.Append("\t\tprotected override bool? GetCallBaseClass()").AppendLine();
		sb.Append("\t\t\t=> _callBaseClass;").AppendLine();
		sb.AppendLine();

		sb.Append("\t\t/// <inheritdoc cref=\"IndexerSetup.HasReturnCalls()\" />").AppendLine();
		sb.Append("\t\tprotected override bool HasReturnCalls()").AppendLine();
		sb.Append("\t\t\t=> _returnCallbacks.Count > 0;").AppendLine();
		sb.AppendLine();

		sb.Append(
				"\t\t/// <inheritdoc cref=\"ExecuteSetterCallback{TValue}(IndexerSetterAccess, TValue, MockBehavior)\" />")
			.AppendLine();
		sb.Append(
				"\t\tprotected override void ExecuteSetterCallback<T>(IndexerSetterAccess indexerSetterAccess, T value, MockBehavior behavior)")
			.AppendLine();
		sb.Append("\t\t{").AppendLine();
		sb.Append("\t\t\tif (TryCast(value, out TValue resultValue, behavior) &&").AppendLine();
		sb.Append("\t\t\t    indexerSetterAccess.Parameters.Length == ").Append(numberOfParameters);
		for (int i = 1; i <= numberOfParameters; i++)
		{
			sb.Append(" &&").AppendLine();
			sb.Append("\t\t\t    TryCast(indexerSetterAccess.Parameters[").Append(i - 1).Append("], out T").Append(i)
				.Append(" p")
				.Append(i).Append(", behavior)");
		}

		sb.Append(")").AppendLine();
		sb.Append("\t\t\t{").AppendLine();
		sb.Append("\t\t\t\tbool wasInvoked = false;").AppendLine();
		sb.Append("\t\t\t\tint currentSetterCallbacksIndex = _currentSetterCallbacksIndex;").AppendLine();
		sb.Append("\t\t\t\tfor (int i = 0; i < _setterCallbacks.Count; i++)").AppendLine();
		sb.Append("\t\t\t\t{").AppendLine();
		sb.Append("\t\t\t\t\tCallback<Action<int, ").Append(typeParams).Append(", TValue>> setterCallback =")
			.AppendLine();
		sb.Append("\t\t\t\t\t\t_setterCallbacks[(currentSetterCallbacksIndex + i) % _setterCallbacks.Count];")
			.AppendLine();
		sb.Append(
				"\t\t\t\t\tif (setterCallback.Invoke(wasInvoked, ref _currentSetterCallbacksIndex, (invocationCount, @delegate)")
			.AppendLine();
		sb.Append("\t\t\t\t\t\t=> @delegate(invocationCount, ").Append(parameters).Append(", resultValue)))")
			.AppendLine();
		sb.Append("\t\t\t\t\t{").AppendLine();
		sb.Append("\t\t\t\t\t\twasInvoked = true;").AppendLine();
		sb.Append("\t\t\t\t\t}").AppendLine();
		sb.Append("\t\t\t\t}").AppendLine();
		sb.Append("\t\t\t}").AppendLine();
		sb.Append("\t\t}").AppendLine();
		sb.AppendLine();

		sb.Append("\t\t/// <inheritdoc cref=\"IsMatch(object?[])\" />").AppendLine();
		sb.Append("\t\tprotected override bool IsMatch(object?[] parameters)").AppendLine();
		sb.Append("\t\t\t=> Matches([")
			.Append(string.Join(", ",
				Enumerable.Range(1, numberOfParameters).Select(i => $"(IParameter)match{i}")))
			.Append("], parameters);").AppendLine();
		sb.AppendLine();

		sb.Append(
				"\t\t/// <inheritdoc cref=\"IndexerSetup.TryGetInitialValue{T}(MockBehavior, Func{T}, object?[], out T)\" />")
			.AppendLine();
		sb.Append(
				"\t\tprotected override bool TryGetInitialValue<T>(MockBehavior behavior, Func<T> defaultValueGenerator, object?[] parameters, [NotNullWhen(true)] out T value)")
			.AppendLine();
		sb.Append("\t\t{").AppendLine();
		sb.Append("\t\t\tif (_initialization is not null &&").AppendLine();
		sb.Append("\t\t\t    parameters.Length == ").Append(numberOfParameters).Append(" &&").AppendLine();
		for (int i = 1; i <= numberOfParameters; i++)
		{
			sb.Append("\t\t\t    TryCast(parameters[").Append(i - 1).Append("], out T").Append(i).Append(" p")
				.Append(i).Append(", behavior)").Append(" &&").AppendLine();
		}

		sb.Append("\t\t\t    _initialization.Invoke(").Append(parameters).Append(") is T initialValue)").AppendLine();
		sb.Append("\t\t\t{").AppendLine();
		sb.Append("\t\t\t\tvalue = initialValue;").AppendLine();
		sb.Append("\t\t\t\treturn true;").AppendLine();
		sb.Append("\t\t\t}").AppendLine();
		sb.AppendLine();
		sb.Append("\t\t\tvalue = defaultValueGenerator();").AppendLine();
		sb.Append("\t\t\treturn false;").AppendLine();
		sb.Append("\t\t}").AppendLine();
		sb.AppendLine();

		sb.Append("\t}").AppendLine();
	}
}
