using System;
using System.Collections.Generic;
using System.Text;

namespace Mockolate.Tests.TestHelpers;

public class MyServiceBase
{
	public virtual void DoSomething(int value, bool flag)
	{
		throw new NotImplementedException();
	}
}
