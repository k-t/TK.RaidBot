using System;
using System.Collections.Generic;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace TK.RaidBot.Model.Data
{
    public class Raid
    {
        [BsonId]
        public ObjectId Id { get; set; }

        public ulong MessageId { get; set; }

        public ulong GuildId { get; set; }

        public ulong ChannelId { get; set; }

        public ulong OwnerId { get; set; }

        public string OwnerDisplayName { get; set; }

        public DateTime CreationDate { get; set; }

        public DateTime Timestamp { get; set; }

        public string Title { get; set; }

        public DateTime Date { get; set; }

        public RaidStatus Status { get; set; }

        public List<RaidParticipant> Participants { get; set; }
    }
}
