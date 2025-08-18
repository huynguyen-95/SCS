using ErrorOr;
using SCS.Api.App.Features.Premise;
using SCS.Api.Domain;

namespace SCS.Api.UnitTests.Features.Premise;

public class GetPremiseIncidentListTests : BaseTest
{
    private readonly GetPremiseIncidentList.Validator _validator;

    public GetPremiseIncidentListTests()
    {
        _validator = new GetPremiseIncidentList.Validator();
    }

    [Fact]
    public async Task Handle_WhenValidPremiseIdWithNoIncidents_ShouldReturnEmptyList()
    {
        // Arrange
        var handler = new GetPremiseIncidentList.Handler(DbContext, _validator);
        var query = new GetPremiseIncidentList.Query(1);
        var cancellationToken = CancellationToken.None;

        // Act
        var result = await handler.Handle(query, cancellationToken);

        // Assert
        Assert.False(result.IsError);
        Assert.NotNull(result.Value);
        Assert.Empty(result.Value);
    }

    [Fact]
    public async Task Handle_WhenValidPremiseIdWithIncidents_ShouldReturnIncidentsOrderedByDateDescending()
    {
        // Arrange
        var now = DateTimeOffset.UtcNow;
        var incidents = new[]
        {
            new Incident(1, "Security breach detected", now.AddHours(-2), "/files/incident1.jpg", "guard1"),
            new Incident(1, "Suspicious activity", now.AddHours(-5), "/files/incident2.jpg", "guard2"),
            new Incident(1, "Equipment malfunction", now.AddHours(-1), "/files/incident3.jpg", "guard1"),
            new Incident(2, "Different premise incident", now.AddHours(-3), "/files/incident4.jpg", "guard3") // Different premise
        };

        DbContext.Incidents.AddRange(incidents);
        await DbContext.SaveChangesAsync();

        var handler = new GetPremiseIncidentList.Handler(DbContext, _validator);
        var query = new GetPremiseIncidentList.Query(1);
        var cancellationToken = CancellationToken.None;

        // Act
        var result = await handler.Handle(query, cancellationToken);

        // Assert
        Assert.False(result.IsError);
        Assert.NotNull(result.Value);
        var incidentList = result.Value.ToList();
        Assert.Equal(3, incidentList.Count); // Only incidents for premise 1

        // Verify ordering (most recent first)
        Assert.Equal("Equipment malfunction", incidentList[0].Description);
        Assert.Equal("Security breach detected", incidentList[1].Description);
        Assert.Equal("Suspicious activity", incidentList[2].Description);

        // Verify all properties are mapped correctly
        var firstIncident = incidentList[0];
        Assert.Equal("Equipment malfunction", firstIncident.Description);
        Assert.Equal("/files/incident3.jpg", firstIncident.FilePath);
        Assert.True(Math.Abs((firstIncident.Date - now.AddHours(-1)).TotalMinutes) < 1);
    }

    [Fact]
    public async Task Handle_WhenPremiseIdIsZero_ShouldReturnValidationError()
    {
        // Arrange
        var handler = new GetPremiseIncidentList.Handler(DbContext, _validator);
        var query = new GetPremiseIncidentList.Query(0);
        var cancellationToken = CancellationToken.None;

        // Act
        var result = await handler.Handle(query, cancellationToken);

        // Assert
        Assert.True(result.IsError);
        Assert.Single(result.Errors);

        var error = result.FirstError;
        Assert.Equal(ErrorType.Validation, error.Type);
        Assert.Equal("GetPremiseIncidentList.Validation", error.Code);
        Assert.Equal("validation failed", error.Description);
    }

