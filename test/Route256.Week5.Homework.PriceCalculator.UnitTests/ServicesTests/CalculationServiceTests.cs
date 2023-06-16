using System.Linq;
using System.Threading.Tasks;
using System.Transactions;
using FluentAssertions;
using FluentMigrator.Runner.Generators.Base;
using Route256.Week5.Homework.PriceCalculator.Bll.Services;
using Route256.Week5.Homework.PriceCalculator.UnitTests.Builders;
using Route256.Week5.Homework.PriceCalculator.UnitTests.Extensions;
using Route256.Week5.Homework.PriceCalculator.UnitTests.Fakers;
using Route256.Week5.Homework.TestingInfrastructure.Creators;
using Route256.Week5.Homework.TestingInfrastructure.Fakers;
using Xunit;

namespace Route256.Week5.Homework.PriceCalculator.UnitTests.ServicesTests;

public class CalculationServiceTests
{
    [Fact]
    public async Task SaveCalculation_Success()
    {
        // arrange
        const int goodsCount = 5;
        
        var userId = Create.RandomId();
        var calculationId = Create.RandomId();
        
        var goodModels = GoodModelFaker.Generate(goodsCount)
            .ToArray();
        
        var goods = goodModels
            .Select(x => GoodEntityV1Faker.Generate().Single()
                .WithUserId(userId)
                .WithHeight(x.Height)
                .WithWidth(x.Width)
                .WithLength(x.Length)
                .WithWeight(x.Weight))
            .ToArray();
        var goodIds = goods.Select(x => x.Id)
            .ToArray();

        var calculationModel = CalculationModelFaker.Generate()
            .Single()
            .WithUserId(userId)
            .WithGoods(goodModels);
        
        var calculations = CalculationEntityV1Faker.Generate(1)
            .Select(x => x
                .WithId(calculationId)
                .WithUserId(userId)
                .WithPrice(calculationModel.Price)
                .WithTotalWeight(calculationModel.TotalWeight)
                .WithTotalVolume(calculationModel.TotalVolume))
            .ToArray();
        
        var builder = new CalculationServiceBuilder();
        builder.CalculationRepository
            .SetupAddCalculations(new [] { calculationId })
            .SetupCreateTransactionScope();
        builder.GoodsRepository
            .SetupAddGoods(goodIds);

        var service = builder.Build();

        // act
        var result = await service.SaveCalculation(calculationModel, default);

        // assert
        result.Should().Be(calculationId);
        service.CalculationRepository
            .VerifyAddWasCalledOnce(calculations)
            .VerifyCreateTransactionScopeWasCalledOnce(IsolationLevel.ReadCommitted);
        service.GoodsRepository
            .VerifyAddWasCalledOnce(goods);
        service.VerifyNoOtherCalls();
    }

    [Fact]
    public void CalculatePriceByVolume_Success()
    {
        // arrange
        var goodModels = GoodModelFaker.Generate(5)
            .ToArray();
        
        var builder = new CalculationServiceBuilder();
        var service = builder.Build();

        //act
        var price = service.CalculatePriceByVolume(goodModels, out var volume);

        //asserts
        volume.Should().BeApproximately(goodModels.Sum(x => x.Height * x.Width * x.Length), 1e-9d);
        price.Should().Be((decimal)volume * CalculationService.VolumeToPriceRatio);
    }
    
    [Fact]
    public void CalculatePriceByWeight_Success()
    {
        // arrange
        var goodModels = GoodModelFaker.Generate(5)
            .ToArray();
        
        var builder = new CalculationServiceBuilder();
        var service = builder.Build();

        //act
        var price = service.CalculatePriceByWeight(goodModels, out var weight);

        //asserts
        weight.Should().Be(goodModels.Sum(x => x.Weight));
        price.Should().Be((decimal)weight * CalculationService.WeightToPriceRatio);
    }
    
    [Fact]
    public async Task QueryCalculations_Success()
    {
        // arrange
        var userId = Create.RandomId();

        var filter = QueryCalculationFilterFaker.Generate()
            .WithUserId(userId);
        
        var calculations = CalculationEntityV1Faker.Generate(5)
            .Select(x => x.WithUserId(userId))
            .ToArray();

        var queryModel = CalculationHistoryQueryModelFaker.Generate()
            .WithUserId(userId)
            .WithLimit(filter.Limit)
            .WithOffset(filter.Offset);
        
        var builder = new CalculationServiceBuilder();
        builder.CalculationRepository
            .SetupQueryCalculation(calculations);
        var service = builder.Build();

        //act
        var result = await service.QueryCalculations(filter, default);

        //asserts
        service.CalculationRepository
            .VerifyQueryWasCalledOnce(queryModel);
        
        service.VerifyNoOtherCalls();

        result.Should().NotBeEmpty();
        result.Should().OnlyContain(x => x.UserId == userId);
        result.Should().OnlyContain(x => x.Id > 0);
        result.Select(x => x.TotalWeight)
            .Should().IntersectWith(calculations.Select(x => x.TotalWeight));
        result.Select(x => x.TotalVolume)
            .Should().IntersectWith(calculations.Select(x => x.TotalVolume));
        result.Select(x => x.Price)
            .Should().IntersectWith(calculations.Select(x => x.Price));
    }

