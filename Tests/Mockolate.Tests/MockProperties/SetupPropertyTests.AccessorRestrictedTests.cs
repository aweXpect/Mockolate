using System.Collections.Generic;

namespace Mockolate.Tests.MockProperties;

public sealed partial class SetupPropertyTests
{
	public sealed class AccessorRestrictedTests
	{
		[Fact]
		public async Task SetOnlyProperty_OnSet_ShouldFireAndRecordTheWrite()
		{
			IAccessorService sut = IAccessorService.CreateMock();
			List<string> written = new();
			sut.Mock.Setup.WriteOnly.OnSet.Do(value => written.Add(value));

			sut.WriteOnly = "Ada";

			await That(written).IsEqualTo(["Ada",]);
			await That(sut.Mock.Verify.WriteOnly.Set("Ada")).Once()
				.Because("the setter facade must be keyed on the setter member id, not the getter's");
		}

		[Fact]
		public async Task SetOnlyProperty_VerifyOtherValue_ShouldNotMatch()
		{
			IAccessorService sut = IAccessorService.CreateMock();

			sut.WriteOnly = "Ada";

			await That(sut.Mock.Verify.WriteOnly.Set("Grace")).Never();
		}

		[Fact]
		public async Task SetOnlyProperty_VerifyWithParameterMatcher_ShouldMatch()
		{
			IAccessorService sut = IAccessorService.CreateMock();

			sut.WriteOnly = "Ada";

			await That(sut.Mock.Verify.WriteOnly.Set(It.IsAny<string>())).Once();
		}

		[Fact]
		public async Task SetOnlyProperty_Register_ShouldAllowWriteWithoutSetup()
		{
			IAccessorService sut = IAccessorService.CreateMock(MockBehavior.Default.ThrowingWhenNotSetup());
			sut.Mock.Setup.WriteOnly.Register();

			void Act() => sut.WriteOnly = "Ada";

			await That(Act).DoesNotThrow();
		}

		[Fact]
		public async Task GetOnlyProperty_Register_ShouldAllowReadWithoutSetup()
		{
			IAccessorService sut = IAccessorService.CreateMock(MockBehavior.Default.ThrowingWhenNotSetup());
			sut.Mock.Setup.ReadOnly.Register();

			int result = sut.ReadOnly;

			await That(result).IsEqualTo(0);
			await That(sut.Mock.Verify.ReadOnly.Got()).Once();
		}

		[Theory]
		[InlineData(false, 1)]
		[InlineData(true, 0)]
		public async Task SetOnlyClassProperty_ShouldSkipCallingBaseWhenRequested(bool skipBaseClass,
			int expectedCallCount)
		{
			AccessorService sut = AccessorService.CreateMock();
			sut.Mock.Setup.WriteOnly.SkippingBaseClass(skipBaseClass);

			sut.WriteOnly = 1;

			await That(sut.WriteOnlySetterCallCount).IsEqualTo(expectedCallCount);
		}

		public interface IAccessorService
		{
			int ReadOnly { get; }
			string WriteOnly { set; }
		}

		public class AccessorService
		{
			public int WriteOnlySetterCallCount { get; private set; }

			public virtual int WriteOnly
			{
				set => WriteOnlySetterCallCount += value;
			}
		}
	}
}
