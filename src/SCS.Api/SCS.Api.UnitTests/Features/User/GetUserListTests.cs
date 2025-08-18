using SCS.Api.App.Features.User;
using SCS.Api.Domain;

namespace SCS.Api.UnitTests.Features.User;

public class GetUserListTests : BaseTest
{
    [Fact]
    public async Task Handle_WhenDatabaseIsEmpty_ShouldReturnEmptyList()
    {
        // Arrange
        var handler = new GetUserList.Handler(DbContext);
        var query = new GetUserList.Query();
        var cancellationToken = CancellationToken.None;

        // Act
        var result = await handler.Handle(query, cancellationToken);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task Handle_WhenSingleUserExists_ShouldReturnSingleUser()
    {
        // Arrange
        var user = new Domain.User("EMP001", "John Doe", false);
        DbContext.Users.Add(user);
        await DbContext.SaveChangesAsync();

        var handler = new GetUserList.Handler(DbContext);
        var query = new GetUserList.Query();
        var cancellationToken = CancellationToken.None;

        // Act
        var result = await handler.Handle(query, cancellationToken);

        // Assert
        Assert.NotNull(result);
        var userList = result.ToList();
        Assert.Single(userList);

        var userDto = userList.First();
        Assert.Equal("EMP001", userDto.EmpNo);
        Assert.Equal("John Doe", userDto.Name);
        Assert.False(userDto.IsAdmin);
    }

    [Fact]
    public async Task Handle_WhenMultipleUsersExist_ShouldReturnAllUsers()
    {
        // Arrange
        var users = new[]
        {
            new Domain.User("EMP001", "John Doe", false),
            new Domain.User("EMP002", "Jane Smith", true),
            new Domain.User("EMP003", "Bob Johnson", false)
        };

        DbContext.Users.AddRange(users);
        await DbContext.SaveChangesAsync();

        var handler = new GetUserList.Handler(DbContext);
        var query = new GetUserList.Query();
        var cancellationToken = CancellationToken.None;

        // Act
        var result = await handler.Handle(query, cancellationToken);

        // Assert
        Assert.NotNull(result);
        var userList = result.ToList();
        Assert.Equal(3, userList.Count);

        // Verify all users are returned with correct mapping
        var user1 = userList.First(u => u.EmpNo == "EMP001");
        Assert.Equal("John Doe", user1.Name);
        Assert.False(user1.IsAdmin);

        var user2 = userList.First(u => u.EmpNo == "EMP002");
        Assert.Equal("Jane Smith", user2.Name);
        Assert.True(user2.IsAdmin);

        var user3 = userList.First(u => u.EmpNo == "EMP003");
        Assert.Equal("Bob Johnson", user3.Name);
        Assert.False(user3.IsAdmin);
    }

    [Fact]
    public async Task Handle_WhenUsersHaveDifferentAdminStatus_ShouldReturnCorrectAdminFlags()
    {
        // Arrange
        var users = new[]
        {
            new Domain.User("ADMIN001", "Admin User", true),
            new Domain.User("USER001", "Regular User", false)
        };

        DbContext.Users.AddRange(users);
        await DbContext.SaveChangesAsync();

        var handler = new GetUserList.Handler(DbContext);
        var query = new GetUserList.Query();
        var cancellationToken = CancellationToken.None;

        // Act
        var result = await handler.Handle(query, cancellationToken);

        // Assert
        Assert.NotNull(result);
        var userList = result.ToList();
        Assert.Equal(2, userList.Count);

        var adminUser = userList.First(u => u.EmpNo == "ADMIN001");
        Assert.True(adminUser.IsAdmin);

        var regularUser = userList.First(u => u.EmpNo == "USER001");
        Assert.False(regularUser.IsAdmin);
    }

    [Fact]
    public async Task Handle_WhenCancellationRequested_ShouldHandleCancellation()
    {
        // Arrange
        var users = new[]
        {
            new Domain.User("EMP001", "John Doe", false),
            new Domain.User("EMP002", "Jane Smith", true)
        };

        DbContext.Users.AddRange(users);
        await DbContext.SaveChangesAsync();

        var handler = new GetUserList.Handler(DbContext);
        var query = new GetUserList.Query();
        var cancellationTokenSource = new CancellationTokenSource();
        cancellationTokenSource.Cancel();

        // Act & Assert
        await Assert.ThrowsAsync<OperationCanceledException>(async () =>
            await handler.Handle(query, cancellationTokenSource.Token));
    }

    [Fact]
    public async Task Handle_WhenUsersWithSpecialCharacters_ShouldHandleCorrectly()
    {
        // Arrange
        var users = new[]
        {
            new Domain.User("EMP001", "José García", false),
            new Domain.User("EMP002", "李小明", true),
            new Domain.User("EMP003", "O'Connor", false)
        };

        DbContext.Users.AddRange(users);
        await DbContext.SaveChangesAsync();

        var handler = new GetUserList.Handler(DbContext);
        var query = new GetUserList.Query();
        var cancellationToken = CancellationToken.None;

        // Act
        var result = await handler.Handle(query, cancellationToken);

        // Assert
        Assert.NotNull(result);
        var userList = result.ToList();
        Assert.Equal(3, userList.Count);

        Assert.Contains(userList, u => u.Name == "José García");
        Assert.Contains(userList, u => u.Name == "李小明");
        Assert.Contains(userList, u => u.Name == "O'Connor");
    }

    [Fact]
    public async Task Handle_WhenLargeNumberOfUsers_ShouldReturnAllUsers()
    {
        // Arrange
        var users = new List<Domain.User>();
        for (int i = 1; i <= 100; i++)
        {
            users.Add(new Domain.User($"EMP{i:D3}", $"User {i}", i % 10 == 0)); // Every 10th user is admin
        }

        DbContext.Users.AddRange(users);
        await DbContext.SaveChangesAsync();

        var handler = new GetUserList.Handler(DbContext);
        var query = new GetUserList.Query();
        var cancellationToken = CancellationToken.None;

        // Act
        var result = await handler.Handle(query, cancellationToken);

        // Assert
        Assert.NotNull(result);
        var userList = result.ToList();
        Assert.Equal(100, userList.Count);

        // Verify admin count (every 10th user should be admin)
        var adminCount = userList.Count(u => u.IsAdmin);
        Assert.Equal(10, adminCount);
    }

    [Fact]
    public void Query_ShouldImplementIRequest()
    {
        // Arrange & Act
        var query = new GetUserList.Query();

        // Assert
        Assert.IsAssignableFrom<SCS.Api.App.Abstraction.Messaging.IRequest<IEnumerable<GetUserList.UserDto>>>(query);
    }

    [Fact]
    public void UserDto_ShouldHaveCorrectProperties()
    {
        // Arrange & Act
        var userDto = new GetUserList.UserDto("EMP001", "Test User", true);

        // Assert
        Assert.Equal("EMP001", userDto.EmpNo);
        Assert.Equal("Test User", userDto.Name);
        Assert.True(userDto.IsAdmin);
    }

    [Fact]
    public void Handler_ShouldImplementIRequestHandler()
    {
        // Arrange & Act
        var handler = new GetUserList.Handler(DbContext);

        // Assert
        Assert.IsAssignableFrom<SCS.Api.App.Abstraction.Messaging.IRequestHandler<GetUserList.Query, IEnumerable<GetUserList.UserDto>>>(handler);
    }
}
