using System.Reflection;
using Mockolate.Interactions;
using Mockolate.Parameters;

namespace Mockolate.Internal.Tests.Interactions;

public class FastBufferConsumeMatchingTests
{
	[Test]
	public async Task FastEventBuffer_ConsumeMatching_ShouldReturnCount()
	{
		FastMockInteractions store = new(1);
		FastEventBuffer buffer = store.InstallEventSubscribe(0);

		await That(buffer.ConsumeMatching()).IsEqualTo(0);

		MethodInfo method = typeof(FastBufferConsumeMatchingTests).GetMethod(
			nameof(FastEventBuffer_ConsumeMatching_ShouldReturnCount))!;
		buffer.Append("OnFoo", null, method);
		buffer.Append("OnFoo", null, method);

		await That(buffer.ConsumeMatching()).IsEqualTo(2);
	}

	[Test]
	public async Task FastIndexerGetterBuffer1_ConsumeMatching_ShouldHonorMatcher()
	{
		FastMockInteractions store = new(1);
		FastIndexerGetterBuffer<int> buffer = store.InstallIndexerGetter<int>(0);

		buffer.Append(1);
		buffer.Append(2);
		buffer.Append(1);

		await That(buffer.ConsumeMatching((IParameterMatch<int>)It.IsAny<int>())).IsEqualTo(3);
		await That(buffer.ConsumeMatching((IParameterMatch<int>)It.Is(1))).IsEqualTo(2);
		await That(buffer.ConsumeMatching((IParameterMatch<int>)It.Is(99))).IsEqualTo(0);
	}

	[Test]
	public async Task FastIndexerGetterBuffer2_ConsumeMatching_ShouldHonorAllMatchers()
	{
		FastMockInteractions store = new(1);
		FastIndexerGetterBuffer<int, string> buffer = store.InstallIndexerGetter<int, string>(0);

		buffer.Append(1, "a");
		buffer.Append(1, "b");
		buffer.Append(2, "a");

		await That(buffer.ConsumeMatching(
			(IParameterMatch<int>)It.IsAny<int>(),
			(IParameterMatch<string>)It.IsAny<string>())).IsEqualTo(3);
		await That(buffer.ConsumeMatching(
			(IParameterMatch<int>)It.Is(1),
			(IParameterMatch<string>)It.Is<string>("a"))).IsEqualTo(1);
	}

	[Test]
	public async Task FastIndexerGetterBuffer3_ConsumeMatching_ShouldHonorAllMatchers()
	{
		FastMockInteractions store = new(1);
		FastIndexerGetterBuffer<int, string, bool> buffer = store.InstallIndexerGetter<int, string, bool>(0);

		buffer.Append(1, "a", true);
		buffer.Append(2, "a", false);
		buffer.Append(1, "b", true);

		await That(buffer.ConsumeMatching(
			(IParameterMatch<int>)It.IsAny<int>(),
			(IParameterMatch<string>)It.IsAny<string>(),
			(IParameterMatch<bool>)It.IsAny<bool>())).IsEqualTo(3);
		await That(buffer.ConsumeMatching(
			(IParameterMatch<int>)It.Is(1),
			(IParameterMatch<string>)It.IsAny<string>(),
			(IParameterMatch<bool>)It.Is(true))).IsEqualTo(2);
		await That(buffer.ConsumeMatching(
			(IParameterMatch<int>)It.Is(1),
			(IParameterMatch<string>)It.Is<string>("a"),
			(IParameterMatch<bool>)It.Is(false))).IsEqualTo(0);
	}

	[Test]
	public async Task FastIndexerGetterBuffer4_ConsumeMatching_ShouldHonorAllMatchers()
	{
		FastMockInteractions store = new(1);
		FastIndexerGetterBuffer<int, string, bool, double> buffer =
			store.InstallIndexerGetter<int, string, bool, double>(0);

		buffer.Append(1, "a", true, 1.0);
		buffer.Append(1, "b", true, 2.0);
		buffer.Append(2, "a", false, 3.0);

		await That(buffer.ConsumeMatching(
			(IParameterMatch<int>)It.IsAny<int>(),
			(IParameterMatch<string>)It.IsAny<string>(),
			(IParameterMatch<bool>)It.IsAny<bool>(),
			(IParameterMatch<double>)It.IsAny<double>())).IsEqualTo(3);
		await That(buffer.ConsumeMatching(
			(IParameterMatch<int>)It.Is(1),
			(IParameterMatch<string>)It.IsAny<string>(),
			(IParameterMatch<bool>)It.IsAny<bool>(),
			(IParameterMatch<double>)It.IsAny<double>())).IsEqualTo(2);
		await That(buffer.ConsumeMatching(
			(IParameterMatch<int>)It.Is(99),
			(IParameterMatch<string>)It.IsAny<string>(),
			(IParameterMatch<bool>)It.IsAny<bool>(),
			(IParameterMatch<double>)It.IsAny<double>())).IsEqualTo(0);
	}

