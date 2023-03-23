using DiscordMusicBot.Classes;
using DSharpPlus.CommandsNext;
using System;
using System.Net.Http;

namespace DiscordMusicBot
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var bot = new Bot();
            bot.RunAsync().GetAwaiter().GetResult();
        }
    }
}