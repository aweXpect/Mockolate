using System;

namespace Mockolate.Parameters;

/// <summary>
///     Matches a method parameter of type <typeparamref name="T" /> and supports registering
///     callbacks that run when the parameter matches.
/// </summary>
public interface IParameterWithCallback<out T> : IParameter<T>
{
	/// <summary>
	///     Registers a <paramref name="callback" /> to execute for matching parameters.
	/// </summary>
	IParameterWithCallback<T> Do(Action<T> callback);
}
