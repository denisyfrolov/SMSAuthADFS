using Microsoft.IdentityServer.Web.Authentication.External;
using System;
using System.IO;
using System.Net;
using System.Runtime.Serialization.Json;
using System.Text;
using Claim = System.Security.Claims.Claim;

namespace SMSAuthADFS
{
    class SmsAdapter : IAuthenticationAdapter
    {
        private static string UPN = string.Empty;
        private static string SMSCURL = string.Empty;
        private static string SMSCUserName = string.Empty;
        private static string SMSCPassword = string.Empty;
        private static string SMSCSender = string.Empty;
        private static string PIN = string.Empty;
        private static string TelNumber = string.Empty;
        private static string OTPCode = string.Empty;
        private static string PinAnswer = string.Empty;
        private static string OTPCodeAnswer = string.Empty;
        private static string LDAPdomainName = string.Empty;
        private static string PinAttributeName = string.Empty;
        private static string TelNumberAttributeName = string.Empty;
        private static string SmsTemplate = string.Empty;
        private static SmsMessage SmsMessage;
        private static int OTPCodeLength = 0;
        private static Debug Debug = null;
        private static bool DebugEnabled = false;

        public IAuthenticationAdapterMetadata Metadata
        {
            //get { return new <instance of IAuthenticationAdapterMetadata derived class>; }
            get { return new SmsMetadata(); }
        }

        public IAdapterPresentation BeginAuthentication(Claim identityClaim, HttpListenerRequest request, IAuthenticationContext authContext)
        {
            UPN = identityClaim.Value;
            PIN = AuthHelper.GetPinByUpn(UPN, LDAPdomainName, PinAttributeName);
            TelNumber = AuthHelper.GetTelNumberByUpn(UPN, LDAPdomainName, TelNumberAttributeName);
            OTPCode = AuthHelper.GenerateOTPCode(OTPCodeLength);

            SmsMessage = new SmsMessage()
            {
                Address = new string[] { $"tel:{TelNumber}" },
                SenderAddress = SMSCSender,
                OutboundSMSTextMessage = new OutboundSMSTextMessage()
                {
                    Message = SmsTemplate.Replace("%OTPCODE%", OTPCode).Replace("%UPN%", UPN)
                }
            };

            _ = AuthHelper.SendSms(SmsMessage, SMSCUserName, SMSCPassword, SMSCURL);

            PinAnswer = string.Empty;
            OTPCodeAnswer = string.Empty;

            Debug = new Debug() {
                Enabled = DebugEnabled,
                Caller = "BeginAuthentication",
                UPN = UPN,
                PIN = PIN,
                TelNumber = TelNumber,
                OTPCode = OTPCode
            };

            //return new instance of IAdapterPresentationForm derived class
            return new SmsPresentationFormPin(Debug);
        }

        public bool IsAvailableForUser(Claim identityClaim, IAuthenticationContext authContext)
        {
            string UPN = identityClaim.Value;
            string PIN = AuthHelper.GetPinByUpn(UPN, LDAPdomainName, PinAttributeName);
            string TelNumber = AuthHelper.GetTelNumberByUpn(UPN, LDAPdomainName, TelNumberAttributeName);

            if (AuthHelper.ValidateUpn(UPN) && AuthHelper.ValidatePin(PIN) && AuthHelper.ValidateTelNumber(TelNumber))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public void OnAuthenticationPipelineLoad(IAuthenticationMethodConfigData ConfigData)
        {
            //this is where AD FS passes us the Config data, if such data was supplied at registration of the adapter
            if (ConfigData != null)
            {
                if (ConfigData.Data != null)
                {
                    using (StreamReader reader = new StreamReader(ConfigData.Data, Encoding.UTF8))
                    {
                        //Config should be in a json format, and needs to be registered with the 
                        //-ConfigurationFilePath parameter when registering the MFA Adapter (Register-AdfsAuthenticationProvider cmdlet)
                        try
                        {
                            string Config = reader.ReadToEnd();
                            DataContractJsonSerializer JsonSer = new DataContractJsonSerializer(typeof(SmsAdapterConfig));
                            MemoryStream MemStrConfig = new MemoryStream(UTF8Encoding.UTF8.GetBytes(Config));
                            SmsAdapterConfig MFAConfig = (SmsAdapterConfig)JsonSer.ReadObject(MemStrConfig);

                            SMSCURL = MFAConfig.SMSCURL;
                            SMSCUserName = MFAConfig.SMSCUserName;
                            SMSCPassword = MFAConfig.SMSCPassword;
                            SMSCSender = MFAConfig.SMSCSender;
                            LDAPdomainName = MFAConfig.LDAPdomainName;
                            PinAttributeName = MFAConfig.PinAttributeName;
                            OTPCodeLength = MFAConfig.OTPCodeLength;
                            TelNumberAttributeName = MFAConfig.TelNumberAttributeName;
                            SmsTemplate = MFAConfig.SmsTemplate;
                            DebugEnabled = MFAConfig.DebugEnabled;
                        }
                        catch
                        {
                            throw new ArgumentException();
                        }
                    }
                }
            }
            else
            {
                throw new ArgumentNullException();
            }
        }

        public void OnAuthenticationPipelineUnload()
        {

        }

        public IAdapterPresentation OnError(HttpListenerRequest request, ExternalAuthenticationException ex)
        {
            Debug.Caller = "OnError";

            //return new instance of IAdapterPresentationForm derived class
            return new SmsPresentationFormPin(Debug);
        }

        static bool ValidateProofPIN(IProofData proofData, IAuthenticationContext authContext)
        {
            if (proofData.Properties.ContainsKey("PIN"))
            {
                PinAnswer = (string)proofData.Properties["PIN"];
                Debug.PinAnswer = PinAnswer;
            }

            if (!String.IsNullOrEmpty(PinAnswer) && PinAnswer == PIN)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        static bool ValidateProofOTPCode(IProofData proofData, IAuthenticationContext authContext)
        {
            if(proofData.Properties.ContainsKey("OTPCode"))
            {
                OTPCodeAnswer = (string)proofData.Properties["OTPCode"];
                Debug.OTPCodeAnswer = OTPCodeAnswer;
            }

            if (!String.IsNullOrEmpty(OTPCodeAnswer) && OTPCodeAnswer == OTPCode)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public IAdapterPresentation TryEndAuthentication(IAuthenticationContext authContext, IProofData proofData, HttpListenerRequest request, out Claim[] outgoingClaims)
        {
            Debug.Caller = "TryEndAuthentication";
            outgoingClaims = new Claim[0];

            if (proofData == null || proofData.Properties == null || (!proofData.Properties.ContainsKey("PIN") && !proofData.Properties.ContainsKey("OTPCode")))
            {
                throw new ExternalAuthenticationException("Error - no answer found", authContext);
            }

            if(!ValidateProofPIN(proofData, authContext))
            {
                //authentication not complete - return new instance of IAdapterPresentationForm derived class
                return new SmsPresentationFormPin(Debug);
            }

            if (!ValidateProofOTPCode(proofData, authContext))
            {
                //authentication not complete - return new instance of IAdapterPresentationForm derived class
                return new SmsPresentationFormOtp(Debug);
            }

            outgoingClaims = new[]
            {
                // Return the required authentication method claim, indicating the particulate authentication method used.
                new Claim( "http://schemas.microsoft.com/ws/2008/06/identity/claims/authenticationmethod", "http://example.com/myauthenticationmethod1" )
            };
            
            return null;
        }
    }
}
