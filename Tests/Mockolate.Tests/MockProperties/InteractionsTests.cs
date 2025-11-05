using Mockolate.Interactions;
using Mockolate.Tests.TestHelpers;
using Mockolate.Verify;

namespace Mockolate.Tests.MockProperties;
public sealed class InteractionsTests
{
	[Fact]
	public async Task MockGot_WhenNameDoesNotMatch_ShouldReturnNever()
	{
		MockInteractions mockInteractions = new();
		IMockInteractions interactions = mockInteractions;
		MockVerify<int, Mock<int>> verify = new(mockInteractions, new MyMock<int>(1), "MyMock");
		IMockGot<IMockVerify<int, Mock<int>>> mockGot = verify;
		interactions.RegisterInteraction(new PropertyGetterAccess(0, "foo.bar"));

		VerificationResult<IMockVerify<int, Mock<int>>> result = mockGot.Property("baz.bar");

		await That(result).Never();
	}

	[Fact]
	public async Task MockGot_WhenNameMatches_ShouldReturnOnce()
	{
		MockInteractions mockInteractions = new();
		IMockInteractions interactions = mockInteractions;
		MockVerify<int, Mock<int>> verify = new(mockInteractions, new MyMock<int>(1), "MyMock");
		IMockGot<IMockVerify<int, Mock<int>>> mockGot = verify;
		interactions.RegisterInteraction(new PropertyGetterAccess(0, "foo.bar"));

		VerificationResult<IMockVerify<int, Mock<int>>> result = mockGot.Property("foo.bar");

		await That(result).Once();
	}

	[Fact]
	public async Task MockGot_WithoutInteractions_ShouldReturnNeverResult()
	{
		MockInteractions mockInteractions = new();
		MockVerify<int, Mock<int>> verify = new(mockInteractions, new MyMock<int>(1), "MyMock");
		IMockGot<IMockVerify<int, Mock<int>>> mockGot = verify;

		VerificationResult<IMockVerify<int, Mock<int>>> result = mockGot.Property("foo.bar");

		await That(result).Never();
		await That(((IVerificationResult)result).Expectation).IsEqualTo("got property bar");
	}

	[Fact]
	public async Task MockSet_WhenNameAndValueMatches_ShouldReturnOnce()
	{
		MockInteractions mockInteractions = new();
		IMockInteractions interactions = mockInteractions;
		MockVerify<int, Mock<int>> verify = new(mockInteractions, new MyMock<int>(1), "MyMock");
		IMockSet<IMockVerify<int, Mock<int>>> mockSet = verify;
		interactions.RegisterInteraction(new PropertySetterAccess(0, "foo.bar", 4));

		VerificationResult<IMockVerify<int, Mock<int>>> result = mockSet.Property("foo.bar", With.Any<int>());

		await That(result).Once();
	}

	[Fact]
	public async Task MockSet_WhenOnlyNameMatches_ShouldReturnNever()
	{
		MockInteractions mockInteractions = new();
		IMockInteractions interactions = mockInteractions;
		MockVerify<int, Mock<int>> verify = new(mockInteractions, new MyMock<int>(1), "MyMock");
		IMockSet<IMockVerify<int, Mock<int>>> mockSet = verify;
		interactions.RegisterInteraction(new PropertySetterAccess(0, "foo.bar", 4));

		VerificationResult<IMockVerify<int, Mock<int>>> result = mockSet.Property("foo.bar", With.Any<string>());

		await That(result).Never();
	}

	[Fact]
	public async Task MockSet_WhenOnlyValueMatches_ShouldReturnNever()
	{
		MockInteractions mockInteractions = new();
		IMockInteractions interactions = mockInteractions;
		MockVerify<int, Mock<int>> verify = new(mockInteractions, new MyMock<int>(1), "MyMock");
		IMockSet<IMockVerify<int, Mock<int>>> mockSet = verify;
		interactions.RegisterInteraction(new PropertySetterAccess(0, "foo.bar", 4));

		VerificationResult<IMockVerify<int, Mock<int>>> result = mockSet.Property("baz.bar", With.Any<int>());

		await That(result).Never();
	}

	[Fact]
	public async Task MockSet_WithoutInteractions_ShouldReturnNeverResult()
	{
		MockInteractions mockInteractions = new();
		MockVerify<int, Mock<int>> verify = new(mockInteractions, new MyMock<int>(1), "MyMock");
		IMockSet<IMockVerify<int, Mock<int>>> mockSet = verify;

		VerificationResult<IMockVerify<int, Mock<int>>> result = mockSet.Property("foo.bar", With.Any<int>());

		await That(result).Never();
		await That(((IVerificationResult)result).Expectation).IsEqualTo("set property bar to value With.Any<int>()");
	}

	[Fact]
	public async Task PropertyGetterAccess_ToString_ShouldReturnExpectedValue()
	{
		PropertyGetterAccess interaction = new(3, "SomeProperty");
		string expectedValue = "[3] get property SomeProperty";

		await That(interaction.ToString()).IsEqualTo(expectedValue);
	}

	[Fact]
	public async Task PropertySetterAccess_ToString_ShouldReturnExpectedValue()
	{
		PropertySetterAccess interaction = new(3, "SomeProperty", 5);
		string expectedValue = "[3] set property SomeProperty to 5";

		await That(interaction.ToString()).IsEqualTo(expectedValue);
	}

	[Fact]
	public async Task PropertySetterAccess_ToString_WithNull_ShouldReturnExpectedValue()
	{
		PropertySetterAccess interaction = new(4, "SomeProperty", null);
		string expectedValue = "[4] set property SomeProperty to null";

		await That(interaction.ToString()).IsEqualTo(expectedValue);
	}
}
