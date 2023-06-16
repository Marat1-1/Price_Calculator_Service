using Microsoft.AspNetCore.Http;

namespace Route256.Week5.Homework.PriceCalculator.Bll.Exceptions;

internal sealed class OneOrManyCalculationsNotFoundException : BadHttpRequestException
{
    public OneOrManyCalculationsNotFoundException(string message) : base(message)
    {
    }
}