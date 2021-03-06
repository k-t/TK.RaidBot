﻿using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Interactivity;
using NLog;
using TK.RaidBot.Model;
using TK.RaidBot.Model.Raids;
using TK.RaidBot.Model.Raids.Templates;
using TK.RaidBot.Services;

namespace TK.RaidBot.Discord
{
    [Group("raid")]
    public class RaidCommands : BaseCommandModule
    {
        private const string DefaultRaidTime = "21:00"; // MSK

        private static readonly ILogger Log = LogManager.GetCurrentClassLogger();

        private static readonly Regex DateRegex =
            new Regex(@"^(?<day>[0-9]{1,2})(\.(?<month>[0-9]{1,2})?)(\.(?<year>[0-9]+))?$", RegexOptions.Compiled);

        private static readonly Regex TimeRegex =
            new Regex(@"^(?<hour>[0-9]{1,2})\:(?<minute>[0-9]{1,2})$", RegexOptions.Compiled);

        private readonly RaidService raidService;
        private readonly EmojiService emojiService;
        private readonly RaidMessageService messageBuilder;

        public RaidCommands(RaidService raidService, RaidMessageService messageBuilder, EmojiService emojiService)
        {
            this.raidService = raidService;
            this.messageBuilder = messageBuilder;
            this.emojiService = emojiService;
        }

        [Command("wvw")]
        public Task CreateWvwRaid(CommandContext ctx, params string[] args)
        {
            return CreateRaid(RaidTemplates.Wvw, ctx, args);
        }

        [Command("pve")]
        public Task CreatePveRaid(CommandContext ctx, params string[] args)
        {
            return CreateRaid(RaidTemplates.Pve, ctx, args);
        }

        public async Task CreateRaid(IRaidTemplate template, CommandContext ctx, params string[] args)
        {
            try
            {
                if (ctx.User.IsBot || ctx.Guild == null || ctx.Channel == null)
                    return;

                // parse args

                if (args.Length == 0)
                {
                    await ctx.RespondAsync("Формат команды:\n```!raid create <date> [time]```");
                    return;
                }

                var dateStr = args[0];
                var timeStr = args.Length > 1 ? args[1] : DefaultRaidTime;

                if (!TryParseRaidDate(dateStr, timeStr, out DateTime raidDate))
                {
                    await ctx.RespondAsync("Ошибка:\n```Неправильный формат даты или времени```");
                    return;
                }

                var interactivity = ctx.Client.GetInteractivity();

                await ctx.Member.SendMessageAsync(
                    $"Вы хотите создать рейд в канале **#{ctx.Channel.Name}** сервера **{ctx.Guild.Name}**.\nОтправьте название рейда в ответе на это сообщение.");

                var titleMessage = await interactivity.WaitForMessageAsync(x => x.Channel.IsPrivate && x.Author.Id == ctx.User.Id);
                var title = titleMessage.Result.Content;

                // create message

                var timestamp = DateTime.Now;

                var raid = new Raid
                {
                    GuildId = ctx.Guild.Id,
                    ChannelId = ctx.Channel.Id,
                    CreationDate = timestamp,
                    Timestamp = timestamp,
                    Title = title,
                    Date = raidDate,
                    Status = RaidStatus.Scheduled,
                    Participants = new List<RaidParticipant>(),
                    OwnerId = ctx.User.Id,
                    OwnerDisplayName = ctx.Member.DisplayName,
                    TemplateCode = template.Code
                };

                var messageEmbed = messageBuilder.BuildEmbed(ctx.Client, raid);

                var message = await ctx.RespondAsync(embed: messageEmbed);
                raid.MessageId = message.Id;

                // store raid

                raidService.AddRaid(raid);

                // add reactions

                await message.CreateReactionAsync(emojiService.GetStatusEmoji(ctx.Client, ParticipationStatus.Available));
                await message.CreateReactionAsync(emojiService.GetStatusEmoji(ctx.Client, ParticipationStatus.Maybe));
                await message.CreateReactionAsync(emojiService.GetStatusEmoji(ctx.Client, ParticipationStatus.NotAvailable));

                foreach (var group in template.Groups)
                {
                    foreach (var profession in group.Professions)
                    {
                        var emoji = emojiService.GetRoleEmoji(ctx.Client, profession);
                        await message.CreateReactionAsync(emoji);
                    }
                }
            }
            catch (Exception e)
            {
                Log.Error(e, "CreateRaid: Unhandled exception");
            }
        }

        private bool TryParseRaidDate(string dateStr, string timeStr, out DateTime result)
        {
            result = DateTime.MinValue;

            try
            {
                var dateMatch = DateRegex.Match(dateStr);
                if (dateMatch.Success)
                {
                    var day = int.Parse(dateMatch.Groups["day"].Value);

                    var monthStr = dateMatch.Groups["month"].Value;
                    var month = !string.IsNullOrEmpty(monthStr) ? int.Parse(monthStr) : DateTime.Now.Month;

                    var yearStr = dateMatch.Groups["year"].Value;
                    var year = !string.IsNullOrEmpty(yearStr) ? int.Parse(yearStr) : DateTime.Now.Year;

                    var timeMatch = TimeRegex.Match(timeStr);
                    if (timeMatch.Success)
                    {
                        var hour = int.Parse(timeMatch.Groups["hour"].Value);
                        var minute = int.Parse(timeMatch.Groups["minute"].Value);

                        result = new DateTimeOffset(year, month, day, hour, minute, 0, TimeSpan.FromHours(3)).UtcDateTime;
                        return true;
                    }
                }

                return false;
            }
            catch (Exception e)
            {
                Log.Debug(e, "Unexpected exception while parsing raid date: date={0} time={1}", dateStr, timeStr);
                return false;
            }
        }
    }
}
