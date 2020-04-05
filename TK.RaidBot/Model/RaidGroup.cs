using System;
using System.Text;
using TK.RaidBot.Model.Data;

namespace TK.RaidBot.Model
{
    public class RaidGroup
    {
        private readonly string title;
        private readonly Func<RaidParticipant, bool> filter;
        private readonly bool displayRole;
        private readonly bool inline;
        private readonly StringBuilder valueBuilder;
        private int count;

        public RaidGroup(string title, Func<RaidParticipant, bool> filter,
            bool displayRole = false,
            bool inline = false)
        {
            this.title = title;
            this.filter = filter;
            this.displayRole = displayRole;
            this.inline = inline;
            valueBuilder = new StringBuilder();
        }

        public bool DisplayRole
        {
            get { return displayRole; }
        }

        public bool Inline
        {
            get { return inline; }
        }

        public void AddParticipant(string name, string role = null)
        {
            if (!string.IsNullOrEmpty(name))
            {
                if (valueBuilder.Length != 0)
                    valueBuilder.Append('\n');

                if (!string.IsNullOrEmpty(role))
                {
                    valueBuilder.Append(role);
                    valueBuilder.Append(' ');
                }

                valueBuilder.Append(name);
            }

            count++;
        }

        public bool Includes(RaidParticipant participant)
        {
            return filter(participant);
        }

        public string GetTitle()
        {
            return $"**{title}** ({count})";
        }

        public string GetParticipants()
        {
            var value = valueBuilder.ToString();

            return !string.IsNullOrEmpty(value) ? value : "\u200b";
        }
    }
}
