using System;
using System.Linq;
using MongoDB.Bson;
using MongoDB.Driver;
using TK.RaidBot.Config;
using TK.RaidBot.Model.Raids;

namespace TK.RaidBot.Services
{
    public class DataService
    {
        private const string RaidCollectionName = "raids";

        private readonly MongoClient dbClient;
        private readonly string dbName;

        public DataService(DatabaseConfig config)
        {
            dbClient = new MongoClient(config.DatabaseUrl);
            dbName = config.DatabaseName;
        }

        public Raid AddRaid(Raid raid)
        {
            var now = DateTime.Now;
            raid.Timestamp = now;
            raid.CreationDate = now;

            var raids = dbClient.GetDatabase(dbName).GetCollection<Raid>(RaidCollectionName);
            raids.InsertOne(raid);

            return raid;
        }

        public bool DeleteRaid(ulong channelId, ulong messageId)
        {
            var filter = CreateByMessageFilter(channelId, messageId);
            var collection = dbClient.GetDatabase(dbName).GetCollection<Raid>(RaidCollectionName);
            var result = collection.DeleteOne(filter);
            return result.DeletedCount > 0;
        }

        public Raid GetRaid(ObjectId id)
        {
            var filter = CreateByIdFilter(id);
            var collection = dbClient.GetDatabase(dbName).GetCollection<Raid>(RaidCollectionName);
            return collection.Find(filter).FirstOrDefault();
        }

        public Raid GetRaid(ulong channelId, ulong messageId)
        {
            var filter = CreateByMessageFilter(channelId, messageId);
            var collection = dbClient.GetDatabase(dbName).GetCollection<Raid>(RaidCollectionName);
            return collection.Find(filter).FirstOrDefault();
        }

        public Raid UpdateRaid(Raid raid)
        {
            raid.Timestamp = DateTime.Now;

            var filter = CreateByIdFilter(raid.Id);
            var collection = dbClient.GetDatabase(dbName).GetCollection<Raid>(RaidCollectionName);
            collection.ReplaceOne(filter, raid);

            return raid;
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
