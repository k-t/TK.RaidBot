using System;
using System.Text;
namespace TK.RaidBot.Model.Raids
{
    public class RaidGroupEmbedBuilder
    {
        private readonly RaidGroup group;
        private readonly StringBuilder valueBuilder;
        private int count;

        public RaidGroupEmbedBuilder(RaidGroup group)
        {
            this.group = group;
            this.valueBuilder = new StringBuilder();
        }

        public RaidGroup Group
        {
            get { return group; }
        }

        public int Count
        {
            get { return count; }
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

        public string GetParticipants()
        {
            var value = valueBuilder.ToString();

            return !string.IsNullOrEmpty(value) ? value : "\u200b";
        }
    }
}
