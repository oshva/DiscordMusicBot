﻿using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.EventArgs;
using Newtonsoft.Json;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus.Net;
using DSharpPlus.Lavalink;
using DiscordMusicBot.Commands;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;

namespace DiscordMusicBot.Classes
{
    public class Bot
    {
            
        public DiscordClient Client { get; private set; }
        public CommandsNextExtension Commands { get; private set; }
        public async Task RunAsync()
        {
            var json = string.Empty;

            //Подгрузка токена из конфига.
            using (var fs = File.OpenRead("config.json"))
            using (var sr = new StreamReader(fs, new UTF8Encoding(false)))
                json = await sr.ReadToEndAsync().ConfigureAwait(false);

            var configJson = JsonConvert.DeserializeObject<ConfigJson>(json);

            var config = new DiscordConfiguration
            {
                Token = configJson.Token,
                TokenType = TokenType.Bot,
                AutoReconnect = true
            };

            Client = new DiscordClient(config);

            Client.Ready += OnClientReady;

            var commandsConfig = new CommandsNextConfiguration
            {
                StringPrefixes = new[] { configJson.Prefix },
                EnableDms = false,
                EnableMentionPrefix = true
            };

            //Подключение LavaLink`a
            var endpoint = new ConnectionEndpoint
            {
                Hostname = "127.0.0.1",
                Port = 2333
            };

            var lavalinkConfig = new LavalinkConfiguration
            {
                Password = "youshallnotpass",
                RestEndpoint = endpoint,
                SocketEndpoint = endpoint
            };


            //Регистрация слеш-команд.
            var slash = Client.UseSlashCommands();
            slash.RegisterCommands<SlashMusicCommands>();


            var lavalink = Client.UseLavalink();

            //Регистрация команд с префиксом.
            //Commands = Client.UseCommandsNext(commandsConfig);
            //Commands.RegisterCommands<MusicCommands>();

            await Client.ConnectAsync();

            await lavalink.ConnectAsync(lavalinkConfig); 

            await Task.Delay(-1);
        }

        private Task OnClientReady(DiscordClient client, ReadyEventArgs e)
        {
            return Task.CompletedTask;
        }
    }
}
