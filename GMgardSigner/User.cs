using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace GMgardSigner
{
    public class User
    {

        #region [Url Consts]
        const string UrlHost = "https://gmgard.com";
        const string UrlLogin = "/Account/Login";
        const string UrlCaptcha = "/Captcha/CaptchaImage";
        const string UrlLoginPost = "/Account/Login";
        const string UrlSignInPost = "/Home/PunchIn";
        //const string UrlRatePost = "/Blog/Rate";
        #endregion

        public string Username { get; set; }
        public string Password { get; set; }

        private CookieContainer _cookies;
        public CookieContainer Cookies => _cookies;

        protected HttpClient Client = null;
        protected HttpResponseMessage LastResponse = null;

        protected User() { }

        public static User Create(string username, string password, CookieContainer cookies = null)
        {
            cookies = cookies ?? new CookieContainer();
            var handler = new HttpClientHandler() { CookieContainer = cookies };
            var user = new User() { Username = username, Password = password };
            user.Client = new HttpClient(handler) { BaseAddress = new Uri(UrlHost), Timeout = TimeSpan.FromSeconds(20) };
            user._cookies = cookies;
            return user;
        }

        public async Task<bool> CheckLoginAsync()
        {
            var doc = await GetDocumentAsync(UrlLogin);
            return doc.GetElementbyId("loginLink") == null;
        }

        public async Task LoginAsync()
        {
            HtmlDocument doc;
            if (LastResponse?.RequestMessage.RequestUri.AbsolutePath == UrlLogin)
                doc = await GetDocumentAsync(LastResponse);
            else
                doc = await GetDocumentAsync(UrlLogin);

            // token
            var token = doc.DocumentNode.SelectSingleNode("//input[@name=\"__RequestVerificationToken\"]")
                ?.GetAttributeValue("value", null)
                ?? throw new GMgardException("Cannot parse token");

            // captcha
            int captchaCode;
            {
                var stream = await Client.GetStreamAsync(UrlCaptcha);
                var captchaImg = new System.Drawing.Bitmap(stream);
                captchaCode = Captcha.Read(captchaImg, out var captchaText);
                System.Diagnostics.Trace.WriteLine($"Captcha: {captchaText} => {captchaCode}");
            }

            var postData = BuildPostData(
                "UserName", Username,
                "Password", Password,
                "Captcha", captchaCode.ToString(),
                "RememberMe", "true",
                "__RequestVerificationToken", token);
            var response = await Client.PostAsync(UrlLoginPost, postData);
            if (response.StatusCode == HttpStatusCode.OK)
            {
                if (response.RequestMessage.RequestUri.AbsolutePath == UrlLogin)
                {
                    throw new GMgardException("user/pass/captcha error");
                }
                else
                {
                    LastResponse = response;
                }
            }
            else
            {
                throw new HttpRequestException("POST response code: " + (int)response.StatusCode);
            }
        }

        public async Task ParseUserInfoAsync()
        {
            throw new NotImplementedException("ParseUserInfo not implemented yet.");
        }

        public async Task<SignInResult> SignInAsync()
        {
            HtmlDocument doc = null;
            if (LastResponse?.RequestMessage.RequestUri.AbsolutePath == "/")
                doc = await GetDocumentAsync(LastResponse);
            else
                doc = await GetDocumentAsync(UrlHost);

            var token = doc.DocumentNode.SelectSingleNode("//input[@name=\"__RequestVerificationToken\"]")
                ?.GetAttributeValue("value", null)
                ?? throw new GMgardException("Cannot parse token");

            var postData = BuildPostData("ismakeup", "true", "__RequestVerificationToken", token);
            var response = await Client.PostAsync(UrlSignInPost, postData);

            var stream = await response.Content.ReadAsStreamAsync();
            var serializer = new System.Runtime.Serialization.Json.DataContractJsonSerializer(typeof(SignInResult));
            try
            {
                var result = serializer.ReadObject(stream) as SignInResult;
                return result;
            }
            catch (System.Runtime.Serialization.SerializationException)
            {
                stream.Position = 0;
                var json = await new System.IO.StreamReader(stream).ReadToEndAsync();
                throw new GMgardException($"Json parse error: {json}");
            }
        }

        #region [HttpClient Extensions]
        protected async Task<HttpResponseMessage> GetAsync(string requestUri)
        {
            LastResponse = null;
            var response = await Client.GetAsync(requestUri);
            if (response.IsSuccessStatusCode)
            {
                LastResponse = response;
                return response;
            }
            throw new HttpRequestException("GET response code: " + (int)response.StatusCode);
        }

        protected async Task<HtmlDocument> GetDocumentAsync(string requestUri)
        {
            var response = await GetAsync(requestUri);
            return await GetDocumentAsync(response);
        }

        protected async Task<HtmlDocument> GetDocumentAsync(HttpResponseMessage response = null)
        {
            response = response ?? LastResponse;
            var str = await response.Content.ReadAsStringAsync();
            var document = new HtmlDocument();
            document.LoadHtml(str);
            return document;
        }

        protected static FormUrlEncodedContent BuildPostData(string key1, string value1, params string[] keyValues)
        {
            var collection = new List<KeyValuePair<string, string>>();
            collection.Add(new KeyValuePair<string, string>(key1, value1));
            for (int i = 0; i < keyValues.Length; i += 2)
                collection.Add(new KeyValuePair<string, string>(keyValues[i], keyValues[i + 1]));
            return new FormUrlEncodedContent(collection);
        }
        #endregion
    }
}
