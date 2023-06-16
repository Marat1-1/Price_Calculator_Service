namespace Route256.Week5.Homework.PriceCalculator.Bll.Extensions;

public static class EnumerableExtensions
{
    public static string JoinElements<T>(this IEnumerable<T> sequence, char separator = ' ')
    {
        return sequence.Aggregate(string.Empty, (current, el) => current + el + separator);
    }
}