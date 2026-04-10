using System;
using System.Diagnostics;
using Mockolate.Interactions;
using Mockolate.Parameters;
using Mockolate.Setup;

namespace Mockolate;

public partial class MockRegistry
{
	/// <summary>
	///     The registered setups for the mock, including methods, properties, indexers and events.
	/// </summary>
	internal MockSetups Setup { get; }

	/// <summary>
	///     Retrieves the latest method setup that matches the specified <paramref name="methodInvocation" />,
	///     or returns <see langword="null" /> if no matching setup is found.
	/// </summary>
	private MethodSetup? GetMethodSetup(MethodInvocation methodInvocation)
	{
		return Setup.Methods.GetLatestOrDefault(Predicate);

		[DebuggerNonUserCode]
		bool Predicate(MethodSetup setup)
		{
			return ((IInteractiveMethodSetup)setup).Matches(methodInvocation);
		}
	}

	/// <summary>
	///     Retrieves the latest method setup that matches the method name and single typed parameter,
	///     or returns <see langword="null" /> if no matching setup is found.
	/// </summary>
	private MethodSetup? GetMethodSetupTyped<T1>(string methodName, string n1, T1 v1)
	{
		MethodInvocation? fallback = null;
		return Setup.Methods.GetLatestOrDefault(Predicate);

		[DebuggerNonUserCode]
		bool Predicate(MethodSetup setup)
		{
			IMethodMatch match = setup.GetMatch();
			if (match is ITypedMethodMatch typed)
			{
				return typed.MatchesTyped(methodName, n1, v1);
			}

			fallback ??= new MethodInvocation(methodName, [new NamedParameterValue<T1>(n1, v1)]);
			return ((IInteractiveMethodSetup)setup).Matches(fallback);
		}
	}

	/// <summary>
	///     Retrieves the latest method setup that matches the method name and two typed parameters,
	///     or returns <see langword="null" /> if no matching setup is found.
	/// </summary>
	private MethodSetup? GetMethodSetupTyped<T1, T2>(
		string methodName, string n1, T1 v1, string n2, T2 v2)
	{
		MethodInvocation? fallback = null;
		return Setup.Methods.GetLatestOrDefault(Predicate);

		[DebuggerNonUserCode]
		bool Predicate(MethodSetup setup)
		{
			IMethodMatch match = setup.GetMatch();
			if (match is ITypedMethodMatch typed)
			{
				return typed.MatchesTyped(methodName, n1, v1, n2, v2);
			}

			fallback ??= new MethodInvocation(methodName, [
				new NamedParameterValue<T1>(n1, v1),
				new NamedParameterValue<T2>(n2, v2)]);
			return ((IInteractiveMethodSetup)setup).Matches(fallback);
		}
	}

	/// <summary>
	///     Retrieves the latest method setup that matches the method name and three typed parameters,
	///     or returns <see langword="null" /> if no matching setup is found.
	/// </summary>
	private MethodSetup? GetMethodSetupTyped<T1, T2, T3>(
		string methodName, string n1, T1 v1, string n2, T2 v2, string n3, T3 v3)
	{
		MethodInvocation? fallback = null;
		return Setup.Methods.GetLatestOrDefault(Predicate);

		[DebuggerNonUserCode]
		bool Predicate(MethodSetup setup)
		{
			IMethodMatch match = setup.GetMatch();
			if (match is ITypedMethodMatch typed)
			{
				return typed.MatchesTyped(methodName, n1, v1, n2, v2, n3, v3);
			}

			fallback ??= new MethodInvocation(methodName, [
				new NamedParameterValue<T1>(n1, v1),
				new NamedParameterValue<T2>(n2, v2),
				new NamedParameterValue<T3>(n3, v3)]);
			return ((IInteractiveMethodSetup)setup).Matches(fallback);
		}
	}

	/// <summary>
	///     Retrieves the latest method setup that matches the method name and four typed parameters,
	///     or returns <see langword="null" /> if no matching setup is found.
	/// </summary>
	private MethodSetup? GetMethodSetupTyped<T1, T2, T3, T4>(
		string methodName, string n1, T1 v1, string n2, T2 v2, string n3, T3 v3, string n4, T4 v4)
	{
		MethodInvocation? fallback = null;
		return Setup.Methods.GetLatestOrDefault(Predicate);

		[DebuggerNonUserCode]
		bool Predicate(MethodSetup setup)
		{
			IMethodMatch match = setup.GetMatch();
			if (match is ITypedMethodMatch typed)
			{
				return typed.MatchesTyped(methodName, n1, v1, n2, v2, n3, v3, n4, v4);
			}

			fallback ??= new MethodInvocation(methodName, [
				new NamedParameterValue<T1>(n1, v1),
				new NamedParameterValue<T2>(n2, v2),
				new NamedParameterValue<T3>(n3, v3),
				new NamedParameterValue<T4>(n4, v4)]);
			return ((IInteractiveMethodSetup)setup).Matches(fallback);
		}
	}

	/// <summary>
	///     Gets the indexer value for the given <paramref name="parameters" />.
	/// </summary>
	private TValue GetIndexerValue<TValue>(IInteractiveIndexerSetup? setup, Func<TValue> defaultValueGenerator,
		INamedParameterValue[] parameters)
		=> Setup.Indexers.GetOrAddValue(parameters, defaultValueGenerator);

	/// <summary>
	///     Registers the <paramref name="indexerSetup" /> in the mock.
	/// </summary>
	public void SetupIndexer(IndexerSetup indexerSetup) => Setup.Indexers.Add(indexerSetup);

	/// <summary>
	///     Registers the <paramref name="methodSetup" /> in the mock.
	/// </summary>
	public void SetupMethod(MethodSetup methodSetup) => Setup.Methods.Add(methodSetup);

	/// <summary>
	///     Registers the <paramref name="propertySetup" /> in the mock.
	/// </summary>
	public void SetupProperty(PropertySetup propertySetup)
		=> Setup.Properties.Add(propertySetup);

	/// <summary>
	///     Registers the <paramref name="eventSetup" /> in the mock.
	/// </summary>
	public void SetupEvent(EventSetup eventSetup)
		=> Setup.Events.Add(eventSetup);
}
