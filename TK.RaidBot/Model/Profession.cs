namespace TK.RaidBot.Model
{
    public class Profession
    {
        public Profession()
        {
        }

        public Profession(int id, string name, string emojiName)
        {
            Id = id;
            Name = name;
            EmojiName = emojiName;
        }

        public int Id { get; set; }

        public string Name { get; set; }

        public string EmojiName { get; set; }
    }
}
