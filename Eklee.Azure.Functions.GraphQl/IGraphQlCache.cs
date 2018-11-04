using System;
using System.Threading.Tasks;

namespace Eklee.Azure.Functions.GraphQl
{
    public interface IGraphQlCache
    {
        Task<T> GetByKeyAsync<T>(Func<object, T> getResult, object key, int cacheDurationInSeconds);
    }
}
