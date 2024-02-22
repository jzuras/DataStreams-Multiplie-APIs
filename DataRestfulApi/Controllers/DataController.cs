using Asp.Versioning;
using DataStreams.DataModel.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

using Stream = DataStreams.DataModel.Models.Stream;

namespace DataStreams.DataRestfulApi.Controllers;

[ApiVersion(1)]
[ApiVersion(2)]
[Route("/v{v:apiVersion}")]
[Route("/")]
[ApiController]
public class DataController : ControllerBase
{
    private static Dictionary<int, Stream> Streams { get; } = new Dictionary<int, Stream>();
    private static int NextIDValue { get; set; } = 0;

    public DataController() { }

    #region JSON methods
    // GET: /{id}
    [MapToApiVersion(1)]
    [MapToApiVersion(2)]
    [HttpGet("{id}")]
    [Produces("application/json")]
    public ActionResult<object> GetValue(int id)
    {
        if (Streams.ContainsKey(id))
        {
            Stream Stream = Streams[id];

            Random random = new Random();
            int randomNumber = random.Next(Stream.MinValue!.Value, Stream.MaxValue!.Value + 1);
            var returnValue = new ReturnedObject();
            var requestedVersion = HttpContext?.GetRequestedApiVersion()?.ToString();
            if (requestedVersion == null)
            {
                returnValue.Name =  "Version: default";
            }
            else
            {
                returnValue.Name =  "Version: " + requestedVersion;
            }
            returnValue.Value = randomNumber;
            return returnValue;
        }
        else
        {
            return NotFound();
        }
    }

    // GET: //RL/{id}
    [EnableRateLimiting("fixed")]
    [MapToApiVersion(1)]
    [MapToApiVersion(2)]
    [HttpGet("RL/{id}")]
    [Produces("application/json")]
    public ActionResult<object> GetValueRateLimited(int id)
    {
        return this.GetValue(id);
    }

        // POST: /
        // Adds a Stream with min and max values.
        // Returns ID to use for data retrieval.
        [MapToApiVersion(1)]
    [MapToApiVersion(2)]
    [HttpPost]
    [Produces("application/json")]
    [Consumes("application/json")]
    public ActionResult<int> AddStream(Stream stream)
    {
        KeyValuePair<int, Stream> foundEntry = Streams.FirstOrDefault(entry => entry.Value.Name == stream.Name);

        if (!foundEntry.Equals(default(KeyValuePair<int, Stream>)))
        {
            int foundId = foundEntry.Key;

            // Min/Max may have changed.
            if(foundEntry.Value.MinValue != stream.MinValue ||
               foundEntry.Value.MaxValue != stream.MaxValue)
            {
                Streams[foundId] = stream;
            }

            return foundId;
        }

        if (stream.MinValue == null || stream.MaxValue == null)
        {
            return NotFound();
        }

        Streams.Add(++NextIDValue, stream);
        return NextIDValue;
    }
    #endregion

    #region XML methods
    // GET: /XML/{id}
    [MapToApiVersion(1)]
    [MapToApiVersion(2)]
    [HttpGet("XML/{id}")]
    [Produces("application/xml")]
    public ActionResult<object> GetValueXml(int id)
    {
        if (Streams.ContainsKey(id))
        {
            Stream Stream = Streams[id];

            Random random = new Random();
            int randomNumber = random.Next(Stream.MinValue!.Value, Stream.MaxValue!.Value + 1);
            var returnValue = new ReturnedObject();
            var requestedVersion = HttpContext?.GetRequestedApiVersion()?.ToString();
            if (requestedVersion == null)
            {
                returnValue.Name = "Version: default";
            }
            else
            {
                returnValue.Name = "Version: " + requestedVersion;
            }
            returnValue.Value = randomNumber;
            return returnValue;
        }
        else
        {
            return NotFound();
        }
    }

    // POST: /
    // Adds a Stream with min and max values.
    // Returns ID to use for data retrieval.
    [MapToApiVersion(1)]
    [MapToApiVersion(2)]
    [HttpPost]
    [Produces("application/xml")]
    [Consumes("application/xml")]
    public ActionResult<int> AddStreamXml([FromBody] Stream stream)
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
            return NotFound();
        }

        Streams.Add(++NextIDValue, stream);
        return NextIDValue;
    }
    #endregion
}
