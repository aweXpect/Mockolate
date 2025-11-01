# Mock Creation

- Create mocks for interfaces and classes:
  ```csharp
  var mock = Mock.Create<IMyInterface>();
  var classMock = Mock.Create<MyVirtualClass>();
  ```
- Provide a `MockBehavior` to control the default behavior of the mock.
- Use a `Mock.Factory` to pass a common behavior to all created mocks.
