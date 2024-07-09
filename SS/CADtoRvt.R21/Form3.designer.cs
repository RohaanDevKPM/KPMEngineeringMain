namespace KPMEngineeringB.R
{
    partial class Form3
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form3));
            this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            this.label1 = new System.Windows.Forms.Label();
            this.cb_CAD_Name = new System.Windows.Forms.ComboBox();
            this.label2 = new System.Windows.Forms.Label();
            this.cb_CAD_Layer = new System.Windows.Forms.ComboBox();
            this.label3 = new System.Windows.Forms.Label();
            this.cb_CableTray_Type = new System.Windows.Forms.ComboBox();
            this.label5 = new System.Windows.Forms.Label();
            this.tb_MiddleEle = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.button1 = new System.Windows.Forms.Button();
            this.btn_OK = new System.Windows.Forms.Button();
            this.linkLabel1 = new System.Windows.Forms.LinkLabel();
            this.bt_Finish = new System.Windows.Forms.Button();
            this.tb_Width = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.tb_Height = new System.Windows.Forms.TextBox();
            this.flowLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // flowLayoutPanel1
            // 
            this.flowLayoutPanel1.Controls.Add(this.label1);
            this.flowLayoutPanel1.Controls.Add(this.cb_CAD_Name);
            this.flowLayoutPanel1.Controls.Add(this.label2);
            this.flowLayoutPanel1.Controls.Add(this.cb_CAD_Layer);
            this.flowLayoutPanel1.Controls.Add(this.label3);
            this.flowLayoutPanel1.Controls.Add(this.cb_CableTray_Type);
            this.flowLayoutPanel1.Location = new System.Drawing.Point(8, 12);
            this.flowLayoutPanel1.Name = "flowLayoutPanel1";
            this.flowLayoutPanel1.Size = new System.Drawing.Size(466, 179);
            this.flowLayoutPanel1.TabIndex = 0;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(3, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(178, 20);
            this.label1.TabIndex = 0;
            this.label1.Text = "Select CAD File Name";
            // 
            // cb_CAD_Name
            // 
            this.cb_CAD_Name.BackColor = System.Drawing.SystemColors.Window;
            this.cb_CAD_Name.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cb_CAD_Name.FormattingEnabled = true;
            this.cb_CAD_Name.Location = new System.Drawing.Point(3, 23);
            this.cb_CAD_Name.Name = "cb_CAD_Name";
            this.cb_CAD_Name.Size = new System.Drawing.Size(460, 26);
            this.cb_CAD_Name.TabIndex = 1;
            this.cb_CAD_Name.SelectedIndexChanged += new System.EventHandler(this.cb_CAD_Name_SelectedIndexChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(3, 52);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(193, 20);
            this.label2.TabIndex = 2;
            this.label2.Text = "Select CAD Layer Name";
            // 
            // cb_CAD_Layer
            // 
            this.cb_CAD_Layer.BackColor = System.Drawing.SystemColors.Window;
            this.cb_CAD_Layer.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cb_CAD_Layer.FormattingEnabled = true;
            this.cb_CAD_Layer.Location = new System.Drawing.Point(3, 75);
            this.cb_CAD_Layer.Name = "cb_CAD_Layer";
            this.cb_CAD_Layer.Size = new System.Drawing.Size(460, 26);
            this.cb_CAD_Layer.TabIndex = 3;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(3, 104);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(183, 20);
            this.label3.TabIndex = 4;
            this.label3.Text = "Select Cable Tray Type";
            // 
            // cb_CableTray_Type
            // 
            this.cb_CableTray_Type.BackColor = System.Drawing.SystemColors.Window;
            this.cb_CableTray_Type.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cb_CableTray_Type.FormattingEnabled = true;
            this.cb_CableTray_Type.Location = new System.Drawing.Point(3, 127);
            this.cb_CableTray_Type.Name = "cb_CableTray_Type";
            this.cb_CableTray_Type.Size = new System.Drawing.Size(460, 26);
            this.cb_CableTray_Type.TabIndex = 5;
            this.cb_CableTray_Type.SelectedIndexChanged += new System.EventHandler(this.cb_CableTray_Type_SelectedIndexChanged);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(11, 210);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(194, 20);
            this.label5.TabIndex = 8;
            this.label5.Text = "Middle Elevation (in mm)";
            // 
            // tb_MiddleEle
            // 
            this.tb_MiddleEle.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.tb_MiddleEle.BackColor = System.Drawing.SystemColors.Window;
            this.tb_MiddleEle.Location = new System.Drawing.Point(318, 203);
            this.tb_MiddleEle.Name = "tb_MiddleEle";
            this.tb_MiddleEle.Size = new System.Drawing.Size(153, 27);
            this.tb_MiddleEle.TabIndex = 9;
            this.tb_MiddleEle.TextChanged += new System.EventHandler(this.tb_MiddleEle_TextChanged);
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(11, 251);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(115, 20);
            this.label6.TabIndex = 10;
            this.label6.Text = "Width (in mm)";
            // 
            // button1
            // 
            this.button1.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.button1.Location = new System.Drawing.Point(316, 413);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(115, 40);
            this.button1.TabIndex = 1;
            this.button1.Text = "Cancel";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // btn_OK
            // 
            this.btn_OK.Location = new System.Drawing.Point(147, 362);
            this.btn_OK.Name = "btn_OK";
            this.btn_OK.Size = new System.Drawing.Size(168, 40);
            this.btn_OK.TabIndex = 0;
            this.btn_OK.Text = "Create Cable Trays";
            this.btn_OK.UseVisualStyleBackColor = true;
            this.btn_OK.Click += new System.EventHandler(this.btn_OK_Click);
            // 
            // linkLabel1
            // 
            this.linkLabel1.AutoSize = true;
            this.linkLabel1.Location = new System.Drawing.Point(4, 474);
            this.linkLabel1.Name = "linkLabel1";
            this.linkLabel1.Size = new System.Drawing.Size(139, 20);
            this.linkLabel1.TabIndex = 2;
            this.linkLabel1.TabStop = true;
            this.linkLabel1.Text = "KPM-Engineering";
            this.linkLabel1.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabel1_LinkClicked);
            // 
            // bt_Finish
            // 
            this.bt_Finish.Location = new System.Drawing.Point(35, 413);
            this.bt_Finish.Name = "bt_Finish";
            this.bt_Finish.Size = new System.Drawing.Size(115, 40);
            this.bt_Finish.TabIndex = 12;
            this.bt_Finish.Text = "Finish";
            this.bt_Finish.UseVisualStyleBackColor = true;
            this.bt_Finish.Click += new System.EventHandler(this.bt_Finish_Click);
            // 
            // tb_Width
            // 
            this.tb_Width.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.tb_Width.BackColor = System.Drawing.SystemColors.Window;
            this.tb_Width.Location = new System.Drawing.Point(318, 248);
            this.tb_Width.Name = "tb_Width";
            this.tb_Width.Size = new System.Drawing.Size(153, 27);
            this.tb_Width.TabIndex = 14;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(11, 290);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(121, 20);
            this.label4.TabIndex = 15;
            this.label4.Text = "Height (in mm)";
            // 
            // tb_Height
            // 
            this.tb_Height.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.tb_Height.BackColor = System.Drawing.SystemColors.Window;
            this.tb_Height.Location = new System.Drawing.Point(318, 287);
            this.tb_Height.Name = "tb_Height";
            this.tb_Height.Size = new System.Drawing.Size(153, 27);
            this.tb_Height.TabIndex = 16;
            // 
            // Form3
            // 
            this.AcceptButton = this.btn_OK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(10F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.GradientInactiveCaption;
            this.CancelButton = this.button1;
            this.ClientSize = new System.Drawing.Size(478, 499);
            this.Controls.Add(this.tb_Height);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.tb_Width);
            this.Controls.Add(this.bt_Finish);
            this.Controls.Add(this.linkLabel1);
            this.Controls.Add(this.btn_OK);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.flowLayoutPanel1);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.tb_MiddleEle);
            this.Controls.Add(this.label5);
            this.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(4);
            this.Name = "Form3";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "CAD to Revit Cable Tray";
            this.Load += new System.EventHandler(this.Form3_Load);
            this.flowLayoutPanel1.ResumeLayout(false);
            this.flowLayoutPanel1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox cb_CAD_Name;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ComboBox cb_CAD_Layer;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button btn_OK;
        private System.Windows.Forms.LinkLabel linkLabel1;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.ComboBox cb_CableTray_Type;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox tb_MiddleEle;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Button bt_Finish;
        private System.Windows.Forms.TextBox tb_Width;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox tb_Height;
    }
}