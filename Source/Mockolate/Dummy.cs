using System;
using System.Collections.Generic;
using System.Text;
using static Mockolate.With;

namespace Mockolate;

/// <summary>
/// TODO
/// </summary>
public class Dummy
{
	/// <summary>
	/// TODO
	/// </summary>
	public int MyMethod(IParameter<IMyInterface> myService, Parameter<int> value2, Parameter<int> value3, With.OutParameter<bool> isSuccess)
	{
		var matches = With2._matchObservers;
		//With.Parameter<IMyInterface> p1 = With.Value<IMyInterface>(myService);
		//With.Parameter<int> p2 = With.Value<int>(value2);
		//With.Parameter<int> p3 = With.Value<int>(value3);
		//return value2;
		return -1;
	}

	/// <summary>
	/// TODO
	/// </summary>
	public int MyMethod2(With.Parameter<int> value, With.OutParameter<bool> isSuccess)
	{
		//
		return 0;
	}
}
public class With2
{
	public class MatchObserver(Type type, Parameter parameter)
	{
		Type Type = type;
		Parameter Parameter = parameter;
	}
	[ThreadStatic]
	public static List<MatchObserver> _matchObservers = [];

	/// <summary>
	/// TODO
	/// </summary>
	public static Parameter<T> Any<T>()
	{
		_matchObservers.Add(new MatchObserver(typeof(T), With.Any<T>()));
		return default!;
	}

	public static Parameter<T> Value<T>(T value)
	{
		return null!;
	}
}

public interface IParameter<out T>
{

}
public class Parameter<T> : IParameter<T>
{
	
}

public interface IMyInterface
{
	int MyMethod(int value);
}

public class MyClass : IMyInterface
{
	public int MyMethod(int value) => 42;
}
