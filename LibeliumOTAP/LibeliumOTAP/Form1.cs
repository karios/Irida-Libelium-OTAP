using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;


using System.Threading;
using System.Diagnostics;
using System.IO;
using LibeliumOTAP;
using System.Net;

namespace LibeliumOTAP
{
    
    public partial class Form1 : Form
    {
        

        const string lib = "lib64";
        public Form1()
        {
            InitializeComponent();
        }

        scanType nextScanType = scanType.none;
            

        enum scanType
        {
            scan,
            flush,
            startProgram,
            reset,
            bootList,
            deleteProgram,
            none

        }

        private void runProcess(string FileName, string Arguments)
        {
            // prep process
            ProcessStartInfo psi = new ProcessStartInfo(FileName, Arguments);
            psi.UseShellExecute = false;
            psi.CreateNoWindow = true;
            psi.RedirectStandardOutput = true;
            psi.RedirectStandardError = true;
            // start process
            using (Process process = new Process())
            {
                // pass process data
                process.StartInfo = psi;
                // prep for multithreaded logging
                ProcessOutputHandler outputHandler = new ProcessOutputHandler(process);
                Thread stdOutReader = new Thread(new ThreadStart(outputHandler.ReadStdOut));
                Thread stdErrReader = new Thread(new ThreadStart(outputHandler.ReadStdErr));
                // start process and stream readers
                process.Start();
                stdOutReader.Start();
                stdErrReader.Start();
                // wait for process to complete
                            //while (process.a
                //ActiveForm.Invalidate();
                process.Exited += new EventHandler(processFileEvent);

               
                process.WaitForExit();
                
            }
        }

        private void processFileEvent(object sender, System.EventArgs e)
        {
            processOutputFile(nextScanType, "output.txt");
        }

        private void executeCommand(string command, string parameters)
        {
            //ProcessOutputHandler exe = new ProcessOutputHandler();

            //MessageBox.Show(runProcess(command, parameters));
            Thread t = new Thread(() => runProcess(command, parameters)); // Kick off a new thread
            
            t.Start();
            

        }

        private string getMac(string fullString)
        {
            return fullString.Substring(19, 16);
        }

        private string getName(string fullString)
        {
            return fullString.Substring(0, 16);
        }
 

        private void button1_Click(object sender, EventArgs e)
        {
            checkedListBox1.Items.Clear();
            toolStripStatusLabel1.Text = "Scanning...";
            button1.Enabled = false;
            //System.Diagnostics.Process.Start("..\\otap\\otap64.bat","-scan_nodes --mode BROADCAST --time 2");
            if (checkBox1.Checked)
            {
                string macAddress = Microsoft.VisualBasic.Interaction.InputBox("Please enter Waspmote MAC", "MAC", "", 0, 0);
                if (macAddress != "")
                {
                    executeCommand("java", "-Djava.library.path=../otap/"+lib+"/ -jar ../otap/otap.jar -scan_nodes --mode UNICAST --mac "+macAddress+" --time "+scanTime.Value.ToString());
                }
            }
            else
            {
                executeCommand("java", "-Djava.library.path=../otap/"+lib+"/ -jar ../otap/otap.jar -scan_nodes --mode BROADCAST --time "+scanTime.Value.ToString());
            }
            button1.Enabled = true;
            processOutputFile(scanType.scan);
            toolStripStatusLabel1.Text = "Ready - " + checkedListBox1.Items.Count.ToString()+ " nodes found!";
            selectAll();
        }

        private void processOutputFile(scanType newScanType)
        {
            nextScanType = newScanType;
            processOutputFile(nextScanType, "output.txt");
        }

