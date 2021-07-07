using Newtonsoft.Json;
using TakeToTalk.Enumeradores.Hub;

namespace TakeToTalk.Hub.Protocolo
{
    public class Message
    {
        public string Sender { get; set; }
        public EnumMessageDestinyType DestinyType { get; set; }
        public EnumMessagePrivacy Privacy { get; set; }
        public EnumMessageActions Action { get; set; }
        public string Room { get; set; }
        public string User { get; set; }
        public object Data { get; set; }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}
