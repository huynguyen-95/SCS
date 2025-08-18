using ErrorOr;
using Microsoft.EntityFrameworkCore;
using SCS.Api.App.Abstraction.Messaging;
using SCS.Api.App.Features.User;

namespace SCS.Api.UnitTests.Features.User;

public class AddNewUserTests : BaseTest
{
    [Fact]
    public async Task Handle_WhenUserDoesNotExist_ShouldCreateUserSuccessfully()
    {
        // Arrange
        var handler = new AddNewUser.Handler(DbContext);
        var command = new AddNewUser.Command("EMP001", "John Doe", false);
        var cancellationToken = CancellationToken.None;

        // Act
        var result = await handler.Handle(command, cancellationToken);

        // Assert
        Assert.False(result.IsError);
        Assert.Equal(Unit.Value, result.Value);

        // Verify user was created in database
        var createdUser = await DbContext.Users.FirstOrDefaultAsync(u => u.EmpNo == "EMP001");
        Assert.NotNull(createdUser);
        Assert.Equal("EMP001", createdUser.EmpNo);
        Assert.Equal("John Doe", createdUser.Username);
        Assert.False(createdUser.IsAdmin);
    }

    [Fact]
    public async Task Handle_WhenUserAlreadyExists_ShouldReturnValidationError()
    {
        // Arrange
        var existingUser = new Domain.User("EMP001", "Existing User", false);
        DbContext.Users.Add(existingUser);
        await DbContext.SaveChangesAsync();

        var handler = new AddNewUser.Handler(DbContext);
        var command = new AddNewUser.Command("EMP001", "John Doe", false);
        var cancellationToken = CancellationToken.None;

        // Act
        var result = await handler.Handle(command, cancellationToken);

        // Assert
        Assert.True(result.IsError);
        Assert.Single(result.Errors);

        var error = result.FirstError;
        Assert.Equal(ErrorType.Validation, error.Type);
        Assert.Equal("AddNewUser.UserAlreadyExists", error.Code);
        Assert.Equal("A user with this employee number already exists.", error.Description);

        // Verify no new user was created
        var userCount = await DbContext.Users.CountAsync(u => u.EmpNo == "EMP001");
        Assert.Equal(1, userCount); // Only the original user exists
    }

    [Fact]
    public async Task Handle_WhenCreatingAdminUser_ShouldCreateAdminUserSuccessfully()
    {
        // Arrange
        var handler = new AddNewUser.Handler(DbContext);
        var command = new AddNewUser.Command("ADMIN001", "Admin User", true);
        var cancellationToken = CancellationToken.None;

        // Act
        var result = await handler.Handle(command, cancellationToken);

        // Assert
        Assert.False(result.IsError);
        Assert.Equal(Unit.Value, result.Value);

        // Verify admin user was created
        var createdUser = await DbContext.Users.FirstOrDefaultAsync(u => u.EmpNo == "ADMIN001");
        Assert.NotNull(createdUser);
        Assert.Equal("ADMIN001", createdUser.EmpNo);
        Assert.Equal("Admin User", createdUser.Username);
        Assert.True(createdUser.IsAdmin);
    }

    [Fact]
    public async Task Handle_WhenCreatingRegularUser_ShouldCreateRegularUserSuccessfully()
    {
        // Arrange
        var handler = new AddNewUser.Handler(DbContext);
        var command = new AddNewUser.Command("USER001", "Regular User", false);
        var cancellationToken = CancellationToken.None;

        // Act
        var result = await handler.Handle(command, cancellationToken);

        // Assert
        Assert.False(result.IsError);
        Assert.Equal(Unit.Value, result.Value);

        // Verify regular user was created
        var createdUser = await DbContext.Users.FirstOrDefaultAsync(u => u.EmpNo == "USER001");
        Assert.NotNull(createdUser);
        Assert.Equal("USER001", createdUser.EmpNo);
        Assert.Equal("Regular User", createdUser.Username);
        Assert.False(createdUser.IsAdmin);
    }

