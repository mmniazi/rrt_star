using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace SSL_HUB.Central
{
    public partial class Form1 : Form
    {
        private const float Velocity = 1;
        private const float AngularVelocity = (float) (Math.PI/2);

        public Form1()
        {
            InitializeComponent();

            BlueRobots = new List<Robot>(6);
            YellowRobots = new List<Robot>(6);
            Radius = 200;
            FieldWidth = 6000;
            FieldHeight = 4000;

            for (var i = 0; i < 6; i++)
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
            if (IsYellow.Checked)
            {
                var id = Convert.ToInt32(Id.Text);
                var data = Helper.GetData();
                var currentX = data.detection.robots_yellow[id].x;
                var currentY = data.detection.robots_yellow[id].y;
                var goalX = data.detection.balls[0].x;
                var goalY = data.detection.balls[0].y;
                var goalAngle = Math.Atan2(goalY - currentY, goalX - currentX);

                YellowRobots.ElementAt(id).SetGoal(goalX, goalY, goalAngle);
            }
            else
            {
                var id = Convert.ToInt32(Id.Text);
                var data = Helper.GetData();
                var currentX = data.detection.robots_blue[id].x;
                var currentY = data.detection.robots_blue[id].y;
                var goalX = data.detection.balls[0].x;
                var goalY = data.detection.balls[0].y;
                var goalAngle = (float) (Math.Atan2(goalY - currentY, goalX - currentX));

                BlueRobots.ElementAt(id).SetGoal(goalX, goalY, goalAngle);
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

        private void DefendYellow_Click(object sender, EventArgs e)
        {
            YellowKeeper = new Keeper(true, Velocity, AngularVelocity);
        }

        private void DefendBlue_Click(object sender, EventArgs e)
        {
            BlueKeeper = new Keeper(false, Velocity, AngularVelocity);
        }
    }
}