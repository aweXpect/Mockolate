using Mockolate.Protected;
using Mockolate.Verify;

namespace Mockolate.Tests.Protected;

public sealed class ProtectedMockTests
{
	[Fact]
	public async Task CanAccessProtectedEvents()
	{
		int callCount = 0;
		Mock<MyProtectedClass> mock = Mock.Create<MyProtectedClass>();
		MyProtectedClass.MyEventHandler handler = (s, e) => callCount++;

		mock.Subject.RegisterEvent(handler);
		mock.Protected.Raise.MyEvent(this, EventArgs.Empty);

		await That(mock.Protected.Verify.SubscribedTo.MyEvent()).Once();
		await That(mock.Protected.Verify.UnsubscribedFrom.MyEvent()).Never();
		mock.Subject.UnregisterEvent(handler);
		await That(mock.Protected.Verify.UnsubscribedFrom.MyEvent()).Once();
	}

	[Fact]
	public async Task CanAccessProtectedMethods()
	{
		Mock<MyProtectedClass> mock = Mock.Create<MyProtectedClass>();

		mock.Protected.Setup.Method.MyProtectedMethod(With.Any<string>())
			.Returns(v => $"Hello, {v}!");

		string result = mock.Subject.InvokeMyProtectedMethod("foo");

		await That(mock.Protected.Verify.Invoked.MyProtectedMethod("foo")).Once();
		await That(result).IsEqualTo("Hello, foo!");
	}

	[Fact]
	public async Task CanAccessProtectedProperties()
	{
		Mock<MyProtectedClass> mock = Mock.Create<MyProtectedClass>();

		mock.Protected.Setup.Property.MyProtectedProperty.InitializeWith(42);

		int result = mock.Subject.GetMyProtectedProperty();

		await That(mock.Protected.Verify.Got.MyProtectedProperty()).Once();
		await That(result).IsEqualTo(42);
	}

	[Fact]
	public async Task ShouldForwardIMockToInnerMock()
	{
		Mock<MyProtectedClass> mock = Mock.Create<MyProtectedClass>();
		ProtectedMock<MyProtectedClass, Mock<MyProtectedClass>> @protected = mock.Protected;
		IMock innerMock = mock;
		IMock protectedMock = @protected;

		await That(protectedMock.Behavior).IsSameAs(innerMock.Behavior);
		await That(protectedMock.Interactions).IsSameAs(innerMock.Interactions);
		await That(protectedMock.Raise).IsSameAs(@protected.Raise);
		await That(protectedMock.Setup).IsSameAs(innerMock.Setup);
		innerMock.Set("from-inner", 3);
		await That(protectedMock.Get<int>("from-inner")).IsEqualTo(3);
		protectedMock.Set("from-protected", 5);
		await That(innerMock.Get<int>("from-protected")).IsEqualTo(5);
		innerMock.SetIndexer("set-on-inner", 1, 2);
		await That(protectedMock.GetIndexer<string>(1, 2)).IsEqualTo("set-on-inner");
		protectedMock.SetIndexer("set-on-protected", 7, 8, 9);
		await That(innerMock.GetIndexer<string>(7, 8, 9)).IsEqualTo("set-on-protected");
	}

#pragma warning disable CS0067 // Event is never used
#pragma warning disable CA1070 // Do not declare event fields as virtual
	public abstract class MyProtectedClass
	{
		public delegate void MyEventHandler(object? sender, EventArgs e);

		protected virtual int MyProtectedProperty { get; set; }

		protected virtual event MyEventHandler? MyEvent;
		protected abstract string MyProtectedMethod(string input);

		public int GetMyProtectedProperty()
			=> MyProtectedProperty;

		public string InvokeMyProtectedMethod(string input)
			=> MyProtectedMethod(input);

		public void RegisterEvent(MyEventHandler callback)
			=> MyEvent += callback;

		public void UnregisterEvent(MyEventHandler callback)
			=> MyEvent -= callback;
	}
#pragma warning restore CA1070 // Do not declare event fields as virtual
#pragma warning restore CS0067 // Event is never used
}
