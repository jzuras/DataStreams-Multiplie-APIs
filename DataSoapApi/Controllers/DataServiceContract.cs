using Microsoft.AspNetCore.Mvc;
using System.ServiceModel;

using Stream = DataStreams.DataModel.Models.Stream;

namespace DataStreams.DataSoapApi.Controllers;

[ServiceContract]
public interface IDataService
{
    [OperationContract]
    int GetValue([FromBody] int id);

    [OperationContract]
    int AddStream([FromBody] Stream stream);
}

public class DataService : IDataService
{
    private static Dictionary<int, Stream> Streams { get; } = new Dictionary<int, Stream>();
    private static int NextIDValue { get; set; } = 0;

    public int GetValue(int id)
    {
        if (Streams.ContainsKey(id))
        {
            Stream Stream = Streams[id];

            Random random = new Random();
            int randomNumber = random.Next(Stream.MinValue!.Value, Stream.MaxValue!.Value + 1);
            return randomNumber;
        }
        else
        {
            return 0;
        }
    }

    public int AddStream(Stream stream)
    {
        if (stream == null)
        {
            return 0;
        }
        else
        {
            if(string.IsNullOrEmpty(stream.Name))
            {
                return 0;
            }
            else
            {
                KeyValuePair<int, Stream> foundEntry = Streams.FirstOrDefault(entry => entry.Value.Name == stream.Name);

                if (!foundEntry.Equals(default(KeyValuePair<int, Stream>)))
                {
                    int foundId = foundEntry.Key;

                    // Min/Max may have changed.
                    if (foundEntry.Value.MinValue != stream.MinValue ||
                       foundEntry.Value.MaxValue != stream.MaxValue)
                    {
                        Streams[foundId] = stream;
                    }

                    return foundId;
                }

                if (stream.MinValue == null || stream.MaxValue == null)
                {
                    return 0;
                }

                Streams.Add(++NextIDValue, stream);
                return NextIDValue;
            }
        }
    }
}

