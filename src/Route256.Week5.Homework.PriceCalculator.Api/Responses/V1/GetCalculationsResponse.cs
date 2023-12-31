namespace Route256.Week5.Homework.PriceCalculator.Api.Responses.V1;

public record GetCalculationsResponse(GetCalculationsResponse.GetCalculationModelResponse[] Calculations)
{
    public record GetCalculationModelResponse(
        long CalculationId,
        long[] GoodIds,
        double TotalVolume,
        double TotalWeight,
        decimal Price
    );
}