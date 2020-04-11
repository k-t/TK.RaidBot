using System.Collections.Generic;

namespace TK.RaidBot.Model.Raids.Templates
{
    public interface IRaidTemplate
    {
        string Code { get; }

        IEnumerable<RaidGroup> Groups { get; }

        bool AllowsProfession(Profession profession);
    }
}
