using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Windows.Forms;

// TODO: Use delegates for game strategy. -- extended to v2

namespace SSL_HUB.Central
{
    public partial class Form1 : Form
    {
        private const float Velocity = 1;
        private const float AngularVelocity = (float) (Math.PI/2);
        private readonly SerialPort _serialWritter;

        public Form1()
        {
            InitializeComponent();

            BlueRobots = new List<Robot>(5);
            YellowRobots = new List<Robot>(5);
            Radius = 200;
            FieldWidth = 6000;
            FieldHeight = 4000;
            BallPossesedBy = null;
            _serialWritter = new SerialPort();
            Strategy = new List<Action>();
            Helper.SetController(this);

            for (var i = 0; i < 5; i++)
            {
                YellowRobots.Add(new Robot(true, i, Velocity, AngularVelocity, this));
                BlueRobots.Add(new Robot(false, i, Velocity, AngularVelocity, this));
            }
            YellowKeeper = new Keeper(true, Velocity, AngularVelocity, this);
            BlueKeeper = new Keeper(false, Velocity, AngularVelocity, this);

            Helper.StartRecieving();
        }

        public List<Action> Strategy { get; private set; }
        public List<Robot> BlueRobots { get; private set; }
        public List<Robot> YellowRobots { get; private set; }
        public int Radius { get; private set; }
        public int FieldWidth { get; private set; }
        public int FieldHeight { get; private set; }
        public Keeper YellowKeeper { get; private set; }
        public Keeper BlueKeeper { get; private set; }
        public Robot BallPossesedBy { get; set; }

        private void Move_Click(object sender, EventArgs e)
        {
            var id = Convert.ToInt32(Id.Text);
            var x = Convert.ToDouble(X.Text);
            var y = Convert.ToDouble(Y.Text);
            var angle = Helper.Dtr(Convert.ToDouble(Angle.Text));

            if (IsYellow.Checked)
            {
                YellowRobots.ElementAt(id).SetGoal(x, y, angle, 0, 0);
            }
            else
            {
                BlueRobots.ElementAt(id).SetGoal(x, y, angle, 0, 0);
            }
        }

        private void Stop_Click(object sender, EventArgs e)
        {
            var id = Convert.ToInt32(Id.Text);
            if (IsYellow.Checked)
            {
                YellowRobots.ElementAt(id).StopMoving();
            }
            else
            {
                BlueRobots.ElementAt(id).StopMoving();
            }
        }

        private void TrackBall_Click(object sender, EventArgs e)
        {
            var isYellow = IsYellow.Checked;
            var id = Convert.ToInt32(Id.Text);
            if (isYellow)
            {
                YellowRobots.ElementAt(id).TrackBall();
            }
            else
            {
                BlueRobots.ElementAt(id).TrackBall();
            }
        }

        private void StopBallTracking_Click(object sender, EventArgs e)
        {
            var isYellow = IsYellow.Checked;
            var id = Convert.ToInt32(Id.Text);
            if (isYellow)
            {
                YellowRobots.ElementAt(id).StopBallTracking();
            }
            else
            {
                BlueRobots.ElementAt(id).StopBallTracking();
            }
        }

        private void Kick_Click(object sender, EventArgs e)
        {
            var id = Convert.ToInt32(Id.Text);
            var robot = (IsYellow.Checked) ? YellowRobots.ElementAt(id) : BlueRobots.ElementAt(id);
            robot.SetGoal(robot.CurrentX, robot.CurrentY, robot.CurrentAngle, Convert.ToDouble(KickSpeedX.Text),
                Convert.ToDouble(KickSpeedZ.Text));
        }

        private void Spinner_CheckedChanged(object sender, EventArgs e)
        {
            Helper.Spinner = Spinner.Checked;
        }

        private void OpenSerial_Click(object sender, EventArgs e)
        {
            try
            {
                _serialWritter.PortName = COMPort.Text;
                _serialWritter.BaudRate = int.Parse(textBox2.Text);
                _serialWritter.Open();
            }
            catch (Exception ex)
            {
                PrintErrorMessage(ex);
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
                PrintErrorMessage(ex);
            }
        }

        public void Send(byte[] data)
        {
            try
            {
                if (_serialWritter.IsOpen)
                    _serialWritter.Write(data.ToString());
                else
                {
                    _serialWritter.Open();
                    _serialWritter.Write(data.ToString());
                }
            }
            catch (Exception ex)
            {
                PrintErrorMessage(ex);
            }
        }

        public bool SerialChecked()
        {
            return Serial.Checked;
        }

        private void Strategy1_Click(object sender, EventArgs e)
        {
            var robot1 = BlueRobots.ElementAt(0);
            var robot2 = BlueRobots.ElementAt(1);
            var robot3 = BlueRobots.ElementAt(2);
            Strategy.Clear();
            Strategy.AddRange(new List<Action>
            {
                () => robot1.TrackBall(),
                () =>
                {
                    robot1.SetGoal(0, 0, Helper.Dtr(-45), 0, 0);
                    robot2.SetGoal(750, -750, Helper.Dtr(135), 0, 0);
                    robot3.SetGoal(2250, 750, Helper.Dtr(225), 0, 0);
                },
                () => { },
                () => { },
                () => robot1.SetGoal(0, 0, Helper.Dtr(-45), 5, 0),
                () => robot2.SetGoal(750, -750, Helper.Dtr(45), 0, 0),
                () => robot2.SetGoal(750, -750, Helper.Dtr(45), 5, 0),
                () => robot3.SetGoal(2250, 750, Helper.Dtr(-45), 0, 0),
                () => robot3.SetGoal(2250, 750, Helper.Dtr(-45), 5, 0)
            });
            Strategy.First().Invoke();
        }

        private void Strategy2_Click(object sender, EventArgs e)
        {
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            COMPort.DataSource = SerialPort.GetPortNames();
            COMPort.SelectedIndex = 1;
        }

        public void PrintErrorMessage(Exception ex)
        {
            Invoke((MethodInvoker) delegate { textBox3.AppendText("\n" + ex.Message + "\n"); });
        }

        public void InvokeNextMove()
        {
            if (Strategy.Count <= 1) return;
            Strategy.RemoveAt(0);
            Strategy.First().Invoke();
        }
    }
}