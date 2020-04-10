using Polly;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace lalala.Polly.Playground
{
    class Program
    {
        static async Task Main(string[] args)
        {

            Console.WriteLine("Demo is running!");
            await RetryForeverPolicy();
        }
        static async Task CallApi()
        {
            Console.ResetColor();
            Console.WriteLine("Trying to get data");
            var httpClient = new HttpClient();
            HttpResponseMessage result = httpClient.GetAsync("https://iholderapi.azurewebsites.net/api/v1/Tipo_investimento").Result;
            if (result.StatusCode != System.Net.HttpStatusCode.OK)
            {
                var msgerro = "An error has occurred";
                Console.BackgroundColor = ConsoleColor.Red;
                Console.WriteLine(msgerro);
                throw new Exception(msgerro);
            }
            Console.BackgroundColor = ConsoleColor.Green;
            Console.WriteLine("Success in getting data");
            await Task.CompletedTask;

        }

        static async Task RetryForeverPolicy()
        {
            var retry = Policy.Handle<Exception>().RetryForeverAsync();
            await retry.ExecuteAsync(() => CallApi());
        }

    }
}
