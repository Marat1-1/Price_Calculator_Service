using Moq;
using Route256.Week5.Homework.PriceCalculator.Bll.Services.Interfaces;
using Route256.Week5.Homework.PriceCalculator.UnitTests.Stubs;

namespace Route256.Week5.Homework.PriceCalculator.UnitTests.Builders;

public class ClearHistoryCommandHandlerBuilder
{
    public Mock<ICalculationService> CalculationService;
    
    public ClearHistoryCommandHandlerBuilder()
    {
        CalculationService = new Mock<ICalculationService>();
    }

    public ClearHistoryCommandHandlerStub Build()
    {
        return new ClearHistoryCommandHandlerStub(
            CalculationService);
    }
}