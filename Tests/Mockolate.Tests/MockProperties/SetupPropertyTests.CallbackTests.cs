namespace Mockolate.Tests.MockProperties;

public sealed partial class SetupPropertyTests
{
	public sealed class CallbackTests
	{
		[Fact]
		public async Task MultipleOnGet_ShouldAllGetInvoked()
		{
			int callCount1 = 0;
			int callCount2 = 0;
			Mock<IPropertyService> sut = Mock.Create<IPropertyService>();

			sut.Setup.Property.MyProperty
				.OnGet(() => { callCount1++; })
				.OnGet(v => { callCount2 += v; });

			sut.Subject.MyProperty = 1;
			_ = sut.Subject.MyProperty;
			sut.Subject.MyProperty = 2;
			_ = sut.Subject.MyProperty;

			await That(callCount1).IsEqualTo(2);
			await That(callCount2).IsEqualTo(3);
		}

		[Fact]
		public async Task MultipleOnSet_ShouldAllGetInvoked()
		{
			int callCount1 = 0;
			int callCount2 = 0;
			Mock<IPropertyService> sut = Mock.Create<IPropertyService>();

			sut.Setup.Property.MyProperty
				.InitializeWith(2)
				.OnSet(() => { callCount1++; })
				.OnSet((old, @new) => { callCount2 += old * @new; });

			sut.Subject.MyProperty = 4; // 2 * 4 = 8
			sut.Subject.MyProperty = 6; // 4 * 6 = 24

			await That(callCount1).IsEqualTo(2);
			await That(callCount2).IsEqualTo(8 + 24);
		}

		[Fact]
		public async Task OnGet_ShouldExecuteWhenPropertyIsRead()
		{
			int callCount = 0;
			Mock<IPropertyService> sut = Mock.Create<IPropertyService>();

			sut.Setup.Property.MyProperty
				.OnGet(() => { callCount++; });

			_ = sut.Subject.MyProperty;

			await That(callCount).IsEqualTo(1);
		}

		[Fact]
		public async Task OnGet_ShouldNotExecuteWhenPropertyIsWrittenToOrOtherPropertyIsRead()
		{
			int callCount = 0;
			Mock<IPropertyService> sut = Mock.Create<IPropertyService>();

			sut.Setup.Property.MyProperty
				.OnGet(() => { callCount++; });

			_ = sut.Subject.MyOtherProperty;
			sut.Subject.MyProperty = 1;

			await That(callCount).IsEqualTo(0);
		}

		[Fact]
		public async Task OnGet_WithValue_ShouldExecuteWhenPropertyIsRead()
		{
			int callCount = 0;
			int receivedValue = 0;
			Mock<IPropertyService> sut = Mock.Create<IPropertyService>();

			sut.Setup.Property.MyProperty
				.InitializeWith(4)
				.OnGet(v =>
				{
					callCount++;
					receivedValue = v;
				});

			_ = sut.Subject.MyProperty;

			await That(callCount).IsEqualTo(1);
			await That(receivedValue).IsEqualTo(4);
		}

		[Fact]
		public async Task OnGet_WithValue_ShouldNotExecuteWhenPropertyIsWrittenToOrOtherPropertyIsRead()
		{
			int callCount = 0;
			Mock<IPropertyService> sut = Mock.Create<IPropertyService>();

			sut.Setup.Property.MyProperty
				.OnGet(_ => { callCount++; });

			_ = sut.Subject.MyOtherProperty;
			sut.Subject.MyProperty = 1;

			await That(callCount).IsEqualTo(0);
		}

		[Fact]
		public async Task OnSet_ShouldExecuteWhenPropertyIsWrittenTo()
		{
			int callCount = 0;
			Mock<IPropertyService> sut = Mock.Create<IPropertyService>();

			sut.Setup.Property.MyProperty
				.OnSet(() => { callCount++; });

			sut.Subject.MyProperty = 5;

			await That(callCount).IsEqualTo(1);
		}

		[Fact]
		public async Task OnSet_ShouldNotExecuteWhenPropertyIsReadOrOtherPropertyIsWrittenTo()
		{
			int callCount = 0;
			Mock<IPropertyService> sut = Mock.Create<IPropertyService>();

			sut.Setup.Property.MyProperty
				.OnSet(() => { callCount++; });

			sut.Subject.MyOtherProperty = 1;
			_ = sut.Subject.MyProperty;

			await That(callCount).IsEqualTo(0);
		}

		[Fact]
		public async Task OnSet_WithValue_ShouldExecuteWhenPropertyIsWrittenTo()
		{
			int receivedOldValue = 0;
			int receivedNewValue = 0;
			int callCount = 0;
			Mock<IPropertyService> sut = Mock.Create<IPropertyService>();

			sut.Setup.Property.MyProperty
				.InitializeWith(4)
				.OnSet((oldValue, newValue) =>
				{
					callCount++;
					receivedOldValue = oldValue;
					receivedNewValue = newValue;
				});

			sut.Subject.MyProperty = 6;

			await That(callCount).IsEqualTo(1);
			await That(receivedOldValue).IsEqualTo(4);
			await That(receivedNewValue).IsEqualTo(6);
		}

		[Fact]
		public async Task OnSet_WithValue_ShouldNotExecuteWhenPropertyIsReadOrOtherPropertyIsWrittenTo()
		{
			int callCount = 0;
			Mock<IPropertyService> sut = Mock.Create<IPropertyService>();

			sut.Setup.Property.MyProperty
				.OnSet((_, _) => { callCount++; });

			sut.Subject.MyOtherProperty = 1;
			_ = sut.Subject.MyProperty;

			await That(callCount).IsEqualTo(0);
		}
	}
}
