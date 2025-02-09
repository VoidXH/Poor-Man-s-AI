using System.Net;
using System.Runtime.CompilerServices;
using System.Text;

namespace VoidX.WPF {
    /// <summary>
    /// HTTP utilities.
    /// </summary>
    static class HTTP {
        /// <summary>
        /// Merge a <paramref name="server"/> URL with an endpoint <paramref name="path"/>.
        /// </summary>
        public static string Combine(string server, string path) {
            bool trailing = server.EndsWith('/'),
                leading = path.StartsWith('/');
            if (trailing && leading) {
                return server + path[1..];
            } else if (trailing || leading) {
                return server + path;
            } else {
                return $"{server}/{path}";
            }
        }

        /// <summary>
        /// Gets a HTTP resource with a timeout.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string GET(string url, int timeoutSeconds = 5) => GET(url, null, timeoutSeconds);

        /// <summary>
        /// Gets a HTTP resource with a timeout, using some cookies.
        /// </summary>
        public static string GET(string url, CookieCollection cookies, int timeoutSeconds = 5) {
            try {
                using HttpResponseMessage response = CreateClient(cookies, timeoutSeconds).GetAsync(url).Result;
                response.EnsureSuccessStatusCode();
                return response.Content.ReadAsStringAsync().Result;
            } catch { }
            return null;
        }

        /// <summary>
        /// For a set of POST <paramref name="data"/>, fetch the cookies the server sets for them.
        /// </summary>
        public static CookieCollection GetCookies(string url, KeyValuePair<string, string>[] data, int timeoutSeconds = 5) {
            HttpClientHandler handler = new() {
                CookieContainer = new CookieContainer()
            };
            using HttpClient client = new(handler) {
                Timeout = TimeSpan.FromSeconds(timeoutSeconds)
            };
            try {
                // Send the POST request
                var response = client.PostAsync(url, new FormUrlEncodedContent(data)).Result;
                response.EnsureSuccessStatusCode();
                return handler.CookieContainer.GetCookies(new Uri(url));
            } catch {
                return null;
            }
        }

        /// <summary>
        /// Sends an empty POST request.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string POST(string url, int timeoutSeconds = 5) =>
            POST(url, new StringContent(string.Empty), null, timeoutSeconds);

        /// <summary>
        /// Sends an empty POST request with cookies.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string POST(string url, CookieCollection cookies, int timeoutSeconds = 5) =>
            POST(url, new StringContent(string.Empty), cookies, timeoutSeconds);

        /// <summary>
        /// Sends a POST request with JSON content.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string POST(string url, string json, int timeoutSeconds = 5) =>
            POST(url, new StringContent(json, Encoding.UTF8, "application/json"), null, timeoutSeconds);

        /// <summary>
        /// Sends a POST request with JSON content, and calls back with the partial message periodically.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string POST(string url, string json, Action<string> callback, int callbackPeriodMs,
            Func<string, string> transformer, CancellationToken canceller, int timeoutSeconds = 5) =>
            POST(url, new StringContent(json, Encoding.UTF8, "application/json"), null, callback, callbackPeriodMs,
                transformer, canceller, timeoutSeconds).Result;

        /// <summary>
        /// Sends a POST request of key-value pairs with a timeout.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string POST(string url, KeyValuePair<string, string>[] data, int timeoutSeconds = 5) => POST(url, data, null, timeoutSeconds);

        /// <summary>
        /// Sends a POST request of key-value pairs and cookies with a timeout.
        /// </summary>
        public static string POST(string url, KeyValuePair<string, string>[] data, CookieCollection cookies, int timeoutSeconds = 5) {
            using MultipartFormDataContent content = [];
            for (int i = 0; i < data.Length; i++) {
                if (data[i].Value != null) {
                    content.Add(new StringContent(data[i].Value), data[i].Key);
                }
            }
            return POST(url, content, cookies, timeoutSeconds);
        }

        /// <summary>
        /// Sends a POST request of large binary data with a timeout.
        /// </summary>
        public static string POST(string url, (string key, byte[] value)[] data, int timeoutSeconds = 5) {
            using MultipartFormDataContent form = [];
            for (int i = 0; i < data.Length; i++) {
                form.Add(new ByteArrayContent(data[i].value), data[i].key);
            }
            return POST(url, form, null, timeoutSeconds);
        }

        /// <summary>
        /// Sends an arbitrary POST request.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string POST(string url, HttpContent content, CookieCollection cookies, int timeoutSeconds = 5) {
            HttpRequestMessage request = new(HttpMethod.Post, url) {
                Content = content
            };
            try {
                using HttpResponseMessage response = CreateClient(cookies, timeoutSeconds).SendAsync(request).Result;
                response.EnsureSuccessStatusCode();
                return response.Content.ReadAsStringAsync().Result;
            } catch { }
            return null;
        }

        /// <summary>
        /// Sends an arbitrary POST request, and calls back with the partial message periodically.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static async Task<string> POST(string url, HttpContent content, CookieCollection cookies,
            Action<string> callback, int callbackPeriodMs, Func<string, string> transformer, CancellationToken canceller, int timeoutSeconds = 5) {
            HttpRequestMessage request = new(HttpMethod.Post, url) {
                Content = content
            };
            string result = string.Empty;
            TimeSpan interval = TimeSpan.FromMilliseconds(callbackPeriodMs);
            DateTime sendAt = default;
            try {
                using HttpResponseMessage response = await CreateClient(cookies, timeoutSeconds).
                    SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
                response.EnsureSuccessStatusCode();
                using Stream stream = await response.Content.ReadAsStreamAsync();
                using StreamReader reader = new(stream);
                string line;
                DateTime failAt = DateTime.Now + TimeSpan.FromSeconds(timeoutSeconds); // As the timeout for the client is "since last reply"
                while ((line = await reader.ReadLineAsync()) != null) {
                    result += transformer(line);
                    if (canceller.IsCancellationRequested) {
                        return result;
                    }
                    DateTime now = DateTime.Now;
                    if (sendAt == default) { // Prevent sending too small of a progress
                        sendAt = now + interval;
                    }
                    if (sendAt < now) {
                        callback(result);
                        sendAt = now + interval;
                    }
                    if (failAt < now) {
                        return result;
                    }
                }
                return result;
            } catch { }
            return null;
        }

        /// <summary>
        /// Create a <see cref="HttpClient"/> to work with, add <paramref name="cookies"/> if they exist.
        /// </summary>
        static HttpClient CreateClient(CookieCollection cookies, int timeoutSeconds) {
            HttpClient client;
            if (cookies == null) {
                client = new();
            } else {
                CookieContainer container = new(cookies.Count);
                foreach (Cookie cookie in cookies) {
                    container.Add(cookie);
                }
                client = new(new HttpClientHandler() {
                    CookieContainer = container
                });
            }
            client.Timeout = TimeSpan.FromSeconds(timeoutSeconds);
            return client;
        }
    }
}