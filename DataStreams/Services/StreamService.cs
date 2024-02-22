using System.Text;
using System.Text.Json;

using Stream = DataStreams.DataModel.Models.Stream;

namespace DataStreams.DataStreams.Services;

public class StreamService : IStreamService
{
    private HttpClient Client { get; init; }

    private string BasePath = "https://datastreamsapp.azurewebsites.net/Streamapi"; 

    public StreamService(HttpClient client)
    {
        this.Client = client ?? throw new ArgumentNullException(nameof(client));
    }

    public async Task<IEnumerable<Stream>> GetStreamListAsync()
    {
        var response = await this.Client.GetAsync(this.BasePath);

        return await response.ReadJsonContentAsync<List<Stream>>();
    }

    public async Task<Stream> GetStreamAsync(string name)
    {
        var response = await this.Client.GetAsync(this.BasePath + "/" + name);

        return await response.ReadJsonContentAsync<Stream>();
    }

    public async Task<bool> CreateStreamAsync(Stream stream)
    {
        string jsonString = JsonSerializer.Serialize(stream);
        var content = new StringContent(jsonString, Encoding.UTF8, "application/json");
        var response = await this.Client.PostAsync(BasePath, content);

        return response.IsSuccessStatusCode;
    }

    public async Task<bool> EditStreamAsync(string name, Stream stream)
    {
        string jsonString = JsonSerializer.Serialize(stream);
        var content = new StringContent(jsonString, Encoding.UTF8, "application/json");
        var response = await this.Client.PutAsync(BasePath + "/" + name, content);

        return response.IsSuccessStatusCode;
    }

    public async Task<bool> DeleteStreamAsync(string name)
    {
        var response = await this.Client.DeleteAsync(BasePath + "/" + name);

        return response.IsSuccessStatusCode;
    }
}
