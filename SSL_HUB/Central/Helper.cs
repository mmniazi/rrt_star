using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using grSim;
using messages_robocup_ssl_detection;
using messages_robocup_ssl_wrapper;
using ProtoBuf;

namespace SSL_HUB.Central
{
    internal static class Helper
    {
        private static readonly UdpClient Client;
        private static Form1 _controller;
        private static SSL_WrapperPacket _data;

        static Helper()
        {
            Client = new UdpClient {ExclusiveAddressUse = false};
            Client.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            Client.ExclusiveAddressUse = false;
            Client.Client.Bind(new IPEndPoint(IPAddress.Any, 10020));
            Client.JoinMulticastGroup(IPAddress.Parse("224.5.23.2"));
            Spinner = true;
            InitializeData();
        }

        public static bool Spinner { get; set; }

        public static void StartRecieving()
        {
            new Thread(RecieveData).Start();
        }

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

        public static void SendData(bool isYellow, int id, float w1, float w2, float w3, float w4, float kickSpeedX,
            float kickSpeedZ)
        {
            if (_controller.SerialChecked())
            {
                var cmdPacket = new byte[10];

                var btemp = BitConverter.GetBytes((int) Math.Abs(w1));
                cmdPacket[0] = btemp[0];

                btemp = BitConverter.GetBytes((int) Math.Abs(w2));
                cmdPacket[1] = btemp[0];

                btemp = BitConverter.GetBytes((int) Math.Abs(w3));
                cmdPacket[2] = btemp[0];

                btemp = BitConverter.GetBytes((int) Math.Abs(w4));
                cmdPacket[3] = btemp[0];

                var sign = Math.Sign(w1);
                switch (sign)
                {
                    case 1:
                        cmdPacket[4] = 1;
                        break;
                    case -1:
                        cmdPacket[4] = 0;
                        break;
                }
                sign = Math.Sign(w2);
                switch (sign)
                {
                    case 1:
                        cmdPacket[5] = 1;
                        break;
                    case -1:
                        cmdPacket[5] = 0;
                        break;
                }
                sign = Math.Sign(w3);
                switch (sign)
                {
                    case 1:
                        cmdPacket[6] = 1;
                        break;
                    case -1:
                        cmdPacket[6] = 0;
                        break;
                }
                sign = Math.Sign(w4);
                switch (sign)
                {
                    case 1:
                        cmdPacket[7] = 1;
                        break;
                    case -1:
                        cmdPacket[7] = 0;
                        break;
                }

                btemp = BitConverter.GetBytes((int) Math.Abs(kickSpeedX));
                cmdPacket[8] = btemp[0];

                btemp = BitConverter.GetBytes((int) Math.Abs(kickSpeedZ));
                cmdPacket[9] = btemp[0];

                _controller.Send(cmdPacket);
            }
            else
            {
                const float zero = (float) 0.000000000001;
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
                robotCmd.wheel1 = (Math.Abs(w1) < 0.00001) ? zero : w1;
                robotCmd.wheel2 = (Math.Abs(w2) < 0.00001) ? zero : w2;
                robotCmd.wheel3 = (Math.Abs(w3) < 0.00001) ? zero : w3;
                robotCmd.wheel4 = (Math.Abs(w4) < 0.00001) ? zero : w4;

                pkt.commands.robot_commands.Add(robotCmd);

                var stream = new MemoryStream();
                Serializer.Serialize(stream, pkt);
                var array = stream.ToArray();
                try
                {
                    Client.Send(array, array.Length, new IPEndPoint(IPAddress.Broadcast, 20011));
                }
                catch (Exception ex)
                {
                    _controller.PrintErrorMessage(ex);
                }
            }
        }

        private static void RecieveData()
        {
            var endPoint = new IPEndPoint(IPAddress.Parse("224.5.23.2"), 10020);
            while (true)
            {
                Thread.Sleep(10);
                try
                {
                    var packet = Client.Receive(ref endPoint);
                    if (ReferenceEquals(null, packet)) continue;
                    var data = Serializer.Deserialize<SSL_WrapperPacket>(new MemoryStream(packet));
                    data.detection.robots_blue.ForEach(
                        robot => _data.detection.robots_blue[(int) robot.robot_id] = robot);
                    data.detection.robots_yellow.ForEach(
                        robot => _data.detection.robots_yellow[(int) robot.robot_id] = robot);
                    data.detection.balls.ForEach(ball => _data.detection.balls[0] = ball);
                }
                catch (Exception ex)
                {
                    _controller.PrintErrorMessage(ex);
                }
                _controller.YellowKeeper.SetCoordinates(_data);
                _controller.BlueKeeper.SetCoordinates(_data);
                _controller.YellowRobots.ForEach(robot => robot.SetCoordinates(_data));
                _controller.BlueRobots.ForEach(robot => robot.SetCoordinates(_data));
            }
        }

        private static void InitializeData()
        {
            _data = new SSL_WrapperPacket
            {
                detection = new SSL_DetectionFrame
                {
                    robots_yellow =
                    {
                        new SSL_DetectionRobot
                        {
                            robot_id = 0,
                            x = 0,
                            y = 0
                        },
                        new SSL_DetectionRobot
                        {
                            robot_id = 1,
                            x = 0,
                            y = 0
                        },
                        new SSL_DetectionRobot
                        {
                            robot_id = 2,
                            x = 0,
                            y = 0
                        },
                        new SSL_DetectionRobot
                        {
                            robot_id = 3,
                            x = 0,
                            y = 0
                        },
                        new SSL_DetectionRobot
                        {
                            robot_id = 4,
                            x = 0,
                            y = 0
                        },
                        new SSL_DetectionRobot
                        {
                            robot_id = 5,
                            x = 0,
                            y = 0
                        }
                    }
                    ,
                    robots_blue =
                    {
                        new SSL_DetectionRobot
                        {
                            robot_id = 0,
                            x = 0,
                            y = 0
                        },
                        new SSL_DetectionRobot
                        {
                            robot_id = 1,
                            x = 0,
                            y = 0
                        },
                        new SSL_DetectionRobot
                        {
                            robot_id = 2,
                            x = 0,
                            y = 0
                        },
                        new SSL_DetectionRobot
                        {
                            robot_id = 3,
                            x = 0,
                            y = 0
                        },
                        new SSL_DetectionRobot
                        {
                            robot_id = 4,
                            x = 0,
                            y = 0
                        },
                        new SSL_DetectionRobot
                        {
                            robot_id = 5,
                            x = 0,
                            y = 0
                        }
                    },
                    balls = {new SSL_DetectionBall {x = 0, y = 0, z = 0}}
                }
            };
        }
    }
}