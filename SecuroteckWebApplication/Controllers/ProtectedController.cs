using System;
using System.Linq;
using SecuroteckWebApplication.Models;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Web.Http;
using System.IO;
using System.Collections.Generic;

namespace SecuroteckWebApplication.Controllers.Authorisation
{
    public class ProtectedController : ApiController
    {
        [APIAuthorise]
        [ActionName("hello")]
        public HttpResponseMessage Get(HttpRequestMessage httpRequest)
        {
            try
            {
                if (httpRequest.Headers.TryGetValues("ApiKey", out IEnumerable<string> headerValue))
                {
                    if (UserDatabaseAccess.UserExists(new Guid(headerValue.FirstOrDefault()), out User user))
                        return Request.CreateResponse(HttpStatusCode.OK, "Hello " + user.UserName);
                }
                return Request.CreateResponse(HttpStatusCode.BadRequest, "Bad Request");
            }
            catch (Exception e) { return Request.CreateResponse(HttpStatusCode.BadRequest, e.Message); }
        }

        [APIAuthorise]
        [HttpGet]
        [ActionName("sha1")]
        public HttpResponseMessage Get([FromUri] string message, HttpRequestMessage httpRequest)
        {
            try
            {
                if (!string.IsNullOrEmpty(message))
                {
                    SHA1 sha1 = new SHA1CryptoServiceProvider();
                    return Request.CreateResponse(HttpStatusCode.OK, BitConverter.ToString(sha1.ComputeHash(Encoding.ASCII.GetBytes(message))).Replace("-", ""));
                }
                return Request.CreateResponse(HttpStatusCode.BadRequest, "Bad Request");
            }
            catch (Exception e) { return Request.CreateResponse(HttpStatusCode.BadRequest, e.Message); }
        }
        [APIAuthorise]
        [HttpGet]
        [ActionName("sha256")]
        public HttpResponseMessage SHA256([FromUri] string message)
        {
            try
            {
                if (!string.IsNullOrEmpty(message))
                {
                    SHA256 sha256 = new SHA256CryptoServiceProvider();
                    return Request.CreateResponse(HttpStatusCode.OK, BitConverter.ToString(sha256.ComputeHash(Encoding.ASCII.GetBytes(message))).Replace("-", ""));
                }
                return Request.CreateResponse(HttpStatusCode.BadRequest, "Bad Request");
            }
            catch (Exception e) { return Request.CreateResponse(HttpStatusCode.BadRequest, e.Message); }
        }
        [APIAuthorise]
        [HttpGet]
        [ActionName("getpublickey")]
        public HttpResponseMessage GetPublicKey()
        {
            try
            {
                return Request.CreateResponse(HttpStatusCode.OK, WebApiConfig.PublicKey);
            }
            catch (Exception e) { return Request.CreateResponse(HttpStatusCode.BadRequest, e.Message); }
        }
        [APIAuthorise]
        [HttpGet]
        [ActionName("sign")]
        public HttpResponseMessage Sign([FromUri]string message)
        {
            try
            {
                using (RSACryptoServiceProvider RSA = new RSACryptoServiceProvider())
                {
                    RSA.FromXmlString(WebApiConfig.PrivateKey);
                    RSAPKCS1SignatureFormatter RSAFormatter = new RSAPKCS1SignatureFormatter(RSA);
                    RSAFormatter.SetHashAlgorithm("SHA1");
                    SHA1Managed sha = new SHA1Managed();
                    return Request.CreateResponse(HttpStatusCode.OK, BitConverter.ToString(RSAFormatter.CreateSignature(sha.ComputeHash(Encoding.ASCII.GetBytes(message)))));
                }
            }
            catch (Exception e) { return Request.CreateResponse(HttpStatusCode.BadRequest, e.Message); }
        }
        [APIAuthorise]
        [AdminRole]
        [HttpGet]
        [ActionName("addfifty")]
        public HttpResponseMessage GetAddFIDDY([FromUri] string encryptedInteger, [FromUri] string encryptedSymKey, [FromUri] string encryptedIV)
        {
            try
            {
                byte[] encryptedIntegerByteArray = encryptedInteger.Split('-').Select(value => Convert.ToByte(value, 16)).ToArray();
                byte[] encryptedSymKeyByteArray = encryptedSymKey.Split('-').Select(value => Convert.ToByte(value, 16)).ToArray();
                byte[] encryptedIVByteArray = encryptedIV.Split('-').Select(value => Convert.ToByte(value, 16)).ToArray();
                using (RSACryptoServiceProvider RSA = new RSACryptoServiceProvider())
                {
                    RSA.FromXmlString(WebApiConfig.PrivateKey);
                    byte[] integerArray = RSA.Decrypt(encryptedIntegerByteArray, true);
                    byte[] symKeyArray = RSA.Decrypt(encryptedSymKeyByteArray, true);
                    byte[] IVArray = RSA.Decrypt(encryptedIVByteArray, true);
                    int i = BitConverter.ToInt32(integerArray, 0) + 50;

                    //AesCryptoServiceProvider code sourced from:
                    //https://docs.microsoft.com/en-us/dotnet/api/system.security.cryptography.aes?view=netframework-4.7.2
                    using (AesCryptoServiceProvider aes = new AesCryptoServiceProvider())
                    {
                        byte[] aesEncryptedInteger;
                        aes.Key = symKeyArray;
                        aes.IV = IVArray;
                        using (MemoryStream msEncrypt = new MemoryStream())
                        {
                            using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, aes.CreateEncryptor(aes.Key, aes.IV), CryptoStreamMode.Write))
                            {
                                using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                                {
                                    swEncrypt.Write(i.ToString());
                                }
                                aesEncryptedInteger = msEncrypt.ToArray();
                            }
                        }
                        return Request.CreateResponse(HttpStatusCode.OK, BitConverter.ToString(aesEncryptedInteger));
                    }
                    //AesCryptoServiceProvider code sourced from:
                    //https://docs.microsoft.com/en-us/dotnet/api/system.security.cryptography.aes?view=netframework-4.7.2
                }
            }
            catch (Exception e)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, e.Message);
            }
        }
    }
}