#if (NETSTANDARD1_0)
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Doppler.FeatureToggle.Internal
{
    /// <summary>
    /// Custom HttpClient implementation
    /// </summary>
    public class HttpClient : IHttpClient
    {
        private const int BUFFER_SIZE = 1024;

        /// <inheritdoc />
        public async Task<SimplifiedHttpResponse> GetStringAsync(string url, string ifNoneMatch = null)
        {
            var request = WebRequest.CreateHttp(url);

            if (ifNoneMatch != null)
            {
                request.Headers[HttpRequestHeader.IfNoneMatch] = ifNoneMatch;
            }

            var response = await SendAsync(request);

            var newEtag = response.Headers["ETag"];

            if (response.StatusCode == HttpStatusCode.NotModified)
            {
                return new SimplifiedHttpResponse()
                {
                    NotModified = true,
                    Body = null,
                    Etag = newEtag
                };
            }
            else
            {
                return new SimplifiedHttpResponse()
                {
                    NotModified = false,
                    Body = await GetStringAsync(response),
                    Etag = newEtag
                };
            }
        }

        private async Task<HttpWebResponse> SendAsync(HttpWebRequest request)
        {
            try
            {
                var webResponse = await Task.Factory.FromAsync(request.BeginGetResponse, request.EndGetResponse, null);
                return (HttpWebResponse)webResponse;
            }
            catch (WebException e) when (e.Response is HttpWebResponse httpResponse && httpResponse.StatusCode == HttpStatusCode.NotModified)
            {
                return httpResponse;
            }
        }

        private async Task<string> GetStringAsync(HttpWebResponse response)
        {
            var buffer = new byte[BUFFER_SIZE];
            var sb = new StringBuilder();
            using (var stream = response.GetResponseStream())
            {
                while (true)
                {
                    var read = await stream.ReadAsync(buffer, 0, BUFFER_SIZE);
                    if (read > 0)
                    {
                        sb.Append(Encoding.UTF8.GetString(buffer, 0, read));
                    }
                    else
                    {
                        return sb.ToString();
                    }
                }
            }
        }
    }
}
#endif