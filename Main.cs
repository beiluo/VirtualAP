﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows.Forms;

namespace VirtualAP
{
    public partial class frmMain : Form
    {
        public frmMain()
        {
            InitializeComponent();
        }

        #region event handler

        private void btnInitialAndStartAP_Click(object sender, EventArgs e)
        {
            bool isInputValid = ValidInput();

            if (!isInputValid)
                return;

            var ssid = this.txtSSID.Text.Trim();
            var password = this.txtPassword.Text.Trim();

            InitialAndStartAP(ssid, password);
        }

        private void btnStartAP_Click(object sender, EventArgs e)
        {
            StartAP();
        }
        
        private void btnStopAP_Click(object sender, EventArgs e)
        {
            this.txtOutput.Text = string.Empty;

            string cmd = string.Format(@"netsh wlan stop hostednetwork");
            ExecuteCmd(cmd);
        }

        void cmdProcess_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            this.txtOutput.Text += (e.Data + "\r\n");
        }

        void cmdProcess_ErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            this.txtOutput.Text += (e.Data + "\r\n");
        }

        private void frmMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                SaveData();
            }
            catch { }
        }

        private void frmMain_Load(object sender, EventArgs e)
        {
            try
            {
                LoadVersion();
                LoadData();
            }
            catch { }
        }

        #endregion

        #region private methods

        private bool ValidInput()
        {
            string ssid = this.txtSSID.Text.Trim();
            string password = this.txtPassword.Text.Trim();

            if (string.IsNullOrEmpty(ssid))
            {
                MessageBox.Show("SSID cannot be empty./SSID不能为空。");
                return false;
            }

            if (password.Length < 8)
            {
                MessageBox.Show("Password needs at least 8 characters./密码至少需要8位。");
                return false;
            }

            return true;
        }

        private void InitialAndStartAP(string ssid, string password)
        {
            this.txtOutput.Text = string.Empty;

            string cmd = string.Format(@"netsh wlan set hostednetwork mode=allow ssid={0} key=""{1}""", ssid, password);
            ExecuteCmd(cmd);

            StartAP();
        }


        private void StartAP()
        {
            this.txtOutput.Text = string.Empty;

            string cmd = string.Format(@"netsh wlan start hostednetwork");
            ExecuteCmd(cmd);
        }

        private void ExecuteCmd(string cmd)
        {
            ProcessStartInfo cmdStartInfo = new ProcessStartInfo();
            cmdStartInfo.FileName = @"C:\Windows\System32\cmd.exe";
            cmdStartInfo.RedirectStandardOutput = true;
            cmdStartInfo.RedirectStandardError = true;
            cmdStartInfo.RedirectStandardInput = true;
            cmdStartInfo.UseShellExecute = false;
            cmdStartInfo.CreateNoWindow = true;

            Process cmdProcess = new Process();
            cmdProcess.StartInfo = cmdStartInfo;
            cmdProcess.ErrorDataReceived += cmdProcess_ErrorDataReceived;
            cmdProcess.OutputDataReceived += cmdProcess_OutputDataReceived;
            cmdProcess.EnableRaisingEvents = true;
            cmdProcess.Start();
            cmdProcess.BeginOutputReadLine();
            cmdProcess.BeginErrorReadLine();

            cmdProcess.StandardInput.WriteLine(cmd);     //Execute ping bing.com
            cmdProcess.StandardInput.WriteLine("exit");                  //Execute exit.

            cmdProcess.WaitForExit();
        }


        private void LoadVersion()
        {
            this.lVersion.Text = string.Format("Ver:{0}", Assembly.GetExecutingAssembly().GetName().Version);
        }
        
        private void LoadData()
        {
            if (!File.Exists(Consts.DATA_FILE_NAME))
                return;

            string data;
            using (StreamReader reader = new StreamReader(Consts.DATA_FILE_NAME))
            {
                data = reader.ReadLine();
            }

            var info = JsonConvert.DeserializeObject<APInfo>(data);

            this.txtSSID.Text = info.SSID;
            this.txtPassword.Text = info.Password;
        }

        private void SaveData()
        {
            APInfo info = new APInfo
            {
                SSID = this.txtSSID.Text,
                Password = this.txtPassword.Text
            };

            string json = JsonConvert.SerializeObject(info);

            using (StreamWriter writer = new StreamWriter(Consts.DATA_FILE_NAME))
            {
                writer.Write(json);
            }
        }

        #endregion

        

        
    }
}
