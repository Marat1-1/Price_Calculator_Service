using MediatR;
using Route256.Week5.Homework.PriceCalculator.Bll.Models;
using Route256.Week5.Homework.PriceCalculator.Bll.Services.Interfaces;

namespace Route256.Week5.Homework.PriceCalculator.Bll.Queries;

public record GetCalculationsQuery(
    long UserId,
    long[] CalculationIds) : IRequest<GetCalculationsQueryResult>;

public class GetCalculationsQueryHandler
    : IRequestHandler<GetCalculationsQuery, GetCalculationsQueryResult>
{
    private readonly ICalculationService _calculationService;

    public GetCalculationsQueryHandler(ICalculationService calculationService)
    {
        _calculationService = calculationService;
    }

    public async Task<GetCalculationsQueryResult> Handle(
        GetCalculationsQuery request,
        CancellationToken token)
    {
        if (request.CalculationIds is null || request.CalculationIds.Length == 0)
            return new GetCalculationsQueryResult(Array.Empty<QueryCalculationModel>());
        
        var calculations = await _calculationService.GetCalculations(
            request.CalculationIds, 
            token);

        if (CalculationsContainWrongUserId(calculations, request.UserId) || 
            CalculationsNotContainAllIds(calculations, request.CalculationIds)
            )
        {
            return new GetCalculationsQueryResult(Array.Empty<QueryCalculationModel>());
        }

        return new GetCalculationsQueryResult(
            calculations.Select(c =>
                new QueryCalculationModel(
                    c.Id,
                    c.UserId,
                    c.TotalVolume,
                    c.TotalWeight,
                    c.Price,
                    c.GoodIds)).ToArray());
    }

    private static bool CalculationsContainWrongUserId(
        IEnumerable<QueryCalculationModel> calculations,
        long correctUserId)
    {
        return calculations.Any(c => c.UserId != correctUserId);
    }

    private static bool CalculationsNotContainAllIds(
        IEnumerable<QueryCalculationModel> calculations,
        IEnumerable<long> ids)
    {
        return ids.Any(c => !calculations.Select(x => x.Id).Contains(c));
    }
}