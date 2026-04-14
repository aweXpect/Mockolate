namespace Mockolate.AotCompatibility.TestApp;

/// <summary>
///     A dummy interface to test the mock generation for interfaces in an AOT context.
/// </summary>
public interface IMyInterface
{
	/// <summary>
	///     Some method to test the mock generation for interfaces.
	/// </summary>
	int MyMethod();

	bool MyMethod1(int v1, int v2, int v3, ref int v4, out bool v5, int v6);
	void MyMethod2(int v1, int v2, int v3, ref int v4, out bool v5, int v6);
}
