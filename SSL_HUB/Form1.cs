using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace SSL_HUB
{
    public partial class Form1 : Form
    {
        private readonly List<Robot> _blueRobots;
        private readonly List<Robot> _yellowRobots;

        public Form1()
        {
            InitializeComponent();

            _blueRobots = new List<Robot>(6);
            _yellowRobots = new List<Robot>(6);

            for (var i = 0; i < 6; i++)
            {
                _yellowRobots.Add(new Robot(true, i, 1));
                _blueRobots.Add(new Robot(false, i, 1));
            }
        }

        private void Move_Click(object sender, EventArgs e)
        {
            var id = Convert.ToInt32(Id.Text);
            var x = Convert.ToDouble(X.Text);
            var y = Convert.ToDouble(Y.Text);
            var angle = Helper.Dtr(Convert.ToDouble(Angle.Text));

            if (IsYellow.Checked)
            {
                _yellowRobots.ElementAt(id).SetGoal(x, y, angle);
            }
            else
            {
                _blueRobots.ElementAt(id).SetGoal(x, y, angle);
            }
        }

        private void TrackBall_Click(object sender, EventArgs e)
        {
            if (IsYellow.Checked)
            {
                var id = Convert.ToInt32(Id.Text);
                var wrapper = Helper.ReceiveData();
                var currentX = wrapper.detection.robots_yellow[id].x;
                var currentY = wrapper.detection.robots_yellow[id].y;
                var goalX = wrapper.detection.balls[0].x;
                var goalY = wrapper.detection.balls[0].y;
                var goalAngle = Math.Atan2(goalY - currentY, goalX - currentX);

                _yellowRobots.ElementAt(id).SetGoal(goalX, goalY, goalAngle);
            }
            else
            {
                var id = Convert.ToInt32(Id.Text);
                var wrapper = Helper.ReceiveData();
                var currentX = wrapper.detection.robots_blue[id].x;
                var currentY = wrapper.detection.robots_blue[id].y;
                var goalX = wrapper.detection.balls[0].x;
                var goalY = wrapper.detection.balls[0].y;
                var goalAngle = (float) (Math.Atan2(goalY - currentY, goalX - currentX));

                _blueRobots.ElementAt(id).SetGoal(goalX, goalY, goalAngle);
            }
        }

        private void Stop_Click(object sender, EventArgs e)
        {
            var id = Convert.ToInt32(Id.Text);
            if (IsYellow.Checked)
            {
                _yellowRobots.ElementAt(id).Stop();
            }
            else
            {
                _blueRobots.ElementAt(id).Stop();
            }
        }
    }
}