using System;
using System.Net;
using System.Windows;
using System.IO;
using System.Text;
using Newtonsoft.Json.Linq;
using System.Threading;
using System.IO.IsolatedStorage;
using Resources;

namespace MusicMeTube.Library
{
    public delegate void TokenReceivedEventHandler(object sender,APICompletedEventArgs e);

    public class Authentication : WebMethod
    {
        private string ClientId = "802090968929.apps.googleusercontent.com";
        private string ClientSecret = "17KfsrNBLQhc4NKmg2c4hL50";
        private string AUTHORIZATION_SERVER = "https://accounts.google.com/o/oauth2/token";
        private string RedirectURI = "http://localhost";
        private string Scope = "https://gdata.youtube.com";
        private String ResponseType = "code";
        public String AuthCode;
        
        public event TokenReceivedEventHandler NewTokenReceived;
        
        protected virtual void RaiseNewToken(APICompletedEventArgs e)
        {
            if (NewTokenReceived != null)
                NewTokenReceived(this, e);
        }

        public void GetAccessTokenFromGoogle()
        {
            string postData = "code=" + AuthCode +
                "&client_id=" + ClientId +
                "&client_secret=" + ClientSecret +
                "&redirect_uri=" + RedirectURI +
                "&grant_type=authorization_code";
            POST(AUTHORIZATION_SERVER, postData);
 
        }

        public string getAuthURI()
        {
            String AuthURI = "https://accounts.google.com/o/oauth2/auth?&client_id=" + ClientId
                               + "&redirect_uri=" + RedirectURI
                               + "&scope=" + Scope
                               + "&response_type=" + ResponseType;
            return AuthURI;
        }

        public void RefreshToken()
        {
            string refresh_token;
            IsolatedStorageSettings.ApplicationSettings.TryGetValue("refresh_token", out refresh_token);
            if (refresh_token != null && refresh_token != string.Empty)
            {
                string post = "client_id=" + ClientId + "&" +
                        "client_secret=" + ClientSecret + "&" +
                        "refresh_token=" + refresh_token + "&" +
                        "grant_type=refresh_token";
                POST(AUTHORIZATION_SERVER, post);
            }
            
        }

        public override void GET_Method_CallBack(IAsyncResult res)
        {
            throw new NotImplementedException();
        }

        public override void POST_Method_CallBack(IAsyncResult res)
        {
            try
            {
                HttpWebRequest wr = (HttpWebRequest)res.AsyncState;
                HttpWebResponse response = (HttpWebResponse)wr.EndGetResponse(res);
                StreamReader sr = new StreamReader(response.GetResponseStream());
                JObject api_response = JObject.Parse(sr.ReadToEnd());

                IsolatedStorageSettings.ApplicationSettings["request_time"] = DateTime.Now;
                IsolatedStorageSettings.ApplicationSettings["access_token"] = (string)api_response["access_token"];
                IsolatedStorageSettings.ApplicationSettings["token_type"] = (string)api_response["token_type"];
                IsolatedStorageSettings.ApplicationSettings["expires_in"] = (int)api_response["expires_in"];
                JToken refresh_token;
                api_response.TryGetValue("refresh_token", out refresh_token);
                if (refresh_token != null)
                    IsolatedStorageSettings.ApplicationSettings["refresh_token"] = (string)refresh_token;
                IsolatedStorageSettings.ApplicationSettings.Save();
                RaiseNewToken(new APICompletedEventArgs(api_response.ToString()));
            }
            catch (Exception e)
            {
                Resources.ErrorLogging.Log(this.GetType().ToString(), e.Message, "Authentication", string.Empty);
                System.Windows.Deployment.Current.Dispatcher.BeginInvoke(() => {
                    MessageBox.Show("Authentication Error");
                });
            }
        }
    }
}
