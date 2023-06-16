using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Route256.Week5.Homework.PriceCalculator.Dal.Repositories.Interfaces;
using Route256.Week5.Homework.PriceCalculator.IntegrationTests.Fixtures;
using Route256.Week5.Homework.TestingInfrastructure.Creators;
using Route256.Week5.Homework.TestingInfrastructure.Fakers;
using Xunit;

namespace Route256.Week5.Homework.PriceCalculator.IntegrationTests.RepositoryTests;

[Collection(nameof(TestFixture))]
public class GoodsRepositoryTests
{
    private readonly double _requiredDoublePrecision = 0.00001d;

    private readonly IGoodsRepository _goodsRepository;

    public GoodsRepositoryTests(TestFixture fixture)
    {
        _goodsRepository = fixture.GoodsRepository;
    }

    [Theory]
    [InlineData(1)]
    [InlineData(5)]
    public async Task Add_Goods_Success(int count)
    {
        // Arrange
        var userId = Create.RandomId();
        
        var goods = GoodEntityV1Faker.Generate(count)
            .Select(x => x.WithUserId(userId))
            .ToArray();

        // Act
        var goodIds = await _goodsRepository.Add(goods, default);

        // Asserts
        goodIds.Should().HaveCount(count);
        goodIds.Should().OnlyContain(x => x > 0);
    }
    
    [Fact]
    public async Task Query_Goods_Success()
    {
        // Arrange
        var userId = Create.RandomId();
        
        var goods = GoodEntityV1Faker.Generate(10)
            .Select(x => x.WithUserId(userId))
            .ToArray();

        var goodIds = (await _goodsRepository.Add(goods, default))
            .ToHashSet();

        // Act
        var foundGoods = await _goodsRepository.Query(userId, default);

        // Assert
        foundGoods.Should().NotBeEmpty();
        foundGoods.Should().OnlyContain(x => x.UserId == userId);
        foundGoods.Should().OnlyContain(x => goodIds.Contains(x.Id));
    }
    
    [Fact]
    public async Task Query_SingleGood_Success()
    {
        // Arrange
        var userId = Create.RandomId();
        
        var goods = GoodEntityV1Faker.Generate()
            .Select(x => x.WithUserId(userId))
            .ToArray();
        var expected = goods.Single();

        var goodId = (await _goodsRepository.Add(goods, default))
            .Single();

        // Act
        var foundGoods = await _goodsRepository.Query(userId, default);

        // Assert
        foundGoods.Should().HaveCount(1);
        var good = foundGoods.Single();

        good.Id.Should().Be(goodId);
        good.UserId.Should().Be(expected.UserId);
        good.Height.Should().BeApproximately(expected.Height, _requiredDoublePrecision);
        good.Width.Should().BeApproximately(expected.Width, _requiredDoublePrecision);
        good.Length.Should().BeApproximately(expected.Length, _requiredDoublePrecision);
        good.Weight.Should().BeApproximately(expected.Weight, _requiredDoublePrecision);
    }
    
    [Fact]
    public async Task Query_Goods_ReturnsEmpty_WhenForWrongUser()
    {
        // Arrange
        var userId = Create.RandomId();
        var anotherUserId = Create.RandomId();
        
        var goods = GoodEntityV1Faker.Generate(10)
            .Select(x => x.WithUserId(userId))
            .ToArray();

        await _goodsRepository.Add(goods, default);

        // Act
        var foundGoods = await _goodsRepository.Query(anotherUserId, default);

        // Assert
        foundGoods.Should().BeEmpty();
    }
    
    [Fact]
    public async Task Delete_WhenOneGoodAreContainedInDB_ResultQueryShouldBeEmpty()
    {
        // Arrange
        var userId = Create.RandomId();
        var good = GoodEntityV1Faker.Generate()
            .Select(x => x.WithUserId(userId)).Single();

        var goodId = (await _goodsRepository.Add(new []{good}, default)).Single();
        
        // Act
        await _goodsRepository.Delete(new []{goodId}, default);
        var foundGood = await _goodsRepository.Query(userId, default);
        
        // Assert
        foundGood.Should().BeEmpty();
    }

    [Fact]
    public async Task Delete_WhenManyGoodAreContainedInDB_ResultQueryShouldBeEmpty()
    {
        // Arrange
        var userId = Create.RandomId();
        var goods = GoodEntityV1Faker.Generate(5)
            .Select(x => x.WithUserId(userId))
            .ToArray();

        var goodIds = (await _goodsRepository.Add(goods, default)).ToArray();
        
        // Act
        await _goodsRepository.Delete(goodIds, default);
        var foundGoods = await _goodsRepository.Query(userId, default);
        
        // Assert
        foundGoods.Should().BeEmpty();
    }
    
    [Fact]
    public async Task Delete_WhenSomeGoodAreContainedInDB_ResultQueryShouldBeEmpty()
    {
        // Arrange
        var userId = Create.RandomId();
        var goods = GoodEntityV1Faker.Generate(5)
            .Select(x => x.WithUserId(userId))
            .ToArray();

        var goodIds = (await _goodsRepository.Add(goods, default)).ToList();
        goodIds.AddRange(Enumerable.Range(0, 3).Select(x => Create.RandomId()));
        
        // Act
        await _goodsRepository.Delete(goodIds.ToArray(), default);
        var foundGoods = await _goodsRepository.Query(userId, default);
        
        // Assert
        foundGoods.Should().BeEmpty();
    }

    [Fact]
    public async Task DeleteAllFromUser_WhenGoodsWithOneUserIdAreContainedInDB_ResultQueryShouldBeEmpty()
    {
        // Arrange
        var userId = Create.RandomId();
        var goods = GoodEntityV1Faker.Generate(5)
            .Select(x => x.WithUserId(userId))
            .ToArray();
        await _goodsRepository.Add(goods, default);
        
        // Act
        await _goodsRepository.DeleteAllFromUser(userId, default);
        var foundGoods = await _goodsRepository.Query(userId, default);

        // Assert
        foundGoods.Should().BeEmpty();
    }
    
    [Fact]
    public async Task DeleteAllFromUser_WhenGoodsWithDifferentUserIdAreContainedInDB_ResultQueryShouldBeEmpty()
    {
        // Arrange
        var userId1 = Create.RandomId();
        var userId2 = Create.RandomId();
        var goods = GoodEntityV1Faker.Generate(5)
            .Select(x => x.WithUserId(userId1))
            .ToList();
        goods.AddRange(GoodEntityV1Faker.Generate(5)
            .Select(x => x.WithUserId(userId2)));
        await _goodsRepository.Add(goods.ToArray(), default);
        
        // Act
        await _goodsRepository.DeleteAllFromUser(userId1, default);
        var foundGoods1 = await _goodsRepository.Query(userId1, default);
        var foundGoods2 = await _goodsRepository.Query(userId2, default);

        // Assert
        foundGoods1.Should().BeEmpty();
        foundGoods2.Should().NotBeEmpty();
        foundGoods2.Should().HaveCount(5);
    }
}
