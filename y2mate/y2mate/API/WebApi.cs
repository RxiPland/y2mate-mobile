using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.IO.Compression;
using System.IO;
using System.Web;
using Newtonsoft.Json.Linq;
using Plugin.Connectivity;
using y2mate.Config;
using System.Text.RegularExpressions;
using Newtonsoft.Json;

namespace y2mate.API
{
    class WebApi
    {

        public static async Task<JObject> SearchForVideo(string YoutubeUrl)
        {

            JObject result = new JObject
            {
                { "Success", null },
                { "ErrorMessage", null },
                { "ResponseHtml", null }
            };


            // Validate for complete URL
            if (!IsFullUrl(YoutubeUrl))
            {
                result["Success"] = false;
                result["ErrorMessage"] = Messages.FullUrlRequiredError;
                return result;
            }

            
            JObject PostData = new JObject
            {
                {"k_query", YoutubeUrl},
                {"k_page", "mp3"},
                {"hl", "en"},
                {"q_auto", 0}
            };
            

            string postData = JsonUrlEncode(PostData);

            JObject PostResult = await Task.Run(() => PostRequest(GlobalConfig.ApiSearchUrl, postData, "application/x-www-form-urlencoded"));



            string ErrorMessage = PostResult["ErrorMessage"]?.ToString() ?? string.Empty;
            bool Success = bool.Parse(PostResult["Success"]?.ToString() ?? "false");

            if ((!Success) || (ErrorMessage != string.Empty))
            {
                result["Success"] = false;
                result["ErrorMessage"] = ErrorMessage;
                return result;
            }


            // Everything OK
            result["Success"] = true;
            result["ErrorMessage"] = ErrorMessage ?? string.Empty;
            result["ResponseHtml"] = PostResult["Response"];
            return result;
        }



        public static async Task<JObject> PostRequest(string Url, string data, string contentType, CookieContainer cookieContainer = null)
        {
            JObject result = new JObject
            {
                { "Success", null },
                { "ErrorMessage", null },
                { "Response", null },
                { "Cookies", null }
            };

            if (!CrossConnectivity.Current.IsConnected)
            {
                // No Internet connection

                result["Success"] = false;
                result["ErrorMessage"] = Messages.NoInternetConnectionError;

                return result;

            }

            bool ServerAvailable = await IsServerReachable();
            if (!ServerAvailable)
            {
                // Server down

                result["Success"] = false;
                result["ErrorMessage"] = Messages.ServerNotReachableError;

                return result;
            }

            try
            {
                HttpClientHandler handler = new HttpClientHandler()
                {
                    ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => GlobalConfig.IgnoreSSLCertificates
                };

                // Handle Cookie Container
                if (cookieContainer != null)
                {
                    handler.CookieContainer = cookieContainer;
                }
                else
                {
                    handler.CookieContainer = new CookieContainer();
                }


                using HttpClient client = new HttpClient(handler);
                client.Timeout = TimeSpan.FromSeconds(GlobalConfig.RequestTimeout);

                // Encode data
                using HttpContent content = new StringContent(data, Encoding.UTF8, contentType);

                using HttpRequestMessage requestMessage = new HttpRequestMessage()
                {
                    Content = content,
                    Method = HttpMethod.Post,
                    RequestUri = new Uri(Url)
                };

                // Add headers
                requestMessage.Headers.Add("User-Agent", GlobalConfig.UserAgent);


                // Make POST
                using HttpResponseMessage response = await client.SendAsync(requestMessage);


                // If request will be unsuccesfull
                if (response.StatusCode != System.Net.HttpStatusCode.OK)
                {
                    // Status code error

                    result["Success"] = false;
                    result["ErrorMessage"] = $"{Messages.StatusCodeError} {response.StatusCode}";

                    return result;

                }
                else if (response.Content == null)
                {
                    // Empty response

                    result["Success"] = false;
                    result["ErrorMessage"] = $"{Messages.EmptyResponseError}";

                    return result;
                }


                string cookiesStr = JsonConvert.SerializeObject(handler.CookieContainer.GetCookies(new Uri(GlobalConfig.ApiUrl)))?.ToString() ?? string.Empty;

                string PlainTextData = await DecompressResponseContent(response);

                result["Success"] = true;
                result["Response"] = PlainTextData;
                result["Cookies"] = cookiesStr;

                return result;

            }
            catch (SocketException ex)
            {
                // HTTP timeout exception

                result["Success"] = false;
                result["ErrorMessage"] = ex?.ToString() ?? string.Empty;

                return result;
            }
            catch (HttpRequestException ex)
            {
                // HTTP request exception

                result["Success"] = false;
                result["ErrorMessage"] = ex?.ToString() ?? string.Empty;

                return result;
            }
            catch (Exception e)
            {
                // Another exception

                result["Success"] = false;
                result["ErrorMessage"] = e?.ToString() ?? string.Empty;

                return result;
            }
        }


