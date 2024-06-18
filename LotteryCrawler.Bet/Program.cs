// See https://aka.ms/new-console-template for more information
using LotteryCrawler.Bet.Crawlers;
using LotteryCrawler.Bet.Strategies;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Http;
using Polly;
using System.Configuration;
using System.Linq;
using System.Text.Json;

namespace LotteryCrawler.Bet
{
    public class Progam
    {
        private static IHost SetupHost()
        {
            var timeoutPolicy = Policy.TimeoutAsync<HttpResponseMessage>(TimeSpan.FromSeconds(10));
            return new HostBuilder()
                .ConfigureAppConfiguration(c => c.AddJsonFile("appsettings.json", false, true))
                .ConfigureServices((c,s) =>
                {
                    s.AddHttpClient<LotteryService<ApostaDTO>, MegaSena>
                    (
                        c => c.BaseAddress = new Uri("https://servicebus2.caixa.gov.br/portaldeloterias/api/megasena")
                    ).AddTransientHttpErrorPolicy(p => p.WaitAndRetryAsync(3, _ => TimeSpan.FromSeconds(5), (response, a, currentTry, ctx) =>
                    {
                        Console.WriteLine($"Failed - {response.Result.StatusCode}. Current try: {currentTry}");
                    }))
                    .AddPolicyHandler(request =>
                    {
                        if (request.Method == HttpMethod.Get)
                            return timeoutPolicy;

                        return Policy.NoOpAsync<HttpResponseMessage>();
                    });

                    s.Configure<ConfigVisualizationOptions>(c.Configuration.GetSection("ConfigVisualizationOptions"));                    
                    s.AddTransient<App>();
                })
                .Build();
        }

        static void Main(string[] args)
        {
            var host = SetupHost();
            var myApp = host.Services.GetRequiredService<App>();
            myApp.Run(args);
        }
    }
}