using System;
using System.IO;
using System.Net;

namespace Auth
{
    class Login
    {
        private readonly string _appUrl;
        private CookieContainer _authCookie;
        private readonly string _authServiceUrl;
        private readonly string _userName;
        private readonly string _userPassword;


        public Login(string appUrl, string userName, string userPassword)
        {
            _appUrl = appUrl;
            _authServiceUrl = _appUrl + @"/ServiceModel/AuthService.svc/Login";
            _userName = userName;
            _userPassword = userPassword;
        }

        public void TryLogin()
        {
            string requestData = @"{""UserName"":""" + _userName + @""", ""UserPassword"":""" + _userPassword + @"""}";

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(_authServiceUrl);
            request.ContentType = "application/json";
            request.Method = "POST";
            request.KeepAlive = true;

            if (!string.IsNullOrEmpty(requestData))
            {
                using (var requestStream = request.GetRequestStream())
                {
                    using (StreamWriter writer = new StreamWriter(requestStream))
                    {
                        writer.Write(requestData);
                    }
                }
            }

            _authCookie = new CookieContainer();
            request.CookieContainer = _authCookie;
            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            {
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    using (var reader = new StreamReader(response.GetResponseStream()))
                    {
                        var responseMessage = reader.ReadToEnd();
                        Console.WriteLine(responseMessage);
                    }
                }
            }
        }

        public void GetWTLCallCdr()
        {
            string requestString = @"https://eva.terrasoft.ua/0/odata/WTLCallCdr?$count=true&$filter=(WTLDirection eq 'inbound' or WTLDirection eq 'outbound') and WTLCreatedTime ge 2022-08-24T00:00:00z and WTLCreatedTime le 2022-08-24T23:59:59z";
            HttpWebRequest request = HttpWebRequest.Create(requestString) as HttpWebRequest;
            request.Method = "GET";
            request.CookieContainer = _authCookie;
            using (var response = request.GetResponse())
            {
                using (var reader = new StreamReader(response.GetResponseStream()))
                {
                    string responseText = reader.ReadToEnd();
                    Console.WriteLine(responseText);
                }
            }
        }
    }
}