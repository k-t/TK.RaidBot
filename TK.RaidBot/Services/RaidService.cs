using System;
using System.Collections.Generic;
using TK.RaidBot.Model.Raids;

namespace TK.RaidBot.Services
{
    public class RaidService
    {
        private readonly DataService dataService;
        private readonly Dictionary<(ulong, ulong), Raid> cache;

        public RaidService(DataService dataService)
        {
            this.dataService = dataService;
            this.cache = new Dictionary<(ulong, ulong), Raid>();
        }

        public void AddRaid(Raid raid)
        {
            lock (cache)
            {
                raid = dataService.AddRaid(raid);
                cache[(raid.ChannelId, raid.MessageId)] = raid;
            }
        }

        public Raid GetRaid(ulong channelId, ulong messageId)
        {
            var key = (channelId, messageId);

            lock (cache)
            {
                if (cache.TryGetValue(key, out Raid raid))
                    return raid;

                raid = dataService.GetRaid(channelId, messageId);
                cache[key] = raid;

                return raid;
            }
        }

        public bool DeleteRaid(ulong channelId, ulong messageId)
        {
            lock (cache)
            {
                var key = (channelId, messageId);
                cache.Remove(key);
                return dataService.DeleteRaid(channelId, messageId);
            }
        }

        public void UpdateRaid(Raid raid)
        {
            dataService.UpdateRaid(raid);
        }
    }
}
