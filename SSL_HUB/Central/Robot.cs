﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using messages_robocup_ssl_detection;
using messages_robocup_ssl_wrapper;
using SSL_HUB.Rrt;

namespace SSL_HUB.Central
{
    public class Robot
    {
        // TODO: Stop on grabbing ball

        private const double RobotRadius = 0.0875;
        private const double WheelRadius = 0.0289;
        private readonly Form1 _controller;
        private readonly Rrt.Rrt _rrt;
        private Thread _trackBallThread;

        public Robot(bool isYellow, int id, float velocity, float angularVelocity, Form1 controller)
        {
            IsYellow = isYellow;
            Id = id;
            Velocity = velocity;
            AngularVelocity = angularVelocity;
            Moving = false;
            GoalX = 0;
            GoalY = 0;
            GoalAngle = 0;
            CurrentX = 0;
            CurrentY = 0;
            CurrentAngle = 0;
            KickSpeedX = 0;
            KickSpeedZ = 0;
            BallX = 0;
            BallY = 0;
            TrackingBall = false;
            Path = new List<Node>();
            _controller = controller;
            _rrt = new Rrt.Rrt(controller.Radius/10, controller.FieldWidth/10, controller.FieldHeight/10, _controller);
            new Thread(MoveRobot).Start();
        }

        public int Id { get; private set; }
        public float Velocity { get; private set; }
        public float AngularVelocity { get; private set; }
        public bool IsYellow { get; private set; }
        public float GoalX { get; private set; }
        public float GoalY { get; private set; }
        public float GoalAngle { get; private set; }
        public float CurrentX { get; private set; }
        public float CurrentY { get; private set; }
        public float CurrentAngle { get; private set; }
        public bool Moving { get; private set; }
        public List<Node> Path { get; private set; }
        public float KickSpeedX { get; private set; }
        public float KickSpeedZ { get; private set; }
        public float BallX { get; private set; }
        public float BallY { get; private set; }
        public SSL_WrapperPacket Data { get; private set; }
        public bool TrackingBall { get; private set; }

        public void SetGoal(double goalX, double goalY, double goalAngle, double kickSpeedX, double kickSpeedZ)
        {
            GoalX = (float) goalX;
            GoalY = (float) goalY;
            GoalAngle = (float) goalAngle;
            KickSpeedX = (float) kickSpeedX;
            KickSpeedZ = (float) kickSpeedZ;
            if (!Moving)
            {
                Moving = true;
                new Thread(MoveRobot).Start();
            }
        }

        public void TrackBall()
        {
            if (!ReferenceEquals(null, _trackBallThread)) return;
            TrackingBall = true;
            _trackBallThread = new Thread(() =>
            {
                float oldGoalX = 99999;
                float oldGoalY = 99999;
                float oldGoalAngle = 0;
                while (TrackingBall)
                {
                    Thread.Sleep(10);
                    var goalX = BallX;
                    var goalY = BallY;
                    var currentX = CurrentX;
                    var currentY = CurrentY;
                    var currentAngle = CurrentAngle;
                    var goalAngle = (float) Math.Atan2(goalY - currentY, goalX - currentX);

                    var distance = Math.Sqrt(Math.Pow(goalX - oldGoalX, 2) + Math.Pow(goalY - oldGoalY, 2));
                    var dTheeta = Math.Abs(Helper.Rtd(goalAngle - currentAngle));
                    var distanceToBall = Math.Sqrt(Math.Pow(goalX - currentX, 2) + Math.Pow(goalY - currentY, 2));

                    if (distance > 10 && dTheeta > 5)
                    {
                        SetGoal(goalX, goalY, goalAngle, 0, 0);
                        oldGoalX = goalX;
                        oldGoalY = goalY;
                        oldGoalAngle = goalAngle;
                    }
                    else if (distance > 10)
                    {
                        SetGoal(goalX, goalY, oldGoalAngle, 0, 0);
                        oldGoalX = goalX;
                        oldGoalY = goalY;
                    }
                    else if (dTheeta > 5)
                    {
                        SetGoal(oldGoalX, oldGoalY, goalAngle, 0, 0);
                        oldGoalAngle = goalAngle;
                    }
                    if (distanceToBall <= 120 && dTheeta < 45)
                    {
                        TrackingBall = false;
                        Moving = false;
                    }
                }
            });
            _trackBallThread.Start();
        }

