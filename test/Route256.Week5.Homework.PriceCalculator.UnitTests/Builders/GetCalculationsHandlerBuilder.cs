using Moq;
using Route256.Week5.Homework.PriceCalculator.Bll.Services.Interfaces;
using Route256.Week5.Homework.PriceCalculator.UnitTests.Stubs;

namespace Route256.Week5.Homework.PriceCalculator.UnitTests.Builders;

public class GetCalculationsHandlerBuilder
{
    public Mock<ICalculationService> CalculationService;
    
    public GetCalculationsHandlerBuilder()
    {
        CalculationService = new Mock<ICalculationService>();
    }
    
    public GetCalculationsHandlerStub Build()
    {
        return new GetCalculationsHandlerStub(
            CalculationService);
    }
}