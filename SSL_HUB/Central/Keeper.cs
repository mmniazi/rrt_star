﻿using System;
using System.Threading;

namespace SSL_HUB.Central
{
    public class Keeper
    {
        private const double RobotRadius = 0.0875;
        private const double WheelRadius = 0.0289;
        private const float Zero = (float) 0.000000000001;
        private readonly float _angularVelocity;
        private readonly bool _isYellow;
        private readonly float _velocity;

        public Keeper(bool isYellow, float velocity, float angularVelocity)
        {
            _isYellow = isYellow;
            _velocity = velocity;
            _angularVelocity = angularVelocity;
            new Thread(Trace).Start();
        }

        private void Trace()
        {
            float ballX = 0;
            float ballY = 0;
            while (true)
            {
                Thread.Sleep(10);
                float currentX, currentY, currentAngle, goalY, goalAngle;
                var data = Helper.GetData();
                var ballX1 = data.detection.balls[0].x;
                var ballY1 = data.detection.balls[0].y;
                if (_isYellow)
                {
                    currentX = data.detection.robots_yellow[5].x;
                    currentY = data.detection.robots_yellow[5].y;
                    currentAngle = data.detection.robots_yellow[5].orientation;
                }
                else
                {
                    currentX = data.detection.robots_blue[5].x;
                    currentY = data.detection.robots_blue[5].y;
                    currentAngle = data.detection.robots_blue[5].orientation;
                }

                var ballAngle = Math.Atan2(ballY1 - ballY, ballX1 - ballX);
                float goalX = (_isYellow) ? 2800 : -2800;
                var angle = Helper.Rtd((ballAngle < 0) ? (float) (ballAngle + 2*Math.PI) : ballAngle);
                if (_isYellow && (angle >= 90 && angle <= 270))
                {
                    goalY = 0;
                    goalAngle = (float) Math.PI;
                }
                else if (!_isYellow && !(angle >= 90 && angle <= 270))
                {
                    goalY = 0;
                    goalAngle = 0;
                }
                else
                {
                    if (_isYellow)
                    {
                        goalAngle = (float) (ballAngle + Math.PI);
                        goalY = (float) (ballY1 - (ballX1 - goalX)/Math.Tan(goalAngle));
                    }
                    else
                    {
                        goalY = 0;
                        goalAngle = 0;
                    }
                }

                if (goalY < -700)
                {
                    goalY = -700;
                }
                else if (goalY > 700)
                {
                    goalY = 700;
                }

                var angle1 = (currentAngle < 0) ? (float) (currentAngle + 2*Math.PI) : currentAngle;
                var angle2 = (goalAngle < 0) ? (float) (goalAngle + 2*Math.PI) : goalAngle;

                var theeta = Math.Atan2(goalY - currentY, goalX - currentX) - currentAngle;
                var distance = Math.Sqrt(Math.Pow(currentX - goalX, 2) + Math.Pow(currentY - goalY, 2));
                double vx, vy, vw;
                double[] motorAlpha = {Helper.Dtr(45), Helper.Dtr(120), Helper.Dtr(-120), Helper.Dtr(-45)};

                if (distance > 100 && Math.Abs(Helper.Rtd(angle2 - angle1)) > 5)
                {
                    vx = _velocity*Math.Cos(theeta);
                    vy = _velocity*Math.Sin(theeta);
                    if (Math.Sin(angle2 - angle1) > 0)
                    {
                        vw = _angularVelocity;
                    }
                    else
                    {
                        vw = -_angularVelocity;
                    }
                }
                else if (distance > 100)
                {
                    vx = _velocity*Math.Cos(theeta);
                    vy = _velocity*Math.Sin(theeta);
                    vw = Zero;
                }
                else if (Math.Abs(Helper.Rtd(angle2 - angle1)) > 5)
                {
                    vx = Zero;
                    vy = Zero;
                    if (Math.Sin(angle2 - angle1) > 0)
                    {
                        vw = _angularVelocity;
                    }
                    else
                    {
                        vw = -_angularVelocity;
                    }
                }
                else
                {
                    vx = Zero;
                    vy = Zero;
                    vw = Zero;
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

                Helper.SendData(_isYellow, 5, v1, v2, v3, v4);

//                ballX = ballX1;
//                ballY = ballY1;
            }
        }
    }
}