using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.IO;

namespace LoginSystem_server
{
    internal class Program
    {
        static Socket socket;
        static IPAddress ip_addr;
        static IPEndPoint endPoint;
        static public string ips;


        static IPAddress ip2;
        static IPEndPoint endPoint2;
        static Thread listenThread;
        static int port = 80;


        static int opp_dec;
        static public string last_msg = "";
        static Random rand = new Random();

        //static string website_root = @"C:\Users\lucky\source\repos\webserver_csharp\www\";            //Has to be CHANGED
        static string website_root = @"L:\Visual Studio 2019\webserver_csharp\www";
        static List<string> headers = new List<string>();



        public static void Main()
        {
            Console.WriteLine(GetLocalIPAddress() + ":" + port);
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            Listen1();


        }


        static byte[] FileToBytes(string file)
        {
            string path;
            path = website_root + file.Replace("/", @"\");
            if (file == "/")
            {
                path += "index.html";
            }
            if (!File.Exists(path))
            {
                path = website_root + @"\404.html";
            }
            FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            StreamReader reader = new StreamReader(fs);
            var bytes = default(byte[]);
            using (var memstream = new MemoryStream())
            {
                reader.BaseStream.CopyTo(memstream);
                bytes = memstream.ToArray();
            }
            return bytes;
        }
        static string GetContentType(string file)
        {
            string content_type = "";
            if(file == "/")
            {
                file += "index.html";
            }
            if(file.Split('.').Length != 2)
            {
                return "text/html";
            }
            string ext = file.Split('.')[1];
            switch (ext)
            {
                case "html":
                    content_type = "text/html";
                    break;
                case "css":
                    content_type = "text/css";
                    break;
                case "js":
                    content_type = "text/javascript";
                    break;
                case "xml":
                    content_type = "text/xml";
                    break;
                case "ico":
                    content_type = "image/x-icon";
                    break;
                case "png":
                    content_type = "image/png";
                    break;
                case "jpg":
                    content_type = "image/jpeg";
                    break;
                case "gif":
                    content_type = "image/gif";
                    break;
                default:
                    content_type = "text/plain";
                    break;
            }
            return content_type;
        }
        static List<string> ParseRequest(string request)
        {
            List<string> words = new List<string>();
            string word = "";
            for(int i = 0; i < request.Length; i++)
            {
                if (request[i] == ' ')
                {
                    words.Add(word);
                    word = "";
                    continue;
                }
                word += request[i];
            }
            return words;
        }
        static string MergeHeadersInString()
        {
            string header_str = "";
            for(int i = 0; i < headers.Count; i++)
            {
                header_str += headers[i];
                header_str += "\r\n";
                if (i == headers.Count - 1)
                {
                    header_str += "\r\n";
                }
            }
            return header_str;
        }

        private static void Listen1()
        {
            listenThread = new Thread(new ThreadStart(Listen));
            listenThread.Start();
        }
        public static string GetRequest(List<string> parsed_message, Socket clientSocket)
        {
            byte[] response_body = FileToBytes(parsed_message[1]);
            headers.Clear();
            if (Encoding.Default.GetString(response_body) != Encoding.Default.GetString(FileToBytes("/404.html")))
            {
                headers.Add("HTTP/1.1 200 OK");
                headers.Add($"Content-Type: {GetContentType(parsed_message[1])}; charset=UTF-8");
            }
            else
            {
                headers.Add("HTTP/1.1 404 Not Found");
                headers.Add($"Content-Type: {GetContentType("/404.html")}; charset=UTF-8");
            }

            headers.Add("Content-Length: " + response_body.Length);
            byte[] headers_bytes = Encoding.ASCII.GetBytes(MergeHeadersInString());
            byte[] response = new byte[headers_bytes.Length + response_body.Length];
            Buffer.BlockCopy(headers_bytes, 0, response, 0, headers_bytes.Length);
            Buffer.BlockCopy(response_body, 0, response, headers_bytes.Length, response_body.Length);
            Send(response, clientSocket);
            return Encoding.Default.GetString(response);
        }

        private static void Listen()
        {
            try
            {
                ip2 = IPAddress.Parse(GetLocalIPAddress());
                endPoint2 = new IPEndPoint(ip2, port);
                socket.Bind(endPoint2);
                socket.Listen(100);
                while (true)
                {
                    Socket clientSocket = socket.Accept();
                    byte[] buffer = new byte[1024];
                    int bytesReceived = clientSocket.Receive(buffer);
                    string message = Encoding.ASCII.GetString(buffer, 0, bytesReceived);
                    last_msg = message;


                    string response_str = "";
                    List<string> parsed_message = ParseRequest(message);

                    if(parsed_message.Count > 2)
                    {
                        switch (parsed_message[0])
                        {
                            case "GET":
                                response_str = GetRequest(parsed_message, clientSocket);
                                break;
                            case "POST":
                                break;
                            default:
                                break;
                        }
                    } else
                    {
                        Send(Encoding.ASCII.GetBytes("Invalid request"), clientSocket);
                    }
                    

                    clientSocket.Close();





                    //[DEBUG] Print request
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine(message);
                    //_________________________
                    //[DEBUG] Print parsed message
                    Console.WriteLine("____________");
                    foreach (string s in parsed_message)
                    {
                        Console.WriteLine(s);
                    }
                    Console.WriteLine("____________");
                    //_________________________
                    //[DEBUG] Print response
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine(response_str);
                    //_________________________
                    //[DEBUG] Separating by block of request-response
                    Console.WriteLine("\n\n\n\n");
                    //_________________________
                }
            }
            catch (DivideByZeroException ex)
            {
                Console.WriteLine("Listening error: " + ex.Message);
                Listen1();
            }

        }
        private static void Send(byte[] msg, Socket clientSocket)
        {
            try
            {
                clientSocket.Send(msg);
                clientSocket.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Sending error: " + ex.Message);
                Console.WriteLine("IP is offline");
            }
        }

        public static string CreateMD5(string input)
        {
            StringBuilder hash = new StringBuilder();
            MD5CryptoServiceProvider md5provider = new MD5CryptoServiceProvider();
            byte[] bytes = md5provider.ComputeHash(new UTF8Encoding().GetBytes(input));

            for (int i = 0; i < bytes.Length; i++)
            {
                hash.Append(bytes[i].ToString("x2"));
            }
            return hash.ToString();
        }
        public static string GetLocalIPAddress()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    return ip.ToString();
                }
            }
            return "255.255.255.255";
        }
    }
}
