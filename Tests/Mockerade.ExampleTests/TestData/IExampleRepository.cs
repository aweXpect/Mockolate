namespace Mockerade.Tests.Dummy;

public interface IExampleRepository
{
	event EventHandler UsersChanged;
	User AddUser(string name);
	bool RemoveUser(Guid id);
	void UpdateUser(Guid id, string newName);

	bool TryDelete(Guid id, out User? user);

	bool SaveChanges();

	event MyDelegate MyEvent;
}
public interface IOrderRepository
{
	event EventHandler OrdersChanged;
	Order AddOrder(string name);
	bool RemoveOrder(Guid id);
	void UpdateOrder(Guid id, string newName);

	bool TryDelete(Guid id, out Order? user);

	bool SaveChanges();
}

public delegate bool MyDelegate(int x, int y);

public record User(Guid Id, string Name);

public record Order(Guid Id, string Name);
