using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;

namespace LibeliumOTAP
{
    public partial class Form2 : Form
    {
        const string fileName = "xbee.conf";

        public Form2()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string[] theSerialPortNames = System.IO.Ports.SerialPort.GetPortNames();

            comboBox1.Items.Clear();
            foreach (string comPort in theSerialPortNames)
            {
                comboBox1.Items.Add(comPort);
            }
            if (comboBox1.Items.Count > 0)
            {
                comboBox1.SelectedIndex = 0;
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            loadSettings();
        }

        private void saveSettings()
        {
            System.IO.StreamWriter tw = new System.IO.StreamWriter(fileName,false);
            
            tw.WriteLine("port = "+comboBox1.Text);
            tw.WriteLine("auth_key = " + textBox2.Text);
            tw.WriteLine("panID = " + textBox4.Text);
            tw.WriteLine("xbeeModel = " + comboBox2.Text);

            string encr = "off";
            if (checkBox1.Checked)
            {
                encr = "on";
            }
            tw.WriteLine("encryption = " + encr);
            tw.WriteLine("encryptionKey = " + textBox1.Text);
            if (checkBox2.Checked)
            {
                tw.WriteLine("discardedDataFile = " + textBox3.Text);
            }
            else
            {
                tw.WriteLine("#discardedDataFile = " + textBox3.Text);
            }
            tw.WriteLine("channel = " + textBox5.Text);
            tw.Flush();
            tw.Close();
        }

        private void loadSettings()
        {
            comboBox1.Text = "";
            comboBox2.Text = "";

            textBox1.Text = "";
            textBox2.Text = "";
            textBox3.Text = "";
            textBox4.Text = "";
            textBox5.Text = "";
            checkBox1.Checked = false;
            checkBox2.Checked = false;
            if (File.Exists(fileName))
                using (StreamReader sr = new StreamReader(fileName))
                {
                    String line;

                    while ((line = sr.ReadLine()) != null)
                    {
                        line = line.Trim();
                        if (line.Length > 0)
                        {
                            if (line[0] != '#') //Comments
                            {
                                string[] commands = line.Split(new string[] { "=" }, StringSplitOptions.RemoveEmptyEntries);
                                if (commands.Length == 2)
                                {
                                    commands[0] = commands[0].Trim();
                                    commands[1] = commands[1].Trim();
                                    switch (commands[0])
                                    {
                                        case "port": comboBox1.Text = commands[1]; break;
                                        case "auth_key": textBox2.Text = commands[1]; break;
                                        case "panID": textBox4.Text = commands[1]; break;
                                        case "xbeeModel": comboBox2.Text = commands[1]; break;
                                        case "encryption": checkBox1.Checked = commands[1] == "on"; break;
                                        case "encryptionKey": textBox1.Text = commands[1]; break;
                                        case "discardedDataFile": textBox3.Text = commands[1]; checkBox2.Checked = true; break;
                                        case "channel": textBox5.Text = commands[1]; break;
                                    }
                                }
                            }
                        }
                    }
                }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            saveSettings();
        }

        private void Form2_Load(object sender, EventArgs e)
        {
            loadSettings();
        }
    }
}
