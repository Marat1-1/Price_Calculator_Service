using System;
using System.Linq;
using AutoBogus;
using Bogus;
using Route256.Week5.Homework.PriceCalculator.Bll.Commands;

namespace Route256.Week5.Homework.PriceCalculator.UnitTests.Fakers;

public static class ClearCalculationsHistoryCommandFaker
{
    private static readonly object locker = new();

    private static readonly Faker<ClearCalculationsHistoryCommand> Faker = new AutoFaker<ClearCalculationsHistoryCommand>()
        .RuleFor(x => x.UserId, f => f.Random.Long(0L))
        .RuleFor(x => x.CalculationIds, 
            f => Enumerable.Range(0, 5).Select(x => f.Random.Long(1, 4)).ToArray());

    public static ClearCalculationsHistoryCommand Generate()
    {
        lock (locker)
        {
            return Faker.Generate();
        }
    }

    public static ClearCalculationsHistoryCommand WithUserId(
        this ClearCalculationsHistoryCommand command,
        long userId)
    {
        return command with { UserId = userId };
    }
    
    public static ClearCalculationsHistoryCommand WithUserCalculationIds(
        this ClearCalculationsHistoryCommand command,
        long[] calculationIds)
    {
        return command with { CalculationIds = calculationIds };
    }
    
    public static ClearCalculationsHistoryCommand WithEmptyCalculationIds(
        this ClearCalculationsHistoryCommand command)
    {
        return command with { CalculationIds = Array.Empty<long>()};
    }
}