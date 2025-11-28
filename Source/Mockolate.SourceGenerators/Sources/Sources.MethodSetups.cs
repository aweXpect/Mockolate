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
		]);

		sb.Append("""
		          namespace Mockolate.Setup;

		          #nullable enable

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

		sb.AppendLine("#nullable disable");
		return sb.ToString();
	}

	internal static string MethodSetupsActionFunc(IEnumerable<int> parameterCounts)
	{
		StringBuilder sb = InitializeBuilder([]);

		sb.AppendLine("""
		              namespace System;

		              #nullable enable

		              """);
		foreach (int parameterCount in parameterCounts)
		{
			sb.AppendLine($$"""
			                /// <summary>
			                ///     Encapsulates a method that has {{parameterCount}} parameters and does not return a value.
			                /// </summary>
			                public delegate void Action<{{string.Join(", ", Enumerable.Range(1, parameterCount).Select(i => $"in T{i}"))}}>({{string.Join(", ", Enumerable.Range(1, parameterCount).Select(i => $"T{i} arg{i}"))}});
			                """);
			sb.AppendLine();
			sb.AppendLine($$"""
			                /// <summary>
			                ///     Encapsulates a method that has {{parameterCount}} parameters and returns a value of the type specified by the <typeparamref name="TResult" /> parameter.
			                /// </summary>
			                public delegate TResult Func<{{string.Join(", ", Enumerable.Range(1, parameterCount).Select(i => $"in T{i}"))}}, out TResult>({{string.Join(", ", Enumerable.Range(1, parameterCount).Select(i => $"T{i} arg{i}"))}});
			                """);
			sb.AppendLine();
		}

		sb.AppendLine("#nullable disable");
		return sb.ToString();
	}

	private static void AppendVoidMethodSetup(StringBuilder sb, int numberOfParameters)
	{
		string typeParams = string.Join(", ", Enumerable.Range(1, numberOfParameters).Select(i => $"T{i}"));
		string parameters = string.Join(", ", Enumerable.Range(1, numberOfParameters).Select(i => $"p{i}"));
		string discards = string.Join(", ", Enumerable.Range(1, numberOfParameters).Select(_ => "_"));

		sb.Append("/// <summary>").AppendLine();
		sb.Append("///     Sets up a method with ").Append(numberOfParameters).Append(" parameters ");
		for (int i = 1; i < numberOfParameters - 1; i++)
		{
			sb.Append("<typeparamref name=\"T").Append(i).Append("\" />, ");
		}

		sb.Append("<typeparamref name=\"T").Append(numberOfParameters - 1).Append("\" /> and <typeparamref name=\"T")
			.Append(numberOfParameters).Append("\" /> returning <see langword=\"void\" />.").AppendLine();
		sb.Append("/// </summary>").AppendLine();
		sb.Append("internal interface IVoidMethodSetup<").Append(typeParams).Append(">").AppendLine();
		sb.Append("{").AppendLine();
		sb.Append("\t/// <summary>").AppendLine();
		sb.Append(
				"\t///     Flag indicating if the base class implementation should be called, and its return values used as default values.")
			.AppendLine();
		sb.Append("\t/// </summary>").AppendLine();
		sb.Append("\t/// <remarks>").AppendLine();
		sb.Append("\t///     If not specified, use <see cref=\"MockBehavior.CallBaseClass\" />.")
			.AppendLine();
		sb.Append("\t/// </remarks>").AppendLine();
		sb.Append("\tIVoidMethodSetup<").Append(typeParams).Append("> CallingBaseClass(bool callBaseClass = true);")
			.AppendLine();
		sb.AppendLine();

		sb.Append("\t/// <summary>").AppendLine();
		sb.Append("\t///     Registers a <paramref name=\"callback\" /> to execute when the method is called.")
			.AppendLine();
		sb.Append("\t/// </summary>").AppendLine();
		sb.Append("\tIVoidMethodSetupCallbackBuilder<").Append(typeParams).Append("> Do(Action callback);")
			.AppendLine();
		sb.AppendLine();

		sb.Append("\t/// <summary>").AppendLine();
		sb.Append("\t///     Registers a <paramref name=\"callback\" /> to execute when the method is called.")
			.AppendLine();
		sb.Append("\t/// </summary>").AppendLine();
		sb.Append("\tIVoidMethodSetupCallbackBuilder<").Append(typeParams).Append("> Do(Action<").Append(typeParams)
			.Append("> callback);").AppendLine();
		sb.AppendLine();

		sb.Append("\t/// <summary>").AppendLine();
		sb.Append("\t///     Registers a <paramref name=\"callback\" /> to execute when the method is called.")
			.AppendLine();
		sb.Append("\t/// </summary>").AppendLine();
		sb.Append("\tIVoidMethodSetupCallbackBuilder<").Append(typeParams).Append("> Do(Action<int, ")
			.Append(typeParams)
			.Append("> callback);").AppendLine();
		sb.AppendLine();

		sb.Append("\t/// <summary>").AppendLine();
		sb.Append("\t///     Registers an iteration in the sequence of method invocations, that does not throw.")
			.AppendLine();
		sb.Append("\t/// </summary>").AppendLine();
		sb.Append("\tIVoidMethodSetupReturnBuilder<").Append(typeParams).Append("> DoesNotThrow();").AppendLine();
		sb.AppendLine();

		sb.Append("\t/// <summary>").AppendLine();
		sb.Append("\t///     Registers an <typeparamref name=\"TException\" /> to throw when the method is invoked.")
			.AppendLine();
		sb.Append("\t/// </summary>").AppendLine();
		sb.Append("\tIVoidMethodSetupReturnBuilder<").Append(typeParams).Append("> Throws<TException>()")
			.AppendLine();
		sb.Append("\t\twhere TException : Exception, new();").AppendLine();
		sb.AppendLine();

		sb.Append("\t/// <summary>").AppendLine();
		sb.Append("\t///     Registers an <paramref name=\"exception\" /> to throw when the method is invoked.")
			.AppendLine();
		sb.Append("\t/// </summary>").AppendLine();
		sb.Append("\tIVoidMethodSetupReturnBuilder<").Append(typeParams).Append("> Throws(Exception exception);").AppendLine();
		sb.AppendLine();

		sb.Append("\t/// <summary>").AppendLine();
		sb.Append(
				"\t///     Registers a <paramref name=\"callback\" /> that will calculate the exception to throw when the method is invoked.")
			.AppendLine();
		sb.Append("\t/// </summary>").AppendLine();
		sb.Append("\tIVoidMethodSetupReturnBuilder<").Append(typeParams).Append("> Throws(Func<")
			.Append(typeParams).Append(", Exception> callback);").AppendLine();
		sb.AppendLine();

		sb.Append("\t/// <summary>").AppendLine();
		sb.Append(
				"\t///     Registers a <paramref name=\"callback\" /> that will calculate the exception to throw when the method is invoked.")
			.AppendLine();
		sb.Append("\t/// </summary>").AppendLine();
		sb.Append("\tIVoidMethodSetupReturnBuilder<").Append(typeParams).Append("> Throws(Func<Exception> callback);").AppendLine();
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

		sb.Append("\t/// <inheritdoc cref=\"IVoidMethodSetupCallbackBuilder{").Append(typeParams)
			.Append("}.When(Func{int, bool})\" />").AppendLine();
		sb.Append("\tIVoidMethodSetupCallbackWhenBuilder<").Append(typeParams)
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
		sb.Append("\t/// <inheritdoc cref=\"IVoidMethodSetupCallbackWhenBuilder{").Append(typeParams)
			.Append("}.For(int)\" />").AppendLine();
		sb.Append("\tIVoidMethodSetup<").Append(typeParams).Append("> For(int times);").AppendLine();
		sb.Append("}").AppendLine();
		sb.AppendLine();
		

		sb.Append("/// <summary>").AppendLine();
		sb.Append("///     Sets up a return callback for a method with ").Append(numberOfParameters).Append(" parameters ");
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

		sb.Append("\t/// <inheritdoc cref=\"IVoidMethodSetupReturnBuilder{").Append(typeParams)
			.Append("}.When(Func{int, bool})\" />").AppendLine();
		sb.Append("\tIVoidMethodSetupReturnWhenBuilder<").Append(typeParams)
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
		sb.Append("\t/// <inheritdoc cref=\"IVoidMethodSetupReturnWhenBuilder{").Append(typeParams)
			.Append("}.For(int)\" />").AppendLine();
		sb.Append("\tIVoidMethodSetup<").Append(typeParams).Append("> For(int times);").AppendLine();
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
		sb.Append("\tIVoidMethodSetupCallbackBuilder<").Append(typeParams).Append(">,").AppendLine();
		sb.Append("\tIVoidMethodSetupReturnBuilder<").Append(typeParams).Append(">").AppendLine();
		sb.Append("{").AppendLine();
		sb.Append("\tprivate readonly List<Callback<Action<int, ").Append(typeParams).Append(">>> _callbacks = [];")
			.AppendLine();
		sb.Append("\tprivate readonly List<Callback<Action<int, ").Append(typeParams).Append(">>> _returnCallbacks = [];")
			.AppendLine();
		sb.Append("\tprivate readonly string _name;").AppendLine();
		sb.Append("\tprivate readonly Match.IParameters? _matches;").AppendLine();
		for (int i = 1; i <= numberOfParameters; i++)
		{
			sb.Append("\tprivate readonly Match.NamedParameter? _match").Append(i).Append(";").AppendLine();
		}

		sb.Append("\tprivate bool? _callBaseClass;").AppendLine();
		sb.Append("\tprivate Callback? _currentCallback;").AppendLine();
		sb.Append("\tprivate Callback? _currentReturnCallback;").AppendLine();
		sb.Append("\tprivate int _currentReturnCallbackIndex;").AppendLine();
		sb.AppendLine();

		sb.Append("\t/// <inheritdoc cref=\"VoidMethodSetup{")
			.Append(string.Join(", ", Enumerable.Range(1, numberOfParameters).Select(i => $"T{i}"))).Append("}\" />")
			.AppendLine();
		sb.Append("\tpublic VoidMethodSetup(").AppendLine();
		sb.Append("\t\tstring name");
		for (int i = 1; i <= numberOfParameters; i++)
		{
			sb.Append(',').AppendLine().Append("\t\tMatch.NamedParameter match").Append(i);
		}

		sb.Append(')').AppendLine();
		sb.Append("\t{").AppendLine();
		sb.Append("\t\t_name = name;").AppendLine();
		for (int i = 1; i <= numberOfParameters; i++)
		{
			sb.Append("\t\t_match").Append(i).Append(" = match").Append(i).Append(";").AppendLine();
		}

		sb.Append("\t}").AppendLine();
		sb.AppendLine();

		sb.Append("\t/// <inheritdoc cref=\"VoidMethodSetup{")
			.Append(string.Join(", ", Enumerable.Range(1, numberOfParameters).Select(i => $"T{i}"))).Append("}\" />")
			.AppendLine();
		sb.Append("\tpublic VoidMethodSetup(string name, Match.IParameters matches)").AppendLine();
		sb.Append("\t{").AppendLine();
		sb.Append("\t\t_name = name;").AppendLine();
		sb.Append("\t\t_matches = matches;").AppendLine();
		sb.Append("\t}").AppendLine();
		sb.AppendLine();

		sb.Append("\t/// <summary>").AppendLine();
		sb.Append(
				"\t///     Flag indicating if the base class implementation should be called, and its return values used as default values.")
			.AppendLine();
		sb.Append("\t/// </summary>").AppendLine();
		sb.Append("\t/// <remarks>").AppendLine();
		sb.Append("\t///     If not specified, use <see cref=\"MockBehavior.CallBaseClass\" />.")
			.AppendLine();
		sb.Append("\t/// </remarks>").AppendLine();
		sb.Append("\tpublic IVoidMethodSetup<").Append(typeParams)
			.Append("> CallingBaseClass(bool callBaseClass = true)").AppendLine();
		sb.Append("\t{").AppendLine();
		sb.Append("\t\t_callBaseClass = callBaseClass;").AppendLine();
		sb.Append("\t\treturn this;").AppendLine();
		sb.Append("\t}").AppendLine();
		sb.AppendLine();

		sb.Append("\t/// <summary>").AppendLine();
		sb.Append("\t///     Registers a <paramref name=\"callback\" /> to execute when the method is called.")
			.AppendLine();
		sb.Append("\t/// </summary>").AppendLine();
		sb.Append("\tpublic IVoidMethodSetupCallbackBuilder<").Append(typeParams).Append("> Do(Action callback)")
			.AppendLine();
		sb.Append("\t{").AppendLine();
		sb.Append("\t\tCallback<Action<int, ").Append(typeParams).Append(">>? currentCallback = new((_, ")
			.Append(discards).Append(") => callback());").AppendLine();
		sb.Append("\t\t_currentCallback = currentCallback;").AppendLine();
		sb.Append("\t\t_callbacks.Add(currentCallback);").AppendLine();
		sb.Append("\t\treturn this;").AppendLine();
		sb.Append("\t}").AppendLine();
		sb.AppendLine();

		sb.Append("\t/// <summary>").AppendLine();
		sb.Append("\t///     Registers a <paramref name=\"callback\" /> to execute when the method is called.")
			.AppendLine();
		sb.Append("\t/// </summary>").AppendLine();
		sb.Append("\tpublic IVoidMethodSetupCallbackBuilder<").Append(typeParams).Append("> Do(Action<")
			.Append(typeParams)
			.Append("> callback)").AppendLine();
		sb.Append("\t{").AppendLine();
		sb.Append("\t\tCallback<Action<int, ").Append(typeParams).Append(">>? currentCallback = new((_, ")
			.Append(parameters).Append(") => callback(").Append(parameters).Append("));").AppendLine();
		sb.Append("\t\t_currentCallback = currentCallback;").AppendLine();
		sb.Append("\t\t_callbacks.Add(currentCallback);").AppendLine();
		sb.Append("\t\treturn this;").AppendLine();
		sb.Append("\t}").AppendLine();
		sb.AppendLine();

		sb.Append("\t/// <summary>").AppendLine();
		sb.Append("\t///     Registers a <paramref name=\"callback\" /> to execute when the method is called.")
			.AppendLine();
		sb.Append("\t/// </summary>").AppendLine();
		sb.Append("\tpublic IVoidMethodSetupCallbackBuilder<").Append(typeParams).Append("> Do(Action<int, ")
			.Append(typeParams)
			.Append("> callback)").AppendLine();
		sb.Append("\t{").AppendLine();
		sb.Append("\t\tCallback<Action<int, ").Append(typeParams).Append(">>? currentCallback = new(callback);")
			.AppendLine();
		sb.Append("\t\t_currentCallback = currentCallback;").AppendLine();
		sb.Append("\t\t_callbacks.Add(currentCallback);").AppendLine();
		sb.Append("\t\treturn this;").AppendLine();
		sb.Append("\t}").AppendLine();
		sb.AppendLine();

		sb.Append("\t/// <summary>").AppendLine();
		sb.Append("\t///     Registers an iteration in the sequence of method invocations, that does not throw.")
			.AppendLine();
		sb.Append("\t/// </summary>").AppendLine();
		sb.Append("\tpublic IVoidMethodSetupReturnBuilder<").Append(typeParams).Append("> DoesNotThrow()")
			.AppendLine();
		sb.Append("\t{").AppendLine();
		sb.Append("\t\tvar currentCallback = new Callback<Action<int, ").Append(typeParams).Append(">>((_, ")
			.Append(string.Join(", ", Enumerable.Range(0, numberOfParameters).Select(_ => "_")))
			.Append(") => { });").AppendLine();
		sb.Append("\t\t_currentReturnCallback = currentCallback;").AppendLine();
		sb.Append("\t\t_returnCallbacks.Add(currentCallback);").AppendLine();
		sb.Append("\t\treturn this;").AppendLine();
		sb.Append("\t}").AppendLine();
		sb.AppendLine();

		sb.Append("\t/// <summary>").AppendLine();
		sb.Append("\t///     Registers an <typeparamref name=\"TException\" /> to throw when the method is invoked.")
			.AppendLine();
		sb.Append("\t/// </summary>").AppendLine();
		sb.Append("\tpublic IVoidMethodSetupReturnBuilder<").Append(typeParams).Append("> Throws<TException>()")
			.AppendLine();
		sb.Append("\t\twhere TException : Exception, new()").AppendLine();
		sb.Append("\t{").AppendLine();
		sb.Append("\t\tvar currentCallback = new Callback<Action<int, ").Append(typeParams).Append(">>((_, ")
			.Append(string.Join(", ", Enumerable.Range(0, numberOfParameters).Select(_ => "_")))
			.Append(") => throw new TException());").AppendLine();
		sb.Append("\t\t_currentReturnCallback = currentCallback;").AppendLine();
		sb.Append("\t\t_returnCallbacks.Add(currentCallback);").AppendLine();
		sb.Append("\t\treturn this;").AppendLine();
		sb.Append("\t}").AppendLine();
		sb.AppendLine();

		sb.Append("\t/// <summary>").AppendLine();
		sb.Append("\t///     Registers an <paramref name=\"exception\" /> to throw when the method is invoked.")
			.AppendLine();
		sb.Append("\t/// </summary>").AppendLine();
		sb.Append("\tpublic IVoidMethodSetupReturnBuilder<").Append(typeParams).Append("> Throws(Exception exception)")
			.AppendLine();
		sb.Append("\t{").AppendLine();
		sb.Append("\t\tvar currentCallback = new Callback<Action<int, ").Append(typeParams).Append(">>((_, ")
			.Append(string.Join(", ", Enumerable.Range(0, numberOfParameters).Select(_ => "_")))
			.Append(") => throw exception);").AppendLine();
		sb.Append("\t\t_currentReturnCallback = currentCallback;").AppendLine();
		sb.Append("\t\t_returnCallbacks.Add(currentCallback);").AppendLine();
		sb.Append("\t\treturn this;").AppendLine();
		sb.Append("\t}").AppendLine();
		sb.AppendLine();

		sb.Append("\t/// <summary>").AppendLine();
		sb.Append(
				"\t///     Registers a <paramref name=\"callback\" /> that will calculate the exception to throw when the method is invoked.")
			.AppendLine();
		sb.Append("\t/// </summary>").AppendLine();
		sb.Append("\tpublic IVoidMethodSetupReturnBuilder<").Append(typeParams).Append("> Throws(Func<")
			.Append(typeParams).Append(", Exception> callback)").AppendLine();
		sb.Append("\t{").AppendLine();
		sb.Append("\t\tvar currentCallback = new Callback<Action<int, ").Append(typeParams).Append(">>((_, ").Append(parameters).Append(") => throw callback(").Append(parameters).Append("));").AppendLine();
		sb.Append("\t\t_currentReturnCallback = currentCallback;").AppendLine();
		sb.Append("\t\t_returnCallbacks.Add(currentCallback);").AppendLine();
		sb.Append("\t\treturn this;").AppendLine();
		sb.Append("\t}").AppendLine();
		sb.AppendLine();

		sb.Append("\t/// <summary>").AppendLine();
		sb.Append(
				"\t///     Registers a <paramref name=\"callback\" /> that will calculate the exception to throw when the method is invoked.")
			.AppendLine();
		sb.Append("\t/// </summary>").AppendLine();
		sb.Append("\tpublic IVoidMethodSetupReturnBuilder<").Append(typeParams).Append("> Throws(Func<Exception> callback)")
			.AppendLine();
		sb.Append("\t{").AppendLine();
		sb.Append("\t\tvar currentCallback = new Callback<Action<int, ").Append(typeParams).Append(">>((_, ")
			.Append(string.Join(", ", Enumerable.Range(0, numberOfParameters).Select(_ => "_")))
			.Append(") => throw callback());").AppendLine();
		sb.Append("\t\t_currentReturnCallback = currentCallback;").AppendLine();
		sb.Append("\t\t_returnCallbacks.Add(currentCallback);").AppendLine();
		sb.Append("\t\treturn this;").AppendLine();
		sb.Append("\t}").AppendLine();
		sb.AppendLine();

		sb.Append("\t/// <inheritdoc cref=\"IVoidMethodSetupCallbackBuilder{").Append(typeParams)
			.Append("}.When(Func{int, bool})\" />").AppendLine();
		sb.Append("\tIVoidMethodSetupCallbackWhenBuilder<").Append(typeParams)
			.Append("> IVoidMethodSetupCallbackBuilder<").Append(typeParams)
			.Append(">.When(Func<int, bool> predicate)").AppendLine();
		sb.Append("\t{").AppendLine();
		sb.Append("\t\t_currentCallback?.When(predicate);").AppendLine();
		sb.Append("\t\treturn this;").AppendLine();
		sb.Append("\t}").AppendLine();
		sb.AppendLine();

		sb.Append("\t/// <inheritdoc cref=\"IVoidMethodSetupCallbackWhenBuilder{").Append(typeParams)
			.Append("}.For(int)\" />").AppendLine();
		sb.Append("\tIVoidMethodSetup<").Append(typeParams).Append("> IVoidMethodSetupCallbackWhenBuilder<").Append(typeParams)
			.Append(">.For(int times)").AppendLine();
		sb.Append("\t{").AppendLine();
		sb.Append("\t\t_currentCallback?.For(x => x < times);").AppendLine();
		sb.Append("\t\treturn this;").AppendLine();
		sb.Append("\t}").AppendLine();
		sb.AppendLine();

		sb.Append("\t/// <inheritdoc cref=\"IVoidMethodSetupReturnBuilder{").Append(typeParams)
			.Append("}.When(Func{int, bool})\" />").AppendLine();
		sb.Append("\tIVoidMethodSetupReturnWhenBuilder<").Append(typeParams)
			.Append("> IVoidMethodSetupReturnBuilder<").Append(typeParams)
			.Append(">.When(Func<int, bool> predicate)").AppendLine();
		sb.Append("\t{").AppendLine();
		sb.Append("\t\t_currentReturnCallback?.When(predicate);").AppendLine();
		sb.Append("\t\treturn this;").AppendLine();
		sb.Append("\t}").AppendLine();
		sb.AppendLine();

		sb.Append("\t/// <inheritdoc cref=\"IVoidMethodSetupReturnWhenBuilder{").Append(typeParams)
			.Append("}.For(int)\" />").AppendLine();
		sb.Append("\tIVoidMethodSetup<").Append(typeParams).Append("> IVoidMethodSetupReturnWhenBuilder<").Append(typeParams)
			.Append(">.For(int times)").AppendLine();
		sb.Append("\t{").AppendLine();
		sb.Append("\t\t_currentReturnCallback?.For(x => x < times);").AppendLine();
		sb.Append("\t\treturn this;").AppendLine();
		sb.Append("\t}").AppendLine();
		sb.AppendLine();

		sb.Append("\t/// <inheritdoc cref=\"MethodSetup.ExecuteCallback(MethodInvocation, MockBehavior)\" />")
			.AppendLine();
		sb.Append("\tprotected override void ExecuteCallback(MethodInvocation invocation, MockBehavior behavior)")
			.AppendLine();
		sb.Append("\t{").AppendLine();
		sb.Append("\t\tif (");
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
		sb.Append("\t\t{").AppendLine();
		sb.Append("\t\t\t_callbacks.ForEach(callback => callback.Invoke((invocationCount, @delegate)").AppendLine();
		sb.Append("\t\t\t\t=> @delegate(invocationCount, ")
			.Append(string.Join(", ", Enumerable.Range(1, numberOfParameters).Select(x => $"p{x}"))).Append(")));")
			.AppendLine();
		sb.Append("\t\t\tforeach (var _ in _returnCallbacks)").AppendLine();
		sb.Append("\t\t\t{").AppendLine();
		sb.Append("\t\t\t\tvar returnCallback = _returnCallbacks[_currentReturnCallbackIndex % _returnCallbacks.Count];").AppendLine();
		sb.Append("\t\t\t\tif (returnCallback.Invoke(ref _currentReturnCallbackIndex, (invocationCount, @delegate)").AppendLine();
		sb.Append("\t\t\t\t\t=> @delegate(invocationCount, ")
			.Append(string.Join(", ", Enumerable.Range(1, numberOfParameters).Select(x => $"p{x}"))).Append(")))").AppendLine();
		sb.Append("\t\t\t\t{").AppendLine();
		sb.Append("\t\t\t\t\treturn;").AppendLine();
		sb.Append("\t\t\t\t}").AppendLine();
		sb.Append("\t\t\t}").AppendLine();
		sb.Append("\t\t}").AppendLine();
		sb.Append("\t}").AppendLine();
		sb.AppendLine();

		sb.Append("\t/// <inheritdoc cref=\"MethodSetup.GetReturnValue{TResult}(MethodInvocation, MockBehavior, Func{TResult})\" />")
			.AppendLine();
		sb.Append(
				"\tprotected override TResult GetReturnValue<TResult>(MethodInvocation invocation, MockBehavior behavior, Func<TResult> defaultValueGenerator)")
			.AppendLine();
		sb.Append("\t\twhere TResult : default").AppendLine();
		sb.Append("\t\t=> throw new MockException(\"The method setup does not support return values.\");").AppendLine();
		sb.AppendLine();

		sb.Append("\t/// <inheritdoc cref=\"MethodSetup.IsMatch(MethodInvocation)\" />").AppendLine();
		sb.Append("\tprotected override bool IsMatch(MethodInvocation invocation)").AppendLine();
		sb.Append("\t\t=> invocation.Name.Equals(_name) &&").AppendLine();
		sb.Append("\t\t\t(_matches is not null").AppendLine();
		sb.Append("\t\t\t\t? _matches.Matches(invocation.Parameters)").AppendLine();
		sb.Append("\t\t\t\t: Matches([")
			.Append(string.Join(", ", Enumerable.Range(1, numberOfParameters).Select(x => $"_match{x}!")))
			.Append("], invocation.Parameters));").AppendLine();
		sb.AppendLine();

		sb.Append("\t/// <inheritdoc cref=\"MethodSetup.TriggerParameterCallbacks(object?[])\" />").AppendLine();
		sb.Append("\tprotected override void TriggerParameterCallbacks(object?[] parameters)").AppendLine();
		sb.Append("\t\t=> TriggerCallbacks([")
			.Append(string.Join(", ", Enumerable.Range(1, numberOfParameters).Select(x => $"_match{x}")))
			.Append("], parameters);").AppendLine();
		sb.AppendLine();

		sb.Append("\t/// <inheritdoc cref=\"MethodSetup.GetCallBaseClass()\" />").AppendLine();
		sb.Append("\tprotected override bool? GetCallBaseClass()").AppendLine();
		sb.Append("\t\t=> _callBaseClass;").AppendLine();
		sb.AppendLine();

		sb.Append("\t/// <inheritdoc cref=\"MethodSetup.HasReturnCalls()\" />").AppendLine();
		sb.Append("\tprotected override bool HasReturnCalls()").AppendLine();
		sb.Append("\t\t=> _returnCallbacks.Count > 0;").AppendLine();
		sb.AppendLine();

		sb.Append("\t/// <inheritdoc cref=\"MethodSetup.SetOutParameter{T}(string, Func{T})\" />").AppendLine();
		sb.Append("\tprotected override T SetOutParameter<T>(string parameterName, Func<T> defaultValueGenerator)")
			.AppendLine();
		sb.Append("\t{").AppendLine();
		sb.Append("\t\tif (")
			.Append(string.Join(" && ", Enumerable.Range(1, numberOfParameters).Select(x => $"_match{x} is not null")))
			.Append(" &&").AppendLine();
		sb.Append("\t\t\tHasOutParameter([")
			.Append(string.Join(", ", Enumerable.Range(1, numberOfParameters).Select(x => $"_match{x}")))
			.Append("], parameterName, out Match.IOutParameter<T>? outParameter))").AppendLine();
		sb.Append("\t\t{").AppendLine();
		sb.Append("\t\t\treturn outParameter.GetValue(defaultValueGenerator);").AppendLine();
		sb.Append("\t\t}").AppendLine();
		sb.AppendLine();
		sb.Append("\t\treturn defaultValueGenerator();").AppendLine();
		sb.Append("\t}").AppendLine();
		sb.AppendLine();

		sb.Append("\t/// <inheritdoc cref=\"MethodSetup.SetRefParameter{T}(string, T, MockBehavior)\" />").AppendLine();
		sb.Append("\tprotected override T SetRefParameter<T>(string parameterName, T value, MockBehavior behavior)")
			.AppendLine();
		sb.Append("\t{").AppendLine();
		sb.Append("\t\tif (")
			.Append(string.Join(" && ", Enumerable.Range(1, numberOfParameters).Select(x => $"_match{x} is not null")))
			.Append(" &&").AppendLine();
		sb.Append("\t\t\tHasRefParameter([")
			.Append(string.Join(", ", Enumerable.Range(1, numberOfParameters).Select(x => $"_match{x}")))
			.Append("], parameterName, out Match.IRefParameter<T>? refParameter))").AppendLine();
		sb.Append("\t\t{").AppendLine();
		sb.Append("\t\t\treturn refParameter.GetValue(value);").AppendLine();
		sb.Append("\t\t}").AppendLine();
		sb.AppendLine();
		sb.Append("\t\treturn value;").AppendLine();
		sb.Append("\t}").AppendLine();
		sb.AppendLine();

		sb.Append("\t/// <inheritdoc cref=\"object.ToString()\" />").AppendLine();
		sb.Append("\tpublic override string ToString()").AppendLine();
		sb.Append("\t{").AppendLine();
		sb.Append("\t\tif (_matches is not null)").AppendLine();
		sb.Append("\t\t{").AppendLine();
		sb.Append("\t\t\treturn $\"void {_name}({_matches})\";").AppendLine();
		sb.Append("\t\t}").AppendLine();
		sb.AppendLine();
		sb.Append("\t\treturn $\"void {_name}(")
			.Append(string.Join(", ", Enumerable.Range(1, numberOfParameters).Select(x => $"{{_match{x}}}")))
			.Append(")\";").AppendLine();
		sb.Append("\t}").AppendLine();
		sb.Append("}").AppendLine();
		sb.AppendLine();
	}

	private static void AppendReturnMethodSetup(StringBuilder sb, int numberOfParameters)
	{
		string typeParams = string.Join(", ", Enumerable.Range(1, numberOfParameters).Select(i => $"T{i}"));
		string parameters = string.Join(", ", Enumerable.Range(1, numberOfParameters).Select(i => $"p{i}"));
		string discards = string.Join(", ", Enumerable.Range(1, numberOfParameters).Select(_ => "_"));

		sb.Append("/// <summary>").AppendLine();
		sb.Append("///     Sets up a method with ").Append(numberOfParameters).Append(" parameters ");
		for (int i = 1; i < numberOfParameters - 1; i++)
		{
			sb.Append("<typeparamref name=\"T").Append(i).Append("\" />, ");
		}

		sb.Append("<typeparamref name=\"T").Append(numberOfParameters - 1).Append("\" /> and <typeparamref name=\"T")
			.Append(numberOfParameters).Append("\" /> returning <typeparamref name=\"TReturn\" />.").AppendLine();
		sb.Append("/// </summary>").AppendLine();
		sb.Append("internal interface IReturnMethodSetup<TReturn, ").Append(typeParams).Append(">")
			.AppendLine();
		sb.Append("{").AppendLine();
		sb.Append("\t/// <summary>").AppendLine();
		sb.Append(
				"\t///     Flag indicating if the base class implementation should be called, and its return values used as default values.")
			.AppendLine();
		sb.Append("\t/// </summary>").AppendLine();
		sb.Append("\t/// <remarks>").AppendLine();
		sb.Append("\t///     If not specified, use <see cref=\"MockBehavior.CallBaseClass\" />.")
			.AppendLine();
		sb.Append("\t/// </remarks>").AppendLine();
		sb.Append("\tIReturnMethodSetup<TReturn, ").Append(typeParams)
			.Append("> CallingBaseClass(bool callBaseClass = true);").AppendLine();
		sb.AppendLine();

		sb.Append("\t/// <summary>").AppendLine();
		sb.Append("\t///     Registers a <paramref name=\"callback\" /> to execute when the method is called.")
			.AppendLine();
		sb.Append("\t/// </summary>").AppendLine();
		sb.Append("\tIReturnMethodSetupCallbackBuilder<TReturn, ").Append(typeParams).Append("> Do(Action callback);")
			.AppendLine();
		sb.AppendLine();

		sb.Append("\t/// <summary>").AppendLine();
		sb.Append("\t///     Registers a <paramref name=\"callback\" /> to execute when the method is called.")
			.AppendLine();
		sb.Append("\t/// </summary>").AppendLine();
		sb.Append("\tIReturnMethodSetupCallbackBuilder<TReturn, ").Append(typeParams).Append("> Do(Action<")
			.Append(typeParams).Append("> callback);").AppendLine();
		sb.AppendLine();

		sb.Append("\t/// <summary>").AppendLine();
		sb.Append("\t///     Registers a <paramref name=\"callback\" /> to execute when the method is called.")
			.AppendLine();
		sb.Append("\t/// </summary>").AppendLine();
		sb.Append("\tIReturnMethodSetupCallbackBuilder<TReturn, ").Append(typeParams).Append("> Do(Action<int, ")
			.Append(typeParams).Append("> callback);").AppendLine();
		sb.AppendLine();

		sb.Append("\t/// <summary>").AppendLine();
		sb.Append("\t///     Registers a <paramref name=\"callback\" /> to setup the return value for this method.")
			.AppendLine();
		sb.Append("\t/// </summary>").AppendLine();
		sb.Append("\tIReturnMethodSetupReturnBuilder<TReturn, ").Append(typeParams).Append("> Returns(Func<")
			.Append(typeParams).Append(", TReturn> callback);").AppendLine();
		sb.AppendLine();

		sb.Append("\t/// <summary>").AppendLine();
		sb.Append("\t///     Registers a <paramref name=\"callback\" /> to setup the return value for this method.")
			.AppendLine();
		sb.Append("\t/// </summary>").AppendLine();
		sb.Append("\tIReturnMethodSetupReturnBuilder<TReturn, ").Append(typeParams).Append("> Returns(Func<TReturn> callback);")
			.AppendLine();
		sb.AppendLine();

		sb.Append("\t/// <summary>").AppendLine();
		sb.Append("\t///     Registers the <paramref name=\"returnValue\" /> for this method.").AppendLine();
		sb.Append("\t/// </summary>").AppendLine();
		sb.Append("\tIReturnMethodSetupReturnBuilder<TReturn, ").Append(typeParams).Append("> Returns(TReturn returnValue);")
			.AppendLine();
		sb.AppendLine();

		sb.Append("\t/// <summary>").AppendLine();
		sb.Append("\t///     Registers an <typeparamref name=\"TException\" /> to throw when the method is invoked.")
			.AppendLine();
		sb.Append("\t/// </summary>").AppendLine();
		sb.Append("\tIReturnMethodSetupReturnBuilder<TReturn, ").Append(typeParams).Append("> Throws<TException>()")
			.AppendLine();
		sb.Append("\t\twhere TException : Exception, new();").AppendLine();
		sb.AppendLine();

		sb.Append("\t/// <summary>").AppendLine();
		sb.Append("\t///     Registers an <paramref name=\"exception\" /> to throw when the method is invoked.")
			.AppendLine();
		sb.Append("\t/// </summary>").AppendLine();
		sb.Append("\tIReturnMethodSetupReturnBuilder<TReturn, ").Append(typeParams).Append("> Throws(Exception exception);")
			.AppendLine();
		sb.AppendLine();

		sb.Append("\t/// <summary>").AppendLine();
		sb.Append(
				"\t///     Registers a <paramref name=\"callback\" /> that will calculate the exception to throw when the method is invoked.")
			.AppendLine();
		sb.Append("\t/// </summary>").AppendLine();
		sb.Append("\tIReturnMethodSetupReturnBuilder<TReturn, ").Append(typeParams).Append("> Throws(Func<")
			.Append(typeParams).Append(", Exception> callback);").AppendLine();
		sb.AppendLine();

		sb.Append("\t/// <summary>").AppendLine();
		sb.Append(
				"\t///     Registers a <paramref name=\"callback\" /> that will calculate the exception to throw when the method is invoked.")
			.AppendLine();
		sb.Append("\t/// </summary>").AppendLine();
		sb.Append("\tIReturnMethodSetupReturnBuilder<TReturn, ").Append(typeParams)
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
		sb.Append("\t/// <inheritdoc cref=\"IReturnMethodSetupCallbackBuilder{TReturn, ").Append(typeParams)
			.Append("}.When(Func{int, bool})\" />").AppendLine();
		sb.Append("\tIReturnMethodSetupCallbackWhenBuilder<TReturn, ").Append(typeParams)
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
		sb.Append("\t/// <inheritdoc cref=\"IReturnMethodSetupCallbackWhenBuilder{TReturn, ").Append(typeParams)
			.Append("}.For(int)\" />").AppendLine();
		sb.Append("\tIReturnMethodSetup<TReturn, ").Append(typeParams).Append("> For(int times);").AppendLine();
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
		sb.Append("\t/// <inheritdoc cref=\"IReturnMethodSetupReturnBuilder{TReturn, ").Append(typeParams)
			.Append("}.When(Func{int, bool})\" />").AppendLine();
		sb.Append("\tIReturnMethodSetupReturnWhenBuilder<TReturn, ").Append(typeParams)
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
		sb.Append("\t/// <inheritdoc cref=\"IReturnMethodSetupReturnWhenBuilder{TReturn, ").Append(typeParams)
			.Append("}.For(int)\" />").AppendLine();
		sb.Append("\tIReturnMethodSetup<TReturn, ").Append(typeParams).Append("> For(int times);").AppendLine();
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
		sb.Append("\tIReturnMethodSetupCallbackBuilder<TReturn, ").Append(typeParams).Append(">,").AppendLine();
		sb.Append("\tIReturnMethodSetupReturnBuilder<TReturn, ").Append(typeParams).Append(">").AppendLine();
		sb.Append("{").AppendLine();
		sb.Append("\tprivate readonly List<Callback<Action<int, ").Append(typeParams).Append(">>> _callbacks = [];")
			.AppendLine();
		sb.Append("\tprivate readonly List<Callback<Func<int, ").Append(typeParams).Append(", TReturn>>> _returnCallbacks = [];")
			.AppendLine();
		sb.Append("\tprivate readonly string _name;").AppendLine();
		sb.Append("\tprivate readonly Match.IParameters? _matches;").AppendLine();
		for (int i = 1; i <= numberOfParameters; i++)
		{
			sb.Append("\tprivate readonly Match.NamedParameter? _match").Append(i).Append(";").AppendLine();
		}

		sb.Append("\tprivate bool? _callBaseClass;").AppendLine();
		sb.Append("\tprivate Callback? _currentCallback;").AppendLine();
		sb.Append("\tprivate Callback? _currentReturnCallback;").AppendLine();
		sb.Append("\tprivate int _currentReturnCallbackIndex;").AppendLine();
		sb.AppendLine();

		sb.Append("\t/// <inheritdoc cref=\"ReturnMethodSetup{TReturn, ")
			.Append(string.Join(", ", Enumerable.Range(1, numberOfParameters).Select(i => $"T{i}"))).Append("}\" />")
			.AppendLine();
		sb.Append("\tpublic ReturnMethodSetup(").AppendLine();
		sb.Append("\t\tstring name");
		for (int i = 1; i <= numberOfParameters; i++)
		{
			sb.Append(',').AppendLine().Append("\t\tMatch.NamedParameter match").Append(i);
		}

		sb.Append(')').AppendLine();
		sb.Append("\t{").AppendLine();
		sb.Append("\t\t_name = name;").AppendLine();
		for (int i = 1; i <= numberOfParameters; i++)
		{
			sb.Append("\t\t_match").Append(i).Append(" = match").Append(i).Append(";").AppendLine();
		}

		sb.Append("\t}").AppendLine();
		sb.AppendLine();

		sb.Append("\t/// <inheritdoc cref=\"ReturnMethodSetup{TReturn, ")
			.Append(string.Join(", ", Enumerable.Range(1, numberOfParameters).Select(i => $"T{i}"))).Append("}\" />")
			.AppendLine();
		sb.Append("\tpublic ReturnMethodSetup(string name, Match.IParameters matches)").AppendLine();
		sb.Append("\t{").AppendLine();
		sb.Append("\t\t_name = name;").AppendLine();
		sb.Append("\t\t_matches = matches;").AppendLine();
		sb.Append("\t}").AppendLine();
		sb.AppendLine();

		sb.Append("\t/// <summary>").AppendLine();
		sb.Append(
				"\t///     Flag indicating if the base class implementation should be called, and its return values used as default values.")
			.AppendLine();
		sb.Append("\t/// </summary>").AppendLine();
		sb.Append("\t/// <remarks>").AppendLine();
		sb.Append("\t///     If not specified, use <see cref=\"MockBehavior.CallBaseClass\" />.")
			.AppendLine();
		sb.Append("\t/// </remarks>").AppendLine();
		sb.Append("\tpublic IReturnMethodSetup<TReturn, ").Append(typeParams)
			.Append("> CallingBaseClass(bool callBaseClass = true)")
			.AppendLine();
		sb.Append("\t{").AppendLine();
		sb.Append("\t\t_callBaseClass = callBaseClass;").AppendLine();
		sb.Append("\t\treturn this;").AppendLine();
		sb.Append("\t}").AppendLine();
		sb.AppendLine();

		sb.Append("\t/// <summary>").AppendLine();
		sb.Append("\t///     Registers a <paramref name=\"callback\" /> to execute when the method is called.")
			.AppendLine();
		sb.Append("\t/// </summary>").AppendLine();
		sb.Append("\tpublic IReturnMethodSetupCallbackBuilder<TReturn, ").Append(typeParams)
			.Append("> Do(Action callback)")
			.AppendLine();
		sb.Append("\t{").AppendLine();
		sb.Append("\t\tCallback<Action<int, ").Append(typeParams).Append(">>? currentCallback = new((_, ")
			.Append(discards).Append(") => callback());").AppendLine();
		sb.Append("\t\t_currentCallback = currentCallback;").AppendLine();
		sb.Append("\t\t_callbacks.Add(currentCallback);").AppendLine();
		sb.Append("\t\treturn this;").AppendLine();
		sb.Append("\t}").AppendLine();
		sb.AppendLine();

		sb.Append("\t/// <summary>").AppendLine();
		sb.Append("\t///     Registers a <paramref name=\"callback\" /> to execute when the method is called.")
			.AppendLine();
		sb.Append("\t/// </summary>").AppendLine();
		sb.Append("\tpublic IReturnMethodSetupCallbackBuilder<TReturn, ").Append(typeParams).Append("> Do(Action<")
			.Append(typeParams).Append("> callback)").AppendLine();
		sb.Append("\t{").AppendLine();
		sb.Append("\t\tCallback<Action<int, ").Append(typeParams).Append(">>? currentCallback = new((_, ")
			.Append(parameters).Append(") => callback(").Append(parameters).Append("));").AppendLine();
		sb.Append("\t\t_currentCallback = currentCallback;").AppendLine();
		sb.Append("\t\t_callbacks.Add(currentCallback);").AppendLine();
		sb.Append("\t\treturn this;").AppendLine();
		sb.Append("\t}").AppendLine();
		sb.AppendLine();

		sb.Append("\t/// <summary>").AppendLine();
		sb.Append("\t///     Registers a <paramref name=\"callback\" /> to execute when the method is called.")
			.AppendLine();
		sb.Append("\t/// </summary>").AppendLine();
		sb.Append("\tpublic IReturnMethodSetupCallbackBuilder<TReturn, ").Append(typeParams).Append("> Do(Action<int, ")
			.Append(typeParams).Append("> callback)").AppendLine();
		sb.Append("\t{").AppendLine();
		sb.Append("\t\tCallback<Action<int, ").Append(typeParams).Append(">>? currentCallback = new(callback);")
			.AppendLine();
		sb.Append("\t\t_currentCallback = currentCallback;").AppendLine();
		sb.Append("\t\t_callbacks.Add(currentCallback);").AppendLine();
		sb.Append("\t\treturn this;").AppendLine();
		sb.Append("\t}").AppendLine();
		sb.AppendLine();

		sb.Append("\t/// <summary>").AppendLine();
		sb.Append("\t///     Registers a <paramref name=\"callback\" /> to setup the return value for this method.")
			.AppendLine();
		sb.Append("\t/// </summary>").AppendLine();
		sb.Append("\tpublic IReturnMethodSetupReturnBuilder<TReturn, ").Append(typeParams).Append("> Returns(Func<")
			.Append(typeParams).Append(", TReturn> callback)").AppendLine();
		sb.Append("\t{").AppendLine();
		sb.Append("\t\tvar currentCallback = new Callback<Func<int, ").Append(typeParams).Append(", TReturn>>((_, ")
			.Append(parameters)
			.Append(") => callback(").Append(parameters).Append("));").AppendLine();
		sb.Append("\t\t_currentReturnCallback = currentCallback;").AppendLine();
		sb.Append("\t\t_returnCallbacks.Add(currentCallback);").AppendLine();
		sb.Append("\t\treturn this;").AppendLine();
		sb.Append("\t}").AppendLine();
		sb.AppendLine();

		sb.Append("\t/// <summary>").AppendLine();
		sb.Append("\t///     Registers a <paramref name=\"callback\" /> to setup the return value for this method.")
			.AppendLine();
		sb.Append("\t/// </summary>").AppendLine();
		sb.Append("\tpublic IReturnMethodSetupReturnBuilder<TReturn, ").Append(typeParams)
			.Append("> Returns(Func<TReturn> callback)")
			.AppendLine();
		sb.Append("\t{").AppendLine();
		sb.Append("\t\tvar currentCallback = new Callback<Func<int, ").Append(typeParams).Append(", TReturn>>((_, ")
			.Append(string.Join(", ", Enumerable.Range(0, numberOfParameters).Select(_ => "_")))
			.Append(") => callback());").AppendLine();
		sb.Append("\t\t_currentReturnCallback = currentCallback;").AppendLine();
		sb.Append("\t\t_returnCallbacks.Add(currentCallback);").AppendLine();
		sb.Append("\t\treturn this;").AppendLine();
		sb.Append("\t}").AppendLine();
		sb.AppendLine();

		sb.Append("\t/// <summary>").AppendLine();
		sb.Append("\t///     Registers the <paramref name=\"returnValue\" /> for this method.").AppendLine();
		sb.Append("\t/// </summary>").AppendLine();
		sb.Append("\tpublic IReturnMethodSetupReturnBuilder<TReturn, ").Append(typeParams).Append("> Returns(TReturn returnValue)")
			.AppendLine();
		sb.Append("\t{").AppendLine();
		sb.Append("\t\tvar currentCallback = new Callback<Func<int, ").Append(typeParams).Append(", TReturn>>((_, ")
			.Append(string.Join(", ", Enumerable.Range(0, numberOfParameters).Select(_ => "_")))
			.Append(") => returnValue);").AppendLine();
		sb.Append("\t\t_currentReturnCallback = currentCallback;").AppendLine();
		sb.Append("\t\t_returnCallbacks.Add(currentCallback);").AppendLine();
		sb.Append("\t\treturn this;").AppendLine();
		sb.Append("\t}").AppendLine();
		sb.AppendLine();

		sb.Append("\t/// <summary>").AppendLine();
		sb.Append("\t///     Registers an <typeparamref name=\"TException\" /> to throw when the method is invoked.")
			.AppendLine();
		sb.Append("\t/// </summary>").AppendLine();
		sb.Append("\tpublic IReturnMethodSetupReturnBuilder<TReturn, ").Append(typeParams).Append("> Throws<TException>()")
			.AppendLine();
		sb.Append("\t\twhere TException : Exception, new()").AppendLine();
		sb.Append("\t{").AppendLine();
		sb.Append("\t\tvar currentCallback = new Callback<Func<int, ").Append(typeParams).Append(", TReturn>>((_, ")
			.Append(string.Join(", ", Enumerable.Range(0, numberOfParameters).Select(_ => "_")))
			.Append(") => throw new TException());").AppendLine();
		sb.Append("\t\t_currentReturnCallback = currentCallback;").AppendLine();
		sb.Append("\t\t_returnCallbacks.Add(currentCallback);").AppendLine();
		sb.Append("\t\treturn this;").AppendLine();
		sb.Append("\t}").AppendLine();
		sb.AppendLine();

		sb.Append("\t/// <summary>").AppendLine();
		sb.Append("\t///     Registers an <paramref name=\"exception\" /> to throw when the method is invoked.")
			.AppendLine();
		sb.Append("\t/// </summary>").AppendLine();
		sb.Append("\tpublic IReturnMethodSetupReturnBuilder<TReturn, ").Append(typeParams).Append("> Throws(Exception exception)")
			.AppendLine();
		sb.Append("\t{").AppendLine();
		sb.Append("\t\tvar currentCallback = new Callback<Func<int, ").Append(typeParams).Append(", TReturn>>((_, ")
			.Append(string.Join(", ", Enumerable.Range(0, numberOfParameters).Select(_ => "_")))
			.Append(") => throw exception);").AppendLine();
		sb.Append("\t\t_currentReturnCallback = currentCallback;").AppendLine();
		sb.Append("\t\t_returnCallbacks.Add(currentCallback);").AppendLine();
		sb.Append("\t\treturn this;").AppendLine();
		sb.Append("\t}").AppendLine();
		sb.AppendLine();

		sb.Append("\t/// <summary>").AppendLine();
		sb.Append(
				"\t///     Registers a <paramref name=\"callback\" /> that will calculate the exception to throw when the method is invoked.")
			.AppendLine();
		sb.Append("\t/// </summary>").AppendLine();
		sb.Append("\tpublic IReturnMethodSetupReturnBuilder<TReturn, ").Append(typeParams).Append("> Throws(Func<")
			.Append(typeParams).Append(", Exception> callback)").AppendLine();
		sb.Append("\t{").AppendLine();
		sb.Append("\t\tvar currentCallback = new Callback<Func<int, ").Append(typeParams).Append(", TReturn>>((_, ")
			.Append(parameters)
			.Append(") => throw callback(").Append(parameters)
			.Append("));").AppendLine();
		sb.Append("\t\t_currentReturnCallback = currentCallback;").AppendLine();
		sb.Append("\t\t_returnCallbacks.Add(currentCallback);").AppendLine();
		sb.Append("\t\treturn this;").AppendLine();
		sb.Append("\t}").AppendLine();
		sb.AppendLine();

		sb.Append("\t/// <summary>").AppendLine();
		sb.Append(
				"\t///     Registers a <paramref name=\"callback\" /> that will calculate the exception to throw when the method is invoked.")
			.AppendLine();
		sb.Append("\t/// </summary>").AppendLine();
		sb.Append("\tpublic IReturnMethodSetupReturnBuilder<TReturn, ").Append(typeParams)
			.Append("> Throws(Func<Exception> callback)")
			.AppendLine();
		sb.Append("\t{").AppendLine();
		sb.Append("\t\tvar currentCallback = new Callback<Func<int, ").Append(typeParams).Append(", TReturn>>((_, ")
			.Append(string.Join(", ", Enumerable.Range(0, numberOfParameters).Select(_ => "_")))
			.Append(") => throw callback());").AppendLine();
		sb.Append("\t\t_currentReturnCallback = currentCallback;").AppendLine();
		sb.Append("\t\t_returnCallbacks.Add(currentCallback);").AppendLine();
		sb.Append("\t\treturn this;").AppendLine();
		sb.Append("\t}").AppendLine();
		sb.AppendLine();

		sb.Append("\t/// <inheritdoc cref=\"IReturnMethodSetupCallbackBuilder{TReturn, ").Append(typeParams)
			.Append("}.When(Func{int, bool})\" />").AppendLine();
		sb.Append("\tIReturnMethodSetupCallbackWhenBuilder<TReturn, ").Append(typeParams)
			.Append("> IReturnMethodSetupCallbackBuilder<TReturn, ").Append(typeParams)
			.Append(">.When(Func<int, bool> predicate)").AppendLine();
		sb.Append("\t{").AppendLine();
		sb.Append("\t\t_currentCallback?.When(predicate);").AppendLine();
		sb.Append("\t\treturn this;").AppendLine();
		sb.Append("\t}").AppendLine();
		sb.AppendLine();

		sb.Append("\t/// <inheritdoc cref=\"IReturnMethodSetupCallbackWhenBuilder{TReturn, ").Append(typeParams)
			.Append("}.For(int)\" />").AppendLine();
		sb.Append("\tIReturnMethodSetup<TReturn, ").Append(typeParams).Append("> IReturnMethodSetupCallbackWhenBuilder<TReturn, ").Append(typeParams)
			.Append(">.For(int times)").AppendLine();
		sb.Append("\t{").AppendLine();
		sb.Append("\t\t_currentCallback?.For(x => x < times);").AppendLine();
		sb.Append("\t\treturn this;").AppendLine();
		sb.Append("\t}").AppendLine();
		sb.AppendLine();
		
		sb.Append("\t/// <inheritdoc cref=\"IReturnMethodSetupReturnBuilder{TReturn, ").Append(typeParams)
			.Append("}.When(Func{int, bool})\" />").AppendLine();
		sb.Append("\tIReturnMethodSetupReturnWhenBuilder<TReturn, ").Append(typeParams)
			.Append("> IReturnMethodSetupReturnBuilder<TReturn, ").Append(typeParams)
			.Append(">.When(Func<int, bool> predicate)").AppendLine();
		sb.Append("\t{").AppendLine();
		sb.Append("\t\t_currentReturnCallback?.When(predicate);").AppendLine();
		sb.Append("\t\treturn this;").AppendLine();
		sb.Append("\t}").AppendLine();
		sb.AppendLine();

		sb.Append("\t/// <inheritdoc cref=\"IReturnMethodSetupReturnWhenBuilder{TReturn, ").Append(typeParams)
			.Append("}.For(int)\" />").AppendLine();
		sb.Append("\tIReturnMethodSetup<TReturn, ").Append(typeParams).Append("> IReturnMethodSetupReturnWhenBuilder<TReturn, ").Append(typeParams)
			.Append(">.For(int times)").AppendLine();
		sb.Append("\t{").AppendLine();
		sb.Append("\t\t_currentReturnCallback?.For(x => x < times);").AppendLine();
		sb.Append("\t\treturn this;").AppendLine();
		sb.Append("\t}").AppendLine();
		sb.AppendLine();

		sb.Append("\t/// <inheritdoc cref=\"MethodSetup.ExecuteCallback(MethodInvocation, MockBehavior)\" />")
			.AppendLine();
		sb.Append("\tprotected override void ExecuteCallback(MethodInvocation invocation, MockBehavior behavior)")
			.AppendLine();
		sb.Append("\t{").AppendLine();
		sb.Append("\t\tif (");
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
		sb.Append("\t\t{").AppendLine();
		sb.Append("\t\t\t_callbacks.ForEach(callback => callback.Invoke((invocationCount, @delegate)").AppendLine();
		sb.Append("\t\t\t\t=> @delegate(invocationCount, ")
			.Append(string.Join(", ", Enumerable.Range(1, numberOfParameters).Select(x => $"p{x}"))).Append(")));")
			.AppendLine();
		sb.Append("\t\t}").AppendLine();
		sb.Append("\t}").AppendLine();
		sb.AppendLine();

		sb.Append("\t/// <inheritdoc cref=\"MethodSetup.GetReturnValue{TResult}(MethodInvocation, MockBehavior, Func{TResult})\" />")
			.AppendLine();
		sb.Append(
				"\tprotected override TResult GetReturnValue<TResult>(MethodInvocation invocation, MockBehavior behavior, Func<TResult> defaultValueGenerator)")
			.AppendLine();
		sb.Append("\t\twhere TResult : default").AppendLine();
		sb.Append("\t{").AppendLine();
		for (int i = 1; i <= numberOfParameters; i++)
		{
			sb.Append("\t\tif (!TryCast(invocation.Parameters[").Append(i - 1)
				.Append("], out T").Append(i).Append(" p").Append(i).Append(", behavior))").AppendLine();
			sb.Append("\t\t{").AppendLine();
			sb.Append("\t\t\tthrow new MockException($\"The input parameter ").Append(i)
				.Append(" only supports '{FormatType(typeof(T").Append(i)
				.Append("))}', but is '{FormatType(invocation.Parameters[")
				.Append(i - 1).Append("]!.GetType())}'.\");").AppendLine();
			sb.Append("\t\t}").AppendLine();
			sb.AppendLine();
		}

		sb.Append("\t\tforeach (var _ in _returnCallbacks)").AppendLine();
		sb.Append("\t\t{").AppendLine();
		sb.Append("\t\t\tvar returnCallback = _returnCallbacks[_currentReturnCallbackIndex % _returnCallbacks.Count];").AppendLine();
		sb.Append("\t\t\tif (returnCallback.Invoke<TReturn>(ref _currentReturnCallbackIndex, (invocationCount, @delegate)").AppendLine();
		sb.Append("\t\t\t\t=> @delegate(invocationCount, ")
			.Append(string.Join(", ", Enumerable.Range(1, numberOfParameters).Select(x => $"p{x}")))
			.Append("), out TReturn? newValue))").AppendLine();
		sb.Append("\t\t\t{").AppendLine();
		sb.Append("\t\t\t\tif (newValue is null)").AppendLine();
		sb.Append("\t\t\t\t{").AppendLine();
		sb.Append("\t\t\t\t\treturn default!;").AppendLine();
		sb.Append("\t\t\t\t}").AppendLine();
		sb.AppendLine();
		sb.Append("\t\t\t\tif (!TryCast(newValue, out TResult returnValue, behavior))").AppendLine();
		sb.Append("\t\t\t\t{").AppendLine();
		sb.Append("\t\t\t\t\tthrow new MockException($\"The return callback only supports '{FormatType(typeof(TReturn))}' and not '{FormatType(typeof(TResult))}'.\");")
			.AppendLine();
		sb.Append("\t\t\t\t}").AppendLine();
		sb.AppendLine();
		sb.Append("\t\t\t\treturn returnValue;").AppendLine();
		sb.Append("\t\t\t}").AppendLine();
		sb.Append("\t\t}").AppendLine();
		sb.AppendLine();
		sb.Append("\t\treturn defaultValueGenerator();").AppendLine();
		sb.Append("\t}").AppendLine();
		sb.AppendLine();

		sb.Append("\t/// <inheritdoc cref=\"MethodSetup.IsMatch(MethodInvocation)\" />").AppendLine();
		sb.Append("\tprotected override bool IsMatch(MethodInvocation invocation)").AppendLine();
		sb.Append("\t\t=> invocation.Name.Equals(_name) &&").AppendLine();
		sb.Append("\t\t\t(_matches is not null").AppendLine();
		sb.Append("\t\t\t\t? _matches.Matches(invocation.Parameters)").AppendLine();
		sb.Append("\t\t\t\t: Matches([")
			.Append(string.Join(", ", Enumerable.Range(1, numberOfParameters).Select(x => $"_match{x}!")))
			.Append("], invocation.Parameters));").AppendLine();
		sb.AppendLine();

		sb.Append("\t/// <inheritdoc cref=\"MethodSetup.TriggerParameterCallbacks(object?[])\" />").AppendLine();
		sb.Append("\tprotected override void TriggerParameterCallbacks(object?[] parameters)").AppendLine();
		sb.Append("\t\t=> TriggerCallbacks([")
			.Append(string.Join(", ", Enumerable.Range(1, numberOfParameters).Select(x => $"_match{x}")))
			.Append("], parameters);").AppendLine();
		sb.AppendLine();

		sb.Append("\t/// <inheritdoc cref=\"MethodSetup.GetCallBaseClass()\" />").AppendLine();
		sb.Append("\tprotected override bool? GetCallBaseClass()").AppendLine();
		sb.Append("\t\t=> _callBaseClass;").AppendLine();
		sb.AppendLine();

		sb.Append("\t/// <inheritdoc cref=\"MethodSetup.HasReturnCalls()\" />").AppendLine();
		sb.Append("\tprotected override bool HasReturnCalls()").AppendLine();
		sb.Append("\t\t=> _returnCallbacks.Count > 0;").AppendLine();
		sb.AppendLine();

		sb.Append("\t/// <inheritdoc cref=\"MethodSetup.SetOutParameter{T}(string, Func{T})\" />").AppendLine();
		sb.Append("\tprotected override T SetOutParameter<T>(string parameterName, Func<T> defaultValueGenerator)")
			.AppendLine();
		sb.Append("\t{").AppendLine();
		sb.Append("\t\tif (")
			.Append(string.Join(" && ", Enumerable.Range(1, numberOfParameters).Select(x => $"_match{x} is not null")))
			.Append(" &&").AppendLine();
		sb.Append("\t\t\tHasOutParameter([")
			.Append(string.Join(", ", Enumerable.Range(1, numberOfParameters).Select(x => $"_match{x}")))
			.Append("], parameterName, out Match.IOutParameter<T>? outParameter))").AppendLine();
		sb.Append("\t\t{").AppendLine();
		sb.Append("\t\t\treturn outParameter.GetValue(defaultValueGenerator);").AppendLine();
		sb.Append("\t\t}").AppendLine();
		sb.AppendLine();
		sb.Append("\t\treturn defaultValueGenerator();").AppendLine();
		sb.Append("\t}").AppendLine();
		sb.AppendLine();

		sb.Append("\t/// <inheritdoc cref=\"MethodSetup.SetRefParameter{T}(string, T, MockBehavior)\" />").AppendLine();
		sb.Append("\tprotected override T SetRefParameter<T>(string parameterName, T value, MockBehavior behavior)")
			.AppendLine();
		sb.Append("\t{").AppendLine();
		sb.Append("\t\tif (")
			.Append(string.Join(" && ", Enumerable.Range(1, numberOfParameters).Select(x => $"_match{x} is not null")))
			.Append(" &&").AppendLine();
		sb.Append("\t\t\tHasRefParameter([")
			.Append(string.Join(", ", Enumerable.Range(1, numberOfParameters).Select(x => $"_match{x}")))
			.Append("], parameterName, out Match.IRefParameter<T>? refParameter))").AppendLine();
		sb.Append("\t\t{").AppendLine();
		sb.Append("\t\t\treturn refParameter.GetValue(value);").AppendLine();
		sb.Append("\t\t}").AppendLine();
		sb.AppendLine();
		sb.Append("\t\treturn value;").AppendLine();
		sb.Append("\t}").AppendLine();
		sb.AppendLine();

		sb.Append("\t/// <inheritdoc cref=\"object.ToString()\" />").AppendLine();
		sb.Append("\tpublic override string ToString()").AppendLine();
		sb.Append("\t{").AppendLine();
		sb.Append("\t\tif (_matches is not null)").AppendLine();
		sb.Append("\t\t{").AppendLine();
		sb.Append("\t\t\treturn $\"{FormatType(typeof(TReturn))} {_name}({_matches})\";").AppendLine();
		sb.Append("\t\t}").AppendLine();
		sb.AppendLine();
		sb.Append("\t\treturn $\"{FormatType(typeof(TReturn))} {_name}(")
			.Append(string.Join(", ", Enumerable.Range(1, numberOfParameters).Select(x => $"{{_match{x}}}")))
			.Append(")\";").AppendLine();
		sb.Append("\t}").AppendLine();
		sb.Append("}").AppendLine();
		sb.AppendLine();
	}
}
