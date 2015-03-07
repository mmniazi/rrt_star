using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Windows.Forms;

namespace SSL_HUB.Central
{
    public partial class Form1 : Form
    {
        private const float Velocity = (float) 0.5;
        private const float AngularVelocity = (float) (Math.PI/4);
        private readonly SerialPort _serialWritter;

        public Form1()
        {
            InitializeComponent();


            BlueRobots = new List<Robot>(5);
            YellowRobots = new List<Robot>(5);
            Radius = 200;
            FieldWidth = 6000;
            FieldHeight = 4000;
            BallPossesedBy = -1;
            _serialWritter = new SerialPort();
            Helper.SetController(this);

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
        public int BallPossesedBy { get; set; }

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
                YellowRobots.ElementAt(id).Stop();
            }
            else
            {
                BlueRobots.ElementAt(id).Stop();
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
                YellowRobots.ElementAt(id).StopTrackBall();
            }
            else
            {
                BlueRobots.ElementAt(id).StopTrackBall();
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

        private void Kick_Click(object sender, EventArgs e)
        {
            var id = Convert.ToInt32(Id.Text);
            if (IsYellow.Checked)
            {
                YellowRobots.ElementAt(id)
                    .Kick((float) Convert.ToDouble(KickSpeedX.Text), (float) Convert.ToDouble(KickSpeedZ.Text));
            }
            else
            {
                BlueRobots.ElementAt(id)
                    .Kick((float) Convert.ToDouble(KickSpeedX.Text), (float) Convert.ToDouble(KickSpeedZ.Text));
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
                textBox3.AppendText("\n" + ex.Message + "\n");
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
            COMPort.SelectedIndex = 0;
        }
    }
}