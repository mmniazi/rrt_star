using System;
using System.Threading;

namespace SSL_HUB.Central
{
    public class Keeper
    {
        private const double RobotRadius = 0.0875;
        private const double WheelRadius = 0.0289;

        public Keeper(bool isYellow, float velocity, float angularVelocity)
        {
            IsYellow = isYellow;
            Velocity = velocity;
            AngularVelocity = angularVelocity;
            KickSpeedX = 0;
            KickSpeedZ = 0;
            BallX = 0;
            BallY = 0;
            new Thread(Track).Start();
        }

        public float AngularVelocity { get; private set; }
        public bool IsYellow { get; private set; }
        public float Velocity { get; private set; }
        public float KickSpeedX { get; set; }
        public float KickSpeedZ { get; set; }
        public float BallX { get; private set; }
        public float BallY { get; private set; }
        public float CurrentX { get; private set; }
        public float CurrentY { get; private set; }
        public float CurrentAngle { get; private set; }
        public float GoalY { get; private set; }
        public float GoalX { get; private set; }
        public float GoalAngle { get; private set; }

        private void Track()
        {
            while (true)
            {
                Thread.Sleep(10);
                FindGoal();
                MoveToGoal();
            }
        }

        private void FindGoal()
        {
            var data = Helper.GetData();
            var ballX1 = data.detection.balls[0].x;
            var ballY1 = data.detection.balls[0].y;

            if (IsYellow)
            {
                CurrentX = data.detection.robots_yellow[5].x;
                CurrentY = data.detection.robots_yellow[5].y;
                CurrentAngle = data.detection.robots_yellow[5].orientation;
            }
            else
            {
                CurrentX = data.detection.robots_blue[5].x;
                CurrentY = data.detection.robots_blue[5].y;
                CurrentAngle = data.detection.robots_blue[5].orientation;
            }

            var ballAngle = Math.Atan2(ballY1 - BallY, ballX1 - BallX);
            var angle = Helper.Rtd((ballAngle < 0) ? (float)(ballAngle + 2 * Math.PI) : ballAngle);
            if (IsYellow && (angle >= 90 && angle <= 270))
            {
                GoalY = 0;
                GoalAngle = (float)Math.PI;
                BallX = ballX1;
                BallY = ballY1;
            }
            else if (!IsYellow && angle <= 90 && angle >= 270)
            {
                GoalY = 0;
                GoalAngle = 0;
                BallX = ballX1;
                BallY = ballY1;
            }
            else if (Math.Sqrt(Math.Pow(ballX1 - BallX, 2) + Math.Pow(ballY1 - BallY, 2)) < 5)
            {
                GoalY = CurrentY;
                GoalAngle = (float)(Math.Atan2(ballY1 - CurrentY, ballX1 - CurrentX));
            }
            else
            {
                GoalAngle = (float)(ballAngle + Math.PI);

                // Parameters for eq of ball line
                var a1 = ballY1 - BallY;
                var b1 = BallX - ballX1;
                var c1 = a1 * BallX + b1 * BallY;

                // Parameters for eq of goal line
                const int a2 = 700 - (-700);
                const int b2 = 2800 - 2800;
                var c2 = a2 * ((IsYellow) ? 2800 : -2800) + b2 * -700;

                var det = a1 * b2 - a2 * b1;
                GoalY = (a1 * c2 - a2 * c1) / det;

                BallX = ballX1;
                BallY = ballY1;
            }

            GoalX = (IsYellow) ? 2800 : -2800;

            if (GoalY < -700)
            {
                GoalY = -700;
            }
            else if (GoalY > 700)
            {
                GoalY = 700;
            }
        }

        private void MoveToGoal()
        {
            var angle1 = (CurrentAngle < 0) ? (float) (CurrentAngle + 2*Math.PI) : CurrentAngle;
            var angle2 = (GoalAngle < 0) ? (float) (GoalAngle + 2*Math.PI) : GoalAngle;

            var theeta = Math.Atan2(GoalY - CurrentY, GoalX - CurrentX) - CurrentAngle;
            var distance = Math.Sqrt(Math.Pow(CurrentX - GoalX, 2) + Math.Pow(CurrentY - GoalY, 2));
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
            else if (distance > 100)
            {
                vx = Velocity*Math.Cos(theeta);
                vy = Velocity*Math.Sin(theeta);
                vw = 0;
            }
            else if (Math.Abs(Helper.Rtd(angle2 - angle1)) > 5)
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
            else
            {
                vx = 0;
                vy = 0;
                vw = 0;
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

            Helper.SendData(IsYellow, 5, v1, v2, v3, v4, KickSpeedX, KickSpeedZ);
            KickSpeedX = 0;
            KickSpeedZ = 0;
        }
    }
}