using System.Net.Http.Headers;
using System.Runtime.Serialization;
using System.Text;
using System.Xml;
using System.Xml.Linq;

using Stream = DataStreams.DataModel.Models.Stream;
using ApiType = DataStreams.DataStreams.Services.IDataService.ApiType;

namespace DataStreams.DataStreams.Services;

public class DataService : IDataService
{
    
    private class ApiInfo
    {
        public string BaseAddress { get; set; }
        public ApiType ApiType { get; set; }

        public ApiInfo(string baseAddress, ApiType apiType)
        {
            this.BaseAddress = baseAddress;
            this.ApiType = apiType;
        }
    }

    private HttpClient Client { get; init; }
    private string SavedBaseAddress { get; set; }
    private ApiType SavedApiType { get; set; }

    private readonly ApiInfo[] BasePaths =
    [
        // Note - array order must match ApiType Enum order
        new ApiInfo("", ApiType.Unset),
        new ApiInfo("https://datastreamsapp.azurewebsites.net/DataRestfulApi", ApiType.Default), // (default version)
        new ApiInfo("https://datastreamsapp.azurewebsites.net/DataMinimalApi", ApiType.Minimal),  // (version 1)
        new ApiInfo("https://datastreamsapp.azurewebsites.net/DataRestfulApi", ApiType.Xml), // Rest using XML (default version)
        new ApiInfo("https://datastreamsapp.azurewebsites.net/DataSoapApi/Service.asmx", ApiType.Soap), // (not versioned)
        new ApiInfo("https://datastreamsapp.azurewebsites.net/DataRestfulApi", ApiType.RateLimited) // (same as Rest) (not versioned)
    ];

    public DataService(HttpClient client)
    {
        this.Client = client ?? throw new ArgumentNullException(nameof(client));
        this.SavedBaseAddress = string.Empty;
        this.SavedApiType = ApiType.Unset;
    }

    public async Task<int> InitializeAsync(Stream stream)
    {
        var apiType = this.GetApiType(stream.Name);
        var basePath = this.GetBaseAddress((int)apiType, this.GetVersion(stream.Name));
        if (string.IsNullOrEmpty(basePath))
        {
            return 0;
        }

        if (apiType == ApiType.Xml)
        {
            var xmlContent = new StringContent(stream.ToXml(), Encoding.UTF8, "application/xml");
            var response = await this.Client.PostAsync(basePath, xmlContent);
            response.EnsureSuccessStatusCode();

            return await response.ReadXmlContentAsync<int>();
        }
        else if (apiType == ApiType.Soap)
        {
            string soapEnvelope = this.StreamParameterClassSoapEnvelope(stream, "AddStream", "stream");

            var content = new StringContent(soapEnvelope, Encoding.UTF8, "text/xml");

            var response = await this.Client.PostAsync(basePath, content);
            response.EnsureSuccessStatusCode();

            var soapResult = await response.Content.ReadAsStringAsync();

            return this.ParseIntFromSoapResponse(soapResult, "AddStream", "AddStreamResult");
        }
        else
        {
            var jsonContent = JsonContent.Create(stream);
            var response = await this.Client.PostAsync(basePath, jsonContent);
            response.EnsureSuccessStatusCode();

            return await response.ReadJsonContentAsync<int>();
        }
    }

