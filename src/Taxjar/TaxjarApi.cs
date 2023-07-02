using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
using Taxjar.Entities;
using Taxjar.Infrastructure;
using RestRequest = RestSharp.RestRequest;

namespace Taxjar
{
    public static class TaxjarConstants
    {
        public const string DefaultApiUrl = "https://api.taxjar.com";
        public const string SandboxApiUrl = "https://api.sandbox.taxjar.com";
        public const string ApiVersion = "v2";
    }

    public class TaxjarApi
    {
        internal RestClient ApiClient;
        public string ApiToken { get; set; }
        public string ApiUrl { get; set; }
        public IDictionary<string, string> Headers { get; set; }
        public int Timeout { get; set; }

        public TaxjarApi(string token, object parameters = null)
        {
            ApiToken = token;
            ApiUrl = TaxjarConstants.DefaultApiUrl + "/" + TaxjarConstants.ApiVersion + "/";
            Headers = new Dictionary<string, string>();
            Timeout = 0; // Milliseconds

            if (parameters != null)
            {
                if (parameters.GetType().GetProperty("apiUrl") != null)
                {
                    ApiUrl = parameters.GetType().GetProperty("apiUrl").GetValue(parameters).ToString();
                    ApiUrl += "/" + TaxjarConstants.ApiVersion + "/";
                }

                if (parameters.GetType().GetProperty("headers") != null)
                {
                    Headers = (IDictionary<string, string>)parameters.GetType().GetProperty("headers").GetValue(parameters);
                }

                if (parameters.GetType().GetProperty("timeout") != null)
                {
                    Timeout = (int)parameters.GetType().GetProperty("timeout").GetValue(parameters);
                }
            }

            if (string.IsNullOrWhiteSpace(ApiToken))
            {
                throw new ArgumentException("Please provide a TaxJar API key.", nameof(ApiToken));
            }

            ApiClient = new RestClient(ApiUrl);
            ApiClient.AddDefaultParameter("User-Agent", GetUserAgent(), ParameterType.HttpHeader);
        }

        public virtual void SetApiConfig(string key, object value)
        {
            if (key == "apiUrl")
            {
                value += "/" + TaxjarConstants.ApiVersion + "/";
                ApiClient = new RestClient(value.ToString());
            }

            GetType().GetProperty(key).SetValue(this, value, null);
        }

        public virtual object GetApiConfig(string key)
        {
            return GetType().GetProperty(key).GetValue(this);
        }

        protected virtual RestRequest CreateRequest(string action, Method method = Method.Post, object body = null)
        {
            var request = new RestRequest(action, method)
            {
                RequestFormat = DataFormat.Json
            };
            var includeBody = new[] { Method.Post, Method.Put, Method.Patch }.Contains(method);

            foreach (var header in Headers)
            {
                request.AddHeader(header.Key, header.Value);
            }

            request.AddHeader("Authorization", "Bearer " + ApiToken);
            request.AddHeader("User-Agent", GetUserAgent());

            request.Timeout = Timeout;

            if (body != null)
            {
                if (IsAnonymousType(body.GetType()))
                {
                    if (includeBody)
                    {
                        request.AddJsonBody(body);
                    }
                    else
                    {
                        foreach (var prop in body.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public))
                        {
                            request.AddQueryParameter(prop.Name, prop.GetValue(body).ToString());
                        }
                    }
                }
                else
                {
                    if (includeBody)
                    {
                        request.AddParameter("application/json", JsonConvert.SerializeObject(body), ParameterType.RequestBody);
                    }
                    else
                    {
                        body = JsonConvert.DeserializeObject(JsonConvert.SerializeObject(body));

                        foreach (var prop in JObject.FromObject(body).Properties())
                        {
                            request.AddQueryParameter(prop.Name, prop.Value.ToString());
                        }
                    }
                }
            }

            return request;
        }

        protected virtual T SendRequest<T>(string endpoint, object body = null, Method httpMethod = Method.Post) where T : new()
        {
            var request = CreateRequest(endpoint, httpMethod, body);
            var response = ApiClient.Execute<T>(request);

            if ((int)response.StatusCode >= 400)
            {
                var taxjarError = JsonConvert.DeserializeObject<TaxjarError>(response.Content);
                var errorMessage = taxjarError.Error + " - " + taxjarError.Detail;
                throw new TaxjarException(response.StatusCode, taxjarError, errorMessage);
            }

            if (response.ErrorException != null)
            {
                throw new Exception(response.ErrorMessage, response.ErrorException);
            }

            return JsonConvert.DeserializeObject<T>(response.Content);
        }