    [Fact]
    public async Task GetCalculations_ShouldBeSuccess()
    {
        // Arrange
        var userId = Create.RandomId();
        var calculations = CalculationEntityV1Faker.Generate(5)
            .Select(x => x.WithUserId(userId))
            .ToArray();
        var calculationIds = calculations.Select(x => x.Id).ToArray();

        var builder = new CalculationServiceBuilder();
        builder.CalculationRepository
            .SetupGetCalculations(calculations);
        var service = builder.Build();

        // Act
        var result = await service.GetCalculations(calculationIds, default);

        // Assert
        service.CalculationRepository.VerifyGetCalculationsWasCalledOnce(calculationIds);
        service.VerifyNoOtherCalls();
        
        result.Should().NotBeEmpty();
        result.Should().OnlyContain(x => x.UserId == userId);
        result.Should().OnlyContain(x => x.Id > 0);
        result.Select(x => x.TotalWeight)
            .Should().IntersectWith(calculations.Select(x => x.TotalWeight));
        result.Select(x => x.TotalVolume)
            .Should().IntersectWith(calculations.Select(x => x.TotalVolume));
        result.Select(x => x.Price)
            .Should().IntersectWith(calculations.Select(x => x.Price));
    }

    [Fact]
    public async Task DeleteCalculations_ShouldBeSuccess()
    {
        // Arrange
        var calculationIds = Enumerable.Range(0, 5).Select(x => Create.RandomId()).ToArray();
        
        var builder = new CalculationServiceBuilder();
        builder.CalculationRepository
            .SetupDelete()
            .SetupCreateTransactionScope();
        var service = builder.Build();

        // Act
        await service.DeleteCalculations(calculationIds, default);

        // Assert
        service.CalculationRepository.VerifyDeleteWasCalledOnce(calculationIds);
        service.CalculationRepository.VerifyCreateTransactionScopeWasCalledOnce(IsolationLevel.ReadCommitted);
        service.VerifyNoOtherCalls();
    }
    
    [Fact]
    public async Task DeleteGoods_ShouldBeSuccess()
    {
        // Arrange
        var goodIds = Enumerable.Range(0, 5).Select(x => Create.RandomId()).ToArray();
        
        var builder = new CalculationServiceBuilder();
        builder.GoodsRepository
            .SetupDelete()
            .SetupCreateTransactionScope();
        var service = builder.Build();

        // Act
        await service.DeleteGoods(goodIds, default);

        // Assert
        service.GoodsRepository.VerifyDeleteWasCalledOnce(goodIds);
        service.GoodsRepository.VerifyCreateTransactionScopeWasCalledOnce(IsolationLevel.ReadCommitted);
        service.VerifyNoOtherCalls();
    }
    
    [Fact]
    public async Task DeleteAllCalculationsFromUser_ShouldBeSuccess()
    {
        // Arrange
        var userId = Create.RandomId();
        
        var builder = new CalculationServiceBuilder();
        builder.CalculationRepository
            .SetupDeleteAllFromUser()
            .SetupCreateTransactionScope();
        var service = builder.Build();

        // Act
        await service.DeleteAllCalculationsFromUser(userId, default);

        // Assert
        service.CalculationRepository.VerifyDeleteAllFromUserWasCalledOnce(userId);
        service.CalculationRepository.VerifyCreateTransactionScopeWasCalledOnce(IsolationLevel.ReadCommitted);
        service.VerifyNoOtherCalls();
    }
    
    [Fact]
    public async Task DeleteAllGoodsFromUser_ShouldBeSuccess()
    {
        // Arrange
        var userId = Create.RandomId();
        
        var builder = new CalculationServiceBuilder();
        builder.GoodsRepository
            .SetupDeleteAllFromUser()
            .SetupCreateTransactionScope();
        var service = builder.Build();

        // Act
        await service.DeleteAllGoodsFromUser(userId, default);

        // Assert
        service.GoodsRepository.VerifyDeleteAllFromUserWasCalledOnce(userId);
        service.GoodsRepository.VerifyCreateTransactionScopeWasCalledOnce(IsolationLevel.ReadCommitted);
        service.VerifyNoOtherCalls();
    }
}
