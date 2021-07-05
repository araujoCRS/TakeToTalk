using TakeToTalk.Enumeradores.Hub;

namespace TakeToTalk.Hub.Protocolo
{
    public class Message
    {
        public EnumMessageDestiny Destiny { get; set; }
        public EnumMessagePrivacy Privacy { get; set; }
        public EnumMessageActions Action { get; set; }
        public string Value { get; set; }
        public string Room { get; set; }
        public string DestinyName { get; set; }
        public dynamic Header { get; set; }
    }
}
