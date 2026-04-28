using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace IAP_Demo
{
    public partial class ProgressBarForm : Form
    {
        public ProgressBarForm()
        {
            InitializeComponent();
            this.ControlBox = false;
        }

        public delegate void dSetProgress(int total, int current);
        public delegate void dSetOprationInfo(string str);
        public delegate void dSetControl();


        public void SetOprationInfo(string str)
        {
            if (this.InvokeRequired)
            {
                try
                {
                    this.Invoke(new dSetOprationInfo(this.SetOprationInfo),
                        new object[] { str });
                }
                catch { }
            }
            else
            {
                this.label1.Text = str;
            }
        }

        public void SetProgress(int total, int current)
        {
            if (this.InvokeRequired)
            {
                try
                {
                    this.Invoke(new dSetProgress(this.SetProgress),
                        new object[] { total, current });
                }
                catch { }
            }
            else
            {
                this.progressBar1.Maximum = total;
                this.progressBar1.Value = current;
                this.label2.Text = current.ToString() + "%";

                //YQ Test
                //达到最大值后退出
                if (this.progressBar1.Value == this.progressBar1.Maximum)
                {
                    this.DialogResult = DialogResult.Cancel;
                }
            }
        }

        public void SetAllProgress()
        {
            if (this.InvokeRequired)
            {
                try
                {                    
                    this.Invoke(new dSetControl(this.SetAllProgress),
                        new object[] { });
                }
                catch { }
            }
            else
            {
                this.progressBar1.Style = System.Windows.Forms.ProgressBarStyle.Marquee;
                this.label2.Text = string.Empty;
            }
        }

        public void CloseBar()
        {
            if (this.InvokeRequired)
            {
                try
                {
                    this.Invoke(new dSetControl(this.CloseBar),
                        new object[] { });
                }
                catch { }
            }
            else
            {
                this.progressBar1.Style = ProgressBarStyle.Continuous;
                this.DialogResult = DialogResult.Cancel;
            }
        }

        //取消操作
        //         private void btn_Cancel_Click(object sender, EventArgs e)
        //         {
        //             this.DialogResult = DialogResult.Cancel;
        //         }
    }
}
