using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MakingSense.DopplerFeatureToggle.Internal
{
    /// <summary>
    /// Abstraction of HTTP Client
    /// </summary>
    public interface IHttpClient
    {
        /// <summary>
        /// Get response body as string after GET specified URL
        /// </summary>
        /// <param name="url"></param>
        /// <param name="ifNoneMatch"></param>
        /// <returns></returns>
        Task<SimplifiedHttpResponse> GetStringAsync(string url, string ifNoneMatch = null);
    }
}
