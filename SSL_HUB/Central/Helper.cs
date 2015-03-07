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
        private static Form1 _controller;

        static Helper()
        {
            Client = new UdpClient {ExclusiveAddressUse = false};
            Client.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            Client.ExclusiveAddressUse = false;
            Client.Client.Bind(new IPEndPoint(IPAddress.Any, 10020));
            Client.JoinMulticastGroup(IPAddress.Parse("224.5.23.2"));
            new Thread(RecieveData).Start();
            Spinner = true;
        }

        public static bool Spinner { get; set; }

        public static void SetController(Form1 controller)
        {
            _controller = controller;
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

        public static void SendData(bool isYellow, int id, float w1, float w2, float w3, float w4, float kickSpeedX,
            float kickSpeedZ)
        {
            // TODO: remove these and check affect on real system
            const float zero = (float) 0.000000000001;
            w1 = (w1 == 0) ? zero : w1;
            w2 = (w2 == 0) ? zero : w2;
            w3 = (w3 == 0) ? zero : w3;
            w4 = (w4 == 0) ? zero : w4;

            var pkt = new grSim_Packet {commands = new grSim_Commands()};
            var robotCmd = new grSim_Robot_Command();
            pkt.commands.isteamyellow = isYellow;
            pkt.commands.timestamp = 0;
            robotCmd.id = (uint) id;
            robotCmd.wheelsspeed = true;
            robotCmd.velnormal = 0;
            robotCmd.veltangent = 0;
            robotCmd.velangular = 0;
            robotCmd.kickspeedx = kickSpeedX;
            robotCmd.kickspeedz = kickSpeedZ;
            robotCmd.spinner = Spinner;
            robotCmd.wheel1 = w1;
            robotCmd.wheel2 = w2;
            robotCmd.wheel3 = w3;
            robotCmd.wheel4 = w4;

            pkt.commands.robot_commands.Add(robotCmd);

            var stream = new MemoryStream();
            Serializer.Serialize(stream, pkt);
            var array = stream.ToArray();
            if (_controller.SerialChecked())
            {
                _controller.Send(array);
            }
            else
            {
                Client.Send(array, array.Length, new IPEndPoint(IPAddress.Broadcast, 20011));
            }
        }

        private static void RecieveData()
        {
            var endPoint = new IPEndPoint(IPAddress.Parse("224.5.23.2"), 10002);
            while (true)
            {
                Thread.Sleep(10);
                _data = Client.Receive(ref endPoint);
            }
        }
    }
}