using System.Runtime.Serialization;

namespace SMSAuthADFS
{
    [DataContract]
    class SmsMessage
    {
        [DataMember(Name = "address")]
        public string[] Address;
        [DataMember(Name = "senderAddress")]
        public string SenderAddress = string.Empty;
        [DataMember(Name = "outboundSMSTextMessage")]
        public OutboundSMSTextMessage OutboundSMSTextMessage;
    }

    [DataContract]
    class OutboundSMSTextMessage
    {
        [DataMember(Name = "message")]
        public string Message = string.Empty;
    }
}
