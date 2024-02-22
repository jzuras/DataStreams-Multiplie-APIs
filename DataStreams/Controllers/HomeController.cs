using Azure.Communication.Email;
using Azure;
using DataStreams.DataStreams.Models;
using DataStreams.DataStreams.Services;
using DataStreams.DataStreams.Shared;
using Microsoft.AspNetCore.Mvc;
using SendGrid.Helpers.Mail;
using SendGrid;
using System.Diagnostics;
using DataStreams.Shared;
using Microsoft.FeatureManagement;
using Microsoft.Extensions.Options;

using ApiType = DataStreams.DataStreams.Services.IDataService.ApiType;

namespace DataStreams.DataStreams.Controllers;

public class HomeController : Controller
{
    private ILogger<HomeController> Logger { get; init; }
    private IFeatureManager FeatureManager { get; init; }
    private Settings Settings { get; init; }
    private IStreamService StreamService { get; init; }
    private IDataService DataService { get; init; }

    public HomeController(ILogger<HomeController> logger, IFeatureManager featureManager, IOptionsSnapshot<Settings> options,
        IStreamService StreamService, IDataService dataService)
    {
        this.Logger = logger;
        this.StreamService = StreamService;
        this.DataService = dataService;
        this.Settings = options.Value;
        this.FeatureManager = featureManager;

        this.Logger.LogTraceExt("HomeController initialized.");
    }

    public async Task<IActionResult> Index()
    {
        var Streams = await this.StreamService.GetStreamListAsync();

        var viewModel = new StreamViewModel();
        viewModel.StreamList = Streams;

        var ipv4Address = HttpContext.Connection.RemoteIpAddress?.MapToIPv4().ToString();
        var referringUrl = HttpContext.Request.Headers.TryGetValue("Referer", out var refererValues)
            ? refererValues.ToString() : null;

        this.Logger.LogInformationExt($"Home Controller->Index() - Calling IPv4: {ipv4Address} referringURL: {referringUrl}.");

        await this.SendEmail("DataStreams App", $"Home Page accessed. Calling IPv4: {ipv4Address} referringURL: {referringUrl}.");

        return View(viewModel);
    }

    // AJAX call
    public async Task<IActionResult> GetUpdatedValue(int id, int apiType, int version)
    {
        var returnValue = await this.DataService.GetValueAsync(id, (ApiType)apiType, version);

        return Json(returnValue);
    }

    // AJAX call
    public async Task<IActionResult> CountUpdatesForThreeSeconds(int id, int apiType, int version)
    {
        // Count how many calls can be made in 3 seconds.

        var stopwatch = Stopwatch.StartNew();
        int callCount = 0;

        while (stopwatch.Elapsed.TotalSeconds < 3)
        {
            var returnValue = await this.DataService.GetValueAsync(id, (ApiType)apiType, version);
            callCount++;
        }

        return Json(callCount);
    }

    [HttpPost]
    public async Task<IActionResult> Create(StreamViewModel model)
    {
        DataModel.Models.Stream stream = new DataModel.Models.Stream
        {
            MaxValue = model.EmptyStream.MaxValue,
            MinValue = model.EmptyStream.MinValue,
            Name = model.EmptyStream.Name
        };

        var success = await this.StreamService.CreateStreamAsync(stream);
        if (success)
        {
           return RedirectToAction("Index");
        }
        else
        {
            return NoContent();
        }
    }

    [HttpPost]
    public async Task<IActionResult> Delete(string name)
    {
        var success = await this.StreamService.DeleteStreamAsync(name);
        if (success)
        {
            return RedirectToAction("Index");
        }
        else
        {
            return NoContent();
        }
    }

    [HttpPost]
    public async Task<IActionResult> Edit(DataModel.Models.Stream stream)
    {
        var success = await this.StreamService.EditStreamAsync(stream.Name, stream);
        if (success)
        {
            return RedirectToAction("Index");
        }
        else
        {
            return NoContent();
        }
    }

    [HttpGet]
    public async Task<IActionResult> Display(string name)
    {
        var apiType= this.DataService.GetApiType(name);
        var version= this.DataService.GetVersion(name);
        var Stream = await this.StreamService.GetStreamAsync(name);
        var id = await this.DataService.InitializeAsync(Stream);

        var viewModel = new StreamViewModel();
        viewModel.ApiType = (int)apiType;
        viewModel.Version = version;
        viewModel.IdAssignedByApi = id;
        viewModel.CurrentlySelectedStream = name;
        viewModel.IsStreamSelected = true;
        return PartialView("_Stream", viewModel);
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }

    #region Helper Method
    /// <summary>
    /// Uses SendGrid API and/or Azure Email Service to send an email using configuration data.
    /// </summary>
    /// <param name="subject">Subject of the email.</param>
    /// <param name="message">Body of the email.</param>
    private async Task SendEmail(string subject, string message)
    {
        if (await this.FeatureManager.IsEnabledAsync(FeatureFlags.SendEmails))
        {
            this.Logger.LogInformationExt("SendEmail() called, feature enabled.");

            if (this.Settings.Email.SendToAddress != null)
            {
                if (this.Settings.Email.UseAzureEmailService == "true")
                {
                    #region Azure Email Service
                    var emailConnectionString = this.Settings.Email.AzureEmailConnectionString;
                    var emailClient = new EmailClient(emailConnectionString);

                    try
                    {
                        EmailSendOperation emailSendOperation = await emailClient.SendAsync(
                            WaitUntil.Started,
                            senderAddress: "DoNotReply@zuras.com",
                            recipientAddress: this.Settings.Email.SendToAddress,
                            subject: subject,
                            htmlContent: message,
                            plainTextContent: message);
                        this.Logger.LogInformationExt($"Azure email sent.");
                    }
                    catch (Exception ex)
                    {
                        this.Logger.LogWarningExt(ex, "Exception attempting to send email with Azure.");
                    }
                    #endregion
                }

                if (this.Settings.Email.UseSendGridEmailService == "true")
                {
                    #region SendGrid Email Service
                    var apiKey = this.Settings.Email.SendGridKey;
                    var client = new SendGridClient(apiKey);
                    var msg = new SendGridMessage()
                    {
                        From = new SendGrid.Helpers.Mail.EmailAddress(this.Settings.Email.FromAddress, this.Settings.Email.FromName),
                        Subject = subject,
                        PlainTextContent = message
                    };
                    msg.AddTo(new SendGrid.Helpers.Mail.EmailAddress(this.Settings.Email.SendToAddress, this.Settings.Email.SendToName));

                    try
                    {
                        var response = await client.SendEmailAsync(msg);
                        this.Logger.LogInformationExt($"SendGrid email sent - status code: {response.StatusCode}.");
                    }
                    catch (Exception ex)
                    {
                        this.Logger.LogWarningExt(ex, "Exception attempting to send email with SendGrid.");
                    }
                    #endregion
                }
            }
        }
        else
        {
            this.Logger.LogInformationExt("SendEmail() called, feature disabled.");
        }
    }
    #endregion
}
