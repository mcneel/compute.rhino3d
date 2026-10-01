using System;
using Newtonsoft.Json;

namespace compute.geometry
{
    // A request compute can't use because of what the caller sent. Answered with status 400 and this message.
    public class BadRequestException : Exception
    {
        public BadRequestException(string message, Exception inner = null) : base(message, inner) { }

        // The exceptions reading a value the caller sent can throw.
        public static bool IsBadValue(Exception ex) =>
            ex is JsonException || ex is FormatException || ex is InvalidCastException || ex is OverflowException || ex is ArgumentException;
    }
}
