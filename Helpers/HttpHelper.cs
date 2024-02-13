using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Navigation;

namespace Debricked.Helpers
{
    internal static class HttpHelper
    {
        internal static async Task<string> MakeGetRequestAsync(HttpClient client, string endpoint)
        {
            var response = await client.GetAsync(endpoint);
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadAsStringAsync();
            }
            else
            {
                var we = new WebException(string.Format("Endpoint: '{0}', Response: '{1}", endpoint, response.ToString()), WebExceptionStatus.UnknownError);
                we.Data.Add("responseStatusCode", response.StatusCode);
                throw we;
            }
        }

        internal static async Task<string> MakeDeleteRequestAsync(HttpClient client, string endpoint)
        {
            var response = await client.DeleteAsync(endpoint);
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadAsStringAsync();
            }
            else
            {
                throw new Exception(string.Format("Endpoint: '{0}', Response: '{1}", endpoint, response.ToString()));
            }
        }

        internal static async Task<string> MakePutRequestAsync(HttpClient client, string endpoint, StringContent content)
        {
            var response = await client.PutAsync(endpoint, content);
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadAsStringAsync();
            }
            else
            {
                throw new Exception(string.Format("Endpoint: '{0}', Response: '{1}", endpoint, response.ToString()));
            }
        }

        internal static async Task<string> MakePostRequestAsync(HttpClient client, string endpoint, ByteArrayContent content)
        {
            var response = await client.PostAsync(endpoint, content);
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadAsStringAsync();
            }
            else
            {
                throw new Exception(string.Format("Endpoint: '{0}', Response: '{1}", endpoint, response.ToString()));
            }
        }

        internal static async Task<string> MakePostRequestAsync(HttpClient client, string endpoint, MultipartFormDataContent content)
        {
            var response = await client.PostAsync(endpoint, content);
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadAsStringAsync();
            }
            else
            {
                throw new Exception(string.Format("Endpoint: '{0}', Response: '{1}", endpoint, response.ToString()));
            }
        }

        internal static async Task<T> MakeGetRequestAsync<T>(HttpClient client, string endpoint, Action<WebException> handleException) where T : new()
        {
            try
            {
                return ConversionHelper.JsonStringToClass<T>(await MakeGetRequestAsync(client, endpoint));
            }
            catch (WebException we)
            {
                handleException(we);
                return default(T);
            }
        }

        internal static async Task<T> MakePostRequestAsync<T>(HttpClient client, string endpoint, ByteArrayContent content) where T : new()
        {
            return ConversionHelper.JsonStringToClass<T>(await MakePostRequestAsync(client, endpoint, content));
        }

        internal static async Task<T> MakePostRequestAsync<T>(HttpClient client, string endpoint, MultipartFormDataContent content) where T : new()
        {
            return ConversionHelper.JsonStringToClass<T>(await MakePostRequestAsync(client, endpoint, content));
        }

        internal static async Task<T> MakeDeleteRequestAsync<T>(HttpClient client, string endpoint) where T : new()
        {
            return ConversionHelper.JsonStringToClass<T>(await MakeDeleteRequestAsync(client, endpoint));
        }

        public static HttpClientHandler GetHttpClientHandlerWithProxy(string proxyUrl, bool ignoreCertErrors)
        {
            var handler = new HttpClientHandler
            {
                Proxy = new WebProxy
                {
                    Address = new Uri(proxyUrl),
                    UseDefaultCredentials = true
                }
            };
            if(ignoreCertErrors)
            {
                handler.ServerCertificateCustomValidationCallback = (a, b, c, d) => { return true; };
            }
            return handler;
        }
    }
}