    [Fact]
    public async Task Handle_WhenPremiseIdIsNegative_ShouldReturnValidationError()
    {
        // Arrange
        var handler = new GetPremiseIncidentList.Handler(DbContext, _validator);
        var query = new GetPremiseIncidentList.Query(-5);
        var cancellationToken = CancellationToken.None;

        // Act
        var result = await handler.Handle(query, cancellationToken);

        // Assert
        Assert.True(result.IsError);
        Assert.Single(result.Errors);

        var error = result.FirstError;
        Assert.Equal(ErrorType.Validation, error.Type);
        Assert.Equal("GetPremiseIncidentList.Validation", error.Code);
        Assert.Equal("validation failed", error.Description);
    }

    [Fact]
    public async Task Handle_WhenMultiplePremisesHaveIncidents_ShouldOnlyReturnIncidentsForSpecifiedPremise()
    {
        // Arrange
        var now = DateTimeOffset.UtcNow;
        var incidents = new[]
        {
            new Incident(1, "Premise 1 incident 1", now.AddHours(-1), "/files/p1_inc1.jpg", "guard1"),
            new Incident(1, "Premise 1 incident 2", now.AddHours(-2), "/files/p1_inc2.jpg", "guard2"),
            new Incident(2, "Premise 2 incident 1", now.AddHours(-1), "/files/p2_inc1.jpg", "guard3"),
            new Incident(3, "Premise 3 incident 1", now.AddHours(-1), "/files/p3_inc1.jpg", "guard4")
        };

        DbContext.Incidents.AddRange(incidents);
        await DbContext.SaveChangesAsync();

        var handler = new GetPremiseIncidentList.Handler(DbContext, _validator);
        var query = new GetPremiseIncidentList.Query(2);
        var cancellationToken = CancellationToken.None;

        // Act
        var result = await handler.Handle(query, cancellationToken);

        // Assert
        Assert.False(result.IsError);
        var incidentList = result.Value.ToList();
        Assert.Single(incidentList);
        Assert.Equal("Premise 2 incident 1", incidentList[0].Description);
        Assert.Equal("/files/p2_inc1.jpg", incidentList[0].FilePath);
    }

    [Fact]
    public async Task Handle_WhenCancellationRequested_ShouldHandleCancellation()
    {
        // Arrange
        var incidents = new[]
        {
            new Incident(1, "Test incident", DateTimeOffset.UtcNow, "/files/test.jpg", "guard1")
        };

        DbContext.Incidents.AddRange(incidents);
        await DbContext.SaveChangesAsync();

        var handler = new GetPremiseIncidentList.Handler(DbContext, _validator);
        var query = new GetPremiseIncidentList.Query(1);
        var cancellationTokenSource = new CancellationTokenSource();
        cancellationTokenSource.Cancel();

        // Act & Assert
        await Assert.ThrowsAsync<OperationCanceledException>(async () =>
            await handler.Handle(query, cancellationTokenSource.Token));
    }

    [Fact]
    public async Task Handle_WhenIncidentsHaveSpecialCharacters_ShouldHandleCorrectly()
    {
        // Arrange
        var now = DateTimeOffset.UtcNow;
        var incidents = new[]
        {
            new Incident(1, "异常活动检测 - 中文描述", now.AddHours(-1), "/files/中文文件.jpg", "guard1"),
            new Incident(1, "Activité suspecte détectée", now.AddHours(-2), "/files/français.jpg", "guard2"),
            new Incident(1, "Verdächtige Aktivität - Müller's Bericht", now.AddHours(-3), "/files/deutsch.jpg", "guard3")
        };

        DbContext.Incidents.AddRange(incidents);
        await DbContext.SaveChangesAsync();

        var handler = new GetPremiseIncidentList.Handler(DbContext, _validator);
        var query = new GetPremiseIncidentList.Query(1);
        var cancellationToken = CancellationToken.None;

        // Act
        var result = await handler.Handle(query, cancellationToken);

        // Assert
        Assert.False(result.IsError);
        var incidentList = result.Value.ToList();
        Assert.Equal(3, incidentList.Count);

        Assert.Contains(incidentList, i => i.Description == "异常活动检测 - 中文描述");
        Assert.Contains(incidentList, i => i.Description == "Activité suspecte détectée");
        Assert.Contains(incidentList, i => i.Description == "Verdächtige Aktivität - Müller's Bericht");
    }

