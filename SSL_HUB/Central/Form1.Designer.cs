namespace SSL_HUB.Central
{
    partial class Form1
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.Stop = new System.Windows.Forms.Button();
            this.TrackBall = new System.Windows.Forms.Button();
            this.Move = new System.Windows.Forms.Button();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.Angle = new System.Windows.Forms.TextBox();
            this.Y = new System.Windows.Forms.TextBox();
            this.X = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.Id = new System.Windows.Forms.TextBox();
            this.IsYellow = new System.Windows.Forms.CheckBox();
            this.DefendYellow = new System.Windows.Forms.Button();
            this.DefendBlue = new System.Windows.Forms.Button();
            this.MoveToBall = new System.Windows.Forms.Button();
            this.Kick = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // Stop
            // 
            this.Stop.Location = new System.Drawing.Point(279, 37);
            this.Stop.Name = "Stop";
            this.Stop.Size = new System.Drawing.Size(75, 23);
            this.Stop.TabIndex = 17;
            this.Stop.Text = "Stop";
            this.Stop.UseVisualStyleBackColor = true;
            this.Stop.Click += new System.EventHandler(this.Stop_Click);
            // 
            // TrackBall
            // 
            this.TrackBall.Location = new System.Drawing.Point(279, 64);
            this.TrackBall.Name = "TrackBall";
            this.TrackBall.Size = new System.Drawing.Size(75, 23);
            this.TrackBall.TabIndex = 16;
            this.TrackBall.Text = "Track Ball";
            this.TrackBall.UseVisualStyleBackColor = true;
            this.TrackBall.Click += new System.EventHandler(this.TrackBall_Click);
            // 
            // Move
            // 
            this.Move.Location = new System.Drawing.Point(279, 8);
            this.Move.Name = "Move";
            this.Move.Size = new System.Drawing.Size(75, 23);
            this.Move.TabIndex = 15;
            this.Move.Text = "Move";
            this.Move.UseVisualStyleBackColor = true;
            this.Move.Click += new System.EventHandler(this.Move_Click);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(41, 71);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(34, 13);
            this.label3.TabIndex = 14;
            this.label3.Text = "Angle";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(41, 45);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(14, 13);
            this.label2.TabIndex = 13;
            this.label2.Text = "Y";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(41, 18);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(14, 13);
            this.label1.TabIndex = 12;
            this.label1.Text = "X";
            // 
            // Angle
            // 
            this.Angle.Location = new System.Drawing.Point(118, 64);
            this.Angle.Name = "Angle";
            this.Angle.Size = new System.Drawing.Size(100, 20);
            this.Angle.TabIndex = 11;
            this.Angle.Text = "0";
            // 
            // Y
            // 
            this.Y.Location = new System.Drawing.Point(118, 38);
            this.Y.Name = "Y";
            this.Y.Size = new System.Drawing.Size(100, 20);
            this.Y.TabIndex = 10;
            this.Y.Text = "0";
            // 
            // X
            // 
            this.X.Location = new System.Drawing.Point(118, 12);
            this.X.Name = "X";
            this.X.Size = new System.Drawing.Size(100, 20);
            this.X.TabIndex = 9;
            this.X.Text = "0";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(41, 97);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(16, 13);
            this.label4.TabIndex = 19;
            this.label4.Text = "Id";
            // 
            // Id
            // 
            this.Id.Location = new System.Drawing.Point(118, 90);
            this.Id.Name = "Id";
            this.Id.Size = new System.Drawing.Size(100, 20);
            this.Id.TabIndex = 18;
            this.Id.Text = "0";
            // 
            // IsYellow
            // 
            this.IsYellow.AutoSize = true;
            this.IsYellow.Location = new System.Drawing.Point(118, 116);
            this.IsYellow.Name = "IsYellow";
            this.IsYellow.Size = new System.Drawing.Size(68, 17);
            this.IsYellow.TabIndex = 20;
            this.IsYellow.Text = "Is Yellow";
            this.IsYellow.UseVisualStyleBackColor = true;
            // 
            // DefendYellow
            // 
            this.DefendYellow.Location = new System.Drawing.Point(44, 153);
            this.DefendYellow.Name = "DefendYellow";
            this.DefendYellow.Size = new System.Drawing.Size(174, 23);
            this.DefendYellow.TabIndex = 21;
            this.DefendYellow.Text = "Defend Goal Yellow";
            this.DefendYellow.UseVisualStyleBackColor = true;
            this.DefendYellow.Click += new System.EventHandler(this.DefendYellow_Click);
            // 
            // DefendBlue
            // 
            this.DefendBlue.Location = new System.Drawing.Point(44, 182);
            this.DefendBlue.Name = "DefendBlue";
            this.DefendBlue.Size = new System.Drawing.Size(174, 23);
            this.DefendBlue.TabIndex = 22;
            this.DefendBlue.Text = "Defend Goal Blue";
            this.DefendBlue.UseVisualStyleBackColor = true;
            this.DefendBlue.Click += new System.EventHandler(this.DefendBlue_Click);
            // 
            // MoveToBall
            // 
            this.MoveToBall.Location = new System.Drawing.Point(279, 93);
            this.MoveToBall.Name = "MoveToBall";
            this.MoveToBall.Size = new System.Drawing.Size(75, 23);
            this.MoveToBall.TabIndex = 23;
            this.MoveToBall.Text = "Move to Ball";
            this.MoveToBall.UseVisualStyleBackColor = true;
            this.MoveToBall.Click += new System.EventHandler(this.MoveToBall_Click);
            // 
            // Kick
            // 
            this.Kick.Location = new System.Drawing.Point(279, 122);
            this.Kick.Name = "Kick";
            this.Kick.Size = new System.Drawing.Size(75, 23);
            this.Kick.TabIndex = 24;
            this.Kick.Text = "Kick";
            this.Kick.UseVisualStyleBackColor = true;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(384, 262);
            this.Controls.Add(this.Kick);
            this.Controls.Add(this.MoveToBall);
            this.Controls.Add(this.DefendBlue);
            this.Controls.Add(this.DefendYellow);
            this.Controls.Add(this.IsYellow);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.Id);
            this.Controls.Add(this.Stop);
            this.Controls.Add(this.TrackBall);
            this.Controls.Add(this.Move);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.Angle);
            this.Controls.Add(this.Y);
            this.Controls.Add(this.X);
            this.Name = "Form1";
            this.Text = "Form1";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button Stop;
        private System.Windows.Forms.Button TrackBall;
        private System.Windows.Forms.Button Move;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox Angle;
        private System.Windows.Forms.TextBox Y;
        private System.Windows.Forms.TextBox X;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox Id;
        private System.Windows.Forms.CheckBox IsYellow;
        private System.Windows.Forms.Button DefendYellow;
        private System.Windows.Forms.Button DefendBlue;
        private System.Windows.Forms.Button MoveToBall;
        private System.Windows.Forms.Button Kick;

    }
}

