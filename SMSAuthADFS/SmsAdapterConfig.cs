using System.Runtime.Serialization;

namespace SMSAuthADFS
{
    //Helper class for deserializing configuration data (located in external json-formatted file)
    [DataContract]
    class SmsAdapterConfig
    {
        [DataMember]
        public string SMSCURL = string.Empty;
        [DataMember]
        public string SMSCUserName = string.Empty;
        [DataMember]
        public string SMSCPassword = string.Empty;
        [DataMember]
        public string SMSCSender = string.Empty;
        [DataMember]
        public string LDAPdomainName = string.Empty;
        [DataMember]
        public string PinAttributeName = string.Empty;
        [DataMember]
        public int OTPCodeLength = 0;
        [DataMember]
        public string TelNumberAttributeName = string.Empty;
        [DataMember]
        public string SmsTemplate = string.Empty;
        [DataMember]
        public bool DebugEnabled = false;
    }
}