    [Fact]
    public async Task Handle_WhenLargeNumberOfIncidents_ShouldReturnAllIncidentsForPremise()
    {
        // Arrange
        var now = DateTimeOffset.UtcNow;
        var incidents = new List<Incident>();

        // Create 50 incidents for premise 1
        for (int i = 1; i <= 50; i++)
        {
            incidents.Add(new Incident(1, $"Incident {i}", now.AddHours(-i), $"/files/incident{i}.jpg", $"guard{i % 5 + 1}"));
        }

        // Create 25 incidents for premise 2 (should not be returned)
        for (int i = 1; i <= 25; i++)
        {
            incidents.Add(new Incident(2, $"Other premise incident {i}", now.AddHours(-i), $"/files/other{i}.jpg", "guard1"));
        }

        DbContext.Incidents.AddRange(incidents);
        await DbContext.SaveChangesAsync();

        var handler = new GetPremiseIncidentList.Handler(DbContext, _validator);
        var query = new GetPremiseIncidentList.Query(1);
        var cancellationToken = CancellationToken.None;

        // Act
        var result = await handler.Handle(query, cancellationToken);

        // Assert
        Assert.False(result.IsError);
        var incidentList = result.Value.ToList();
        Assert.Equal(50, incidentList.Count); // Only incidents for premise 1

        // Verify ordering (most recent first)
        Assert.Equal("Incident 1", incidentList[0].Description);
        Assert.Equal("Incident 50", incidentList[49].Description);
    }

    [Fact]
    public async Task Validator_WhenPremiseIdIsValid_ShouldPassValidation()
    {
        // Arrange
        var query = new GetPremiseIncidentList.Query(1);

        // Act
        var validationResult = await _validator.ValidateAsync(query);

        // Assert
        Assert.True(validationResult.IsValid);
        Assert.Empty(validationResult.Errors);
    }

    [Fact]
    public async Task Validator_WhenPremiseIdIsZero_ShouldFailValidation()
    {
        // Arrange
        var query = new GetPremiseIncidentList.Query(0);

        // Act
        var validationResult = await _validator.ValidateAsync(query);

        // Assert
        Assert.False(validationResult.IsValid);
        Assert.Single(validationResult.Errors);
        Assert.Equal("Premise ID must be greater than 0.", validationResult.Errors[0].ErrorMessage);
    }

    [Fact]
    public async Task Validator_WhenPremiseIdIsNegative_ShouldFailValidation()
    {
        // Arrange
        var query = new GetPremiseIncidentList.Query(-10);

        // Act
        var validationResult = await _validator.ValidateAsync(query);

        // Assert
        Assert.False(validationResult.IsValid);
        Assert.Single(validationResult.Errors);
        Assert.Equal("Premise ID must be greater than 0.", validationResult.Errors[0].ErrorMessage);
    }

    [Fact]
    public void Query_ShouldImplementIRequest()
    {
        // Arrange & Act
        var query = new GetPremiseIncidentList.Query(1);

        // Assert
        Assert.IsAssignableFrom<SCS.Api.App.Abstraction.Messaging.IRequest<ErrorOr<IEnumerable<GetPremiseIncidentList.PremiseIncidentDto>>>>(query);
        Assert.Equal(1, query.PremiseId);
    }

    [Fact]
    public void PremiseIncidentDto_ShouldHaveCorrectProperties()
    {
        // Arrange & Act
        var date = DateTimeOffset.UtcNow;
        var dto = new GetPremiseIncidentList.PremiseIncidentDto("Test Description", date, "/files/test.jpg");

        // Assert
        Assert.Equal("Test Description", dto.Description);
        Assert.Equal(date, dto.Date);
        Assert.Equal("/files/test.jpg", dto.FilePath);
    }
}
