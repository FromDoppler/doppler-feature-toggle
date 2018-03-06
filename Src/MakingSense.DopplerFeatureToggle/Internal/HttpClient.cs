#if (!NETSTANDARD1_0)
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace MakingSense.DopplerFeatureToggle.Internal
{
    /// <summary>
    /// Wrapper around System.Net.Http.HttpClient to allow easily implement a compatible version for netstandard1.0
    /// </summary>
    public class HttpClient : IHttpClient
    {

        private static System.Net.Http.HttpClient _httpClient = new System.Net.Http.HttpClient();

        /// <inheritdoc />
        public async Task<SimplifiedHttpResponse> GetStringAsync(string url, string ifNoneMatch = null)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, url);
            if (ifNoneMatch != null)
            {
                request.Headers.TryAddWithoutValidation("If-None-Match", ifNoneMatch);
            }

            var response = await _httpClient.SendAsync(request);

            var newEtag = response.Headers.ETag.Tag;

            if (response.StatusCode == System.Net.HttpStatusCode.NotModified)
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
                    Body = await _httpClient.GetStringAsync(url),
                    Etag = newEtag
                };
            }
        }
    }
}
#endif
