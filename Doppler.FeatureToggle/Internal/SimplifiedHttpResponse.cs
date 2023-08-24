using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MakingSense.DopplerFeatureToggle
{
    /// <summary>
    /// A simplified representation of a HTTP response
    /// </summary>
    public class SimplifiedHttpResponse
    {
        /// <summary>
        /// 304 Not Modified(RFC 7232), Indicates that the resource has not been modified since the version specified 
        /// by the request headers If-Modified-Since or If-None-Match.
        /// </summary>
        /// <remarks>
        /// In such case, body will be null since the client still has a previously-downloaded copy.
        /// </remarks>
        public bool NotModified { get; set; } = false;

        /// <summary>
        /// Response etag
        /// </summary>
        public string Etag { get; set; } = null;

        /// <summary>
        /// Response body
        /// </summary>
        public string Body { get; set; }
    }
}
