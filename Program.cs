using CefSharp.OffScreen;
using CefSharp;
using MyPoorConsoleChat.Chat;
using System;
using System.IO;
using System.Threading;

namespace MyPoorConsoleChat
{
    internal class Program
    {
        private static Read ChatYoutube;
        private static Read ChatTwitch;

        static void Main(string[] args)
        {
            string channelYT = GetArgumentValue(args, "-yt");
            string channelTW = GetArgumentValue(args, "-tw");

            if (string.IsNullOrEmpty(channelYT))
            {
                Console.Clear();
                channelYT = SetChannel("YouTube", "Insert YouTube channel URL that is currently live streaming:\n(Leave blank if not in use)\n");
            }

            if (string.IsNullOrEmpty(channelTW))
            {
                Console.Clear();
                channelTW = SetChannel("Twitch", "Insert Twitch channel username:\n(Leave blank if not in use)\n");
            }

            InitializeCefSharp();

            Console.Clear();

            Console.WriteLine("Press 'q' to exit...");
            Thread.Sleep(5000);

            StartChat(channelYT, false);
            StartChat(channelTW, true);

            while (Console.ReadKey(true).Key != ConsoleKey.Q)
            {
            }

            StopChat(ChatYoutube);
            StopChat(ChatTwitch);

            Cef.Shutdown();
        }

        static void InitializeCefSharp()
        {
            var settings = new CefSettings()
            {
                LogSeverity = LogSeverity.Disable,
                CachePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "CefSharp\\Cache")
            };
            Cef.Initialize(settings, performDependencyCheck: true, browserProcessHandler: null);
        }

        static void StartChat(string channel, bool isTwitch)
        {
            if (!string.IsNullOrEmpty(channel))
            {
                var chat = new Read();
                chat.Start(isTwitch, channel);

                if (isTwitch)
                {
                    ChatTwitch = chat;
                }
                else
                {
                    ChatYoutube = chat;
                }
            }
        }

        static void StopChat(Read chat)
        {
            if (chat != null && chat.IsConnected)
            {
                chat.Stop();
            }
        }

        static string GetArgumentValue(string[] args, string argumentName)
        {
            foreach (string arg in args)
            {
                if (arg.StartsWith($"{argumentName}="))
                {
                    return arg.Substring(argumentName.Length + 1);
                }
            }
            return null;
        }

        static string SetChannel(string platform, string promptMessage)
        {
            Console.WriteLine(promptMessage);
            var input = Console.ReadLine()?.Trim();
            if (!string.IsNullOrEmpty(input))
            {
                if (platform.Equals("YouTube", StringComparison.OrdinalIgnoreCase))
                {
                    return Livestream.GetChatUrl(input);
                }
                return input;
            }
            return null;
        }
    }
}
