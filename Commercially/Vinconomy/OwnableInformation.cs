namespace Commercially.Vinconomy
{
    public class OwnableShopInformation
    {
        public int ID { get; internal set; } = -1;
        public string Name { get; internal set; }
        public string ShortDescription { get; set; }
        public string Description { get; set; }
        public string WebHook { get; set; }
    }
}