using System.Text;

namespace Mockolate.SourceGenerators.Sources;

internal static partial class Sources
{
	public static string IndexerSetups(HashSet<int> methodSetups)
	{
		StringBuilder sb = InitializeBuilder([
			"System",
			"System.Collections.Generic",
			"System.Diagnostics.CodeAnalysis",
			"System.Threading",
			"Mockolate.Exceptions",
			"Mockolate.Interactions",
		]);

		sb.Append("""
		          namespace Mockolate.Setup;

		          #nullable enable

		          """);
		foreach (int item in methodSetups)
		{
			sb.AppendLine();
			AppendIndexerSetup(sb, item);
		}

		sb.AppendLine("#nullable disable");
		return sb.ToString();
	}

	private static void AppendIndexerSetup(StringBuilder sb, int numberOfParameters)
	{
		string typeParams = string.Join(", ", Enumerable.Range(1, numberOfParameters).Select(i => $"T{i}"));
		string parameters = string.Join(", ", Enumerable.Range(1, numberOfParameters).Select(i => $"p{i}"));
		string discards = string.Join(", ", Enumerable.Range(1, numberOfParameters).Select(_ => "_"));
		sb.Append("/// <summary>").AppendLine();
		sb.Append("///     Sets up a <typeparamref name=\"TValue\"/> indexer for ");
		for (int i = 1; i < numberOfParameters - 1; i++)
		{
			sb.Append("<typeparamref name=\"T").Append(i).Append("\" />, ");
		}

		sb.Append("<typeparamref name=\"T").Append(numberOfParameters - 1).Append("\" /> and <typeparamref name=\"T")
			.Append(numberOfParameters).Append("\" />.").AppendLine();
		sb.Append("/// </summary>").AppendLine();
		sb.Append("internal class IndexerSetup<TValue, ").Append(typeParams).Append(">(")
			.Append(
				string.Join(", ", Enumerable.Range(1, numberOfParameters).Select(i => $"Match.IParameter match{i}")))
			.Append(") : IndexerSetup").AppendLine();
		sb.Append("{").AppendLine();
		sb.Append("\tprivate readonly List<Action<").Append(typeParams).Append(">> _getterCallbacks = [];")
			.AppendLine();
		sb.Append("\tprivate readonly List<Action<TValue, ").Append(typeParams).Append(">> _setterCallbacks = [];")
			.AppendLine();
		sb.Append("\tprivate readonly List<Func<TValue, ").Append(typeParams)
			.Append(", TValue>> _returnCallbacks = [];")
			.AppendLine();
		sb.Append("\tprivate bool? _callBaseClass;").AppendLine();
		sb.Append("\tprivate int _currentReturnCallbackIndex = -1;").AppendLine();
		sb.Append("\tprivate Func<").Append(typeParams).Append(", TValue>? _initialization;").AppendLine();
		sb.AppendLine();

		sb.Append("\t/// <summary>").AppendLine();
		sb.Append("\t///     Flag indicating if the base class implementation should be called, and its return values used as default values.").AppendLine();
		sb.Append("\t/// </summary>").AppendLine();
		sb.Append("\t/// <remarks>").AppendLine();
		sb.Append("\t///     If not specified, use <see cref=\"MockBehavior.CallBaseClass\" />.").AppendLine();
		sb.Append("\t/// </remarks>").AppendLine();
		sb.Append("\tpublic IndexerSetup<TValue, ").Append(typeParams).Append("> CallingBaseClass(bool callBaseClass = true)")
			.AppendLine();
		sb.Append("\t{").AppendLine();
		sb.Append("\t\t_callBaseClass = callBaseClass;").AppendLine();
		sb.Append("\t\treturn this;").AppendLine();
		sb.Append("\t}").AppendLine();
		sb.AppendLine();

		sb.Append("\t/// <summary>").AppendLine();
		sb.Append("\t///     Initializes the indexer with the given <paramref name=\"value\" />.").AppendLine();
		sb.Append("\t/// </summary>").AppendLine();
		sb.Append("\tpublic IndexerSetup<TValue, ").Append(typeParams).Append("> InitializeWith(TValue value)")
			.AppendLine();
		sb.Append("\t{").AppendLine();
		sb.Append("\t\tif (_initialization is not null)").AppendLine();
		sb.Append("\t\t{").AppendLine();
		sb.Append(
				"\t\t\tthrow new MockException(\"The indexer is already initialized. You cannot initialize it twice.\");")
			.AppendLine();
		sb.Append("\t\t}").AppendLine();
		sb.AppendLine();
		sb.Append("\t\t_initialization = (").Append(discards).Append(") => value;").AppendLine();
		sb.Append("\t\treturn this;").AppendLine();
		sb.Append("\t}").AppendLine();
		sb.AppendLine();

		sb.Append("\t/// <summary>").AppendLine();
		sb.Append("\t///     Initializes the according to the given <paramref name=\"valueGenerator\" />.")
			.AppendLine();
		sb.Append("\t/// </summary>").AppendLine();
		sb.Append("\tpublic IndexerSetup<TValue, ").Append(typeParams).Append("> InitializeWith(Func<")
			.Append(typeParams).Append(", TValue> valueGenerator)").AppendLine();
		sb.Append("\t{").AppendLine();
		sb.Append("\t\tif (_initialization is not null)").AppendLine();
		sb.Append("\t\t{").AppendLine();
		sb.Append(
				"\t\t\tthrow new MockException(\"The indexer is already initialized. You cannot initialize it twice.\");")
			.AppendLine();
		sb.Append("\t\t}").AppendLine();
		sb.AppendLine();
		sb.Append("\t\t_initialization = valueGenerator;").AppendLine();
		sb.Append("\t\treturn this;").AppendLine();
		sb.Append("\t}").AppendLine();
		sb.AppendLine();

		sb.Append("\t/// <summary>").AppendLine();
		sb.Append(
				"\t///     Registers a <paramref name=\"callback\"/> to be invoked whenever the indexer's getter is accessed.")
			.AppendLine();
		sb.Append("\t/// </summary>").AppendLine();
		sb.Append("\tpublic IndexerSetup<TValue, ").Append(typeParams).Append("> OnGet(Action callback)").AppendLine();
		sb.Append("\t{").AppendLine();
		sb.Append("\t\t_getterCallbacks.Add((").Append(discards).Append(") => callback());").AppendLine();
		sb.Append("\t\treturn this;").AppendLine();
		sb.Append("\t}").AppendLine();
		sb.AppendLine();

		sb.Append("\t/// <summary>").AppendLine();
		sb.Append(
				"\t///     Registers a <paramref name=\"callback\"/> to be invoked whenever the indexer's getter is accessed.")
			.AppendLine();
		sb.Append("\t/// </summary>").AppendLine();
		sb.Append("\tpublic IndexerSetup<TValue, ").Append(typeParams).Append("> OnGet(Action<").Append(typeParams)
			.Append("> callback)").AppendLine();
		sb.Append("\t{").AppendLine();
		sb.Append("\t\t_getterCallbacks.Add(callback);").AppendLine();
		sb.Append("\t\treturn this;").AppendLine();
		sb.Append("\t}").AppendLine();
		sb.AppendLine();

		sb.Append("\t/// <summary>").AppendLine();
		sb.Append(
				"\t///     Registers a <paramref name=\"callback\"/> to be invoked whenever the indexer's setter is accessed.")
			.AppendLine();
		sb.Append("\t/// </summary>").AppendLine();
		sb.Append("\tpublic IndexerSetup<TValue, ").Append(typeParams).Append("> OnSet(Action callback)").AppendLine();
		sb.Append("\t{").AppendLine();
		sb.Append("\t\t_setterCallbacks.Add((_, ").Append(discards).Append(") => callback());").AppendLine();
		sb.Append("\t\treturn this;").AppendLine();
		sb.Append("\t}").AppendLine();
		sb.AppendLine();

		sb.Append("\t/// <summary>").AppendLine();
		sb.Append(
				"\t///     Registers a <paramref name=\"callback\"/> to be invoked whenever the indexer's setter is accessed.")
			.AppendLine();
		sb.Append("\t/// </summary>").AppendLine();
		sb.Append("\tpublic IndexerSetup<TValue, ").Append(typeParams).Append("> OnSet(Action<TValue> callback)")
			.AppendLine();
		sb.Append("\t{").AppendLine();
		sb.Append("\t\t_setterCallbacks.Add((v, ").Append(discards).Append(") => callback(v));").AppendLine();
		sb.Append("\t\treturn this;").AppendLine();
		sb.Append("\t}").AppendLine();
		sb.AppendLine();

		sb.Append("\t/// <summary>").AppendLine();
		sb.Append(
				"\t///     Registers a <paramref name=\"callback\"/> to be invoked whenever the indexer's setter is accessed.")
			.AppendLine();
		sb.Append("\t/// </summary>").AppendLine();
		sb.Append("\tpublic IndexerSetup<TValue, ").Append(typeParams).Append("> OnSet(Action<TValue, ")
			.Append(typeParams).Append("> callback)").AppendLine();
		sb.Append("\t{").AppendLine();
		sb.Append("\t\t_setterCallbacks.Add(callback);").AppendLine();
		sb.Append("\t\treturn this;").AppendLine();
		sb.Append("\t}").AppendLine();
		sb.AppendLine();

		sb.Append("\t/// <summary>").AppendLine();
		sb.Append("\t///     Registers a <paramref name=\"callback\" /> to setup the return value for this indexer.")
			.AppendLine();
		sb.Append("\t/// </summary>").AppendLine();
		sb.Append("\tpublic IndexerSetup<TValue, ").Append(typeParams).Append("> Returns(Func<TValue, ")
			.Append(typeParams).Append(", TValue> callback)").AppendLine();
		sb.Append("\t{").AppendLine();
		sb.Append("\t\t_returnCallbacks.Add(callback);").AppendLine();
		sb.Append("\t\treturn this;").AppendLine();
		sb.Append("\t}").AppendLine();
		sb.AppendLine();

		sb.Append("\t/// <summary>").AppendLine();
		sb.Append("\t///     Registers a <paramref name=\"callback\" /> to setup the return value for this indexer.")
			.AppendLine();
		sb.Append("\t/// </summary>").AppendLine();
		sb.Append("\tpublic IndexerSetup<TValue, ").Append(typeParams).Append("> Returns(Func<").Append(typeParams)
			.Append(", TValue> callback)").AppendLine();
		sb.Append("\t{").AppendLine();
		sb.Append("\t\t_returnCallbacks.Add((_, ").Append(parameters).Append(") => callback(").Append(parameters)
			.Append("));").AppendLine();
		sb.Append("\t\treturn this;").AppendLine();
		sb.Append("\t}").AppendLine();
		sb.AppendLine();

		sb.Append("\t/// <summary>").AppendLine();
		sb.Append("\t///     Registers a <paramref name=\"callback\" /> to setup the return value for this indexer.")
			.AppendLine();
		sb.Append("\t/// </summary>").AppendLine();
		sb.Append("\tpublic IndexerSetup<TValue, ").Append(typeParams).Append("> Returns(Func<TValue> callback)")
			.AppendLine();
		sb.Append("\t{").AppendLine();
		sb.Append("\t\t_returnCallbacks.Add((_, ").Append(discards).Append(") => callback());").AppendLine();
		sb.Append("\t\treturn this;").AppendLine();
		sb.Append("\t}").AppendLine();
		sb.AppendLine();

		sb.Append("\t/// <summary>").AppendLine();
		sb.Append("\t///     Registers the <paramref name=\"returnValue\" /> for this indexer.").AppendLine();
		sb.Append("\t/// </summary>").AppendLine();
		sb.Append("\tpublic IndexerSetup<TValue, ").Append(typeParams).Append("> Returns(TValue returnValue)")
			.AppendLine();
		sb.Append("\t{").AppendLine();
		sb.Append("\t\t_returnCallbacks.Add((_, ").Append(discards).Append(") => returnValue);").AppendLine();
		sb.Append("\t\treturn this;").AppendLine();
		sb.Append("\t}").AppendLine();
		sb.AppendLine();

		sb.Append("\t/// <summary>").AppendLine();
		sb.Append("\t///     Registers an <typeparamref name=\"TException\" /> to throw when the indexer is read.")
			.AppendLine();
		sb.Append("\t/// </summary>").AppendLine();
		sb.Append("\tpublic IndexerSetup<TValue, ").Append(typeParams).Append("> Throws<TException>()")
			.AppendLine();
		sb.Append("\t\twhere TException : Exception, new()").AppendLine();
		sb.Append("\t{").AppendLine();
		sb.Append("\t\t_returnCallbacks.Add((_, ").Append(discards).Append(") => throw new TException());")
			.AppendLine();
		sb.Append("\t\treturn this;").AppendLine();
		sb.Append("\t}").AppendLine();
		sb.AppendLine();

		sb.Append("\t/// <summary>").AppendLine();
		sb.Append("\t///     Registers an <paramref name=\"exception\" /> to throw when the indexer is read.")
			.AppendLine();
		sb.Append("\t/// </summary>").AppendLine();
		sb.Append("\tpublic IndexerSetup<TValue, ").Append(typeParams).Append("> Throws(Exception exception)")
			.AppendLine();
		sb.Append("\t{").AppendLine();
		sb.Append("\t\t_returnCallbacks.Add((_, ").Append(discards).Append(") => throw exception);").AppendLine();
		sb.Append("\t\treturn this;").AppendLine();
		sb.Append("\t}").AppendLine();
		sb.AppendLine();

		sb.Append("\t/// <summary>").AppendLine();
		sb.Append(
				"\t///     Registers a <paramref name=\"callback\" /> that will calculate the exception to throw when the indexer is read.")
			.AppendLine();
		sb.Append("\t/// </summary>").AppendLine();
		sb.Append("\tpublic IndexerSetup<TValue, ").Append(typeParams).Append("> Throws(Func<Exception> callback)")
			.AppendLine();
		sb.Append("\t{").AppendLine();
		sb.Append("\t\t_returnCallbacks.Add((_, ").Append(discards).Append(") => throw callback());").AppendLine();
		sb.Append("\t\treturn this;").AppendLine();
		sb.Append("\t}").AppendLine();
		sb.AppendLine();

		sb.Append("\t/// <summary>").AppendLine();
		sb.Append(
				"\t///     Registers a <paramref name=\"callback\" /> that will calculate the exception to throw when the indexer is read.")
			.AppendLine();
		sb.Append("\t/// </summary>").AppendLine();
		sb.Append("\tpublic IndexerSetup<TValue, ").Append(typeParams).Append("> Throws(Func<").Append(typeParams)
			.Append(", Exception> callback)").AppendLine();
		sb.Append("\t{").AppendLine();
		sb.Append("\t\t_returnCallbacks.Add((_, ").Append(parameters).Append(") => throw callback(").Append(parameters)
			.Append("));").AppendLine();
		sb.Append("\t\treturn this;").AppendLine();
		sb.Append("\t}").AppendLine();
		sb.AppendLine();

		sb.Append("\t/// <summary>").AppendLine();
		sb.Append(
				"\t///     Registers a <paramref name=\"callback\" /> that will calculate the exception to throw when the indexer is read.")
			.AppendLine();
		sb.Append("\t/// </summary>").AppendLine();
		sb.Append("\tpublic IndexerSetup<TValue, ").Append(typeParams).Append("> Throws(Func<TValue, ")
			.Append(typeParams).Append(", Exception> callback)").AppendLine();
		sb.Append("\t{").AppendLine();
		sb.Append("\t\t_returnCallbacks.Add((v, ").Append(parameters).Append(") => throw callback(v, ")
			.Append(parameters).Append("));").AppendLine();
		sb.Append("\t\treturn this;").AppendLine();
		sb.Append("\t}").AppendLine();
		sb.AppendLine();

		sb.Append(
				"\t/// <inheritdoc cref=\"ExecuteGetterCallback{TValue}(IndexerGetterAccess, TValue, MockBehavior)\" />")
			.AppendLine();
		sb.Append(
				"\tprotected override T ExecuteGetterCallback<T>(IndexerGetterAccess indexerGetterAccess, T value, MockBehavior behavior)")
			.AppendLine();
		sb.Append("\t{").AppendLine();
		sb.Append("\t\tif (TryCast(value, out TValue resultValue, behavior) &&").AppendLine();
		sb.Append("\t\t    indexerGetterAccess.Parameters.Length == ").Append(numberOfParameters);
		for (int i = 1; i <= numberOfParameters; i++)
		{
			sb.Append(" &&").AppendLine();
			sb.Append("\t\t    TryCast(indexerGetterAccess.Parameters[").Append(i - 1).Append("], out T").Append(i)
				.Append(" p")
				.Append(i).Append(", behavior)");
		}

		sb.Append(")").AppendLine();
		sb.Append("\t\t{").AppendLine();
		sb.Append("\t\t\t_getterCallbacks.ForEach(callback => callback.Invoke(").Append(parameters).Append("));")
			.AppendLine();
		sb.Append("\t\t\tif (_returnCallbacks.Count > 0)").AppendLine();
		sb.Append("\t\t\t{").AppendLine();
		sb.Append("\t\t\t\tint index = Interlocked.Increment(ref _currentReturnCallbackIndex);").AppendLine();
		sb.Append("\t\t\t\tFunc<TValue, ").Append(typeParams)
			.Append(", TValue> returnCallback = _returnCallbacks[index % _returnCallbacks.Count];").AppendLine();
		sb.Append("\t\t\t\tTValue newValue = returnCallback(resultValue, ").Append(parameters).Append(");")
			.AppendLine();
		sb.Append("\t\t\t\tif (TryCast<T>(newValue, out T? returnValue, behavior))").AppendLine();
		sb.Append("\t\t\t\t{").AppendLine();
		sb.Append("\t\t\t\t\treturn returnValue;").AppendLine();
		sb.Append("\t\t\t\t}").AppendLine();
		sb.Append("\t\t\t}").AppendLine();
		sb.Append("\t\t}").AppendLine();
		sb.AppendLine();
		sb.Append("\t\treturn value;").AppendLine();
		sb.Append("\t}").AppendLine();
		sb.AppendLine();

		sb.Append("\t/// <inheritdoc cref=\"PropertySetup.GetCallBaseClass()\" />").AppendLine();
		sb.Append("\tprotected override bool? GetCallBaseClass()").AppendLine();
		sb.Append("\t\t=> _callBaseClass;").AppendLine();
		sb.AppendLine();

		sb.Append(
				"\t/// <inheritdoc cref=\"ExecuteSetterCallback{TValue}(IndexerSetterAccess, TValue, MockBehavior)\" />")
			.AppendLine();
		sb.Append(
				"\tprotected override void ExecuteSetterCallback<T>(IndexerSetterAccess indexerSetterAccess, T value, MockBehavior behavior)")
			.AppendLine();
		sb.Append("\t{").AppendLine();
		sb.Append("\t\tif (TryCast(value, out TValue resultValue, behavior) &&").AppendLine();
		sb.Append("\t\t    indexerSetterAccess.Parameters.Length == ").Append(numberOfParameters);
		for (int i = 1; i <= numberOfParameters; i++)
		{
			sb.Append(" &&").AppendLine();
			sb.Append("\t\t    TryCast(indexerSetterAccess.Parameters[").Append(i - 1).Append("], out T").Append(i)
				.Append(" p")
				.Append(i).Append(", behavior)");
		}

		sb.Append(")").AppendLine();
		sb.Append("\t\t{").AppendLine();
		sb.Append("\t\t\t_setterCallbacks.ForEach(callback => callback.Invoke(resultValue, ").Append(parameters)
			.Append("));").AppendLine();
		sb.Append("\t\t}").AppendLine();
		sb.Append("\t}").AppendLine();
		sb.AppendLine();

		sb.Append("\t/// <inheritdoc cref=\"IsMatch(object?[])\" />").AppendLine();
		sb.Append("\tprotected override bool IsMatch(object?[] parameters)").AppendLine();
		sb.Append("\t\t=> Matches([")
			.Append(string.Join(", ", Enumerable.Range(1, numberOfParameters).Select(i => $"match{i}")))
			.Append("], parameters);").AppendLine();
		sb.AppendLine();

		sb.Append("\t/// <inheritdoc cref=\"IndexerSetup.TryGetInitialValue{T}(MockBehavior, object?[], out T)\" />")
			.AppendLine();
		sb.Append(
				"\tprotected override bool TryGetInitialValue<T>(MockBehavior behavior, object?[] parameters, [NotNullWhen(true)] out T value)")
			.AppendLine();
		sb.Append("\t{").AppendLine();
		sb.Append("\t\tif (_initialization is not null &&").AppendLine();
		sb.Append("\t\t    parameters.Length == ").Append(numberOfParameters).Append(" &&").AppendLine();
		for (int i = 1; i <= numberOfParameters; i++)
		{
			sb.Append("\t\t    TryCast(parameters[").Append(i - 1).Append("], out T").Append(i).Append(" p")
				.Append(i).Append(", behavior)").Append(" &&").AppendLine();
		}

		sb.Append("\t\t    _initialization.Invoke(").Append(parameters).Append(") is T initialValue)").AppendLine();
		sb.Append("\t\t{").AppendLine();
		sb.Append("\t\t\tvalue = initialValue;").AppendLine();
		sb.Append("\t\t\treturn true;").AppendLine();
		sb.Append("\t\t}").AppendLine();
		sb.AppendLine();
		sb.Append("\t\tvalue = behavior.DefaultValue.Generate<T>();").AppendLine();
		sb.Append("\t\treturn false;").AppendLine();
		sb.Append("\t}").AppendLine();
		sb.AppendLine();

		sb.Append("}").AppendLine();
		sb.AppendLine();
	}
}
