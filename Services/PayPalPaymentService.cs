using BraintreeHttp;
using PayPal.v1.Payments;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PayPalWebDemo.Services
{
    public static class PayPalPaymentService
    {
        public static async Task<Payment> CreatePayment(string baseUrl, string intent)
        {
            var client = PayPalConfiguration.GetClient();

            var payment = new Payment()
            {
                Intent = intent,    // `sale` or `authorize`
                Payer = new Payer() { PaymentMethod = "paypal" },
                Transactions = GetTransactionsList(),
                RedirectUrls = GetReturnUrls(baseUrl, intent)
            };

            PaymentCreateRequest request = new PaymentCreateRequest();
            request.RequestBody(payment);

            try 
            {
                //TODO - ASYNC
                HttpResponse response = await client.Execute(request);
                var statusCode = response.StatusCode;
                return response.Result<Payment>();
            } 
            catch(HttpException httpException) 
            {
                var statusCode = httpException.StatusCode;
                var debugId = httpException.Headers.GetValues("PayPal-Debug-Id").FirstOrDefault();

                throw httpException;
            }
        }

         public static async Task<Payment> GetPayment(string paymentId)
        {
            var client = PayPalConfiguration.GetClient();

            PaymentGetRequest request = new PaymentGetRequest(paymentId);

            try 
            {
                //TODO - ASYNC
                HttpResponse response = await client.Execute(request);
                var statusCode = response.StatusCode;
                return response.Result<Payment>();
            } 
            catch(HttpException httpException) 
            {
                var statusCode = httpException.StatusCode;
                var debugId = httpException.Headers.GetValues("PayPal-Debug-Id").FirstOrDefault();

                throw httpException;
            }
        }


        private static List<Transaction> GetTransactionsList()
        {
            // A transaction defines the contract of a payment
            // what is the payment for and who is fulfilling it. 
            var transactionList = new List<Transaction>();

            // The Payment creation API requires a list of Transaction; 
            // add the created Transaction to a List
            transactionList.Add(new Transaction()
            {
                Description = "Transaction description.",
                InvoiceNumber = GetRandomInvoiceNumber(),
                Amount = new Amount()
                {
                    Currency = "USD",
                    Total = "100.00",       // Total must be equal to sum of shipping, tax and subtotal.
                    Details = new AmountDetails() // Details: Let's you specify details of a payment amount.
                    {
                        Tax = "15",
                        Shipping = "10",
                        Subtotal = "75"
                    }
                },
                ItemList = new ItemList()
                {
                    Items = new List<Item>()
                    {
                        new Item()
                        {
                            Name = "Item Name",
                            Currency = "USD",
                            Price = "15",
                            Quantity = "5",
                            Sku = "sku"
                        }
                    }
                }
            });
            return transactionList;
        }

        private static RedirectUrls GetReturnUrls(string baseUrl, string intent)
        {
            var returnUrl = intent == "sale" ? "/Home/PaymentSuccessful" : "/Home/AuthorizeSuccessful";

            // Redirect URLS
            // These URLs will determine how the user is redirected from PayPal 
            // once they have either approved or canceled the payment.
            return new RedirectUrls()
            {
                CancelUrl = baseUrl + "/Home/PaymentCancelled",
                ReturnUrl = baseUrl + returnUrl
            };
        }

        public static async Task<Payment> ExecutePayment(string paymentId, string payerId)
        {
            var client = PayPalConfiguration.GetClient();
            
            var paymentExecution = new PaymentExecution() { PayerId = payerId };
            var payment = new Payment() { Id = paymentId };

            PaymentExecuteRequest request = new PaymentExecuteRequest(paymentId);
            request.RequestBody(paymentExecution);

            try 
            {
                //TODO - ASYNC
                HttpResponse response = await client.Execute(request);
                var statusCode = response.StatusCode;
                return response.Result<Payment>();
            } 
            catch(HttpException httpException) 
            {
                var statusCode = httpException.StatusCode;
                var debugId = httpException.Headers.GetValues("PayPal-Debug-Id").FirstOrDefault();

                throw httpException;
            }
        }

        public static async Task<Capture> CapturePayment(string paymentId, string PayerID)
        {
            var client = PayPalConfiguration.GetClient();

            var payment = await PayPalPaymentService.ExecutePayment(paymentId, PayerID); //https://github.com/paypal/PayPal-NET-SDK/issues/156

            var authId = payment
                .Transactions[0]
                .RelatedResources[0]
                .Authorization.Id;

            AuthorizationCaptureRequest request = new AuthorizationCaptureRequest(authId);
         
            // Specify an amount to capture.  By setting 'is_final_capture' to true, all remaining funds held by the authorization will be released from the funding instrument.
            var capture = new Capture()
            {
                Amount = new Amount()
                {
                    Currency = "USD",
                    Total = "4.54"
                },
                IsFinalCapture = true
            };

               request.RequestBody(capture);

            try 
            {
                //TODO - ASYNC
                HttpResponse response = await client.Execute(request);
                var statusCode = response.StatusCode;
                return response.Result<Capture>();
            } 
            catch(HttpException httpException) 
            {
                var statusCode = httpException.StatusCode;
                var debugId = httpException.Headers.GetValues("PayPal-Debug-Id").FirstOrDefault();

                throw httpException;
            }
        }
        
        public static string GetRandomInvoiceNumber()
        {
            return new Random().Next(999999).ToString();
        }
    }
}
