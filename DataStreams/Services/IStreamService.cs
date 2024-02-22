using Stream = DataStreams.DataModel.Models.Stream;

namespace DataStreams.DataStreams.Services;

public interface IStreamService
{
    public Task<IEnumerable<Stream>> GetStreamListAsync();
    public Task<Stream> GetStreamAsync(string name);
    public Task<bool> CreateStreamAsync(Stream stream);
    public Task<bool> EditStreamAsync(string name, Stream stream);
    public Task<bool> DeleteStreamAsync(string name);

}
