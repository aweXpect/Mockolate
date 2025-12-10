using System.Text;

namespace Mockolate.SourceGenerators.Sources;

internal static partial class Sources
{
	public static string MethodSetups(HashSet<(int, bool)> methodSetups)
	{
		StringBuilder sb = InitializeBuilder([
			"System",
			"System.Collections.Generic",
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
		foreach ((int, bool) item in methodSetups)
		{
			sb.AppendLine();
			if (item.Item2)
			{
				AppendVoidMethodSetup(sb, item.Item1);
			}
			else
			{
				AppendReturnMethodSetup(sb, item.Item1);
			}
		}

		sb.Append("""
		          }

		          namespace Mockolate
		          {
		          	internal static class MethodSetupExtensions
		          	{
		          """).AppendLine();
		foreach ((int, bool) item in methodSetups)
		{
			string genericParameters = string.Join(", ", Enumerable.Range(1, item.Item1).Select(i => $"T{i}"));
			if (!item.Item2)
			{
				genericParameters = "TReturn, " + genericParameters;
			}

			string xmlDocSummary = item.Item2 ? "returning void" : "returning <typeparamref name=\"TReturn\"/>";
			string typePrefix = item.Item2
				? "Mockolate.Setup.IVoidMethodSetupReturnBuilder"
				: "Mockolate.Setup.IReturnMethodSetupReturnBuilder";
			string setupTypePrefix = item.Item2
				? "Mockolate.Setup.IVoidMethodSetup"
				: "Mockolate.Setup.IReturnMethodSetup";
			string setupCallbackPrefix = item.Item2
				? "Mockolate.Setup.IVoidMethodSetupCallbackBuilder"
				: "Mockolate.Setup.IReturnMethodSetupCallbackBuilder";
			sb.Append($$"""
			            		/// <summary>
			            		///     Extensions for method callback setup {{xmlDocSummary}} with {{item.Item1}} parameters.
			            		/// </summary>
			            		extension<{{genericParameters}}>({{setupCallbackPrefix}}<{{genericParameters}}> setup)
			            		{
			            			/// <summary>
			            			///     Executes the callback only once.
			            			/// </summary>
			            			public {{setupTypePrefix}}<{{genericParameters}}> OnlyOnce()
			            				=> setup.Only(1);
			            		}
			            		
			            		/// <summary>
			            		///     Extensions for method setup {{xmlDocSummary}} with {{item.Item1}} parameters.
			            		/// </summary>
			            		extension<{{genericParameters}}>({{typePrefix}}<{{genericParameters}}> setup)
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
			            			public {{setupTypePrefix}}<{{genericParameters}}> OnlyOnce()
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

	private static void AppendVoidMethodSetup(StringBuilder sb, int numberOfParameters)
	{
		string typeParams = string.Join(", ", Enumerable.Range(1, numberOfParameters).Select(i => $"T{i}"));
		string parameters = string.Join(", ", Enumerable.Range(1, numberOfParameters).Select(i => $"p{i}"));
		string discards = string.Join(", ", Enumerable.Range(1, numberOfParameters).Select(_ => "_"));

		sb.Append("\t/// <summary>").AppendLine();
		sb.Append("\t///     Sets up a method with ").Append(numberOfParameters).Append(" parameters ");
		for (int i = 1; i < numberOfParameters - 1; i++)
		{
			sb.Append("<typeparamref name=\"T").Append(i).Append("\" />, ");
		}

		sb.Append("<typeparamref name=\"T").Append(numberOfParameters - 1).Append("\" /> and <typeparamref name=\"T")
			.Append(numberOfParameters).Append("\" /> returning <see langword=\"void\" />.").AppendLine();
		sb.Append("\t/// </summary>").AppendLine();
		sb.Append("\tinternal interface IVoidMethodSetup<").Append(typeParams).Append(">").AppendLine();
		sb.Append("\t{").AppendLine();
		sb.Append("\t\t/// <summary>").AppendLine();
		sb.Append(
				"\t\t///     Flag indicating if the base class implementation should be called, and its return values used as default values.")
			.AppendLine();
		sb.Append("\t\t/// </summary>").AppendLine();
		sb.Append("\t\t/// <remarks>").AppendLine();
		sb.Append("\t\t///     If not specified, use <see cref=\"MockBehavior.CallBaseClass\" />.")
			.AppendLine();
		sb.Append("\t\t/// </remarks>").AppendLine();
		sb.Append("\t\tIVoidMethodSetup<").Append(typeParams).Append("> CallingBaseClass(bool callBaseClass = true);")
			.AppendLine();
		sb.AppendLine();

		sb.Append("\t\t/// <summary>").AppendLine();
		sb.Append("\t\t///     Registers a <paramref name=\"callback\" /> to execute when the method is called.")
			.AppendLine();
		sb.Append("\t\t/// </summary>").AppendLine();
		sb.Append("\t\tIVoidMethodSetupCallbackBuilder<").Append(typeParams).Append("> Do(Action callback);")
			.AppendLine();
		sb.AppendLine();

		sb.Append("\t\t/// <summary>").AppendLine();
		sb.Append("\t\t///     Registers a <paramref name=\"callback\" /> to execute when the method is called.")
			.AppendLine();
		sb.Append("\t\t/// </summary>").AppendLine();
		sb.Append("\t\tIVoidMethodSetupCallbackBuilder<").Append(typeParams).Append("> Do(Action<").Append(typeParams)
			.Append("> callback);").AppendLine();
		sb.AppendLine();

		sb.Append("\t\t/// <summary>").AppendLine();
		sb.Append("\t\t///     Registers a <paramref name=\"callback\" /> to execute when the method is called.")
			.AppendLine();
		sb.Append("\t\t/// </summary>").AppendLine();
		sb.Append("\t\tIVoidMethodSetupCallbackBuilder<").Append(typeParams).Append("> Do(Action<int, ")
			.Append(typeParams)
			.Append("> callback);").AppendLine();
		sb.AppendLine();

		sb.Append("\t\t/// <summary>").AppendLine();
		sb.Append("\t\t///     Registers an iteration in the sequence of method invocations, that does not throw.")
			.AppendLine();
		sb.Append("\t\t/// </summary>").AppendLine();
		sb.Append("\t\tIVoidMethodSetupReturnBuilder<").Append(typeParams).Append("> DoesNotThrow();").AppendLine();
		sb.AppendLine();

		sb.Append("\t\t/// <summary>").AppendLine();
		sb.Append("\t\t///     Registers an <typeparamref name=\"TException\" /> to throw when the method is invoked.")
			.AppendLine();
		sb.Append("\t\t/// </summary>").AppendLine();
		sb.Append("\t\tIVoidMethodSetupReturnBuilder<").Append(typeParams).Append("> Throws<TException>()")
			.AppendLine();
		sb.Append("\t\t\twhere TException : Exception, new();").AppendLine();
		sb.AppendLine();

		sb.Append("\t\t/// <summary>").AppendLine();
		sb.Append("\t\t///     Registers an <paramref name=\"exception\" /> to throw when the method is invoked.")
			.AppendLine();
		sb.Append("\t\t/// </summary>").AppendLine();
		sb.Append("\t\tIVoidMethodSetupReturnBuilder<").Append(typeParams).Append("> Throws(Exception exception);")
			.AppendLine();
		sb.AppendLine();

		sb.Append("\t\t/// <summary>").AppendLine();
		sb.Append(
				"\t\t///     Registers a <paramref name=\"callback\" /> that will calculate the exception to throw when the method is invoked.")
			.AppendLine();
		sb.Append("\t\t/// </summary>").AppendLine();
		sb.Append("\t\tIVoidMethodSetupReturnBuilder<").Append(typeParams).Append("> Throws(Func<")
			.Append(typeParams).Append(", Exception> callback);").AppendLine();
		sb.AppendLine();

		sb.Append("\t\t/// <summary>").AppendLine();
		sb.Append(
				"\t\t///     Registers a <paramref name=\"callback\" /> that will calculate the exception to throw when the method is invoked.")
			.AppendLine();
		sb.Append("\t\t/// </summary>").AppendLine();
		sb.Append("\t\tIVoidMethodSetupReturnBuilder<").Append(typeParams).Append("> Throws(Func<Exception> callback);")
			.AppendLine();
		sb.Append("}").AppendLine();
		sb.AppendLine();

		sb.Append("/// <summary>").AppendLine();
		sb.Append("///     Sets up a callback for a method with ").Append(numberOfParameters).Append(" parameters ");
		for (int i = 1; i < numberOfParameters - 1; i++)
		{
			sb.Append("<typeparamref name=\"T").Append(i).Append("\" />, ");
		}

		sb.Append("<typeparamref name=\"T").Append(numberOfParameters - 1).Append("\" /> and <typeparamref name=\"T")
			.Append(numberOfParameters).Append("\" /> returning <see langword=\"void\" />.").AppendLine();
		sb.Append("/// </summary>").AppendLine();
		sb.Append("internal interface IVoidMethodSetupCallbackBuilder<").Append(typeParams)
			.Append("> : IVoidMethodSetupCallbackWhenBuilder<").Append(typeParams).Append(">").AppendLine();
		sb.Append("{").AppendLine();
		sb.Append("\t\t/// <summary>").AppendLine();
		sb.Append(
				"\t\t///     Runs the callback in parallel to the other callbacks.")
			.AppendLine();
		sb.Append("\t\t/// </summary>").AppendLine();
		sb.Append("\t\tIVoidMethodSetupCallbackBuilder<").Append(typeParams).Append("> InParallel();").AppendLine();
		sb.AppendLine();
		sb.Append("\t\t/// <summary>").AppendLine();
		sb.Append(
				"\t\t///     Limits the callback to only execute for indexer accesses where the predicate returns true.")
			.AppendLine();
		sb.Append("\t\t/// <summary>").AppendLine();
		sb.Append(
				"\t\t///     Limits the callback to only execute for method invocations where the predicate returns true.")
			.AppendLine();
		sb.Append("\t\t/// </summary>").AppendLine();
		sb.Append("\t\t/// <remarks>").AppendLine();
		sb.Append(
				"\t\t///     Provides a zero-based counter indicating how many times the method has been invoked so far.")
			.AppendLine();
		sb.Append("\t\t/// </remarks>").AppendLine();
		sb.Append("\t\tIVoidMethodSetupCallbackWhenBuilder<").Append(typeParams)
			.Append("> When(Func<int, bool> predicate);").AppendLine();
		sb.Append("}").AppendLine();
		sb.AppendLine();

		sb.Append("/// <summary>").AppendLine();
		sb.Append("///     Sets up a when callback for a method with ").Append(numberOfParameters)
			.Append(" parameters ");
		for (int i = 1; i < numberOfParameters - 1; i++)
		{
			sb.Append("<typeparamref name=\"T").Append(i).Append("\" />, ");
		}

		sb.Append("<typeparamref name=\"T").Append(numberOfParameters - 1).Append("\" /> and <typeparamref name=\"T")
			.Append(numberOfParameters).Append("\" /> returning <see langword=\"void\" />.").AppendLine();
		sb.Append("/// </summary>").AppendLine();
		sb.Append("internal interface IVoidMethodSetupCallbackWhenBuilder<").Append(typeParams)
			.Append("> : IVoidMethodSetup<").Append(typeParams).Append(">").AppendLine();
		sb.Append("{").AppendLine();
		sb.Append("\t\t/// <summary>").AppendLine();
		sb.Append(
				"\t\t///     Repeats the callback for the given number of <paramref name=\"times\" />.")
			.AppendLine();
		sb.Append("\t\t/// </summary>").AppendLine();
		sb.Append("\t\t/// <remarks>").AppendLine();
		sb.Append(
				"\t\t///     The number of times is only counted for actual executions (<see cref=\"IVoidMethodSetupCallbackBuilder{")
			.Append(typeParams).Append("}.When(Func{int, bool})\" /> evaluates to <see langword=\"true\" />).")
			.AppendLine();
		sb.Append("\t\t/// </remarks>").AppendLine();
		sb.Append("\t\tIVoidMethodSetupCallbackWhenBuilder<").Append(typeParams).Append("> For(int times);")
			.AppendLine();
		sb.AppendLine();
		sb.Append("\t\t/// <summary>").AppendLine();
		sb.Append(
				"\t\t///     Deactivates the callback after the given number of <paramref name=\"times\" />.")
			.AppendLine();
		sb.Append("\t\t/// </summary>").AppendLine();
		sb.Append("\t\t/// <remarks>").AppendLine();
		sb.Append(
				"\t\t///     The number of times is only counted for actual executions (<see cref=\"IVoidMethodSetupCallbackBuilder{")
			.Append(typeParams).Append("}.When(Func{int, bool})\" /> evaluates to <see langword=\"true\" />).")
			.AppendLine();
		sb.Append("\t\t/// </remarks>").AppendLine();
		sb.Append("\t\tIVoidMethodSetup<").Append(typeParams).Append("> Only(int times);").AppendLine();
		sb.Append("}").AppendLine();
		sb.AppendLine();


		sb.Append("/// <summary>").AppendLine();
		sb.Append("///     Sets up a return callback for a method with ").Append(numberOfParameters)
			.Append(" parameters ");
		for (int i = 1; i < numberOfParameters - 1; i++)
		{
			sb.Append("<typeparamref name=\"T").Append(i).Append("\" />, ");
		}

		sb.Append("<typeparamref name=\"T").Append(numberOfParameters - 1).Append("\" /> and <typeparamref name=\"T")
			.Append(numberOfParameters).Append("\" /> returning <see langword=\"void\" />.").AppendLine();
		sb.Append("/// </summary>").AppendLine();
		sb.Append("internal interface IVoidMethodSetupReturnBuilder<").Append(typeParams)
			.Append("> : IVoidMethodSetupReturnWhenBuilder<").Append(typeParams).Append(">").AppendLine();
		sb.Append("{").AppendLine();
		sb.Append("\t\t/// <summary>").AppendLine();
		sb.Append(
				"\t\t///     Limits the throw to only execute for method invocations where the predicate returns true.")
			.AppendLine();
		sb.Append("\t\t/// </summary>").AppendLine();
		sb.Append("\t\t/// <remarks>").AppendLine();
		sb.Append(
				"\t\t///     Provides a zero-based counter indicating how many times the method has been invoked so far.")
			.AppendLine();
		sb.Append("\t\t/// </remarks>").AppendLine();
		sb.Append("\t\tIVoidMethodSetupReturnWhenBuilder<").Append(typeParams)
			.Append("> When(Func<int, bool> predicate);").AppendLine();
		sb.Append("}").AppendLine();
		sb.AppendLine();

		sb.Append("/// <summary>").AppendLine();
		sb.Append("///     Sets up a when callback for a method with ").Append(numberOfParameters)
			.Append(" parameters ");
		for (int i = 1; i < numberOfParameters - 1; i++)
		{
			sb.Append("<typeparamref name=\"T").Append(i).Append("\" />, ");
		}

		sb.Append("<typeparamref name=\"T").Append(numberOfParameters - 1).Append("\" /> and <typeparamref name=\"T")
			.Append(numberOfParameters).Append("\" /> returning <see langword=\"void\" />.").AppendLine();
		sb.Append("/// </summary>").AppendLine();
		sb.Append("internal interface IVoidMethodSetupReturnWhenBuilder<").Append(typeParams)
			.Append("> : IVoidMethodSetup<").Append(typeParams).Append(">").AppendLine();
		sb.Append("{").AppendLine();
		sb.Append("\t\t/// <summary>").AppendLine();
		sb.Append(
				"\t\t///     Repeats the throw for the given number of <paramref name=\"times\" />.")
			.AppendLine();
		sb.Append("\t\t/// </summary>").AppendLine();
		sb.Append("\t\t/// <remarks>").AppendLine();
		sb.Append(
				"\t\t///     The number of times is only counted for actual executions (<see cref=\"IVoidMethodSetupReturnBuilder{")
			.Append(typeParams).Append("}.When(Func{int, bool})\" /> evaluates to <see langword=\"true\" />).")
			.AppendLine();
		sb.Append("\t\t/// </remarks>").AppendLine();
		sb.Append("\t\tIVoidMethodSetupReturnWhenBuilder<").Append(typeParams).Append("> For(int times);").AppendLine();
		sb.AppendLine();
		sb.Append("\t\t/// <summary>").AppendLine();
		sb.Append(
				"\t\t///     Deactivates the throw after the given number of <paramref name=\"times\" />.")
			.AppendLine();
		sb.Append("\t\t/// </summary>").AppendLine();
		sb.Append("\t\t/// <remarks>").AppendLine();
		sb.Append(
				"\t\t///     The number of times is only counted for actual executions (<see cref=\"IVoidMethodSetupReturnBuilder{")
			.Append(typeParams).Append("}.When(Func{int, bool})\" /> evaluates to <see langword=\"true\" />).")
			.AppendLine();
		sb.Append("\t\t/// </remarks>").AppendLine();
		sb.Append("\t\tIVoidMethodSetup<").Append(typeParams).Append("> Only(int times);").AppendLine();
		sb.Append("}").AppendLine();
		sb.AppendLine();

		sb.Append("/// <summary>").AppendLine();
		sb.Append("///     Sets up a method with ").Append(numberOfParameters).Append(" parameters ");
		for (int i = 1; i < numberOfParameters - 1; i++)
		{
			sb.Append("<typeparamref name=\"T").Append(i).Append("\" />, ");
		}

		sb.Append("<typeparamref name=\"T").Append(numberOfParameters - 1).Append("\" /> and <typeparamref name=\"T")
			.Append(numberOfParameters).Append("\" /> returning <see langword=\"void\" />.").AppendLine();
		sb.Append("/// </summary>").AppendLine();
		sb.Append("internal class VoidMethodSetup<").Append(typeParams)
			.Append("> : MethodSetup,").AppendLine();
		sb.Append("\t\tIVoidMethodSetupCallbackBuilder<").Append(typeParams).Append(">,").AppendLine();
		sb.Append("\t\tIVoidMethodSetupReturnBuilder<").Append(typeParams).Append(">").AppendLine();
		sb.Append("{").AppendLine();
		sb.Append("\t\tprivate readonly List<Callback<Action<int, ").Append(typeParams).Append(">>> _callbacks = [];")
			.AppendLine();
		sb.Append("\t\tprivate readonly List<Callback<Action<int, ").Append(typeParams)
			.Append(">>> _returnCallbacks = [];")
			.AppendLine();
		sb.Append("\t\tprivate readonly string _name;").AppendLine();
		sb.Append("\t\tprivate readonly IParameters? _matches;").AppendLine();
		for (int i = 1; i <= numberOfParameters; i++)
		{
			sb.Append("\t\tprivate readonly NamedParameter? _match").Append(i).Append(";").AppendLine();
		}

		sb.Append("\t\tprivate bool? _callBaseClass;").AppendLine();
		sb.Append("\t\tprivate Callback? _currentCallback;").AppendLine();
		sb.Append("\t\tprivate Callback? _currentReturnCallback;").AppendLine();
		sb.Append("\t\tprivate int _currentCallbacksIndex;").AppendLine();
		sb.Append("\t\tprivate int _currentReturnCallbackIndex;").AppendLine();
		sb.AppendLine();

		sb.Append("\t\t/// <inheritdoc cref=\"VoidMethodSetup{")
			.Append(string.Join(", ", Enumerable.Range(1, numberOfParameters).Select(i => $"T{i}"))).Append("}\" />")
			.AppendLine();
		sb.Append("\t\tpublic VoidMethodSetup(").AppendLine();
		sb.Append("\t\t\tstring name");
		for (int i = 1; i <= numberOfParameters; i++)
		{
			sb.Append(',').AppendLine().Append("\t\tNamedParameter match").Append(i);
		}

		sb.Append(')').AppendLine();
		sb.Append("\t\t{").AppendLine();
		sb.Append("\t\t\t_name = name;").AppendLine();
		for (int i = 1; i <= numberOfParameters; i++)
		{
			sb.Append("\t\t\t_match").Append(i).Append(" = match").Append(i).Append(";").AppendLine();
		}

		sb.Append("\t\t}").AppendLine();
		sb.AppendLine();

		sb.Append("\t\t/// <inheritdoc cref=\"VoidMethodSetup{")
			.Append(string.Join(", ", Enumerable.Range(1, numberOfParameters).Select(i => $"T{i}"))).Append("}\" />")
			.AppendLine();
		sb.Append("\t\tpublic VoidMethodSetup(string name, IParameters matches)").AppendLine();
		sb.Append("\t\t{").AppendLine();
		sb.Append("\t\t\t_name = name;").AppendLine();
		sb.Append("\t\t\t_matches = matches;").AppendLine();
		sb.Append("\t\t}").AppendLine();
		sb.AppendLine();

		sb.Append("\t\t/// <summary>").AppendLine();
		sb.Append(
				"\t\t///     Flag indicating if the base class implementation should be called, and its return values used as default values.")
			.AppendLine();
		sb.Append("\t\t/// </summary>").AppendLine();
		sb.Append("\t\t/// <remarks>").AppendLine();
		sb.Append("\t\t///     If not specified, use <see cref=\"MockBehavior.CallBaseClass\" />.")
			.AppendLine();
		sb.Append("\t\t/// </remarks>").AppendLine();
		sb.Append("\t\tpublic IVoidMethodSetup<").Append(typeParams)
			.Append("> CallingBaseClass(bool callBaseClass = true)").AppendLine();
		sb.Append("\t\t{").AppendLine();
		sb.Append("\t\t\t_callBaseClass = callBaseClass;").AppendLine();
		sb.Append("\t\t\treturn this;").AppendLine();
		sb.Append("\t\t}").AppendLine();
		sb.AppendLine();

		sb.Append("\t\t/// <summary>").AppendLine();
		sb.Append("\t\t///     Registers a <paramref name=\"callback\" /> to execute when the method is called.")
			.AppendLine();
		sb.Append("\t\t/// </summary>").AppendLine();
		sb.Append("\t\tpublic IVoidMethodSetupCallbackBuilder<").Append(typeParams).Append("> Do(Action callback)")
			.AppendLine();
		sb.Append("\t\t{").AppendLine();
		sb.Append("\t\t\tCallback<Action<int, ").Append(typeParams).Append(">>? currentCallback = new((_, ")
			.Append(discards).Append(") => callback());").AppendLine();
		sb.Append("\t\t\t_currentCallback = currentCallback;").AppendLine();
		sb.Append("\t\t\t_callbacks.Add(currentCallback);").AppendLine();
		sb.Append("\t\t\treturn this;").AppendLine();
		sb.Append("\t\t}").AppendLine();
		sb.AppendLine();

		sb.Append("\t\t/// <summary>").AppendLine();
		sb.Append("\t\t///     Registers a <paramref name=\"callback\" /> to execute when the method is called.")
			.AppendLine();
		sb.Append("\t\t/// </summary>").AppendLine();
		sb.Append("\t\tpublic IVoidMethodSetupCallbackBuilder<").Append(typeParams).Append("> Do(Action<")
			.Append(typeParams)
			.Append("> callback)").AppendLine();
		sb.Append("\t\t{").AppendLine();
		sb.Append("\t\t\tCallback<Action<int, ").Append(typeParams).Append(">>? currentCallback = new((_, ")
			.Append(parameters).Append(") => callback(").Append(parameters).Append("));").AppendLine();
		sb.Append("\t\t\t_currentCallback = currentCallback;").AppendLine();
		sb.Append("\t\t\t_callbacks.Add(currentCallback);").AppendLine();
		sb.Append("\t\t\treturn this;").AppendLine();
		sb.Append("\t\t}").AppendLine();
		sb.AppendLine();

		sb.Append("\t\t/// <summary>").AppendLine();
		sb.Append("\t\t///     Registers a <paramref name=\"callback\" /> to execute when the method is called.")
			.AppendLine();
		sb.Append("\t\t/// </summary>").AppendLine();
		sb.Append("\t\tpublic IVoidMethodSetupCallbackBuilder<").Append(typeParams).Append("> Do(Action<int, ")
			.Append(typeParams)
			.Append("> callback)").AppendLine();
		sb.Append("\t\t{").AppendLine();
		sb.Append("\t\t\tCallback<Action<int, ").Append(typeParams).Append(">>? currentCallback = new(callback);")
			.AppendLine();
		sb.Append("\t\t\t_currentCallback = currentCallback;").AppendLine();
		sb.Append("\t\t\t_callbacks.Add(currentCallback);").AppendLine();
		sb.Append("\t\t\treturn this;").AppendLine();
		sb.Append("\t\t}").AppendLine();
		sb.AppendLine();

		sb.Append("\t\t/// <summary>").AppendLine();
		sb.Append("\t\t///     Registers an iteration in the sequence of method invocations, that does not throw.")
			.AppendLine();
		sb.Append("\t\t/// </summary>").AppendLine();
		sb.Append("\t\tpublic IVoidMethodSetupReturnBuilder<").Append(typeParams).Append("> DoesNotThrow()")
			.AppendLine();
		sb.Append("\t\t{").AppendLine();
		sb.Append("\t\t\tvar currentCallback = new Callback<Action<int, ").Append(typeParams).Append(">>((_, ")
			.Append(string.Join(", ", Enumerable.Range(0, numberOfParameters).Select(_ => "_")))
			.Append(") => { });").AppendLine();
		sb.Append("\t\t\t_currentReturnCallback = currentCallback;").AppendLine();
		sb.Append("\t\t\t_returnCallbacks.Add(currentCallback);").AppendLine();
		sb.Append("\t\t\treturn this;").AppendLine();
		sb.Append("\t\t}").AppendLine();
		sb.AppendLine();

		sb.Append("\t\t/// <summary>").AppendLine();
		sb.Append("\t\t///     Registers an <typeparamref name=\"TException\" /> to throw when the method is invoked.")
			.AppendLine();
		sb.Append("\t\t/// </summary>").AppendLine();
		sb.Append("\t\tpublic IVoidMethodSetupReturnBuilder<").Append(typeParams).Append("> Throws<TException>()")
			.AppendLine();
		sb.Append("\t\t\twhere TException : Exception, new()").AppendLine();
		sb.Append("\t\t{").AppendLine();
		sb.Append("\t\t\tvar currentCallback = new Callback<Action<int, ").Append(typeParams).Append(">>((_, ")
			.Append(string.Join(", ", Enumerable.Range(0, numberOfParameters).Select(_ => "_")))
			.Append(") => throw new TException());").AppendLine();
		sb.Append("\t\t\t_currentReturnCallback = currentCallback;").AppendLine();
		sb.Append("\t\t\t_returnCallbacks.Add(currentCallback);").AppendLine();
		sb.Append("\t\t\treturn this;").AppendLine();
		sb.Append("\t\t}").AppendLine();
		sb.AppendLine();

		sb.Append("\t\t/// <summary>").AppendLine();
		sb.Append("\t\t///     Registers an <paramref name=\"exception\" /> to throw when the method is invoked.")
			.AppendLine();
		sb.Append("\t\t/// </summary>").AppendLine();
		sb.Append("\t\tpublic IVoidMethodSetupReturnBuilder<").Append(typeParams)
			.Append("> Throws(Exception exception)")
			.AppendLine();
		sb.Append("\t\t{").AppendLine();
		sb.Append("\t\t\tvar currentCallback = new Callback<Action<int, ").Append(typeParams).Append(">>((_, ")
			.Append(string.Join(", ", Enumerable.Range(0, numberOfParameters).Select(_ => "_")))
			.Append(") => throw exception);").AppendLine();
		sb.Append("\t\t\t_currentReturnCallback = currentCallback;").AppendLine();
		sb.Append("\t\t\t_returnCallbacks.Add(currentCallback);").AppendLine();
		sb.Append("\t\t\treturn this;").AppendLine();
		sb.Append("\t\t}").AppendLine();
		sb.AppendLine();

		sb.Append("\t\t/// <summary>").AppendLine();
		sb.Append(
				"\t\t///     Registers a <paramref name=\"callback\" /> that will calculate the exception to throw when the method is invoked.")
			.AppendLine();
		sb.Append("\t\t/// </summary>").AppendLine();
		sb.Append("\t\tpublic IVoidMethodSetupReturnBuilder<").Append(typeParams).Append("> Throws(Func<")
			.Append(typeParams).Append(", Exception> callback)").AppendLine();
		sb.Append("\t\t{").AppendLine();
		sb.Append("\t\t\tvar currentCallback = new Callback<Action<int, ").Append(typeParams).Append(">>((_, ")
			.Append(parameters).Append(") => throw callback(").Append(parameters).Append("));").AppendLine();
		sb.Append("\t\t\t_currentReturnCallback = currentCallback;").AppendLine();
		sb.Append("\t\t\t_returnCallbacks.Add(currentCallback);").AppendLine();
		sb.Append("\t\t\treturn this;").AppendLine();
		sb.Append("\t\t}").AppendLine();
		sb.AppendLine();

		sb.Append("\t\t/// <summary>").AppendLine();
		sb.Append(
				"\t\t///     Registers a <paramref name=\"callback\" /> that will calculate the exception to throw when the method is invoked.")
			.AppendLine();
		sb.Append("\t\t/// </summary>").AppendLine();
		sb.Append("\t\tpublic IVoidMethodSetupReturnBuilder<").Append(typeParams)
			.Append("> Throws(Func<Exception> callback)")
			.AppendLine();
		sb.Append("\t\t{").AppendLine();
		sb.Append("\t\t\tvar currentCallback = new Callback<Action<int, ").Append(typeParams).Append(">>((_, ")
			.Append(string.Join(", ", Enumerable.Range(0, numberOfParameters).Select(_ => "_")))
			.Append(") => throw callback());").AppendLine();
		sb.Append("\t\t\t_currentReturnCallback = currentCallback;").AppendLine();
		sb.Append("\t\t\t_returnCallbacks.Add(currentCallback);").AppendLine();
		sb.Append("\t\t\treturn this;").AppendLine();
		sb.Append("\t\t}").AppendLine();
		sb.AppendLine();

		sb.Append("\t\t/// <inheritdoc cref=\"IVoidMethodSetupCallbackBuilder{").Append(typeParams)
			.Append("}.InParallel()\" />").AppendLine();
		sb.Append("\t\tIVoidMethodSetupCallbackBuilder<").Append(typeParams)
			.Append("> IVoidMethodSetupCallbackBuilder<").Append(typeParams)
			.Append(">.InParallel()").AppendLine();
		sb.Append("\t\t{").AppendLine();
		sb.Append("\t\t\t_currentCallback?.InParallel();").AppendLine();
		sb.Append("\t\t\treturn this;").AppendLine();
		sb.Append("\t\t}").AppendLine();
		sb.AppendLine();

		sb.Append("\t\t/// <inheritdoc cref=\"IVoidMethodSetupCallbackBuilder{").Append(typeParams)
			.Append("}.When(Func{int, bool})\" />").AppendLine();
		sb.Append("\t\tIVoidMethodSetupCallbackWhenBuilder<").Append(typeParams)
			.Append("> IVoidMethodSetupCallbackBuilder<").Append(typeParams)
			.Append(">.When(Func<int, bool> predicate)").AppendLine();
		sb.Append("\t\t{").AppendLine();
		sb.Append("\t\t\t_currentCallback?.When(predicate);").AppendLine();
		sb.Append("\t\t\treturn this;").AppendLine();
		sb.Append("\t\t}").AppendLine();
		sb.AppendLine();

		sb.Append("\t\t/// <inheritdoc cref=\"IVoidMethodSetupCallbackWhenBuilder{").Append(typeParams)
			.Append("}.For(int)\" />").AppendLine();
		sb.Append("\t\tIVoidMethodSetupCallbackWhenBuilder<").Append(typeParams)
			.Append("> IVoidMethodSetupCallbackWhenBuilder<")
			.Append(typeParams)
			.Append(">.For(int times)").AppendLine();
		sb.Append("\t\t{").AppendLine();
		sb.Append("\t\t\t_currentCallback?.For(times);").AppendLine();
		sb.Append("\t\t\treturn this;").AppendLine();
		sb.Append("\t\t}").AppendLine();
		sb.AppendLine();

		sb.Append("\t\t/// <inheritdoc cref=\"IVoidMethodSetupCallbackWhenBuilder{").Append(typeParams)
			.Append("}.Only(int)\" />").AppendLine();
		sb.Append("\t\tIVoidMethodSetup<").Append(typeParams).Append("> IVoidMethodSetupCallbackWhenBuilder<")
			.Append(typeParams)
			.Append(">.Only(int times)").AppendLine();
		sb.Append("\t\t{").AppendLine();
		sb.Append("\t\t\t_currentCallback?.Only(times);").AppendLine();
		sb.Append("\t\t\treturn this;").AppendLine();
		sb.Append("\t\t}").AppendLine();
		sb.AppendLine();

		sb.Append("\t\t/// <inheritdoc cref=\"IVoidMethodSetupReturnBuilder{").Append(typeParams)
			.Append("}.When(Func{int, bool})\" />").AppendLine();
		sb.Append("\t\tIVoidMethodSetupReturnWhenBuilder<").Append(typeParams)
			.Append("> IVoidMethodSetupReturnBuilder<").Append(typeParams)
			.Append(">.When(Func<int, bool> predicate)").AppendLine();
		sb.Append("\t\t{").AppendLine();
		sb.Append("\t\t\t_currentReturnCallback?.When(predicate);").AppendLine();
		sb.Append("\t\t\treturn this;").AppendLine();
		sb.Append("\t\t}").AppendLine();
		sb.AppendLine();

		sb.Append("\t\t/// <inheritdoc cref=\"IVoidMethodSetupReturnWhenBuilder{").Append(typeParams)
			.Append("}.For(int)\" />").AppendLine();
		sb.Append("\t\tIVoidMethodSetupReturnWhenBuilder<").Append(typeParams)
			.Append("> IVoidMethodSetupReturnWhenBuilder<")
			.Append(typeParams)
			.Append(">.For(int times)").AppendLine();
		sb.Append("\t\t{").AppendLine();
		sb.Append("\t\t\t_currentReturnCallback?.For(times);").AppendLine();
		sb.Append("\t\t\treturn this;").AppendLine();
		sb.Append("\t\t}").AppendLine();
		sb.AppendLine();

		sb.Append("\t\t/// <inheritdoc cref=\"IVoidMethodSetupReturnWhenBuilder{").Append(typeParams)
			.Append("}.Only(int)\" />").AppendLine();
		sb.Append("\t\tIVoidMethodSetup<").Append(typeParams).Append("> IVoidMethodSetupReturnWhenBuilder<")
			.Append(typeParams)
			.Append(">.Only(int times)").AppendLine();
		sb.Append("\t\t{").AppendLine();
		sb.Append("\t\t\t_currentReturnCallback?.Only(times);").AppendLine();
		sb.Append("\t\t\treturn this;").AppendLine();
		sb.Append("\t\t}").AppendLine();
		sb.AppendLine();

		sb.Append("\t\t/// <inheritdoc cref=\"MethodSetup.ExecuteCallback(MethodInvocation, MockBehavior)\" />")
			.AppendLine();
		sb.Append("\t\tprotected override void ExecuteCallback(MethodInvocation invocation, MockBehavior behavior)")
			.AppendLine();
		sb.Append("\t\t{").AppendLine();
		sb.Append("\t\t\tif (");
		for (int i = 1; i < numberOfParameters; i++)
		{
			sb.Append("TryCast(invocation.Parameters[").Append(i - 1).Append("], out T").Append(i).Append(" p")
				.Append(i).Append(", behavior) &&");
			sb.AppendLine().Append("\t\t    ");
		}

		sb.Append("TryCast(invocation.Parameters[")
			.Append(numberOfParameters - 1).Append("], out T").Append(numberOfParameters).Append(" p")
			.Append(numberOfParameters).Append(", behavior))")
			.AppendLine();
		sb.Append("\t\t\t{").AppendLine();
		sb.Append("\t\t\t\tbool wasInvoked = false;").AppendLine();
		sb.Append("\t\t\t\tint currentCallbacksIndex = _currentCallbacksIndex;").AppendLine();
		sb.Append("\t\t\t\tfor (int i = 0; i < _callbacks.Count; i++)").AppendLine();
		sb.Append("\t\t\t\t{").AppendLine();
		sb.Append("\t\t\t\t\tCallback<Action<int, ").Append(typeParams).Append(">>? callback =").AppendLine();
		sb.Append("\t\t\t\t\t\t_callbacks[(currentCallbacksIndex + i) % _callbacks.Count];").AppendLine();
		sb.Append("\t\t\t\t\tif (callback.Invoke(wasInvoked, ref _currentCallbacksIndex, (invocationCount, @delegate)")
			.AppendLine();
		sb.Append("\t\t\t\t\t=> @delegate(invocationCount, ")
			.Append(string.Join(", ", Enumerable.Range(1, numberOfParameters).Select(x => $"p{x}"))).Append(")))")
			.AppendLine();
		sb.Append("\t\t\t\t\t{").AppendLine();
		sb.Append("\t\t\t\t\t\twasInvoked = true;").AppendLine();
		sb.Append("\t\t\t\t\t}").AppendLine();
		sb.Append("\t\t\t\t}").AppendLine();
		sb.AppendLine();
		sb.Append("\t\t\t\tforeach (var _ in _returnCallbacks)").AppendLine();
		sb.Append("\t\t\t\t{").AppendLine();
		sb.Append(
				"\t\t\t\t\tvar returnCallback = _returnCallbacks[_currentReturnCallbackIndex % _returnCallbacks.Count];")
			.AppendLine();
		sb.Append("\t\t\t\t\tif (returnCallback.Invoke(ref _currentReturnCallbackIndex, (invocationCount, @delegate)")
			.AppendLine();
		sb.Append("\t\t\t\t\t\t=> @delegate(invocationCount, ")
			.Append(string.Join(", ", Enumerable.Range(1, numberOfParameters).Select(x => $"p{x}"))).Append(")))")
			.AppendLine();
		sb.Append("\t\t\t\t\t{").AppendLine();
		sb.Append("\t\t\t\t\t\treturn;").AppendLine();
		sb.Append("\t\t\t\t\t}").AppendLine();
		sb.Append("\t\t\t\t}").AppendLine();
		sb.Append("\t\t\t}").AppendLine();
		sb.Append("\t\t}").AppendLine();
		sb.AppendLine();

		sb.Append(
				"\t\t/// <inheritdoc cref=\"MethodSetup.GetReturnValue{TResult}(MethodInvocation, MockBehavior, Func{TResult})\" />")
			.AppendLine();
		sb.Append(
				"\t\tprotected override TResult GetReturnValue<TResult>(MethodInvocation invocation, MockBehavior behavior, Func<TResult> defaultValueGenerator)")
			.AppendLine();
		sb.Append("\t\t\twhere TResult : default").AppendLine();
		sb.Append("\t\t\t=> throw new MockException(\"The method setup does not support return values.\");")
			.AppendLine();
		sb.AppendLine();

		sb.Append("\t\t/// <inheritdoc cref=\"MethodSetup.IsMatch(MethodInvocation)\" />").AppendLine();
		sb.Append("\t\tprotected override bool IsMatch(MethodInvocation invocation)").AppendLine();
		sb.Append("\t\t\t=> invocation.Name.Equals(_name) &&").AppendLine();
		sb.Append("\t\t\t\t(_matches is not null").AppendLine();
		sb.Append("\t\t\t\t\t? _matches.Matches(invocation.Parameters)").AppendLine();
		sb.Append("\t\t\t\t\t: Matches([")
			.Append(string.Join(", ", Enumerable.Range(1, numberOfParameters).Select(x => $"_match{x}!")))
			.Append("], invocation.Parameters));").AppendLine();
		sb.AppendLine();

		sb.Append("\t\t/// <inheritdoc cref=\"MethodSetup.TriggerParameterCallbacks(object?[])\" />").AppendLine();
		sb.Append("\t\tprotected override void TriggerParameterCallbacks(object?[] parameters)").AppendLine();
		sb.Append("\t\t\t=> TriggerCallbacks([")
			.Append(string.Join(", ", Enumerable.Range(1, numberOfParameters).Select(x => $"_match{x}")))
			.Append("], parameters);").AppendLine();
		sb.AppendLine();

		sb.Append("\t\t/// <inheritdoc cref=\"MethodSetup.GetCallBaseClass()\" />").AppendLine();
		sb.Append("\t\tprotected override bool? GetCallBaseClass()").AppendLine();
		sb.Append("\t\t\t=> _callBaseClass;").AppendLine();
		sb.AppendLine();

		sb.Append("\t\t/// <inheritdoc cref=\"MethodSetup.HasReturnCalls()\" />").AppendLine();
		sb.Append("\t\tprotected override bool HasReturnCalls()").AppendLine();
		sb.Append("\t\t\t=> _returnCallbacks.Count > 0;").AppendLine();
		sb.AppendLine();

		sb.Append("\t\t/// <inheritdoc cref=\"MethodSetup.SetOutParameter{T}(string, Func{T})\" />").AppendLine();
		sb.Append("\t\tprotected override T SetOutParameter<T>(string parameterName, Func<T> defaultValueGenerator)")
			.AppendLine();
		sb.Append("\t\t{").AppendLine();
		sb.Append("\t\t\tif (")
			.Append(string.Join(" && ", Enumerable.Range(1, numberOfParameters).Select(x => $"_match{x} is not null")))
			.Append(" &&").AppendLine();
		sb.Append("\t\t\t\tHasOutParameter([")
			.Append(string.Join(", ", Enumerable.Range(1, numberOfParameters).Select(x => $"_match{x}")))
			.Append("], parameterName, out IOutParameter<T>? outParameter))").AppendLine();
		sb.Append("\t\t\t{").AppendLine();
		sb.Append("\t\t\t\treturn outParameter.GetValue(defaultValueGenerator);").AppendLine();
		sb.Append("\t\t\t}").AppendLine();
		sb.AppendLine();
		sb.Append("\t\t\treturn defaultValueGenerator();").AppendLine();
		sb.Append("\t\t}").AppendLine();
		sb.AppendLine();

		sb.Append("\t\t/// <inheritdoc cref=\"MethodSetup.SetRefParameter{T}(string, T, MockBehavior)\" />")
			.AppendLine();
		sb.Append("\t\tprotected override T SetRefParameter<T>(string parameterName, T value, MockBehavior behavior)")
			.AppendLine();
		sb.Append("\t\t{").AppendLine();
		sb.Append("\t\t\tif (")
			.Append(string.Join(" && ", Enumerable.Range(1, numberOfParameters).Select(x => $"_match{x} is not null")))
			.Append(" &&").AppendLine();
		sb.Append("\t\t\t\tHasRefParameter([")
			.Append(string.Join(", ", Enumerable.Range(1, numberOfParameters).Select(x => $"_match{x}")))
			.Append("], parameterName, out IRefParameter<T>? refParameter))").AppendLine();
		sb.Append("\t\t\t{").AppendLine();
		sb.Append("\t\t\t\treturn refParameter.GetValue(value);").AppendLine();
		sb.Append("\t\t\t}").AppendLine();
		sb.AppendLine();
		sb.Append("\t\t\treturn value;").AppendLine();
		sb.Append("\t\t}").AppendLine();
		sb.AppendLine();

		sb.Append("\t\t/// <inheritdoc cref=\"object.ToString()\" />").AppendLine();
		sb.Append("\t\tpublic override string ToString()").AppendLine();
		sb.Append("\t\t{").AppendLine();
		sb.Append("\t\t\tif (_matches is not null)").AppendLine();
		sb.Append("\t\t\t{").AppendLine();
		sb.Append("\t\t\t\treturn $\"void {_name}({_matches})\";").AppendLine();
		sb.Append("\t\t\t}").AppendLine();
		sb.AppendLine();
		sb.Append("\t\t\treturn $\"void {_name}(")
			.Append(string.Join(", ", Enumerable.Range(1, numberOfParameters).Select(x => $"{{_match{x}}}")))
			.Append(")\";").AppendLine();
		sb.Append("\t\t}").AppendLine();
		sb.Append("\t}").AppendLine();
		sb.AppendLine();
	}

	private static void AppendReturnMethodSetup(StringBuilder sb, int numberOfParameters)
	{
		string typeParams = string.Join(", ", Enumerable.Range(1, numberOfParameters).Select(i => $"T{i}"));
		string parameters = string.Join(", ", Enumerable.Range(1, numberOfParameters).Select(i => $"p{i}"));
		string discards = string.Join(", ", Enumerable.Range(1, numberOfParameters).Select(_ => "_"));

		sb.Append("\t/// <summary>").AppendLine();
		sb.Append("\t///     Sets up a method with ").Append(numberOfParameters).Append(" parameters ");
		for (int i = 1; i < numberOfParameters - 1; i++)
		{
			sb.Append("<typeparamref name=\"T").Append(i).Append("\" />, ");
		}

		sb.Append("<typeparamref name=\"T").Append(numberOfParameters - 1).Append("\" /> and <typeparamref name=\"T")
			.Append(numberOfParameters).Append("\" /> returning <typeparamref name=\"TReturn\" />.").AppendLine();
		sb.Append("\t/// </summary>").AppendLine();
		sb.Append("\tinternal interface IReturnMethodSetup<TReturn, ").Append(typeParams).Append(">")
			.AppendLine();
		sb.Append("\t{").AppendLine();
		sb.Append("\t\t/// <summary>").AppendLine();
		sb.Append(
				"\t\t///     Flag indicating if the base class implementation should be called, and its return values used as default values.")
			.AppendLine();
		sb.Append("\t\t/// </summary>").AppendLine();
		sb.Append("\t\t/// <remarks>").AppendLine();
		sb.Append("\t\t///     If not specified, use <see cref=\"MockBehavior.CallBaseClass\" />.")
			.AppendLine();
		sb.Append("\t\t/// </remarks>").AppendLine();
		sb.Append("\t\tIReturnMethodSetup<TReturn, ").Append(typeParams)
			.Append("> CallingBaseClass(bool callBaseClass = true);").AppendLine();
		sb.AppendLine();

		sb.Append("\t\t/// <summary>").AppendLine();
		sb.Append("\t\t///     Registers a <paramref name=\"callback\" /> to execute when the method is called.")
			.AppendLine();
		sb.Append("\t\t/// </summary>").AppendLine();
		sb.Append("\t\tIReturnMethodSetupCallbackBuilder<TReturn, ").Append(typeParams).Append("> Do(Action callback);")
			.AppendLine();
		sb.AppendLine();

		sb.Append("\t\t/// <summary>").AppendLine();
		sb.Append("\t\t///     Registers a <paramref name=\"callback\" /> to execute when the method is called.")
			.AppendLine();
		sb.Append("\t\t/// </summary>").AppendLine();
		sb.Append("\t\tIReturnMethodSetupCallbackBuilder<TReturn, ").Append(typeParams).Append("> Do(Action<")
			.Append(typeParams).Append("> callback);").AppendLine();
		sb.AppendLine();

		sb.Append("\t\t/// <summary>").AppendLine();
		sb.Append("\t\t///     Registers a <paramref name=\"callback\" /> to execute when the method is called.")
			.AppendLine();
		sb.Append("\t\t/// </summary>").AppendLine();
		sb.Append("\t\tIReturnMethodSetupCallbackBuilder<TReturn, ").Append(typeParams).Append("> Do(Action<int, ")
			.Append(typeParams).Append("> callback);").AppendLine();
		sb.AppendLine();

		sb.Append("\t\t/// <summary>").AppendLine();
		sb.Append("\t\t///     Registers a <paramref name=\"callback\" /> to setup the return value for this method.")
			.AppendLine();
		sb.Append("\t\t/// </summary>").AppendLine();
		sb.Append("\t\tIReturnMethodSetupReturnBuilder<TReturn, ").Append(typeParams).Append("> Returns(Func<")
			.Append(typeParams).Append(", TReturn> callback);").AppendLine();
		sb.AppendLine();

		sb.Append("\t\t/// <summary>").AppendLine();
		sb.Append("\t\t///     Registers a <paramref name=\"callback\" /> to setup the return value for this method.")
			.AppendLine();
		sb.Append("\t\t/// </summary>").AppendLine();
		sb.Append("\t\tIReturnMethodSetupReturnBuilder<TReturn, ").Append(typeParams)
			.Append("> Returns(Func<TReturn> callback);")
			.AppendLine();
		sb.AppendLine();

		sb.Append("\t\t/// <summary>").AppendLine();
		sb.Append("\t\t///     Registers the <paramref name=\"returnValue\" /> for this method.").AppendLine();
		sb.Append("\t\t/// </summary>").AppendLine();
		sb.Append("\t\tIReturnMethodSetupReturnBuilder<TReturn, ").Append(typeParams)
			.Append("> Returns(TReturn returnValue);")
			.AppendLine();
		sb.AppendLine();

		sb.Append("\t\t/// <summary>").AppendLine();
		sb.Append("\t\t///     Registers an <typeparamref name=\"TException\" /> to throw when the method is invoked.")
			.AppendLine();
		sb.Append("\t\t/// </summary>").AppendLine();
		sb.Append("\t\tIReturnMethodSetupReturnBuilder<TReturn, ").Append(typeParams).Append("> Throws<TException>()")
			.AppendLine();
		sb.Append("\t\t\twhere TException : Exception, new();").AppendLine();
		sb.AppendLine();

		sb.Append("\t\t/// <summary>").AppendLine();
		sb.Append("\t\t///     Registers an <paramref name=\"exception\" /> to throw when the method is invoked.")
			.AppendLine();
		sb.Append("\t\t/// </summary>").AppendLine();
		sb.Append("\t\tIReturnMethodSetupReturnBuilder<TReturn, ").Append(typeParams)
			.Append("> Throws(Exception exception);")
			.AppendLine();
		sb.AppendLine();

		sb.Append("\t\t/// <summary>").AppendLine();
		sb.Append(
				"\t\t///     Registers a <paramref name=\"callback\" /> that will calculate the exception to throw when the method is invoked.")
			.AppendLine();
		sb.Append("\t\t/// </summary>").AppendLine();
		sb.Append("\t\tIReturnMethodSetupReturnBuilder<TReturn, ").Append(typeParams).Append("> Throws(Func<")
			.Append(typeParams).Append(", Exception> callback);").AppendLine();
		sb.AppendLine();

		sb.Append("\t\t/// <summary>").AppendLine();
		sb.Append(
				"\t\t///     Registers a <paramref name=\"callback\" /> that will calculate the exception to throw when the method is invoked.")
			.AppendLine();
		sb.Append("\t\t/// </summary>").AppendLine();
		sb.Append("\t\tIReturnMethodSetupReturnBuilder<TReturn, ").Append(typeParams)
			.Append("> Throws(Func<Exception> callback);").AppendLine();
		sb.Append("}").AppendLine();
		sb.AppendLine();

		sb.Append("/// <summary>").AppendLine();
		sb.Append("///     Sets up a method with ").Append(numberOfParameters).Append(" parameters ");
		for (int i = 1; i < numberOfParameters - 1; i++)
		{
			sb.Append("<typeparamref name=\"T").Append(i).Append("\" />, ");
		}

		sb.Append("<typeparamref name=\"T").Append(numberOfParameters - 1).Append("\" /> and <typeparamref name=\"T")
			.Append(numberOfParameters).Append("\" /> returning <typeparamref name=\"TReturn\" />.").AppendLine();
		sb.Append("/// </summary>").AppendLine();
		sb.Append("internal interface IReturnMethodSetupCallbackBuilder<TReturn, ").Append(typeParams)
			.Append("> : IReturnMethodSetupCallbackWhenBuilder<TReturn, ").Append(typeParams).Append(">")
			.AppendLine();
		sb.Append("{").AppendLine();
		sb.Append("\t\t/// <summary>").AppendLine();
		sb.Append(
				"\t\t///     Runs the callback in parallel to the other callbacks.")
			.AppendLine();
		sb.Append("\t\t/// </summary>").AppendLine();
		sb.Append("\t\tIReturnMethodSetupCallbackBuilder<TReturn, ").Append(typeParams).Append("> InParallel();")
			.AppendLine();
		sb.AppendLine();
		sb.Append("\t\t/// <summary>").AppendLine();
		sb.Append(
				"\t\t///     Limits the callback to only execute for indexer accesses where the predicate returns true.")
			.AppendLine();
		sb.Append("\t\t/// <summary>").AppendLine();
		sb.Append(
				"\t\t///     Limits the callback to only execute for method invocations where the predicate returns true.")
			.AppendLine();
		sb.Append("\t\t/// </summary>").AppendLine();
		sb.Append("\t\t/// <remarks>").AppendLine();
		sb.Append(
				"\t\t///     Provides a zero-based counter indicating how many times the method has been invoked so far.")
			.AppendLine();
		sb.Append("\t\t/// </remarks>").AppendLine();
		sb.Append("\t\tIReturnMethodSetupCallbackWhenBuilder<TReturn, ").Append(typeParams)
			.Append("> When(Func<int, bool> predicate);").AppendLine();
		sb.Append("}").AppendLine();
		sb.AppendLine();

		sb.Append("/// <summary>").AppendLine();
		sb.Append("///     Sets up a method with ").Append(numberOfParameters).Append(" parameters ");
		for (int i = 1; i < numberOfParameters - 1; i++)
		{
			sb.Append("<typeparamref name=\"T").Append(i).Append("\" />, ");
		}

		sb.Append("<typeparamref name=\"T").Append(numberOfParameters - 1).Append("\" /> and <typeparamref name=\"T")
			.Append(numberOfParameters).Append("\" /> returning <typeparamref name=\"TReturn\" />.").AppendLine();
		sb.Append("/// </summary>").AppendLine();
		sb.Append("internal interface IReturnMethodSetupCallbackWhenBuilder<TReturn, ").Append(typeParams)
			.Append("> : IReturnMethodSetup<TReturn, ").Append(typeParams).Append(">")
			.AppendLine();
		sb.Append("{").AppendLine();
		sb.Append("\t\t/// <summary>").AppendLine();
		sb.Append(
				"\t\t///     Repeats the callback for the given number of <paramref name=\"times\" />.")
			.AppendLine();
		sb.Append("\t\t/// </summary>").AppendLine();
		sb.Append("\t\t/// <remarks>").AppendLine();
		sb.Append(
				"\t\t///     The number of times is only counted for actual executions (<see cref=\"IReturnMethodSetupCallbackBuilder{TReturn, ")
			.Append(typeParams).Append("}.When(Func{int, bool})\" /> evaluates to <see langword=\"true\" />).")
			.AppendLine();
		sb.Append("\t\t/// </remarks>").AppendLine();
		sb.Append("\t\tIReturnMethodSetupCallbackWhenBuilder<TReturn, ").Append(typeParams).Append("> For(int times);")
			.AppendLine();
		sb.AppendLine();
		sb.Append("\t\t/// <summary>").AppendLine();
		sb.Append(
				"\t\t///     Deactivates the callback after the given number of <paramref name=\"times\" />.")
			.AppendLine();
		sb.Append("\t\t/// </summary>").AppendLine();
		sb.Append("\t\t/// <remarks>").AppendLine();
		sb.Append(
				"\t\t///     The number of times is only counted for actual executions (<see cref=\"IReturnMethodSetupCallbackBuilder{TReturn, ")
			.Append(typeParams).Append("}.When(Func{int, bool})\" /> evaluates to <see langword=\"true\" />).")
			.AppendLine();
		sb.Append("\t\t/// </remarks>").AppendLine();
		sb.Append("\t\tIReturnMethodSetup<TReturn, ").Append(typeParams).Append("> Only(int times);").AppendLine();
		sb.Append("}").AppendLine();
		sb.AppendLine();

		sb.Append("/// <summary>").AppendLine();
		sb.Append("///     Sets up a method with ").Append(numberOfParameters).Append(" parameters ");
		for (int i = 1; i < numberOfParameters - 1; i++)
		{
			sb.Append("<typeparamref name=\"T").Append(i).Append("\" />, ");
		}

		sb.Append("<typeparamref name=\"T").Append(numberOfParameters - 1).Append("\" /> and <typeparamref name=\"T")
			.Append(numberOfParameters).Append("\" /> returning <typeparamref name=\"TReturn\" />.").AppendLine();
		sb.Append("/// </summary>").AppendLine();
		sb.Append("internal interface IReturnMethodSetupReturnBuilder<TReturn, ").Append(typeParams)
			.Append("> : IReturnMethodSetupReturnWhenBuilder<TReturn, ").Append(typeParams).Append(">")
			.AppendLine();
		sb.Append("{").AppendLine();
		sb.Append("\t\t/// <summary>").AppendLine();
		sb.Append(
				"\t\t///     Limits the return/throw to only execute for method invocations where the predicate returns true.")
			.AppendLine();
		sb.Append("\t\t/// </summary>").AppendLine();
		sb.Append("\t\t/// <remarks>").AppendLine();
		sb.Append(
				"\t\t///     Provides a zero-based counter indicating how many times the method has been invoked so far.")
			.AppendLine();
		sb.Append("\t\t/// </remarks>").AppendLine();
		sb.Append("\t\tIReturnMethodSetupReturnWhenBuilder<TReturn, ").Append(typeParams)
			.Append("> When(Func<int, bool> predicate);").AppendLine();
		sb.Append("}").AppendLine();
		sb.AppendLine();

		sb.Append("/// <summary>").AppendLine();
		sb.Append("///     Sets up a method with ").Append(numberOfParameters).Append(" parameters ");
		for (int i = 1; i < numberOfParameters - 1; i++)
		{
			sb.Append("<typeparamref name=\"T").Append(i).Append("\" />, ");
		}

		sb.Append("<typeparamref name=\"T").Append(numberOfParameters - 1).Append("\" /> and <typeparamref name=\"T")
			.Append(numberOfParameters).Append("\" /> returning <typeparamref name=\"TReturn\" />.").AppendLine();
		sb.Append("/// </summary>").AppendLine();
		sb.Append("internal interface IReturnMethodSetupReturnWhenBuilder<TReturn, ").Append(typeParams)
			.Append("> : IReturnMethodSetup<TReturn, ").Append(typeParams).Append(">")
			.AppendLine();
		sb.Append("{").AppendLine();
		sb.Append("\t\t/// <summary>").AppendLine();
		sb.Append(
				"\t\t///     Repeats the callback for the given number of <paramref name=\"times\" />.")
			.AppendLine();
		sb.Append("\t\t/// </summary>").AppendLine();
		sb.Append("\t\t/// <remarks>").AppendLine();
		sb.Append(
				"\t\t///     The number of times is only counted for actual executions (<see cref=\"IReturnMethodSetupReturnBuilder{TReturn, ")
			.Append(typeParams).Append("}.When(Func{int, bool})\" /> evaluates to <see langword=\"true\" />).")
			.AppendLine();
		sb.Append("\t\t/// </remarks>").AppendLine();
		sb.Append("\t\tIReturnMethodSetupReturnWhenBuilder<TReturn, ").Append(typeParams).Append("> For(int times);")
			.AppendLine();
		sb.AppendLine();
		sb.Append("\t\t/// <summary>").AppendLine();
		sb.Append(
				"\t\t///     Deactivates the return/throw after the given number of <paramref name=\"times\" />.")
			.AppendLine();
		sb.Append("\t\t/// </summary>").AppendLine();
		sb.Append("\t\t/// <remarks>").AppendLine();
		sb.Append(
				"\t\t///     The number of times is only counted for actual executions (<see cref=\"IReturnMethodSetupReturnBuilder{TReturn, ")
			.Append(typeParams).Append("}.When(Func{int, bool})\" /> evaluates to <see langword=\"true\" />).")
			.AppendLine();
		sb.Append("\t\t/// </remarks>").AppendLine();
		sb.Append("\t\tIReturnMethodSetup<TReturn, ").Append(typeParams).Append("> Only(int times);").AppendLine();
		sb.Append("}").AppendLine();
		sb.AppendLine();


		sb.Append("/// <summary>").AppendLine();
		sb.Append("///     Sets up a method with ").Append(numberOfParameters).Append(" parameters ");
		for (int i = 1; i < numberOfParameters - 1; i++)
		{
			sb.Append("<typeparamref name=\"T").Append(i).Append("\" />, ");
		}

		sb.Append("<typeparamref name=\"T").Append(numberOfParameters - 1).Append("\" /> and <typeparamref name=\"T")
			.Append(numberOfParameters).Append("\" /> returning <typeparamref name=\"TReturn\" />.").AppendLine();
		sb.Append("/// </summary>").AppendLine();
		sb.Append("internal class ReturnMethodSetup<TReturn, ").Append(typeParams)
			.Append("> : MethodSetup,").AppendLine();
		sb.Append("\t\tIReturnMethodSetupCallbackBuilder<TReturn, ").Append(typeParams).Append(">,").AppendLine();
		sb.Append("\t\tIReturnMethodSetupReturnBuilder<TReturn, ").Append(typeParams).Append(">").AppendLine();
		sb.Append("{").AppendLine();
		sb.Append("\t\tprivate readonly List<Callback<Action<int, ").Append(typeParams).Append(">>> _callbacks = [];")
			.AppendLine();
		sb.Append("\t\tprivate readonly List<Callback<Func<int, ").Append(typeParams)
			.Append(", TReturn>>> _returnCallbacks = [];")
			.AppendLine();
		sb.Append("\t\tprivate readonly string _name;").AppendLine();
		sb.Append("\t\tprivate readonly IParameters? _matches;").AppendLine();
		for (int i = 1; i <= numberOfParameters; i++)
		{
			sb.Append("\t\tprivate readonly NamedParameter? _match").Append(i).Append(";").AppendLine();
		}

		sb.Append("\t\tprivate bool? _callBaseClass;").AppendLine();
		sb.Append("\t\tprivate Callback? _currentCallback;").AppendLine();
		sb.Append("\t\tprivate Callback? _currentReturnCallback;").AppendLine();
		sb.Append("\t\tprivate int _currentCallbacksIndex;").AppendLine();
		sb.Append("\t\tprivate int _currentReturnCallbackIndex;").AppendLine();
		sb.AppendLine();

		sb.Append("\t\t/// <inheritdoc cref=\"ReturnMethodSetup{TReturn, ")
			.Append(string.Join(", ", Enumerable.Range(1, numberOfParameters).Select(i => $"T{i}"))).Append("}\" />")
			.AppendLine();
		sb.Append("\t\tpublic ReturnMethodSetup(").AppendLine();
		sb.Append("\t\t\tstring name");
		for (int i = 1; i <= numberOfParameters; i++)
		{
			sb.Append(',').AppendLine().Append("\t\tNamedParameter match").Append(i);
		}

		sb.Append(')').AppendLine();
		sb.Append("\t\t{").AppendLine();
		sb.Append("\t\t\t_name = name;").AppendLine();
		for (int i = 1; i <= numberOfParameters; i++)
		{
			sb.Append("\t\t\t_match").Append(i).Append(" = match").Append(i).Append(";").AppendLine();
		}

		sb.Append("\t\t}").AppendLine();
		sb.AppendLine();

		sb.Append("\t\t/// <inheritdoc cref=\"ReturnMethodSetup{TReturn, ")
			.Append(string.Join(", ", Enumerable.Range(1, numberOfParameters).Select(i => $"T{i}"))).Append("}\" />")
			.AppendLine();
		sb.Append("\t\tpublic ReturnMethodSetup(string name, IParameters matches)").AppendLine();
		sb.Append("\t\t{").AppendLine();
		sb.Append("\t\t\t_name = name;").AppendLine();
		sb.Append("\t\t\t_matches = matches;").AppendLine();
		sb.Append("\t\t}").AppendLine();
		sb.AppendLine();

		sb.Append("\t\t/// <summary>").AppendLine();
		sb.Append(
				"\t\t///     Flag indicating if the base class implementation should be called, and its return values used as default values.")
			.AppendLine();
		sb.Append("\t\t/// </summary>").AppendLine();
		sb.Append("\t\t/// <remarks>").AppendLine();
		sb.Append("\t\t///     If not specified, use <see cref=\"MockBehavior.CallBaseClass\" />.")
			.AppendLine();
		sb.Append("\t\t/// </remarks>").AppendLine();
		sb.Append("\t\tpublic IReturnMethodSetup<TReturn, ").Append(typeParams)
			.Append("> CallingBaseClass(bool callBaseClass = true)")
			.AppendLine();
		sb.Append("\t\t{").AppendLine();
		sb.Append("\t\t\t_callBaseClass = callBaseClass;").AppendLine();
		sb.Append("\t\t\treturn this;").AppendLine();
		sb.Append("\t\t}").AppendLine();
		sb.AppendLine();

		sb.Append("\t\t/// <summary>").AppendLine();
		sb.Append("\t\t///     Registers a <paramref name=\"callback\" /> to execute when the method is called.")
			.AppendLine();
		sb.Append("\t\t/// </summary>").AppendLine();
		sb.Append("\t\tpublic IReturnMethodSetupCallbackBuilder<TReturn, ").Append(typeParams)
			.Append("> Do(Action callback)")
			.AppendLine();
		sb.Append("\t\t{").AppendLine();
		sb.Append("\t\t\tCallback<Action<int, ").Append(typeParams).Append(">>? currentCallback = new((_, ")
			.Append(discards).Append(") => callback());").AppendLine();
		sb.Append("\t\t\t_currentCallback = currentCallback;").AppendLine();
		sb.Append("\t\t\t_callbacks.Add(currentCallback);").AppendLine();
		sb.Append("\t\t\treturn this;").AppendLine();
		sb.Append("\t\t}").AppendLine();
		sb.AppendLine();

		sb.Append("\t\t/// <summary>").AppendLine();
		sb.Append("\t\t///     Registers a <paramref name=\"callback\" /> to execute when the method is called.")
			.AppendLine();
		sb.Append("\t\t/// </summary>").AppendLine();
		sb.Append("\t\tpublic IReturnMethodSetupCallbackBuilder<TReturn, ").Append(typeParams).Append("> Do(Action<")
			.Append(typeParams).Append("> callback)").AppendLine();
		sb.Append("\t\t{").AppendLine();
		sb.Append("\t\t\tCallback<Action<int, ").Append(typeParams).Append(">>? currentCallback = new((_, ")
			.Append(parameters).Append(") => callback(").Append(parameters).Append("));").AppendLine();
		sb.Append("\t\t\t_currentCallback = currentCallback;").AppendLine();
		sb.Append("\t\t\t_callbacks.Add(currentCallback);").AppendLine();
		sb.Append("\t\t\treturn this;").AppendLine();
		sb.Append("\t\t}").AppendLine();
		sb.AppendLine();

		sb.Append("\t\t/// <summary>").AppendLine();
		sb.Append("\t\t///     Registers a <paramref name=\"callback\" /> to execute when the method is called.")
			.AppendLine();
		sb.Append("\t\t/// </summary>").AppendLine();
		sb.Append("\t\tpublic IReturnMethodSetupCallbackBuilder<TReturn, ").Append(typeParams)
			.Append("> Do(Action<int, ")
			.Append(typeParams).Append("> callback)").AppendLine();
		sb.Append("\t\t{").AppendLine();
		sb.Append("\t\t\tCallback<Action<int, ").Append(typeParams).Append(">>? currentCallback = new(callback);")
			.AppendLine();
		sb.Append("\t\t\t_currentCallback = currentCallback;").AppendLine();
		sb.Append("\t\t\t_callbacks.Add(currentCallback);").AppendLine();
		sb.Append("\t\t\treturn this;").AppendLine();
		sb.Append("\t\t}").AppendLine();
		sb.AppendLine();

		sb.Append("\t\t/// <summary>").AppendLine();
		sb.Append("\t\t///     Registers a <paramref name=\"callback\" /> to setup the return value for this method.")
			.AppendLine();
		sb.Append("\t\t/// </summary>").AppendLine();
		sb.Append("\t\tpublic IReturnMethodSetupReturnBuilder<TReturn, ").Append(typeParams).Append("> Returns(Func<")
			.Append(typeParams).Append(", TReturn> callback)").AppendLine();
		sb.Append("\t\t{").AppendLine();
		sb.Append("\t\t\tvar currentCallback = new Callback<Func<int, ").Append(typeParams).Append(", TReturn>>((_, ")
			.Append(parameters)
			.Append(") => callback(").Append(parameters).Append("));").AppendLine();
		sb.Append("\t\t\t_currentReturnCallback = currentCallback;").AppendLine();
		sb.Append("\t\t\t_returnCallbacks.Add(currentCallback);").AppendLine();
		sb.Append("\t\t\treturn this;").AppendLine();
		sb.Append("\t\t}").AppendLine();
		sb.AppendLine();

		sb.Append("\t\t/// <summary>").AppendLine();
		sb.Append("\t\t///     Registers a <paramref name=\"callback\" /> to setup the return value for this method.")
			.AppendLine();
		sb.Append("\t\t/// </summary>").AppendLine();
		sb.Append("\t\tpublic IReturnMethodSetupReturnBuilder<TReturn, ").Append(typeParams)
			.Append("> Returns(Func<TReturn> callback)")
			.AppendLine();
		sb.Append("\t\t{").AppendLine();
		sb.Append("\t\t\tvar currentCallback = new Callback<Func<int, ").Append(typeParams).Append(", TReturn>>((_, ")
			.Append(string.Join(", ", Enumerable.Range(0, numberOfParameters).Select(_ => "_")))
			.Append(") => callback());").AppendLine();
		sb.Append("\t\t\t_currentReturnCallback = currentCallback;").AppendLine();
		sb.Append("\t\t\t_returnCallbacks.Add(currentCallback);").AppendLine();
		sb.Append("\t\t\treturn this;").AppendLine();
		sb.Append("\t\t}").AppendLine();
		sb.AppendLine();

		sb.Append("\t\t/// <summary>").AppendLine();
		sb.Append("\t\t///     Registers the <paramref name=\"returnValue\" /> for this method.").AppendLine();
		sb.Append("\t\t/// </summary>").AppendLine();
		sb.Append("\t\tpublic IReturnMethodSetupReturnBuilder<TReturn, ").Append(typeParams)
			.Append("> Returns(TReturn returnValue)")
			.AppendLine();
		sb.Append("\t\t{").AppendLine();
		sb.Append("\t\t\tvar currentCallback = new Callback<Func<int, ").Append(typeParams).Append(", TReturn>>((_, ")
			.Append(string.Join(", ", Enumerable.Range(0, numberOfParameters).Select(_ => "_")))
			.Append(") => returnValue);").AppendLine();
		sb.Append("\t\t\t_currentReturnCallback = currentCallback;").AppendLine();
		sb.Append("\t\t\t_returnCallbacks.Add(currentCallback);").AppendLine();
		sb.Append("\t\t\treturn this;").AppendLine();
		sb.Append("\t\t}").AppendLine();
		sb.AppendLine();

		sb.Append("\t\t/// <summary>").AppendLine();
		sb.Append("\t\t///     Registers an <typeparamref name=\"TException\" /> to throw when the method is invoked.")
			.AppendLine();
		sb.Append("\t\t/// </summary>").AppendLine();
		sb.Append("\t\tpublic IReturnMethodSetupReturnBuilder<TReturn, ").Append(typeParams)
			.Append("> Throws<TException>()")
			.AppendLine();
		sb.Append("\t\t\twhere TException : Exception, new()").AppendLine();
		sb.Append("\t\t{").AppendLine();
		sb.Append("\t\t\tvar currentCallback = new Callback<Func<int, ").Append(typeParams).Append(", TReturn>>((_, ")
			.Append(string.Join(", ", Enumerable.Range(0, numberOfParameters).Select(_ => "_")))
			.Append(") => throw new TException());").AppendLine();
		sb.Append("\t\t\t_currentReturnCallback = currentCallback;").AppendLine();
		sb.Append("\t\t\t_returnCallbacks.Add(currentCallback);").AppendLine();
		sb.Append("\t\t\treturn this;").AppendLine();
		sb.Append("\t\t}").AppendLine();
		sb.AppendLine();

		sb.Append("\t\t/// <summary>").AppendLine();
		sb.Append("\t\t///     Registers an <paramref name=\"exception\" /> to throw when the method is invoked.")
			.AppendLine();
		sb.Append("\t\t/// </summary>").AppendLine();
		sb.Append("\t\tpublic IReturnMethodSetupReturnBuilder<TReturn, ").Append(typeParams)
			.Append("> Throws(Exception exception)")
			.AppendLine();
		sb.Append("\t\t{").AppendLine();
		sb.Append("\t\t\tvar currentCallback = new Callback<Func<int, ").Append(typeParams).Append(", TReturn>>((_, ")
			.Append(string.Join(", ", Enumerable.Range(0, numberOfParameters).Select(_ => "_")))
			.Append(") => throw exception);").AppendLine();
		sb.Append("\t\t\t_currentReturnCallback = currentCallback;").AppendLine();
		sb.Append("\t\t\t_returnCallbacks.Add(currentCallback);").AppendLine();
		sb.Append("\t\t\treturn this;").AppendLine();
		sb.Append("\t\t}").AppendLine();
		sb.AppendLine();

		sb.Append("\t\t/// <summary>").AppendLine();
		sb.Append(
				"\t\t///     Registers a <paramref name=\"callback\" /> that will calculate the exception to throw when the method is invoked.")
			.AppendLine();
		sb.Append("\t\t/// </summary>").AppendLine();
		sb.Append("\t\tpublic IReturnMethodSetupReturnBuilder<TReturn, ").Append(typeParams).Append("> Throws(Func<")
			.Append(typeParams).Append(", Exception> callback)").AppendLine();
		sb.Append("\t\t{").AppendLine();
		sb.Append("\t\t\tvar currentCallback = new Callback<Func<int, ").Append(typeParams).Append(", TReturn>>((_, ")
			.Append(parameters)
			.Append(") => throw callback(").Append(parameters)
			.Append("));").AppendLine();
		sb.Append("\t\t\t_currentReturnCallback = currentCallback;").AppendLine();
		sb.Append("\t\t\t_returnCallbacks.Add(currentCallback);").AppendLine();
		sb.Append("\t\t\treturn this;").AppendLine();
		sb.Append("\t\t}").AppendLine();
		sb.AppendLine();

		sb.Append("\t\t/// <summary>").AppendLine();
		sb.Append(
				"\t\t///     Registers a <paramref name=\"callback\" /> that will calculate the exception to throw when the method is invoked.")
			.AppendLine();
		sb.Append("\t\t/// </summary>").AppendLine();
		sb.Append("\t\tpublic IReturnMethodSetupReturnBuilder<TReturn, ").Append(typeParams)
			.Append("> Throws(Func<Exception> callback)")
			.AppendLine();
		sb.Append("\t\t{").AppendLine();
		sb.Append("\t\t\tvar currentCallback = new Callback<Func<int, ").Append(typeParams).Append(", TReturn>>((_, ")
			.Append(string.Join(", ", Enumerable.Range(0, numberOfParameters).Select(_ => "_")))
			.Append(") => throw callback());").AppendLine();
		sb.Append("\t\t\t_currentReturnCallback = currentCallback;").AppendLine();
		sb.Append("\t\t\t_returnCallbacks.Add(currentCallback);").AppendLine();
		sb.Append("\t\t\treturn this;").AppendLine();
		sb.Append("\t\t}").AppendLine();
		sb.AppendLine();

		sb.Append("\t\t/// <inheritdoc cref=\"IReturnMethodSetupCallbackBuilder{TReturn, ").Append(typeParams)
			.Append("}.InParallel()\" />").AppendLine();
		sb.Append("\t\tIReturnMethodSetupCallbackBuilder<TReturn, ").Append(typeParams)
			.Append("> IReturnMethodSetupCallbackBuilder<TReturn, ").Append(typeParams)
			.Append(">.InParallel()").AppendLine();
		sb.Append("\t\t{").AppendLine();
		sb.Append("\t\t\t_currentCallback?.InParallel();").AppendLine();
		sb.Append("\t\t\treturn this;").AppendLine();
		sb.Append("\t\t}").AppendLine();
		sb.AppendLine();

		sb.Append("\t\t/// <inheritdoc cref=\"IReturnMethodSetupCallbackBuilder{TReturn, ").Append(typeParams)
			.Append("}.When(Func{int, bool})\" />").AppendLine();
		sb.Append("\t\tIReturnMethodSetupCallbackWhenBuilder<TReturn, ").Append(typeParams)
			.Append("> IReturnMethodSetupCallbackBuilder<TReturn, ").Append(typeParams)
			.Append(">.When(Func<int, bool> predicate)").AppendLine();
		sb.Append("\t\t{").AppendLine();
		sb.Append("\t\t\t_currentCallback?.When(predicate);").AppendLine();
		sb.Append("\t\t\treturn this;").AppendLine();
		sb.Append("\t\t}").AppendLine();
		sb.AppendLine();

		sb.Append("\t\t/// <inheritdoc cref=\"IReturnMethodSetupCallbackWhenBuilder{TReturn, ").Append(typeParams)
			.Append("}.For(int)\" />").AppendLine();
		sb.Append("\t\tIReturnMethodSetupCallbackWhenBuilder<TReturn, ").Append(typeParams)
			.Append("> IReturnMethodSetupCallbackWhenBuilder<TReturn, ").Append(typeParams)
			.Append(">.For(int times)").AppendLine();
		sb.Append("\t\t{").AppendLine();
		sb.Append("\t\t\t_currentCallback?.For(times);").AppendLine();
		sb.Append("\t\t\treturn this;").AppendLine();
		sb.Append("\t\t}").AppendLine();
		sb.AppendLine();

		sb.Append("\t\t/// <inheritdoc cref=\"IReturnMethodSetupCallbackWhenBuilder{TReturn, ").Append(typeParams)
			.Append("}.Only(int)\" />").AppendLine();
		sb.Append("\t\tIReturnMethodSetup<TReturn, ").Append(typeParams)
			.Append("> IReturnMethodSetupCallbackWhenBuilder<TReturn, ").Append(typeParams)
			.Append(">.Only(int times)").AppendLine();
		sb.Append("\t\t{").AppendLine();
		sb.Append("\t\t\t_currentCallback?.Only(times);").AppendLine();
		sb.Append("\t\t\treturn this;").AppendLine();
		sb.Append("\t\t}").AppendLine();
		sb.AppendLine();

		sb.Append("\t\t/// <inheritdoc cref=\"IReturnMethodSetupReturnBuilder{TReturn, ").Append(typeParams)
			.Append("}.When(Func{int, bool})\" />").AppendLine();
		sb.Append("\t\tIReturnMethodSetupReturnWhenBuilder<TReturn, ").Append(typeParams)
			.Append("> IReturnMethodSetupReturnBuilder<TReturn, ").Append(typeParams)
			.Append(">.When(Func<int, bool> predicate)").AppendLine();
		sb.Append("\t\t{").AppendLine();
		sb.Append("\t\t\t_currentReturnCallback?.When(predicate);").AppendLine();
		sb.Append("\t\t\treturn this;").AppendLine();
		sb.Append("\t\t}").AppendLine();
		sb.AppendLine();

		sb.Append("\t\t/// <inheritdoc cref=\"IReturnMethodSetupReturnWhenBuilder{TReturn, ").Append(typeParams)
			.Append("}.For(int)\" />").AppendLine();
		sb.Append("\t\tIReturnMethodSetupReturnWhenBuilder<TReturn, ").Append(typeParams)
			.Append("> IReturnMethodSetupReturnWhenBuilder<TReturn, ").Append(typeParams)
			.Append(">.For(int times)").AppendLine();
		sb.Append("\t\t{").AppendLine();
		sb.Append("\t\t\t_currentReturnCallback?.For(times);").AppendLine();
		sb.Append("\t\t\treturn this;").AppendLine();
		sb.Append("\t\t}").AppendLine();
		sb.AppendLine();

		sb.Append("\t\t/// <inheritdoc cref=\"IReturnMethodSetupReturnWhenBuilder{TReturn, ").Append(typeParams)
			.Append("}.Only(int)\" />").AppendLine();
		sb.Append("\t\tIReturnMethodSetup<TReturn, ").Append(typeParams)
			.Append("> IReturnMethodSetupReturnWhenBuilder<TReturn, ").Append(typeParams)
			.Append(">.Only(int times)").AppendLine();
		sb.Append("\t\t{").AppendLine();
		sb.Append("\t\t\t_currentReturnCallback?.Only(times);").AppendLine();
		sb.Append("\t\t\treturn this;").AppendLine();
		sb.Append("\t\t}").AppendLine();
		sb.AppendLine();

		sb.Append("\t\t/// <inheritdoc cref=\"MethodSetup.ExecuteCallback(MethodInvocation, MockBehavior)\" />")
			.AppendLine();
		sb.Append("\t\tprotected override void ExecuteCallback(MethodInvocation invocation, MockBehavior behavior)")
			.AppendLine();
		sb.Append("\t\t{").AppendLine();
		sb.Append("\t\t\tif (");
		for (int i = 1; i < numberOfParameters; i++)
		{
			sb.Append("TryCast(invocation.Parameters[").Append(i - 1).Append("], out T").Append(i).Append(" p")
				.Append(i).Append(", behavior) &&");
			sb.AppendLine().Append("\t\t    ");
		}

		sb.Append("TryCast(invocation.Parameters[")
			.Append(numberOfParameters - 1).Append("], out T").Append(numberOfParameters).Append(" p")
			.Append(numberOfParameters).Append(", behavior))")
			.AppendLine();
		sb.Append("\t\t\t{").AppendLine();
		sb.Append("\t\t\t\tbool wasInvoked = false;").AppendLine();
		sb.Append("\t\t\t\tint currentCallbacksIndex = _currentCallbacksIndex;").AppendLine();
		sb.Append("\t\t\t\tfor (int i = 0; i < _callbacks.Count; i++)").AppendLine();
		sb.Append("\t\t\t\t{").AppendLine();
		sb.Append("\t\t\t\t\tCallback<Action<int, ").Append(typeParams).Append(">>? callback =").AppendLine();
		sb.Append("\t\t\t\t\t\t_callbacks[(currentCallbacksIndex + i) % _callbacks.Count];").AppendLine();
		sb.Append("\t\t\t\t\tif (callback.Invoke(wasInvoked, ref _currentCallbacksIndex, (invocationCount, @delegate)")
			.AppendLine();
		sb.Append("\t\t\t\t\t=> @delegate(invocationCount, ")
			.Append(string.Join(", ", Enumerable.Range(1, numberOfParameters).Select(x => $"p{x}"))).Append(")))")
			.AppendLine();
		sb.Append("\t\t\t\t\t{").AppendLine();
		sb.Append("\t\t\t\t\t\twasInvoked = true;").AppendLine();
		sb.Append("\t\t\t\t\t}").AppendLine();
		sb.Append("\t\t\t\t}").AppendLine();
		sb.Append("\t\t\t}").AppendLine();
		sb.Append("\t\t}").AppendLine();
		sb.AppendLine();

		sb.Append(
				"\t\t/// <inheritdoc cref=\"MethodSetup.GetReturnValue{TResult}(MethodInvocation, MockBehavior, Func{TResult})\" />")
			.AppendLine();
		sb.Append(
				"\t\tprotected override TResult GetReturnValue<TResult>(MethodInvocation invocation, MockBehavior behavior, Func<TResult> defaultValueGenerator)")
			.AppendLine();
		sb.Append("\t\t\twhere TResult : default").AppendLine();
		sb.Append("\t\t{").AppendLine();
		for (int i = 1; i <= numberOfParameters; i++)
		{
			sb.Append("\t\t\tif (!TryCast(invocation.Parameters[").Append(i - 1)
				.Append("], out T").Append(i).Append(" p").Append(i).Append(", behavior))").AppendLine();
			sb.Append("\t\t\t{").AppendLine();
			sb.Append("\t\t\t\tthrow new MockException($\"The input parameter ").Append(i)
				.Append(" only supports '{FormatType(typeof(T").Append(i)
				.Append("))}', but is '{FormatType(invocation.Parameters[")
				.Append(i - 1).Append("]!.GetType())}'.\");").AppendLine();
			sb.Append("\t\t\t}").AppendLine();
			sb.AppendLine();
		}

		sb.Append("\t\t\tforeach (var _ in _returnCallbacks)").AppendLine();
		sb.Append("\t\t\t{").AppendLine();
		sb.Append(
				"\t\t\t\tvar returnCallback = _returnCallbacks[_currentReturnCallbackIndex % _returnCallbacks.Count];")
			.AppendLine();
		sb.Append(
				"\t\t\t\tif (returnCallback.Invoke<TReturn>(ref _currentReturnCallbackIndex, (invocationCount, @delegate)")
			.AppendLine();
		sb.Append("\t\t\t\t\t=> @delegate(invocationCount, ")
			.Append(string.Join(", ", Enumerable.Range(1, numberOfParameters).Select(x => $"p{x}")))
			.Append("), out TReturn? newValue))").AppendLine();
		sb.Append("\t\t\t\t{").AppendLine();
		sb.Append("\t\t\t\t\tif (newValue is null)").AppendLine();
		sb.Append("\t\t\t\t\t{").AppendLine();
		sb.Append("\t\t\t\t\t\treturn default!;").AppendLine();
		sb.Append("\t\t\t\t\t}").AppendLine();
		sb.AppendLine();
		sb.Append("\t\t\t\t\tif (!TryCast(newValue, out TResult returnValue, behavior))").AppendLine();
		sb.Append("\t\t\t\t\t{").AppendLine();
		sb.Append(
				"\t\t\t\t\t\tthrow new MockException($\"The return callback only supports '{FormatType(typeof(TReturn))}' and not '{FormatType(typeof(TResult))}'.\");")
			.AppendLine();
		sb.Append("\t\t\t\t\t}").AppendLine();
		sb.AppendLine();
		sb.Append("\t\t\t\t\treturn returnValue;").AppendLine();
		sb.Append("\t\t\t\t}").AppendLine();
		sb.Append("\t\t\t}").AppendLine();
		sb.AppendLine();
		sb.Append("\t\t\treturn defaultValueGenerator();").AppendLine();
		sb.Append("\t\t}").AppendLine();
		sb.AppendLine();

		sb.Append("\t\t/// <inheritdoc cref=\"MethodSetup.IsMatch(MethodInvocation)\" />").AppendLine();
		sb.Append("\t\tprotected override bool IsMatch(MethodInvocation invocation)").AppendLine();
		sb.Append("\t\t\t=> invocation.Name.Equals(_name) &&").AppendLine();
		sb.Append("\t\t\t\t(_matches is not null").AppendLine();
		sb.Append("\t\t\t\t\t? _matches.Matches(invocation.Parameters)").AppendLine();
		sb.Append("\t\t\t\t\t: Matches([")
			.Append(string.Join(", ", Enumerable.Range(1, numberOfParameters).Select(x => $"_match{x}!")))
			.Append("], invocation.Parameters));").AppendLine();
		sb.AppendLine();

		sb.Append("\t\t/// <inheritdoc cref=\"MethodSetup.TriggerParameterCallbacks(object?[])\" />").AppendLine();
		sb.Append("\t\tprotected override void TriggerParameterCallbacks(object?[] parameters)").AppendLine();
		sb.Append("\t\t\t=> TriggerCallbacks([")
			.Append(string.Join(", ", Enumerable.Range(1, numberOfParameters).Select(x => $"_match{x}")))
			.Append("], parameters);").AppendLine();
		sb.AppendLine();

		sb.Append("\t\t/// <inheritdoc cref=\"MethodSetup.GetCallBaseClass()\" />").AppendLine();
		sb.Append("\t\tprotected override bool? GetCallBaseClass()").AppendLine();
		sb.Append("\t\t\t=> _callBaseClass;").AppendLine();
		sb.AppendLine();

		sb.Append("\t\t/// <inheritdoc cref=\"MethodSetup.HasReturnCalls()\" />").AppendLine();
		sb.Append("\t\tprotected override bool HasReturnCalls()").AppendLine();
		sb.Append("\t\t\t=> _returnCallbacks.Count > 0;").AppendLine();
		sb.AppendLine();

		sb.Append("\t\t/// <inheritdoc cref=\"MethodSetup.SetOutParameter{T}(string, Func{T})\" />").AppendLine();
		sb.Append("\t\tprotected override T SetOutParameter<T>(string parameterName, Func<T> defaultValueGenerator)")
			.AppendLine();
		sb.Append("\t\t{").AppendLine();
		sb.Append("\t\t\tif (")
			.Append(string.Join(" && ", Enumerable.Range(1, numberOfParameters).Select(x => $"_match{x} is not null")))
			.Append(" &&").AppendLine();
		sb.Append("\t\t\t\tHasOutParameter([")
			.Append(string.Join(", ", Enumerable.Range(1, numberOfParameters).Select(x => $"_match{x}")))
			.Append("], parameterName, out IOutParameter<T>? outParameter))").AppendLine();
		sb.Append("\t\t\t{").AppendLine();
		sb.Append("\t\t\t\treturn outParameter.GetValue(defaultValueGenerator);").AppendLine();
		sb.Append("\t\t\t}").AppendLine();
		sb.AppendLine();
		sb.Append("\t\t\treturn defaultValueGenerator();").AppendLine();
		sb.Append("\t\t}").AppendLine();
		sb.AppendLine();

		sb.Append("\t\t/// <inheritdoc cref=\"MethodSetup.SetRefParameter{T}(string, T, MockBehavior)\" />")
			.AppendLine();
		sb.Append("\t\tprotected override T SetRefParameter<T>(string parameterName, T value, MockBehavior behavior)")
			.AppendLine();
		sb.Append("\t\t{").AppendLine();
		sb.Append("\t\t\tif (")
			.Append(string.Join(" && ", Enumerable.Range(1, numberOfParameters).Select(x => $"_match{x} is not null")))
			.Append(" &&").AppendLine();
		sb.Append("\t\t\t\tHasRefParameter([")
			.Append(string.Join(", ", Enumerable.Range(1, numberOfParameters).Select(x => $"_match{x}")))
			.Append("], parameterName, out IRefParameter<T>? refParameter))").AppendLine();
		sb.Append("\t\t\t{").AppendLine();
		sb.Append("\t\t\t\treturn refParameter.GetValue(value);").AppendLine();
		sb.Append("\t\t\t}").AppendLine();
		sb.AppendLine();
		sb.Append("\t\t\treturn value;").AppendLine();
		sb.Append("\t\t}").AppendLine();
		sb.AppendLine();

		sb.Append("\t\t/// <inheritdoc cref=\"object.ToString()\" />").AppendLine();
		sb.Append("\t\tpublic override string ToString()").AppendLine();
		sb.Append("\t\t{").AppendLine();
		sb.Append("\t\t\tif (_matches is not null)").AppendLine();
		sb.Append("\t\t\t{").AppendLine();
		sb.Append("\t\t\t\treturn $\"{FormatType(typeof(TReturn))} {_name}({_matches})\";").AppendLine();
		sb.Append("\t\t\t}").AppendLine();
		sb.AppendLine();
		sb.Append("\t\t\treturn $\"{FormatType(typeof(TReturn))} {_name}(")
			.Append(string.Join(", ", Enumerable.Range(1, numberOfParameters).Select(x => $"{{_match{x}}}")))
			.Append(")\";").AppendLine();
		sb.Append("\t\t}").AppendLine();
		sb.Append("\t}").AppendLine();
		sb.AppendLine();
	}
}
