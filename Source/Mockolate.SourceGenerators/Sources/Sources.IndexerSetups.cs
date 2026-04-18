using System.Text;

namespace Mockolate.SourceGenerators.Sources;

internal static partial class Sources
{
	public static string IndexerSetups(HashSet<int> indexerSetups)
	{
		StringBuilder sb = InitializeBuilder();

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
		          """);
#if !DEBUG
		sb.Append("[global::System.Diagnostics.DebuggerNonUserCode]").AppendLine();
#endif
		sb.AppendLine("""
		              	[global::System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
		              	internal static class IndexerSetupExtensions
		              	{
		              """).AppendLine();
		foreach (int item in indexerSetups)
		{
			string types = GetGenericTypeParameters(item);
			sb.Append($$"""
			            		/// <summary>
			            		///     Extensions for indexer getter callback setups with {{item}} parameters.
			            		/// </summary>
			            		extension<TValue, {{types}}>(Mockolate.Setup.IIndexerGetterSetupCallbackWhenBuilder<TValue, {{types}}> setup)
			            		{
			            			/// <summary>
			            			///     Executes the callback only once.
			            			/// </summary>
			            			public global::Mockolate.Setup.IIndexerSetup<TValue, {{types}}> OnlyOnce()
			            				=> setup.Only(1);
			            		}

			            		/// <summary>
			            		///     Extensions for indexer setter callback setups with {{item}} parameters.
			            		/// </summary>
			            		extension<TValue, {{types}}>(Mockolate.Setup.IIndexerSetterSetupCallbackWhenBuilder<TValue, {{types}}> setup)
			            		{
			            			/// <summary>
			            			///     Executes the callback only once.
			            			/// </summary>
			            			public global::Mockolate.Setup.IIndexerSetup<TValue, {{types}}> OnlyOnce()
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
			            			public global::Mockolate.Setup.IIndexerSetup<TValue, {{types}}> OnlyOnce()
			            				=> setup.Only(1);
			            		}
			            """).AppendLine();
		}

		sb.Append("""
		          	}
		          }
		          """).AppendLine();
		sb.Append("namespace Mockolate.Interactions").AppendLine();
		sb.Append("{").AppendLine();
		foreach (int count in indexerSetups)
		{
			AppendIndexerGetterAccess(sb, count);
			AppendIndexerSetterAccess(sb, count);
		}

		sb.Append("}").AppendLine();
		sb.AppendLine();

		sb.AppendLine("#nullable disable");
		return sb.ToString();
	}

	private static void AppendIndexerGetterAccess(StringBuilder sb, int numberOfParameters)
	{
		string typeParams = GetGenericTypeParameters(numberOfParameters);
		sb.AppendXmlSummary(
			$"An access of an indexer getter with {numberOfParameters} typed parameters.", "\t");
		sb.Append("\t[global::System.Diagnostics.DebuggerDisplay(\"{ToString()}\")]").AppendLine();
#if !DEBUG
		sb.Append("\t[global::System.Diagnostics.DebuggerNonUserCode]").AppendLine();
#endif
		sb.Append("\tpublic class IndexerGetterAccess<").Append(typeParams).Append(">(")
			.Append(string.Join(", ",
				Enumerable.Range(1, numberOfParameters).Select(x => $"string parameterName{x}, T{x} parameter{x}")))
			.Append(")").AppendLine();
		sb.Append("\t\t: global::Mockolate.Interactions.IndexerAccess").AppendLine();
		sb.Append("\t{").AppendLine();
		for (int i = 1; i <= numberOfParameters; i++)
		{
			sb.AppendXmlSummary($"The name of parameter {i}.");
			sb.Append("\t\tpublic string ParameterName").Append(i).Append(" { get; } = parameterName").Append(i)
				.Append(";").AppendLine();
			sb.AppendXmlSummary($"The value of parameter {i}.");
			sb.Append("\t\tpublic T").Append(i).Append(" Parameter").Append(i).Append(" { get; } = parameter")
				.Append(i).Append(";").AppendLine();
		}

		AppendParameterHooks(sb, numberOfParameters);

		sb.Append("\t\t/// <inheritdoc cref=\"object.ToString()\" />").AppendLine();
		sb.Append("\t\tpublic override string ToString()").AppendLine();
		sb.Append("\t\t\t=> $\"get indexer [").Append(string.Join(", ",
			Enumerable.Range(1, numberOfParameters).Select(i => $"{{Parameter{i}?.ToString() ?? \"null\"}}"))).Append("]\";").AppendLine();
		sb.Append("\t}").AppendLine();
	}

	private static void AppendIndexerSetterAccess(StringBuilder sb, int numberOfParameters)
	{
		string typeParams = GetGenericTypeParameters(numberOfParameters);
		sb.AppendXmlSummary(
			$"An access of an indexer setter with {numberOfParameters} typed parameters.", "\t");
		sb.Append("\t[global::System.Diagnostics.DebuggerDisplay(\"{ToString()}\")]").AppendLine();
#if !DEBUG
		sb.Append("\t[global::System.Diagnostics.DebuggerNonUserCode]").AppendLine();
#endif
		sb.Append("\tpublic class IndexerSetterAccess<").Append(typeParams).Append(", TValue>(")
			.Append(string.Join(", ",
				Enumerable.Range(1, numberOfParameters).Select(x => $"string parameterName{x}, T{x} parameter{x}")))
			.Append(", TValue value)").AppendLine();
		sb.Append("\t\t: global::Mockolate.Interactions.IndexerAccess").AppendLine();
		sb.Append("\t{").AppendLine();
		for (int i = 1; i <= numberOfParameters; i++)
		{
			sb.AppendXmlSummary($"The name of parameter {i}.");
			sb.Append("\t\tpublic string ParameterName").Append(i).Append(" { get; } = parameterName").Append(i)
				.Append(";").AppendLine();
			sb.AppendXmlSummary($"The value of parameter {i}.");
			sb.Append("\t\tpublic T").Append(i).Append(" Parameter").Append(i).Append(" { get; } = parameter")
				.Append(i).Append(";").AppendLine();
		}

		sb.AppendXmlSummary("The typed value the indexer was being set to.");
		sb.Append("\t\tpublic TValue TypedValue { get; } = value;").AppendLine();

		AppendParameterHooks(sb, numberOfParameters);

		sb.Append("\t\t/// <inheritdoc cref=\"object.ToString()\" />").AppendLine();
		sb.Append("\t\tpublic override string ToString()").AppendLine();
		sb.Append("\t\t\t=> $\"set indexer [").Append(string.Join(", ",
			Enumerable.Range(1, numberOfParameters).Select(i => $"{{Parameter{i}?.ToString() ?? \"null\"}}"))).Append("] to {TypedValue?.ToString() ?? \"null\"}\";").AppendLine();
		sb.Append("\t}").AppendLine();
	}

	private static void AppendParameterHooks(StringBuilder sb, int numberOfParameters)
	{
		sb.Append("\t\t/// <inheritdoc cref=\"global::Mockolate.Interactions.IndexerAccess.ParameterCount\" />").AppendLine();
		sb.Append("\t\tpublic override int ParameterCount => ").Append(numberOfParameters).Append(";").AppendLine();

		sb.Append("\t\t/// <inheritdoc cref=\"global::Mockolate.Interactions.IndexerAccess.GetParameterValueAt(int)\" />").AppendLine();
		sb.Append("\t\tpublic override object? GetParameterValueAt(int index)").AppendLine();
		sb.Append("\t\t\t=> index switch").AppendLine();
		sb.Append("\t\t\t{").AppendLine();
		for (int i = 1; i <= numberOfParameters; i++)
		{
			sb.Append("\t\t\t\t").Append(i - 1).Append(" => Parameter").Append(i).Append(",").AppendLine();
		}

		sb.Append("\t\t\t\t_ => null,").AppendLine();
		sb.Append("\t\t\t};").AppendLine();
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
		sb.Append("\t\tIIndexerGetterSetupCallbackBuilder<TValue, ").Append(typeParams).Append("> Do(global::System.Action callback);")
			.AppendLine();
		sb.AppendLine();

		sb.AppendXmlSummary("Registers a <paramref name=\"callback\"/> to be invoked whenever the indexer's getter is accessed.");
		sb.AppendXmlRemarks("The callback receives the parameters of the indexer.");
		sb.Append("\t\tIIndexerGetterSetupCallbackBuilder<TValue, ").Append(typeParams).Append("> Do(global::System.Action<")
			.Append(typeParams)
			.Append("> callback);").AppendLine();
		sb.AppendLine();

		sb.AppendXmlSummary("Registers a <paramref name=\"callback\"/> to be invoked whenever the indexer's getter is accessed.");
		sb.AppendXmlRemarks("The callback receives the parameters of the indexer and the value of the indexer as last parameter.");
		sb.Append("\t\tIIndexerGetterSetupCallbackBuilder<TValue, ").Append(typeParams).Append("> Do(global::System.Action<")
			.Append(typeParams)
			.Append(", TValue> callback);").AppendLine();
		sb.AppendLine();

		sb.AppendXmlSummary("Registers a <paramref name=\"callback\"/> to be invoked whenever the indexer's getter is accessed.");
		sb.AppendXmlRemarks("The callback receives an incrementing access counter as first parameter, the parameters of the indexer and the value of the indexer as last parameter.");
		sb.Append("\t\tIIndexerGetterSetupCallbackBuilder<TValue, ").Append(typeParams).Append("> Do(global::System.Action<int, ")
			.Append(typeParams)
			.Append(", TValue> callback);").AppendLine();
		sb.AppendLine();

		sb.AppendXmlSummary("Transitions the scenario to the given <paramref name=\"scenario\" /> whenever the indexer is read.");
		sb.Append("\t\tIIndexerGetterSetupParallelCallbackBuilder<TValue, ").Append(typeParams).Append("> TransitionTo(string scenario);")
			.AppendLine();
		sb.Append("\t}").AppendLine();
		sb.AppendLine();

		sb.AppendXmlSummary($"Sets up a <typeparamref name=\"TValue\"/> indexer setter for {GetTypeParametersDescription(numberOfParameters)}.", "\t");
		sb.Append("\tinternal interface IIndexerSetterSetup<TValue, ").Append(outTypeParams).Append(">").AppendLine();
		sb.Append("\t{").AppendLine();
		sb.AppendXmlSummary("Registers a <paramref name=\"callback\"/> to be invoked whenever the indexer's setter is accessed.");
		sb.Append("\t\tIIndexerSetterSetupCallbackBuilder<TValue, ").Append(typeParams).Append("> Do(global::System.Action callback);")
			.AppendLine();
		sb.AppendLine();

		sb.AppendXmlSummary("Registers a <paramref name=\"callback\"/> to be invoked whenever the indexer's setter is accessed.");
		sb.AppendXmlRemarks("The callback receives the value the indexer is set to as single parameter.");
		sb.Append("\t\tIIndexerSetterSetupCallbackBuilder<TValue, ").Append(typeParams)
			.Append("> Do(global::System.Action<TValue> callback);").AppendLine();
		sb.AppendLine();

		sb.AppendXmlSummary("Registers a <paramref name=\"callback\"/> to be invoked whenever the indexer's setter is accessed.");
		sb.AppendXmlRemarks("The callback receives the parameters of the indexer and the value the indexer is set to as last parameter.");
		sb.Append("\t\tIIndexerSetterSetupCallbackBuilder<TValue, ").Append(typeParams).Append("> Do(global::System.Action<")
			.Append(typeParams).Append(", TValue> callback);").AppendLine();
		sb.AppendLine();

		sb.AppendXmlSummary("Registers a <paramref name=\"callback\"/> to be invoked whenever the indexer's setter is accessed.");
		sb.AppendXmlRemarks("The callback receives an incrementing access counter as first parameter, the parameters of the indexer and the value the indexer is set to as last parameter.");
		sb.Append("\t\tIIndexerSetterSetupCallbackBuilder<TValue, ").Append(typeParams).Append("> Do(global::System.Action<int, ")
			.Append(typeParams).Append(", TValue> callback);").AppendLine();
		sb.AppendLine();

		sb.AppendXmlSummary("Transitions the scenario to the given <paramref name=\"scenario\" /> whenever the indexer is written to.");
		sb.Append("\t\tIIndexerSetterSetupParallelCallbackBuilder<TValue, ").Append(typeParams).Append("> TransitionTo(string scenario);")
			.AppendLine();
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

		sb.AppendXmlSummary("Specifies if calling the base class implementation should be skipped.");
		sb.AppendXmlRemarks("If not specified, use <see cref=\"global::Mockolate.MockBehavior.SkipBaseClass\" />.");
		sb.Append("\t\tglobal::Mockolate.Setup.IIndexerSetup<TValue, ").Append(typeParams)
			.Append("> SkippingBaseClass(bool skipBaseClass = true);").AppendLine();
		sb.AppendLine();

		sb.AppendXmlSummary("Initializes the indexer with the given <paramref name=\"value\" />.");
		sb.Append("\t\tglobal::Mockolate.Setup.IIndexerSetup<TValue, ").Append(typeParams).Append("> InitializeWith(TValue value);")
			.AppendLine();
		sb.AppendLine();

		sb.AppendXmlSummary("Initializes the indexer according to the given <paramref name=\"valueGenerator\" />.");
		sb.Append("\t\tglobal::Mockolate.Setup.IIndexerSetup<TValue, ").Append(typeParams).Append("> InitializeWith(global::System.Func<")
			.Append(typeParams).Append(", TValue> valueGenerator);").AppendLine();
		sb.AppendLine();

		sb.AppendXmlSummary("Registers the <paramref name=\"returnValue\" /> for this indexer.");
		sb.Append("\t\tIIndexerSetupReturnBuilder<TValue, ").Append(typeParams).Append("> Returns(TValue returnValue);")
			.AppendLine();
		sb.AppendLine();

		sb.AppendXmlSummary("Registers a <paramref name=\"callback\" /> to setup the return value for this indexer.");
		sb.Append("\t\tIIndexerSetupReturnBuilder<TValue, ").Append(typeParams)
			.Append("> Returns(global::System.Func<TValue> callback);")
			.AppendLine();
		sb.AppendLine();

		sb.AppendXmlSummary("Registers a <paramref name=\"callback\" /> to setup the return value for this indexer.");
		sb.AppendXmlRemarks("The callback receives the parameters of the indexer.");
		sb.Append("\t\tIIndexerSetupReturnBuilder<TValue, ").Append(typeParams).Append("> Returns(global::System.Func<")
			.Append(typeParams)
			.Append(", TValue> callback);").AppendLine();
		sb.AppendLine();

		sb.AppendXmlSummary("Registers a <paramref name=\"callback\" /> to setup the return value for this indexer.");
		sb.AppendXmlRemarks("The callback receives the parameters of the indexer and the value of the indexer as last parameter.");
		sb.Append("\t\tIIndexerSetupReturnBuilder<TValue, ").Append(typeParams).Append("> Returns(global::System.Func<")
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
			.Append("> Throws(global::System.Func<Exception> callback);")
			.AppendLine();
		sb.AppendLine();

		sb.AppendXmlSummary("Registers a <paramref name=\"callback\" /> that will calculate the exception to throw when the indexer is read.");
		sb.AppendXmlRemarks("The callback receives the parameters of the indexer.");
		sb.Append("\t\tIIndexerSetupReturnBuilder<TValue, ").Append(typeParams).Append("> Throws(global::System.Func<")
			.Append(typeParams)
			.Append(", Exception> callback);").AppendLine();
		sb.AppendLine();

		sb.AppendXmlSummary("Registers a <paramref name=\"callback\" /> that will calculate the exception to throw when the indexer is read.");
		sb.AppendXmlRemarks("The callback receives the parameters of the indexer and the value of the indexer as last parameter.");
		sb.Append("\t\tIIndexerSetupReturnBuilder<TValue, ").Append(typeParams).Append("> Throws(global::System.Func<")
			.Append(typeParams).Append(", TValue, Exception> callback);").AppendLine();

		sb.Append("\t}").AppendLine();
		sb.AppendLine();

		foreach (string side in new[] { "Getter", "Setter" })
		{
			sb.AppendXmlSummary($"Sets up a {side.ToLowerInvariant()} callback for a <typeparamref name=\"TValue\"/> indexer for {GetTypeParametersDescription(numberOfParameters)}.", "\t");
			sb.Append("\tinternal interface IIndexer").Append(side).Append("SetupCallbackBuilder<TValue, ").Append(outTypeParams)
				.Append("> : IIndexer").Append(side).Append("SetupParallelCallbackBuilder<TValue, ").Append(typeParams).Append(">").AppendLine();
			sb.Append("\t{").AppendLine();
			sb.AppendXmlSummary("Runs the callback in parallel to the other callbacks.");
			sb.Append("\t\tIIndexer").Append(side).Append("SetupParallelCallbackBuilder<TValue, ").Append(typeParams).Append("> InParallel();")
				.AppendLine();
			sb.Append("\t}").AppendLine();
			sb.AppendLine();

			sb.AppendXmlSummary($"Sets up a parallel {side.ToLowerInvariant()} callback for a <typeparamref name=\"TValue\"/> indexer for {GetTypeParametersDescription(numberOfParameters)}.", "\t");
			sb.Append("\tinternal interface IIndexer").Append(side).Append("SetupParallelCallbackBuilder<TValue, ").Append(outTypeParams)
				.Append("> : IIndexer").Append(side).Append("SetupCallbackWhenBuilder<TValue, ").Append(typeParams).Append(">").AppendLine();
			sb.Append("\t{").AppendLine();
			sb.AppendXmlSummary("Limits the callback to only execute for indexer accesses where the predicate returns true.");
			sb.AppendXmlRemarks("Provides a zero-based counter indicating how many times the indexer has been accessed so far.");
			sb.Append("\t\tIIndexer").Append(side).Append("SetupCallbackWhenBuilder<TValue, ").Append(typeParams)
				.Append("> When(global::System.Func<int, bool> predicate);").AppendLine();
			sb.Append("\t}").AppendLine();
			sb.AppendLine();

			sb.AppendXmlSummary($"Sets up a when {side.ToLowerInvariant()} callback for a <typeparamref name=\"TValue\"/> indexer for {GetTypeParametersDescription(numberOfParameters)}.", "\t");
			sb.Append("\tinternal interface IIndexer").Append(side).Append("SetupCallbackWhenBuilder<TValue, ").Append(outTypeParams)
				.Append("> : global::Mockolate.Setup.IIndexerSetup<TValue, ").Append(typeParams).Append(">").AppendLine();
			sb.Append("\t{").AppendLine();

			sb.AppendXmlSummary("Repeats the callback for the given number of <paramref name=\"times\" />.");
			sb.AppendXmlRemarks($"The number of times is only counted for actual executions (<see cref=\"IIndexer{side}SetupParallelCallbackBuilder{{TValue, {typeParams}}}.When(global::System.Func{{int, bool}})\" /> evaluates to <see langword=\"true\" />).");
			sb.Append("\t\tIIndexer").Append(side).Append("SetupCallbackWhenBuilder<TValue, ").Append(typeParams).Append("> For(int times);")
				.AppendLine();
			sb.AppendLine();
			sb.AppendXmlSummary("Deactivates the callback after the given number of <paramref name=\"times\" />.");
			sb.AppendXmlRemarks($"The number of times is only counted for actual executions (<see cref=\"IIndexer{side}SetupParallelCallbackBuilder{{TValue, {typeParams}}}.When(global::System.Func{{int, bool}})\" /> evaluates to <see langword=\"true\" />).");
			sb.Append("\t\tglobal::Mockolate.Setup.IIndexerSetup<TValue, ").Append(typeParams).Append("> Only(int times);").AppendLine();
			sb.Append("\t}").AppendLine();
			sb.AppendLine();
		}

		sb.AppendXmlSummary($"Sets up a return/throw callback for a <typeparamref name=\"TValue\"/> indexer for {GetTypeParametersDescription(numberOfParameters)}.", "\t");
		sb.Append("\tinternal interface IIndexerSetupReturnBuilder<TValue, ").Append(outTypeParams)
			.Append("> : IIndexerSetupReturnWhenBuilder<TValue, ").Append(typeParams).Append(">").AppendLine();
		sb.Append("\t{").AppendLine();
		sb.AppendXmlSummary("Limits the return/throw callback to only execute for indexer accesses where the predicate returns true.");
		sb.AppendXmlRemarks("Provides a zero-based counter indicating how many times the indexer has been accessed so far.");
		sb.Append("\t\tIIndexerSetupReturnWhenBuilder<TValue, ").Append(typeParams)
			.Append("> When(global::System.Func<int, bool> predicate);").AppendLine();
		sb.Append("\t}").AppendLine();
		sb.AppendLine();

		sb.AppendXmlSummary($"Sets up a when return/throw callback for a <typeparamref name=\"TValue\"/> indexer for {GetTypeParametersDescription(numberOfParameters)}.", "\t");
		sb.Append("\tinternal interface IIndexerSetupReturnWhenBuilder<TValue, ").Append(outTypeParams)
			.Append("> : global::Mockolate.Setup.IIndexerSetup<TValue, ").Append(typeParams).Append(">").AppendLine();
		sb.Append("\t{").AppendLine();
		sb.AppendXmlSummary("Repeats the return/throw callback for the given number of <paramref name=\"times\" />.");
		sb.AppendXmlRemarks($"The number of times is only counted for actual executions (<see cref=\"IIndexerSetupReturnBuilder{{TValue, {typeParams}}}.When(global::System.Func{{int, bool}})\" /> evaluates to <see langword=\"true\" />).");
		sb.Append("\t\tIIndexerSetupReturnWhenBuilder<TValue, ").Append(typeParams).Append("> For(int times);")
			.AppendLine();
		sb.AppendLine();
		sb.AppendXmlSummary("Deactivates the return/throw after the given number of <paramref name=\"times\" />.");
		sb.AppendXmlRemarks($"The number of times is only counted for actual executions (<see cref=\"IIndexerSetupReturnBuilder{{TValue, {typeParams}}}.When(global::System.Func{{int, bool}})\" /> evaluates to <see langword=\"true\" />).");
		sb.Append("\t\tglobal::Mockolate.Setup.IIndexerSetup<TValue, ").Append(typeParams).Append("> Only(int times);").AppendLine();
		sb.Append("\t}").AppendLine();
		sb.AppendLine();

		sb.AppendXmlSummary($"Sets up a <typeparamref name=\"TValue\"/> indexer for {GetTypeParametersDescription(numberOfParameters)}.", "\t");
#if !DEBUG
		sb.Append("\t[global::System.Diagnostics.DebuggerNonUserCode]").AppendLine();
#endif
		sb.Append("\t[global::System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]").AppendLine();
		sb.Append("\tinternal class IndexerSetup<TValue, ").Append(typeParams).Append(">(")
			.Append("global::Mockolate.MockRegistry mockRegistry")
			.Append(", ")
			.Append(
				string.Join(", ", Enumerable.Range(1, numberOfParameters).Select(i => $"global::Mockolate.Parameters.IParameterMatch<T{i}> parameter{i}")))
			.Append(") : global::Mockolate.Setup.IndexerSetup(mockRegistry),")
			.AppendLine();
		sb.Append("\t\tglobal::Mockolate.Setup.IIndexerGetterSetupCallbackBuilder<TValue, ").Append(typeParams).Append(">,").AppendLine();
		sb.Append("\t\tglobal::Mockolate.Setup.IIndexerSetterSetupCallbackBuilder<TValue, ").Append(typeParams).Append(">,").AppendLine();
		sb.Append("\t\tglobal::Mockolate.Setup.IIndexerSetupReturnBuilder<TValue, ").Append(typeParams).Append(">,").AppendLine();
		sb.Append("\t\tglobal::Mockolate.Setup.IIndexerGetterSetup<TValue, ").Append(typeParams).Append(">,").AppendLine();
		sb.Append("\t\tglobal::Mockolate.Setup.IIndexerSetterSetup<TValue, ").Append(typeParams).Append(">").AppendLine();
		sb.Append("\t{").AppendLine();
		sb.Append("\t\tprivate Callbacks<global::System.Action<int, ").Append(typeParams)
			.Append(", TValue>>? _getterCallbacks;")
			.AppendLine();
		sb.Append("\t\tprivate Callbacks<global::System.Action<int, ").Append(typeParams)
			.Append(", TValue>>? _setterCallbacks;")
			.AppendLine();
		sb.Append("\t\tprivate Callbacks<global::System.Func<int, ").Append(typeParams)
			.Append(", TValue, TValue>>? _returnCallbacks;")
			.AppendLine();
		sb.Append("\t\tprivate bool? _skipBaseClass;").AppendLine();
		sb.Append("\t\tprivate global::System.Func<").Append(typeParams).Append(", TValue>? _initialization;").AppendLine();
		sb.AppendLine();

		sb.Append("\t\t/// <inheritdoc cref=\"IIndexerSetup{TValue, ").Append(typeParams)
			.Append("}.SkippingBaseClass(bool)\" />").AppendLine();
		sb.Append("\t\tpublic global::Mockolate.Setup.IIndexerSetup<TValue, ").Append(typeParams)
			.Append("> SkippingBaseClass(bool skipBaseClass = true)")
			.AppendLine();
		sb.Append("\t\t{").AppendLine();
		sb.Append("\t\t\t_skipBaseClass = skipBaseClass;").AppendLine();
		sb.Append("\t\t\treturn this;").AppendLine();
		sb.Append("\t\t}").AppendLine();
		sb.AppendLine();

		sb.Append("\t\t/// <inheritdoc cref=\"IIndexerSetup{TValue, ").Append(typeParams)
			.Append("}.InitializeWith(TValue)\" />").AppendLine();
		sb.Append("\t\tpublic global::Mockolate.Setup.IIndexerSetup<TValue, ").Append(typeParams).Append("> InitializeWith(TValue value)")
			.AppendLine();
		sb.Append("\t\t{").AppendLine();
		sb.Append("\t\t\tif (_initialization is not null)").AppendLine();
		sb.Append("\t\t\t{").AppendLine();
		sb.Append(
				"\t\t\t\tthrow new global::Mockolate.Exceptions.MockException(\"The indexer is already initialized. You cannot initialize it twice.\");")
			.AppendLine();
		sb.Append("\t\t\t}").AppendLine();
		sb.AppendLine();
		sb.Append("\t\t\t_initialization = (").Append(discards).Append(") => value;").AppendLine();
		sb.Append("\t\t\treturn this;").AppendLine();
		sb.Append("\t\t}").AppendLine();
		sb.AppendLine();

		sb.Append("\t\t/// <inheritdoc cref=\"IIndexerSetup{TValue, ").Append(typeParams)
			.Append("}.InitializeWith(global::System.Func{").Append(typeParams).Append(", TValue})\" />").AppendLine();
		sb.Append("\t\tpublic global::Mockolate.Setup.IIndexerSetup<TValue, ").Append(typeParams).Append("> InitializeWith(global::System.Func<")
			.Append(typeParams).Append(", TValue> valueGenerator)").AppendLine();
		sb.Append("\t\t{").AppendLine();
		sb.Append("\t\t\tif (_initialization is not null)").AppendLine();
		sb.Append("\t\t\t{").AppendLine();
		sb.Append(
				"\t\t\t\tthrow new global::Mockolate.Exceptions.MockException(\"The indexer is already initialized. You cannot initialize it twice.\");")
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
			.Append("}.Do(global::System.Action)\" />").AppendLine();
		sb.Append("\t\tIIndexerGetterSetupCallbackBuilder<TValue, ").Append(typeParams)
			.Append("> IIndexerGetterSetup<TValue, ").Append(typeParams)
			.Append(">.Do(global::System.Action callback)").AppendLine();
		sb.Append("\t\t{").AppendLine();
		sb.Append("\t\t\tCallback<global::System.Action<int, ").Append(typeParams).Append(", TValue>>? currentCallback = new((_, ")
			.Append(discards).Append(", _) => callback());").AppendLine();
		sb.Append("\t\t\t_getterCallbacks = _getterCallbacks.Register(currentCallback);").AppendLine();
		sb.Append("\t\t\treturn this;").AppendLine();
		sb.Append("\t\t}").AppendLine();
		sb.AppendLine();

		sb.Append("\t\t/// <inheritdoc cref=\"IIndexerGetterSetup{TValue, ").Append(typeParams).Append("}.Do(global::System.Action{")
			.Append(typeParams).Append("})\" />").AppendLine();
		sb.Append("\t\tIIndexerGetterSetupCallbackBuilder<TValue, ").Append(typeParams)
			.Append("> IIndexerGetterSetup<TValue, ").Append(typeParams)
			.Append(">.Do(global::System.Action<")
			.Append(typeParams)
			.Append("> callback)").AppendLine();
		sb.Append("\t\t{").AppendLine();
		sb.Append("\t\t\tCallback<global::System.Action<int, ").Append(typeParams).Append(", TValue>>? currentCallback = new((_, ")
			.Append(parameters).Append(", _) => callback(").Append(parameters).Append("));").AppendLine();
		sb.Append("\t\t\t_getterCallbacks = _getterCallbacks.Register(currentCallback);").AppendLine();
		sb.Append("\t\t\treturn this;").AppendLine();
		sb.Append("\t\t}").AppendLine();
		sb.AppendLine();

		sb.Append("\t\t/// <inheritdoc cref=\"IIndexerGetterSetup{TValue, ").Append(typeParams).Append("}.Do(global::System.Action{")
			.Append(typeParams).Append(", TValue})\" />").AppendLine();
		sb.Append("\t\tIIndexerGetterSetupCallbackBuilder<TValue, ").Append(typeParams)
			.Append("> IIndexerGetterSetup<TValue, ").Append(typeParams)
			.Append(">.Do(global::System.Action<")
			.Append(typeParams)
			.Append(", TValue> callback)").AppendLine();
		sb.Append("\t\t{").AppendLine();
		sb.Append("\t\t\tCallback<global::System.Action<int, ").Append(typeParams).Append(", TValue>>? currentCallback = new((_, ")
			.Append(parameters).Append(", v) => callback(").Append(parameters).Append(", v));").AppendLine();
		sb.Append("\t\t\t_getterCallbacks = _getterCallbacks.Register(currentCallback);").AppendLine();
		sb.Append("\t\t\treturn this;").AppendLine();
		sb.Append("\t\t}").AppendLine();
		sb.AppendLine();

		sb.Append("\t\t/// <inheritdoc cref=\"IIndexerGetterSetup{TValue, ").Append(typeParams)
			.Append("}.Do(global::System.Action{int, ").Append(typeParams).Append(", TValue})\" />").AppendLine();
		sb.Append("\t\tIIndexerGetterSetupCallbackBuilder<TValue, ").Append(typeParams)
			.Append("> IIndexerGetterSetup<TValue, ").Append(typeParams)
			.Append(">.Do(global::System.Action<int, ")
			.Append(typeParams)
			.Append(", TValue> callback)").AppendLine();
		sb.Append("\t\t{").AppendLine();
		sb.Append("\t\t\tCallback<global::System.Action<int, ").Append(typeParams)
			.Append(", TValue>>? currentCallback = new(callback);")
			.AppendLine();
		sb.Append("\t\t\t_getterCallbacks = _getterCallbacks.Register(currentCallback);").AppendLine();
		sb.Append("\t\t\treturn this;").AppendLine();
		sb.Append("\t\t}").AppendLine();
		sb.AppendLine();

		sb.Append("\t\tIIndexerGetterSetupParallelCallbackBuilder<TValue, ").Append(typeParams)
			.Append("> IIndexerGetterSetup<TValue, ").Append(typeParams)
			.Append(">.TransitionTo(string scenario)").AppendLine();
		sb.Append("\t\t{").AppendLine();
		sb.Append("\t\t\tCallback<global::System.Action<int, ").Append(typeParams)
			.Append(", TValue>>? currentCallback = new((_, ")
			.Append(discards).Append(", _) => TransitionScenario(scenario));").AppendLine();
		sb.Append("\t\t\tcurrentCallback.InParallel();").AppendLine();
		sb.Append("\t\t\t_getterCallbacks = _getterCallbacks.Register(currentCallback);").AppendLine();
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
			.Append("}.Do(global::System.Action)\" />").AppendLine();
		sb.Append("\t\tIIndexerSetterSetupCallbackBuilder<TValue, ").Append(typeParams)
			.Append("> IIndexerSetterSetup<TValue, ").Append(typeParams)
			.Append(">.Do(global::System.Action callback)").AppendLine();
		sb.Append("\t\t{").AppendLine();
		sb.Append("\t\t\tCallback<global::System.Action<int, ").Append(typeParams).Append(", TValue>>? currentCallback = new((_, _, ")
			.Append(discards).Append(") => callback());").AppendLine();
		sb.Append("\t\t\t_setterCallbacks = _setterCallbacks.Register(currentCallback);").AppendLine();
		sb.Append("\t\t\treturn this;").AppendLine();
		sb.Append("\t\t}").AppendLine();
		sb.AppendLine();

		sb.Append("\t\t/// <inheritdoc cref=\"IIndexerSetterSetup{TValue, ").Append(typeParams)
			.Append("}.Do(global::System.Action{TValue})\" />").AppendLine();
		sb.Append("\t\tIIndexerSetterSetupCallbackBuilder<TValue, ").Append(typeParams)
			.Append("> IIndexerSetterSetup<TValue, ").Append(typeParams)
			.Append(">.Do(global::System.Action<TValue> callback)")
			.AppendLine();
		sb.Append("\t\t{").AppendLine();
		sb.Append("\t\t\tCallback<global::System.Action<int, ").Append(typeParams).Append(", TValue>>? currentCallback = new((_, ")
			.Append(discards).Append(", v) => callback(v));").AppendLine();
		sb.Append("\t\t\t_setterCallbacks = _setterCallbacks.Register(currentCallback);").AppendLine();
		sb.Append("\t\t\treturn this;").AppendLine();
		sb.Append("\t\t}").AppendLine();
		sb.AppendLine();

		sb.Append("\t\t/// <inheritdoc cref=\"IIndexerSetterSetup{TValue, ").Append(typeParams).Append("}.Do(global::System.Action{")
			.Append(typeParams).Append(", TValue})\" />").AppendLine();
		sb.Append("\t\tIIndexerSetterSetupCallbackBuilder<TValue, ").Append(typeParams)
			.Append("> IIndexerSetterSetup<TValue, ").Append(typeParams)
			.Append(">.Do(global::System.Action<")
			.Append(typeParams).Append(", TValue> callback)").AppendLine();
		sb.Append("\t\t{").AppendLine();
		sb.Append("\t\t\tCallback<global::System.Action<int, ").Append(typeParams).Append(", TValue>>? currentCallback = new((_, ")
			.Append(parameters).Append(", v) => callback(").Append(parameters).Append(", v));").AppendLine();
		sb.Append("\t\t\t_setterCallbacks = _setterCallbacks.Register(currentCallback);").AppendLine();
		sb.Append("\t\t\treturn this;").AppendLine();
		sb.Append("\t\t}").AppendLine();
		sb.AppendLine();

		sb.Append("\t\t/// <inheritdoc cref=\"IIndexerSetterSetup{TValue, ").Append(typeParams)
			.Append("}.Do(global::System.Action{int, ").Append(typeParams).Append(", TValue})\" />").AppendLine();
		sb.Append("\t\tIIndexerSetterSetupCallbackBuilder<TValue, ").Append(typeParams)
			.Append("> IIndexerSetterSetup<TValue, ").Append(typeParams)
			.Append(">.Do(global::System.Action<int, ")
			.Append(typeParams).Append(", TValue> callback)").AppendLine();
		sb.Append("\t\t{").AppendLine();
		sb.Append("\t\t\tCallback<global::System.Action<int, ").Append(typeParams)
			.Append(", TValue>>? currentCallback = new(callback);")
			.AppendLine();
		sb.Append("\t\t\t_setterCallbacks = _setterCallbacks.Register(currentCallback);").AppendLine();
		sb.Append("\t\t\treturn this;").AppendLine();
		sb.Append("\t\t}").AppendLine();
		sb.AppendLine();

		sb.Append("\t\tIIndexerSetterSetupParallelCallbackBuilder<TValue, ").Append(typeParams)
			.Append("> IIndexerSetterSetup<TValue, ").Append(typeParams)
			.Append(">.TransitionTo(string scenario)").AppendLine();
		sb.Append("\t\t{").AppendLine();
		sb.Append("\t\t\tCallback<global::System.Action<int, ").Append(typeParams)
			.Append(", TValue>>? currentCallback = new((_, ")
			.Append(discards).Append(", _) => TransitionScenario(scenario));").AppendLine();
		sb.Append("\t\t\tcurrentCallback.InParallel();").AppendLine();
		sb.Append("\t\t\t_setterCallbacks = _setterCallbacks.Register(currentCallback);").AppendLine();
		sb.Append("\t\t\treturn this;").AppendLine();
		sb.Append("\t\t}").AppendLine();
		sb.AppendLine();

		sb.Append("\t\t/// <inheritdoc cref=\"IIndexerSetup{TValue, ").Append(typeParams)
			.Append("}.Returns(TValue)\" />").AppendLine();
		sb.Append("\t\tpublic IIndexerSetupReturnBuilder<TValue, ").Append(typeParams)
			.Append("> Returns(TValue returnValue)")
			.AppendLine();
		sb.Append("\t\t{").AppendLine();
		sb.Append("\t\t\tvar currentCallback = new Callback<global::System.Func<int, ").Append(typeParams)
			.Append(", TValue, TValue>>((_, ").Append(discards).Append(", _) => returnValue);").AppendLine();
		sb.Append("\t\t\t_returnCallbacks = _returnCallbacks.Register(currentCallback);").AppendLine();
		sb.Append("\t\t\treturn this;").AppendLine();
		sb.Append("\t\t}").AppendLine();
		sb.AppendLine();

		sb.Append("\t\t/// <inheritdoc cref=\"IIndexerSetup{TValue, ").Append(typeParams)
			.Append("}.Returns(global::System.Func{TValue})\" />").AppendLine();
		sb.Append("\t\tpublic IIndexerSetupReturnBuilder<TValue, ").Append(typeParams)
			.Append("> Returns(global::System.Func<TValue> callback)")
			.AppendLine();
		sb.Append("\t\t{").AppendLine();
		sb.Append("\t\t\tvar currentCallback = new Callback<global::System.Func<int, ").Append(typeParams)
			.Append(", TValue, TValue>>((_, ").Append(discards).Append(", _) => callback());").AppendLine();
		sb.Append("\t\t\t_returnCallbacks = _returnCallbacks.Register(currentCallback);").AppendLine();
		sb.Append("\t\t\treturn this;").AppendLine();
		sb.Append("\t\t}").AppendLine();
		sb.AppendLine();

		sb.Append("\t\t/// <inheritdoc cref=\"IIndexerSetup{TValue, ").Append(typeParams).Append("}.Returns(global::System.Func{")
			.Append(typeParams).Append(", TValue})\" />").AppendLine();
		sb.Append("\t\tpublic IIndexerSetupReturnBuilder<TValue, ").Append(typeParams).Append("> Returns(global::System.Func<")
			.Append(typeParams)
			.Append(", TValue> callback)").AppendLine();
		sb.Append("\t\t{").AppendLine();
		sb.Append("\t\t\tvar currentCallback = new Callback<global::System.Func<int, ").Append(typeParams)
			.Append(", TValue, TValue>>((_, ").Append(parameters).Append(", _) => callback(").Append(parameters)
			.Append("));")
			.AppendLine();
		sb.Append("\t\t\t_returnCallbacks = _returnCallbacks.Register(currentCallback);").AppendLine();
		sb.Append("\t\t\treturn this;").AppendLine();
		sb.Append("\t\t}").AppendLine();
		sb.AppendLine();

		sb.Append("\t\t/// <inheritdoc cref=\"IIndexerSetup{TValue, ").Append(typeParams).Append("}.Returns(global::System.Func{")
			.Append(typeParams).Append(", TValue, TValue})\" />").AppendLine();
		sb.Append("\t\tpublic IIndexerSetupReturnBuilder<TValue, ").Append(typeParams).Append("> Returns(global::System.Func<")
			.Append(typeParams).Append(", TValue, TValue> callback)").AppendLine();
		sb.Append("\t\t{").AppendLine();
		sb.Append("\t\t\tvar currentCallback = new Callback<global::System.Func<int, ").Append(typeParams)
			.Append(", TValue, TValue>>((_, v, ").Append(parameters).Append(") => callback(v, ").Append(parameters)
			.Append("));").AppendLine();
		sb.Append("\t\t\t_returnCallbacks = _returnCallbacks.Register(currentCallback);").AppendLine();
		sb.Append("\t\t\treturn this;").AppendLine();
		sb.Append("\t\t}").AppendLine();
		sb.AppendLine();

		sb.Append("\t\t/// <inheritdoc cref=\"IIndexerSetup{TValue, ").Append(typeParams)
			.Append("}.Throws{TException}()\" />").AppendLine();
		sb.Append("\t\tpublic IIndexerSetupReturnBuilder<TValue, ").Append(typeParams).Append("> Throws<TException>()")
			.AppendLine();
		sb.Append("\t\t\twhere TException : Exception, new()").AppendLine();
		sb.Append("\t\t{").AppendLine();
		sb.Append("\t\t\tvar currentCallback = new Callback<global::System.Func<int, ").Append(typeParams)
			.Append(", TValue, TValue>>((_, ").Append(discards).Append(", _) => throw new TException());").AppendLine();
		sb.Append("\t\t\t_returnCallbacks = _returnCallbacks.Register(currentCallback);").AppendLine();
		sb.Append("\t\t\treturn this;").AppendLine();
		sb.Append("\t\t}").AppendLine();
		sb.AppendLine();

		sb.Append("\t\t/// <inheritdoc cref=\"IIndexerSetup{TValue, ").Append(typeParams)
			.Append("}.Throws(Exception)\" />").AppendLine();
		sb.Append("\t\tpublic IIndexerSetupReturnBuilder<TValue, ").Append(typeParams)
			.Append("> Throws(Exception exception)")
			.AppendLine();
		sb.Append("\t\t{").AppendLine();
		sb.Append("\t\t\tvar currentCallback = new Callback<global::System.Func<int, ").Append(typeParams)
			.Append(", TValue, TValue>>((_, ").Append(discards).Append(", _) => throw exception);").AppendLine();
		sb.Append("\t\t\t_returnCallbacks = _returnCallbacks.Register(currentCallback);").AppendLine();
		sb.Append("\t\t\treturn this;").AppendLine();
		sb.Append("\t\t}").AppendLine();
		sb.AppendLine();

		sb.Append("\t\t/// <inheritdoc cref=\"IIndexerSetup{TValue, ").Append(typeParams)
			.Append("}.Throws(global::System.Func{Exception})\" />").AppendLine();
		sb.Append("\t\tpublic IIndexerSetupReturnBuilder<TValue, ").Append(typeParams)
			.Append("> Throws(global::System.Func<Exception> callback)")
			.AppendLine();
		sb.Append("\t\t{").AppendLine();
		sb.Append("\t\t\tvar currentCallback = new Callback<global::System.Func<int, ").Append(typeParams)
			.Append(", TValue, TValue>>((_, ").Append(discards).Append(", _) => throw callback());").AppendLine();
		sb.Append("\t\t\t_returnCallbacks = _returnCallbacks.Register(currentCallback);").AppendLine();
		sb.Append("\t\t\treturn this;").AppendLine();
		sb.Append("\t\t}").AppendLine();
		sb.AppendLine();

		sb.Append("\t\t/// <inheritdoc cref=\"IIndexerSetup{TValue, ").Append(typeParams).Append("}.Throws(global::System.Func{")
			.Append(typeParams).Append(", Exception})\" />").AppendLine();
		sb.Append("\t\tpublic IIndexerSetupReturnBuilder<TValue, ").Append(typeParams).Append("> Throws(global::System.Func<")
			.Append(typeParams)
			.Append(", Exception> callback)").AppendLine();
		sb.Append("\t\t{").AppendLine();
		sb.Append("\t\t\tvar currentCallback = new Callback<global::System.Func<int, ").Append(typeParams)
			.Append(", TValue, TValue>>((_, ").Append(parameters).Append(", _) => throw callback(").Append(parameters)
			.Append("));").AppendLine();
		sb.Append("\t\t\t_returnCallbacks = _returnCallbacks.Register(currentCallback);").AppendLine();
		sb.Append("\t\t\treturn this;").AppendLine();
		sb.Append("\t\t}").AppendLine();
		sb.AppendLine();

		sb.Append("\t\t/// <inheritdoc cref=\"IIndexerSetup{TValue, ").Append(typeParams).Append("}.Throws(global::System.Func{")
			.Append(typeParams).Append(", TValue, Exception})\" />").AppendLine();
		sb.Append("\t\tpublic IIndexerSetupReturnBuilder<TValue, ").Append(typeParams).Append("> Throws(global::System.Func<")
			.Append(typeParams).Append(", TValue, Exception> callback)").AppendLine();
		sb.Append("\t\t{").AppendLine();
		sb.Append("\t\t\tvar currentCallback = new Callback<global::System.Func<int, ").Append(typeParams)
			.Append(", TValue, TValue>>((_, ").Append(parameters).Append(", v) => throw callback(").Append(parameters)
			.Append(", v));").AppendLine();
		sb.Append("\t\t\t_returnCallbacks = _returnCallbacks.Register(currentCallback);").AppendLine();
		sb.Append("\t\t\treturn this;").AppendLine();
		sb.Append("\t\t}").AppendLine();
		sb.AppendLine();

		foreach ((string side, string fieldName) in new[] { ("Getter", "_getterCallbacks"), ("Setter", "_setterCallbacks") })
		{
			sb.Append("\t\t/// <inheritdoc cref=\"IIndexer").Append(side).Append("SetupParallelCallbackBuilder{TValue, ").Append(typeParams)
				.Append("}.When(global::System.Func{int, bool})\" />").AppendLine();
			sb.Append("\t\tIIndexer").Append(side).Append("SetupCallbackWhenBuilder<TValue, ").Append(typeParams)
				.Append("> IIndexer").Append(side).Append("SetupParallelCallbackBuilder<TValue, ").Append(typeParams)
				.Append(">.When(global::System.Func<int, bool> predicate)").AppendLine();
			sb.Append("\t\t{").AppendLine();
			sb.Append("\t\t\t").Append(fieldName).Append("?.Active?.When(predicate);").AppendLine();
			sb.Append("\t\t\treturn this;").AppendLine();
			sb.Append("\t\t}").AppendLine();
			sb.AppendLine();

			sb.Append("\t\t/// <inheritdoc cref=\"IIndexer").Append(side).Append("SetupCallbackBuilder{TValue, ").Append(typeParams)
				.Append("}.InParallel()\" />").AppendLine();
			sb.Append("\t\tIIndexer").Append(side).Append("SetupParallelCallbackBuilder<TValue, ").Append(typeParams)
				.Append("> IIndexer").Append(side).Append("SetupCallbackBuilder<TValue, ").Append(typeParams)
				.Append(">.InParallel()").AppendLine();
			sb.Append("\t\t{").AppendLine();
			sb.Append("\t\t\t").Append(fieldName).Append("?.Active?.InParallel();").AppendLine();
			sb.Append("\t\t\treturn this;").AppendLine();
			sb.Append("\t\t}").AppendLine();
			sb.AppendLine();

			sb.Append("\t\t/// <inheritdoc cref=\"IIndexer").Append(side).Append("SetupCallbackWhenBuilder{TValue, ").Append(typeParams)
				.Append("}.For(int)\" />").AppendLine();
			sb.Append("\t\tIIndexer").Append(side).Append("SetupCallbackWhenBuilder<TValue, ").Append(typeParams)
				.Append("> IIndexer").Append(side).Append("SetupCallbackWhenBuilder<TValue, ")
				.Append(typeParams)
				.Append(">.For(int times)").AppendLine();
			sb.Append("\t\t{").AppendLine();
			sb.Append("\t\t\t").Append(fieldName).Append("?.Active?.For(times);").AppendLine();
			sb.Append("\t\t\treturn this;").AppendLine();
			sb.Append("\t\t}").AppendLine();
			sb.AppendLine();

			sb.Append("\t\t/// <inheritdoc cref=\"IIndexer").Append(side).Append("SetupCallbackWhenBuilder{TValue, ").Append(typeParams)
				.Append("}.Only(int)\" />").AppendLine();
			sb.Append("\t\tglobal::Mockolate.Setup.IIndexerSetup<TValue, ").Append(typeParams).Append("> IIndexer").Append(side).Append("SetupCallbackWhenBuilder<TValue, ")
				.Append(typeParams)
				.Append(">.Only(int times)").AppendLine();
			sb.Append("\t\t{").AppendLine();
			sb.Append("\t\t\t").Append(fieldName).Append("?.Active?.Only(times);").AppendLine();
			sb.Append("\t\t\treturn this;").AppendLine();
			sb.Append("\t\t}").AppendLine();
			sb.AppendLine();
		}

		sb.Append("\t\t/// <inheritdoc cref=\"IIndexerSetupReturnBuilder{TValue, ").Append(typeParams)
			.Append("}.When(global::System.Func{int, bool})\" />").AppendLine();
		sb.Append("\t\tIIndexerSetupReturnWhenBuilder<TValue, ").Append(typeParams)
			.Append("> IIndexerSetupReturnBuilder<TValue, ").Append(typeParams)
			.Append(">.When(global::System.Func<int, bool> predicate)").AppendLine();
		sb.Append("\t\t{").AppendLine();
		sb.Append("\t\t\t_returnCallbacks?.Active?.When(predicate);").AppendLine();
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
		sb.Append("\t\t\t_returnCallbacks?.Active?.For(times);").AppendLine();
		sb.Append("\t\t\treturn this;").AppendLine();
		sb.Append("\t\t}").AppendLine();
		sb.AppendLine();

		sb.Append("\t\t/// <inheritdoc cref=\"IIndexerSetupReturnWhenBuilder{TValue, ").Append(typeParams)
			.Append("}.Only(int)\" />").AppendLine();
		sb.Append("\t\tglobal::Mockolate.Setup.IIndexerSetup<TValue, ").Append(typeParams).Append("> IIndexerSetupReturnWhenBuilder<TValue, ")
			.Append(typeParams)
			.Append(">.Only(int times)").AppendLine();
		sb.Append("\t\t{").AppendLine();
		sb.Append("\t\t\t_returnCallbacks?.Active?.Only(times);").AppendLine();
		sb.Append("\t\t\treturn this;").AppendLine();
		sb.Append("\t\t}").AppendLine();
		sb.AppendLine();

		// Matches(T1..TN)
		sb.Append("\t\t/// <summary>").AppendLine();
		sb.Append("\t\t///     Check if the setup matches the specified parameter values.").AppendLine();
		sb.Append("\t\t/// </summary>").AppendLine();
		sb.Append("\t\tpublic virtual bool Matches(").Append(
			string.Join(", ", Enumerable.Range(1, numberOfParameters).Select(i => $"T{i} p{i}"))).Append(")").AppendLine();
		sb.Append("\t\t{").AppendLine();
		sb.Append("\t\t\tif (");
		sb.Append(string.Join(" || ",
			Enumerable.Range(1, numberOfParameters).Select(i => $"!parameter{i}.Matches(p{i})")));
		sb.Append(")").AppendLine();
		sb.Append("\t\t\t{").AppendLine();
		sb.Append("\t\t\t\treturn false;").AppendLine();
		sb.Append("\t\t\t}").AppendLine();
		sb.AppendLine();
		for (int i = 1; i <= numberOfParameters; i++)
		{
			sb.Append("\t\t\tparameter").Append(i).Append(".InvokeCallbacks(p").Append(i).Append(");").AppendLine();
		}

		sb.Append("\t\t\treturn true;").AppendLine();
		sb.Append("\t\t}").AppendLine();
		sb.AppendLine();

		// Matches(T1..TN, TValue)
		sb.Append("\t\t/// <summary>").AppendLine();
		sb.Append("\t\t///     Check if the setup matches the specified parameter values.").AppendLine();
		sb.Append("\t\t/// </summary>").AppendLine();
		sb.Append("\t\tpublic virtual bool Matches(").Append(
			string.Join(", ", Enumerable.Range(1, numberOfParameters).Select(i => $"T{i} p{i}"))).Append(", TValue value)").AppendLine();
		sb.Append("\t\t\t=> Matches(").Append(parameters).Append(");").AppendLine();
		sb.AppendLine();

		// MatchesAccess override
		sb.Append("\t\t/// <inheritdoc cref=\"global::Mockolate.Setup.IndexerSetup.MatchesAccess(global::Mockolate.Interactions.IndexerAccess)\" />").AppendLine();
		sb.Append("\t\tprotected override bool MatchesAccess(global::Mockolate.Interactions.IndexerAccess access)").AppendLine();
		sb.Append("\t\t{").AppendLine();
		sb.Append("\t\t\tif (access is global::Mockolate.Interactions.IndexerGetterAccess<").Append(typeParams).Append("> getter)").AppendLine();
		sb.Append("\t\t\t{").AppendLine();
		sb.Append("\t\t\t\treturn Matches(").Append(
			string.Join(", ", Enumerable.Range(1, numberOfParameters).Select(i => $"getter.Parameter{i}"))).Append(");").AppendLine();
		sb.Append("\t\t\t}").AppendLine();
		sb.AppendLine();
		sb.Append("\t\t\tif (access is global::Mockolate.Interactions.IndexerSetterAccess<").Append(typeParams).Append(", TValue> setter)").AppendLine();
		sb.Append("\t\t\t{").AppendLine();
		sb.Append("\t\t\t\treturn Matches(").Append(
			string.Join(", ", Enumerable.Range(1, numberOfParameters).Select(i => $"setter.Parameter{i}"))).Append(", setter.TypedValue);").AppendLine();
		sb.Append("\t\t\t}").AppendLine();
		sb.AppendLine();
		sb.Append("\t\t\treturn false;").AppendLine();
		sb.Append("\t\t}").AppendLine();
		sb.AppendLine();

		// SkipBaseClass
		sb.Append("\t\t/// <inheritdoc cref=\"global::Mockolate.Setup.IndexerSetup.SkipBaseClass()\" />").AppendLine();
		sb.Append("\t\tpublic override bool? SkipBaseClass()").AppendLine();
		sb.Append("\t\t\t=> _skipBaseClass;").AppendLine();
		sb.AppendLine();

		// GetResult(TResult baseValue)
		sb.Append("\t\t/// <inheritdoc cref=\"global::Mockolate.Setup.IndexerSetup.GetResult{TResult}(global::Mockolate.Interactions.IndexerAccess, global::Mockolate.MockBehavior, TResult)\" />").AppendLine();
		sb.Append("\t\tpublic override TResult GetResult<TResult>(global::Mockolate.Interactions.IndexerAccess access, global::Mockolate.MockBehavior behavior, TResult baseValue)").AppendLine();
		sb.Append("\t\t{").AppendLine();
		sb.Append("\t\t\tif (!TryExtractParameters(access");
		for (int i = 1; i <= numberOfParameters; i++)
		{
			sb.Append(", out T").Append(i).Append(" p").Append(i);
		}

		sb.Append("))").AppendLine();
		sb.Append("\t\t\t{").AppendLine();
		sb.Append("\t\t\t\treturn baseValue;").AppendLine();
		sb.Append("\t\t\t}").AppendLine();
		sb.AppendLine();
		sb.Append("\t\t\tTValue currentValue = TryCast(baseValue, out TValue casted, behavior) ? casted : default!;").AppendLine();
		sb.Append("\t\t\tcurrentValue = ExecuteGetterCallbacks(").Append(parameters).Append(", currentValue);").AppendLine();
		sb.Append("\t\t\tcurrentValue = ExecuteReturnCallbacks(").Append(parameters).Append(", currentValue, out _);").AppendLine();
		sb.Append("\t\t\taccess.StoreValue(currentValue);").AppendLine();
		sb.Append("\t\t\treturn TryCast(currentValue, out TResult result, behavior) ? result : baseValue;").AppendLine();
		sb.Append("\t\t}").AppendLine();
		sb.AppendLine();

		// GetResult(Func<TResult> defaultValueGenerator)
		sb.Append("\t\t/// <inheritdoc cref=\"global::Mockolate.Setup.IndexerSetup.GetResult{TResult}(global::Mockolate.Interactions.IndexerAccess, global::Mockolate.MockBehavior, global::System.Func{TResult})\" />").AppendLine();
		sb.Append("\t\tpublic override TResult GetResult<TResult>(global::Mockolate.Interactions.IndexerAccess access, global::Mockolate.MockBehavior behavior, global::System.Func<TResult> defaultValueGenerator)").AppendLine();
		sb.Append("\t\t{").AppendLine();
		sb.Append("\t\t\tif (!TryExtractParameters(access");
		for (int i = 1; i <= numberOfParameters; i++)
		{
			sb.Append(", out T").Append(i).Append(" p").Append(i);
		}

		sb.Append("))").AppendLine();
		sb.Append("\t\t\t{").AppendLine();
		sb.Append("\t\t\t\treturn defaultValueGenerator();").AppendLine();
		sb.Append("\t\t\t}").AppendLine();
		sb.AppendLine();
		sb.Append("\t\t\tTValue currentValue;").AppendLine();
		sb.Append("\t\t\tif (access.TryFindStoredValue(out TValue existing))").AppendLine();
		sb.Append("\t\t\t{").AppendLine();
		sb.Append("\t\t\t\tcurrentValue = existing;").AppendLine();
		sb.Append("\t\t\t}").AppendLine();
		sb.Append("\t\t\telse if (_initialization is not null)").AppendLine();
		sb.Append("\t\t\t{").AppendLine();
		sb.Append("\t\t\t\tcurrentValue = _initialization.Invoke(").Append(parameters).Append(");").AppendLine();
		sb.Append("\t\t\t}").AppendLine();
		sb.Append("\t\t\telse").AppendLine();
		sb.Append("\t\t\t{").AppendLine();
		sb.Append("\t\t\t\tcurrentValue = TryCast(defaultValueGenerator(), out TValue casted, behavior) ? casted : default!;").AppendLine();
		sb.Append("\t\t\t}").AppendLine();
		sb.AppendLine();
		sb.Append("\t\t\tcurrentValue = ExecuteGetterCallbacks(").Append(parameters).Append(", currentValue);").AppendLine();
		sb.Append("\t\t\tcurrentValue = ExecuteReturnCallbacks(").Append(parameters).Append(", currentValue, out _);").AppendLine();
		sb.Append("\t\t\taccess.StoreValue(currentValue);").AppendLine();
		sb.Append("\t\t\treturn TryCast(currentValue, out TResult result, behavior) ? result : defaultValueGenerator();").AppendLine();
		sb.Append("\t\t}").AppendLine();
		sb.AppendLine();

		// SetResult
		sb.Append("\t\t/// <inheritdoc cref=\"global::Mockolate.Setup.IndexerSetup.SetResult{TResult}(global::Mockolate.Interactions.IndexerAccess, global::Mockolate.MockBehavior, TResult)\" />").AppendLine();
		sb.Append("\t\tpublic override void SetResult<TResult>(global::Mockolate.Interactions.IndexerAccess access, global::Mockolate.MockBehavior behavior, TResult value)").AppendLine();
		sb.Append("\t\t{").AppendLine();
		sb.Append("\t\t\taccess.StoreValue(value);").AppendLine();
		sb.Append("\t\t\tif (!TryExtractParameters(access");
		for (int i = 1; i <= numberOfParameters; i++)
		{
			sb.Append(", out T").Append(i).Append(" p").Append(i);
		}

		sb.Append("))").AppendLine();
		sb.Append("\t\t\t{").AppendLine();
		sb.Append("\t\t\t\treturn;").AppendLine();
		sb.Append("\t\t\t}").AppendLine();
		sb.AppendLine();
		sb.Append("\t\t\tif (!TryCast(value, out TValue resultValue, behavior))").AppendLine();
		sb.Append("\t\t\t{").AppendLine();
		sb.Append("\t\t\t\treturn;").AppendLine();
		sb.Append("\t\t\t}").AppendLine();
		sb.AppendLine();
		sb.Append("\t\t\tif (_setterCallbacks is not null)").AppendLine();
		sb.Append("\t\t\t{").AppendLine();
		sb.Append("\t\t\t\tbool wasInvoked = false;").AppendLine();
		sb.Append("\t\t\t\tint currentSetterCallbacksIndex = _setterCallbacks.CurrentIndex;").AppendLine();
		sb.Append("\t\t\t\tfor (int i = 0; i < _setterCallbacks.Count; i++)").AppendLine();
		sb.Append("\t\t\t\t{").AppendLine();
		sb.Append("\t\t\t\t\tCallback<global::System.Action<int, ").Append(typeParams).Append(", TValue>> setterCallback =").AppendLine();
		sb.Append("\t\t\t\t\t\t_setterCallbacks[(currentSetterCallbacksIndex + i) % _setterCallbacks.Count];").AppendLine();
		sb.Append("\t\t\t\t\tif (setterCallback.Invoke(wasInvoked, ref _setterCallbacks.CurrentIndex, (invocationCount, @delegate)").AppendLine();
		sb.Append("\t\t\t\t\t\t=> @delegate(invocationCount, ").Append(parameters).Append(", resultValue)))").AppendLine();
		sb.Append("\t\t\t\t\t{").AppendLine();
		sb.Append("\t\t\t\t\t\twasInvoked = true;").AppendLine();
		sb.Append("\t\t\t\t\t}").AppendLine();
		sb.Append("\t\t\t\t}").AppendLine();
		sb.Append("\t\t\t}").AppendLine();
		sb.Append("\t\t}").AppendLine();
		sb.AppendLine();

		// ExecuteGetterCallbacks (private)
		sb.Append("\t\tprivate TValue ExecuteGetterCallbacks(").Append(
			string.Join(", ", Enumerable.Range(1, numberOfParameters).Select(i => $"T{i} p{i}"))).Append(", TValue currentValue)").AppendLine();
		sb.Append("\t\t{").AppendLine();
		sb.Append("\t\t\tif (_getterCallbacks is not null)").AppendLine();
		sb.Append("\t\t\t{").AppendLine();
		sb.Append("\t\t\t\tbool wasInvoked = false;").AppendLine();
		sb.Append("\t\t\t\tint currentGetterCallbacksIndex = _getterCallbacks.CurrentIndex;").AppendLine();
		sb.Append("\t\t\t\tfor (int i = 0; i < _getterCallbacks.Count; i++)").AppendLine();
		sb.Append("\t\t\t\t{").AppendLine();
		sb.Append("\t\t\t\t\tCallback<global::System.Action<int, ").Append(typeParams).Append(", TValue>> getterCallback =").AppendLine();
		sb.Append("\t\t\t\t\t\t_getterCallbacks[(currentGetterCallbacksIndex + i) % _getterCallbacks.Count];").AppendLine();
		sb.Append("\t\t\t\t\tif (getterCallback.Invoke(wasInvoked, ref _getterCallbacks.CurrentIndex, (invocationCount, @delegate)").AppendLine();
		sb.Append("\t\t\t\t\t\t=> @delegate(invocationCount, ").Append(parameters).Append(", currentValue)))").AppendLine();
		sb.Append("\t\t\t\t\t{").AppendLine();
		sb.Append("\t\t\t\t\t\twasInvoked = true;").AppendLine();
		sb.Append("\t\t\t\t\t}").AppendLine();
		sb.Append("\t\t\t\t}").AppendLine();
		sb.Append("\t\t\t}").AppendLine();
		sb.AppendLine();
		sb.Append("\t\t\treturn currentValue;").AppendLine();
		sb.Append("\t\t}").AppendLine();
		sb.AppendLine();

		// ExecuteReturnCallbacks (private)
		sb.Append("\t\tprivate TValue ExecuteReturnCallbacks(").Append(
			string.Join(", ", Enumerable.Range(1, numberOfParameters).Select(i => $"T{i} p{i}"))).Append(", TValue currentValue, out bool matched)").AppendLine();
		sb.Append("\t\t{").AppendLine();
		sb.Append("\t\t\tmatched = false;").AppendLine();
		sb.Append("\t\t\tif (_returnCallbacks is not null)").AppendLine();
		sb.Append("\t\t\t{").AppendLine();
		sb.Append("\t\t\t\tforeach (Callback<global::System.Func<int, ").Append(typeParams).Append(", TValue, TValue>> _ in _returnCallbacks)").AppendLine();
		sb.Append("\t\t\t\t{").AppendLine();
		sb.Append("\t\t\t\t\tCallback<global::System.Func<int, ").Append(typeParams).Append(", TValue, TValue>> returnCallback =").AppendLine();
		sb.Append("\t\t\t\t\t\t_returnCallbacks[_returnCallbacks.CurrentIndex % _returnCallbacks.Count];").AppendLine();
		sb.Append("\t\t\t\t\tif (returnCallback.Invoke(ref _returnCallbacks.CurrentIndex, (invocationCount, @delegate)").AppendLine();
		sb.Append("\t\t\t\t\t\t=> @delegate(invocationCount, ").Append(parameters).Append(", currentValue), out TValue? newValue))").AppendLine();
		sb.Append("\t\t\t\t\t{").AppendLine();
		sb.Append("\t\t\t\t\t\tmatched = true;").AppendLine();
		sb.Append("\t\t\t\t\t\treturn newValue!;").AppendLine();
		sb.Append("\t\t\t\t\t}").AppendLine();
		sb.Append("\t\t\t\t}").AppendLine();
		sb.Append("\t\t\t}").AppendLine();
		sb.AppendLine();
		sb.Append("\t\t\treturn currentValue;").AppendLine();
		sb.Append("\t\t}").AppendLine();
		sb.AppendLine();

		// TryExtractParameters (private static)
		sb.Append("\t\tprivate static bool TryExtractParameters(global::Mockolate.Interactions.IndexerAccess access");
		for (int i = 1; i <= numberOfParameters; i++)
		{
			sb.Append(", out T").Append(i).Append(" p").Append(i);
		}

		sb.Append(")").AppendLine();
		sb.Append("\t\t{").AppendLine();
		sb.Append("\t\t\tif (access is global::Mockolate.Interactions.IndexerGetterAccess<").Append(typeParams).Append("> getter)").AppendLine();
		sb.Append("\t\t\t{").AppendLine();
		for (int i = 1; i <= numberOfParameters; i++)
		{
			sb.Append("\t\t\t\tp").Append(i).Append(" = getter.Parameter").Append(i).Append(";").AppendLine();
		}

		sb.Append("\t\t\t\treturn true;").AppendLine();
		sb.Append("\t\t\t}").AppendLine();
		sb.AppendLine();
		sb.Append("\t\t\tif (access is global::Mockolate.Interactions.IndexerSetterAccess<").Append(typeParams).Append(", TValue> setter)").AppendLine();
		sb.Append("\t\t\t{").AppendLine();
		for (int i = 1; i <= numberOfParameters; i++)
		{
			sb.Append("\t\t\t\tp").Append(i).Append(" = setter.Parameter").Append(i).Append(";").AppendLine();
		}

		sb.Append("\t\t\t\treturn true;").AppendLine();
		sb.Append("\t\t\t}").AppendLine();
		sb.AppendLine();
		for (int i = 1; i <= numberOfParameters; i++)
		{
			sb.Append("\t\t\tp").Append(i).Append(" = default!;").AppendLine();
		}

		sb.Append("\t\t\treturn false;").AppendLine();
		sb.Append("\t\t}").AppendLine();
		sb.AppendLine();

		// ToString
		sb.Append("\t\t/// <inheritdoc cref=\"object.ToString()\" />").AppendLine();
		sb.Append("\t\tpublic override string ToString()").AppendLine();
		sb.Append("\t\t\t=> $\"{FormatType(typeof(TValue))} this[").Append(string.Join(", ",
			Enumerable.Range(1, numberOfParameters).Select(i => $"{{parameter{i}}}"))).Append("]\";").AppendLine();
		sb.AppendLine();

		sb.Append("\t}").AppendLine();
	}
}
