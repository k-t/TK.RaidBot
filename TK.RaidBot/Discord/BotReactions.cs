﻿using DSharpPlus.Entities;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TK.RaidBot.Discord.Reactions;
using TK.RaidBot.Model.Data;
using TK.RaidBot.Services;

namespace TK.RaidBot.Discord
{
    public class BotReactions : IReactionsModule
    {
        private static readonly ILogger Log = LogManager.GetCurrentClassLogger();

        private readonly RaidService raidService;
        private readonly EmojiService emojiService;
        private readonly MessageBuilderService messageBuilder;

        public BotReactions(RaidService raidService, MessageBuilderService messageBuilder, EmojiService emojiService)
        {
            this.raidService = raidService;
            this.messageBuilder = messageBuilder;
            this.emojiService = emojiService;
        }

        public AsyncReactionHandler GetReactionHandler(string emojiName, ReactionContext ctx)
        {
            if (ctx.User.IsBot)
                return null;

            var raid = raidService.GetRaid(ctx.Channel.Id, ctx.Message.Id);
            if (raid != null)
            {
                bool isAdmin =
                    ctx.User.Id == raid.OwnerId ||
                    ctx.User.Id == ctx.Guild.Owner.Id ||
                    ctx.Client.CurrentApplication.Owners.Any(x => x.Id == ctx.User.Id);

                Log.Debug("Got reaction '{0}' from '{1}', isAdmin={2}",
                    emojiName, ctx.User.Username, isAdmin);

                switch (emojiName)
                {
                    case ":stop_button:":
                        return isAdmin
                            ? ctx => SetRaidStatus(RaidStatus.Expired, ctx)
                            : (AsyncReactionHandler)null;
                    case ":arrow_forward:":
                        return isAdmin
                            ? ctx => SetRaidStatus(RaidStatus.Scheduled, ctx)
                            : (AsyncReactionHandler)null;
                    case ":hammer:":
                        return isAdmin
                            ? FixReactions
                            : (AsyncReactionHandler)null;
                    case ":question:":
                        return ctx => SetParticipantRole(RaidRole.Unknown, ctx);
                    default:
                        {
                            // allow to change participation state only for scheduled raids
                            if (raid.Status == RaidStatus.Scheduled)
                            {
                                var status = emojiService.GetStatusByEmoji(emojiName);
                                if (status.HasValue)
                                    return ctx => SetParticipantStatus(status.Value, ctx);

                                var role = emojiService.GetRoleByEmoji(emojiName);
                                if (role.HasValue)
                                    return ctx => SetParticipantRole(role.Value, ctx);
                            }
                            return HandleUnknownEmoji;
                        }
                }
            }

            return null;
        }

        private async Task SetRaidStatus(RaidStatus status, ReactionContext ctx)
        {
            var raid = raidService.GetRaid(ctx.Channel.Id, ctx.Message.Id);
            if (raid == null)
                return;

            lock (raid)
            {
                // if nothing changed
                if (raid.Status == status)
                    return;

                raid.Status = status;
                raidService.UpdateRaid(raid);
            }

            Log.Debug("Updating message {0}", ctx.Message.Id);

            switch (status)
            {
                case RaidStatus.Scheduled:
                    await ctx.Message.CreateReactionAsync(emojiService.GetStatusEmoji(ctx.Client, ParticipationStatus.Available));
                    await ctx.Message.CreateReactionAsync(emojiService.GetStatusEmoji(ctx.Client, ParticipationStatus.Maybe));
                    await ctx.Message.CreateReactionAsync(emojiService.GetStatusEmoji(ctx.Client, ParticipationStatus.NotAvailable));
                    break;
                case RaidStatus.Expired:
                    await ctx.Message.DeleteAllReactionsAsync();
                    break;
            }

            await ctx.Message.ModifyAsync(embed: messageBuilder.BuildEmbed(ctx.Client, raid));
        }

