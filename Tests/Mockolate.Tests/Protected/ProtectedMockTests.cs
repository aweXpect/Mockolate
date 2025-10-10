namespace Mockolate.Tests.Protected;

public sealed class ProtectedMockTests
{
	[Fact]
	public async Task CanAccessProtectedEvents()
	{
		int callCount = 0;
		Mock<MyProtectedClass> mock = Mock.Create<MyProtectedClass>();
		MyProtectedClass.MyEventHandler handler = (s, e) => callCount++;

		mock.Object.RegisterEvent(handler);
		mock.Protected.Raise.MyEvent(this, EventArgs.Empty);

		await That(mock.Protected.Event.MyEvent.Subscribed()).Once();
		await That(mock.Protected.Event.MyEvent.Unsubscribed()).Never();
		mock.Object.UnregisterEvent(handler);
		await That(mock.Protected.Event.MyEvent.Unsubscribed()).Once();
	}

	[Fact]
	public async Task CanAccessProtectedMethods()
	{
		Mock<MyProtectedClass> mock = Mock.Create<MyProtectedClass>();

		mock.Protected.Setup.Method.MyProtectedMethod(With.Any<string>())
			.Returns(v => $"Hello, {v}!");

		string result = mock.Object.InvokeMyProtectedMethod("foo");

		await That(mock.Protected.Invoked.MyProtectedMethod("foo")).Once();
		await That(result).IsEqualTo("Hello, foo!");
	}

	[Fact]
	public async Task CanAccessProtectedProperties()
	{
		Mock<MyProtectedClass> mock = Mock.Create<MyProtectedClass>();

		mock.Protected.Setup.Property.MyProtectedProperty.InitializeWith(42);

		int result = mock.Object.GetMyProtectedProperty();

		await That(mock.Protected.Accessed.MyProtectedProperty.Getter()).Once();
		await That(result).IsEqualTo(42);
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
