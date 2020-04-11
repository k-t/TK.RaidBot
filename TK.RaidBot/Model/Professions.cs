using System;
using System.Collections.Generic;
using System.Linq;

namespace TK.RaidBot.Model
{
    public static class Professions
    {
        static Professions()
        {
            All = new List<Profession>()
            {
                Guardian,
                Dragonhunter,
                Firebrand,
                Warrior,
                Berserker,
                Spellbreaker,
                Engineer,
                Scrapper,
                Holosmith,
                Ranger,
                Druid,
                Soulbeast,
                Thief,
                Daredevil,
                Deadeye,
                Elementalist,
                Tempest,
                Weaver,
                Mesmer,
                Chronomancer,
                Mirage,
                Necromancer,
                Reaper,
                Scourge,
                Revenant,
                Herald,
                Renegade,
                Unknown
            };
        }

        public static IEnumerable<Profession> All { get; }

        public static Profession Unknown { get; } = new Profession(0, "Unknown", ":question:");

        #region Core

        public static Profession Guardian { get; } = new Profession(1, "Guardian", ":Guardian:");

        public static Profession Warrior { get; } = new Profession(2, "Warrior", ":Warrior:");

        public static Profession Engineer { get; } = new Profession(3, "Engineer", ":Engineer:");

        public static Profession Ranger { get; } = new Profession(4, "Ranger", ":Ranger:");

        public static Profession Thief { get; } = new Profession(5, "Thief", ":Thief:");

        public static Profession Elementalist { get; } = new Profession(6, "Elementalist", ":Elementalist:");

        public static Profession Mesmer { get; } = new Profession(7, "Mesmer", ":Mesmer:");

        public static Profession Necromancer { get; } = new Profession(8, "Necromancer", ":Necromancer:");

        public static Profession Revenant { get; } = new Profession(9, "Revenant", ":Revenant:");

        #endregion

        #region HoT specs

        public static Profession Dragonhunter { get; } = new Profession(101, "Dragonhunter", ":Dragonhunter:");

        public static Profession Berserker { get; } = new Profession(102, "Berserker", ":Berserker:");

        public static Profession Scrapper { get; } = new Profession(103, "Scrapper", ":Scrapper:");

        public static Profession Druid { get; } = new Profession(104, "Druid", ":Druid:");

        public static Profession Daredevil { get; } = new Profession(105, "Daredevil", ":Daredevil:");

        public static Profession Tempest { get; } = new Profession(106, "Tempest", ":Tempest:");

        public static Profession Chronomancer { get; } = new Profession(107, "Chronomancer", ":Chronomancer:");

        public static Profession Reaper { get; } = new Profession(108, "Reaper", ":Reaper:");

        public static Profession Herald { get; } = new Profession(109, "Herald", ":Herald:");

        #endregion

        #region PoF specs

        public static Profession Firebrand { get; } = new Profession(201, "Firebrand", ":Firebrand:");

        public static Profession Spellbreaker { get; } = new Profession(202, "Spellbreaker", ":Spellbreaker:");

        public static Profession Holosmith { get; } = new Profession(203, "Holosmith", ":Holosmith:");

        public static Profession Soulbeast { get; } = new Profession(204, "Soulbeast", ":Soulbeast:");

        public static Profession Deadeye { get; } = new Profession(205, "Deadeye", ":Deadeye:");

        public static Profession Weaver { get; } = new Profession(206, "Weaver", ":Weaver:");

        public static Profession Mirage { get; } = new Profession(207, "Mirage", ":Mirage:");

        public static Profession Scourge { get; } = new Profession(208, "Scourge", ":Scourge:");

        public static Profession Renegade { get; } = new Profession(209, "Renegade", ":Renegade:");

        #endregion

        public static Profession GetById(int id)
        {
            return All.FirstOrDefault(x => x.Id == id);
        }

        public static Profession GetByName(string name)
        {
            return All.FirstOrDefault(x => x.Name == name);
        }

        public static Profession GetByEmojiName(string emojiName)
        {
            return All.FirstOrDefault(x => x.EmojiName == emojiName);
        }
    }
}
