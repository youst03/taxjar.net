﻿using System.Collections.Generic;
using Newtonsoft.Json;

namespace Taxjar.Entities
{
    public class RefundsResponse
    {
        [JsonProperty("refunds")]
        public List<string>? Refunds { get; set; }
    }

    public class RefundResponse
    {
        [JsonProperty("refund")]
        public RefundResponseAttributes? Refund { get; set; }
    }

    public class RefundResponseAttributes
    {
        [JsonProperty("transaction_id")]
        public string? TransactionId { get; set; }

        [JsonProperty("transaction_reference_id")]
        public string? TransactionReferenceId { get; set; }

        [JsonProperty("user_id")]
        public int UserId { get; set; }

        [JsonProperty("transaction_date")]
        public string? TransactionDate { get; set; }

        [JsonProperty("provider")]
        public string? Provider { get; set; }

        [JsonProperty("exemption_type")]
        public string? ExemptionType { get; set; }

        [JsonProperty("from_country")]
        public string? FromCountry { get; set; }

        [JsonProperty("from_zip")]
        public string? FromZip { get; set; }

        [JsonProperty("from_state")]
        public string? FromState { get; set; }

        [JsonProperty("from_city")]
        public string? FromCity { get; set; }

        [JsonProperty("from_street")]
        public string? FromStreet { get; set; }

        [JsonProperty("to_country")]
        public string? ToCountry { get; set; }

        [JsonProperty("to_zip")]
        public string? ToZip { get; set; }

        [JsonProperty("to_state")]
        public string? ToState { get; set; }

        [JsonProperty("to_city")]
        public string? ToCity { get; set; }

        [JsonProperty("to_street")]
        public string? ToStreet { get; set; }

        [JsonProperty("amount", NullValueHandling = NullValueHandling.Ignore)]
        public decimal Amount { get; set; }

        [JsonProperty("shipping", NullValueHandling = NullValueHandling.Ignore)]
        public decimal Shipping { get; set; }

        [JsonProperty("sales_tax", NullValueHandling = NullValueHandling.Ignore)]
        public decimal SalesTax { get; set; }

        [JsonProperty("line_items")]
        public List<LineItem>? LineItems { get; set; }
    }

    public class Refund
    {
        [JsonProperty("transaction_id")]
        public string? TransactionId { get; set; }

        [JsonProperty("transaction_reference_id")]
        public string? TransactionReferenceId { get; set; }

        [JsonProperty("transaction_date")]
        public string? TransactionDate { get; set; }

        [JsonProperty("provider", NullValueHandling = NullValueHandling.Ignore)]
        public string? Provider { get; set; }

        [JsonProperty("exemption_type", NullValueHandling = NullValueHandling.Ignore)]
        public string? ExemptionType { get; set; }

        [JsonProperty("from_country")]
        public string? FromCountry { get; set; }

        [JsonProperty("from_zip")]
        public string? FromZip { get; set; }

        [JsonProperty("from_state")]
        public string? FromState { get; set; }

        [JsonProperty("from_city")]
        public string? FromCity { get; set; }

        [JsonProperty("from_street")]
        public string? FromStreet { get; set; }

        [JsonProperty("to_country")]
        public string? ToCountry { get; set; }

        [JsonProperty("to_zip")]
        public string? ToZip { get; set; }

        [JsonProperty("to_state")]
        public string? ToState { get; set; }

        [JsonProperty("to_city")]
        public string? ToCity { get; set; }

        [JsonProperty("to_street")]
        public string? ToStreet { get; set; }

        [JsonProperty("amount", NullValueHandling = NullValueHandling.Ignore)]
        public decimal Amount { get; set; }

        [JsonProperty("shipping", NullValueHandling = NullValueHandling.Ignore)]
        public decimal Shipping { get; set; }

        [JsonProperty("sales_tax", NullValueHandling = NullValueHandling.Ignore)]
        public decimal SalesTax { get; set; }

        [JsonProperty("customer_id")]
        public string? CustomerId { get; set; }

        [JsonProperty("line_items")]
        public List<LineItem>? LineItems { get; set; }
    }

    public class RefundFilter
    {
        [JsonProperty("transaction_date")]
        public string? TransactionDate { get; set; }

        [JsonProperty("from_transaction_date")]
        public string? FromTransactionDate { get; set; }

        [JsonProperty("to_transaction_date")]
        public string? ToTransactionDate { get; set; }

        [JsonProperty("provider")]
        public string? Provider { get; set; }
    }
}
