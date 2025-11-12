namespace Mockolate;

public interface IMockSubject<T>
{
	Mock<T> Mock { get; }
}