        public void SetCoordinates(SSL_WrapperPacket data)
        {
            try
            {
                Data = data;
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

                BallX = data.detection.balls[0].x;
                BallY = data.detection.balls[0].y;
            }
            catch (Exception ex)
            {
                _controller.PrintErrorMessage(ex);
            }
            SetBallPossesed();
        }

        public void StopBallTracking()
        {
            TrackingBall = false;
            _trackBallThread = null;
        }

        public void StopMoving()
        {
            Moving = false;
        }

        private void MoveRobot()
        {
            while (Moving)
            {
                Thread.Sleep(10);

                CalculatePath();

                var goalX = Path.First().X;
                var goalY = Path.First().Y;

                var angle1 = (CurrentAngle < 0) ? (float) (CurrentAngle + 2*Math.PI) : CurrentAngle;
                var angle2 = (GoalAngle < 0) ? (float) (GoalAngle + 2*Math.PI) : GoalAngle;

                var theeta = Math.Atan2(goalY - CurrentY, goalX - CurrentX) - CurrentAngle;
                var distance = Math.Sqrt(Math.Pow(CurrentX - goalX, 2) + Math.Pow(CurrentY - goalY, 2));
                double vx, vy, vw;
                double[] motorAlpha = {Helper.Dtr(45), Helper.Dtr(120), Helper.Dtr(-120), Helper.Dtr(-45)};

                if (distance > 100 && Math.Abs(Helper.Rtd(angle2 - angle1)) > 5)
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
                else if (Math.Abs(Helper.Rtd(angle2 - angle1)) > 5 && Path.Count == 1)
                {
                    vx = 0;
                    vy = 0;
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
                    vw = 0;
                }
                else
                {
                    vx = 0;
                    vy = 0;
                    vw = 0;
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

                Helper.SendData(IsYellow, Id, v1, v2, v3, v4, KickSpeedX, KickSpeedZ);
                KickSpeedX = 0;
                KickSpeedZ = 0;
            }
            Stop();
        }

        private void CalculatePath()
        {
            var obstacles = new List<Node>();
            try
            {
                var yellowRobots = new List<SSL_DetectionRobot>(Data.detection.robots_yellow);
                var blueRobots = new List<SSL_DetectionRobot>(Data.detection.robots_blue);
                if (IsYellow)
                {
                    obstacles.AddRange(
                        yellowRobots.Where(robot => robot.robot_id != Id)
                            .Select(robot => new Node(ScaleDownX(robot.x), ScaleDownY(robot.y))));
                    obstacles.AddRange(
                        blueRobots.Select(robot => new Node(ScaleDownX(robot.x), ScaleDownY(robot.y))));
                }
                else
                {
                    obstacles.AddRange(
                        blueRobots.Where(robot => robot.robot_id != Id)
                            .Select(robot => new Node(ScaleDownX(robot.x), ScaleDownY(robot.y))));
                    obstacles.AddRange(
                        yellowRobots.Select(robot => new Node(ScaleDownX(robot.x), ScaleDownY(robot.y))));
                }
            }
            catch (Exception ex)
            {
                _controller.PrintErrorMessage(ex);
            }

            _rrt.SetConditions(new Node(ScaleDownX(CurrentX), ScaleDownY(CurrentY)),
                new Node(ScaleDownX(GoalX), ScaleDownY(GoalY)), obstacles);

            var scaledPath = _rrt.GetPath();
            if (ReferenceEquals(null, scaledPath)) return;
            Path.Clear();

            Path.AddRange(scaledPath.Select(node => new Node(ScaleUpX(node.X), ScaleUpY(node.Y))));
        }

        private void SetBallPossesed()
        {
            var distance = Math.Sqrt(Math.Pow(CurrentX - BallX, 2) + Math.Pow(CurrentY - BallY, 2));
            var dTheeta = Math.Abs(Helper.Rtd(Math.Atan2(BallY - CurrentY, BallX - CurrentX) - CurrentAngle));
            if (distance < 120 && dTheeta < 45)
            {
                _controller.BallPossesedBy = this;
            }
            else if (_controller.BallPossesedBy == this)
            {
                _controller.BallPossesedBy = null;
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

        private void Stop()
        {
            Moving = false;
            Helper.SendData(IsYellow, Id, 0, 0, 0, 0, KickSpeedX, KickSpeedZ);
            KickSpeedX = 0;
            KickSpeedZ = 0;
            _controller.InvokeNextMove();
        }
    }
}