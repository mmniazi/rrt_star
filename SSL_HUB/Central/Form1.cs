﻿using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Threading;
using System.Windows.Forms;

namespace SSL_HUB.Central
{
    public partial class Form1 : Form
    {
        private const float Velocity = 1;
        private const float AngularVelocity = (float) (Math.PI/2);
        private readonly SerialPort _serialWritter;
        private Thread _moveToBall;
        private Thread _trackBall;

        public Form1()
        {
            InitializeComponent();


            BlueRobots = new List<Robot>(5);
            YellowRobots = new List<Robot>(5);
            Radius = 200;
            FieldWidth = 6000;
            FieldHeight = 4000;
            _serialWritter = new SerialPort();

            for (var i = 0; i < 5; i++)
            {
                YellowRobots.Add(new Robot(true, i, Velocity, AngularVelocity, this));
                BlueRobots.Add(new Robot(false, i, Velocity, AngularVelocity, this));
            }
        }

        public List<Robot> BlueRobots { get; private set; }
        public List<Robot> YellowRobots { get; private set; }
        public int Radius { get; private set; }
        public int FieldWidth { get; private set; }
        public int FieldHeight { get; private set; }
        public Keeper YellowKeeper { get; private set; }
        public Keeper BlueKeeper { get; private set; }

        private void Move_Click(object sender, EventArgs e)
        {
            var id = Convert.ToInt32(Id.Text);
            var x = Convert.ToDouble(X.Text);
            var y = Convert.ToDouble(Y.Text);
            var angle = Helper.Dtr(Convert.ToDouble(Angle.Text));

            if (IsYellow.Checked)
            {
                YellowRobots.ElementAt(id).SetGoal(x, y, angle);
            }
            else
            {
                BlueRobots.ElementAt(id).SetGoal(x, y, angle);
            }
        }

        private void TrackBall_Click(object sender, EventArgs e)
        {
            if (!ReferenceEquals(null, _moveToBall))
            {
                _moveToBall.Abort();
            }
            if (!ReferenceEquals(null, _trackBall))
            {
                _trackBall.Abort();
            }
            _trackBall = new Thread(() =>
            {
                float oldGoalX = 99999;
                float oldGoalY = 99999;
                float oldGoalAngle = 0;
                while (true)
                {
                    Thread.Sleep(10);
                    var id = Convert.ToInt32(Id.Text);
                    var data = Helper.GetData();
                    var isYellow = IsYellow.Checked;

                    var currentX = (isYellow)
                        ? data.detection.robots_yellow[id].x
                        : data.detection.robots_blue[id].x;
                    var currentY = (isYellow)
                        ? data.detection.robots_yellow[id].y
                        : data.detection.robots_blue[id].y;
                    var currentAngle = (isYellow)
                        ? data.detection.robots_yellow[id].orientation
                        : data.detection.robots_blue[id].orientation;
                    var goalX = data.detection.balls[0].x;
                    var goalY = data.detection.balls[0].y;
                    var goalAngle = (float) Math.Atan2(goalY - currentY, goalX - currentX);

                    var distance = Math.Sqrt(Math.Pow(goalX - oldGoalX, 2) + Math.Pow(goalY - oldGoalY, 2));
                    var dTheeta = Math.Abs(Helper.Rtd(goalAngle - currentAngle));

                    if (isYellow)
                    {
                        if (distance > 10 && dTheeta > 5)
                        {
                            YellowRobots.ElementAt(id).SetGoal(goalX, goalY, goalAngle);
                            oldGoalX = goalX;
                            oldGoalY = goalY;
                            oldGoalAngle = goalAngle;
                        }
                        else if (distance > 10)
                        {
                            YellowRobots.ElementAt(id).SetGoal(goalX, goalY, oldGoalAngle);
                            oldGoalX = goalX;
                            oldGoalY = goalY;
                        }
                        else if (dTheeta > 5)
                        {
                            YellowRobots.ElementAt(id).SetGoal(oldGoalX, oldGoalY, goalAngle);
                            oldGoalAngle = goalAngle;
                        }
                    }
                    else
                    {
                        if (distance > 10 && dTheeta > 5)
                        {
                            BlueRobots.ElementAt(id).SetGoal(goalX, goalY, goalAngle);
                            oldGoalX = goalX;
                            oldGoalY = goalY;
                            oldGoalAngle = goalAngle;
                        }
                        else if (distance > 10)
                        {
                            BlueRobots.ElementAt(id).SetGoal(goalX, goalY, oldGoalAngle);
                            oldGoalX = goalX;
                            oldGoalY = goalY;
                        }
                        else if (dTheeta > 5)
                        {
                            BlueRobots.ElementAt(id).SetGoal(oldGoalX, oldGoalY, goalAngle);
                            oldGoalAngle = goalAngle;
                        }
                    }
                }
            });
            _trackBall.Start();
        }

