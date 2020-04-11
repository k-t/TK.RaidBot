namespace TK.RaidBot.Model.Raids.Templates
{
    public static class RaidTemplates
    {
        public static IRaidTemplate Wvw { get; } = new WvwTemplate();

        public static IRaidTemplate Pve { get; } = new PveTemplate();

        public static IRaidTemplate GetByCode(string templateCode)
        {
            if (Wvw.Code == templateCode)
                return Wvw;

            if (Pve.Code == templateCode)
                return Pve;

            return null;
        }
    }
}
