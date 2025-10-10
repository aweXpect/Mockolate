﻿using Mockolate.Interactions;
using Mockolate.Tests.TestHelpers;
using Mockolate.Verify;

namespace Mockolate.Tests.Checks;

public sealed partial class MockSetTests
{
	public sealed class ProtectedTests
	{
		[Fact]
		public void ShouldForwardToInner()
		{
			MockInteractions mockInteractions = new();
			IMockInteractions interactions = mockInteractions;
			MyMock<int> mock = new(1);
			MockVerify<int, Mock<int>> verify = new(mockInteractions, mock);
			IMockSet<Mock<int>> mockSet = new MockSet<int, Mock<int>>(verify);
			IMockSet<Mock<int>> @protected = new MockSet<int, Mock<int>>.Protected(verify);
			interactions.RegisterInteraction(new PropertySetterAccess(0, "foo.bar", 1));
			interactions.RegisterInteraction(new PropertySetterAccess(1, "foo.bar", 2));

			VerificationResult<Mock<int>> result1 = mockSet.Property("foo.bar", With.Any<int>());
			VerificationResult<Mock<int>> result2 = @protected.Property("foo.bar", With.Any<int>());

			result1.Exactly(2);
			result2.Exactly(2);
		}
	}
}
