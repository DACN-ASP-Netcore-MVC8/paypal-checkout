using Microsoft.Extensions.Configuration;
using PayPal.Api;
using System.Collections.Generic;

public static class PaypalConfiguration
{
    private static readonly IConfiguration Configuration;

    static PaypalConfiguration()
    {
        var builder = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

        Configuration = builder.Build();
        ClientId = Configuration["PayPal:ClientId"];
        ClientSecret = Configuration["PayPal:ClientSecret"];
    }

    public readonly static string ClientId;
    public readonly static string ClientSecret;

    public static Dictionary<string, string> GetConfig()
    {
        return new Dictionary<string, string>
        {
            { "mode", Configuration["PayPal:Mode"] },
            { "connectionTimeout", Configuration["PayPal:ConnectionTimeout"] },
            { "requestRetries", Configuration["PayPal:RequestRetries"] }
        };
    }

    private static string GetAccessToken()
    {
        return new OAuthTokenCredential(ClientId, ClientSecret, GetConfig()).GetAccessToken();
    }

    public static APIContext GetAPIContext()
    {
        var accessToken = GetAccessToken();
        Console.WriteLine("PayPal Access Token: " + accessToken); // Or use a logger

        APIContext apiContext = new APIContext(accessToken);
        apiContext.Config = GetConfig();

        // Log API context details (be careful not to log sensitive data)
        Console.WriteLine("API Context Configuration:");
        foreach (var kvp in apiContext.Config)
        {
            Console.WriteLine($"{kvp.Key}: {kvp.Value}");
        }

        return apiContext;
    }
}
