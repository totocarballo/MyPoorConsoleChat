using System;
using System.Net;
using System.Text.RegularExpressions;

namespace MyPoorConsoleChat.Chat
{
    internal class Livestream
    {
        public static string GetChatUrl(string channelUrl)
        {
            Console.Clear();

            string username = channelUrl;
            string pattern = username.Contains("channel") ? @"(?:\/channel\/)([^\/]+)(?:\/.*)?$" : @"(?:youtube\.com\/)([^\/]+)";

            var match = Regex.Match(username, pattern);

            if (!match.Success)
            {
                return "Channel URL isn't in the correct format";
            }

            try
            {
                using (WebClient webClient = new WebClient())
                {
                    string htmlContent = webClient.DownloadString($"https://www.youtube.com/{match.Groups[1].Value}/live");
                    string url = ParseChatUrl(htmlContent);
                    if (string.IsNullOrEmpty(url))
                    {
                        return "The channel isn't livestreaming right now.";
                    }
                    return url;
                }
            }
            catch (Exception ex)
            {
                return $"Error: {ex}";
            }
        }

        private static string ParseChatUrl(string htmlContent)
        {
            int indexUrl = htmlContent.IndexOf("<link rel=\"canonical\" href=\"https://www.youtube.com/watch?v=");

            if (indexUrl != -1)
            {
                int indexStartUrl = indexUrl + 60;
                int indexEndUrl = htmlContent.IndexOf("\"", indexStartUrl);
                return $"https://www.youtube.com/live_chat?is_popout=1&v={htmlContent.Substring(indexStartUrl, indexEndUrl - indexStartUrl)}";
            }

            return null;
        }
    }
}
