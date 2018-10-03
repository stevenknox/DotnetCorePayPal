using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using PayPalWebDemo.Models;
using PayPalWebDemo.Services;

namespace PayPalWebDemo.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        
        #region Single PayPal Payment
        public async Task<IActionResult> CreatePayment()
        {
            var payment = await PayPalPaymentService.CreatePayment(GetBaseUrl(), "sale");
            
            var redirectUrl = payment.Links.FirstOrDefault(f=> f.Rel == "approval_url").Href;

            return Redirect(redirectUrl);
        }

        public async Task<IActionResult> PaymentCancelled()
        {
            // TODO: Handle cancelled payment
            return RedirectToAction("Error");
        }

        public async Task<IActionResult> PaymentSuccessful(string paymentId, string token, string PayerID)
        {
            // Execute Payment
            var payment = await PayPalPaymentService.ExecutePayment(paymentId, PayerID);

            return View();
        }
        #endregion

        #region Authorize PayPal Payment
        public async Task<IActionResult> AuthorizePayment()
        {
            var payment = await PayPalPaymentService.CreatePayment(GetBaseUrl(), "authorize");
            
            var redirectUrl = payment.Links.FirstOrDefault(f=> f.Rel == "approval_url").Href;

            return Redirect(redirectUrl);
        }

        public async Task<IActionResult> AuthorizeSuccessful(string paymentId, string token, string PayerID)
        {
            // Capture Payment
            var capture = await PayPalPaymentService.CapturePayment(paymentId, PayerID);

            return View();
        }
        #endregion

        #region Billing Plan and subscription
        // Create a billing plan and subscribe to it
        public async Task<IActionResult> Subscribe()
        {
            var plan = await PayPalSubscriptionsService.CreateBillingPlan("Tuts+ Plan", "Test plan for this article", GetBaseUrl());

            //active the plan
            await PayPalSubscriptionsService.ActivateBillingPlan(plan.Id);

            var subscription = await PayPalSubscriptionsService.CreateBillingAgreement(plan.Id, 
                new PayPal.v1.BillingAgreements.SimplePostalAddress
                {
                    City = "London", 
                    Line1 = "line 1",
                    PostalCode = "SW1A 1AA",
                    CountryCode = "GB"
                }, "Pedro Alonso", "Tuts+", DateTime.Now);
            
            var redirectUrl = subscription.Links.FirstOrDefault(f=> f.Rel == "approval_url").Href;

            return Redirect(redirectUrl);
        }

        public async Task<IActionResult> SubscribeSuccess(string token)
        {
            // Execute approved agreement
            await PayPalSubscriptionsService.ExecuteBillingAgreement(token);

            return View();
        }

        public async Task<IActionResult> SubscribeCancel(string token)
        {
            // TODO: Handle cancelled payment
            return RedirectToAction("Error");
        }
        #endregion

        // public async Task<IActionResult> Webhook()
        // {
        //     var client = PayPalConfiguration.GetClient();

        //     // Get the received request's headers
        //     var requestheaders = HttpContext.Request.Headers;

        //     // Get the received request's body
        //     var requestBody = string.Empty;
        //     using (var reader = new System.IO.StreamReader(HttpContext.Request.Body))
        //     {
        //         requestBody = reader.ReadToEnd();
        //     }

        //     dynamic jsonBody = JObject.Parse(requestBody);
        //     string webhookId = jsonBody.id;
        //     var ev = PayPal.v1.Webhooks.WebWebhookEvent.Get(apiContext, webhookId);
            
        //     // We have all the information the SDK needs, so perform the validation.
        //     // Note: at least on Sandbox environment this returns false.
        //     // var isValid = WebhookEvent.ValidateReceivedEvent(apiContext, ToNameValueCollection(requestheaders), requestBody, webhookId);
            
        //     switch (ev.event_type)
        //     {
        //         case "PAYMENT.CAPTURE.COMPLETED":
        //             // Handle payment completed
        //             break;
        //         case "PAYMENT.CAPTURE.DENIED":
        //             // Handle payment denied
        //             break;
        //             // Handle other webhooks
        //         default:
        //             break;
        //     }

        //     return Ok();
        // }

        public string GetBaseUrl()
        {
            return Request.Scheme + "://" + Request.Host;
        }

        public NameValueCollection ToNameValueCollection(IHeaderDictionary dict)
        {
            var nameValueCollection = new NameValueCollection();

            foreach (var kvp in dict)
            {
                string value = null;
                if (!string.IsNullOrEmpty(kvp.Value))
                {
                    value = kvp.Value.ToString();
                }

                nameValueCollection.Add(kvp.Key.ToString(), value);
            }

            return nameValueCollection;
        }
    }
}
