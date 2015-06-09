using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace GoogleMusicApi
{
    class AuthHelper
    {
        private static string AuthUrl = "https://android.clients.google.com/auth";
        private static string b64_key_7_3_29 = ("AAAAgMom/1a/v0lblO2Ubrt60J2gcuXSljGFQXgcyZWveWLEwo6prwgi3" +
                  "iJIZdodyhKZQrNWp5nKJ3srRXcUW+F1BD3baEVGcmEgqaLZUNBjm057pK" +
                  "RI16kB0YppeGx5qIQ5QjKzsR8ETQbKLNWgRY0QRNVz34kMJR3P/LgHax/" +
                  "6rmf5AAAAAwEAAQ==");
        private static String _googleDefaultPublicKey = "AAAAgMom/1a/v0lblO2Ubrt60J2gcuXSljGFQXgcyZWveWLEwo6prwgi3iJIZdodyhKZQrNWp5nKJ3srRXcUW+F1BD3baEVGcmEgqaLZUNBjm057pKRI16kB0YppeGx5qIQ5QjKzsR8ETQbKLNWgRY0QRNVz34kMJR3P/LgHax/6rmf5AAAAAwEAAQ==";


        public static string MasterLogin(string email, string password, string androidId, HttpHelper helper)
        {
            HttpContent content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("accountType", "HOSTED_OR_GOOGLE"),
                new KeyValuePair<string, string>("Email", email),
                new KeyValuePair<string, string>("has_permission", "1"),
                new KeyValuePair<string, string>("add_account", "1"), 
                new KeyValuePair<string, string>("EncryptedPasswd", GetEncryptedPassword(email, password)),
                new KeyValuePair<string, string>("service", "ac2dm"), 
                new KeyValuePair<string, string>("source", "android"), 
                new KeyValuePair<string, string>("androidId", androidId), 
                new KeyValuePair<string, string>("device_country", "us"), 
                new KeyValuePair<string, string>("operatorCountry", "us"), 
                new KeyValuePair<string, string>("lang", "en"), 
                new KeyValuePair<string, string>("sdk_version", "17"), 
            });

            var responseString = helper.POST(new Uri(AuthUrl), content);
            var authMasterToken = ParseMasterToken(responseString.Result);
            return OauthLogin(email, authMasterToken, androidId, helper);
        }

        private static string OauthLogin(string email, string token, string androidId, HttpHelper helper)
        {
            HttpContent content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("accountType", "HOSTED_OR_GOOGLE"),
                new KeyValuePair<string, string>("Email", email),
                new KeyValuePair<string, string>("has_permission", "1"),
                new KeyValuePair<string, string>("add_account", "1"), 
                new KeyValuePair<string, string>("EncryptedPasswd", token),
                new KeyValuePair<string, string>("service", "sj"), 
                new KeyValuePair<string, string>("source", "android"), 
                new KeyValuePair<string, string>("androidId", androidId), 
                new KeyValuePair<string, string>("app", "com.google.android.music"),
                new KeyValuePair<string, string>("client_sig", "38918a453d07199354f8b19af05ec6562ced5788"),
                new KeyValuePair<string, string>("device_country", "us"), 
                new KeyValuePair<string, string>("operatorCountry", "us"), 
                new KeyValuePair<string, string>("lang", "en"), 
                new KeyValuePair<string, string>("sdk_version", "17"), 
            });
            var responseString = helper.POST(new Uri(AuthUrl), content);
            return ParseAuthToken(responseString.Result);
        }

        private static string ParseMasterToken(string responseData)
        {
            if (responseData.IndexOf("Token") == -1)
                throw new InvalidOperationException();
            var parsedToken = responseData.Substring(responseData.IndexOf("Token") + ("Token=").Length);
            return parsedToken.Substring(0, parsedToken.IndexOf('\n'));
        }

        private static string ParseAuthToken(string responseData)
        {
            if (responseData.IndexOf("Auth") == -1)
                throw new InvalidOperationException();
            var parsedToken = responseData.Substring(responseData.IndexOf("Auth") + ("Auth=").Length);
            return parsedToken.Substring(0, parsedToken.IndexOf('\n'));
        }

        private static System.Security.Cryptography.RSAParameters RSAParams()
        {
            var rsaParameters = new System.Security.Cryptography.RSAParameters();

            var encryptionBytes = Convert.FromBase64String(b64_key_7_3_29);

            var i = encryptionBytes.Take(4).Sum(b => b);
            rsaParameters.Modulus = encryptionBytes.Skip(4).Take((int)i).ToArray();
            var j = encryptionBytes.Skip((int)i + 4).Take(4).Sum(b => b);
            rsaParameters.Exponent = encryptionBytes.Skip((int)i + 8).Take((int)j).ToArray();
            return rsaParameters;
        }

        private static byte[] ComputeStruct(RSAParameters parameters)
        {
            var bytes = new List<byte>() { 0, 0, 0, 0x80 };
            bytes.AddRange(parameters.Modulus);
            bytes.AddRange(new byte[] { 0, 0, 0, 0x03 });
            bytes.AddRange(parameters.Exponent);

            return bytes.ToArray();
        }

        private static string EncryptPassword(string email, string password)
        {
            var signature = new List<byte>();
            var rsaParams = RSAParams();
            signature.Add(0);
            using (SHA1Managed sha1 = new SHA1Managed())
            {
                var hash = sha1.ComputeHash(ComputeStruct(rsaParams));
                signature.AddRange(hash.Take(4));
            }
            RSACryptoServiceProvider csp = new RSACryptoServiceProvider(2048);
            csp.ImportParameters(rsaParams);
            signature.AddRange(csp.Encrypt(System.Text.Encoding.UTF8.GetBytes(string.Concat(email, 0x00, password)), true));
            return Convert.ToBase64String(signature.ToArray()).Replace('+', '-').Replace('/', '_');
        }

        private static string GetEncryptedPassword(string email, string password)
        {
            // Step 1
            var binaryKey = Convert.FromBase64String(_googleDefaultPublicKey);
            var rsaParameters = new System.Security.Cryptography.RSAParameters();

            // Step 2
            int i = readInt(binaryKey, 0);
            byte[] half = binaryKey.Skip(4).Take(i).ToArray();
            var firstKeyInteger = BitConverter.ToInt64(half, 0);
            rsaParameters.Modulus = half;

            // Step 3
            int j = readInt(binaryKey, i + 4);
            half = binaryKey.Skip(i + 8).Take(4).ToArray();
            var otherHalf = new byte[8];
            System.Array.Copy(half, 0, otherHalf, 0, half.Length);
            var secondKeyInteger = BitConverter.ToInt64(otherHalf, 0);
            rsaParameters.Exponent = half;

            // Step 4
            byte[] signature = new byte[5];
            signature[0] = 0;
            using (SHA1Managed sha1 = new SHA1Managed())
            {
                var hash = sha1.ComputeHash(binaryKey);
                System.Array.Copy(hash, 0, signature, 1, 4);
            }

            // Step 5
            String combined = email + "\u0000" + password;
            byte[] plain = System.Text.Encoding.UTF8.GetBytes(combined);
            byte[] output = new byte[133];
            System.Array.Copy(signature, 0, output, 0, signature.Length);

            using (var csp = new RSACryptoServiceProvider(1028))
            {
                csp.ImportParameters(rsaParameters);
                var encrypted = csp.Encrypt(plain, true);
                System.Array.Copy(encrypted, 0, output, signature.Length, encrypted.Length);
            }
            return Convert.ToBase64String(output).Replace('+', '-').Replace('/', '_');
        }
            

        private static int readInt(byte[] arrayOfByte, int start)
        {
            return 0x0 | (0xFF & arrayOfByte[start]) << 24 | (0xFF & arrayOfByte[(start + 1)]) << 16 | (0xFF & arrayOfByte[(start + 2)]) << 8 | 0xFF & arrayOfByte[(start + 3)];
        }
    }
}
