using SoapCore;
using DataStreams.DataSoapApi.Controllers;

namespace DataStreams.DataSoapApi;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddControllers();

        builder.Services.AddSingleton<IDataService, DataService>();

        var app = builder.Build();

        app.UseHttpsRedirection();

        app.UseSoapEndpoint<IDataService>("/Service.asmx", new SoapEncoderOptions());

        app.UseAuthorization();

        app.MapControllers();

        app.Run();
    }
}
