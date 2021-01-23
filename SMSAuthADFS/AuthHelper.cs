using System;
using System.IO;
using System.Text;
using System.DirectoryServices;
using System.Text.RegularExpressions;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Runtime.Serialization.Json;

namespace SMSAuthADFS
{
    static class AuthHelper
    {
        public static bool ValidatePin(string PIN)
        {
            return !String.IsNullOrEmpty(PIN);
        }

        public static string GetPinByUpn(string UPN, string LDAPdomainName, string PinAttributeName)
        {
            return GetAttributeValueByName(UPN, LDAPdomainName, PinAttributeName);
        }

        public static bool ValidateTelNumber(string TelNumber)
        {
            return (!String.IsNullOrEmpty(TelNumber) && Regex.Match(TelNumber, @"^(\+[7]|[8])(\d{10})$").Success);
        }

        public static string GetTelNumberByUpn(string UPN, string LDAPdomainName, string TelNumberAttributeName)
        {
            return GetAttributeValueByName(UPN, LDAPdomainName, TelNumberAttributeName);
        }

        public static bool ValidateUpn(string UPN)
        {
            return !String.IsNullOrEmpty(UPN);
        }

        public static string GenerateOTPCode(int OTPCodeLength = 5)
        {
            Random rand = new Random();
            StringBuilder OTPCode = new StringBuilder();

            for (int ctr = 0; ctr < OTPCodeLength; ctr++)
            {
                OTPCode.Append(rand.Next(0, 9));
            }

            return OTPCode.ToString();
        }

        public static async Task<bool> SendSms(SmsMessage SmsMessage, string UserName, string Password, string Url)
        {
            bool IsSuccess;
            string jsonMessage = string.Empty;

            using (HttpClient client = new HttpClient())
            {
                byte[] AuthToken = Encoding.ASCII.GetBytes($"{UserName}:{Password}");
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(AuthToken));

                DataContractJsonSerializer JsonSer = new DataContractJsonSerializer(typeof(SmsMessage));
                using (MemoryStream MemStrSmsMessage = new MemoryStream())
                {
                    JsonSer.WriteObject(MemStrSmsMessage, SmsMessage);
                    MemStrSmsMessage.Position = 0;
                    StreamReader sr = new StreamReader(MemStrSmsMessage);
                    jsonMessage = sr.ReadToEnd();
                    sr.Close();
                    MemStrSmsMessage.Close();
                }

                StringContent sContent = new StringContent(jsonMessage, Encoding.UTF8, "application/json");

                HttpResponseMessage response = await client.PostAsync(Url, sContent);
                IsSuccess = response.EnsureSuccessStatusCode().IsSuccessStatusCode;
            }
                
            return IsSuccess;
        }

        private static string GetAttributeValueByName(string UPN, string LDAPdomainName, string AttributeName)
        {
            DirectoryEntry dEntry = new DirectoryEntry("LDAP://" + LDAPdomainName);
            DirectorySearcher dSearch = new DirectorySearcher(dEntry);
            dSearch.Filter = "(&(objectClass=user)(userPrincipalName=" + UPN + "))";
            SearchResult sResultSet = dSearch.FindOne();

            if (sResultSet.Properties.Contains(AttributeName))
            {
                return sResultSet.Properties[AttributeName][0].ToString();
            }
            else
            {
                return string.Empty;
            }
        }
    }
}
