using System.Net;

namespace PoorMansAI.Extensions {
    /// <summary>
    /// Extension point using the Website, thus authentication is required that is provided by the application.
    /// </summary>
    public abstract class WebsiteExtension : Extension {
        /// <summary>
        /// Cookies used for admin authentication, set by the application when received.
        /// </summary>
        /// <remarks>Can be null when authentication was not yet finished.</remarks>
        internal static CookieCollection cookies;
    }
}