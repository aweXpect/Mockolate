using System.Collections.Generic;
using System.Linq;
using Mockolate.Tests.TestHelpers;
using static Mockolate.Match;

namespace Mockolate.Tests;

public sealed class MockWrapTests
{
	[Fact]
	public async Task Wrap_Interface_ShouldDelegateToWrappedInstance()
	{
		// Arrange
		var realService = new MyServiceImplementation();
		
		// Act
		IMyService wrapped = Mock.Wrap<IMyService>(realService);
		string result = wrapped.DoSomethingAndReturn(42);
		
		// Assert
		await That(result).IsEqualTo("Executed with value: 42");
	}

	[Fact]
	public async Task Wrap_WithSetup_ShouldOverrideMethod()
	{
		// Arrange
		var realService = new MyServiceImplementation();
		IMyService wrapped = Mock.Wrap<IMyService>(realService);
		
		// Act - Setup override
		wrapped.SetupMock.Method.DoSomethingAndReturn(With(42)).Returns(() => "Mocked!");
		string result = wrapped.DoSomethingAndReturn(42);
		
		// Assert
		await That(result).IsEqualTo("Mocked!");
	}

	[Fact]
	public async Task Wrap_WithoutSetup_ShouldDelegateToWrappedInstance()
	{
		// Arrange
		var realService = new MyServiceImplementation();
		IMyService wrapped = Mock.Wrap<IMyService>(realService);
		
		// Setup override for one method
		wrapped.SetupMock.Method.DoSomethingAndReturn(With(42)).Returns(() => "Mocked!");
		
		// Act - Call different method (not setup)
		bool isValid = wrapped.IsValid(5);
		
		// Assert - Should delegate to real instance
		await That(isValid).IsTrue();
	}

	[Fact]
	public async Task Wrap_ShouldSupportVerification()
	{
		// Arrange
		var realService = new MyServiceImplementation();
		IMyService wrapped = Mock.Wrap<IMyService>(realService);
		
		// Act
		wrapped.DoSomethingAndReturn(42);
		wrapped.DoSomethingAndReturn(43);
		
		// Assert - Verify interactions
		await That(wrapped.VerifyMock.Invoked.DoSomethingAndReturn(With(42))).Once();
		await That(wrapped.VerifyMock.Invoked.DoSomethingAndReturn(With(43))).Once();
	}

	[Fact]
	public async Task Wrap_PropertyAccess_ShouldDelegateToWrappedInstance()
	{
		// Arrange
		var realService = new MyServiceImplementation { SomeFlag = true };
		IMyService wrapped = Mock.Wrap<IMyService>(realService);
		
		// Act
		bool result = wrapped.SomeFlag;
		
		// Assert
		await That(result).IsTrue();
	}

	// Test implementation
	private class MyServiceImplementation : IMyService
	{
		public string this[int index] { get => $"Index {index}"; set { } }
		public string this[int index1, int index2] { get => $"Index {index1},{index2}"; set { } }
		public string this[int index1, int index2, int index3] { get => $"Index {index1},{index2},{index3}"; set { } }
		public string? this[int? index1, int? index2, int? index3, int? index4] { get => $"Index {index1},{index2},{index3},{index4}"; set { } }
		
		public bool SomeFlag { get; set; }
		public int SomeReadOnlyValue => 42;
		
		public void DoSomething(int value) { }
		public void DoSomething(int value, bool flag) { }
		public bool IsValid(int id) => id > 0;
		public string DoSomethingAndReturn(int value) => $"Executed with value: {value}";
		public void MyMethodWithOutParam(out int value) { value = 10; }
		public void MyMethodWithRefParam(ref int value) { value *= 2; }
		
		public event EventHandler? MyEvent;
		
		public TService GetInstance<TService>() => default!;
		public TService GetInstance<TService>(string key) => default!;
		public IEnumerable<TService> GetAllInstances<TService>() => Enumerable.Empty<TService>();
	}
}
