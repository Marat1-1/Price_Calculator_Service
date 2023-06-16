using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Route256.Week5.Homework.PriceCalculator.Bll.Queries;
using Route256.Week5.Homework.PriceCalculator.UnitTests.Builders;
using Route256.Week5.Homework.PriceCalculator.UnitTests.Extensions;
using Route256.Week5.Homework.PriceCalculator.UnitTests.Fakers;
using Route256.Week5.Homework.TestingInfrastructure.Creators;
using Xunit;

namespace Route256.Week5.Homework.PriceCalculator.UnitTests.HandlersTests;

public class GetCalculationsQueryHandlerTests
{
    [Fact]
    public async Task Handle_WhenOneOrManyCalculationIdsAreNotContainedInDB_ResultShouldBeEmpty()
    {
        // Arrange
        var userId = Create.RandomId();
        var calculations = 
            QueryCalculationModelFaker
                .Generate(5)
                .Select(x => x.WithUserId(userId))
                .ToArray();
        var calculationIds = calculations.Select(x => x.Id).ToArray();
        var queryCalculationIds = calculationIds
            .Concat(
                Enumerable.Range(0, 3)
                    .Select(x => Create.RandomId()))
            .ToArray();

        var builder = new GetCalculationsHandlerBuilder();
        builder.CalculationService
            .SetupGetCalculations(calculations);
        var handler = builder.Build();

        var query = new GetCalculationsQuery(userId, queryCalculationIds);
        
        // Act
        var result = await handler.Handle(
            query,
            default);
        
        // Assert
        result.Calculations.Should().BeEmpty();
        handler.CalculationService
            .VerifyGetCalculationsWasCalledOnce(queryCalculationIds)
            .VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Handle_WhenOneOrManyCalculationsBelongsToAnotherUser_ResultShouldBeEmpty()
    {
        // Arrange
        var userId1 = Create.RandomId();
        var userId2 = Create.RandomId();
        
        var calculations1 = 
            QueryCalculationModelFaker
                .Generate(5)
                .Select(x => x.WithUserId(userId1))
                .ToArray();
        var calculations2 = 
            QueryCalculationModelFaker
                .Generate(5)
                .Select(x => x.WithUserId(userId2))
                .ToArray();
        var calculations = calculations1.Concat(calculations2).ToArray();

        var queryCalculationIds = calculations.Select(x => x.Id).ToArray();
        
        var builder = new GetCalculationsHandlerBuilder();
        builder.CalculationService
            .SetupGetCalculations(calculations);
        var handler = builder.Build();
        
        // Act
        var result = await handler.Handle(
            new GetCalculationsQuery(userId1, queryCalculationIds),
            default);

        // Assert
        result.Calculations.Should().BeEmpty();
        handler.CalculationService
            .VerifyGetCalculationsWasCalledOnce(queryCalculationIds)
            .VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Handle_WhenQueryIsCorrect_ResultShouldBeValid()
    {
        // Arrange
        var userId = Create.RandomId();
        
        var calculations = 
            QueryCalculationModelFaker
                .Generate(5)
                .Select(x => x.WithUserId(userId))
                .Select(x => x
                    .WithGoodIds(Enumerable.Range(0, 3)
                        .Select(x => Create.RandomId())
                        .ToArray()))
                .ToArray();
        var calculationIds = calculations.Select(x => x.Id).ToArray();
        
        var builder = new GetCalculationsHandlerBuilder();
        builder.CalculationService
            .SetupGetCalculations(calculations);
        var handler = builder.Build();
        
        // Act
        var result = await handler.Handle(
            new GetCalculationsQuery(
                userId,
                calculationIds),
            default);
        
        // Assert
        handler.CalculationService
            .VerifyGetCalculationsWasCalledOnce(calculationIds)
            .VerifyNoOtherCalls();
        
        result.Calculations.Should().HaveCount(calculations.Length);
        result.Calculations.Should().OnlyContain(x => x.UserId == userId);
        result.Calculations.Select(x => x.Id)
            .Should().IntersectWith(calculationIds);
        result.Calculations.Select(x => x.Price)
            .Should().IntersectWith(calculations.Select(x => x.Price));
        result.Calculations.Select(x => x.TotalVolume)
            .Should().IntersectWith(calculations.Select(x => x.TotalVolume));
        result.Calculations.Select(x => x.TotalWeight)
            .Should().IntersectWith(calculations.Select(x => x.TotalWeight));
        result.Calculations.Select(x => x.GoodIds)
            .Should().IntersectWith(calculations.Select(x => x.GoodIds));
    }
}