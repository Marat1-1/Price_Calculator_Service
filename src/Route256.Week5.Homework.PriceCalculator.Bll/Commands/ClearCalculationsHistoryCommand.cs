using System.Net;
using MediatR;
using Microsoft.AspNetCore.Http;
using Route256.Week5.Homework.PriceCalculator.Bll.Exceptions;
using Route256.Week5.Homework.PriceCalculator.Bll.Extensions;
using Route256.Week5.Homework.PriceCalculator.Bll.Services.Interfaces;

namespace Route256.Week5.Homework.PriceCalculator.Bll.Commands;

public record ClearCalculationsHistoryCommand(
    long UserId,
    long[] CalculationIds) : IRequest;

public class ClearHistoryCommandHandler : IRequestHandler<ClearCalculationsHistoryCommand>
{
    private readonly ICalculationService _calculationService;
    
    public ClearHistoryCommandHandler(ICalculationService calculationService)
    {
        _calculationService = calculationService;
    }
    
    public async Task Handle(
        ClearCalculationsHistoryCommand command, 
        CancellationToken token)
    {
        if (command.CalculationIds is null || command.CalculationIds.Length == 0)
        {
            await _calculationService.DeleteAllCalculationsFromUser(command.UserId, token);
            await _calculationService.DeleteAllGoodsFromUser(command.UserId, token);
            return;
        }
        
        var calculations =
            await _calculationService.GetCalculations(command.CalculationIds, token);
        var calculationIds = calculations.Select(x => x.Id).ToArray();

        var calculationIdsWereNotFoundInDb = command.CalculationIds
            .Where(x => !calculationIds.Contains(x))
            .ToArray();
        if (calculationIdsWereNotFoundInDb.Length != 0)
        {
            throw new OneOrManyCalculationsNotFoundException($"Some calculation ids were not found in DB!; " +
                                                             $"wrong_calculation_ids: " +
                                                             $"{calculationIdsWereNotFoundInDb.JoinElements(',')}");
        }

        var calculationIdsBelongToAnotherUser = calculations
            .Where(x => x.UserId != command.UserId)
            .Select(x => x.UserId)
            .ToArray();
        if (calculationIdsBelongToAnotherUser.Length != 0)
        {
            throw new OneOrManyCalculationsBelongsToAnotherUserException(
                $"Some calculations don't belong for user id: {command.UserId}; " +
                $"wrong_calculation_ids: {calculationIdsBelongToAnotherUser.JoinElements(',')}",
                StatusCodes.Status403Forbidden);
        }

        var goodIds = calculations
            .SelectMany(c => c.GoodIds)
            .ToArray();

        await _calculationService.DeleteCalculations(calculationIds, token);
        await _calculationService.DeleteGoods(goodIds.ToArray(), token);
    }
}    