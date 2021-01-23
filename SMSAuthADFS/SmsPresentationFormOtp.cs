using Microsoft.IdentityServer.Web.Authentication.External;

namespace SMSAuthADFS
{
    class SmsPresentationFormOtp : IAdapterPresentationForm
    {
        private Debug Debug = null;

        public SmsPresentationFormOtp()
        {
        }

        public SmsPresentationFormOtp(Debug Debug)
        {
            this.Debug = Debug;
        }

        /// Returns the HTML Form fragment that contains the adapter user interface. This data will be included in the web page that is presented
        /// to the cient.
        public string GetFormHtml(int lcid)
        {
            string htmlTemplate = Resources.MfaFormHtmlOtp; // Resxfilename.resourcename

            string html = htmlTemplate
                    .Replace("DEBUG:Enabled", Debug.Enabled.ToString().ToLower());

            if (Debug.Enabled)
            {
                html = html
                    .Replace("DEBUG:Enabled", Debug.Enabled.ToString().ToLower())
                    .Replace("DEBUG:Caller", Debug.Caller)
                    .Replace("DEBUG:UPN", Debug.UPN)
                    .Replace("DEBUG:PIN", Debug.PIN)
                    .Replace("DEBUG:TelNumber", Debug.TelNumber)
                    .Replace("DEBUG:PinAnswer", Debug.PinAnswer)
                    .Replace("DEBUG:OTPCodeAnswer", Debug.OTPCodeAnswer)
                    .Replace("DEBUG:OTPCode", Debug.OTPCode)
                    ;
            }

            return html;
        }

        /// Return any external resources, ie references to libraries etc., that should be included in
        /// the HEAD section of the presentation form html.
        public string GetFormPreRenderHtml(int lcid)
        {
            return null;
        }

        //returns the title string for the web page which presents the HTML form content to the end user
        public string GetPageTitle(int lcid)
        {
            return "MFA Adapter";
        }
    }
}
