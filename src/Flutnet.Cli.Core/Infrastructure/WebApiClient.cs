using System;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;
using Flutnet.Cli.Core.Utilities;
using FlutnetSite.WebApi.DTO;
using Newtonsoft.Json;
using RestSharp;
using RestSharp.Serialization;

namespace Flutnet.Cli.Core.Infrastructure
{
    internal static class WebApiClient
    {
        public static readonly Uri BaseUri = new Uri("https://www.flutnet.com/rest-api/");
        public static readonly Uri VersionBaseUri = new Uri(BaseUri, "version");

        public static CheckForUpdatesResponse CheckForUpdates(string sdkTableRevision)
        {
            CheckForUpdatesRequest dto = new CheckForUpdatesRequest
            {
                CurrentVersion = Assembly.GetEntryAssembly().GetProductVersion(),
                OS = Utilities.OperatingSystem.IsMacOS() ? 1 : 0,
                Is64BitOS = Utilities.OperatingSystem.Is64Bit,
                SdkTableRevision = sdkTableRevision
            };
    
            RestClient client = new RestClient(VersionBaseUri);

            RestRequest request = new RestRequest("check-updates", Method.POST);
            request.AddJsonBody(dto);

            WebApiAdvancedResponse<CheckForUpdatesResponse, ApiError> response = MakeRequest<CheckForUpdatesResponse, ApiError>(client, request);
            ThrowIfNotSuccessful(response);
            return response.Content;
        }

        private static void ThrowIfNotSuccessful(WebApiResponse response)
        {
            if (response.Successful)
                return;

            if (response.ErrorException != null)
                throw new CommandLineException(CommandLineErrorCode.WebApiUnreachable, response.ErrorException);

            if (response is IResponseWithErrorData<ApiError> r && r.ErrorContent != null)
                throw new CommandLineException(CommandLineErrorCode.WebApiCallFriendlyError, r.ErrorContent.Message);

            throw new CommandLineException(
                CommandLineErrorCode.WebApiCallGenericError, 
                string.Format(CommandLineErrorCode.WebApiCallGenericError.GetDefaultMessage(), response.ErrorStatusCode));
        }

#region Core methods

        private static WebApiRawResponse Download(RestClient client, RestRequest request)
        {
            IRestResponse response = client.Execute(request);
            return WrapRawRestResponse(client, request, response);
        }

        private static WebApiAdvancedRawResponse<TErrorContent> Download<TErrorContent>(RestClient client, RestRequest request)
        {
            IRestResponse response = client.Execute(request);
            return WrapAdvancedRawRestResponse<TErrorContent>(client, request, response);
        }

        private static async Task<WebApiRawResponse> DownloadAsync(RestClient client, RestRequest request)
        {
            IRestResponse response = await client.ExecuteAsync(request);
            return WrapRawRestResponse(client, request, response);
        }

        private static async Task<WebApiAdvancedRawResponse<TErrorContent>> DownloadAsync<TErrorContent>(RestClient client, RestRequest request)
        {
            IRestResponse response = await client.ExecuteAsync(request);
            return WrapAdvancedRawRestResponse<TErrorContent>(client, request, response);
        }

        private static WebApiResponse<TContent> MakeRequest<TContent>(RestClient client, RestRequest request)
        {
            IRestResponse response = client.Execute(request);
            return WrapRestResponse<TContent>(client, request, response);
        }

        private static WebApiAdvancedResponse<TContent, TErrorContent> MakeRequest<TContent, TErrorContent>(RestClient client, RestRequest request)
        {
            IRestResponse response = client.Execute(request);
            return WrapAdvancedRestResponse<TContent, TErrorContent>(client, request, response);
        }

        private static async Task<WebApiResponse<TContent>> MakeRequestAsync<TContent>(RestClient client, RestRequest request)
        {
            IRestResponse response = await client.ExecuteAsync(request);
            return WrapRestResponse<TContent>(client, request, response);
        }

        private static async Task<WebApiAdvancedResponse<TContent, TErrorContent>> MakeRequestAsync<TContent, TErrorContent>(RestClient client, RestRequest request)
        {
            IRestResponse response = await client.ExecuteAsync(request);
            return WrapAdvancedRestResponse<TContent, TErrorContent>(client, request, response);
        }

        private static WebApiResponse MakeCall(RestClient client, RestRequest request)
        {
            IRestResponse response = client.Execute(request);
            return WrapRestResponse(client, request, response);
        }

        private static WebApiAdvancedResponse<TErrorContent> MakeCall<TErrorContent>(RestClient client, RestRequest request)
        {
            IRestResponse response = client.Execute(request);
            return WrapAdvancedRestResponse<TErrorContent>(client, request, response);
        }

