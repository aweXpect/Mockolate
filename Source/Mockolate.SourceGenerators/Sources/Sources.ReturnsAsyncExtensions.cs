using System.Text;

namespace Mockolate.SourceGenerators.Sources;

internal static partial class Sources
{
	public static string ReturnsThrowsAsyncExtensions(int[] numberOfParameters)
	{
		StringBuilder sb = InitializeBuilder();

		sb.Append("""
		          namespace Mockolate;

		          #nullable enable

		          """);
		sb.AppendLine();
		sb.Append("/// <summary>").AppendLine();
		sb.Append("///     Extensions for setting up return values and throwing exceptions for <see langword=\"async\" /> methods.").AppendLine();
		sb.Append("/// </summary>").AppendLine();
		sb.Append("[global::System.Diagnostics.DebuggerNonUserCode]").AppendLine();
		sb.Append("internal static class ReturnsThrowsAsyncExtensions2").AppendLine();
		sb.Append("{").AppendLine();
		foreach (int number in numberOfParameters)
		{
			string types = string.Join(", ", Enumerable.Range(1, number).Select(i => $"T{i}"));
			string variables = string.Join(", ", Enumerable.Range(1, number).Select(i => $"v{i}"));
			sb.Append("\t/// <summary>").AppendLine();
			sb.Append(
					"\t///     Registers the <see langword=\"async\" /> <paramref name=\"returnValue\" /> for this method.")
				.AppendLine();
			sb.Append("\t/// </summary>").AppendLine();
			sb.Append("\tpublic static global::Mockolate.Setup.IReturnMethodSetupReturnBuilder<global::System.Threading.Tasks.Task<TReturn>, ").Append(types)
				.Append("> ReturnsAsync<TReturn, ").Append(types).Append(">(this global::Mockolate.Setup.IReturnMethodSetup<global::System.Threading.Tasks.Task<TReturn>, ")
				.Append(types).Append("> setup, TReturn returnValue)").AppendLine();
			sb.Append("\t\t=> setup.Returns(global::System.Threading.Tasks.Task.FromResult(returnValue));").AppendLine();
			sb.AppendLine();

			sb.Append("\t/// <summary>").AppendLine();
			sb.Append(
					"\t///     Registers an <see langword=\"async\" /> <paramref name=\"callback\" /> to setup the return value for this method.")
				.AppendLine();
			sb.Append("\t/// </summary>").AppendLine();
			sb.Append("\tpublic static global::Mockolate.Setup.IReturnMethodSetupReturnBuilder<global::System.Threading.Tasks.Task<TReturn>, ").Append(types)
				.Append("> ReturnsAsync<TReturn, ").Append(types).Append(">(this global::Mockolate.Setup.IReturnMethodSetup<global::System.Threading.Tasks.Task<TReturn>, ")
				.Append(types).Append("> setup, global::System.Func<TReturn> callback)").AppendLine();
			sb.Append("\t\t=> setup.Returns(() => global::System.Threading.Tasks.Task.FromResult(callback()));").AppendLine();
			sb.AppendLine();

			sb.Append("\t/// <summary>").AppendLine();
			sb.Append(
					"\t///     Registers an <see langword=\"async\" /> <paramref name=\"callback\" /> to setup the return value for this method.")
				.AppendLine();
			sb.Append("\t/// </summary>").AppendLine();
			sb.Append("\tpublic static global::Mockolate.Setup.IReturnMethodSetupReturnBuilder<global::System.Threading.Tasks.Task<TReturn>, ").Append(types)
				.Append("> ReturnsAsync<TReturn, ").Append(types).Append(">(this global::Mockolate.Setup.IReturnMethodSetup<global::System.Threading.Tasks.Task<TReturn>, ")
				.Append(types).Append("> setup, global::System.Func<").Append(types).Append(", TReturn> callback)").AppendLine();
			sb.Append("\t\t=> setup.Returns((").Append(variables).Append(") => global::System.Threading.Tasks.Task.FromResult(callback(")
				.Append(variables).Append(")));").AppendLine();
			sb.AppendLine();

			sb.Append("\t/// <summary>").AppendLine();
			sb.Append(
					"\t///     Registers an <paramref name=\"exception\" /> to throw when the <see langword=\"async\" /> method is awaited.")
				.AppendLine();
			sb.Append("\t/// </summary>").AppendLine();
			sb.Append("\tpublic static global::Mockolate.Setup.IReturnMethodSetupReturnBuilder<global::System.Threading.Tasks.Task<TReturn>, ").Append(types)
				.Append("> ThrowsAsync<TReturn, ").Append(types).Append(">(this global::Mockolate.Setup.IReturnMethodSetup<global::System.Threading.Tasks.Task<TReturn>, ")
				.Append(types).Append("> setup, global::System.Exception exception)").AppendLine();
			sb.Append("\t\t=> setup.Returns(global::System.Threading.Tasks.Task.FromException<TReturn>(exception));").AppendLine();
			sb.AppendLine();

			sb.Append("\t/// <summary>").AppendLine();
			sb.Append(
					"\t///     Registers a <paramref name=\"callback\" /> that calculates the exception to throw when the <see langword=\"async\" /> method is awaited.")
				.AppendLine();
			sb.Append("\t/// </summary>").AppendLine();
			sb.Append("\tpublic static global::Mockolate.Setup.IReturnMethodSetupReturnBuilder<global::System.Threading.Tasks.Task<TReturn>, ").Append(types)
				.Append("> ThrowsAsync<TReturn, ").Append(types).Append(">(this global::Mockolate.Setup.IReturnMethodSetup<global::System.Threading.Tasks.Task<TReturn>, ")
				.Append(types).Append("> setup, global::System.Func<global::System.Exception> callback)").AppendLine();
			sb.Append("\t\t=> setup.Returns(() => global::System.Threading.Tasks.Task.FromException<TReturn>(callback()));").AppendLine();
			sb.AppendLine();

			sb.Append("\t/// <summary>").AppendLine();
			sb.Append(
					"\t///     Registers a <paramref name=\"callback\" /> that calculates the exception to throw when the <see langword=\"async\" /> method is awaited.")
				.AppendLine();
			sb.Append("\t/// </summary>").AppendLine();
			sb.Append("\tpublic static global::Mockolate.Setup.IReturnMethodSetupReturnBuilder<global::System.Threading.Tasks.Task<TReturn>, ").Append(types)
				.Append("> ThrowsAsync<TReturn, ").Append(types).Append(">(this global::Mockolate.Setup.IReturnMethodSetup<global::System.Threading.Tasks.Task<TReturn>, ")
				.Append(types).Append("> setup, global::System.Func<").Append(types).Append(", global::System.Exception> callback)").AppendLine();
			sb.Append("\t\t=> setup.Returns((").Append(variables).Append(") => global::System.Threading.Tasks.Task.FromException<TReturn>(callback(")
				.Append(variables).Append(")));").AppendLine();
			sb.AppendLine();
		}

		sb.AppendLine("#if NET8_0_OR_GREATER");
		sb.AppendLine();
		foreach (int number in numberOfParameters)
		{
			string types = string.Join(", ", Enumerable.Range(1, number).Select(i => $"T{i}"));
			string variables = string.Join(", ", Enumerable.Range(1, number).Select(i => $"v{i}"));
			sb.Append("\t/// <summary>").AppendLine();
			sb.Append(
					"\t///     Registers the <see langword=\"async\" /> <paramref name=\"returnValue\" /> for this method.")
				.AppendLine();
			sb.Append("\t/// </summary>").AppendLine();
			sb.Append("\tpublic static global::Mockolate.Setup.IReturnMethodSetupReturnBuilder<global::System.Threading.Tasks.ValueTask<TReturn>, ").Append(types)
				.Append("> ReturnsAsync<TReturn, ").Append(types)
				.Append(">(this global::Mockolate.Setup.IReturnMethodSetup<global::System.Threading.Tasks.ValueTask<TReturn>, ").Append(types)
				.Append("> setup, TReturn returnValue)").AppendLine();
			sb.Append("\t\t=> setup.Returns(global::System.Threading.Tasks.ValueTask.FromResult(returnValue));").AppendLine();
			sb.AppendLine();

			sb.Append("\t/// <summary>").AppendLine();
			sb.Append(
					"\t///     Registers an <see langword=\"async\" /> <paramref name=\"callback\" /> to setup the return value for this method.")
				.AppendLine();
			sb.Append("\t/// </summary>").AppendLine();
			sb.Append("\tpublic static global::Mockolate.Setup.IReturnMethodSetupReturnBuilder<global::System.Threading.Tasks.ValueTask<TReturn>, ").Append(types)
				.Append("> ReturnsAsync<TReturn, ").Append(types)
				.Append(">(this global::Mockolate.Setup.IReturnMethodSetup<global::System.Threading.Tasks.ValueTask<TReturn>, ").Append(types)
				.Append("> setup, global::System.Func<TReturn> callback)").AppendLine();
			sb.Append("\t\t=> setup.Returns(() => global::System.Threading.Tasks.ValueTask.FromResult(callback()));").AppendLine();
			sb.AppendLine();

			sb.Append("\t/// <summary>").AppendLine();
			sb.Append(
					"\t///     Registers an <see langword=\"async\" /> <paramref name=\"callback\" /> to setup the return value for this method.")
				.AppendLine();
			sb.Append("\t/// </summary>").AppendLine();
			sb.Append("\tpublic static global::Mockolate.Setup.IReturnMethodSetupReturnBuilder<global::System.Threading.Tasks.ValueTask<TReturn>, ").Append(types)
				.Append("> ReturnsAsync<TReturn, ").Append(types)
				.Append(">(this global::Mockolate.Setup.IReturnMethodSetup<global::System.Threading.Tasks.ValueTask<TReturn>, ").Append(types).Append("> setup, global::System.Func<")
				.Append(types).Append(", TReturn> callback)").AppendLine();
			sb.Append("\t\t=> setup.Returns((").Append(variables).Append(") => global::System.Threading.Tasks.ValueTask.FromResult(callback(")
				.Append(variables).Append(")));").AppendLine();
			sb.AppendLine();

			sb.Append("\t/// <summary>").AppendLine();
			sb.Append(
					"\t///     Registers an <paramref name=\"exception\" /> to throw when the <see langword=\"async\" /> method is awaited.")
				.AppendLine();
			sb.Append("\t/// </summary>").AppendLine();
			sb.Append("\tpublic static global::Mockolate.Setup.IReturnMethodSetupReturnBuilder<global::System.Threading.Tasks.ValueTask<TReturn>, ").Append(types)
				.Append("> ThrowsAsync<TReturn, ").Append(types)
				.Append(">(this global::Mockolate.Setup.IReturnMethodSetup<global::System.Threading.Tasks.ValueTask<TReturn>, ").Append(types)
				.Append("> setup, global::System.Exception exception)").AppendLine();
			sb.Append("\t\t=> setup.Returns(global::System.Threading.Tasks.ValueTask.FromException<TReturn>(exception));").AppendLine();
			sb.AppendLine();

			sb.Append("\t/// <summary>").AppendLine();
			sb.Append(
					"\t///     Registers a <paramref name=\"callback\" /> that calculates the exception to throw when the <see langword=\"async\" /> method is awaited.")
				.AppendLine();
			sb.Append("\t/// </summary>").AppendLine();
			sb.Append("\tpublic static global::Mockolate.Setup.IReturnMethodSetupReturnBuilder<global::System.Threading.Tasks.ValueTask<TReturn>, ").Append(types)
				.Append("> ThrowsAsync<TReturn, ").Append(types)
				.Append(">(this global::Mockolate.Setup.IReturnMethodSetup<global::System.Threading.Tasks.ValueTask<TReturn>, ").Append(types)
				.Append("> setup, global::System.Func<global::System.Exception> callback)").AppendLine();
			sb.Append("\t\t=> setup.Returns(() => global::System.Threading.Tasks.ValueTask.FromException<TReturn>(callback()));").AppendLine();
			sb.AppendLine();

			sb.Append("\t/// <summary>").AppendLine();
			sb.Append(
					"\t///     Registers a <paramref name=\"callback\" /> that calculates the exception to throw when the <see langword=\"async\" /> method is awaited.")
				.AppendLine();
			sb.Append("\t/// </summary>").AppendLine();
			sb.Append("\tpublic static global::Mockolate.Setup.IReturnMethodSetupReturnBuilder<global::System.Threading.Tasks.ValueTask<TReturn>, ").Append(types)
				.Append("> ThrowsAsync<TReturn, ").Append(types)
				.Append(">(this global::Mockolate.Setup.IReturnMethodSetup<global::System.Threading.Tasks.ValueTask<TReturn>, ").Append(types).Append("> setup, global::System.Func<")
				.Append(types).Append(", global::System.Exception> callback)").AppendLine();
			sb.Append("\t\t=> setup.Returns((").Append(variables).Append(") => global::System.Threading.Tasks.ValueTask.FromException<TReturn>(callback(")
				.Append(variables).Append(")));").AppendLine();
			sb.AppendLine();
		}

		sb.AppendLine("#endif");
		sb.Append("}").AppendLine();

		sb.AppendLine("#nullable disable");
		return sb.ToString();
	}
}
