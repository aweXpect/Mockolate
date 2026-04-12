using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Mockolate.Interactions;
using Mockolate.Parameters;
using Mockolate.Setup;

namespace Mockolate.Internal.Tests.MinimalTest;

public class Foo
{
	[Fact]
	public async Task X()
	{
		var sut = new Mock.IFoo();
		sut.MockSetup.MyMethod(It.Is(6)).Returns("foo");

		var result1 = sut.MyMethod(6);
		var result2 = sut.MyMethod(5);

		await That(result1).IsEqualTo("foo");
		await That(result2).IsEmpty();
	}
	[Fact]
	public async Task Mockolate_New()
	{
		var sut = IFoo.CreateMock();
		sut.Mock.Setup.MyMethod(It.Is(6)).Returns("foo");

		var result1 = sut.MyMethod(6);
		var result2 = sut.MyMethod(5);

		await That(result1).IsEqualTo("foo");
		await That(result2).IsEmpty();
	}
}

public class Mock
{
	public class IFoo : Mockolate.Internal.Tests.MinimalTest.IFoo, ISetupForIFoo
	{
		private MockRegistry Registry { get; } = new(MockBehavior.Default);
		public ISetupForIFoo MockSetup => this;
		
		public string MyMethod(int value)
		{
			var setup = Registry.GetMethodSetup<global::Mockolate.Setup.ReturnMethodSetup<string, int>>(nameof(MyMethod), m => m.Matches("value", value));
			Registry.RegisterInteraction(new global::Mockolate.Interactions.MethodInvocation<int>(nameof(MyMethod), value));
			if (!(setup?.SkipBaseClass(Registry.Behavior) ?? Registry.Behavior.SkipBaseClass))
			{
				//base.MyMethod(value);
			}
			
			setup?.TriggerCallbacks(value);
			if (setup?.TryGetReturnValue(value, out var returnValue) == true)
			{
				return returnValue;
			}
			return Registry.Behavior.DefaultValue.Generate(default(string)!, [new NamedParameterValue<int>("value", value)]);
		}
#if NET8_0_OR_GREATER
		/// <inheritdoc cref="global::System.IO.Abstractions.IPath.Combine(global::System.ReadOnlySpan{string})" />
		public string Combine(params global::System.ReadOnlySpan<string> paths)
		{
			var pathsWrapper = new global::Mockolate.Setup.ReadOnlySpanWrapper<string>(paths);
			var methodSetup = this.Registry.GetMethodSetup<global::Mockolate.Setup.ReturnMethodSetup<string, global::Mockolate.Setup.ReadOnlySpanWrapper<string>>>("global::System.IO.Abstractions.IPath.Combine", m => m.Matches("paths", pathsWrapper));
			Registry.RegisterInteraction(new global::Mockolate.Interactions.MethodInvocation<global::Mockolate.Setup.ReadOnlySpanWrapper<string>>("global::System.IO.Abstractions.IPath.Combine", null));
			if (this.Registry.Wraps is IFoo wraps)
			{
				var baseResult = wraps.Combine(paths);
				if (methodSetup is not null)
				{
					methodSetup?.TriggerCallbacks(new global::Mockolate.Setup.ReadOnlySpanWrapper<string>(paths));
					return baseResult;
				}
			}
			methodSetup?.TriggerCallbacks(new global::Mockolate.Setup.ReadOnlySpanWrapper<string>(paths));
			if(methodSetup?.TryGetReturnValue(new global::Mockolate.Setup.ReadOnlySpanWrapper<string>(paths), out var returnValue) == true)
			{
				return returnValue;
			}
			return this.Registry.Behavior.DefaultValue.Generate(default(string)!);
		}
	#endif

		public string MyOtherMethod(int value, ref int v1, out bool v2)
			=> throw new NotImplementedException();

		IReturnMethodSetup<string, int> ISetupForIFoo.MyMethod(IParameter<int> value)
		{
			var methodSetup = new ReturnMethodSetup<string, int>.WithParameterCollection(Registry, nameof(MyMethod), (IParameterMatch<int>)value);
			this.Registry.SetupMethod(methodSetup);
			return methodSetup;
		}
	}

