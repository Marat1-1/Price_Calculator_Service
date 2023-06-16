using System;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Route256.Week5.Homework.PriceCalculator.Dal.Models;
using Route256.Week5.Homework.PriceCalculator.Dal.Repositories.Interfaces;
using Route256.Week5.Homework.PriceCalculator.IntegrationTests.Fixtures;
using Route256.Week5.Homework.TestingInfrastructure.Creators;
using Route256.Week5.Homework.TestingInfrastructure.Fakers;
using Xunit;

namespace Route256.Week5.Homework.PriceCalculator.IntegrationTests.RepositoryTests;

[Collection(nameof(TestFixture))]
public class CalculationsRepositoryTests
{
    private readonly double _requiredDoublePrecision = 0.00001d;
    private readonly decimal _requiredDecimalPrecision = 0.00001m;
    private readonly TimeSpan _requiredDateTimePrecision = TimeSpan.FromMilliseconds(100);
    
    private readonly ICalculationRepository _calculationRepository;

    public CalculationsRepositoryTests(TestFixture fixture)
    {
        _calculationRepository = fixture.CalculationRepository;
    }

    [Theory]
    [InlineData(1)]
    [InlineData(5)]
    public async Task Add_Calculations_Success(int count)
    {
        // Arrange
        var userId = Create.RandomId();
        var now = DateTimeOffset.UtcNow;

        var calculations = CalculationEntityV1Faker.Generate(count)
            .Select(x => x.WithUserId(userId)
                .WithAt(now))
            .ToArray();
        
        // Act
        var calculationIds = await _calculationRepository.Add(calculations, default);

        // Asserts
        calculationIds.Should().HaveCount(count);
        calculationIds.Should().OnlyContain(x => x > 0);
    }
    
    [Fact]
    public async Task Query_SingleCalculation_Success()
    {
        // Arrange
        var userId = Create.RandomId();
        var now = DateTimeOffset.UtcNow;

        var calculations = CalculationEntityV1Faker.Generate()
            .Select(x => x.WithUserId(userId)
                .WithAt(now))
            .ToArray();
        var expected = calculations.Single();
        
        var calculationId = (await _calculationRepository.Add(calculations, default))
            .Single();

        // Act
        var foundCalculations = await _calculationRepository.Query(
            new CalculationHistoryQueryModel(userId, 1, 0), 
            default);

        // Asserts
        foundCalculations.Should().HaveCount(1);
        var calculation = foundCalculations.Single();

        calculation.Id.Should().Be(calculationId);
        calculation.UserId.Should().Be(expected.UserId);
        calculation.At.Should().BeCloseTo(expected.At, _requiredDateTimePrecision);
        calculation.Price.Should().BeApproximately(expected.Price, _requiredDecimalPrecision);
        calculation.GoodIds.Should().BeEquivalentTo(expected.GoodIds);
        calculation.TotalVolume.Should().BeApproximately(expected.TotalVolume, _requiredDoublePrecision);
        calculation.TotalWeight.Should().BeApproximately(expected.TotalWeight, _requiredDoublePrecision);
    }
    
    [Theory]
    [InlineData(3,  2, 3)]
    [InlineData(1,  6, 1)]
    [InlineData(2,  8, 2)]
    [InlineData(3, 10, 0)]
    public async Task Query_CalculationsInRange_Success(int take, int skip, int expectedCount)
    {
        // Arrange
        var userId = Create.RandomId();
        var now = DateTimeOffset.UtcNow;

        var calculations = CalculationEntityV1Faker.Generate(10)
            .Select(x => x.WithUserId(userId)
                .WithAt(now))
            .ToArray();

        await _calculationRepository.Add(calculations, default);

        var allCalculations = await _calculationRepository.Query(
            new CalculationHistoryQueryModel(userId, 100, 0), 
            default);
        
        var expected = allCalculations
            .OrderByDescending(x => x.At)
            .Skip(skip)
            .Take(take);
        
        // Act
        var foundCalculations = await _calculationRepository.Query(
            new CalculationHistoryQueryModel(userId, take, skip), 
            default);

        // Asserts
        foundCalculations.Should().HaveCount(expectedCount);

        if (expectedCount > 0)
        {
            foundCalculations.Should().BeEquivalentTo(expected);
        }
    }
    
