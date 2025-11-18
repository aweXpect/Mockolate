using System.Collections.Generic;

namespace Mockolate.Tests.MockProperties;

public sealed partial class SetupPropertyTests
{
	public sealed class OnGetTests
	{
		[Fact]
		public async Task For_ShouldStopExecutingCallbackAfterTheGivenTimes()
		{
			List<int> callCount = [];
			IPropertyService sut = Mock.Create<IPropertyService>();
			sut.SetupMock.Property.MyProperty
				.OnGet((i, _) => { callCount.Add(i); })
				.For(4);

			for (int i = 0; i < 20; i++)
			{
				_ = sut.MyProperty;
			}

			await That(callCount).IsEqualTo([0, 1, 2, 3,]);
		}

		[Fact]
		public async Task For_WithWhen_ShouldStopExecutingCallbackAfterTheGivenTimes()
		{
			List<int> callCount = [];
			IPropertyService sut = Mock.Create<IPropertyService>();
			sut.SetupMock.Property.MyProperty
				.OnGet((i, _) => { callCount.Add(i); })
				.When(x => x > 2)
				.For(4);

			for (int i = 0; i < 20; i++)
			{
				_ = sut.MyProperty;
			}

			await That(callCount).IsEqualTo([3, 4, 5, 6,]);
		}

		[Fact]
		public async Task MultipleOnGet_ShouldAllGetInvoked()
		{
			int callCount1 = 0;
			int callCount2 = 0;
			IPropertyService sut = Mock.Create<IPropertyService>();

			sut.SetupMock.Property.MyProperty
				.OnGet(() => { callCount1++; })
				.OnGet(v => { callCount2 += v; });

			sut.MyProperty = 1;
			_ = sut.MyProperty;
			sut.MyProperty = 2;
			_ = sut.MyProperty;

			await That(callCount1).IsEqualTo(2);
			await That(callCount2).IsEqualTo(3);
		}

		[Fact]
		public async Task ShouldExecuteWhenPropertyIsRead()
		{
			int callCount = 0;
			IPropertyService sut = Mock.Create<IPropertyService>();

			sut.SetupMock.Property.MyProperty
				.OnGet(() => { callCount++; });

			_ = sut.MyProperty;

			await That(callCount).IsEqualTo(1);
		}

		[Fact]
		public async Task ShouldNotExecuteWhenPropertyIsWrittenToOrOtherPropertyIsRead()
		{
			int callCount = 0;
			IPropertyService sut = Mock.Create<IPropertyService>();

			sut.SetupMock.Property.MyProperty
				.OnGet(() => { callCount++; });

			_ = sut.MyOtherProperty;
			sut.MyProperty = 1;

			await That(callCount).IsEqualTo(0);
		}

		[Fact]
		public async Task When_ShouldOnlyExecuteCallbackWhenInvocationCountMatches()
		{
			List<int> callCount = [];
			IPropertyService sut = Mock.Create<IPropertyService>();
			sut.SetupMock.Property.MyProperty
				.OnGet((i, _) => { callCount.Add(i); })
				.When(x => x is > 3 and < 9);

			for (int i = 0; i < 20; i++)
			{
				_ = sut.MyProperty;
			}

			await That(callCount).IsEqualTo([4, 5, 6, 7, 8,]);
		}

		[Fact]
		public async Task WithValue_ShouldExecuteWhenPropertyIsRead()
		{
			int callCount = 0;
			int receivedValue = 0;
			IPropertyService sut = Mock.Create<IPropertyService>();

			sut.SetupMock.Property.MyProperty
				.InitializeWith(4)
				.OnGet(v =>
				{
					callCount++;
					receivedValue = v;
				});

			_ = sut.MyProperty;

			await That(callCount).IsEqualTo(1);
			await That(receivedValue).IsEqualTo(4);
		}

		[Fact]
		public async Task WithValue_ShouldNotExecuteWhenPropertyIsWrittenToOrOtherPropertyIsRead()
		{
			int callCount = 0;
			IPropertyService sut = Mock.Create<IPropertyService>();

			sut.SetupMock.Property.MyProperty
				.OnGet(_ => { callCount++; });

			_ = sut.MyOtherProperty;
			sut.MyProperty = 1;

			await That(callCount).IsEqualTo(0);
		}
	}
}
