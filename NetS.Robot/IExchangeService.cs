using System;
using System.Threading.Tasks;

namespace NetS.Robot
{
    public interface IExchangeService : IDisposable
    {
        Task<T> GetTickerAsync<T>();
    }
}