namespace Mockolate.Tests.Protected;

public sealed class ProtectedMockTests
{
	[Test]
	public async Task CanAccessProtectedEvents()
	{
		int callCount = 0;
		MyProtectedClass mock = Mock.Create<MyProtectedClass>();
		MyProtectedClass.MyEventHandler handler = (_, _) => callCount++;

		mock.RegisterEvent(handler);
		mock.RaiseOnMock.Protected.MyEvent(this, EventArgs.Empty);

		await That(mock.VerifyMock.SubscribedTo.Protected.MyEvent()).Once();
		await That(mock.VerifyMock.UnsubscribedFrom.Protected.MyEvent()).Never();
		mock.UnregisterEvent(handler);
		await That(mock.VerifyMock.UnsubscribedFrom.Protected.MyEvent()).Once();
		await That(callCount).IsEqualTo(1);
	}

	[Test]
	public async Task CanAccessProtectedMethods()
	{
		MyProtectedClass mock = Mock.Create<MyProtectedClass>();

		mock.SetupMock.Protected.Method.MyProtectedMethod(It.IsAny<string>())
			.Returns(v => $"Hello, {v}!");

		string result = mock.InvokeMyProtectedMethod("foo");

		await That(mock.VerifyMock.Invoked.Protected.MyProtectedMethod(It.Is("foo"))).Once();
		await That(result).IsEqualTo("Hello, foo!");
	}

	[Test]
	public async Task CanAccessProtectedProperties()
	{
		MyProtectedClass mock = Mock.Create<MyProtectedClass>();

		mock.SetupMock.Protected.Property.MyProtectedProperty.InitializeWith(42);

		int result = mock.GetMyProtectedProperty();

		await That(mock.VerifyMock.Got.Protected.MyProtectedProperty()).Once();
		await That(result).IsEqualTo(42);
	}

	[Test]
	public async Task CanReadProtectedIndexers()
	{
		int callCount = 0;
		MyProtectedClass mock = Mock.Create<MyProtectedClass>();

		mock.SetupMock.Protected.Indexer(It.IsAny<int>()).InitializeWith(42).OnGet.Do(() => callCount++);

		int result = mock.GetMyProtectedIndexer(3);

		await That(mock.VerifyMock.GotProtectedIndexer(It.Is(3))).Once();
		await That(result).IsEqualTo(42);
		await That(callCount).IsEqualTo(1);
	}

	[Test]
	public async Task CanWriteProtectedIndexers()
	{
		int callCount = 0;
		MyProtectedClass mock = Mock.Create<MyProtectedClass>();

		mock.SetupMock.Protected.Indexer(It.IsAny<int>()).OnSet.Do(() => callCount++);

		mock.SetMyProtectedIndexer(3, 4);

		await That(mock.VerifyMock.SetProtectedIndexer(It.Is(3), It.Is(4))).Once();
		await That(mock.GetMyProtectedIndexer(3)).IsEqualTo(4);
		await That(callCount).IsEqualTo(1);
	}

	[Test]
	public async Task OnlyProtectedVirtualMembers()
	{
		OnlyProtectedVirtualMembersService mock = Mock.Create<OnlyProtectedVirtualMembersService>();

		bool result1 = mock.DoValidate(-1);

		mock.SetupMock.Protected.Method.Validate(It.IsAny<int>()).Returns(true);

		bool result2 = mock.DoValidate(-1);

		await That(result1).IsFalse();
		await That(result2).IsTrue();
	}

	public abstract class OnlyProtectedVirtualMembersService
	{
		public bool DoValidate(int value)
			=> Validate(value);

		protected virtual bool Validate(int value)
			=> value > 0;
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
