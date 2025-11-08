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
		mock.Raise.Protected.MyEvent(this, EventArgs.Empty);

		await That(mock.Verify.SubscribedTo.Protected.MyEvent()).Once();
		await That(mock.Verify.UnsubscribedFrom.Protected.MyEvent()).Never();
		mock.Subject.UnregisterEvent(handler);
		await That(mock.Verify.UnsubscribedFrom.Protected.MyEvent()).Once();
	}

	[Fact]
	public async Task CanAccessProtectedMethods()
	{
		Mock<MyProtectedClass> mock = Mock.Create<MyProtectedClass>();

		mock.Setup.Protected.Method.MyProtectedMethod(WithAny<string>())
			.Returns(v => $"Hello, {v}!");

		string result = mock.Subject.InvokeMyProtectedMethod("foo");

		await That(mock.Verify.Invoked.Protected.MyProtectedMethod(With("foo"))).Once();
		await That(result).IsEqualTo("Hello, foo!");
	}

	[Fact]
	public async Task CanAccessProtectedProperties()
	{
		Mock<MyProtectedClass> mock = Mock.Create<MyProtectedClass>();

		mock.Setup.Protected.Property.MyProtectedProperty.InitializeWith(42);

		int result = mock.Subject.GetMyProtectedProperty();

		await That(mock.Verify.Got.Protected.MyProtectedProperty()).Once();
		await That(result).IsEqualTo(42);
	}

	[Fact]
	public async Task CanReadProtectedIndexers()
	{
		int callCount = 0;
		Mock<MyProtectedClass> mock = Mock.Create<MyProtectedClass>();

		mock.Setup.Protected.Indexer(WithAny<int>()).InitializeWith(42).OnGet(() => callCount++);

		int result = mock.Subject.GetMyProtectedIndexer(3);

		await That(mock.Verify.GotProtectedIndexer(With(3))).Once();
		await That(result).IsEqualTo(42);
		await That(callCount).IsEqualTo(1);
	}

	[Fact]
	public async Task CanWriteProtectedIndexers()
	{
		int callCount = 0;
		Mock<MyProtectedClass> mock = Mock.Create<MyProtectedClass>();

		mock.Setup.Protected.Indexer(WithAny<int>()).OnSet(() => callCount++);

		mock.Subject.SetMyProtectedIndexer(3, 4);

		await That(mock.Verify.SetProtectedIndexer(With(3), With(4))).Once();
		await That(mock.Subject.GetMyProtectedIndexer(3)).IsEqualTo(4);
		await That(callCount).IsEqualTo(1);
	}

#pragma warning disable CS0067 // Event is never used
#pragma warning disable CA1070 // Do not declare event fields as virtual
	public abstract class MyProtectedClass
	{
		public delegate void MyEventHandler(object? sender, EventArgs e);

		protected virtual int MyProtectedProperty { get; set; }

		protected abstract int this[int key] { get; set; }

		protected virtual event MyEventHandler? MyEvent;
		protected abstract string MyProtectedMethod(string input);

		public int GetMyProtectedProperty()
			=> MyProtectedProperty;

		public int GetMyProtectedIndexer(int key)
			=> this[key];

		public void SetMyProtectedIndexer(int key, int value)
			=> this[key] = value;

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