    public async Task<string> GetValueAsync(int id, ApiType apiType, int version)
    {
        // This may be called in a loop so save and re-use address and call type.
        if (string.IsNullOrEmpty(this.SavedBaseAddress))
        {
            this.SavedBaseAddress = this.GetBaseAddress((int)apiType, version);

            if (string.IsNullOrEmpty(this.SavedBaseAddress))
            {
                return "API Type Not Found.";
            }
        }
        HttpResponseMessage response;

        if (apiType == ApiType.Xml)
        {
            string requestUrl = $"{this.SavedBaseAddress}/XML/{id}";

            var request = new HttpRequestMessage(HttpMethod.Get, requestUrl);
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/xml"));

            response = await this.Client.SendAsync(request);
        }
        else if (apiType == ApiType.Soap)
        {
            string soapEnvelope = this.IntegerParameterSoapEnvelope(id, "GetValue", "id");

            var content = new StringContent(soapEnvelope, Encoding.UTF8, "text/xml");

            response = await this.Client.PostAsync(this.SavedBaseAddress, content);
        }
        else
        {
            string requestUrl;
            if(apiType == ApiType.RateLimited)
            {
                requestUrl = $"{this.SavedBaseAddress}/RL/{id}";
            }
            else
            {
                requestUrl = $"{this.SavedBaseAddress}/{id}";
            }

            var request = new HttpRequestMessage(HttpMethod.Get, requestUrl);
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            response = await this.Client.SendAsync(request);
        }

        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            return content;
        }
        else
        {
            return $"Invalid response status code: {response.StatusCode}";
        }
    }

    public ApiType GetApiType(string streamName)
    {
        // Api Types are chosen based on the name of the Stream.
        // (Must be kept in sync with BaseAddresses array.)
        if (streamName.ToLower().Contains("minimal")) return ApiType.Minimal;
        if (streamName.ToLower().Contains("xml")) return ApiType.Xml;
        if (streamName.ToLower().Contains("soap")) return ApiType.Soap;
        if (streamName.ToLower().Contains("rate")) return ApiType.RateLimited;

        return ApiType.Default;
    }

    public int GetVersion(string streamName)
    {
        // Versions are determined by -v1 or -v2 in the name.
        if (streamName.ToLower().Contains("-v1")) return 1;
        if (streamName.ToLower().Contains("-v2")) return 2;

        // Minimal API does not handle a default version.
        if (streamName.ToLower().Contains("minimal"))
        {
            return 1;
        }

        return 0;
    }

    #region Helper Methods
    private string GetBaseAddress(int apiType, int version)
    {
        if (apiType < this.BasePaths.Length)
        {
            var baseAddress = this.BasePaths[apiType].BaseAddress;
            if (version > 0)
            {
                baseAddress += "/v" + version;
            }

            return baseAddress;
        }

        return string.Empty;
    }
    #endregion

    #region SOAP Methods
    private int ParseIntFromSoapResponse(string soapResponse, string methodName, string parameterName)
    {
        XDocument xdoc = XDocument.Parse(soapResponse);

        XNamespace soapNamespace = "http://schemas.xmlsoap.org/soap/envelope/";
        XNamespace tempuriNamespace = "http://tempuri.org/";

        var resultElement = xdoc.Descendants(soapNamespace + "Body")
                               .Elements(tempuriNamespace + methodName + "Response")
                               .Elements(tempuriNamespace + parameterName)
                               .FirstOrDefault();

        if (resultElement != null && int.TryParse(resultElement.Value, out int result))
        {
            return result;
        }

        return 0; // 0 is never returned so this will indicate an error.
    }

    private string IntegerParameterSoapEnvelope(int input, string methodName, string parameterName)
    {
        string xmlContent = $"<{parameterName}>{input}</{parameterName}>";
        return CreateCompleteSoapEnvelope(CreateSoapBody(xmlContent, methodName));
    }

    private string StreamParameterClassSoapEnvelope(Stream stream, string methodName, string parameterName)
    {
        var serializer = new DataContractSerializer(typeof(Stream));

        using (var writer = new StringWriter())
        {
            using (var xmlWriter = XmlWriter.Create(writer, new XmlWriterSettings { OmitXmlDeclaration = true }))
            {
                serializer.WriteObject(xmlWriter, stream);
            }
            string xmlContent = writer.ToString();
            xmlContent = ReplaceClassNameWithParameterName(xmlContent, "Stream", parameterName);

            return CreateCompleteSoapEnvelope(CreateSoapBody(xmlContent, methodName));
        }
    }

    private string CreateCompleteSoapEnvelope(string soapBody)
    {
        return $@"<?xml version=""1.0"" encoding=""utf-8""?>
            <soap:Envelope xmlns:soap=""http://schemas.xmlsoap.org/soap/envelope/""
                           xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance""
                           xmlns:xsd=""http://www.w3.org/2001/XMLSchema"">
                <soap:Body>
                    {soapBody}
                </soap:Body>
            </soap:Envelope>".Trim();
    }

    private string ReplaceClassNameWithParameterName(string xmlContent, string className, string parameterName)
    {
        string startTag = $"<{className}";
        string endTag = $"</{className}>";

        if (xmlContent.StartsWith(startTag) && xmlContent.EndsWith(endTag))
        {
            xmlContent = xmlContent.Substring(startTag.Length, xmlContent.Length - startTag.Length - endTag.Length);
            xmlContent = $"<{parameterName}{xmlContent}</{parameterName}>";
        }

        return xmlContent;
    }

    private string CreateSoapBody(string xmlContent, string methodName)
    {
        return $@"<{methodName} xmlns=""http://tempuri.org/"">
                 {xmlContent}
                 </{methodName}>";
    }
    #endregion
}