	public interface ISetupForIFoo
	{
		IReturnMethodSetup<string, int> MyMethod(IParameter<int> value);
	}

	public class MockRegistryFoo
	{
		/// <summary>
		///     Gets the collection of interactions recorded by the mock object.
		/// </summary>
		public MockInteractions Interactions { get; } = new();
		private List<IMethodSetup> _methodSetups = [];
		public MethodSetup<TReturn, T> RegisterMethod<TReturn, T>(string methodName, IParameter<T> value)
		{
			var methodSetup = new MethodSetup<TReturn, T>(methodName, value);
			_methodSetups.Add(methodSetup);
			return methodSetup;
		}

		public MethodSetup<TReturn, T>? GetMethod<TReturn, T>(string methodName, T value)
		{
			foreach (var methodSetup in _methodSetups.Where(x => x.MethodName.Equals(methodName)))
			{
				if (methodSetup is MethodSetup<TReturn, T> matching && matching.Matches(value))
				{
					return matching;
				}
			}

			return null;
		}

		public void RegisterMethodInvocation<TReturn, T1>(string methodName, T1 value, MethodSetup<TReturn, T1>? setup)
		{
			((IMockInteractions)Interactions).RegisterInteraction(new MethodInvocation2<T1>(methodName, value));
		}
	}
}
public class MethodInvocation2<T1>(string name, T1 parameter) : IInteraction, ISettableInteraction
{
	private int? _index;
	/// <summary>
	///     The name of the method.
	/// </summary>
	public string Name { get; } = name;

	/// <summary>
	///     The named parameters of the method.
	/// </summary>
	public T1 Parameter1 { get; } = parameter;

	/// <inheritdoc cref="IInteraction.Index" />
	public int Index => _index.GetValueOrDefault();

	void ISettableInteraction.SetIndex(int value) => _index ??= value;

	/// <inheritdoc cref="object.ToString()" />
	public override string ToString()
		=> $"[{Index}] invoke method {Name}({Parameter1})";
}

public interface IMethodSetup<TReturn, T>
{
	IMethodSetup<TReturn, T> Returns(TReturn value);
}

public class MethodSetup<TReturn, T1>(string methodName, IParameter<T1> parameter) : IMethodSetup<TReturn, T1>, IMethodSetup
{
	private readonly List<Callback<Action<int, T1>>> _callbacks = [];
	private readonly List<Callback<Func<int, T1, TReturn>>> _returnCallbacks = [];
	private Callback? _currentCallback;
	private int _currentCallbacksIndex;
	private Callback? _currentReturnCallback;
	private int _currentReturnCallbackIndex;

	IMethodSetup<TReturn, T1> IMethodSetup<TReturn, T1>.Returns(TReturn returnValue)
	{
		Callback<Func<int, T1, TReturn>> currentCallback = new(Delegate);
		_currentReturnCallback = currentCallback;
		_returnCallbacks.Add(currentCallback);
		return this;

		[DebuggerNonUserCode]
		TReturn Delegate(int _, T1 p1)
		{
			return returnValue;
		}
	}

	public string MethodName { get; } = methodName;

	public bool Matches(T1 value)
	{
		return (parameter as IParameter).Matches(new NamedParameterValue<T1>("", value));
	}

	public TReturn GetReturnValue(T1 p1)
	{
		foreach (Callback<Func<int, T1, TReturn>> _ in _returnCallbacks)
		{
			Callback<Func<int, T1, TReturn>> returnCallback =
				_returnCallbacks[_currentReturnCallbackIndex % _returnCallbacks.Count];
			if (returnCallback.Invoke(ref _currentReturnCallbackIndex, Callback, out TReturn? newValue))
			{
				return newValue;
			}
		}
		return default(TReturn)!;
		[DebuggerNonUserCode]
		TReturn Callback(int invocationCount, Func<int, T1, TReturn> @delegate)
		{
			return @delegate(invocationCount, p1);
		}
	}
}

public interface IMethodSetup
{
	string  MethodName { get; }
}

public interface IFoo
{
	string MyMethod(int value);
	string MyOtherMethod(int value, ref int v1, out bool v2);
#if NET8_0_OR_GREATER
	string Combine(params global::System.ReadOnlySpan<string> paths);
#endif
}