	[Test]
	public async Task FastIndexerSetterBuffer1_ConsumeMatching_ShouldHonorMatchers()
	{
		FastMockInteractions store = new(1);
		FastIndexerSetterBuffer<int, string> buffer = store.InstallIndexerSetter<int, string>(0);

		buffer.Append(1, "a");
		buffer.Append(1, "b");
		buffer.Append(2, "a");

		await That(buffer.ConsumeMatching(
			(IParameterMatch<int>)It.IsAny<int>(),
			(IParameterMatch<string>)It.IsAny<string>())).IsEqualTo(3);
		await That(buffer.ConsumeMatching(
			(IParameterMatch<int>)It.Is(1),
			(IParameterMatch<string>)It.Is<string>("a"))).IsEqualTo(1);
		await That(buffer.ConsumeMatching(
			(IParameterMatch<int>)It.Is(99),
			(IParameterMatch<string>)It.IsAny<string>())).IsEqualTo(0);
	}

	[Test]
	public async Task FastIndexerSetterBuffer2_ConsumeMatching_ShouldHonorAllMatchers()
	{
		FastMockInteractions store = new(1);
		FastIndexerSetterBuffer<int, string, bool> buffer =
			store.InstallIndexerSetter<int, string, bool>(0);

		buffer.Append(1, "a", true);
		buffer.Append(1, "b", false);
		buffer.Append(2, "a", true);

		await That(buffer.ConsumeMatching(
			(IParameterMatch<int>)It.IsAny<int>(),
			(IParameterMatch<string>)It.IsAny<string>(),
			(IParameterMatch<bool>)It.IsAny<bool>())).IsEqualTo(3);
		await That(buffer.ConsumeMatching(
			(IParameterMatch<int>)It.Is(1),
			(IParameterMatch<string>)It.IsAny<string>(),
			(IParameterMatch<bool>)It.Is(true))).IsEqualTo(1);
		await That(buffer.ConsumeMatching(
			(IParameterMatch<int>)It.Is(99),
			(IParameterMatch<string>)It.IsAny<string>(),
			(IParameterMatch<bool>)It.IsAny<bool>())).IsEqualTo(0);
	}

	[Test]
	public async Task FastIndexerSetterBuffer3_ConsumeMatching_ShouldHonorAllMatchers()
	{
		FastMockInteractions store = new(1);
		FastIndexerSetterBuffer<int, string, bool, double> buffer =
			store.InstallIndexerSetter<int, string, bool, double>(0);

		buffer.Append(1, "a", true, 1.0);
		buffer.Append(1, "b", true, 2.0);
		buffer.Append(2, "a", false, 1.0);

		await That(buffer.ConsumeMatching(
			(IParameterMatch<int>)It.IsAny<int>(),
			(IParameterMatch<string>)It.IsAny<string>(),
			(IParameterMatch<bool>)It.IsAny<bool>(),
			(IParameterMatch<double>)It.IsAny<double>())).IsEqualTo(3);
		await That(buffer.ConsumeMatching(
			(IParameterMatch<int>)It.Is(1),
			(IParameterMatch<string>)It.IsAny<string>(),
			(IParameterMatch<bool>)It.Is(true),
			(IParameterMatch<double>)It.Is(1.0))).IsEqualTo(1);
		await That(buffer.ConsumeMatching(
			(IParameterMatch<int>)It.Is(99),
			(IParameterMatch<string>)It.IsAny<string>(),
			(IParameterMatch<bool>)It.IsAny<bool>(),
			(IParameterMatch<double>)It.IsAny<double>())).IsEqualTo(0);
	}

