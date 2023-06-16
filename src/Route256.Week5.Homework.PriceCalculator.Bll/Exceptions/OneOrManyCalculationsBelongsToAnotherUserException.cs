using Microsoft.AspNetCore.Http;

namespace Route256.Week5.Homework.PriceCalculator.Bll.Exceptions;

internal sealed class OneOrManyCalculationsBelongsToAnotherUserException : BadHttpRequestException
{
    public OneOrManyCalculationsBelongsToAnotherUserException(string message, int statusCode) : base(message, statusCode)
    {
    }
}