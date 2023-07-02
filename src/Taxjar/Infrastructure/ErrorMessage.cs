using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("TaxJar.Tests")]

namespace Taxjar.Infrastructure
{
    internal static class ErrorMessage
    {
        public const string MissingTransactionId = "Transaction ID cannot be null or an empty string.";
        public const string MissingCustomerId = "Customer ID cannot be null or an empty string.";
    }
}