        private static async Task<WebApiResponse> MakeCallAsync(RestClient client, RestRequest request)
        {
            IRestResponse response = await client.ExecuteAsync(request);
            return WrapRestResponse(client, request, response);
        }

        private static async Task<WebApiAdvancedResponse<TErrorContent>> MakeCallAsync<TErrorContent>(RestClient client, RestRequest request)
        {
            IRestResponse response = await client.ExecuteAsync(request);
            return WrapAdvancedRestResponse<TErrorContent>(client, request, response);
        }

        private static WebApiResponse WrapRestResponse(RestClient client, RestRequest request, IRestResponse response)
        {
            WebApiResponse result = new WebApiResponse();

            Uri requestUri = client.BuildUri(request);
            if (response.IsSuccessful)
            {
                result.Successful = true;
            }
            else if (response.ErrorException != null)
            {
                Log.Error("Network/framework exception on REST request '{0}'", requestUri);
                Log.Ex(response.ErrorException);
                result.ErrorException = response.ErrorException;
            }
            else
            {
                Log.Error("Error {1} ({2}) on REST request '{0}'", requestUri, (int) response.StatusCode, response.StatusDescription);
                result.ErrorStatusCode = response.StatusCode;
            }
            return result;
        }

        private static WebApiResponse<TContent> WrapRestResponse<TContent>(RestClient client, RestRequest request, IRestResponse response)
        {
            WebApiResponse<TContent> result = new WebApiResponse<TContent>();

            Uri requestUri = client.BuildUri(request);
            if (response.IsSuccessful)
            {
                result.Successful = true;
                result.Content = ParseRestResponse<TContent>(client, response);
            }
            else if (response.ErrorException != null)
            {
                Log.Error("Network/framework exception on REST request '{0}'", requestUri);
                Log.Ex(response.ErrorException);
                result.ErrorException = response.ErrorException;
            }
            else
            {
                Log.Error("Error {1} ({2}) on REST request '{0}'", requestUri, (int) response.StatusCode, response.StatusDescription);
                result.ErrorStatusCode = response.StatusCode;
            }
            return result;
        }

        private static WebApiRawResponse WrapRawRestResponse(RestClient client, RestRequest request, IRestResponse response)
        {
            WebApiRawResponse result = new WebApiRawResponse();

            Uri requestUri = client.BuildUri(request);
            if (response.IsSuccessful)
            {
                result.Successful = true;
                result.Content = response.RawBytes;
            }
            else if (response.ErrorException != null)
            {
                Log.Error("Network/framework exception on REST request '{0}'", requestUri);
                Log.Ex(response.ErrorException);
                result.ErrorException = response.ErrorException;
            }
            else
            {
                Log.Error("Error {1} ({2}) on REST request '{0}'", requestUri, (int)response.StatusCode, response.StatusDescription);
                result.ErrorStatusCode = response.StatusCode;
            }
            return result;
        }

        private static WebApiAdvancedResponse<TErrorContent> WrapAdvancedRestResponse<TErrorContent>(RestClient client, RestRequest request, IRestResponse response)
        {
            WebApiAdvancedResponse<TErrorContent> result = new WebApiAdvancedResponse<TErrorContent>();

            Uri requestUri = client.BuildUri(request);
            if (response.IsSuccessful)
            {
                result.Successful = true;
            }
            else if (response.ErrorException != null)
            {
                Log.Error("Network/framework exception on REST request '{0}'", requestUri);
                Log.Ex(response.ErrorException);
                result.ErrorException = response.ErrorException;
            }
            else
            {
                Log.Error("Error {1} ({2}) on REST request '{0}'", requestUri, (int) response.StatusCode, response.StatusDescription);
                result.ErrorStatusCode = response.StatusCode;
                result.ErrorContent = ParseRestResponse<TErrorContent>(client, response);
            }
            return result;
        }

        private static WebApiAdvancedResponse<TContent, TErrorContent> WrapAdvancedRestResponse<TContent, TErrorContent>(RestClient client, RestRequest request, IRestResponse response)
        {
            WebApiAdvancedResponse<TContent, TErrorContent> result = new WebApiAdvancedResponse<TContent, TErrorContent>();

            Uri requestUri = client.BuildUri(request);
            if (response.IsSuccessful)
            {
                result.Successful = true;
                result.Content = ParseRestResponse<TContent>(client, response);
            }
            else if (response.ErrorException != null)
            {
                Log.Error("Network/framework exception on REST request '{0}'", requestUri);
                Log.Ex(response.ErrorException);
                result.ErrorException = response.ErrorException;
            }
            else
            {
                Log.Error("Error {1} ({2}) on REST request '{0}'", requestUri, (int) response.StatusCode, response.StatusDescription);
                result.ErrorStatusCode = response.StatusCode;
                result.ErrorContent = ParseRestResponse<TErrorContent>(client, response);
            }
            return result;
        }