        private void processOutputFile(scanType newScanType,string filename)
        {
            switch (newScanType)
            {
                case scanType.scan:
                    listBox1.Items.Clear();
                    checkedListBox1.Items.Clear();
                    break;
                case scanType.bootList:
                    listBox1.Items.Clear();
                    break;
            }
            if (File.Exists(filename))
            {
                try
                {
                    using (StreamReader sr = new StreamReader(filename))
                    {
                        String line;

                        while ((line = sr.ReadLine()) != null)
                        {
                            line = line.Trim();
                            if (line.Length > 0)
                            {
                                string finalString = "";
                                switch (newScanType)
                                {
                                    case scanType.scan:
                                        if (line.Contains("READY"))
                                        {
                                            finalString = line.Substring(30, 16) + " - " + line.Substring(11, 16);
                                            if (checkedListBox1.Items.IndexOf(finalString) == -1)
                                            {
                                                checkedListBox1.Items.Add(finalString);
                                            }
                                        }

                                        break;
                                    case scanType.bootList:

                                        if (line.Contains("Bootlist Node"))
                                        {
                                            finalString = line.Substring(9, 21);
                                        }
                                        else
                                            if (line.Contains("PID:"))
                                            {
                                                finalString = line.Substring(10, 7);
                                            }
                                        if (finalString != "")
                                        {
                                            listBox1.Items.Add(finalString);
                                        }
                                        break;
                                }
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    return;
                }
            }
        }

        private void changeSettingsToolStripMenuItem_Click(object sender, System.EventArgs e)
        {
            Form2 SettingsForm = new Form2();
            SettingsForm.ShowDialog();
        }

        private void button2_Click(object sender, System.EventArgs e)
        {
            selectAll();
        }

        private void selectAll()
        {
            for (int i = 0; i < checkedListBox1.Items.Count; i++)
            {
                checkedListBox1.SetItemCheckState(i, CheckState.Checked);
            }
        }

        private void button3_Click(object sender, System.EventArgs e)
        {
            for (int i = 0; i < checkedListBox1.Items.Count; i++)
            {
                checkedListBox1.SetItemCheckState(i, CheckState.Unchecked);
            }
        }

        private void inverseClick(object sender, System.EventArgs e)
        {
            for (int i = 0; i < checkedListBox1.Items.Count; i++)
            {
                CheckState inverseState = checkedListBox1.SelectedIndices.Contains(i)?CheckState.Unchecked:CheckState.Checked;

                checkedListBox1.SetItemCheckState(i, inverseState);
            }
        }

        private void checkBox1_CheckedChanged(object sender, System.EventArgs e)
        {

        }

        private void resetButtonClick(object sender, EventArgs e)
        {
            toolStripStatusLabel1.Text = "Resetting...";
            System.IO.StreamWriter tw = new System.IO.StreamWriter("macsToReset.txt", false);

            for (int x = 0; x <= checkedListBox1.CheckedItems.Count - 1; x++)
            {
                string macAddress = getMac(checkedListBox1.CheckedItems[x].ToString());
                tw.WriteLine(macAddress);

            }

            tw.Flush();
            tw.Close();
            string args = "-Djava.library.path=../otap/" + lib + "/ -jar ../otap/otap.jar -reset --mode MULTICAST --macs_file macsToReset.txt";
            executeCommand("java", args);
            toolStripStatusLabel1.Text = "Ready";
        }

        private void button6_Click(object sender, EventArgs e)
        {
            if (label2.Text== "")
            {
                MessageBox.Show("Please enter a pID first!");
                
            }
            else
            if (openFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                
            

                toolStripStatusLabel1.Text = "Transmitting firmware...";
                string macAddresses="";
                string UniMulti = "UNICAST";

                
                for (int x = 0; x <= checkedListBox1.CheckedItems.Count - 1; x++)
                {
                    string macAddress = getMac(checkedListBox1.CheckedItems[x].ToString());
                    if (macAddresses == "")
                    {
                        macAddresses = macAddress;
                    }
                    else
                    {
                        macAddresses += "," + macAddress;
                        UniMulti = "MULTICAST";
                    }

                }

                if (listBox1.Text == "BROADCAST")
                {
                    UniMulti = "BROADCAST";
                }
                string file = openFileDialog1.FileName;
                file = file.Replace("\\", "/");
                string args = "-Djava.library.path=../otap/" + lib + "/ -jar ../otap/otap.jar -send --file " + file + " --mode " + UniMulti + " --mac " + macAddresses + " --pid "+label2.Text;
                executeCommand("java", args);
                toolStripStatusLabel1.Text = "Ready";
            }

        }

        private void button7_Click(object sender, EventArgs e)
        {
            string pID = Microsoft.VisualBasic.Interaction.InputBox("Please enter new pID", "pID", "", 0, 0);
            if (pID.Length == 7)
            {
                label2.Text = pID;
            }
            else
            {
                MessageBox.Show("pID must be exactly 7 characters!");
            }
        }

        private void openFileDialog1_FileOk(object sender, CancelEventArgs e)
        {

        }

        private void scanTime_ValueChanged(object sender, EventArgs e)
        {
            label4.Text = scanTime.Value.ToString();
        }

        private void button8_Click(object sender, EventArgs e)
        {
            List<string> mac_addresses = new List<string>();
            toolStripStatusLabel1.Text = "Getting firmwares list...";
            

            for (int x = 0; x <= checkedListBox1.CheckedItems.Count - 1; x++)
            {
                string macAddress = getMac(checkedListBox1.CheckedItems[x].ToString());
                mac_addresses.Add(macAddress);

            }
            string args = "-Djava.library.path=../otap/" + lib + "/ -jar ../otap/otap.jar -get_boot_list --mode UNICAST --mac "+string.Join(",",mac_addresses);
            executeCommand("java", args);
            processOutputFile(scanType.bootList);
            toolStripStatusLabel1.Text = "Ready";
        }

        private void panel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void button9_Click(object sender, EventArgs e)
        {
            if (listBox1.SelectedIndex > -1)
            {
                string pID = listBox1.Items[listBox1.SelectedIndex].ToString();
                if (pID.Length == 7)
                {
                    toolStripStatusLabel1.Text = "Switching to "+pID;
                    System.IO.StreamWriter tw = new System.IO.StreamWriter("macsToRun.txt", false);

                    for (int x = 0; x <= checkedListBox1.CheckedItems.Count - 1; x++)
                    {
                        string macAddress = getMac(checkedListBox1.CheckedItems[x].ToString());
                        tw.WriteLine(macAddress);

                    }

                    tw.Flush();
                    tw.Close();
                    string args = "-Djava.library.path=../otap/" + lib + "/ -jar ../otap/otap.jar -start_new_program --mode MULTICAST --macs_file macsToRun.txt -pid "+pID;
                    executeCommand("java", args);
                    toolStripStatusLabel1.Text = "Ready";
                }
                else
                {
                    toolStripStatusLabel1.Text = "PID " + pID + " is not valid!";
                }

            }
            else
            {
                toolStripStatusLabel1.Text = "Please select a PID first";
            }
        }

        private void button10_Click(object sender, EventArgs e)
        {
            toolStripStatusLabel1.Text = "Resetting all...";
            string args = "-Djava.library.path=../otap/" + lib + "/ -jar ../otap/otap.jar -reset --mode BROADCAST";
            executeCommand("java", args);
            toolStripStatusLabel1.Text = "Ready";
        }

        //Async socket
        SynchronousSocketListener ssl = new SynchronousSocketListener();
        public List<string> commandsBuffer = new List<string>();
        public List<string> log = new List<string>();

        Thread t;

        private void Form1_Load(object sender, EventArgs e)
        {
            comboBox1.SelectedIndex = 0;
            initThread();

        }

        private void initThread()
        {
            ssl.initPort(3000);
            t = new Thread(() => ssl.StartListening(commandsBuffer)); // Kick off a new thread
            t.Start();
        }


        private void button11_Click(object sender, EventArgs e)
        {
            if (listBox1.SelectedIndex > -1)
            {
                string pID = listBox1.Items[listBox1.SelectedIndex].ToString();
                if (pID.Length == 7)
                {
                    toolStripStatusLabel1.Text = "Deleting program " + pID;
                    System.IO.StreamWriter tw = new System.IO.StreamWriter("macsToRun.txt", false);

                    for (int x = 0; x <= checkedListBox1.CheckedItems.Count - 1; x++)
                    {
                        string macAddress = getMac(checkedListBox1.CheckedItems[x].ToString());
                        tw.WriteLine(macAddress);

                    }

                    tw.Flush();
                    tw.Close();
                    string args = "-Djava.library.path=../otap/" + lib + "/ -jar ../otap/otap.jar -delete_program --mode MULTICAST --macs_file macsToRun.txt -pid " + pID;
                    executeCommand("java", args);
                    toolStripStatusLabel1.Text = "Ready";
                }
                else
                {
                    toolStripStatusLabel1.Text = "PID " + pID + " is not valid!";
                }

            }
            else
            {
                toolStripStatusLabel1.Text = "Please select a PID first";
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            while (commandsBuffer.Count > 0)
            {
                if (commandsBuffer[0] == "/" || commandsBuffer[0] == "|" || commandsBuffer[0] == "\\" ||
                    commandsBuffer[0] == "-")
                {

                    //progressBar1.Maximum = 10;
                    progressBar1.Visible = true;
                    progressBar1.Value++;
                    if (progressBar1.Value >= progressBar1.Maximum)
                    {
                        progressBar1.Value = 0;
                    }
                    
                }
                else
                    if (commandsBuffer[0].Contains("Sending"))
                    {
                        String percentage;
                        percentage = commandsBuffer[0][12].ToString();
                        if (commandsBuffer[0][13] != '%')
                        {
                            percentage += commandsBuffer[0][13].ToString();
                            if (commandsBuffer[0][14] != '%')
                            {
                                percentage += commandsBuffer[0][14].ToString();
                            }
                        }

                        progressBar1.Visible = true;
                        progressBar1.Maximum = 100;
                        progressBar1.Value = Int32.Parse(percentage);
                        if (progressBar1.Value == 100)
                        {
                            progressBar1.Value = 0;
                        }
                    }
                    else
                        if (commandsBuffer[0].Trim()!="")
                {
                    progressBar1.Visible = false;
                    textBox1.Text=textBox1.Text+"\r\n"+commandsBuffer[0];
                    

                }
                textBox1.SelectionStart = textBox1.Text.Length;
                textBox1.ScrollToCaret();

                commandsBuffer.RemoveAt(0);
            }
        }

        private void button12_Click(object sender, EventArgs e)
        {
            processOutputFile(nextScanType, "output.txt");
        }
        

    }
}
 
 public class ProcessOutputHandler
 {
        Process proc;

        public ProcessOutputHandler(Process process)
  {
   proc = process;
   Debug.Assert(proc.StartInfo.RedirectStandardError, "RedirectStandardError must be true to use ProcessOutputHandler.");
   Debug.Assert(proc.StartInfo.RedirectStandardOutput, "RedirectStandardOut must be true to use ProcessOutputHandler.");
  }
  /// <summary>
  /// This method starts reading the standard error stream from Process.
  /// </summary>
  public void ReadStdErr()
  {
   try
   {
       string line;
       //System.IO.StreamWriter tw = new System.IO.StreamWriter("error.txt", false); 
        while ((!proc.HasExited) && ((line = proc.StandardError.ReadLine()) != null))
        {
            UdpSend.Instance.SendMessage("localhost", 3000, "ERROR:"+line);
            //tw.WriteLine(line);
        }
        //tw.Flush();
        //tw.Close();
   }
   catch (InvalidOperationException)
   {
    // The process has exited or StandardError hasn't been redirected.
   }
  }

  
  /// <summary>
  /// This method starts reading the standard output sream from Process.
  /// </summary>
  public void ReadStdOut()
  {
   try
   {
    string line;
    System.IO.StreamWriter tw = new System.IO.StreamWriter("output.txt", false);
    while ((!proc.HasExited) && ((line = proc.StandardOutput.ReadLine()) != null))
    {
        UdpSend.Instance.SendMessage("localhost",3000, line);
        tw.WriteLine(line);
    }
    tw.Flush();
    tw.Close();
   }
   catch (InvalidOperationException)
   {
    // The process has exited or StandardError hasn't been redirected.
   }
  }
 
 
 }
    

