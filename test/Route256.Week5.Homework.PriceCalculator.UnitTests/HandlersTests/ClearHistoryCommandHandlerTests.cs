using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Route256.Week5.Homework.PriceCalculator.Bll.Commands;
using Route256.Week5.Homework.PriceCalculator.Bll.Exceptions;
using Route256.Week5.Homework.PriceCalculator.UnitTests.Builders;
using Route256.Week5.Homework.PriceCalculator.UnitTests.Extensions;
using Route256.Week5.Homework.PriceCalculator.UnitTests.Fakers;
using Route256.Week5.Homework.TestingInfrastructure.Creators;
using Xunit;

namespace Route256.Week5.Homework.PriceCalculator.UnitTests.HandlersTests;

public class ClearHistoryCommandHandlerTests
{
    [Fact]
    public async Task Handle_WhenCalculationIdsAreEmpty_ShouldBeSuccess()
    {
        // Arrange
        var command = ClearCalculationsHistoryCommandFaker.Generate().WithEmptyCalculationIds();
        
        var builder = new ClearHistoryCommandHandlerBuilder();
        builder.CalculationService
            .SetupDeleteAllCalculationsFromUser()
            .SetupDeleteAllGoodsFromUser();
        var handler = builder.Build();
        
        // Act
        await handler.Handle(command, default);
        
        // Assert
        handler.CalculationService
            .VerifyDeleteAllCalculationsFromUserWasCalledOnce(command.UserId)
            .VerifyDeleteAllGoodsFromUserWasCalledOnce(command.UserId)
            .VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Handle_WhenOneOrManyCalculationsNotFoundInDB_ShouldThrowOneOrManyCalculationsNotFoundException()
    {
        // Arrange
        var calculations = QueryCalculationModelFaker.Generate();
        var calculationIdsAreContainedInDb = calculations.Select(x => x.Id).ToArray();
        var command = ClearCalculationsHistoryCommandFaker.Generate();
        command.WithUserCalculationIds(
            command.CalculationIds
            .Concat(calculationIdsAreContainedInDb)
            .ToArray());

        var builder = new ClearHistoryCommandHandlerBuilder();
        builder.CalculationService
            .SetupGetCalculations(calculations);
        var handler = builder.Build();

        // Act, Assert
        await Assert.ThrowsAsync<OneOrManyCalculationsNotFoundException>(
            async () => await handler.Handle(command, default));
        handler.CalculationService
            .VerifyGetCalculationsWasCalledOnce(command.CalculationIds)
            .VerifyNoOtherCalls();
    }
    
    [Fact]
    public async Task Handle_WhenOneOrManyCalculationsBelongsToAnotherUser_ShouldThrowOneOrManyCalculationsNotFoundException()
    {
        // Arrange
        var userId1 = Create.RandomId();
        var userId2 = Create.RandomId();
        
        var calculations1 = QueryCalculationModelFaker
            .Generate(3)
            .Select(x => x.WithUserId(userId1))
            .ToArray();
        var calculations2 = QueryCalculationModelFaker
            .Generate(3)
            .Select(x => x.WithUserId(userId2))
            .ToArray();
        var calculations = calculations1.Concat(calculations2).ToArray();
        var calculationIds = calculations.Select(x => x.Id).ToArray();

        var command = new ClearCalculationsHistoryCommand(
            userId1, 
            calculationIds);
        
        var builder = new ClearHistoryCommandHandlerBuilder();
        builder.CalculationService
            .SetupGetCalculations(calculations);
        var handler = builder.Build();
        
        // Act, Assert
        await Assert.ThrowsAsync<OneOrManyCalculationsBelongsToAnotherUserException>(
            async () => await handler.Handle(command, default));
        handler.CalculationService
            .VerifyGetCalculationsWasCalledOnce(calculationIds)
            .VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Handle_WhenCommandIsCorrect_ShouldBeSuccess()
    {
        // Arrange
        var userId = Create.RandomId();
        
        var calculations = QueryCalculationModelFaker
            .Generate(5)
            .Select(x => x
                .WithUserId(userId)
                .WithGoodIds(
                    Enumerable
                        .Range(0, 3)
                        .Select(c => Create.RandomId())
                        .ToArray()))
            .ToArray();
        var calculationIds = calculations.Select(x => x.Id).ToArray();
        
        var goodIds = new List<long>();
        foreach (var calculation in calculations)
        {
            goodIds.AddRange(calculation.GoodIds);
        }
        
        var command = new ClearCalculationsHistoryCommand(userId, calculationIds);
        
        var builder = new ClearHistoryCommandHandlerBuilder();
        builder.CalculationService
            .SetupGetCalculations(calculations)
            .SetupDeleteCalculations()
            .SetupDeleteGoods();
        var handler = builder.Build();
        
        // Act
        await handler.Handle(command, default);
        
        // Assert
        handler.CalculationService
            .VerifyGetCalculationsWasCalledOnce(calculationIds)
            .VerifyDeleteCalculationsWasCalledOnce(calculationIds)
            .VerifyDeleteGoodsWasCalledOnce(goodIds.ToArray())
            .VerifyNoOtherCalls();
    }
}