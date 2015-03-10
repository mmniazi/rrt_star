using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Windows.Forms;

// TODO: Use delegates for game strategy. -- extended to v2
// TODO: For whole project handle ball and robot missing.
// TODO: Handle null for all kind of data.

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
            // TODO: change to proper method
            CheckForIllegalCrossThreadCalls = false;

            BlueRobots = new List<Robot>(5);
            YellowRobots = new List<Robot>(5);
            Radius = 200;
            FieldWidth = 6000;
            FieldHeight = 4000;
            BallPossesedBy = null;
            _serialWritter = new SerialPort();
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
                YellowRobots.ElementAt(id).SetGoal(x, y, angle);
            }
            else
            {
                BlueRobots.ElementAt(id).SetGoal(x, y, angle);
            }
        }

        private void Stop_Click(object sender, EventArgs e)
        {
            var id = Convert.ToInt32(Id.Text);
            if (IsYellow.Checked)
            {
                YellowRobots.ElementAt(id).Moving = false;
            }
            else
            {
                BlueRobots.ElementAt(id).Moving = false;
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
            if (IsYellow.Checked)
            {
                YellowRobots.ElementAt(id).KickSpeedX = (float) Convert.ToDouble(KickSpeedX.Text);
                YellowRobots.ElementAt(id).KickSpeedZ = (float) Convert.ToDouble(KickSpeedZ.Text);
            }
            else
            {
                BlueRobots.ElementAt(id).KickSpeedX = (float)Convert.ToDouble(KickSpeedX.Text);
                BlueRobots.ElementAt(id).KickSpeedZ = (float)Convert.ToDouble(KickSpeedZ.Text);
            }
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
            textBox3.AppendText("\n" + ex.Message + "\n");
        }
    }
}