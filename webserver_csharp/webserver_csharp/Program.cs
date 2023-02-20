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


        static int opp_dec;
        static public string last_msg = "";
        static Random rand = new Random();

        static string website_root = @"C:\Users\lucky\source\repos\webserver_csharp\www\";            //Has to be CHANGED
        static List<string> headers = new List<string>();



        public static void Main()
        {
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            Listen1();


        }


        static string FileToString(string file)
        {
            FileStream fs = new FileStream(website_root+file, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            StreamReader reader = new StreamReader(fs);
            return reader.ReadToEnd();
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
                }
                word += request[i];
            }
            return null;
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

        private static void Listen()
        {
            try
            {
                ip2 = IPAddress.Parse(GetLocalIPAddress());
                endPoint2 = new IPEndPoint(ip2, 80);
                socket.Bind(endPoint2);
                socket.Listen(100);
                while (true)
                {
                    Socket clientSocket = socket.Accept();
                    byte[] buffer = new byte[1024];
                    int bytesReceived = clientSocket.Receive(buffer);
                    string message = Encoding.ASCII.GetString(buffer, 0, bytesReceived);
                    last_msg = message;
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine(message);

                    string response_body = FileToString("index.html");
                    headers.Clear();
                    headers.Add("HTTP/1.1 200 OK");
                    headers.Add("Content-Type: text/html; charset=UTF-8");
                    headers.Add("Content-Length: " + response_body.Length);
                    string response = MergeHeadersInString() + response_body;
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine(response);
                    Send(response, clientSocket);
                    
                    
                    
                    
                    clientSocket.Close();


                    Console.WriteLine("\n\n\n\n");

                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Listening error: " + ex.Message);
                Listen1();
            }

        }
        private static void Send(string str, Socket clientSocket)
        {
            try
            {
                byte[] buffer = Encoding.ASCII.GetBytes(str);
                clientSocket.Send(buffer);
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