    [Fact]
    public async Task Handle_WhenCancellationRequested_ShouldHandleCancellation()
    {
        // Arrange
        var handler = new AddNewUser.Handler(DbContext);
        var command = new AddNewUser.Command("EMP001", "John Doe", false);
        var cancellationTokenSource = new CancellationTokenSource();
        cancellationTokenSource.Cancel();

        // Act & Assert
        await Assert.ThrowsAsync<OperationCanceledException>(async () =>
            await handler.Handle(command, cancellationTokenSource.Token));
    }

    [Fact]
    public async Task Handle_WhenUserWithSpecialCharacters_ShouldCreateSuccessfully()
    {
        // Arrange
        var handler = new AddNewUser.Handler(DbContext);
        var command = new AddNewUser.Command("EMP001", "José García-O'Connor", false);
        var cancellationToken = CancellationToken.None;

        // Act
        var result = await handler.Handle(command, cancellationToken);

        // Assert
        Assert.False(result.IsError);
        Assert.Equal(Unit.Value, result.Value);

        // Verify user was created with special characters
        var createdUser = await DbContext.Users.FirstOrDefaultAsync(u => u.EmpNo == "EMP001");
        Assert.NotNull(createdUser);
        Assert.Equal("José García-O'Connor", createdUser.Username);
    }

    [Fact]
    public async Task Handle_WhenMultipleUsersWithDifferentEmpNo_ShouldCreateAllSuccessfully()
    {
        // Arrange
        var handler = new AddNewUser.Handler(DbContext);
        var commands = new[]
        {
            new AddNewUser.Command("EMP001", "User One", false),
            new AddNewUser.Command("EMP002", "User Two", true),
            new AddNewUser.Command("EMP003", "User Three", false)
        };
        var cancellationToken = CancellationToken.None;

        // Act
        var results = new List<ErrorOr<Unit>>();
        foreach (var command in commands)
        {
            var result = await handler.Handle(command, cancellationToken);
            results.Add(result);
        }

        // Assert
        Assert.All(results, result => Assert.False(result.IsError));

        // Verify all users were created
        var userCount = await DbContext.Users.CountAsync();
        Assert.Equal(3, userCount);

        var users = await DbContext.Users.ToListAsync();
        Assert.Contains(users, u => u.EmpNo == "EMP001" && u.Username == "User One" && !u.IsAdmin);
        Assert.Contains(users, u => u.EmpNo == "EMP002" && u.Username == "User Two" && u.IsAdmin);
        Assert.Contains(users, u => u.EmpNo == "EMP003" && u.Username == "User Three" && !u.IsAdmin);
    }

    [Fact]
    public async Task Handle_WhenCheckingExistingUserWithSameEmpNo_ShouldReturnValidationError()
    {
        // Arrange
        var existingUser = new Domain.User("EMP001", "Original User", true);
        DbContext.Users.Add(existingUser);
        await DbContext.SaveChangesAsync();

        var handler = new AddNewUser.Handler(DbContext);
        var command = new AddNewUser.Command("EMP001", "Different Name", false); // Same EmpNo, different details
        var cancellationToken = CancellationToken.None;

        // Act
        var result = await handler.Handle(command, cancellationToken);

        // Assert
        Assert.True(result.IsError);

        // Verify original user remains unchanged
        var unchangedUser = await DbContext.Users.FirstOrDefaultAsync(u => u.EmpNo == "EMP001");
        Assert.NotNull(unchangedUser);
        Assert.Equal("Original User", unchangedUser.Username);
        Assert.True(unchangedUser.IsAdmin);
    }

    [Fact]
    public async Task Handle_WhenEmptyDatabase_ShouldCreateFirstUserSuccessfully()
    {
        // Arrange
        var handler = new AddNewUser.Handler(DbContext);
        var command = new AddNewUser.Command("EMP001", "First User", true);
        var cancellationToken = CancellationToken.None;

        // Verify database is empty
        var initialCount = await DbContext.Users.CountAsync();
        Assert.Equal(0, initialCount);

        // Act
        var result = await handler.Handle(command, cancellationToken);

        // Assert
        Assert.False(result.IsError);

        // Verify first user was created
        var finalCount = await DbContext.Users.CountAsync();
        Assert.Equal(1, finalCount);

        var createdUser = await DbContext.Users.FirstAsync();
        Assert.Equal("EMP001", createdUser.EmpNo);
        Assert.Equal("First User", createdUser.Username);
        Assert.True(createdUser.IsAdmin);
    }
}
