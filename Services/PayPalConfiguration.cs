using PayPal.Core;
using System.Collections.Generic;

namespace PayPalWebDemo.Services
{
    public static class PayPalConfiguration
    {
        public readonly static string ClientId;
        public readonly static string ClientSecret;

        // Static constructor for setting the readonly static members.
        static PayPalConfiguration()
        {
            var config = GetConfig();
            ClientId = config["clientId"];
            ClientSecret = config["clientSecret"];
        }

        // Create the configuration map that contains mode and other optional configuration details.
        public static Dictionary<string, string> GetConfig()
        {
            // ConfigManager.Instance.GetProperties(); // it doesn't work on ASPNET 5
            return new Dictionary<string, string>() {
                { "clientId", "YOUR-CLIENT-ID" },
                { "clientSecret", "YOUR-CLIENT-SECRET" }
            };
        }

        // Create accessToken
        private static SandboxEnvironment GetSandboxEnvironment()
        {
            return new SandboxEnvironment(ClientId, ClientSecret);
        }

        private static LiveEnvironment GetLiveEnvironment()
        {
            return new LiveEnvironment(ClientId, ClientSecret);
        }

        private static PayPalHttpClient GetClient(PayPalEnvironment environment)
        {
            return new PayPalHttpClient(environment);
        }
        
        public static PayPalHttpClient GetClient()
        {
            var env = GetSandboxEnvironment(); //this should toggle from appsettings
            return new PayPalHttpClient(env);
        }


    }
}
