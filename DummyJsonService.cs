using System.Net.Http.Json;
using BoutiqueEnLigne.DTOs;

namespace BoutiqueEnLigne.Services
{
    public class DummyJsonService
    {
        private readonly HttpClient _http;

        public DummyJsonService(HttpClient http)
        {
            _http = http;
        }

        public async Task<DummyProductsResponse?> GetProductsAsync(int limit = 30)
        {
            return await _http.GetFromJsonAsync<DummyProductsResponse>(
                $"https://dummyjson.com/products?limit={limit}");
        }
    }
}