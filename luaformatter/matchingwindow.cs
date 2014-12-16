using Ionic.Zip;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace luaformatter
{
    public partial class matchingwindow : Form
    {
        List<KeyValuePair<string, ZipEntry>> zipentrys;
        List<string> text;
        List<int> lineindex;

        Form1 parent;

        public matchingwindow(Form1 parent, string title, List<KeyValuePair<string, ZipEntry>> zips, List<string> tex, List<int> lineindex)
        {
            this.parent = parent;
            InitializeComponent();

            this.Text = "Search - " + title;

            this.lineindex = lineindex;

            this.zipentrys = zips;
            this.text = tex;


            for (int i = 0; i < zips.Count; i++)
            {
                listBox1.Items.Add(zips[i].Value.FileName);
                listBox2.Items.Add(text[i]);
                listBox3.Items.Add(lineindex[i]);
            }
        }

        private void listBox1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            int index = listBox1.SelectedIndex;

            parent.openentry(zipentrys[index].Key, zipentrys[index].Value.FileName.Split('/').Last(), zipentrys[index].Value);
            parent.scrollto(lineindex[index]);
            parent.Focus();
            parent.scrollto(lineindex[index]);
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            listBox2.TopIndex = listBox1.TopIndex;
            listBox3.TopIndex = listBox1.TopIndex;
            listBox2.SelectedIndex = listBox1.SelectedIndex;
            listBox3.SelectedIndex = listBox1.SelectedIndex;
        }

        private void listBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            listBox1.TopIndex = listBox2.TopIndex;
            listBox3.TopIndex = listBox2.TopIndex;
            listBox1.SelectedIndex = listBox2.SelectedIndex;
            listBox3.SelectedIndex = listBox2.SelectedIndex;
        }

        private void listBox3_SelectedIndexChanged(object sender, EventArgs e)
        {
            listBox1.TopIndex = listBox3.TopIndex;
            listBox2.TopIndex = listBox3.TopIndex;
            listBox1.SelectedIndex = listBox3.SelectedIndex;
            listBox2.SelectedIndex = listBox3.SelectedIndex;
        }

        private void listbox1_Scrolled(object sender, ScrollEventArgs e)
        {
            listBox2.TopIndex = listBox1.TopIndex;
            listBox3.TopIndex = listBox1.TopIndex;
        }

        private void listBox2_Scrolled(object sender, ScrollEventArgs e)
        {
            listBox1.TopIndex = listBox2.TopIndex;
            listBox3.TopIndex = listBox2.TopIndex;
        }

        private void listBox3_Scrolled(object sender, ScrollEventArgs e)
        {
            listBox1.TopIndex = listBox3.TopIndex;
            listBox2.TopIndex = listBox3.TopIndex;
        }


        private void listBox2_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            int index = listBox1.SelectedIndex;

            parent.openentry(zipentrys[index].Key, zipentrys[index].Value.FileName.Split('/').Last(), zipentrys[index].Value);
            parent.scrollto(lineindex[index]);
            parent.Focus();
            parent.scrollto(lineindex[index]);
        }

        private void listBox3_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            int index = listBox1.SelectedIndex;

            parent.openentry(zipentrys[index].Key, zipentrys[index].Value.FileName.Split('/').Last(), zipentrys[index].Value);
            parent.scrollto(lineindex[index]);
            parent.Focus();
            parent.scrollto(lineindex[index]);
        }
    }
}
