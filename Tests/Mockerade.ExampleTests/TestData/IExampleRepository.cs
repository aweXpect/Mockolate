namespace Mockerade.Tests.Dummy;

public interface IExampleRepository
{
	event EventHandler UsersChanged;
	User AddUser(string name);
	bool RemoveUser(Guid id);
	void UpdateUser(Guid id, string newName);

	bool TryDelete(Guid id, out User? user);
	
	event MyDelegate MyEvent;
}

public delegate bool MyDelegate(int x, int y);

public record User(Guid Id, string Name);
