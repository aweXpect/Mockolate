using Mockerade.Exceptions;
using Mockerade.Tests.TestHelpers;

namespace Mockerade.Tests;

public sealed partial class MockTests
{
	[Fact]
	public async Task Behavior_ShouldBeSet()
	{
		var sut = new MyMock<string>("", MockBehavior.Default with
		{
			ThrowWhenNotSetup = true
		});

		await That(sut.Hidden.Behavior.ThrowWhenNotSetup).IsTrue();
	}

	[Fact]
	public async Task ShouldSupportImplicitOperator()
	{
		MyMock<string> sut = new("foo");

		string value = sut;

		await That(value).IsEqualTo("foo");
	}

	[Fact]
	public async Task WithTwoGenericArguments_WhenSecondIsNoInterface_ShouldThrowMockException()
	{
		void Act()
			=> _ = new MyMock<IMyService, MyBaseClass>();

		await That(Act).Throws<MockException>()
			.WithMessage("""
			             The second generic type argument 'Mockerade.Tests.MyBaseClass' is no interface.
			             """);
	}
}

public interface IMyService
{
	public bool? IsValid { get; set; }
	public int Counter { get; set; }

	public int Double(int value);

	public void SetIsValid(bool isValid);
}

public class MyBaseClass
{
	public virtual string VirtualMethod() => "Base";
}