        private async Task SetParticipantRole(RaidRole role, ReactionContext ctx)
        {
            var raid = raidService.GetRaid(ctx.Channel.Id, ctx.Message.Id);
            if (raid == null)
                return;

            lock (raid)
            {
                var participant = raid.Participants.FirstOrDefault(x => x.UserId == ctx.User.Id);
                if (participant == null)
                {
                    raid.Participants.Add(new RaidParticipant { Role = role, UserId = ctx.User.Id });
                    participant = new RaidParticipant
                    {
                        Status = ParticipationStatus.Available,
                        Role = role,
                        UserId = ctx.User.Id
                    };
                    raidService.UpdateRaid(raid);
                }
                else if (participant.Role != role)
                {
                    participant.Role = role;
                    raidService.UpdateRaid(raid);
                }
            }

            Log.Debug("Updating message {0}", ctx.Message.Id);

            await ctx.Message.ModifyAsync(embed: messageBuilder.BuildEmbed(ctx.Client, raid));
        }

        private async Task SetParticipantStatus(ParticipationStatus status, ReactionContext ctx)
        {
            var raid = raidService.GetRaid(ctx.Channel.Id, ctx.Message.Id);
            if (raid == null)
                return;

            lock (raid)
            {
                var participant = raid.Participants.FirstOrDefault(x => x.UserId == ctx.User.Id);
                if (participant == null)
                {
                    participant = new RaidParticipant
                    {
                        Status = status,
                        Role = RaidRole.Unknown,
                        UserId = ctx.User.Id
                    };
                    raid.Participants.Add(participant);
                    raidService.UpdateRaid(raid);
                }
                else if (participant.Status != status)
                {
                    participant.Status = status;
                    raidService.UpdateRaid(raid);
                }
            }

            Log.Debug("Updating message {0}", ctx.Message.Id);

            await ctx.Message.ModifyAsync(embed: messageBuilder.BuildEmbed(ctx.Client, raid));
        }

        /// <summary>
        /// Cleans up reactions which were set on the message while bot was offline.
        /// </summary>
        private async Task FixReactions(ReactionContext ctx)
        {
            var raid = raidService.GetRaid(ctx.Channel.Id, ctx.Message.Id);
            if (raid == null)
                return;

            var actions = new List<Action<Raid>>();
            var deletedReactions = new List<DiscordEmoji>();

            var message = await ctx.Channel.GetMessageAsync(ctx.Message.Id);
            var reactions = message.Reactions.ToArray();

            foreach (var reaction in reactions)
            {
                var emojiName = reaction.Emoji.GetDiscordName();

                var users = await ctx.Message.GetReactionsAsync(reaction.Emoji);

                foreach (var user in users)
                {
                    if (user.IsBot)
                        continue;

                    if (raid.Status == RaidStatus.Scheduled)
                    {
                        var status = emojiService.GetStatusByEmoji(emojiName);
                        if (status.HasValue)
                        {
                            actions.Add(raid =>
                            {
                                var participant = raid.Participants.FirstOrDefault(x => x.UserId == user.Id);
                                if (participant == null)
                                {
                                    participant = new RaidParticipant();
                                    participant.UserId = user.Id;
                                    participant.Role = RaidRole.Unknown;
                                    raid.Participants.Add(participant);
                                }
                                participant.Status = status.Value;
                            });
                        }

                        var role = emojiService.GetRoleByEmoji(emojiName);
                        if (role.HasValue)
                        {
                            actions.Add(raid =>
                            {
                                var participant = raid.Participants.FirstOrDefault(x => x.UserId == ctx.User.Id);
                                if (participant == null)
                                {
                                    participant = new RaidParticipant();
                                    participant.UserId = user.Id;
                                    participant.Status = ParticipationStatus.Available;
                                    raid.Participants.Add(participant);
                                }
                                participant.Role = role.Value;
                            });
                        }
                    }

                    await ctx.Message.DeleteReactionAsync(reaction.Emoji, user);
                }
            }

            lock (raid)
            {   
                foreach (var action in actions)
                {
                    action(raid);
                }
                raidService.UpdateRaid(raid);
            }

            Log.Debug("Updating message {0}", ctx.Message.Id);

            await ctx.Message.ModifyAsync(embed: messageBuilder.BuildEmbed(ctx.Client, raid));

        }

        private Task HandleUnknownEmoji(ReactionContext ctx)
        {
            return Task.CompletedTask;
        }
    }
}
