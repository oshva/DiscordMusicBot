using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Lavalink;
using DSharpPlus.SlashCommands;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace DiscordMusicBot.Commands
{
    public sealed class SlashMusicCommands : ApplicationCommandModule
    {
        //Проверка находится ли польхователь в канале.
        public override async Task<bool> BeforeSlashExecutionAsync(InteractionContext ctx)
        {
            var voice = ctx.Member.VoiceState;
            var channel = voice?.Channel;


            //Проверка подключен ли пользователь к голосовому каналу
            if (channel == null)
            {
                //await ctx.RespondAsync($"Вам нужно находиться в голосовом канале.");
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent("Вам нужно находиться в голосовом канале."));
                return false;
            }

            var member = ctx.Guild.CurrentMember?.VoiceState?.Channel;

            //Проверка находится ли пользователь в одном канале с ботом
            if (member != null && channel != member)
            {
                //await ctx.RespondAsync("Вам нужно находиться в голосовом канале с ботом.");
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent("Вам нужно находиться в голосовом канале c ботом."));
                return false;
            }

            return true;
        }


        [SlashCommand("play", "Добавить трек."), Aliases("p")]
        public async Task Play(InteractionContext ctx, [Option("Ссылка", "Ссылка на трек")][RemainingText] string search)
        {
            var channel = ctx.Member.VoiceState.Channel;
            var lava = ctx.Client.GetLavalink();
            var node = lava.ConnectedNodes.Values.First();

            //Проверка на lavalink
            if (!lava.ConnectedNodes.Any())
            {
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent($"LavaLink не подключен!"));
                return;

                //await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder());
            }

            //Подключение к каналу
            await node.ConnectAsync(channel);

            var loadResult = await node.Rest.GetTracksAsync(search);

            //Проверка поиска
            if (loadResult.LoadResultType == LavalinkLoadResultType.LoadFailed
                || loadResult.LoadResultType == LavalinkLoadResultType.NoMatches)
            {
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent($"Не удалось найти трек: {search}"));
                return;
            }

            var conn = node.GetGuildConnection(ctx.Member.VoiceState.Guild);

            var track = loadResult.Tracks.First();

            //Проигрывание музыки
            await conn.PlayAsync(track);

            var embedB = new DiscordEmbedBuilder
            {
                Title = "Сейчас играет: \n",
                Color = DiscordColor.Black,
                Description = $"{track.Title} \n\n Длительность трека: \n\n{track.Length}"
            };

            await ctx.CreateResponseAsync(embed: embedB);

        }

        [SlashCommand("pause", "Поставить трек на паузу.")]
        public async Task Pause(InteractionContext ctx)
        {
            var lava = ctx.Client.GetLavalink();
            var node = lava.ConnectedNodes.Values.First();
            var conn = node.GetGuildConnection(ctx.Member.VoiceState.Guild);

            if (conn.CurrentState.CurrentTrack == null)
            {
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent("Сейчас ничего не играет."));
                return;
            }

            await conn.PauseAsync().ConfigureAwait(false);
            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent("Трек поставлен на паузу."));
        }

        [SlashCommand("resume", "Снять трек с паузы.")]
        public async Task Resume(InteractionContext ctx)
        {
            var lava = ctx.Client.GetLavalink();
            var node = lava.ConnectedNodes.Values.First();
            var conn = node.GetGuildConnection(ctx.Member.VoiceState.Guild);

            await conn.ResumeAsync();
            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent("Трек снят с паузы."));
        }

        [SlashCommand("leave", "Отключиться от канала.")]
        public async Task Leave(InteractionContext ctx)
        {
            var channel = ctx.Member.VoiceState.Channel;
            var lava = ctx.Client.GetLavalink();
            if (!lava.ConnectedNodes.Any())
            {
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent("Нет подключения!"));
                return;
            }

            var node = lava.ConnectedNodes.Values.First();

            if (channel.Type != ChannelType.Voice)
            {
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent("Это не голосовой канал!"));
                return;
            }

            var conn = node.GetGuildConnection(channel.Guild);

            if (conn == null)
            {
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent("Lavalink не подключен!"));
                return;
            }

            await conn.DisconnectAsync();
            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent($"Вышел из канала {channel.Name}."));
        }

    }
}
