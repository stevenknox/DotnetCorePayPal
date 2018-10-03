using BraintreeHttp;
using PayPal.Core;
using PayPal.v1.BillingAgreements;
using PayPal.v1.BillingPlans;
using PayPal.v1.Orders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PayPalWebDemo.Services
{
    public static class PayPalSubscriptionsService
    {
        public static async Task<Plan> CreateBillingPlan(string name, string description, string baseUrl)
        {
            var client = PayPalConfiguration.GetClient();

            var returnUrl = baseUrl + "/Home/SubscribeSuccess";
            var cancelUrl = baseUrl + "/Home/SubscribeCancel";

            // Plan Details
            var plan = CreatePlanObject("Test Plan", "Plan for Tuts+", returnUrl, cancelUrl, 
                PlanInterval.Month, 1, (decimal)19.90, trial: true, trialLength: 1, trialPrice: 0);

            PlanCreateRequest request = new PlanCreateRequest();
            request.RequestBody(plan);
            try 
            {
                HttpResponse response = await client.Execute(request);
                var statusCode = response.StatusCode;
                return response.Result<Plan>();
            } 
            catch(HttpException httpException) 
            {
                var statusCode = httpException.StatusCode;
                var debugId = httpException.Headers.GetValues("PayPal-Debug-Id").FirstOrDefault();

                throw httpException;
            }

        }

        public static async Task<string> ActivateBillingPlan(string planId)
        {
            var client = PayPalConfiguration.GetClient();

            var updates = new Plan()
            {
                State = "ACTIVE"
            };
            var patch = new PayPal.v1.BillingPlans.JsonPatch<Plan>()
            {
                Op = "replace",
                Path = "/",
                Value = updates
            };

            PlanUpdateRequest<Plan> request = new PlanUpdateRequest<Plan>(planId);
            request.RequestBody(new List<PayPal.v1.BillingPlans.JsonPatch<Plan>> { patch });

            try 
            {
                HttpResponse response = await client.Execute(request);
                var statusCode = response.StatusCode;
                return response.Result<string>();
            } 
            catch(HttpException httpException) 
            {
                var statusCode = httpException.StatusCode;
                var debugId = httpException.Headers.GetValues("PayPal-Debug-Id").FirstOrDefault();

                throw httpException;
            }
        }
        
        public static async Task<Plan> UpdateBillingPlan(string planId, string path, object value)
        {
             var client = PayPalConfiguration.GetClient();

            var updates = new Plan()
            {
                State = "ACTIVE"
            };
            var patch = new PayPal.v1.BillingPlans.JsonPatch<Plan>()
            {
                Op = "replace",
                Path = "/",
                Value = updates
            };

            PlanUpdateRequest<Plan> request = new PlanUpdateRequest<Plan>(planId);
            request.RequestBody(new List<PayPal.v1.BillingPlans.JsonPatch<Plan>> { patch });

            try 
            {
                HttpResponse response = await client.Execute(request);
                var statusCode = response.StatusCode;
                return response.Result<Plan>();
            } 
            catch(HttpException httpException) 
            {
                var statusCode = httpException.StatusCode;
                var debugId = httpException.Headers.GetValues("PayPal-Debug-Id").FirstOrDefault();

                throw httpException;
            }
        }

        public static async Task<Plan> DeactivateBillingPlan(string planId)
        {
            return await UpdateBillingPlan(
                planId: planId,
                path: "/",
                value: new Plan { State = "INACTIVE" });
        }

        public static async Task<Agreement> CreateBillingAgreement(string planId, SimplePostalAddress shippingAddress, 
            string name, string description, DateTime startDate)
        {
            var client = PayPalConfiguration.GetClient();

            var agreement = new Agreement()
            {
                Name = name,
                Description = description,
                StartDate = startDate.ToString("yyyy-MM-ddTHH:mm:ss") + "Z",
                Payer = new PayPal.v1.BillingAgreements.Payer() { PaymentMethod = "paypal" },
                Plan = new PlanWithId() { Id = planId },
                ShippingAddress = shippingAddress
            };

            AgreementCreateRequest request = new AgreementCreateRequest();
            request.RequestBody(agreement);
            request.Body = agreement;

            try 
            {
                HttpResponse response = await client.Execute(request);
                var statusCode = response.StatusCode;
                return response.Result<Agreement>();
            } 
            catch(HttpException httpException) 
            {
                var statusCode = httpException.StatusCode;
                var debugId = httpException.Headers.GetValues("PayPal-Debug-Id").FirstOrDefault();

                throw httpException;
            }
        }

        public static async Task<Agreement> ExecuteBillingAgreement(string token)
        {
           var client = PayPalConfiguration.GetClient();

           AgreementExecuteRequest request = new AgreementExecuteRequest(token);

            // NOTE: There is a known bug where this endpoint requires an empty JSON body.
            request.Body = "{}";
            
            try 
            {
                HttpResponse response = await client.Execute(request);
                var statusCode = response.StatusCode;
                return response.Result<Agreement>();
            } 
            catch(HttpException httpException) 
            {
                var statusCode = httpException.StatusCode;
                var debugId = httpException.Headers.GetValues("PayPal-Debug-Id").FirstOrDefault();

                throw httpException;
            }
        }

        public static async Task<Agreement> SuspendBillingAgreement(string agreementId)
        {
           var client = PayPalConfiguration.GetClient();

           AgreementSuspendRequest request = new AgreementSuspendRequest(agreementId);
           request.RequestBody(new AgreementStateDescriptor
           {
                Note = "Suspending the agreement"
           });

           try 
            {
                HttpResponse response = await client.Execute(request);
                var statusCode = response.StatusCode;
                return response.Result<Agreement>();
            } 
            catch(HttpException httpException) 
            {
                var statusCode = httpException.StatusCode;
                var debugId = httpException.Headers.GetValues("PayPal-Debug-Id").FirstOrDefault();

                throw httpException;
            }
        }

        public static async Task<Agreement> ReactivateBillingAgreement(string agreementId)
        {
           var client = PayPalConfiguration.GetClient();

           AgreementReActivateRequest request = new AgreementReActivateRequest(agreementId);
           request.RequestBody(new AgreementStateDescriptor
           {
                Note = "Re-activating the agreement"
           });

           try 
            {
                HttpResponse response = await client.Execute(request);
                var statusCode = response.StatusCode;
                return response.Result<Agreement>();
            } 
            catch(HttpException httpException) 
            {
                var statusCode = httpException.StatusCode;
                var debugId = httpException.Headers.GetValues("PayPal-Debug-Id").FirstOrDefault();

                throw httpException;
            }
        }
    
        public static async Task<Agreement> CancelBillingAgreement(string agreementId)
        {
           var client = PayPalConfiguration.GetClient();

           AgreementCancelRequest request = new AgreementCancelRequest(agreementId);
           request.RequestBody(new AgreementStateDescriptor
           {
                Note = "Cancelling the agreement"
           });

           try 
            {
                HttpResponse response = await client.Execute(request);
                var statusCode = response.StatusCode;
                return response.Result<Agreement>();
            } 
            catch(HttpException httpException) 
            {
                var statusCode = httpException.StatusCode;
                var debugId = httpException.Headers.GetValues("PayPal-Debug-Id").FirstOrDefault();

                throw httpException;
            }
        }

        #region Helpers
        /// <summary>
        /// Helper method for getting a currency amount.
        /// </summary>
        /// <param name="value">The value for the currency object.</param>
        /// <returns></returns>
        private static PayPal.v1.BillingPlans.Currency GetCurrency(string value)
        {
            return new PayPal.v1.BillingPlans.Currency() { Value = value, CurrencyCode = "USD" };
        }

        private static class PlanType
        {
            /// <summary>
            /// Use Fixed when you want to create a billing plan with a fixed number of payments (cycles)
            /// </summary>
            public static string Fixed { get { return "fixed"; } }

            /// <summary>
            /// Use Infinite and set cycles to 0 for a billing plan that is active until it's manually cancelled
            /// </summary>
            public static string Infinite { get { return "infinite"; } }
        }

        private static class PlanInterval
        {
            public static string Week { get { return "Week"; } }
            public static string Day { get { return "Day"; } }
            public static string Month { get { return "Month"; } }
            public static string Year { get { return "Year"; } }
        }

        public static Plan CreatePlanObject(string planName, string planDescription, string returnUrl, string cancelUrl,
            string frequency, int frequencyInterval, decimal planPrice,
            decimal shippingAmount = 0, decimal taxPercentage = 0, bool trial = false, int trialLength = 0, decimal trialPrice = 0)
        {
            // Define the plan and attach the payment definitions and merchant preferences.
            // More Information: https://developer.paypal.com/docs/rest/api/payments.billing-plans/
            return new Plan
            {
                Name = planName,
                Description = planDescription,
                Type = PlanType.Fixed,

                // Define the merchant preferences.
                // More Information: https://developer.paypal.com/webapps/developer/docs/api/#merchantpreferences-object
                MerchantPreferences = new PayPal.v1.BillingPlans.MerchantPreferences()
                {
                    SetupFee = GetCurrency("1"),
                    ReturnUrl = returnUrl,
                    CancelUrl = cancelUrl,
                    AutoBillAmount = "YES",
                    InitialFailAmountAction = "CONTINUE",
                    MaxFailAttempts = "0"
                },
                PaymentDefinitions = GetPaymentDefinitions(trial, trialLength, trialPrice, frequency, frequencyInterval, planPrice, shippingAmount, taxPercentage)
            };
        }

        private static List<PaymentDefinition> GetPaymentDefinitions(bool trial, int trialLength, decimal trialPrice,
            string frequency, int frequencyInterval, decimal planPrice, decimal shippingAmount, decimal taxPercentage)
        {
            var paymentDefinitions = new List<PaymentDefinition>();

            if (trial)
            {
                // Define a trial plan that will charge 'trialPrice' for 'trialLenght'
                // After that, the standard plan will take over.
                paymentDefinitions.Add(
                    new PaymentDefinition()
                    {
                        Name = "Trial",
                        Type = "TRIAL",
                        Frequency = frequency,
                        FrequencyInterval = frequencyInterval.ToString(),
                        Amount = GetCurrency(trialPrice.ToString()),
                        Cycles = trialLength.ToString(),
                        ChargeModels = GetChargeModels(trialPrice, shippingAmount, taxPercentage)
                    });
            }

            // Define the standard payment plan. It will represent a 'frequency' (monthly, etc)
            // plan for 'planPrice' that charges 'planPrice' (once a month) for #cycles.
            var regularPayment = new PaymentDefinition
            {
                Name = "Standard Plan",
                Type = "REGULAR",
                Frequency = frequency,
                FrequencyInterval = frequencyInterval.ToString(),
                Amount = GetCurrency(planPrice.ToString()),
                // > NOTE: For `IFNINITE` type plans, `cycles` should be 0 for a `REGULAR` `PaymentDefinition` object.
                Cycles = "11",
                ChargeModels = GetChargeModels(trialPrice, shippingAmount, taxPercentage)
            };
            paymentDefinitions.Add(regularPayment);

            return paymentDefinitions;
        }

        private static List<ChargeModel> GetChargeModels(decimal planPrice, decimal shippingAmount, decimal taxPercentage)
        {
            // Create the Billing Plan
            var chargeModels = new List<ChargeModel>();
            if (shippingAmount > 0)
            {
                chargeModels.Add(new ChargeModel()
                {
                    Type = "SHIPPING",
                    Amount = GetCurrency(shippingAmount.ToString())
                });
            }
            if (taxPercentage > 0)
            {
                chargeModels.Add(new ChargeModel()
                {
                    Type = "TAX",
                    Amount = GetCurrency(String.Format("{0:f2}", planPrice * taxPercentage / 100))
                });
            }

            return chargeModels;
        }
        #endregion
    }
}