    [Fact]
    public async Task Query_AllCalculations_Success()
    {
        // Arrange
        var userId = Create.RandomId();
        var now = DateTimeOffset.UtcNow;

        var calculations = CalculationEntityV1Faker.Generate(5)
            .Select(x => x.WithUserId(userId)
                .WithAt(now))
            .ToArray();

        var calculationIds = (await _calculationRepository.Add(calculations, default))
            .ToHashSet();

        // Act
        var foundCalculations = await _calculationRepository.Query(
            new CalculationHistoryQueryModel(userId, 100, 0), 
            default);

        // Assert
        foundCalculations.Should().NotBeEmpty();
        foundCalculations.Should().OnlyContain(x => x.UserId == userId);
        foundCalculations.Should().OnlyContain(x => calculationIds.Contains(x.Id));
        foundCalculations.Should().BeInDescendingOrder(x => x.At);
    }
    
    [Fact]
    public async Task Query_Calculations_ReturnsEmpty_WhenForWrongUser()
    {
        // Arrange
        var userId = Create.RandomId();
        var anotherUserId = Create.RandomId();
        var now = DateTimeOffset.UtcNow;

        var calculations = CalculationEntityV1Faker.Generate(5)
            .Select(x => x.WithUserId(userId)
                .WithAt(now))
            .ToArray();

        var calculationIds = (await _calculationRepository.Add(calculations, default))
            .ToHashSet();

        // Act
        var foundCalculations = await _calculationRepository.Query(
            new CalculationHistoryQueryModel(anotherUserId, 100, 0), 
            default);

        // Assert
        foundCalculations.Should().BeEmpty();
    }

    [Fact]
    public async Task GetCalculation_WhenOneCalculationAreContainedInDB_ShouldBeSuccess()
    {
        // Arrange
        var userId = Create.RandomId();
        var now = DateTimeOffset.UtcNow;
        var calculations = CalculationEntityV1Faker.Generate()
            .Select(x => x.WithUserId(userId)
                .WithAt(now))
            .ToArray();
        var expected = calculations.Single();
        var calcId = (await _calculationRepository.Add(calculations, default)).Single();
        
        // Act
        var foundCalculations = await _calculationRepository.GetCalculations(
            new [] {calcId}, default);
        var foundCalculation = foundCalculations.Single();

        // Assert
        foundCalculations.Should().HaveCount(1);
        foundCalculation.Id.Should().Be(calcId);
        foundCalculation.UserId.Should().Be(expected.UserId);
        foundCalculation.GoodIds.Should().BeEquivalentTo(expected.GoodIds);
        foundCalculation.Price.Should().BeApproximately(expected.Price, _requiredDecimalPrecision);
        foundCalculation.TotalVolume.Should().BeApproximately(expected.TotalVolume, _requiredDoublePrecision);
        foundCalculation.TotalWeight.Should().BeApproximately(expected.TotalWeight, _requiredDoublePrecision);
        foundCalculation.At.Should().BeCloseTo(expected.At, _requiredDateTimePrecision);
    }

    [Fact]
    public async Task GetCalculation_WhenManyCalculationsAreContainedInDB_ShouldBeSuccess()
    {
        // Arrange
        var userId = Create.RandomId();
        var now = DateTimeOffset.UtcNow;
        var calculations = CalculationEntityV1Faker.Generate(5)
            .Select(x => x.WithUserId(userId)
                .WithAt(now))
            .ToArray();
        var calculationIds = await _calculationRepository.Add(calculations, default);

        // Act
        var foundCalculations = await _calculationRepository.GetCalculations(
            calculationIds, default);

        // Assert
        foundCalculations.Should().HaveCount(5);
        foundCalculations.Should().OnlyContain(x => x.UserId == userId);
        foundCalculations.Should().OnlyContain(x => calculationIds.Contains(x.Id));
    }

    [Fact]
    public async Task GetCalculation_WhenCalculationsAreNotContainedInDB_ResultShouldBeEmpty()
    {
        // Arrange
        var calculationIds = Enumerable.Range(0, 5).Select(x => Create.RandomId());
        
        // Act
        var foundCalculations = await _calculationRepository.GetCalculations(
            calculationIds.ToArray(), default);

        // Assert
        foundCalculations.Should().BeEmpty();
    }

    [Fact]
    public async Task GetCalculation_WhenSomeCalculationIdsAreNotContainedInDB_ShouldBeSuccess()
    {
        // Arrange
        var userId = Create.RandomId();
        var now = DateTimeOffset.UtcNow;
        var calculations = CalculationEntityV1Faker.Generate(5)
            .Select(x => x.WithUserId(userId)
                .WithAt(now))
            .ToArray();
        var expectedIds = await _calculationRepository.Add(calculations, default);
        var calculationIds = expectedIds.ToList();
        calculationIds.AddRange(Enumerable.Range(0, 3).Select(x => Create.RandomId()));
        
        // Act
        var foundCalculations = await _calculationRepository.GetCalculations(
            calculationIds.ToArray(), default);
        
        // Assert
        foundCalculations.Should().HaveCount(5);
        foundCalculations.Should().OnlyContain(x => expectedIds.Contains(x.Id));
    }

