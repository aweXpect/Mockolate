namespace Mockolate.Tests.TestHelpers;

internal interface IMyService
{
	string this[int index] { get; set; }
	string this[int index1, int index2] { get; set; }
	string this[int index1, int index2, int index3] { get; set; }
	string this[int? index1, int? index2, int? index3, int? index4] { get; set; }

	bool SomeFlag { get; set; }
	int SomeReadOnlyValue { get; }

	void DoSomething(int value);
	void DoSomething(int value, bool flag);

	string DoSomethingAndReturn(int value);

	event EventHandler? MyEvent;
}
