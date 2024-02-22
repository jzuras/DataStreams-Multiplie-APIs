using DataStreams.DataModel.Models;
using Microsoft.EntityFrameworkCore;

using Stream = DataStreams.DataModel.Models.Stream;

namespace DataStreams.StreamApi;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddControllers();

        builder.Services.AddDbContext<StreamContext>(options =>
            options.UseInMemoryDatabase("StreamDatabase"));

        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        var app = builder.Build();

        using (var scope = app.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<StreamContext>();

            dbContext.Streams.Add(new Stream { Name = "RestAPI (default version)", MinValue = 12, MaxValue = 50 });
            dbContext.Streams.Add(new Stream { Name = "RestAPI-v1", MinValue = 22, MaxValue = 55 });
            dbContext.Streams.Add(new Stream { Name = "RestAPI-v2", MinValue = 32, MaxValue = 60 });
            dbContext.Streams.Add(new Stream { Name = "MinimalAPI (version 1)", MinValue = 20, MaxValue = 30 });
            dbContext.Streams.Add(new Stream { Name = "MinimalAPI-v2", MinValue = 30, MaxValue = 42 });
            dbContext.Streams.Add(new Stream { Name = "RestXML (default version)", MinValue = 25, MaxValue = 45 });
            dbContext.Streams.Add(new Stream { Name = "RestXML-v1", MinValue = 35, MaxValue = 60 });
            dbContext.Streams.Add(new Stream { Name = "RestXML-v2", MinValue = 45, MaxValue = 65 });
            dbContext.Streams.Add(new Stream { Name = "SoapAPI", MinValue = 20, MaxValue = 80 });
            dbContext.Streams.Add(new Stream { Name = "Rate-Limited", MinValue = 15, MaxValue = 40 });

            dbContext.SaveChanges();
        }

        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseHttpsRedirection();

        app.UseAuthorization();

        app.MapControllers();

        app.Run();
    }
}
