using System;
using System.Net.Http;
using System.Threading.Tasks;

public class LabelApiService
{
    private static readonly HttpClient client = new HttpClient();

    public async Task<string> GetLabelDataAsync(string partnumber)
    {
        string url = $"http://192.168.1.1:9090/api/labels?partnumber={partnumber}";

        try
        {
            HttpResponseMessage response = await client.GetAsync(url);
            response.EnsureSuccessStatusCode(); // Lanza excepción si el código de estado no es 2xx

            string responseBody = await response.Content.ReadAsStringAsync();
            return responseBody;
        }
        catch (HttpRequestException ex)
        {
            // Manejo del error (puedes devolver null, throw, o un mensaje)
            return $"Error: {ex.Message}";
        }
    }
}
