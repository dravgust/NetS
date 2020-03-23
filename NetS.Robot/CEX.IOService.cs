using System;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace NetS.Robot
{
    public class CexIoService : IExchangeService
    {
        public async Task<T> GetTickerAsync<T>()
        {
            try
            {
                using (var httpClient = new HttpClient())
                {
                    var result = await httpClient.GetStringAsync("https://cex.io/api/ticker/BTC/USD")
                        .ConfigureAwait(false);

                    return (T)JsonConvert.DeserializeObject(result);
                }
            }
            catch (Exception)
            {

                throw;
            }
        }

        public void Dispose()
        {
            throw new System.NotImplementedException();
        }
    }
}