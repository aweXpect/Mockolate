using System.Linq;
using Mockolate.Interactions;

namespace Mockolate.Internal.Tests;

public interface IPhase54Foo
{
	int Counter { get; set; }
}

public interface IPhase54Bar
{
	bool this[string key] { get; set; }
	event System.EventHandler SomeEvent;
}

public class Phase5_4PropertyTypedBufferTests
{
	[Fact]
	public async Task PropertyGet_RoutesThroughTypedBuffer()
	{
		IPhase54Foo sut = IPhase54Foo.CreateMock();
		sut.Mock.Setup.Counter.InitializeWith(42);

		_ = sut.Counter;
		_ = sut.Counter;

		FastMockInteractions fast = (FastMockInteractions)((IMock)sut).MockRegistry.Interactions;
		int getterId = global::Mockolate.Mock.IPhase54Foo.MemberId_Counter_Get;
		FastPropertyGetterBuffer? buffer = fast.Buffers[getterId] as FastPropertyGetterBuffer;
		await That(buffer).IsNotNull();
		await That(buffer!.Count).IsEqualTo(2);
		await That(fast.Count).IsEqualTo(2);
	}

	[Fact]
	public async Task PropertySet_RoutesThroughTypedBuffer()
	{
		IPhase54Foo sut = IPhase54Foo.CreateMock();

		sut.Counter = 7;
		sut.Counter = 9;
		sut.Counter = 11;

		FastMockInteractions fast = (FastMockInteractions)((IMock)sut).MockRegistry.Interactions;
		int setterId = global::Mockolate.Mock.IPhase54Foo.MemberId_Counter_Set;
		FastPropertySetterBuffer<int>? buffer = fast.Buffers[setterId] as FastPropertySetterBuffer<int>;
		await That(buffer).IsNotNull();
		await That(buffer!.Count).IsEqualTo(3);
	}

	[Fact]
	public async Task IndexerGet_RoutesThroughTypedBuffer()
	{
		IPhase54Bar sut = IPhase54Bar.CreateMock();

		_ = sut["a"];
		_ = sut["b"];

		FastMockInteractions fast = (FastMockInteractions)((IMock)sut).MockRegistry.Interactions;
		int getterId = global::Mockolate.Mock.IPhase54Bar.MemberId_Indexer_string_Get;
		FastIndexerGetterBuffer<string>? buffer = fast.Buffers[getterId] as FastIndexerGetterBuffer<string>;
		await That(buffer).IsNotNull();
		await That(buffer!.Count).IsEqualTo(2);
	}

	[Fact]
	public async Task IndexerSet_RoutesThroughTypedBuffer()
	{
		IPhase54Bar sut = IPhase54Bar.CreateMock();

		sut["a"] = true;
		sut["b"] = false;
		sut["c"] = true;

		FastMockInteractions fast = (FastMockInteractions)((IMock)sut).MockRegistry.Interactions;
		int setterId = global::Mockolate.Mock.IPhase54Bar.MemberId_Indexer_string_Set;
		FastIndexerSetterBuffer<string, bool>? buffer = fast.Buffers[setterId] as FastIndexerSetterBuffer<string, bool>;
		await That(buffer).IsNotNull();
		await That(buffer!.Count).IsEqualTo(3);
	}

	[Fact]
	public async Task EventSubscribe_RoutesThroughTypedBuffer()
	{
		IPhase54Bar sut = IPhase54Bar.CreateMock();

		sut.SomeEvent += static (_, _) => { };
		sut.SomeEvent += static (_, _) => { };

		FastMockInteractions fast = (FastMockInteractions)((IMock)sut).MockRegistry.Interactions;
		int subscribeId = global::Mockolate.Mock.IPhase54Bar.MemberId_SomeEvent_Subscribe;
		FastEventBuffer? buffer = fast.Buffers[subscribeId] as FastEventBuffer;
		await That(buffer).IsNotNull();
		await That(buffer!.Count).IsEqualTo(2);
	}

	[Fact]
	public async Task EventUnsubscribe_RoutesThroughTypedBuffer()
	{
		IPhase54Bar sut = IPhase54Bar.CreateMock();

		System.EventHandler handler1 = (_, _) => { };
		System.EventHandler handler2 = (_, _) => { };
		sut.SomeEvent += handler1;
		sut.SomeEvent += handler2;
		sut.SomeEvent -= handler1;

		FastMockInteractions fast = (FastMockInteractions)((IMock)sut).MockRegistry.Interactions;
		int unsubscribeId = global::Mockolate.Mock.IPhase54Bar.MemberId_SomeEvent_Unsubscribe;
		FastEventBuffer? buffer = fast.Buffers[unsubscribeId] as FastEventBuffer;
		await That(buffer).IsNotNull();
		await That(buffer!.Count).IsEqualTo(1);
	}

	[Fact]
	public async Task SkipInteractionRecording_DoesNotAppendToBuffer()
	{
		MockBehavior behavior = MockBehavior.Default with { SkipInteractionRecording = true, };
		IPhase54Foo sut = IPhase54Foo.CreateMock(behavior);
		sut.Mock.Setup.Counter.InitializeWith(42);

		_ = sut.Counter;
		_ = sut.Counter;

		FastMockInteractions fast = (FastMockInteractions)((IMock)sut).MockRegistry.Interactions;
		int getterId = global::Mockolate.Mock.IPhase54Foo.MemberId_Counter_Get;
		FastPropertyGetterBuffer? buffer = fast.Buffers[getterId] as FastPropertyGetterBuffer;
		await That(buffer).IsNotNull();
		await That(buffer!.Count).IsEqualTo(0);
		await That(fast.Count).IsEqualTo(0);
	}

	[Fact]
	public async Task PropertyGet_BufferRecordsArePreservedOnEnumeration()
	{
		IPhase54Foo sut = IPhase54Foo.CreateMock();
		sut.Mock.Setup.Counter.InitializeWith(42);

		_ = sut.Counter;
		sut.Counter = 7;
		_ = sut.Counter;

		IInteraction[] all = ((IMock)sut).MockRegistry.Interactions.ToArray();
		await That(all).HasCount(3);
		await That(all[0]).IsExactly<PropertyGetterAccess>();
		await That(all[1]).IsExactly<PropertySetterAccess<int>>();
		await That(all[2]).IsExactly<PropertyGetterAccess>();
	}
}