	[Test]
	public async Task FastIndexerSetterBuffer4_ConsumeMatching_ShouldHonorAllMatchers()
	{
		FastMockInteractions store = new(1);
		FastIndexerSetterBuffer<int, string, bool, double, char> buffer =
			store.InstallIndexerSetter<int, string, bool, double, char>(0);

		buffer.Append(1, "a", true, 1.0, 'x');
		buffer.Append(1, "b", true, 2.0, 'x');
		buffer.Append(2, "a", false, 3.0, 'y');

		await That(buffer.ConsumeMatching(
			(IParameterMatch<int>)It.IsAny<int>(),
			(IParameterMatch<string>)It.IsAny<string>(),
			(IParameterMatch<bool>)It.IsAny<bool>(),
			(IParameterMatch<double>)It.IsAny<double>(),
			(IParameterMatch<char>)It.IsAny<char>())).IsEqualTo(3);
		await That(buffer.ConsumeMatching(
			(IParameterMatch<int>)It.Is(1),
			(IParameterMatch<string>)It.IsAny<string>(),
			(IParameterMatch<bool>)It.IsAny<bool>(),
			(IParameterMatch<double>)It.IsAny<double>(),
			(IParameterMatch<char>)It.Is('x'))).IsEqualTo(2);
		await That(buffer.ConsumeMatching(
			(IParameterMatch<int>)It.Is(99),
			(IParameterMatch<string>)It.IsAny<string>(),
			(IParameterMatch<bool>)It.IsAny<bool>(),
			(IParameterMatch<double>)It.IsAny<double>(),
			(IParameterMatch<char>)It.IsAny<char>())).IsEqualTo(0);
	}

	[Test]
	public async Task FastMethod0Buffer_ConsumeMatching_ShouldReturnCount()
	{
		FastMockInteractions store = new(1);
		FastMethod0Buffer buffer = store.InstallMethod(0);

		await That(buffer.ConsumeMatching()).IsEqualTo(0);

		buffer.Append("Foo");
		buffer.Append("Foo");
		buffer.Append("Foo");

		await That(buffer.ConsumeMatching()).IsEqualTo(3);
	}

	[Test]
	public async Task FastMethod1Buffer_ConsumeMatching_ShouldHonorMatcher()
	{
		FastMockInteractions store = new(1);
		FastMethod1Buffer<int> buffer = store.InstallMethod<int>(0);

		buffer.Append("Foo", 1);
		buffer.Append("Foo", 2);
		buffer.Append("Foo", 3);

		await That(buffer.ConsumeMatching((IParameterMatch<int>)It.IsAny<int>())).IsEqualTo(3);
		await That(buffer.ConsumeMatching((IParameterMatch<int>)It.Is(2))).IsEqualTo(1);
		await That(buffer.ConsumeMatching((IParameterMatch<int>)It.Is(99))).IsEqualTo(0);
	}

	[Test]
	public async Task FastMethod2Buffer_ConsumeMatching_ShouldHonorAllMatchers()
	{
		FastMockInteractions store = new(1);
		FastMethod2Buffer<int, string> buffer = store.InstallMethod<int, string>(0);

		buffer.Append("Foo", 1, "a");
		buffer.Append("Foo", 1, "b");
		buffer.Append("Foo", 2, "a");

		await That(buffer.ConsumeMatching(
			(IParameterMatch<int>)It.IsAny<int>(),
			(IParameterMatch<string>)It.IsAny<string>())).IsEqualTo(3);
		await That(buffer.ConsumeMatching(
			(IParameterMatch<int>)It.Is(1),
			(IParameterMatch<string>)It.IsAny<string>())).IsEqualTo(2);
		await That(buffer.ConsumeMatching(
			(IParameterMatch<int>)It.Is(1),
			(IParameterMatch<string>)It.Is<string>("a"))).IsEqualTo(1);
		await That(buffer.ConsumeMatching(
			(IParameterMatch<int>)It.Is(99),
			(IParameterMatch<string>)It.IsAny<string>())).IsEqualTo(0);
	}

