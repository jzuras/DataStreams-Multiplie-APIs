using System.Text.Json;
using System.Xml.Serialization;

namespace DataStreams.DataStreams;

public static class HttpClientExtensions
{
    public static async Task<T> ReadJsonContentAsync<T>(this HttpResponseMessage response)
    {
        if (response.IsSuccessStatusCode == false)
            throw new ApplicationException($"Something went wrong calling the API: {response.ReasonPhrase}");

        var dataAsString = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

        return JsonSerializer.Deserialize<T>(
            dataAsString, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            })!;
    }

    public static async Task<T> ReadXmlContentAsync<T>(this HttpResponseMessage response)
    {
        if (response.IsSuccessStatusCode == false)
            throw new ApplicationException($"Something went wrong calling the API: {response.ReasonPhrase}");

        var dataAsString = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

        var xmlSerializer = new XmlSerializer(typeof(T));
        using (var stringReader = new StringReader(dataAsString))
        {
            return (T)xmlSerializer.Deserialize(stringReader)!;
        }
    }
}
