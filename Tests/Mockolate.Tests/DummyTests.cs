using System;
using System.Collections.Generic;
using System.Text;

namespace Mockolate.Tests;

public class DummyTests
{
	[Fact]
	public async Task X()
	{
		var sut = new MyClass();
		var dummy = new Dummy();
		dummy.MyMethod(With2.Value(sut), 2, With2.Any<int>(), With.Out(() => true));
	}
}
