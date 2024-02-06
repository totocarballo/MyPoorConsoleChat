using CefSharp;
using CefSharp.OffScreen;
using System;
using System.Timers;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace MyPoorConsoleChat.Chat
{
    internal class Read
    {
        private bool IsTwitch = false;
        public bool IsConnected = false;
        private int TimerInterval { get; set; } = 250;
        private Timer TimerRead;
        private ChromiumWebBrowser ChromeBrowser;
        private readonly List<string> MessagesCache = new List<string>();
        public void Start(bool isTwitch, string channel)
        {
            IsTwitch = isTwitch;

            var url = IsTwitch ?
                $"https://nightdev.com/hosted/obschat/?theme=undefined&channel={channel}" :
                channel;//yt;
            
            ChromeBrowser = new ChromiumWebBrowser(url);

            ChromeBrowser.LoadingStateChanged += ChromeBrowser_LoadingStateChanged;
            ChromeBrowser.LoadUrl(url);
            TimerRead = new Timer { Interval = TimerInterval };

            if (IsTwitch)
            {
                TimerRead.Elapsed += Timer_Twitch;
            }
            else
            {
                TimerRead.Elapsed += Timer_Youtube;
            }

            TimerRead.Enabled = true;
        }

        private void ChromeBrowser_LoadingStateChanged(object sender, LoadingStateChangedEventArgs e)
        {
            if (e.IsLoading == false)
            {
                ChromeBrowser.LoadingStateChanged -= ChromeBrowser_LoadingStateChanged;
                IsConnected = true;
            }
        }

        private async void Timer_Twitch(object sender, ElapsedEventArgs e)
        {
            if (!IsConnected)
                return;

            var frame = ChromeBrowser.GetMainFrame();
            var response = await frame.EvaluateScriptAsync(@"
                (function() {
                    var elements = document.querySelectorAll('.chat_line');
                    var result = [];
                    elements.forEach(function(element) {
                        var timestamp = '[' + element.getAttribute('data-timestamp') + ']';
                        var nick = element.querySelector('.nick').textContent.trim();
                        var message = element.querySelector('.message').textContent.trim();
                        result.push(timestamp + ' ' + nick + ': ' + message);
                    });
                    return result.join('\n');
                })();
            ");

            if (response.Success && !string.IsNullOrWhiteSpace(response.Result.ToString()))
            {
                ParseMessage(response.Result.ToString(), "[TW]");
            }
        }

        private async void Timer_Youtube(object sender, ElapsedEventArgs e)
        {
            if (!IsConnected)
                return;

            var frame = ChromeBrowser.GetMainFrame();
            var response = await frame.EvaluateScriptAsync(@"
                (function() { 
                    var items = document.querySelectorAll('yt-live-chat-text-message-renderer');
                    var result = [];
                    items.forEach(function(item) {
                        var timeStamp = item.querySelector('span#timestamp');
                        var message = item.querySelector('span#message');
                        var authorName = item.querySelector('span#author-name');
                        if (timeStamp && message && authorName) {
                            var content = '[' + item.getAttribute('id') + '] ' + authorName.textContent.trim() + ': ' + message.textContent.trim();
                            result.push(content);
                        }
                    });
                    return result.join('\n');
                })();
            ");

            if (response.Success && !string.IsNullOrWhiteSpace(response.Result.ToString()))
            {
                ParseMessage(response.Result.ToString(), "[YT]");
            }
        }

        private void ParseMessage(string response, string platform)
        {
            string patron = @"\[(.*?)\]\s(.*?):\s(.*)";

            var list = response.Split(new char[] { '\n' }, StringSplitOptions.RemoveEmptyEntries).ToList();

            foreach (var item in list)
            {
                var match = Regex.Match(item, patron);
                if (match.Success)
                {
                    string idMessage = match.Groups[1].Value;
                    string user = match.Groups[2].Value;
                    string message = match.Groups[3].Value;

                    if (user.Equals("Chat") || string.IsNullOrEmpty(message))
                        continue;

                    if (!MessagesCache.Contains(idMessage))
                    {
                        MessagesCache.Add(idMessage);
                        Console.WriteLine($"{platform} {user}: {message}");
                    }
                }
            }
        }

        public void Stop()
        {
            try
            {
                IsConnected = false;
                ChromeBrowser.Dispose();
            }
            catch (Exception)
            {
            }
        }
    }
}