        private static WebApiAdvancedRawResponse<TErrorContent> WrapAdvancedRawRestResponse<TErrorContent>(RestClient client, RestRequest request, IRestResponse response)
        {
            WebApiAdvancedRawResponse<TErrorContent> result = new WebApiAdvancedRawResponse<TErrorContent>();

            Uri requestUri = client.BuildUri(request);
            if (response.IsSuccessful)
            {
                result.Successful = true;
                result.Content = response.RawBytes;
            }
            else if (response.ErrorException != null)
            {
                Log.Error("Network/framework exception on REST request '{0}'", requestUri);
                Log.Ex(response.ErrorException);
                result.ErrorException = response.ErrorException;
            }
            else
            {
                Log.Error("Error {1} ({2}) on REST request '{0}'", requestUri, (int)response.StatusCode, response.StatusDescription);
                result.ErrorStatusCode = response.StatusCode;
                result.ErrorContent = ParseRestResponse<TErrorContent>(client, response);
            }
            return result;
        }

        private static T ParseRestResponse<T>(RestClient client, IRestResponse response)
        {
            Type type = typeof(T);
            if (type.IsEnum)
            {
                return (T) Enum.ToObject(type, int.Parse(response.Content));
            }
            else
            {
                IRestResponse<T> typedResponse = client.Deserialize<T>(response);
                return typedResponse.Data;
            }
        }

#endregion
    }

    internal class JsonNetSerializer : IRestSerializer
    {
        public string Serialize(object obj) =>
            JsonConvert.SerializeObject(obj);

        public string Serialize(Parameter parameter) =>
            JsonConvert.SerializeObject(parameter.Value);

        public T Deserialize<T>(IRestResponse response) =>
            JsonConvert.DeserializeObject<T>(response.Content);

        public string[] SupportedContentTypes { get; } =
        {
            "application/json", "text/json", "text/x-json", "text/javascript", "*+json"
        };

        public string ContentType { get; set; } = "application/json";

        public DataFormat DataFormat { get; } = DataFormat.Json;
    }

    /// <summary>
    /// Wrapper for the result of a RESTful API call.
    /// </summary>
    internal class WebApiResponse
    {
        public bool Successful { get; set; }
        public Exception ErrorException { get; set; }
        public HttpStatusCode ErrorStatusCode { get; set; }
    }

    /// <summary>
    /// Wrapper for the result of a RESTful API request.
    /// </summary>
    internal class WebApiResponse<T> : WebApiResponse, IResponseWithData<T>
    {
        public T Content { get; set; }
    }

    /// <summary>
    /// Wrapper for the result of a RESTful API request
    /// that returns raw bytes.
    /// </summary>
    internal class WebApiRawResponse : WebApiResponse<byte[]>
    {
    }

    /// <summary>
    /// Wrapper for the result of a RESTful API call in advanced scenarios
    /// where APIs can return an object inside the error response.
    /// </summary>
    internal class WebApiAdvancedResponse<TErrorContent> : WebApiResponse, IResponseWithErrorData<TErrorContent>
    {
        public TErrorContent ErrorContent { get; set; }
    }

    /// <summary>
    /// Wrapper for the result of a RESTful API request in advanced scenarios
    /// where APIs can return an object inside the error response.
    /// </summary>
    internal class WebApiAdvancedResponse<TContent, TErrorContent> : WebApiResponse<TContent>, IResponseWithErrorData<TErrorContent>
    {
        public TErrorContent ErrorContent { get; set; }
    }

    /// <summary>
    /// Wrapper for the result of a RESTful API request
    /// that returns raw bytes in advanced scenarios
    /// where APIs can return an object inside the error response.
    /// </summary>
    internal class WebApiAdvancedRawResponse<TErrorContent> : WebApiAdvancedResponse<byte[], TErrorContent>
    {
    }

    /// <summary>
    /// Denotes a RESTful API response containing serialized data.
    /// </summary>
    internal interface IResponseWithData<T>
    {
        T Content { get; set; }
    }

    /// <summary>
    /// Denotes an error RESTful API response containing serialized data.
    /// </summary>
    internal interface IResponseWithErrorData<T>
    {
        T ErrorContent { get; set; }
    }
}