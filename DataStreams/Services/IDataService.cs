using Stream = DataStreams.DataModel.Models.Stream;

namespace DataStreams.DataStreams.Services;

public interface IDataService
{ 
    public enum ApiType
    {
        Unset = 0, Default, Minimal, Xml, Soap, RateLimited
    };

    public Task<int> InitializeAsync(Stream Stream);
    public Task<string> GetValueAsync(int id, ApiType apiType, int version);
    public ApiType GetApiType(string StreamName);
    public int GetVersion(string StreamName);
}
