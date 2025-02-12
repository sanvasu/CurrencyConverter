using System;

namespace CurrencyConverterAPI.Models
{
    public class InvalidCurrencyException : Exception
    {
        public InvalidCurrencyException() : base("Invalid currency was provided.")
        {
        }

        public InvalidCurrencyException(string message) : base(message)
        {
        }

        public InvalidCurrencyException(string message, Exception inner) : base(message, inner)
        {
        }
    }
}