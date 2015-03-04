using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using grSim;
using messages_robocup_ssl_wrapper;
using ProtoBuf;

namespace SSL_HUB.Central
{
    internal static class Helper
    {
        private static readonly UdpClient Client;
        private static Byte[] _data;

        static Helper()
        {
            Client = new UdpClient {ExclusiveAddressUse = false};
            Client.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            Client.ExclusiveAddressUse = false;
            Client.Client.Bind(new IPEndPoint(IPAddress.Any, 10020));
            Client.JoinMulticastGroup(IPAddress.Parse("224.5.23.2"));
            new Thread(RecieveData).Start();
        }

        public static double Dtr(double angle)
        {
            return angle*(Math.PI/180);
        }

        public static double Rtd(double angle)
        {
            return angle*(180/Math.PI);
        }

        public static SSL_WrapperPacket GetData()
        {
            return Serializer.Deserialize<SSL_WrapperPacket>(new MemoryStream(_data));
        }

        public static void SendData(bool isYellow, int id, float w1, float w2, float w3, float w4)
        {
            var pkt = new grSim_Packet {commands = new grSim_Commands()};
            var robotCmd = new grSim_Robot_Command();
            pkt.commands.isteamyellow = isYellow;
            pkt.commands.timestamp = 0;
            robotCmd.id = (uint) id;
            robotCmd.wheelsspeed = true;
            robotCmd.velnormal = 0;
            robotCmd.veltangent = 0;
            robotCmd.velangular = 0;
            robotCmd.kickspeedx = 0;
            robotCmd.kickspeedz = 0;
            robotCmd.spinner = false;
            robotCmd.wheel1 = w1;
            robotCmd.wheel2 = w2;
            robotCmd.wheel3 = w3;
            robotCmd.wheel4 = w4;

            pkt.commands.robot_commands.Add(robotCmd);

            var stream = new MemoryStream();
            Serializer.Serialize(stream, pkt);
            var array = stream.ToArray();
            Client.Send(array, array.Length, new IPEndPoint(IPAddress.Broadcast, 20011));
        }

        private static void RecieveData()
        {
            var endPoint = new IPEndPoint(IPAddress.Any, 10020);
            while (true)
            {
                Thread.Sleep(10);
                _data = Client.Receive(ref endPoint);
            }
        }
    }
}