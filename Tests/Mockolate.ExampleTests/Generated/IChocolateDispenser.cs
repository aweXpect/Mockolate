#if NET10_0_OR_GREATER
namespace Mockolate.ExampleTests;

public interface IChocolateDispenser
{
	static abstract bool IsActive { get; }
	void DispenseChocolate(string flavor);
}
public interface IChocolateDispenser2
{
	void DispenseChocolate(string flavor);
}
public interface IChocolateDispenser3
{
	void DispenseChocolate(string flavor);
}
#endif
