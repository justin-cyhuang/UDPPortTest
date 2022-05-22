using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using CommandLine;
using System.Threading;

namespace PortTest
{
    [Verb("server", HelpText = "Start the test Server for listening.")]
    public class ServerOptions
    {
        [Option(shortName: 'p', longName: "port", Required = true, HelpText = "The port you would like to perform the test, available value starts from 1024 to 65535")]
        public int Port { get; set; }

    }

    [Verb("client",HelpText ="Start the test client to sent testing packages.")]
    public class ClientOptions
    {
        [Option(shortName:'i',longName:"IP",HelpText ="The IP address you would like to send the packets to, required for client role")]
        public string IPAddress { get; set; }
        [Option(shortName:'p',longName:"port",Required = true, HelpText ="The port you would like to perform the test, available value starts from 1024 to 65535")]
        public int Port { get; set; }

    }
    internal class Program
    {
        static int Main(string[] args)
        {
            return CommandLine.Parser.Default.ParseArguments<ServerOptions, ClientOptions>(args)
                .MapResult(
                    (ServerOptions opts) => RunServerOptionsCode(opts),
                    (ClientOptions opts) => RunClientOptionsCode(opts),
            errs => 999);
        }

        static int RunServerOptionsCode(ServerOptions opts)
        {
            if ((opts.Port > 65535 || opts.Port < 1024))
            {
                Console.WriteLine("Invalid Port options.");
                return 11;
            }
            else { 
                UdpClient mylistener = new UdpClient(opts.Port);
                IPEndPoint iPEndPoint = new IPEndPoint(IPAddress.Any, opts.Port);

                try
                {
                    Console.WriteLine("Waiting for incoming packets...");

                    while (true)
                    {
                        byte[] buffer = mylistener.Receive(ref iPEndPoint);
                        Console.WriteLine($"Received the packages from {iPEndPoint}:");
                        Console.WriteLine($"{Encoding.UTF8.GetString(buffer,0,buffer.Length)}");
                    }
                }
                catch (SocketException e)
                {
                    Console.WriteLine(e);
                }
                finally
                {
                    mylistener.Close();
                }
            }
            return 0;
        }

        static int RunClientOptionsCode(ClientOptions opts)
        {
            try 
            {
                Socket mysocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

                IPAddress mytargetIP = IPAddress.Parse(opts.IPAddress);

                while (true)
                {
                    string sendtext = $"{DateTime.Now.ToString("yyyy/MM/dd tt hh:mm:ss")}:Test Package from {opts.IPAddress}..";
                    byte[] buffer = Encoding.UTF8.GetBytes(sendtext);
                    IPEndPoint myep = new IPEndPoint(mytargetIP, opts.Port);

                    mysocket.SendTo(buffer, myep);
                    Console.WriteLine("Message sent: "+sendtext);
                    Thread.Sleep(2000);
                }

            }
            catch(SocketException e)
            {
                Console.WriteLine(e);
                return 13;
            }
            
        }
    }
}