        public async static Task<JObject> GetRequest(string Url, string data, string contentType, CookieContainer cookieContainer = null)
        {
            JObject result = new JObject
            {
                { "Success", null },
                { "ErrorMessage", null },
                { "Response", null }
            };

            if (!CrossConnectivity.Current.IsConnected)
            {
                // No Internet connection

                result["Success"] = false;
                result["ErrorMessage"] = Messages.NoInternetConnectionError;

                return result;
            }

            bool ServerAvailable = await IsServerReachable();
            if (!ServerAvailable)
            {
                // Server down

                result["Success"] = false;
                result["ErrorMessage"] = Messages.ServerNotReachableError;

                return result;
            }


            try
            {
                HttpClientHandler handler = new HttpClientHandler()
                {
                    ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => GlobalConfig.IgnoreSSLCertificates
                };

                // Handle Cookie Container
                if (cookieContainer != null)
                {
                    handler.CookieContainer = cookieContainer;
                }
                else
                {
                    handler.CookieContainer = new CookieContainer();
                }


                using HttpClient client = new HttpClient(handler) { Timeout = TimeSpan.FromSeconds(GlobalConfig.RequestTimeout) };

                using HttpContent content = new StringContent(data, Encoding.UTF8, contentType);

                using HttpRequestMessage requestMessage = new HttpRequestMessage()
                {
                    Content = content,
                    Method = HttpMethod.Get,
                    RequestUri = new Uri(Url)
                };

                // Add headers
                requestMessage.Headers.Add("User-Agent", GlobalConfig.UserAgent);


                // Make POST
                using HttpResponseMessage response = await client.SendAsync(requestMessage);


                // If request will be unsuccesfull
                if (response.StatusCode != System.Net.HttpStatusCode.OK)
                {
                    // Status code error

                    result["Success"] = false;
                    result["ErrorMessage"] = $"Status code: {response.StatusCode}";

                    return result;

                }
                else if (response.Content == null)
                {
                    // Empty response

                    result["Success"] = false;
                    result["ErrorMessage"] = $"{Messages.EmptyResponseError}";

                    return result;
                }

                string PlainTextData = await DecompressResponseContent(response);

                result["Success"] = true;
                result["Response"] = PlainTextData;

                return result;

            }
            catch (HttpRequestException ex)
            {
                // HTTP request exception

                result["Success"] = false;
                result["ErrorMessage"] = ex?.Message ?? string.Empty;

                return result;
            }
            catch (Exception e)
            {
                // Another exception

                result["Success"] = false;
                result["ErrorMessage"] = e?.Message ?? string.Empty;

                return result;
            }
        }

        private async static Task<string> DecompressResponseContent(HttpResponseMessage response)
        {
            // Read data as stream
            using var responseStream = await response.Content.ReadAsStreamAsync();

            if (response.Content.Headers.ContentEncoding.Contains("gzip"))
            {
                // GZIP decompress
                using var decompressedStream = new GZipStream(responseStream, CompressionMode.Decompress);

                // Read stream as text
                return await new StreamReader(decompressedStream).ReadToEndAsync();
            }

            else if (response.Content.Headers.ContentEncoding.Contains("deflate"))
            {
                // DEFLATE decompress
                using var decompressedStream = new DeflateStream(responseStream, CompressionMode.Decompress);

                // Read stream as text
                return await new StreamReader(decompressedStream).ReadToEndAsync();
            }
            else
            {
                // No compression

                return await new StreamReader(responseStream).ReadToEndAsync();
            }
        }

        private static string JsonUrlEncode(JObject json, bool PercentageEncoding=true)
        {
            // Encode JSON to URL encoding

            StringBuilder sb = new StringBuilder();;

            string JsonKey;
            string JsonValue;

            int i = 1;
            foreach (var item in json)
            {
                if (PercentageEncoding)
                {
                    JsonKey = HttpUtility.UrlEncode(item.Key?.ToString() ?? string.Empty);
                    JsonValue = HttpUtility.UrlEncode(item.Value?.ToString() ?? string.Empty);
                }
                else
                {
                    JsonKey = item.Key ?? string.Empty;
                    JsonValue = item.Value?.ToString() ?? string.Empty;
                }

                sb.Append(JsonKey + '=' + JsonValue);

                if (i < json.Count)
                {
                    sb.Append('&');
                }

                i++;
            }

            return sb?.ToString() ?? string.Empty;
        }

        public static async Task<bool> IsServerReachable()
        {
            bool reachable = await CrossConnectivity.Current.IsRemoteReachable(GlobalConfig.ApiDomain, 443);

            return reachable;
        }

        public static bool IsFullUrl(string url)
        {
            // Check if url is valid

            if (string.IsNullOrEmpty(url))
                return false;


            Regex reg = new Regex("(^$|(http(s)?://)([\\w-]+\\.)+[\\w-]+([\\w- ;,./?%&=]*))");

            return reg.Match(url).Success;
        }

    }
}