        private void Stop_Click(object sender, EventArgs e)
        {
            var id = Convert.ToInt32(Id.Text);
            if (IsYellow.Checked)
            {
                YellowRobots.ElementAt(id).Stop();
            }
            else
            {
                BlueRobots.ElementAt(id).Stop();
            }
        }

        private void DefendYellow_Click(object sender, EventArgs e)
        {
            YellowKeeper = new Keeper(true, Velocity, AngularVelocity);
        }

        private void DefendBlue_Click(object sender, EventArgs e)
        {
            BlueKeeper = new Keeper(false, Velocity, AngularVelocity);
        }

        private void MoveToBall_Click(object sender, EventArgs e)
        {
            if (!ReferenceEquals(null, _moveToBall))
            {
                _moveToBall.Abort();
            }
            if (!ReferenceEquals(null, _trackBall))
            {
                _trackBall.Abort();
            }
            _moveToBall = new Thread(() =>
            {
                var id = Convert.ToInt32(Id.Text);
                var data = Helper.GetData();
                var isYellow = IsYellow.Checked;
                var goalX = data.detection.balls[0].x;
                var goalY = data.detection.balls[0].y;
                while (true)
                {
                    Thread.Sleep(10);

                    var currentX = (isYellow)
                        ? data.detection.robots_yellow[id].x
                        : data.detection.robots_blue[id].x;
                    var currentY = (isYellow)
                        ? data.detection.robots_yellow[id].y
                        : data.detection.robots_blue[id].y;
                    var currentAngle = (isYellow)
                        ? data.detection.robots_yellow[id].orientation
                        : data.detection.robots_blue[id].orientation;
                    var goalAngle = (float) Math.Atan2(goalY - currentY, goalX - currentX);

                    var dTheeta = Math.Abs(Helper.Rtd(goalAngle - currentAngle));

                    if (isYellow && dTheeta > 5)
                    {
                        YellowRobots.ElementAt(id).SetGoal(goalX, goalY, goalAngle);
                    }
                    else if (dTheeta > 5)
                    {
                        BlueRobots.ElementAt(id).SetGoal(goalX, goalY, goalAngle);
                    }
                }
            });
            _moveToBall.Start();
        }

        private void OpenSerial_Click(object sender, EventArgs e)
        {
            try
            {
                _serialWritter.PortName = textBox1.Text;
                _serialWritter.BaudRate = int.Parse(textBox2.Text);
                _serialWritter.Open();
            }
            catch (Exception ex)
            {
                textBox3.AppendText("\nSerial Port error : " + ex.Message + "\n");
            }
        }

        private void CloseSerial_Click(object sender, EventArgs e)
        {
            try
            {
                _serialWritter.Close();
            }
            catch (Exception ex)
            {
                textBox3.AppendText(ex.Message + "\n");
            }
        }

        private void Send(char motion)
        {
            try
            {
                if (_serialWritter.IsOpen)
                    _serialWritter.Write(motion.ToString());
                else
                {
                    _serialWritter.Open();
                    _serialWritter.Write(motion.ToString());
                }
            }
            catch (Exception ex)
            {
                textBox3.AppendText("\n" + ex.Message + "\n");
            }
        }

        private void Kick_Click(object sender, EventArgs e)
        {
            var id = Convert.ToInt32(Id.Text);
            if (IsYellow.Checked)
            {
                YellowRobots.ElementAt(id).KickSpeedX = (float) Convert.ToDouble(KickSpeedX.Text);
                YellowRobots.ElementAt(id).KickSpeedZ = (float) Convert.ToDouble(KickSpeedZ.Text);
            }
            else
            {
                BlueRobots.ElementAt(id).KickSpeedX = (float) Convert.ToDouble(KickSpeedX.Text);
                BlueRobots.ElementAt(id).KickSpeedZ = (float) Convert.ToDouble(KickSpeedZ.Text);
            }
        }

        private void Spinner_CheckedChanged(object sender, EventArgs e)
        {
            Helper.Spinner = Spinner.Checked;
        }
    }
}