    [Fact]
    public async Task Delete_WhenOneCalculationsAreContainedInDB_ResultGetCalculationHistoryShouldBeEmpty()
    {
        // Arrange
        var userId = Create.RandomId();
        var now = DateTimeOffset.UtcNow;
        var calculations = CalculationEntityV1Faker.Generate()
            .Select(x => x.WithUserId(userId)
                .WithAt(now))
            .ToArray();
        var calculationId = (await _calculationRepository.Add(calculations, default)).Single();
        
        // Act
        await _calculationRepository.Delete(new[] { calculationId }, default);
        var foundCalculation = await _calculationRepository.GetCalculations(
            new[] { calculationId }, default);
        
        // Assert
        foundCalculation.Should().BeEmpty();
    }

    [Fact]
    public async Task Delete_WhenManyCalculationsAreContainedInDB_ResultGetCalculationHistoryShouldBeEmpty()
    {
        // Arrange
        var userId = Create.RandomId();
        var now = DateTimeOffset.UtcNow;
        var calculations = CalculationEntityV1Faker.Generate(5)
            .Select(x => x.WithUserId(userId)
                .WithAt(now))
            .ToArray();
        var calculationIds = await _calculationRepository.Add(calculations, default);
        
        // Act
        await _calculationRepository.Delete(calculationIds, default);
        var foundCalculations = await _calculationRepository.GetCalculations(
            calculationIds, default);
        
        // Assert
        foundCalculations.Should().BeEmpty();
    }

    [Fact]
    public async Task Delete_WhenSomeCalculationsAreContainedInDB_ResultGetCalculationHistoryShouldBeEmpty()
    {
        // Arrange
        var userId = Create.RandomId();
        var now = DateTimeOffset.UtcNow;
        var calculations = CalculationEntityV1Faker.Generate(5)
            .Select(x => x.WithUserId(userId)
                .WithAt(now))
            .ToArray();
        var calculationIds = (await _calculationRepository.Add(calculations, default)).ToList();
        calculationIds.AddRange(Enumerable.Range(0, 3).Select(x => Create.RandomId()));
        
        // Act
        await _calculationRepository.Delete(calculationIds.ToArray(), default);
        var foundCalculations = await _calculationRepository.GetCalculations(
            calculationIds.ToArray(), default);
        
        // Assert
        foundCalculations.Should().BeEmpty();
    }

    [Fact]
    public async Task
        DeleteAllFromUser_WhenCalculationsWithOneUserIdAreContainedInDB_ResultGetCalculationHistoryShouldBeEmpty()
    {
        // Arrange
        var userId = Create.RandomId();
        var now = DateTimeOffset.UtcNow;
        var calculations = CalculationEntityV1Faker.Generate(5)
            .Select(x => x.WithUserId(userId)
                .WithAt(now))
            .ToArray();
        var calculationIds = (await _calculationRepository.Add(calculations, default)).ToList();
        
        // Act
        await _calculationRepository.DeleteAllFromUser(userId, default);
        var foundCalculations = await _calculationRepository.GetCalculations(
            calculationIds.ToArray(), default);
        
        // Assert
        foundCalculations.Should().BeEmpty();
    }

    [Fact]
    public async Task
        DeleteAllFromUser_WhenTwoUserIdAreContainedInDB_ResultGetCalculationHistoryForAnotherUserIdShouldBeNotEmpty()
    {
        var userId1 = Create.RandomId();
        var userId2 = Create.RandomId();
        var now = DateTimeOffset.UtcNow;
        var calculations1 = CalculationEntityV1Faker.Generate(3)
            .Select(x => x.WithUserId(userId1)
                .WithAt(now))
            .ToArray();
        var calculations2 = CalculationEntityV1Faker.Generate(3)
            .Select(x => x.WithUserId(userId2)
                .WithAt(now))
            .ToArray();
        var calculationIds1 = (await _calculationRepository.Add(calculations1, default)).ToArray();
        var calculationIds2 = (await _calculationRepository.Add(calculations2, default)).ToArray();
        
        // Act
        await _calculationRepository.DeleteAllFromUser(userId1, default);
        var foundCalculations1 = await _calculationRepository.GetCalculations(
            calculationIds1, default);
        var foundCalculations2 = await _calculationRepository.GetCalculations(
            calculationIds2, default);
        
        // Assert
        foundCalculations1.Should().BeEmpty();
        foundCalculations2.Should().NotBeEmpty();
        foundCalculations2.Should().HaveCount(3);
    }
}
