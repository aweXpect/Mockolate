using System;
using System.Threading.Tasks;

namespace Mockolate.ExampleTests;

public class FooTests
{
	[Fact]
	public async Task X()
	{
		var y = Foo.Create<IMyInterface>();
		
		await That(y).IsNotNull();
	}
}

public interface IMyInterface
{
	bool MyFunc(int value);
}

public static class IMyInterfaceImposterExtensions
{
	extension(Foo foo)
	{
		public static IMyInterface Create<T>(MockBehavior? mockBehavior = null) where T : IMyInterface
		{
			return new MockForHappyCaseBenchmarksIMyInterface(mockBehavior ?? MockBehavior.Default);
		}
		/// <summary>
		///     Create a new mock for <typeparamref name="T" /> with the default <see cref="global::Mockolate.MockBehavior" />.
		/// </summary>
		/// <typeparam name="T">Type to mock, which can be an interface or a class.</typeparam>
		/// <remarks>
		///     Any interface type can be used for mocking, but for classes, only abstract and virtual members can be mocked.
		/// </remarks>
		public static T Create<T>(params System.Action<Setup.IMockSetup<T>>[] setups)
			where T : class
		{
			return new Foo.MockGenerator().Get<T>(null, MockBehavior.Default, setups)
			       ?? throw new Exceptions.MockException(
				       "Could not generate Mock<T>. Did the source generator run correctly?");
		}
	}
	extension(IMyInterface imposter)
	{
		public static IMyInterface CreateMock(MockBehavior mockBehavior)
		{
			return new MockForHappyCaseBenchmarksIMyInterface(mockBehavior);
		}
	}
}
public class Foo
{
	internal partial struct MockGenerator
	{
#pragma warning disable CS0649 // Field 'field' is never assigned to, and will always have its default value 'value' 
		private object? _value;
#pragma warning restore CS0649

		public T? Get<T>(BaseClass.ConstructorParameters? constructorParameters, MockBehavior mockBehavior, System.Action<Setup.IMockSetup<T>>[] setups)
			where T : class
		{
			Generate<T>(constructorParameters, mockBehavior, setups, typeof(T));
			return _value as T;
		}
		
		public void Generate<T>(BaseClass.ConstructorParameters? constructorParameters, MockBehavior mockBehavior, System.Action<Setup.IMockSetup<T>>[] setups, params System.Type[] types)
		{
			IMockBehaviorAccess mockBehaviorAccess = (IMockBehaviorAccess)mockBehavior;
			if (mockBehaviorAccess.TryInitialize<T>(out System.Action<Setup.IMockSetup<T>>[]? additionalSetups))
			{
				if (setups.Length > 0)
				{
					System.Action<Setup.IMockSetup<T>>[] concatenatedSetups = new System.Action<Setup.IMockSetup<T>>[additionalSetups.Length + setups.Length];
					additionalSetups.CopyTo(concatenatedSetups, 0);
					setups.CopyTo(concatenatedSetups, additionalSetups.Length);
					setups = concatenatedSetups;
				}
				else
				{
					setups = additionalSetups;
				}
			}

			if (constructorParameters is null && mockBehaviorAccess.TryGetConstructorParameters<T>(out object?[]? parameters))
			{
				constructorParameters = new BaseClass.ConstructorParameters(parameters);
			}

			if (types.Length == 1 &&
			    types[0] == typeof(IMyInterface))
			{
				_value = new MockForHappyCaseBenchmarksIMyInterface(mockBehavior);
				if (setups.Length > 0)
				{
					Setup.IMockSetup<IMyInterface> setupTarget = ((IMockSubject<IMyInterface>)_value).Mock;
					foreach (System.Action<Setup.IMockSetup<IMyInterface>> setup in setups)
					{
						setup.Invoke(setupTarget);
					}
				}
			}
		}
	}
}

/// <summary>
///     A mock implementing <see cref="IMyInterface" />.
/// </summary>
internal class MockForHappyCaseBenchmarksIMyInterface : IMyInterface,
	IMockSubject<IMyInterface>
{
	/// <inheritdoc cref="IMockSubject{T}.Mock" />
	Mock<IMyInterface> IMockSubject<IMyInterface>.Mock => _mock;
	[System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
	private readonly Mock<IMyInterface> _mock;
	private readonly IMyInterface? _wrapped;

	/// <inheritdoc cref="global::Mockolate.IHasMockRegistration.Registrations" />
	[System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
	MockRegistration IHasMockRegistration.Registrations => _mock.Registrations;
	[System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
	private MockRegistration MockRegistrations => _mock.Registrations;

	/// <inheritdoc cref="MockForHappyCaseBenchmarksIMyInterface" />
	public MockForHappyCaseBenchmarksIMyInterface(MockBehavior mockBehavior, IMyInterface? wrapped = null)
	{
		_mock = new Mock<IMyInterface>(this, new MockRegistration(mockBehavior, "IMyInterface"));
		_wrapped = wrapped;
	}

	#region IMyInterface
	/// <inheritdoc cref="IMyInterface.MyFunc(int)" />
	public bool MyFunc(int value)
	{
		Setup.MethodSetupResult<bool> methodExecution = MockRegistrations.InvokeMethod<bool>("IMyInterface.MyFunc", p => MockRegistrations.Behavior.DefaultValue.Generate(default(bool)!, p), new Parameters.NamedParameterValue("value", value));
		if (_wrapped is not null)
		{
			var baseResult = _wrapped.MyFunc(value);
			if (!methodExecution.HasSetupResult)
			{
				methodExecution.TriggerCallbacks(value);
				return baseResult;
			}
		}
		methodExecution.TriggerCallbacks(value);
		return methodExecution.Result;
	}
	#endregion IMyInterface
}
