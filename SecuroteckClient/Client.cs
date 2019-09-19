using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Http;
using SecuroteckWebApplication.Models;
using System.Security.Cryptography;

namespace SecuroteckClient
{
    #region Task 10 and beyond
    class Client
    {
        private static HttpClient ThisClient;
        private static HttpRequestMessage RequestMessage;
        public  static string PublicKey;
        private static string Response;
        private static User ThisUser;
        private static readonly Dictionary<string, Action<string[]>> Tasks = new Dictionary<string, Action<string[]>>()
        {
            { "talkbackhello", async p =>
                {
                    try{Console.WriteLine(await (await ThisClient.GetAsync("api/talkback/hello")).Content.ReadAsStringAsync()); }
                    catch (Exception e) { Console.WriteLine(e.Message); }
                    Console.WriteLine("What would you like to do next?");
                }
            },
            { "talkbacksort", async p =>
                {
                    try
                    {
                        if (p[2] != null)
                        {
                            string uri = "";
                            uri += "api/talkback/sort?";
                            int count = 0;
                            if (p.Count() == 2) p[3] = "";
                            foreach (char c in p[2])
                            {
                                if (int.TryParse(c.ToString(), out int i))
                                {
                                    if (count == 0) uri += "integers=" + i.ToString();
                                    else uri += "&integers=" + i.ToString();
                                    count++;
                                }
                            }
                            Console.WriteLine(await (await ThisClient.GetAsync(uri)).Content.ReadAsStringAsync());
                        }
                    }
                    catch (Exception e) { Console.WriteLine(e.Message); }
                    Console.WriteLine("What would you like to do next?");
                }
            },
            { "userget", async p => 
            
                {
                    try {Console.WriteLine((await(await ThisClient.GetAsync("api/user/new?username=" + p[2])).Content.ReadAsStringAsync())); }
                    catch (Exception e) { Console.WriteLine(e.Message); }
                    Console.WriteLine("What would you like to do next?");
                }
            },
            { "userpost", async p =>
                {
                    try
                    {
                        Response = (await (await ThisClient.SendAsync(new HttpRequestMessage(HttpMethod.Post, "api/user/new")
                        {
                            Content = new StringContent("\"" + p[2] + "\"", Encoding.UTF8, "application/json")
                        })).Content.ReadAsStringAsync()).Replace("\"", "");
                        Guid guid;
                        if (Guid.TryParse(Response, out guid))
                        {
                            ThisUser = new User()
                            {
                                ApiKey = Response,
                                UserName = p[2]
                            };
                            Console.WriteLine("Got API Key");
                        }
                        else Console.WriteLine(Response);
                    }
                    catch (Exception e) { Console.WriteLine(e.Message); }
                    Console.WriteLine("What would you like to do next?");
                }
            },
            { "userset", p =>
                {
                    try
                    {
                        ThisUser = new User()
                        {
                            ApiKey = new Guid(p[3]).ToString(),
                            UserName = p[2]
                        };
                        Console.WriteLine("Stored");
                    }
                    catch (Exception e) {Console.WriteLine( e.Message); }
                    Console.WriteLine("What would you like to do next?");
                }
            },
            { "userdelete", async p =>
                {
                    try
                    {
                        if (ThisUser != null)
                        {
                            RequestMessage = new HttpRequestMessage(HttpMethod.Delete, "api/user/removeuser?username=" + ThisUser.UserName);
                            RequestMessage.Headers.Add("ApiKey", ThisUser.ApiKey);
                            Console.WriteLine((await (await ThisClient.SendAsync(RequestMessage)).Content.ReadAsStringAsync() == "true" ? new Func<string>(()=> { ThisUser = null; return "True"; }).Invoke() : "False"));
                        }
                        else Console.WriteLine("You need to do a User Post or User Set first");
                    }
                    catch (Exception e) {Console.WriteLine(e.Message); }
                    Console.WriteLine("What would you like to do next?");
                }
            },
            { "userrole", async p =>
                {
                    try
                    {
                        if (p[2] != null && p[3] != null)
                        {
                            if (ThisUser != null)
                            {
                                RequestMessage = new HttpRequestMessage(HttpMethod.Post, "api/user/changerole")
                                {
                                    Content = new StringContent("{\"username\" : \"" + p[2] + "\",\"role\" : \"" + p[3] + "\"}", Encoding.UTF8, "application/json")
                                };
                                RequestMessage.Headers.Add("ApiKey", ThisUser.ApiKey.ToString());
                                Console.WriteLine(await (await ThisClient.SendAsync(RequestMessage)).Content.ReadAsStringAsync());
                            }
                            else Console.WriteLine("You need to do a User Post or User Set first");
                        }
                    }
                    catch (Exception e) {Console.WriteLine(e.Message); }
                    Console.WriteLine("What would you like to do next?");
                }
            },
            { "protectedhello", async p =>
                {
                    try
                    {
                        if (ThisUser != null)
                        {
                            RequestMessage = new HttpRequestMessage(HttpMethod.Get, "api/protected/hello");
                            RequestMessage.Headers.Add("ApiKey", ThisUser.ApiKey.ToString());
                           Console.WriteLine(await (await ThisClient.SendAsync(RequestMessage)).Content.ReadAsStringAsync());
                        }
                        else Console.WriteLine("You need to do a User Post or User Set first");
                    }
                    catch (Exception e) {Console.WriteLine( e.Message); }
                    Console.WriteLine("What would you like to do next?");
                }
            },
            { "protectedsha1", async p =>
                {
                    try
                    {
                        if (ThisUser != null)
                        {
                            RequestMessage = new HttpRequestMessage(HttpMethod.Get, "api/protected/" + "sha1" + "?message=" + p[2]);
                            RequestMessage.Headers.Add("ApiKey", ThisUser.ApiKey.ToString());
                            Console.WriteLine(await(await ThisClient.SendAsync(RequestMessage)).Content.ReadAsStringAsync());
                        }
                        else Console.WriteLine("You need to do a User Post or User Set first");
                    }
                    catch (Exception e) {Console.WriteLine( e.Message);}
                    Console.WriteLine("What would you like to do next?");
                }
            },
            { "protectedsha256", async p => 
                {
                    try
                    {
                        if (ThisUser != null)
                        {
                            RequestMessage = new HttpRequestMessage(HttpMethod.Get, "api/protected/" + "sha256" + "?message=" + p[2]);
                            RequestMessage.Headers.Add("ApiKey", ThisUser.ApiKey.ToString());
                            Console.WriteLine(await(await ThisClient.SendAsync(RequestMessage)).Content.ReadAsStringAsync());
                        }
                        else Console.WriteLine("You need to do a User Post or User Set first");
                    }
                    catch (Exception e) {Console.WriteLine( e.Message);}
                    Console.WriteLine("What would you like to do next?");
                }
            },
            { "protectedget", async p =>
                {
                    try
                    {
                        if (ThisUser != null)
                        {
                            RequestMessage = new HttpRequestMessage(HttpMethod.Get, "api/protected/" + "getpublickey");
                            RequestMessage.Headers.Add("ApiKey", ThisUser.ApiKey.ToString());
                            HttpResponseMessage message = await ThisClient.SendAsync(RequestMessage);
                            Response = (await message.Content.ReadAsStringAsync());
                            if(Response.Contains("Unauthor")) Console.WriteLine(Response.Replace("\"", ""));
                            else if (message.StatusCode == System.Net.HttpStatusCode.OK)
                            {
                                PublicKey = Response.Replace("\"", "");
                                Console.WriteLine("Got Public Key");
                            }
                            else Console.WriteLine("Couldn't Get the Public Key");
                        }
                        else Console.WriteLine("You need to do a User Post or User Set first");
                    }
                    catch (Exception e) {Console.WriteLine( e.Message);}
                    Console.WriteLine("What would you like to do next?");
                }
            },
            { "protectedsign", async p =>
                {
                    try
                    {
                        if (ThisUser != null)
                        {
                            if (PublicKey != null)
                            {
                                RequestMessage = new HttpRequestMessage(HttpMethod.Get, "api/protected/sign?message=" + p[2]);
                                RequestMessage.Headers.Add("ApiKey", ThisUser.ApiKey);
                                Response = await (await ThisClient.SendAsync(RequestMessage)).Content.ReadAsStringAsync();
                                Response = Response.Replace("\"", "");
                                byte[] byteResponse = Response.Split('-').Select(x => Convert.ToByte(x, 16)).ToArray();
                                using (RSACryptoServiceProvider RSA = new RSACryptoServiceProvider())
                                {
                                    RSA.FromXmlString(PublicKey);
                                    if (RSA.VerifyData(Encoding.ASCII.GetBytes(p[2]), new SHA1CryptoServiceProvider(), byteResponse))
                                        Console.WriteLine("Message was successfully signed");
                                    else
                                        Console.WriteLine("Message was not successfully signed");
                                }
                            }
                            else Console.WriteLine("Client doesn't yet have the public key");
                        }
                        else Console.WriteLine("You need to do a User Post or User Set first");
                    }
                    catch (Exception e) {Console.WriteLine( e.Message);}
                    Console.WriteLine("What would you like to do next?");
                }
            },
            { "protectedaddfifty", async p =>
                {
                    try
                    {
                        if (ThisUser != null)
                        {
                            using (AesManaged aes = new AesManaged())
                            {
                                aes.GenerateKey();
                                aes.GenerateIV();
                                byte[] aesKey = aes.Key;
                                byte[] aesIV = aes.IV;
                                using (RSACryptoServiceProvider rsa = new RSACryptoServiceProvider())
                                {
                                    // Creates an RSA crypto service provider using the server's public key
                                    rsa.FromXmlString(PublicKey);

                                    byte[] encryptedInteger = rsa.Encrypt(BitConverter.GetBytes(int.Parse(p[2])), true);
                                    byte[] encryptedKey = rsa.Encrypt(aesKey, true);
                                    byte[] encryptedIV = rsa.Encrypt(aesIV, true);

                                    string hexEncryptedInteger = BitConverter.ToString(encryptedInteger);
                                    string hexEncryptedKey = BitConverter.ToString(encryptedKey);
                                    string hexEncryptedIV = BitConverter.ToString(encryptedIV);

                                    RequestMessage = new HttpRequestMessage(HttpMethod.Get,
                                        "api/protected/addfifty?encryptedInteger=" + hexEncryptedInteger
                                        + "&encryptedsymkey=" + hexEncryptedKey
                                        + "&encryptedIV=" + hexEncryptedIV);
                                    RequestMessage.Headers.Add("ApiKey", ThisUser.ApiKey.ToString());

                                    Response = await (await ThisClient.SendAsync(RequestMessage)).Content.ReadAsStringAsync();

                                    Response = Response.Replace("\"", "");
                                    if (!Response.Contains("Unauthorized"))
                                    {
                                        try
                                        {
                                            byte[] encryptedByteResponse = Response.Split('-').Select(x => Convert.ToByte(x, 16))
                                                .ToArray();
                                            byte[] byteResponse = aes.CreateDecryptor(aesKey, aesIV).TransformFinalBlock(encryptedByteResponse, 0, encryptedByteResponse.Length);
                                            Console.WriteLine(int.Parse(Encoding.ASCII.GetString(byteResponse)));
                                        }
                                        catch (Exception e){ Console.WriteLine(e.Message); }
                                    }else Console.WriteLine("Unauthorized. Admin access only.");
                                }
                            }
                        }
                        else Console.WriteLine("You need to do a User Post or User Set first");
                    }
                    catch (Exception e) {Console.WriteLine( e.Message);}
                    Console.WriteLine("What would you like to do next?");
                }
            },
            { "exit", (c => { Environment.Exit(0); }) }
        };
        static void Main(string[] args)
        {
            ThisClient = new HttpClient{ BaseAddress = new Uri("http://localhost:24702/") };             
            Console.WriteLine("Hello. What would you like to do?");
            while (true)
            {
                try
                {
                    string[] userInput = (Console.ReadLine()).Split(' '); Console.Clear(); Console.WriteLine("...please wait...");
                    Tasks[(userInput[0] + userInput[1]).ToLower()].Invoke(userInput);
                }
                catch (Exception e) { Console.WriteLine(e.Message); }
            }
        }
    }
    #endregion
}
