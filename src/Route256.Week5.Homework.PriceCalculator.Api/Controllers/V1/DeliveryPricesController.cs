using MediatR;
using Microsoft.AspNetCore.Mvc;
using Route256.Week5.Homework.PriceCalculator.Api.Requests.V1;
using Route256.Week5.Homework.PriceCalculator.Api.Responses.V1;
using Route256.Week5.Homework.PriceCalculator.Bll.Commands;
using Route256.Week5.Homework.PriceCalculator.Bll.Models;
using Route256.Week5.Homework.PriceCalculator.Bll.Queries;

namespace Route256.Week5.Homework.PriceCalculator.Api.Controllers.V1;

[ApiController]
[Route("/v1/delivery-prices")]
public class DeliveryPricesController : ControllerBase
{
    private readonly IMediator _mediator;

    public DeliveryPricesController(
        IMediator mediator)
    {
        _mediator = mediator;
    }
    
    /// <summary>
    /// Метод расчета стоимости доставки на основе объема товаров
    /// или веса товара. Окончательная стоимость принимается как наибольшая
    /// </summary>
    /// <returns></returns>
    [HttpPost("calculate")]
    public async Task<CalculateResponse> Calculate(
        CalculateRequest request,
        CancellationToken ct)
    {
        var command = new CalculateDeliveryPriceCommand(
            request.UserId,
            request.Goods
                .Select(x => new GoodModel(
                    x.Height,
                    x.Length,
                    x.Width,
                    x.Weight))
                .ToArray());
        var result = await _mediator.Send(command, ct);
        
        return new CalculateResponse(
            result.CalculationId,
            result.Price);
    }
    
    
    /// <summary>
    /// Метод получения истории вычисления
    /// </summary>
    /// <returns></returns>
    [HttpPost("get-history")]
    public async Task<GetHistoryResponse[]> History(
        GetHistoryRequest request,
        CancellationToken ct)
    {
        var query = new GetCalculationHistoryQuery(
            request.UserId,
            request.Take,
            request.Skip);
        var result = await _mediator.Send(query, ct);

        return result.Items
            .Select(x => new GetHistoryResponse(
                new GetHistoryResponse.CargoResponse(
                    x.Volume,
                    x.Weight,
                    x.GoodIds),
                x.Price))
            .ToArray();
    }
    
    /// <summary>
    /// Метод удаления указанных расчетов с товарами 
    /// </summary>
    /// <param name="request"></param>
    /// <param name="token"></param>
    /// <returns></returns>
    [HttpPost("clear-history")]
    public async Task ClearHistory(
        ClearHistoryRequest request,
        CancellationToken token)
    {
        var command = new ClearCalculationsHistoryCommand(
            request.UserId,
            request.CalculationIds);

        await _mediator.Send(command, token);
    }
    
    /// <summary>
    /// Метод получения расчетов по айдишникам
    /// </summary>
    /// <param name="request"></param>
    /// <param name="token"></param>
    /// <returns></returns>
    [HttpPost("get-calculations")]
    public async Task<GetCalculationsResponse> GetCalculations(
        GetCalculationsRequest request,
        CancellationToken token)
    {
        var query = new GetCalculationsQuery(
            request.UserId,
            request.CalculationIds);
        
        var result = await _mediator.Send(query, token);
        
        return new GetCalculationsResponse(
            result.Calculations
                .Select(x => 
                    new GetCalculationsResponse.GetCalculationModelResponse(
                        x.Id, 
                        x.GoodIds, 
                        x.TotalVolume, 
                        x.TotalWeight, 
                        x.Price))
                .ToArray());
    }
}