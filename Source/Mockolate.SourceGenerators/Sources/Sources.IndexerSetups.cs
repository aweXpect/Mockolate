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
			string types = string.Join(", ", Enumerable.Range(1, item).Select(i => $"T{i}"));
			sb.Append($$"""
			            		/// <summary>
			            		///     Extensions for indexer setups with {{item}} parameters.
			            		/// </summary>
			            		extension<TValue, {{types}}>(Mockolate.Setup.IIndexerSetupReturnBuilder<TValue, {{types}}> setup)
			            		{
			            			/// <summary>
			            			///     Returns/throws forever.
			            			/// </summary>
			            			public void Forever()
			            			{
			            				setup.For(int.MaxValue);
			            			}
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
		string typeParams = string.Join(", ", Enumerable.Range(1, numberOfParameters).Select(i => $"T{i}"));
		string outTypeParams = string.Join(", ", Enumerable.Range(1, numberOfParameters).Select(i => $"out T{i}"));
		string parameters = string.Join(", ", Enumerable.Range(1, numberOfParameters).Select(i => $"p{i}"));
		string discards = string.Join(", ", Enumerable.Range(1, numberOfParameters).Select(_ => "_"));

		sb.Append("\t/// <summary>").AppendLine();
		sb.Append("\t///     Sets up a <typeparamref name=\"TValue\"/> indexer for ");
		for (int i = 1; i < numberOfParameters - 1; i++)
		{
			sb.Append("<typeparamref name=\"T").Append(i).Append("\" />, ");
		}

		sb.Append("<typeparamref name=\"T").Append(numberOfParameters - 1).Append("\" /> and <typeparamref name=\"T")
			.Append(numberOfParameters).Append("\" />.").AppendLine();
		sb.Append("\t/// </summary>").AppendLine();
		sb.Append("\tinternal interface IIndexerSetup<TValue, ").Append(outTypeParams).Append(">").AppendLine();
		sb.Append("\t{").AppendLine();
		sb.Append("\t\t/// <summary>").AppendLine();
		sb.Append(
				"\t\t///     Flag indicating if the base class implementation should be called, and its return values used as default values.")
			.AppendLine();
		sb.Append("\t\t/// </summary>").AppendLine();
		sb.Append("\t\t/// <remarks>").AppendLine();
		sb.Append("\t\t///     If not specified, use <see cref=\"MockBehavior.CallBaseClass\" />.").AppendLine();
		sb.Append("\t\t/// </remarks>").AppendLine();
		sb.Append("\t\tIIndexerSetup<TValue, ").Append(typeParams)
			.Append("> CallingBaseClass(bool callBaseClass = true);").AppendLine();
		sb.AppendLine();

		sb.Append("\t\t/// <summary>").AppendLine();
		sb.Append("\t\t///     Initializes the indexer with the given <paramref name=\"value\" />.").AppendLine();
		sb.Append("\t\t/// </summary>").AppendLine();
		sb.Append("\t\tIIndexerSetup<TValue, ").Append(typeParams).Append("> InitializeWith(TValue value);")
			.AppendLine();
		sb.AppendLine();

		sb.Append("\t\t/// <summary>").AppendLine();
		sb.Append("\t\t///     Initializes the according to the given <paramref name=\"valueGenerator\" />.")
			.AppendLine();
		sb.Append("\t\t/// </summary>").AppendLine();
		sb.Append("\t\tIIndexerSetup<TValue, ").Append(typeParams).Append("> InitializeWith(Func<")
			.Append(typeParams).Append(", TValue> valueGenerator);").AppendLine();
		sb.AppendLine();

		sb.Append("\t\t/// <summary>").AppendLine();
		sb.Append(
				"\t\t///     Registers a <paramref name=\"callback\"/> to be invoked whenever the indexer's getter is accessed.")
			.AppendLine();
		sb.Append("\t\t/// </summary>").AppendLine();
		sb.Append("\t\tIIndexerSetupCallbackBuilder<TValue, ").Append(typeParams).Append("> OnGet(Action callback);")
			.AppendLine();
		sb.AppendLine();

		sb.Append("\t\t/// <summary>").AppendLine();
		sb.Append(
				"\t\t///     Registers a <paramref name=\"callback\"/> to be invoked whenever the indexer's getter is accessed.")
			.AppendLine();
		sb.Append("\t\t/// </summary>").AppendLine();
		sb.Append("\t\tIIndexerSetupCallbackBuilder<TValue, ").Append(typeParams).Append("> OnGet(Action<")
			.Append(typeParams)
			.Append("> callback);").AppendLine();
		sb.AppendLine();

		sb.Append("\t\t/// <summary>").AppendLine();
		sb.Append(
				"\t\t///     Registers a <paramref name=\"callback\"/> to be invoked whenever the indexer's getter is accessed.")
			.AppendLine();
		sb.Append("\t\t/// </summary>").AppendLine();
		sb.Append("\t\tIIndexerSetupCallbackBuilder<TValue, ").Append(typeParams).Append("> OnGet(Action<int, ")
			.Append(typeParams)
			.Append("> callback);").AppendLine();
		sb.AppendLine();

		sb.Append("\t\t/// <summary>").AppendLine();
		sb.Append(
				"\t\t///     Registers a <paramref name=\"callback\"/> to be invoked whenever the indexer's setter is accessed.")
			.AppendLine();
		sb.Append("\t\t/// </summary>").AppendLine();
		sb.Append("\t\tIIndexerSetupCallbackBuilder<TValue, ").Append(typeParams).Append("> OnSet(Action callback);")
			.AppendLine();
		sb.AppendLine();

		sb.Append("\t\t/// <summary>").AppendLine();
		sb.Append(
				"\t\t///     Registers a <paramref name=\"callback\"/> to be invoked whenever the indexer's setter is accessed.")
			.AppendLine();
		sb.Append("\t\t/// </summary>").AppendLine();
		sb.Append("\t\tIIndexerSetupCallbackBuilder<TValue, ").Append(typeParams)
			.Append("> OnSet(Action<TValue> callback);").AppendLine();
		sb.AppendLine();

		sb.Append("\t\t/// <summary>").AppendLine();
		sb.Append(
				"\t\t///     Registers a <paramref name=\"callback\"/> to be invoked whenever the indexer's setter is accessed.")
			.AppendLine();
		sb.Append("\t\t/// </summary>").AppendLine();
		sb.Append("\t\tIIndexerSetupCallbackBuilder<TValue, ").Append(typeParams).Append("> OnSet(Action<TValue, ")
			.Append(typeParams).Append("> callback);").AppendLine();
		sb.AppendLine();

		sb.Append("\t\t/// <summary>").AppendLine();
		sb.Append(
				"\t\t///     Registers a <paramref name=\"callback\"/> to be invoked whenever the indexer's setter is accessed.")
			.AppendLine();
		sb.Append("\t\t/// </summary>").AppendLine();
		sb.Append("\t\tIIndexerSetupCallbackBuilder<TValue, ").Append(typeParams).Append("> OnSet(Action<int, TValue, ")
			.Append(typeParams).Append("> callback);").AppendLine();
		sb.AppendLine();

		sb.Append("\t\t/// <summary>").AppendLine();
		sb.Append("\t\t///     Registers a <paramref name=\"callback\" /> to setup the return value for this indexer.")
			.AppendLine();
		sb.Append("\t\t/// </summary>").AppendLine();
		sb.Append("\t\tIIndexerSetupReturnBuilder<TValue, ").Append(typeParams).Append("> Returns(Func<TValue, ")
			.Append(typeParams).Append(", TValue> callback);").AppendLine();
		sb.AppendLine();

		sb.Append("\t\t/// <summary>").AppendLine();
		sb.Append("\t\t///     Registers a <paramref name=\"callback\" /> to setup the return value for this indexer.")
			.AppendLine();
		sb.Append("\t\t/// </summary>").AppendLine();
		sb.Append("\t\tIIndexerSetupReturnBuilder<TValue, ").Append(typeParams).Append("> Returns(Func<")
			.Append(typeParams)
			.Append(", TValue> callback);").AppendLine();
		sb.AppendLine();

		sb.Append("\t\t/// <summary>").AppendLine();
		sb.Append("\t\t///     Registers a <paramref name=\"callback\" /> to setup the return value for this indexer.")
			.AppendLine();
		sb.Append("\t\t/// </summary>").AppendLine();
		sb.Append("\t\tIIndexerSetupReturnBuilder<TValue, ").Append(typeParams)
			.Append("> Returns(Func<TValue> callback);")
			.AppendLine();
		sb.AppendLine();

		sb.Append("\t\t/// <summary>").AppendLine();
		sb.Append("\t\t///     Registers the <paramref name=\"returnValue\" /> for this indexer.").AppendLine();
		sb.Append("\t\t/// </summary>").AppendLine();
		sb.Append("\t\tIIndexerSetupReturnBuilder<TValue, ").Append(typeParams).Append("> Returns(TValue returnValue);")
			.AppendLine();
		sb.AppendLine();

		sb.Append("\t\t/// <summary>").AppendLine();
		sb.Append("\t\t///     Registers an <typeparamref name=\"TException\" /> to throw when the indexer is read.")
			.AppendLine();
		sb.Append("\t\t/// </summary>").AppendLine();
		sb.Append("\t\tIIndexerSetupReturnBuilder<TValue, ").Append(typeParams).Append("> Throws<TException>()")
			.AppendLine();
		sb.Append("\t\t\twhere TException : Exception, new();").AppendLine();
		sb.AppendLine();

		sb.Append("\t\t/// <summary>").AppendLine();
		sb.Append("\t\t///     Registers an <paramref name=\"exception\" /> to throw when the indexer is read.")
			.AppendLine();
		sb.Append("\t\t/// </summary>").AppendLine();
		sb.Append("\t\tIIndexerSetupReturnBuilder<TValue, ").Append(typeParams).Append("> Throws(Exception exception);")
			.AppendLine();
		sb.AppendLine();

		sb.Append("\t\t/// <summary>").AppendLine();
		sb.Append(
				"\t\t///     Registers a <paramref name=\"callback\" /> that will calculate the exception to throw when the indexer is read.")
			.AppendLine();
		sb.Append("\t\t/// </summary>").AppendLine();
		sb.Append("\t\tIIndexerSetupReturnBuilder<TValue, ").Append(typeParams)
			.Append("> Throws(Func<Exception> callback);")
			.AppendLine();
		sb.AppendLine();

		sb.Append("\t\t/// <summary>").AppendLine();
		sb.Append(
				"\t\t///     Registers a <paramref name=\"callback\" /> that will calculate the exception to throw when the indexer is read.")
			.AppendLine();
		sb.Append("\t\t/// </summary>").AppendLine();
		sb.Append("\t\tIIndexerSetupReturnBuilder<TValue, ").Append(typeParams).Append("> Throws(Func<")
			.Append(typeParams)
			.Append(", Exception> callback);").AppendLine();
		sb.AppendLine();

		sb.Append("\t\t/// <summary>").AppendLine();
		sb.Append(
				"\t\t///     Registers a <paramref name=\"callback\" /> that will calculate the exception to throw when the indexer is read.")
			.AppendLine();
		sb.Append("\t\t/// </summary>").AppendLine();
		sb.Append("\t\tIIndexerSetupReturnBuilder<TValue, ").Append(typeParams).Append("> Throws(Func<TValue, ")
			.Append(typeParams).Append(", Exception> callback);").AppendLine();

		sb.Append("\t}").AppendLine();
		sb.AppendLine();

		sb.Append("\t/// <summary>").AppendLine();
		sb.Append("\t///     Sets up a callback for a <typeparamref name=\"TValue\"/> indexer for ");
		for (int i = 1; i < numberOfParameters - 1; i++)
		{
			sb.Append("<typeparamref name=\"T").Append(i).Append("\" />, ");
		}

		sb.Append("<typeparamref name=\"T").Append(numberOfParameters - 1).Append("\" /> and <typeparamref name=\"T")
			.Append(numberOfParameters).Append("\" />.").AppendLine();
		sb.Append("\t/// </summary>").AppendLine();
		sb.Append("\tinternal interface IIndexerSetupCallbackBuilder<TValue, ").Append(outTypeParams)
			.Append("> : IIndexerSetupCallbackWhenBuilder<TValue, ").Append(typeParams).Append(">").AppendLine();
		sb.Append("\t{").AppendLine();
		sb.Append("\t\t/// <summary>").AppendLine();
		sb.Append(
				"\t\t///     Limits the callback to only execute for property accesses where the predicate returns true.")
			.AppendLine();
		sb.Append("\t\t/// </summary>").AppendLine();
		sb.Append("\t\t/// <remarks>").AppendLine();
		sb.Append(
				"\t\t///     Provides a zero-based counter indicating how many times the property has been accessed so far.")
			.AppendLine();
		sb.Append("\t\t/// </remarks>").AppendLine();
		sb.Append("\t\tIIndexerSetupCallbackWhenBuilder<TValue, ").Append(typeParams)
			.Append("> When(Func<int, bool> predicate);").AppendLine();
		sb.Append("\t}").AppendLine();
		sb.AppendLine();

		sb.Append("\t/// <summary>").AppendLine();
		sb.Append("\t///     Sets up a when callback for a <typeparamref name=\"TValue\"/> indexer for ");
		for (int i = 1; i < numberOfParameters - 1; i++)
		{
			sb.Append("<typeparamref name=\"T").Append(i).Append("\" />, ");
		}

		sb.Append("<typeparamref name=\"T").Append(numberOfParameters - 1).Append("\" /> and <typeparamref name=\"T")
			.Append(numberOfParameters).Append("\" />.").AppendLine();
		sb.Append("\t/// </summary>").AppendLine();
		sb.Append("\tinternal interface IIndexerSetupCallbackWhenBuilder<TValue, ").Append(outTypeParams)
			.Append("> : IIndexerSetup<TValue, ").Append(typeParams).Append(">").AppendLine();
		sb.Append("\t{").AppendLine();
		sb.Append("\t\t/// <summary>").AppendLine();
		sb.Append(
				"\t\t///     Limits the callback to only execute for the given number of <paramref name=\"times\" />.")
			.AppendLine();
		sb.Append("\t\t/// </summary>").AppendLine();
		sb.Append("\t\t/// <remarks>").AppendLine();
		sb.Append(
				"\t\t///     The number of times is only counted for actual executions (<see cref=\"IIndexerSetupCallbackBuilder{TValue, ")
			.Append(typeParams).Append("}.When(Func{int, bool})\" /> evaluates to <see langword=\"true\" />).")
			.AppendLine();
		sb.Append("\t\t/// </remarks>").AppendLine();
		sb.Append("\t\tIIndexerSetup<TValue, ").Append(typeParams).Append("> For(int times);").AppendLine();
		sb.Append("\t}").AppendLine();
		sb.AppendLine();

		sb.Append("\t/// <summary>").AppendLine();
		sb.Append("\t///     Sets up a callback for a <typeparamref name=\"TValue\"/> indexer for ");
		for (int i = 1; i < numberOfParameters - 1; i++)
		{
			sb.Append("<typeparamref name=\"T").Append(i).Append("\" />, ");
		}

		sb.Append("<typeparamref name=\"T").Append(numberOfParameters - 1).Append("\" /> and <typeparamref name=\"T")
			.Append(numberOfParameters).Append("\" />.").AppendLine();
		sb.Append("\t/// </summary>").AppendLine();
		sb.Append("\tinternal interface IIndexerSetupReturnBuilder<TValue, ").Append(outTypeParams)
			.Append("> : IIndexerSetupReturnWhenBuilder<TValue, ").Append(typeParams).Append(">").AppendLine();
		sb.Append("\t{").AppendLine();
		sb.Append("\t\t/// <summary>").AppendLine();
		sb.Append(
				"\t\t///     Limits the callback to only execute for property accesses where the predicate returns true.")
			.AppendLine();
		sb.Append("\t\t/// </summary>").AppendLine();
		sb.Append("\t\t/// <remarks>").AppendLine();
		sb.Append(
				"\t\t///     Provides a zero-based counter indicating how many times the property has been accessed so far.")
			.AppendLine();
		sb.Append("\t\t/// </remarks>").AppendLine();
		sb.Append("\t\tIIndexerSetupReturnWhenBuilder<TValue, ").Append(typeParams)
			.Append("> When(Func<int, bool> predicate);").AppendLine();
		sb.Append("\t}").AppendLine();
		sb.AppendLine();

		sb.Append("\t/// <summary>").AppendLine();
		sb.Append("\t///     Sets up a when callback for a <typeparamref name=\"TValue\"/> indexer for ");
		for (int i = 1; i < numberOfParameters - 1; i++)
		{
			sb.Append("<typeparamref name=\"T").Append(i).Append("\" />, ");
		}

		sb.Append("<typeparamref name=\"T").Append(numberOfParameters - 1).Append("\" /> and <typeparamref name=\"T")
			.Append(numberOfParameters).Append("\" />.").AppendLine();
		sb.Append("\t/// </summary>").AppendLine();
		sb.Append("\tinternal interface IIndexerSetupReturnWhenBuilder<TValue, ").Append(outTypeParams)
			.Append("> : IIndexerSetup<TValue, ").Append(typeParams).Append(">").AppendLine();
		sb.Append("\t{").AppendLine();
		sb.Append("\t\t/// <summary>").AppendLine();
		sb.Append(
				"\t\t///     Limits the callback to only execute for the given number of <paramref name=\"times\" />.")
			.AppendLine();
		sb.Append("\t\t/// </summary>").AppendLine();
		sb.Append("\t\t/// <remarks>").AppendLine();
		sb.Append(
				"\t\t///     The number of times is only counted for actual executions (<see cref=\"IIndexerSetupReturnBuilder{TValue, ")
			.Append(typeParams).Append("}.When(Func{int, bool})\" /> evaluates to <see langword=\"true\" />).")
			.AppendLine();
		sb.Append("\t\t/// </remarks>").AppendLine();
		sb.Append("\t\tIIndexerSetup<TValue, ").Append(typeParams).Append("> For(int times);").AppendLine();
		sb.Append("\t}").AppendLine();
		sb.AppendLine();

		sb.Append("\t/// <summary>").AppendLine();
		sb.Append("\t///     Sets up a <typeparamref name=\"TValue\"/> indexer for ");
		for (int i = 1; i < numberOfParameters - 1; i++)
		{
			sb.Append("<typeparamref name=\"T").Append(i).Append("\" />, ");
		}

		sb.Append("<typeparamref name=\"T").Append(numberOfParameters - 1).Append("\" /> and <typeparamref name=\"T")
			.Append(numberOfParameters).Append("\" />.").AppendLine();
		sb.Append("\t/// </summary>").AppendLine();
		sb.Append("\tinternal class IndexerSetup<TValue, ").Append(typeParams).Append(">(")
			.Append(
				string.Join(", ", Enumerable.Range(1, numberOfParameters).Select(i => $"IParameter match{i}")))
			.Append(") : IndexerSetup,")
			.AppendLine();
		sb.Append("\t\tIIndexerSetupCallbackBuilder<TValue, ").Append(typeParams).Append(">,").AppendLine();
		sb.Append("\t\tIIndexerSetupReturnBuilder<TValue, ").Append(typeParams).Append(">").AppendLine();
		sb.Append("\t{").AppendLine();
		sb.Append("\t\tprivate readonly List<Callback<Action<int, ").Append(typeParams)
			.Append(">>> _getterCallbacks = [];")
			.AppendLine();
		sb.Append("\t\tprivate readonly List<Callback<Action<int, TValue, ").Append(typeParams)
			.Append(">>> _setterCallbacks = [];")
			.AppendLine();
		sb.Append("\t\tprivate readonly List<Callback<Func<int, TValue, ").Append(typeParams)
			.Append(", TValue>>> _returnCallbacks = [];")
			.AppendLine();
		sb.Append("\t\tprivate bool? _callBaseClass;").AppendLine();
		sb.Append("\t\tprivate Callback? _currentCallback;").AppendLine();
		sb.Append("\t\tprivate Callback? _currentReturnCallback;").AppendLine();
		sb.Append("\t\tprivate int _currentReturnCallbackIndex;").AppendLine();
		sb.Append("\t\tprivate Func<").Append(typeParams).Append(", TValue>? _initialization;").AppendLine();
		sb.AppendLine();

		sb.Append("\t\t/// <summary>").AppendLine();
		sb.Append(
				"\t\t///     Flag indicating if the base class implementation should be called, and its return values used as default values.")
			.AppendLine();
		sb.Append("\t\t/// </summary>").AppendLine();
		sb.Append("\t\t/// <remarks>").AppendLine();
		sb.Append("\t\t///     If not specified, use <see cref=\"MockBehavior.CallBaseClass\" />.").AppendLine();
		sb.Append("\t\t/// </remarks>").AppendLine();
		sb.Append("\t\tpublic IIndexerSetup<TValue, ").Append(typeParams)
			.Append("> CallingBaseClass(bool callBaseClass = true)")
			.AppendLine();
		sb.Append("\t\t{").AppendLine();
		sb.Append("\t\t\t_callBaseClass = callBaseClass;").AppendLine();
		sb.Append("\t\t\treturn this;").AppendLine();
		sb.Append("\t\t}").AppendLine();
		sb.AppendLine();

		sb.Append("\t\t/// <summary>").AppendLine();
		sb.Append("\t\t///     Initializes the indexer with the given <paramref name=\"value\" />.").AppendLine();
		sb.Append("\t\t/// </summary>").AppendLine();
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

		sb.Append("\t\t/// <summary>").AppendLine();
		sb.Append("\t\t///     Initializes the according to the given <paramref name=\"valueGenerator\" />.")
			.AppendLine();
		sb.Append("\t\t/// </summary>").AppendLine();
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

		sb.Append("\t\t/// <summary>").AppendLine();
		sb.Append(
				"\t\t///     Registers a <paramref name=\"callback\"/> to be invoked whenever the indexer's getter is accessed.")
			.AppendLine();
		sb.Append("\t\t/// </summary>").AppendLine();
		sb.Append("\t\tpublic IIndexerSetupCallbackBuilder<TValue, ").Append(typeParams)
			.Append("> OnGet(Action callback)").AppendLine();
		sb.Append("\t\t{").AppendLine();
		sb.Append("\t\t\tCallback<Action<int, ").Append(typeParams).Append(">>? currentCallback = new((_, ")
			.Append(discards).Append(") => callback());").AppendLine();
		sb.Append("\t\t\t_currentCallback = currentCallback;").AppendLine();
		sb.Append("\t\t\t_getterCallbacks.Add(currentCallback);").AppendLine();
		sb.Append("\t\t\treturn this;").AppendLine();
		sb.Append("\t\t}").AppendLine();
		sb.AppendLine();

		sb.Append("\t\t/// <summary>").AppendLine();
		sb.Append(
				"\t\t///     Registers a <paramref name=\"callback\"/> to be invoked whenever the indexer's getter is accessed.")
			.AppendLine();
		sb.Append("\t\t/// </summary>").AppendLine();
		sb.Append("\t\tpublic IIndexerSetupCallbackBuilder<TValue, ").Append(typeParams).Append("> OnGet(Action<")
			.Append(typeParams)
			.Append("> callback)").AppendLine();
		sb.Append("\t\t{").AppendLine();
		sb.Append("\t\t\tCallback<Action<int, ").Append(typeParams).Append(">>? currentCallback = new((_, ")
			.Append(parameters).Append(") => callback(").Append(parameters).Append("));").AppendLine();
		sb.Append("\t\t\t_currentCallback = currentCallback;").AppendLine();
		sb.Append("\t\t\t_getterCallbacks.Add(currentCallback);").AppendLine();
		sb.Append("\t\t\treturn this;").AppendLine();
		sb.Append("\t\t}").AppendLine();
		sb.AppendLine();

		sb.Append("\t\t/// <summary>").AppendLine();
		sb.Append(
				"\t\t///     Registers a <paramref name=\"callback\"/> to be invoked whenever the indexer's getter is accessed.")
			.AppendLine();
		sb.Append("\t\t/// </summary>").AppendLine();
		sb.Append("\t\tpublic IIndexerSetupCallbackBuilder<TValue, ").Append(typeParams).Append("> OnGet(Action<int, ")
			.Append(typeParams)
			.Append("> callback)").AppendLine();
		sb.Append("\t\t{").AppendLine();
		sb.Append("\t\t\tCallback<Action<int, ").Append(typeParams).Append(">>? currentCallback = new(callback);")
			.AppendLine();
		sb.Append("\t\t\t_currentCallback = currentCallback;").AppendLine();
		sb.Append("\t\t\t_getterCallbacks.Add(currentCallback);").AppendLine();
		sb.Append("\t\t\treturn this;").AppendLine();
		sb.Append("\t\t}").AppendLine();
		sb.AppendLine();

		sb.Append("\t\t/// <summary>").AppendLine();
		sb.Append(
				"\t\t///     Registers a <paramref name=\"callback\"/> to be invoked whenever the indexer's setter is accessed.")
			.AppendLine();
		sb.Append("\t\t/// </summary>").AppendLine();
		sb.Append("\t\tpublic IIndexerSetupCallbackBuilder<TValue, ").Append(typeParams)
			.Append("> OnSet(Action callback)").AppendLine();
		sb.Append("\t\t{").AppendLine();
		sb.Append("\t\t\tCallback<Action<int, TValue, ").Append(typeParams).Append(">>? currentCallback = new((_, _, ")
			.Append(discards).Append(") => callback());").AppendLine();
		sb.Append("\t\t\t_currentCallback = currentCallback;").AppendLine();
		sb.Append("\t\t\t_setterCallbacks.Add(currentCallback);").AppendLine();
		sb.Append("\t\t\treturn this;").AppendLine();
		sb.Append("\t\t}").AppendLine();
		sb.AppendLine();

		sb.Append("\t\t/// <summary>").AppendLine();
		sb.Append(
				"\t\t///     Registers a <paramref name=\"callback\"/> to be invoked whenever the indexer's setter is accessed.")
			.AppendLine();
		sb.Append("\t\t/// </summary>").AppendLine();
		sb.Append("\t\tpublic IIndexerSetupCallbackBuilder<TValue, ").Append(typeParams)
			.Append("> OnSet(Action<TValue> callback)")
			.AppendLine();
		sb.Append("\t\t{").AppendLine();
		sb.Append("\t\t\tCallback<Action<int, TValue, ").Append(typeParams).Append(">>? currentCallback = new((_, v, ")
			.Append(discards).Append(") => callback(v));").AppendLine();
		sb.Append("\t\t\t_currentCallback = currentCallback;").AppendLine();
		sb.Append("\t\t\t_setterCallbacks.Add(currentCallback);").AppendLine();
		sb.Append("\t\t\treturn this;").AppendLine();
		sb.Append("\t\t}").AppendLine();
		sb.AppendLine();

		sb.Append("\t\t/// <summary>").AppendLine();
		sb.Append(
				"\t\t///     Registers a <paramref name=\"callback\"/> to be invoked whenever the indexer's setter is accessed.")
			.AppendLine();
		sb.Append("\t\t/// </summary>").AppendLine();
		sb.Append("\t\tpublic IIndexerSetupCallbackBuilder<TValue, ").Append(typeParams)
			.Append("> OnSet(Action<TValue, ")
			.Append(typeParams).Append("> callback)").AppendLine();
		sb.Append("\t\t{").AppendLine();
		sb.Append("\t\t\tCallback<Action<int, TValue, ").Append(typeParams).Append(">>? currentCallback = new((_, v, ")
			.Append(parameters).Append(") => callback(v, ").Append(parameters).Append("));").AppendLine();
		sb.Append("\t\t\t_currentCallback = currentCallback;").AppendLine();
		sb.Append("\t\t\t_setterCallbacks.Add(currentCallback);").AppendLine();
		sb.Append("\t\t\treturn this;").AppendLine();
		sb.Append("\t\t}").AppendLine();
		sb.AppendLine();

		sb.Append("\t\t/// <summary>").AppendLine();
		sb.Append(
				"\t\t///     Registers a <paramref name=\"callback\"/> to be invoked whenever the indexer's setter is accessed.")
			.AppendLine();
		sb.Append("\t\t/// </summary>").AppendLine();
		sb.Append("\t\tpublic IIndexerSetupCallbackBuilder<TValue, ").Append(typeParams)
			.Append("> OnSet(Action<int, TValue, ")
			.Append(typeParams).Append("> callback)").AppendLine();
		sb.Append("\t\t{").AppendLine();
		sb.Append("\t\t\tCallback<Action<int, TValue, ").Append(typeParams)
			.Append(">>? currentCallback = new(callback);")
			.AppendLine();
		sb.Append("\t\t\t_currentCallback = currentCallback;").AppendLine();
		sb.Append("\t\t\t_setterCallbacks.Add(currentCallback);").AppendLine();
		sb.Append("\t\t\treturn this;").AppendLine();
		sb.Append("\t\t}").AppendLine();
		sb.AppendLine();

		sb.Append("\t\t/// <summary>").AppendLine();
		sb.Append("\t\t///     Registers a <paramref name=\"callback\" /> to setup the return value for this indexer.")
			.AppendLine();
		sb.Append("\t\t/// </summary>").AppendLine();
		sb.Append("\t\tpublic IIndexerSetupReturnBuilder<TValue, ").Append(typeParams).Append("> Returns(Func<TValue, ")
			.Append(typeParams).Append(", TValue> callback)").AppendLine();
		sb.Append("\t\t{").AppendLine();
		sb.Append("\t\t\tvar currentCallback = new Callback<Func<int, TValue, ").Append(typeParams)
			.Append(", TValue>>((_, v, ").Append(parameters).Append(") => callback(v, ").Append(parameters)
			.Append("));").AppendLine();
		sb.Append("\t\t\t_currentReturnCallback = currentCallback;").AppendLine();
		sb.Append("\t\t\t_returnCallbacks.Add(currentCallback);").AppendLine();
		sb.Append("\t\t\treturn this;").AppendLine();
		sb.Append("\t\t}").AppendLine();
		sb.AppendLine();

		sb.Append("\t\t/// <summary>").AppendLine();
		sb.Append("\t\t///     Registers a <paramref name=\"callback\" /> to setup the return value for this indexer.")
			.AppendLine();
		sb.Append("\t\t/// </summary>").AppendLine();
		sb.Append("\t\tpublic IIndexerSetupReturnBuilder<TValue, ").Append(typeParams).Append("> Returns(Func<")
			.Append(typeParams)
			.Append(", TValue> callback)").AppendLine();
		sb.Append("\t\t{").AppendLine();
		sb.Append("\t\t\tvar currentCallback = new Callback<Func<int, TValue, ").Append(typeParams)
			.Append(", TValue>>((_, _, ").Append(parameters).Append(") => callback(").Append(parameters).Append("));")
			.AppendLine();
		sb.Append("\t\t\t_currentReturnCallback = currentCallback;").AppendLine();
		sb.Append("\t\t\t_returnCallbacks.Add(currentCallback);").AppendLine();
		sb.Append("\t\t\treturn this;").AppendLine();
		sb.Append("\t\t}").AppendLine();
		sb.AppendLine();

		sb.Append("\t\t/// <summary>").AppendLine();
		sb.Append("\t\t///     Registers a <paramref name=\"callback\" /> to setup the return value for this indexer.")
			.AppendLine();
		sb.Append("\t\t/// </summary>").AppendLine();
		sb.Append("\t\tpublic IIndexerSetupReturnBuilder<TValue, ").Append(typeParams)
			.Append("> Returns(Func<TValue> callback)")
			.AppendLine();
		sb.Append("\t\t{").AppendLine();
		sb.Append("\t\t\tvar currentCallback = new Callback<Func<int, TValue, ").Append(typeParams)
			.Append(", TValue>>((_, _, ").Append(discards).Append(") => callback());").AppendLine();
		sb.Append("\t\t\t_currentReturnCallback = currentCallback;").AppendLine();
		sb.Append("\t\t\t_returnCallbacks.Add(currentCallback);").AppendLine();
		sb.Append("\t\t\treturn this;").AppendLine();
		sb.Append("\t\t}").AppendLine();
		sb.AppendLine();

		sb.Append("\t\t/// <summary>").AppendLine();
		sb.Append("\t\t///     Registers the <paramref name=\"returnValue\" /> for this indexer.").AppendLine();
		sb.Append("\t\t/// </summary>").AppendLine();
		sb.Append("\t\tpublic IIndexerSetupReturnBuilder<TValue, ").Append(typeParams)
			.Append("> Returns(TValue returnValue)")
			.AppendLine();
		sb.Append("\t\t{").AppendLine();
		sb.Append("\t\t\tvar currentCallback = new Callback<Func<int, TValue, ").Append(typeParams)
			.Append(", TValue>>((_, _, ").Append(discards).Append(") => returnValue);").AppendLine();
		sb.Append("\t\t\t_currentReturnCallback = currentCallback;").AppendLine();
		sb.Append("\t\t\t_returnCallbacks.Add(currentCallback);").AppendLine();
		sb.Append("\t\t\treturn this;").AppendLine();
		sb.Append("\t\t}").AppendLine();
		sb.AppendLine();

		sb.Append("\t\t/// <summary>").AppendLine();
		sb.Append("\t\t///     Registers an <typeparamref name=\"TException\" /> to throw when the indexer is read.")
			.AppendLine();
		sb.Append("\t\t/// </summary>").AppendLine();
		sb.Append("\t\tpublic IIndexerSetupReturnBuilder<TValue, ").Append(typeParams).Append("> Throws<TException>()")
			.AppendLine();
		sb.Append("\t\t\twhere TException : Exception, new()").AppendLine();
		sb.Append("\t\t{").AppendLine();
		sb.Append("\t\t\tvar currentCallback = new Callback<Func<int, TValue, ").Append(typeParams)
			.Append(", TValue>>((_, _, ").Append(discards).Append(") => throw new TException());").AppendLine();
		sb.Append("\t\t\t_currentReturnCallback = currentCallback;").AppendLine();
		sb.Append("\t\t\t_returnCallbacks.Add(currentCallback);").AppendLine();
		sb.Append("\t\t\treturn this;").AppendLine();
		sb.Append("\t\t}").AppendLine();
		sb.AppendLine();

		sb.Append("\t\t/// <summary>").AppendLine();
		sb.Append("\t\t///     Registers an <paramref name=\"exception\" /> to throw when the indexer is read.")
			.AppendLine();
		sb.Append("\t\t/// </summary>").AppendLine();
		sb.Append("\t\tpublic IIndexerSetupReturnBuilder<TValue, ").Append(typeParams)
			.Append("> Throws(Exception exception)")
			.AppendLine();
		sb.Append("\t\t{").AppendLine();
		sb.Append("\t\t\tvar currentCallback = new Callback<Func<int, TValue, ").Append(typeParams)
			.Append(", TValue>>((_, _, ").Append(discards).Append(") => throw exception);").AppendLine();
		sb.Append("\t\t\t_currentReturnCallback = currentCallback;").AppendLine();
		sb.Append("\t\t\t_returnCallbacks.Add(currentCallback);").AppendLine();
		sb.Append("\t\t\treturn this;").AppendLine();
		sb.Append("\t\t}").AppendLine();
		sb.AppendLine();

		sb.Append("\t\t/// <summary>").AppendLine();
		sb.Append(
				"\t\t///     Registers a <paramref name=\"callback\" /> that will calculate the exception to throw when the indexer is read.")
			.AppendLine();
		sb.Append("\t\t/// </summary>").AppendLine();
		sb.Append("\t\tpublic IIndexerSetupReturnBuilder<TValue, ").Append(typeParams)
			.Append("> Throws(Func<Exception> callback)")
			.AppendLine();
		sb.Append("\t\t{").AppendLine();
		sb.Append("\t\t\tvar currentCallback = new Callback<Func<int, TValue, ").Append(typeParams)
			.Append(", TValue>>((_, _, ").Append(discards).Append(") => throw callback());").AppendLine();
		sb.Append("\t\t\t_currentReturnCallback = currentCallback;").AppendLine();
		sb.Append("\t\t\t_returnCallbacks.Add(currentCallback);").AppendLine();
		sb.Append("\t\t\treturn this;").AppendLine();
		sb.Append("\t\t}").AppendLine();
		sb.AppendLine();

		sb.Append("\t\t/// <summary>").AppendLine();
		sb.Append(
				"\t\t///     Registers a <paramref name=\"callback\" /> that will calculate the exception to throw when the indexer is read.")
			.AppendLine();
		sb.Append("\t\t/// </summary>").AppendLine();
		sb.Append("\t\tpublic IIndexerSetupReturnBuilder<TValue, ").Append(typeParams).Append("> Throws(Func<")
			.Append(typeParams)
			.Append(", Exception> callback)").AppendLine();
		sb.Append("\t\t{").AppendLine();
		sb.Append("\t\t\tvar currentCallback = new Callback<Func<int, TValue, ").Append(typeParams)
			.Append(", TValue>>((_, _, ").Append(parameters).Append(") => throw callback(").Append(parameters)
			.Append("));").AppendLine();
		sb.Append("\t\t\t_currentReturnCallback = currentCallback;").AppendLine();
		sb.Append("\t\t\t_returnCallbacks.Add(currentCallback);").AppendLine();
		sb.Append("\t\t\treturn this;").AppendLine();
		sb.Append("\t\t}").AppendLine();
		sb.AppendLine();

		sb.Append("\t\t/// <summary>").AppendLine();
		sb.Append(
				"\t\t///     Registers a <paramref name=\"callback\" /> that will calculate the exception to throw when the indexer is read.")
			.AppendLine();
		sb.Append("\t\t/// </summary>").AppendLine();
		sb.Append("\t\tpublic IIndexerSetupReturnBuilder<TValue, ").Append(typeParams).Append("> Throws(Func<TValue, ")
			.Append(typeParams).Append(", Exception> callback)").AppendLine();
		sb.Append("\t\t{").AppendLine();
		sb.Append("\t\t\tvar currentCallback = new Callback<Func<int, TValue, ").Append(typeParams)
			.Append(", TValue>>((_, v, ").Append(parameters).Append(") => throw callback(v, ").Append(parameters)
			.Append("));").AppendLine();
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

		sb.Append("\t\t/// <inheritdoc cref=\"IIndexerSetupCallbackWhenBuilder{TValue, ").Append(typeParams)
			.Append("}.For(int)\" />").AppendLine();
		sb.Append("\t\tIIndexerSetup<TValue, ").Append(typeParams).Append("> IIndexerSetupCallbackWhenBuilder<TValue, ")
			.Append(typeParams)
			.Append(">.For(int times)").AppendLine();
		sb.Append("\t\t{").AppendLine();
		sb.Append("\t\t\t_currentCallback?.For(x => x < times);").AppendLine();
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
		sb.Append("\t\tIIndexerSetup<TValue, ").Append(typeParams).Append("> IIndexerSetupReturnWhenBuilder<TValue, ")
			.Append(typeParams)
			.Append(">.For(int times)").AppendLine();
		sb.Append("\t\t{").AppendLine();
		sb.Append("\t\t\t_currentReturnCallback?.For(x => x < times);").AppendLine();
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
		sb.Append("\t\t\t\t_getterCallbacks.ForEach(callback => callback.Invoke((invocationCount, @delegate)")
			.AppendLine();
		sb.Append("\t\t\t\t\t=> @delegate(invocationCount, ").Append(parameters).Append(")));")
			.AppendLine();
		sb.Append("\t\t\t\tforeach (var _ in _returnCallbacks)").AppendLine();
		sb.Append("\t\t\t\t{").AppendLine();
		sb.Append("\t\t\t\t\tCallback<Func<int, TValue, ").Append(typeParams)
			.Append(
				", TValue>> returnCallback = _returnCallbacks[_currentReturnCallbackIndex % _returnCallbacks.Count];")
			.AppendLine();
		sb.Append(
				"\t\t\t\t\tif (returnCallback.Invoke<TValue>(ref _currentReturnCallbackIndex, (invocationCount, @delegate)")
			.AppendLine();
		sb.Append("\t\t\t\t\t\t\t=> @delegate(invocationCount, resultValue, ").Append(parameters)
			.Append("), out TValue? newValue) &&").AppendLine();
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
		sb.Append("\t\t\t\t_setterCallbacks.ForEach(callback => callback.Invoke((invocationCount, @delegate)")
			.AppendLine();
		sb.Append("\t\t\t\t\t=> @delegate(invocationCount, resultValue, ").Append(parameters).Append(")));")
			.AppendLine();
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
