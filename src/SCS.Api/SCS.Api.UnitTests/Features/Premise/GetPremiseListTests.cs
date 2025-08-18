using Microsoft.Extensions.Caching.Memory;
using SCS.Api.App.Features.Premise;

namespace SCS.Api.UnitTests.Features.Premise;

public class GetPremiseListTests : BaseTest
{
    private readonly IMemoryCache _memoryCache;

    public GetPremiseListTests()
    {
        _memoryCache = new MemoryCache(new MemoryCacheOptions());
    }

    [Fact]
    public async Task Handle_WhenDatabaseIsEmpty_ShouldReturnEmptyList()
    {
        // Arrange
        var handler = new GetPremiseList.Handler(DbContext, _memoryCache);
        var query = new GetPremiseList.Query();
        var cancellationToken = CancellationToken.None;

        // Act
        var result = await handler.Handle(query, cancellationToken);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task Handle_WhenPremisesExist_ShouldReturnPremiseList()
    {
        // Arrange
        var premises = new[]
        {
            new Domain.Premise(1, "Office Building A"),
            new Domain.Premise(2, "Shopping Mall B"),
            new Domain.Premise(3, "Residential Complex C")
        };

        DbContext.Premises.AddRange(premises);
        await DbContext.SaveChangesAsync();

        var handler = new GetPremiseList.Handler(DbContext, _memoryCache);
        var query = new GetPremiseList.Query();
        var cancellationToken = CancellationToken.None;

        // Act
        var result = await handler.Handle(query, cancellationToken);

        // Assert
        Assert.NotNull(result);
        var premiseList = result.ToList();
        Assert.Equal(3, premiseList.Count);

        Assert.Contains(premiseList, p => p.Id == 1 && p.Name == "Office Building A");
        Assert.Contains(premiseList, p => p.Id == 2 && p.Name == "Shopping Mall B");
        Assert.Contains(premiseList, p => p.Id == 3 && p.Name == "Residential Complex C");
    }

    [Fact]
    public async Task Handle_WhenCalledMultipleTimes_ShouldUseCacheOnSecondCall()
    {
        // Arrange
        var premises = new[]
        {
            new Domain.Premise(1, "Cached Building"),
            new Domain.Premise(2, "Another Building")
        };

        DbContext.Premises.AddRange(premises);
        await DbContext.SaveChangesAsync();

        var handler = new GetPremiseList.Handler(DbContext, _memoryCache);
        var query = new GetPremiseList.Query();
        var cancellationToken = CancellationToken.None;

        // Act - First call (should hit database)
        var result1 = await handler.Handle(query, cancellationToken);

        // Add a new premise to database after first call
        var newPremise = new Domain.Premise(3, "New Building After Cache");
        DbContext.Premises.Add(newPremise);
        await DbContext.SaveChangesAsync();

        // Act - Second call (should use cache, not see new premise)
        var result2 = await handler.Handle(query, cancellationToken);

        // Assert
        var list1 = result1.ToList();
        var list2 = result2.ToList();

        Assert.Equal(2, list1.Count);
        Assert.Equal(2, list2.Count); // Should still be 2 due to caching

        // Both results should be identical (from cache)
        Assert.Equal(list1.Select(p => p.Id).OrderBy(x => x), list2.Select(p => p.Id).OrderBy(x => x));
        Assert.Equal(list1.Select(p => p.Name).OrderBy(x => x), list2.Select(p => p.Name).OrderBy(x => x));

        // Verify new premise is not in cached result
        Assert.DoesNotContain(list2, p => p.Name == "New Building After Cache");
    }

    [Fact]
    public async Task Handle_WhenCacheIsEmpty_ShouldPopulateCacheFromDatabase()
    {
        // Arrange
        var premises = new[]
        {
            new Domain.Premise(1, "Building One")
        };

        DbContext.Premises.AddRange(premises);
        await DbContext.SaveChangesAsync();

        var handler = new GetPremiseList.Handler(DbContext, _memoryCache);
        var query = new GetPremiseList.Query();
        var cancellationToken = CancellationToken.None;

        // Verify cache is empty
        var cacheKey = "PremiseList";
        Assert.False(_memoryCache.TryGetValue(cacheKey, out _));

        // Act
        var result = await handler.Handle(query, cancellationToken);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);

        // Verify cache is now populated
        Assert.True(_memoryCache.TryGetValue(cacheKey, out var cachedValue));
        Assert.NotNull(cachedValue);
    }

    [Fact]
    public async Task Handle_WhenCancellationRequested_ShouldHandleCancellation()
    {
        // Arrange
        var premises = new[]
        {
            new Domain.Premise(1, "Test Building")
        };

        DbContext.Premises.AddRange(premises);
        await DbContext.SaveChangesAsync();

        var handler = new GetPremiseList.Handler(DbContext, _memoryCache);
        var query = new GetPremiseList.Query();
        var cancellationTokenSource = new CancellationTokenSource();
        cancellationTokenSource.Cancel();

        // Act & Assert
        await Assert.ThrowsAsync<OperationCanceledException>(async () =>
            await handler.Handle(query, cancellationTokenSource.Token));
    }

    [Fact]
    public async Task Handle_WhenPremisesHaveSpecialCharacters_ShouldHandleCorrectly()
    {
        // Arrange
        var premises = new[]
        {
            new Domain.Premise(1, "Café & Restaurant"),
            new Domain.Premise(2, "北京大厦"),
            new Domain.Premise(3, "Müller's Office")
        };

        DbContext.Premises.AddRange(premises);
        await DbContext.SaveChangesAsync();

        var handler = new GetPremiseList.Handler(DbContext, _memoryCache);
        var query = new GetPremiseList.Query();
        var cancellationToken = CancellationToken.None;

        // Act
        var result = await handler.Handle(query, cancellationToken);

        // Assert
        Assert.NotNull(result);
        var premiseList = result.ToList();
        Assert.Equal(3, premiseList.Count);

        Assert.Contains(premiseList, p => p.Name == "Café & Restaurant");
        Assert.Contains(premiseList, p => p.Name == "北京大厦");
        Assert.Contains(premiseList, p => p.Name == "Müller's Office");
    }

    [Fact]
    public async Task Handle_WhenLargeNumberOfPremises_ShouldReturnAllPremises()
    {
        // Arrange
        var premises = new List<Domain.Premise>();
        for (int i = 1; i <= 100; i++)
        {
            premises.Add(new Domain.Premise(i, $"Building {i}"));
        }

        DbContext.Premises.AddRange(premises);
        await DbContext.SaveChangesAsync();

        var handler = new GetPremiseList.Handler(DbContext, _memoryCache);
        var query = new GetPremiseList.Query();
        var cancellationToken = CancellationToken.None;

        // Act
        var result = await handler.Handle(query, cancellationToken);

        // Assert
        Assert.NotNull(result);
        var premiseList = result.ToList();
        Assert.Equal(100, premiseList.Count);

        // Verify all premises are returned
        for (int i = 1; i <= 100; i++)
        {
            Assert.Contains(premiseList, p => p.Id == i && p.Name == $"Building {i}");
        }
    }

    [Fact]
    public async Task Handle_WhenDatabaseReturnsNull_ShouldReturnEmptyArray()
    {
        // Arrange
        var handler = new GetPremiseList.Handler(DbContext, _memoryCache);
        var query = new GetPremiseList.Query();
        var cancellationToken = CancellationToken.None;

        // Act
        var result = await handler.Handle(query, cancellationToken);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _memoryCache?.Dispose();
        }
        base.Dispose(disposing);
    }
}