	[Test]
	public async Task FastMethod3Buffer_ConsumeMatching_ShouldHonorAllMatchers()
	{
		FastMockInteractions store = new(1);
		FastMethod3Buffer<int, string, bool> buffer = store.InstallMethod<int, string, bool>(0);

		buffer.Append("Foo", 1, "a", true);
		buffer.Append("Foo", 2, "a", false);
		buffer.Append("Foo", 1, "b", true);

		await That(buffer.ConsumeMatching(
			(IParameterMatch<int>)It.IsAny<int>(),
			(IParameterMatch<string>)It.IsAny<string>(),
			(IParameterMatch<bool>)It.IsAny<bool>())).IsEqualTo(3);
		await That(buffer.ConsumeMatching(
			(IParameterMatch<int>)It.Is(1),
			(IParameterMatch<string>)It.IsAny<string>(),
			(IParameterMatch<bool>)It.Is(true))).IsEqualTo(2);
		await That(buffer.ConsumeMatching(
			(IParameterMatch<int>)It.Is(1),
			(IParameterMatch<string>)It.IsAny<string>(),
			(IParameterMatch<bool>)It.Is(false))).IsEqualTo(0);
		await That(buffer.ConsumeMatching(
			(IParameterMatch<int>)It.IsAny<int>(),
			(IParameterMatch<string>)It.Is<string>("b"),
			(IParameterMatch<bool>)It.Is(false))).IsEqualTo(0);
	}

	[Test]
	public async Task FastMethod4Buffer_ConsumeMatching_ShouldHonorAllMatchers()
	{
		FastMockInteractions store = new(1);
		FastMethod4Buffer<int, string, bool, double> buffer =
			store.InstallMethod<int, string, bool, double>(0);

		buffer.Append("Foo", 1, "a", true, 1.0);
		buffer.Append("Foo", 1, "b", true, 2.0);
		buffer.Append("Foo", 2, "a", false, 3.0);

		await That(buffer.ConsumeMatching(
			(IParameterMatch<int>)It.IsAny<int>(),
			(IParameterMatch<string>)It.IsAny<string>(),
			(IParameterMatch<bool>)It.IsAny<bool>(),
			(IParameterMatch<double>)It.IsAny<double>())).IsEqualTo(3);
		await That(buffer.ConsumeMatching(
			(IParameterMatch<int>)It.Is(1),
			(IParameterMatch<string>)It.IsAny<string>(),
			(IParameterMatch<bool>)It.IsAny<bool>(),
			(IParameterMatch<double>)It.IsAny<double>())).IsEqualTo(2);
		await That(buffer.ConsumeMatching(
			(IParameterMatch<int>)It.Is(99),
			(IParameterMatch<string>)It.IsAny<string>(),
			(IParameterMatch<bool>)It.IsAny<bool>(),
			(IParameterMatch<double>)It.IsAny<double>())).IsEqualTo(0);
		await That(buffer.ConsumeMatching(
			(IParameterMatch<int>)It.Is(1),
			(IParameterMatch<string>)It.IsAny<string>(),
			(IParameterMatch<bool>)It.Is(false),
			(IParameterMatch<double>)It.IsAny<double>())).IsEqualTo(0);
		await That(buffer.ConsumeMatching(
			(IParameterMatch<int>)It.IsAny<int>(),
			(IParameterMatch<string>)It.IsAny<string>(),
			(IParameterMatch<bool>)It.IsAny<bool>(),
			(IParameterMatch<double>)It.Is(99.0))).IsEqualTo(0);
	}

	[Test]
	public async Task FastPropertyGetterBuffer_ConsumeMatching_ShouldReturnCount()
	{
		FastMockInteractions store = new(1);
		FastPropertyGetterBuffer buffer = store.InstallPropertyGetter(0);

		await That(buffer.ConsumeMatching()).IsEqualTo(0);

		buffer.Append("Bar");
		buffer.Append("Bar");

		await That(buffer.ConsumeMatching()).IsEqualTo(2);
	}

	[Test]
	public async Task FastPropertySetterBuffer_ConsumeMatching_ShouldHonorMatcher()
	{
		FastMockInteractions store = new(1);
		FastPropertySetterBuffer<int> buffer = store.InstallPropertySetter<int>(0);

		buffer.Append("Bar", 1);
		buffer.Append("Bar", 2);
		buffer.Append("Bar", 1);

		await That(buffer.ConsumeMatching((IParameterMatch<int>)It.IsAny<int>())).IsEqualTo(3);
		await That(buffer.ConsumeMatching((IParameterMatch<int>)It.Is(1))).IsEqualTo(2);
		await That(buffer.ConsumeMatching((IParameterMatch<int>)It.Is(99))).IsEqualTo(0);
	}
}
