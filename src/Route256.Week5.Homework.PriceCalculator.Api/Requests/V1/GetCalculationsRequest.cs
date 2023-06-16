namespace Route256.Week5.Homework.PriceCalculator.Api.Requests.V1;

public sealed record GetCalculationsRequest(
    long UserId,
    long[] CalculationIds);