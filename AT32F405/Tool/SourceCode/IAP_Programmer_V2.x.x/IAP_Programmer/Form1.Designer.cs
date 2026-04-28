namespace IAP_Demo
{
    partial class IAPDemo
    {
        /// <summary>
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 清理所有正在使用的资源。
        /// </summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows 窗体设计器生成的代码

        /// <summary>
        /// 设计器支持所需的方法 - 不要修改
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(IAPDemo));
            this.comboBoxPortName = new System.Windows.Forms.ComboBox();
            this.label2 = new System.Windows.Forms.Label();
            this.buttonBeginTest = new System.Windows.Forms.Button();
            this.buttonOpenfile1 = new System.Windows.Forms.Button();
            this.textBoxFile1Path = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.comboBox_PortType = new System.Windows.Forms.ComboBox();
            this.label3 = new System.Windows.Forms.Label();
            this.checkBox_CRC = new System.Windows.Forms.CheckBox();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.textBox_AppAddress = new System.Windows.Forms.TextBox();
            this.comboBoxBaudRate = new System.Windows.Forms.ComboBox();
            this.label6 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.textBox_I2CAddress = new System.Windows.Forms.TextBox();
            this.textBox_PID = new System.Windows.Forms.TextBox();
            this.label_PID = new System.Windows.Forms.Label();
            this.label_VID = new System.Windows.Forms.Label();
            this.textBox_VID = new System.Windows.Forms.TextBox();
            this.textBox_InterfaceNumber = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // comboBoxPortName
            // 
            this.comboBoxPortName.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxPortName.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.comboBoxPortName.FormattingEnabled = true;
            this.comboBoxPortName.Location = new System.Drawing.Point(101, 80);
            this.comboBoxPortName.Name = "comboBoxPortName";
            this.comboBoxPortName.Size = new System.Drawing.Size(121, 21);
            this.comboBoxPortName.TabIndex = 9;
            this.comboBoxPortName.SelectedIndexChanged += new System.EventHandler(this.comboBoxPortName_SelectedIndexChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label2.Location = new System.Drawing.Point(27, 83);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(67, 13);
            this.label2.TabIndex = 8;
            this.label2.Text = "Port Name";
            // 
            // buttonBeginTest
            // 
            this.buttonBeginTest.BackColor = System.Drawing.SystemColors.ButtonFace;
            this.buttonBeginTest.Font = new System.Drawing.Font("Verdana", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonBeginTest.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.buttonBeginTest.Location = new System.Drawing.Point(358, 195);
            this.buttonBeginTest.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.buttonBeginTest.Name = "buttonBeginTest";
            this.buttonBeginTest.Size = new System.Drawing.Size(112, 30);
            this.buttonBeginTest.TabIndex = 97;
            this.buttonBeginTest.Text = "Download";
            this.buttonBeginTest.UseVisualStyleBackColor = false;
            this.buttonBeginTest.Click += new System.EventHandler(this.buttonBeginTest_Click);
            // 
            // buttonOpenfile1
            // 
            this.buttonOpenfile1.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonOpenfile1.Location = new System.Drawing.Point(434, 166);
            this.buttonOpenfile1.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.buttonOpenfile1.Name = "buttonOpenfile1";
            this.buttonOpenfile1.Size = new System.Drawing.Size(37, 24);
            this.buttonOpenfile1.TabIndex = 96;
            this.buttonOpenfile1.Text = ".....";
            this.buttonOpenfile1.UseVisualStyleBackColor = true;
            this.buttonOpenfile1.Click += new System.EventHandler(this.buttonOpenfile1_Click);
            // 
            // textBoxFile1Path
            // 
            this.textBoxFile1Path.BackColor = System.Drawing.SystemColors.ButtonFace;
            this.textBoxFile1Path.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.textBoxFile1Path.Location = new System.Drawing.Point(30, 166);
            this.textBoxFile1Path.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.textBoxFile1Path.Name = "textBoxFile1Path";
            this.textBoxFile1Path.ReadOnly = true;
            this.textBoxFile1Path.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textBoxFile1Path.Size = new System.Drawing.Size(398, 21);
            this.textBoxFile1Path.TabIndex = 95;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label1.Location = new System.Drawing.Point(27, 143);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(135, 13);
            this.label1.TabIndex = 98;
            this.label1.Text = "App Start Address(0x)";
            // 
            // comboBox_PortType
            // 
            this.comboBox_PortType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBox_PortType.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.comboBox_PortType.FormattingEnabled = true;
            this.comboBox_PortType.Items.AddRange(new object[] {
            "USART",
            "USB",
            "CAN",
            "SPI",
            "I2C"});
            this.comboBox_PortType.Location = new System.Drawing.Point(101, 19);
            this.comboBox_PortType.Name = "comboBox_PortType";
            this.comboBox_PortType.Size = new System.Drawing.Size(121, 21);
            this.comboBox_PortType.TabIndex = 101;
            this.comboBox_PortType.SelectedIndexChanged += new System.EventHandler(this.comboBox_PortType_SelectedIndexChanged);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label3.Location = new System.Drawing.Point(27, 22);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(61, 13);
            this.label3.TabIndex = 100;
            this.label3.Text = "Port Type";
            // 
            // checkBox_CRC
            // 
            this.checkBox_CRC.AutoSize = true;
            this.checkBox_CRC.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.checkBox_CRC.Location = new System.Drawing.Point(314, 143);
            this.checkBox_CRC.Name = "checkBox_CRC";
            this.checkBox_CRC.Size = new System.Drawing.Size(178, 17);
            this.checkBox_CRC.TabIndex = 102;
            this.checkBox_CRC.Text = "CRC Verify after download";
            this.checkBox_CRC.UseVisualStyleBackColor = true;
            // 
            // timer1
            // 
            this.timer1.Enabled = true;
            this.timer1.Interval = 2000;
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // textBox_AppAddress
            // 
            this.textBox_AppAddress.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.textBox_AppAddress.Location = new System.Drawing.Point(160, 140);
            this.textBox_AppAddress.MaxLength = 8;
            this.textBox_AppAddress.Name = "textBox_AppAddress";
            this.textBox_AppAddress.Size = new System.Drawing.Size(120, 21);
            this.textBox_AppAddress.TabIndex = 110;
            this.textBox_AppAddress.Text = "08010000";
            this.textBox_AppAddress.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.textBox_AppAddress_KeyPress);
            // 
            // comboBoxBaudRate
            // 
            this.comboBoxBaudRate.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxBaudRate.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.comboBoxBaudRate.FormattingEnabled = true;
            this.comboBoxBaudRate.Items.AddRange(new object[] {
            "110",
            "300",
            "600",
            "1200",
            "2400",
            "4800",
            "9600",
            "14400",
            "19200",
            "38400",
            "57600",
            "115200",
            "128000",
            "230400",
            "256000",
            "460800",
            "921600",
            "1228800",
            "1382400"});
            this.comboBoxBaudRate.Location = new System.Drawing.Point(363, 81);
            this.comboBoxBaudRate.MaxLength = 10;
            this.comboBoxBaudRate.Name = "comboBoxBaudRate";
            this.comboBoxBaudRate.Size = new System.Drawing.Size(121, 21);
            this.comboBoxBaudRate.TabIndex = 112;
            this.comboBoxBaudRate.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.comboBoxBaudRate_KeyPress);
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label6.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label6.Location = new System.Drawing.Point(269, 84);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(66, 13);
            this.label6.TabIndex = 111;
            this.label6.Text = "Baud Rate";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label4.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label4.Location = new System.Drawing.Point(269, 22);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(77, 13);
            this.label4.TabIndex = 113;
            this.label4.Text = "Address(0x)";
            // 
            // textBox_I2CAddress
            // 
            this.textBox_I2CAddress.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.textBox_I2CAddress.Location = new System.Drawing.Point(363, 19);
            this.textBox_I2CAddress.MaxLength = 8;
            this.textBox_I2CAddress.Name = "textBox_I2CAddress";
            this.textBox_I2CAddress.Size = new System.Drawing.Size(121, 21);
            this.textBox_I2CAddress.TabIndex = 114;
            this.textBox_I2CAddress.Text = "A0";
            // 
            // textBox_PID
            // 
            this.textBox_PID.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.textBox_PID.Location = new System.Drawing.Point(363, 50);
            this.textBox_PID.MaxLength = 8;
            this.textBox_PID.Name = "textBox_PID";
            this.textBox_PID.Size = new System.Drawing.Size(121, 21);
            this.textBox_PID.TabIndex = 118;
            this.textBox_PID.Text = "AF01";
            // 
            // label_PID
            // 
            this.label_PID.AutoSize = true;
            this.label_PID.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label_PID.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label_PID.Location = new System.Drawing.Point(269, 53);
            this.label_PID.Name = "label_PID";
            this.label_PID.Size = new System.Drawing.Size(52, 13);
            this.label_PID.TabIndex = 117;
            this.label_PID.Text = "PID(0x)";
            // 
            // label_VID
            // 
            this.label_VID.AutoSize = true;
            this.label_VID.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label_VID.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label_VID.Location = new System.Drawing.Point(27, 53);
            this.label_VID.Name = "label_VID";
            this.label_VID.Size = new System.Drawing.Size(53, 13);
            this.label_VID.TabIndex = 115;
            this.label_VID.Text = "VID(0x)";
            // 
            // textBox_VID
            // 
            this.textBox_VID.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.textBox_VID.Location = new System.Drawing.Point(101, 50);
            this.textBox_VID.MaxLength = 8;
            this.textBox_VID.Name = "textBox_VID";
            this.textBox_VID.Size = new System.Drawing.Size(121, 21);
            this.textBox_VID.TabIndex = 119;
            this.textBox_VID.Text = "2E3C";
            // 
            // textBox_InterfaceNumber
            // 
            this.textBox_InterfaceNumber.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.textBox_InterfaceNumber.Location = new System.Drawing.Point(160, 110);
            this.textBox_InterfaceNumber.MaxLength = 8;
            this.textBox_InterfaceNumber.Name = "textBox_InterfaceNumber";
            this.textBox_InterfaceNumber.Size = new System.Drawing.Size(62, 21);
            this.textBox_InterfaceNumber.TabIndex = 133;
            this.textBox_InterfaceNumber.Text = "02";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label5.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label5.Location = new System.Drawing.Point(27, 112);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(113, 13);
            this.label5.TabIndex = 132;
            this.label5.Text = "Interface Num(0x)";
            // 
            // IAPDemo
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(494, 232);
            this.Controls.Add(this.textBox_InterfaceNumber);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.textBox_VID);
            this.Controls.Add(this.label_VID);
            this.Controls.Add(this.textBox_I2CAddress);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.comboBoxBaudRate);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.textBox_AppAddress);
            this.Controls.Add(this.checkBox_CRC);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.buttonBeginTest);
            this.Controls.Add(this.buttonOpenfile1);
            this.Controls.Add(this.textBoxFile1Path);
            this.Controls.Add(this.comboBoxPortName);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.textBox_PID);
            this.Controls.Add(this.label_PID);
            this.Controls.Add(this.comboBox_PortType);
            this.Controls.Add(this.label3);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.Name = "IAPDemo";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "IAP_Programmer_V2.0.9";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.IAPDemo_FormClosing);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ComboBox comboBoxPortName;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button buttonBeginTest;
        private System.Windows.Forms.Button buttonOpenfile1;
        private System.Windows.Forms.TextBox textBoxFile1Path;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox comboBox_PortType;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.CheckBox checkBox_CRC;
        private System.Windows.Forms.Timer timer1;
        private System.Windows.Forms.TextBox textBox_AppAddress;
        private System.Windows.Forms.ComboBox comboBoxBaudRate;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox textBox_I2CAddress;
        private System.Windows.Forms.TextBox textBox_PID;
        private System.Windows.Forms.Label label_PID;
        private System.Windows.Forms.Label label_VID;
        private System.Windows.Forms.TextBox textBox_VID;
        private System.Windows.Forms.TextBox textBox_InterfaceNumber;
        private System.Windows.Forms.Label label5;
    }
}

