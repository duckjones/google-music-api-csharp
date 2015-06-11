using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Security.Cryptography;

namespace GoogleMusicApi
{
    public class Api
    {
        private static string MainUrl = "https://mclients.googleapis.com/sj/v1.11/";
        private static string KeyPart1 = "VzeC4H4h+T2f0VI180nVX8x+Mb5HiTtGnKgH52Otj8ZCGDz9jRWyHb6QXK0JskSiOgzQfwTY5xgLLSdUSreaLVMsVVWfxfa8Rw==";
        private static string KeyPart2 = "ZAPnhUkYwQ6y5DdQxWThbvhJHN8msQ1rqJw0ggKdufQjelrKuiGGJI30aswkgCWTDyHkTGK9ynlqTkJ5L4CiGGUabGeo8M6JTQ==";

        private HttpHelper _helper = new HttpHelper();
        private string _deviceID = string.Empty;
        private string _authToken = string.Empty;

        public bool Login(string email, string password, string deviceID)
        {
            if (!string.IsNullOrEmpty(_authToken))
                return true;

            try
            {
                _helper.AuthToken = AuthHelper.MasterLogin(email, password, deviceID, _helper);
            }
            catch(Exception)
            {
                return false;
            }
            return true;
        }

        public RootObject GetPlaylists()
        {
            try
            {
                var result = _helper.POST(new Uri(MainUrl + "playlistfeed")).Result;
                var queryResult = Newtonsoft.Json.JsonConvert.DeserializeObject<RootObject>(result);
                return queryResult;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public RootObject GetPlaylistEntries()
        {
            try
            {
                var result = _helper.POST(new Uri(MainUrl + "plentryfeed")).Result;
                var queryResult = Newtonsoft.Json.JsonConvert.DeserializeObject<RootObject>(result);
                return queryResult;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public string GetStreamUrl(string songId, string deviceId)
        {
            var streamFetchUrl = "https://android.clients.google.com/music/mplay";   // GET
            
            var part1 = Convert.FromBase64String(KeyPart1);
            var part2 = Convert.FromBase64String(KeyPart2);
            var keyList = new List<char>();
            for (var i = 0; i < part1.Length; i++)
            {
                keyList.Add((char)(part1[i] ^ part2[i]));
            }

            var key = string.Join(string.Empty, keyList);
            var salt = DateTime.Now.Ticks.ToString().Substring(0, 12);

            var hmac = new HMACSHA1(System.Text.Encoding.UTF8.GetBytes(key));
            var hash = hmac.ComputeHash(Encoding.ASCII.GetBytes(songId + salt));
            var signature = Convert.ToBase64String(hash).Replace('+', '-').Replace('/', '_').TrimEnd('=');

            var songUrl = streamFetchUrl + querystring(salt, signature, songId);

            var newDeviceID = Int64.Parse(deviceId, System.Globalization.NumberStyles.HexNumber).ToString();
            var result = _helper.GET(new Uri(songUrl), null, true);
            return result.Result;
        }

        private string querystring(string salt, string signature, string songId)
        {
            return string.Format("?opt=hi&net=mob&pt=e&salt={0}&sig={1}&{2}={3}",
                salt, signature, songId[0] == 'T' ? "mjck" : "songid", songId);
        }
    }
}
