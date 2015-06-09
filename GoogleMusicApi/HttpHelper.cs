using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace GoogleMusicApi
{
    class HttpHelper
    {
        public HttpHelper() 
        {
            var handler = new HttpClientHandler()
            {
                AllowAutoRedirect = false
            };
            _client = new HttpClient(handler);
        }

        private HttpClient _client;
        public string AuthToken { get; set; }

        /// <summary>
        /// POST request
        /// </summary>
        /// <param name="address">end point</param>
        /// <param name="content">content</param>
        /// <returns></returns>
        public async Task<String> GET(Uri address, string deviceId, bool authenticate = true)
        {

            SetAuthHeader();
            //RebuildCookieContainer();

            HttpResponseMessage responseMessage = null;
            HttpRequestMessage requestMessage = null;

            try
            {
                requestMessage = new HttpRequestMessage(HttpMethod.Get, address);
                if (!string.IsNullOrEmpty(deviceId))
                    requestMessage.Headers.Add("X-Device-ID", deviceId);

                if (authenticate && !string.IsNullOrEmpty(AuthToken))
                    requestMessage.Headers.Add("Authorization", "GoogleLogin auth=" + AuthToken);
                
                responseMessage = await _client.SendAsync(requestMessage);
            }
            catch (Exception e)
            {
                throw;
            }

            String retnData = String.Empty;

            try
            {
                retnData = await responseMessage.Content.ReadAsStringAsync();
            }
            catch (Exception e)
            {
                throw; // Bubble up
            }

            return retnData;
        }

        /// <summary>
        /// POST request
        /// </summary>
        /// <param name="address">end point</param>
        /// <param name="content">content</param>
        /// <returns></returns>
        public async Task<String> POST(Uri address, HttpContent content = null, bool authenticate = true)
        {
            
                SetAuthHeader();
            //RebuildCookieContainer();

            HttpResponseMessage responseMessage = null;
            HttpRequestMessage requestMessage = null;

            try
            {
                //String reqUri = BuildGoogleRequest(address).ToString();
                requestMessage = new HttpRequestMessage(HttpMethod.Post, address);
                requestMessage.Content = content;
                
                if (authenticate && !string.IsNullOrEmpty(AuthToken))
                    requestMessage.Headers.Add("Authorization", "GoogleLogin auth=" + AuthToken);
                responseMessage = await _client.SendAsync(requestMessage);
            }
            catch (Exception e)
            {
                throw;
            }

            String retnData = String.Empty;

            try
            {
                retnData = await responseMessage.Content.ReadAsStringAsync();
            }
            catch (Exception e)
            {
                throw; // Bubble up
            }

            return retnData;
        }

        private void SetAuthHeader()
        {


        }
    }
}
