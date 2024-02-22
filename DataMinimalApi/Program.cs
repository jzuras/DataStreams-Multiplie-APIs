using DataStreams.DataModel.Models;

using StreamDictionary = System.Collections.Generic.Dictionary<int, DataStreams.DataModel.Models.Stream>;
using Stream = DataStreams.DataModel.Models.Stream;
using Asp.Versioning.Builder;
using Asp.Versioning;

namespace DataMinimalApi;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddAuthorization();

        builder.Services.AddSingleton<StreamDictionary>();

        builder.Services.AddApiVersioning(options =>
        {
            options.DefaultApiVersion = new ApiVersion(1);
            options.ReportApiVersions = true;
            options.AssumeDefaultVersionWhenUnspecified = true;
            options.ApiVersionReader = ApiVersionReader.Combine(
                new UrlSegmentApiVersionReader(),
                new HeaderApiVersionReader("X-Api-Version"));
        });

        var app = builder.Build();

        app.UseHttpsRedirection();

        app.UseAuthorization();

        ApiVersionSet apiVersionSet = app.NewApiVersionSet()
            .HasApiVersion(new ApiVersion(1))
            .HasApiVersion(new ApiVersion(2))
            .Build();

        app.MapGet("/v{version:apiVersion}/{id}", (int id, StreamDictionary streams, HttpContext context) =>
        {
            if (streams.ContainsKey(id))
            {
                Stream Stream = streams[id];

                Random random = new Random();
                int randomNumber = random.Next(Stream.MinValue!.Value, Stream.MaxValue!.Value + 1);
                var returnValue = new ReturnedObject();

                if (context.Request.RouteValues.TryGetValue("version", out var version))
                {
                    var requestedVersion = version!.ToString();
                    returnValue.Name = "Version: " + requestedVersion;
                }
                else
                {
                    returnValue.Name = "Version: default";
                }
                returnValue.Value = randomNumber;
                return Results.Ok(returnValue);
            }
            else
            {
                return Results.NotFound();
            }
        })
        .WithApiVersionSet(apiVersionSet)
        .MapToApiVersion(1)
        .MapToApiVersion(2);


        app.MapPost("/v{version:apiVersion}", (Stream Stream, StreamDictionary streams) =>
        {
            KeyValuePair<int, Stream> foundEntry = streams.FirstOrDefault(entry => entry.Value.Name == Stream.Name);

            if (!foundEntry.Equals(default(KeyValuePair<int, Stream>)))
            {
                int foundId = foundEntry.Key;

                // Min/Max may have changed.
                if (foundEntry.Value.MinValue != Stream.MinValue ||
                   foundEntry.Value.MaxValue != Stream.MaxValue)
                {
                    streams[foundId] = Stream;
                }

                return Results.Ok(foundId);
            }

            if (Stream.MinValue == null || Stream.MaxValue == null)
            {
                return Results.NotFound();
            }

            int nextIDValue = streams.Count;

            streams.Add(++nextIDValue, Stream);
            return Results.Ok(nextIDValue);
        })
        .WithApiVersionSet(apiVersionSet)
        .IsApiVersionNeutral();

        app.Run();
    }
}
