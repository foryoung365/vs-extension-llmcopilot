using System;

namespace LLMCopilot
{
    /// <summary>
    /// Provides utility methods for working with API base URLs.
    /// </summary>
    public static class ApiUtility
    {
        /// <summary>
        /// Cleans the base URL by removing trailing slashes.
        /// </summary>
        /// <param name="baseUrl">The base URL to clean.</param>
        /// <returns>The cleaned base URL without trailing slashes.
        /// Returns the original input if the URL is null or empty.</returns>
        public static string CleanBaseUrl(string baseUrl)
        {
            if (string.IsNullOrEmpty(baseUrl))
                return baseUrl;

            return baseUrl.TrimEnd('/');
        }
    }
}