        protected virtual async Task<T> SendRequestAsync<T>(string endpoint, object body = null, Method httpMethod = Method.Post) where T : new()
        {
            var request = CreateRequest(endpoint, httpMethod, body);
            var response = await ApiClient.ExecuteAsync<T>(request).ConfigureAwait(false);

            if ((int)response.StatusCode >= 400)
            {
                var taxjarError = JsonConvert.DeserializeObject<TaxjarError>(response.Content);
                var errorMessage = taxjarError.Error + " - " + taxjarError.Detail;
                throw new TaxjarException(response.StatusCode, taxjarError, errorMessage);
            }

            if (response.ErrorException != null)
            {
                throw new Exception(response.ErrorMessage, response.ErrorException);
            }

            return JsonConvert.DeserializeObject<T>(response.Content);
        }

        protected virtual bool IsAnonymousType(Type type)
        {
            if (type == null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            return Attribute.IsDefined(type, typeof(CompilerGeneratedAttribute), false)
                && type.IsGenericType && type.Name.Contains("AnonymousType")
                && (type.Name.StartsWith("<>") || type.Name.StartsWith("VB$"))
                && (type.Attributes & TypeAttributes.NotPublic) == TypeAttributes.NotPublic;
        }

        public virtual List<Category> Categories()
        {
            var response = SendRequest<CategoriesResponse>("categories", null, Method.Get);
            return response.Categories;
        }

        public virtual RateResponseAttributes RatesForLocation(string zip, object parameters = null)
        {
            var response = SendRequest<RateResponse>("rates/" + zip, parameters, Method.Get);
            return response.Rate;
        }

        public virtual TaxResponseAttributes TaxForOrder(object parameters)
        {
            var response = SendRequest<TaxResponse>("taxes", parameters, Method.Post);
            return response.Tax;
        }

        public virtual List<String> ListOrders(object parameters = null)
        {
            var response = SendRequest<OrdersResponse>("transactions/orders", parameters, Method.Get);
            return response.Orders;
        }

        public virtual OrderResponseAttributes ShowOrder(string transactionId, object parameters = null)
        {
            var response = SendRequest<OrderResponse>("transactions/orders/" + transactionId, parameters, Method.Get);
            return response.Order;
        }

        public virtual OrderResponseAttributes CreateOrder(object parameters)
        {
            var response = SendRequest<OrderResponse>("transactions/orders", parameters, Method.Post);
            return response.Order;
        }

        public virtual OrderResponseAttributes UpdateOrder(object parameters)
        {
            var transactionId = GetTransactionIdFromParameters(parameters);
            var response = SendRequest<OrderResponse>("transactions/orders/" + transactionId, parameters, Method.Put);
            return response.Order;
        }

        public virtual OrderResponseAttributes DeleteOrder(string transactionId, object parameters = null)
        {
            var response = SendRequest<OrderResponse>("transactions/orders/" + transactionId, parameters, Method.Delete);
            return response.Order;
        }

        public virtual List<String> ListRefunds(object parameters)
        {
            var response = SendRequest<RefundsResponse>("transactions/refunds", parameters, Method.Get);
            return response.Refunds;
        }

        public virtual RefundResponseAttributes ShowRefund(string transactionId, object parameters = null)
        {
            var response = SendRequest<RefundResponse>("transactions/refunds/" + transactionId, parameters, Method.Get);
            return response.Refund;
        }

        public virtual RefundResponseAttributes CreateRefund(object parameters)
        {
            var response = SendRequest<RefundResponse>("transactions/refunds", parameters, Method.Post);
            return response.Refund;
        }

        public virtual RefundResponseAttributes UpdateRefund(object parameters)
        {
            var transactionId = GetTransactionIdFromParameters(parameters);

            var response = SendRequest<RefundResponse>("transactions/refunds/" + transactionId, parameters, Method.Put);
            return response.Refund;
        }

        public virtual RefundResponseAttributes DeleteRefund(string transactionId, object parameters = null)
        {
            var response = SendRequest<RefundResponse>("transactions/refunds/" + transactionId, parameters, Method.Delete);
            return response.Refund;
        }

        public virtual List<String> ListCustomers(object parameters = null)
        {
            var response = SendRequest<CustomersResponse>("customers", parameters, Method.Get);
            return response.Customers;
        }

        public virtual CustomerResponseAttributes ShowCustomer(string customerId)
        {
            var response = SendRequest<CustomerResponse>("customers/" + customerId, null, Method.Get);
            return response.Customer;
        }

        public virtual CustomerResponseAttributes CreateCustomer(object parameters)
        {
            var response = SendRequest<CustomerResponse>("customers", parameters, Method.Post);
            return response.Customer;
        }

        public virtual CustomerResponseAttributes UpdateCustomer(object parameters)
        {
            var customerId = GetCustomerIdFromParameters(parameters);

            var response = SendRequest<CustomerResponse>("customers/" + customerId, parameters, Method.Put);
            return response.Customer;
        }

        public virtual CustomerResponseAttributes DeleteCustomer(string customerId)
        {
            var response = SendRequest<CustomerResponse>("customers/" + customerId, null, Method.Delete);
            return response.Customer;
        }

        public virtual List<NexusRegion> NexusRegions()
        {
            var response = SendRequest<NexusRegionsResponse>("nexus/regions", null, Method.Get);
            return response.Regions;
        }

        public virtual List<Address> ValidateAddress(object parameters)
        {
            var response = SendRequest<AddressValidationResponse>("addresses/validate", parameters, Method.Post);
            return response.Addresses;
        }

        public virtual ValidationResponseAttributes ValidateVat(object parameters)
        {
            var response = SendRequest<ValidationResponse>("validation", parameters, Method.Get);
            return response.Validation;
        }

        public virtual List<SummaryRate> SummaryRates()
        {
            var response = SendRequest<SummaryRatesResponse>("summary_rates", null, Method.Get);
            return response.SummaryRates;
        }

        public virtual async Task<List<Category>> CategoriesAsync()
        {
            var response = await SendRequestAsync<CategoriesResponse>("categories", null, Method.Get).ConfigureAwait(false);
            return response.Categories;
        }

        public virtual async Task<RateResponseAttributes> RatesForLocationAsync(string zip, object parameters = null)
        {
            var response = await SendRequestAsync<RateResponse>("rates/" + zip, parameters, Method.Get).ConfigureAwait(false);
            return response.Rate;
        }

        public virtual async Task<TaxResponseAttributes> TaxForOrderAsync(object parameters)
        {
            var response = await SendRequestAsync<TaxResponse>("taxes", parameters, Method.Post).ConfigureAwait(false);
            return response.Tax;
        }

        public virtual async Task<List<string>> ListOrdersAsync(object parameters = null)
        {
            var response = await SendRequestAsync<OrdersResponse>("transactions/orders", parameters, Method.Get).ConfigureAwait(false);
            return response.Orders;
        }

        public virtual async Task<OrderResponseAttributes> ShowOrderAsync(string transactionId, object parameters = null)
        {
            var response = await SendRequestAsync<OrderResponse>("transactions/orders/" + transactionId, parameters, Method.Get).ConfigureAwait(false);
            return response.Order;
        }

        public virtual async Task<OrderResponseAttributes> CreateOrderAsync(object parameters)
        {
            var response = await SendRequestAsync<OrderResponse>("transactions/orders", parameters, Method.Post).ConfigureAwait(false);
            return response.Order;
        }

        public virtual async Task<OrderResponseAttributes> UpdateOrderAsync(object parameters)
        {
            var transactionId = GetTransactionIdFromParameters(parameters);
            var response = await SendRequestAsync<OrderResponse>("transactions/orders/" + transactionId, parameters, Method.Put).ConfigureAwait(false);
            return response.Order;
        }

        public virtual async Task<OrderResponseAttributes> DeleteOrderAsync(string transactionId, object parameters = null)
        {
            var response = await SendRequestAsync<OrderResponse>("transactions/orders/" + transactionId, parameters, Method.Delete).ConfigureAwait(false);
            return response.Order;
        }

        public virtual async Task<List<string>> ListRefundsAsync(object parameters)
        {
            var response = await SendRequestAsync<RefundsResponse>("transactions/refunds", parameters, Method.Get).ConfigureAwait(false);
            return response.Refunds;
        }

        public virtual async Task<RefundResponseAttributes> ShowRefundAsync(string transactionId, object parameters = null)
        {
            var response = await SendRequestAsync<RefundResponse>("transactions/refunds/" + transactionId, parameters, Method.Get).ConfigureAwait(false);
            return response.Refund;
        }

        public virtual async Task<RefundResponseAttributes> CreateRefundAsync(object parameters)
        {
            var response = await SendRequestAsync<RefundResponse>("transactions/refunds", parameters, Method.Post).ConfigureAwait(false);
            return response.Refund;
        }

        public virtual async Task<RefundResponseAttributes> UpdateRefundAsync(object parameters)
        {
            var transactionId = GetTransactionIdFromParameters(parameters);
            var response = await SendRequestAsync<RefundResponse>("transactions/refunds/" + transactionId, parameters, Method.Put).ConfigureAwait(false);
            return response.Refund;
        }

        public virtual async Task<RefundResponseAttributes> DeleteRefundAsync(string transactionId, object parameters = null)
        {
            var response = await SendRequestAsync<RefundResponse>("transactions/refunds/" + transactionId, parameters, Method.Delete).ConfigureAwait(false);
            return response.Refund;
        }

        public virtual async Task<List<string>> ListCustomersAsync(object parameters = null)
        {
            var response = await SendRequestAsync<CustomersResponse>("customers", parameters, Method.Get).ConfigureAwait(false);
            return response.Customers;
        }

        public virtual async Task<CustomerResponseAttributes> ShowCustomerAsync(string customerId)
        {
            var response = await SendRequestAsync<CustomerResponse>("customers/" + customerId, null, Method.Get).ConfigureAwait(false);
            return response.Customer;
        }

        public virtual async Task<CustomerResponseAttributes> CreateCustomerAsync(object parameters)
        {
            var response = await SendRequestAsync<CustomerResponse>("customers", parameters, Method.Post).ConfigureAwait(false);
            return response.Customer;
        }

        public virtual async Task<CustomerResponseAttributes> UpdateCustomerAsync(object parameters)
        {
            var customerId = GetCustomerIdFromParameters(parameters);
            var response = await SendRequestAsync<CustomerResponse>("customers/" + customerId, parameters, Method.Put).ConfigureAwait(false);
            return response.Customer;
        }

        public virtual async Task<CustomerResponseAttributes> DeleteCustomerAsync(string customerId)
        {
            var response = await SendRequestAsync<CustomerResponse>("customers/" + customerId, null, Method.Delete).ConfigureAwait(false);
            return response.Customer;
        }

        public virtual async Task<List<NexusRegion>> NexusRegionsAsync()
        {
            var response = await SendRequestAsync<NexusRegionsResponse>("nexus/regions", null, Method.Get).ConfigureAwait(false);
            return response.Regions;
        }

        public virtual async Task<List<Address>> ValidateAddressAsync(object parameters)
        {
            var response = await SendRequestAsync<AddressValidationResponse>("addresses/validate", parameters, Method.Post).ConfigureAwait(false);
            return response.Addresses;
        }

        public virtual async Task<ValidationResponseAttributes> ValidateVatAsync(object parameters)
        {
            var response = await SendRequestAsync<ValidationResponse>("validation", parameters, Method.Get).ConfigureAwait(false);
            return response.Validation;
        }

        public virtual async Task<List<SummaryRate>> SummaryRatesAsync()
        {
            var response = await SendRequestAsync<SummaryRatesResponse>("summary_rates", null, Method.Get).ConfigureAwait(false);
            return response.SummaryRates;
        }

        private string GetTransactionIdFromParameters(object parameters)
        {
            var propertyInfo = parameters.GetType().GetProperty("transaction_id") ?? parameters.GetType().GetProperty("TransactionId");
            var transactionId = GetValueOrDefault(parameters, propertyInfo);

            if (string.IsNullOrWhiteSpace(transactionId))
            {
                throw new ArgumentException(ErrorMessage.MissingTransactionId);
            }

            return transactionId;
        }

        private string GetCustomerIdFromParameters(object parameters)
        {
            var propertyInfo = parameters.GetType().GetProperty("customer_id") ?? parameters.GetType().GetProperty("CustomerId");
            var customerId = GetValueOrDefault(parameters, propertyInfo);

            if (string.IsNullOrWhiteSpace(customerId))
            {
                throw new ArgumentException(ErrorMessage.MissingCustomerId);
            }

            return customerId;
        }

        private string GetValueOrDefault(object parameters, PropertyInfo propertyInfo)
        {
            return propertyInfo == null ? null : propertyInfo.GetValue(parameters).ToString();
        }

        private string GetUserAgent()
        {

            string platform = RuntimeInformation.OSDescription;
            string arch = RuntimeInformation.OSArchitecture.ToString();
            string framework = RuntimeInformation.FrameworkDescription;

            string version = GetType().Assembly.GetName().Version.ToString(3);

            return $"TaxJar/.NET ({platform}; {arch}; {framework}) taxjar.net/{version}";
        }
    }
}
