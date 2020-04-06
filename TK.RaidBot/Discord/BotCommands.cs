using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using NLog;
using TK.RaidBot.Actions;
using TK.RaidBot.Model.Data;
using TK.RaidBot.Services;

namespace TK.RaidBot.Discord
{
    public class BotCommands : BaseCommandModule
    {
        private const string DefaultRaidTime = "21:00"; // MSK

        private static readonly ILogger Log = LogManager.GetCurrentClassLogger();

        private static readonly Regex DateRegex =
            new Regex(@"^(?<day>[0-9]{1,2})(\.(?<month>[0-9]{1,2})?)(\.(?<year>[0-9]+))?$", RegexOptions.Compiled);

        private static readonly Regex TimeRegex =
            new Regex(@"^(?<hour>[0-9]{1,2})\:(?<minute>[0-9]{1,2})$", RegexOptions.Compiled);

        private readonly DataService dataService;
        private readonly EmojiService emojiService;
        private readonly MessageBuilderService messageBuilder;

        public BotCommands(DataService dataService, MessageBuilderService messageBuilder, EmojiService emojiService)
        {
            this.dataService = dataService;
            this.messageBuilder = messageBuilder;
            this.emojiService = emojiService;
        }

        [Command("create")]
        public async Task CreateRaid(CommandContext ctx, params string[] args)
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
                    OwnerDisplayName = ctx.Member.DisplayName
                };

                var messageEmbed = messageBuilder.BuildEmbed(ctx.Client, raid);

                var message = await ctx.RespondAsync(embed: messageEmbed);
                raid.MessageId = message.Id;

                // store raid

                dataService.AddRaid(raid);

                // add reactions

                await message.CreateReactionAsync(emojiService.GetStatusEmoji(ctx.Client, ParticipationStatus.Available));
                await message.CreateReactionAsync(emojiService.GetStatusEmoji(ctx.Client, ParticipationStatus.Maybe));
                await message.CreateReactionAsync(emojiService.GetStatusEmoji(ctx.Client, ParticipationStatus.NotAvailable));
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
