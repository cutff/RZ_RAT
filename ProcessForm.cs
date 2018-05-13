using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Windows.Forms;
using helper;

namespace RZ_RAT
{
    public partial class ProcessForm : Form
    {
        public List<ProcessManger.process> listprocess = null;
        int index_;
        public ProcessForm(int Index)
        {
            index_ = Index;
            InitializeComponent();
        }
        public void addprocesstolist()
        {
            listView1.Items.Clear();
            int num = 0;
            foreach (ProcessManger.process process in listprocess)
            {
                try
                {
                    string processname = process.name;
                    string processpid = process.pid;

                    ListViewItem lvi = new ListViewItem(num.ToString());
                    lvi.SubItems.Add(processname);
                    lvi.SubItems.Add(processpid);
                    this.Invoke((MethodInvoker)delegate {
                        listView1.Items.Add(lvi);
                    });
                    num++;
                }
                catch { }
            }
        }
        private void ProcessForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            Form1.ServerObject.ClientsObjs[index_].PF = null;
        }
    }
}
