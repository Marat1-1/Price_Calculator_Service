using Route256.Week5.Homework.PriceCalculator.Bll.Models;

namespace Route256.Week5.Homework.PriceCalculator.Bll.Services.Interfaces;

public interface ICalculationService
{
    /// <summary>
    /// Метод позволяет сохранить расчет и товары, участвующие в нем для конкретного пользователя
    /// </summary>
    /// <param name="saveCalculation"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<long> SaveCalculation(
        SaveCalculationModel saveCalculation,
        CancellationToken cancellationToken);

    /// <summary>
    /// Метод делает расчет стоимости доставки по объему
    /// </summary>
    /// <param name="goods"></param>
    /// <param name="volume"></param>
    /// <returns></returns>
    decimal CalculatePriceByVolume(
        GoodModel[] goods,
        out double volume);

    /// <summary>
    /// Метод делает расчет стоимости доставки по весу
    /// </summary>
    /// <param name="goods"></param>
    /// <param name="weight"></param>
    /// <returns></returns>
    public decimal CalculatePriceByWeight(
        GoodModel[] goods,
        out double weight);

    /// <summary>
    /// Метод позволяет получить расчеты для пользователя с фильтром
    /// </summary>
    /// <param name="query"></param>
    /// <param name="token"></param>
    /// <returns></returns>
    Task<QueryCalculationModel[]> QueryCalculations(
        QueryCalculationFilter query,
        CancellationToken token);
    
    /// <summary>
    /// Метод позволяет получить расчеты по переданным айдишникам
    /// </summary>
    /// <param name="calculationIds"></param>
    /// <param name="token"></param>
    /// <returns></returns>

    Task<QueryCalculationModel[]> GetCalculations(
        long[] calculationIds,
        CancellationToken token);
    
    /// <summary>
    /// Метод позволяет удалить расчеты, идентификаторы которых передаются в параметрах метода
    /// </summary>
    /// <param name="calculationIds"></param>
    /// <param name="token"></param>
    /// <returns></returns>

    Task DeleteCalculations(
        long[] calculationIds,
        CancellationToken token);

    /// <summary>
    /// Метод позволяет удалить товары, идентификаторы которых передаются в параметрах метода
    /// </summary>
    /// <param name="goodIds"></param>
    /// <param name="token"></param>
    /// <returns></returns>
    Task DeleteGoods(
        long[] goodIds,
        CancellationToken token);
    
    /// <summary>
    /// Метод позволяет удалить все расчеты у конкретного пользователя
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="token"></param>
    /// <returns></returns>
    Task DeleteAllCalculationsFromUser(
        long userId,
        CancellationToken token);

    /// <summary>
    /// Метод позволяет удалить все товары, которые участвовали в расчетах у конкретного пользователя
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="token"></param>
    /// <returns></returns>
    Task DeleteAllGoodsFromUser(
        long userId,
        CancellationToken token);
}