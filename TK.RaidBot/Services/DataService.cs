using System;
using System.Linq;
using MongoDB.Bson;
using MongoDB.Driver;
using TK.RaidBot.Model.Data;

namespace TK.RaidBot.Services
{
    public class DataService
    {
        private const string DatabaseUrlVariableName = "DATABASE_URL";

        private const string DatabaseName = "raidBotDB";
        private const string RaidCollectionName = "raids";

        private readonly MongoClient dbClient;
        private readonly object messageUpdateLock = new object();

        public DataService()
        {
            var databaseUrl = Environment.GetEnvironmentVariable(DatabaseUrlVariableName);
            dbClient = new MongoClient(databaseUrl);
        }

        public Raid AddRaid(Raid raid)
        {
            raid.Timestamp = DateTime.Now;

            var raids = dbClient.GetDatabase(DatabaseName).GetCollection<Raid>(RaidCollectionName);
            raids.InsertOne(raid);

            return raid;
        }

        public bool DeleteRaid(ulong channelId, ulong messageId)
        {
            var filter = CreateByMessageFilter(channelId, messageId);

            var raids = dbClient.GetDatabase(DatabaseName).GetCollection<Raid>(RaidCollectionName);
            var result = raids.DeleteOne(filter);

            return result.DeletedCount > 0;
        }

        public Raid GetRaidById(ObjectId id)
        {
            var filter = CreateByIdFilter(id);
            var raids = dbClient.GetDatabase(DatabaseName).GetCollection<Raid>(RaidCollectionName);
            return raids.Find(filter).FirstOrDefault();
        }

        public Raid GetRaidByMessage(ulong channelId, ulong messageId)
        {
            var filter = CreateByMessageFilter(channelId, messageId);
            var raids = dbClient.GetDatabase(DatabaseName).GetCollection<Raid>(RaidCollectionName);
            return raids.Find(filter).FirstOrDefault();
        }

        public void SetRaidStatus(ObjectId raidId, RaidStatus status)
        {
            UpdateRaid(raidId, raid =>
            {
                raid.Status = status;
                return raid;
            });
        }

        public void SetParticipantStatus(ObjectId raidId, ulong userId, ParticipationStatus status)
        {
            UpdateRaid(raidId, raid =>
            {
                var participant = raid.Participants.FirstOrDefault(x => x.UserId == userId);
                if (participant == null)
                {
                    participant = new RaidParticipant { UserId = userId, Role = RaidRole.Unknown };
                    raid.Participants.Add(participant);
                }

                participant.Status = status;
                raid.Timestamp = DateTime.Now;

                return raid;
            });
        }

        public void SetParticipantRole(ObjectId raidId, ulong userId, RaidRole role)
        {
            UpdateRaid(raidId, raid =>
            {
                var participant = raid.Participants.FirstOrDefault(x => x.UserId == userId);
                if (participant == null)
                {
                    participant = new RaidParticipant { UserId = userId, Status = ParticipationStatus.Available };
                    raid.Participants.Add(participant);
                }

                participant.Role = role;
                raid.Timestamp = DateTime.Now;

                return raid;
            });
        }

        private void UpdateRaid(ObjectId raidId, Func<Raid, Raid> updateAction)
        {
            var filter = CreateByIdFilter(raidId);

            // TODO: lock a single document somehow?
            lock (messageUpdateLock)
            {
                using (var session = dbClient.StartSession())
                {
                    var raids = dbClient.GetDatabase(DatabaseName).GetCollection<Raid>(RaidCollectionName);

                    session.StartTransaction();
                    try
                    {
                        var raid = raids.Find(filter).FirstOrDefault();
                        if (raid == null)
                            return;

                        raid = updateAction(raid);
                        raids.ReplaceOne(filter, raid);
                    }
                    catch (Exception)
                    {
                        session.AbortTransaction();
                        throw;
                    }
                }
            }
        }

        private static FilterDefinition<Raid> CreateByIdFilter(ObjectId id)
        {
            return Builders<Raid>.Filter.Eq(raid => raid.Id, id);
        }

        private static FilterDefinition<Raid> CreateByMessageFilter(ulong channelId, ulong messageId)
        {
            var builder = Builders<Raid>.Filter;
            return builder.And(
                builder.Eq(raid => raid.MessageId, messageId),
                builder.Eq(raid => raid.ChannelId, channelId));
        }
    }
}
