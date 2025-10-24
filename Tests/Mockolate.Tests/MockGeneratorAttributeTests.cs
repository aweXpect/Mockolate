using System;
using System.Collections.Generic;
using System.Text;
using Mockolate.Tests.TestHelpers;

namespace Mockolate.Tests;

public class MockGeneratorAttributeTests
{
	[Fact]
	public async Task WithCustomGenerator_ShouldCreateMock()
	{
		var mock = MyGenerator.MyCreator<IFoo>();
		mock.Setup.Method.Bar().Returns(42);

		var result = mock.Subject.Bar();

		await That(result).IsEqualTo(42);
		await That(mock.Verify.Invoked.Bar()).Once();
	}

	public interface IFoo
	{
		int Bar();
	}
}
