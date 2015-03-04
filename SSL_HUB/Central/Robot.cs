using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using SSL_HUB.Rrt;

namespace SSL_HUB.Central
{
    public class Robot
    {
        private const double RobotRadius = 0.0875;
        private const double WheelRadius = 0.0289;
        private const float Zero = (float) 0.000000000001;
        private readonly Rrt.Rrt _rrt;

        public Robot(bool isYellow, int id, float velocity, float angularVelocity, Form1 controller)
        {
            IsYellow = isYellow;
            Id = id;
            Velocity = velocity;
            AngularVelocity = angularVelocity;
            Moving = false;
            Path = new List<Node>();
            _rrt = new Rrt.Rrt(controller.Radius/10, controller.FieldWidth/10, controller.FieldHeight/10);
            new Thread(MoveRobot).Start();
        }

        public int Id { get; private set; }
        public float Velocity { get; set; }
        public float AngularVelocity { get; set; }
        public bool IsYellow { get; private set; }
        public float GoalX { get; private set; }
        public float GoalY { get; private set; }
        public float GoalAngle { get; private set; }
        public float CurrentX { get; private set; }
        public float CurrentY { get; private set; }
        public float CurrentAngle { get; private set; }
        public bool Moving { get; private set; }
        public List<Node> Path { get; private set; }

        public void SetGoal(double goalX, double goalY, double goalAngle)
        {
            GoalX = (float) goalX;
            GoalY = (float) goalY;
            GoalAngle = (float) goalAngle;
            Moving = true;
        }
        // TODO: robot vibrating at angles greater then 180 and path is not smooth
        private void MoveRobot()
        {
            while (true)
            {
                if (!Moving)
                {
                    Thread.Sleep(10);
                    Helper.SendData(IsYellow, Id, Zero, Zero, Zero, Zero);
                }
                else
                {
                    var data = Helper.GetData();
                    if (IsYellow)
                    {
                        CurrentX = data.detection.robots_yellow[Id].x;
                        CurrentY = data.detection.robots_yellow[Id].y;
                        CurrentAngle = data.detection.robots_yellow[Id].orientation;
                    }
                    else
                    {
                        CurrentX = data.detection.robots_blue[Id].x;
                        CurrentY = data.detection.robots_blue[Id].y;
                        CurrentAngle = data.detection.robots_blue[Id].orientation;
                    }

                    CalculatePath();
                    var goalX = Path.First().X;
                    var goalY = Path.First().Y;
                    var angle1 = (CurrentAngle < 0) ? (float) (CurrentAngle + 2*Math.PI) : CurrentAngle;
                    var angle2 = (GoalAngle < 0) ? (float) (GoalAngle + 2*Math.PI) : GoalAngle;

                    var theeta = Math.Atan2(goalY - CurrentY, goalX - CurrentX) - CurrentAngle;
                    var distance = Math.Sqrt(Math.Pow(CurrentX - goalX, 2) + Math.Pow(CurrentY - goalY, 2));
                    double vx, vy, vw;
                    double[] motorAlpha = {Helper.Dtr(45), Helper.Dtr(120), Helper.Dtr(-120), Helper.Dtr(-45)};

                    if (distance > 100 && Math.Abs(Helper.Rtd(GoalAngle - CurrentAngle)) > 5)
                    {
                        vx = Velocity*Math.Cos(theeta);
                        vy = Velocity*Math.Sin(theeta);
                        if (Math.Sin(angle2 - angle1) > 0)
                        {
                            vw = AngularVelocity;
                        }
                        else
                        {
                            vw = -AngularVelocity;
                        }
                    }
                    else if (distance > 100)
                    {
                        vx = Velocity*Math.Cos(theeta);
                        vy = Velocity*Math.Sin(theeta);
                        vw = Zero;
                    }
                    else if (Math.Abs(Helper.Rtd(GoalAngle - CurrentAngle)) > 5)
                    {
                        vx = Zero;
                        vy = Zero;
                        if (Math.Sin(angle2 - angle1) > 0)
                        {
                            vw = AngularVelocity;
                        }
                        else
                        {
                            vw = -AngularVelocity;
                        }
                    }
                    else
                    {
                        vx = Zero;
                        vy = Zero;
                        vw = Zero;
                        if (Path.Count == 1)
                        {
                            Moving = false;
                        }
                        else
                        {
                            Path.RemoveAt(0);
                        }
                    }

                    var v1 =
                        (float)
                            ((1.0/WheelRadius)*
                             (((RobotRadius*vw) - (vx*Math.Sin(motorAlpha[0])) + (vy*Math.Cos(motorAlpha[0])))));
                    var v2 =
                        (float)
                            ((1.0/WheelRadius)*
                             (((RobotRadius*vw) - (vx*Math.Sin(motorAlpha[1])) + (vy*Math.Cos(motorAlpha[1])))));
                    var v3 =
                        (float)
                            ((1.0/WheelRadius)*
                             (((RobotRadius*vw) - (vx*Math.Sin(motorAlpha[2])) + (vy*Math.Cos(motorAlpha[2])))));
                    var v4 =
                        (float)
                            ((1.0/WheelRadius)*
                             (((RobotRadius*vw) - (vx*Math.Sin(motorAlpha[3])) + (vy*Math.Cos(motorAlpha[3])))));

                    Helper.SendData(IsYellow, Id, v1, v2, v3, v4);
                }
            }
        }

        private void CalculatePath()
        {
            var data = Helper.GetData();
            var obstacles = new List<Node>();

            obstacles.AddRange(
                data.detection.robots_yellow.Select(robot => new Node(ScaleDownX(robot.x), ScaleDownY(robot.y))));
            obstacles.AddRange(
                data.detection.robots_blue.Select(robot => new Node(ScaleDownX(robot.x), ScaleDownY(robot.y))));

            if (IsYellow)
            {
                obstacles.RemoveAt(Id);
            }
            else
            {
                obstacles.RemoveAt(Id + 6);
            }

            _rrt.SetConditions(new Node(ScaleDownX(CurrentX), ScaleDownY(CurrentY)),
                new Node(ScaleDownX(GoalX), ScaleDownY(GoalY)), obstacles);

            var scaledPath = _rrt.GetPath();
            Path.Clear();
            foreach (var node in scaledPath)
            {
                Path.Add(new Node(ScaleUpX(node.X), ScaleUpY(node.Y)));
            }
        }

        private static int ScaleDownX(float x)
        {
            return (int) ((x + 3000)/10);
        }

        private static int ScaleDownY(float y)
        {
            return (int) ((y + 2000)/10);
        }

        private static int ScaleUpX(int x)
        {
            return x*10 - 3000;
        }

        private static int ScaleUpY(int y)
        {
            return y*10 - 2000;
        }

        public void Stop()
        {
            Moving = false;
        }
    }
}