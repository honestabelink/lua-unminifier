using FastColoredTextBoxNS;
using Ionic.Zip;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace luaformatter
{
    public partial class Form1 : Form
    {
        public static ZipFile zip;
        public static Dictionary<string, ZipEntry> entrys = new Dictionary<string, ZipEntry>();
        public static Dictionary<string, string> fentrys = new Dictionary<string, string>();
        public static List<luatextbox> texts = new List<luatextbox>();

        static ZipFile lamezip = new ZipFile();

        public Form1()
        {
            InitializeComponent();

            int height = this.fileToolStripMenuItem.Size.Height;
            int panely = splitContainer1.Location.Y;
            int panelheight = splitContainer1.Panel1.Size.Height;
            int panelwidth = splitContainer1.Panel1.Size.Width;

            customTabControl1.Location = new Point(1 + height, 1 + height);
            customTabControl1.Size = new Size(panelwidth, panelheight);
            customTabControl1.Dock = DockStyle.Fill;

            texts.Add(new luatextbox());
            this.customTabControl1.TabPages[0].Controls.Add(texts[0]);
            texts[0].Dock = DockStyle.Fill;
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog t = new OpenFileDialog();
            t.Multiselect = true;
            var result = t.ShowDialog();

            if (result == System.Windows.Forms.DialogResult.OK)
            {
                foreach (var ctt in t.FileNames)
                {
                    string g = ctt.Split('\\').Last();
                    g = g.Replace(".smod", "");
                    g = g.Replace(".zip", "");


                    if (ctt.Contains(".smod") || ctt.Contains(".zip"))
                    {
                        ZipFile f = new ZipFile(ctt);
                        TreeNodeEx n = new TreeNodeEx();
                        n.Text = g;
                        n.NodeKey = g;

                        treeView1.Nodes.Add(n);

                        foreach (var c in f.Entries)
                        {
                            string[] p = c.FileName.Split('/');
                            List<string> path = p.ToList();

                            for (int i = path.Count - 1; i > 0; i--)
                            {
                                if (path[i] == "" || path[i] == "/" || path[i] == "Thumbs.db")
                                    path.RemoveAt(i);
                            }

                            string key = "";

                            if (!c.IsDirectory)
                            {
                                if (entrys.ContainsKey(path.Last()))
                                {
                                    int index = 1;
                                    while (true)
                                    {
                                        if (path.Last() == "manifest.json")
                                        {
                                            int fff = 0;
                                        }
                                        if (entrys.ContainsKey(path.Last() + index.ToString()))
                                        {
                                            index++;
                                        }
                                        else
                                        {
                                            key = path.Last() + index;
                                            entrys.Add(key, c);
                                            break;
                                        }
                                    }
                                }
                                else
                                    entrys.Add(path.Last(), c);
                            }
                            TreeNodeEx parent = null;
                            for (int i = 0; i < path.Count; i++)
                            {
                                if (i == 0)
                                {
                                    TreeNodeEx ex = gettoplevelkey(path[i]);
                                    if (ex != null)
                                        parent = ex;
                                }
                                else
                                {
                                    TreeNodeEx ex = getKey(parent, path[i]);
                                    if (ex != null)
                                    {
                                        parent = ex;
                                    }
                                }
                            }

                            TreeNodeEx newnode = new TreeNodeEx();
                            newnode.Text = path.Last();
                            newnode.NodeKey = !string.IsNullOrEmpty(key) ? key : path.Last();

                            parent.Nodes.Add(newnode);
                        }
                    }
                    else if (ctt.Contains(".lua"))
                    {
                        string[] path = ctt.Split('\\');
                        string key = "";
                        int index = 1;
                        if (fentrys.ContainsKey(path.Last()))
                        {
                            while (true)
                            {
                                if (fentrys.ContainsKey(path.Last() + index.ToString()))
                                {
                                    index++;
                                }
                                else
                                {
                                    key = path.Last() + index;
                                    break;
                                }
                            }
                        }
                        if (entrys.ContainsKey(path.Last()))
                        {
                            while (true)
                            {
                                if (entrys.ContainsKey(path.Last() + index.ToString()))
                                {
                                    index++;
                                }
                                else
                                {
                                    key = path.Last() + index;
                                    break;
                                }
                            }
                        }
                        fentrys.Add(path.Last(), ctt);

                        TreeNodeEx parent = null;
                        for (int i = 0; i < path.Length; i++)
                        {
                            if (i == 0)
                            {
                                TreeNodeEx ex = gettoplevelkey(path[i]);
                                if (ex != null)
                                    parent = ex;
                            }
                            else
                            {
                                TreeNodeEx ex = getKey(parent, path[i]);
                                if (ex != null)
                                {
                                    parent = ex;
                                }
                            }
                        }

                        TreeNodeEx newnode = new TreeNodeEx();
                        newnode.Text = path.Last();
                        newnode.NodeKey = !string.IsNullOrEmpty(key) ? key : path.Last();

                        if (parent != null)
                            parent.Nodes.Add(newnode);
                        else
                            this.treeView1.Nodes.Add(newnode);
                    }
                }
            }
        }

        TreeNodeEx gettoplevelkey(string text)
        {
            foreach (TreeNodeEx ex in this.treeView1.Nodes)
            {
                if (ex.NodeKey == text)
                    return ex;
            }

            return null;
        }

        int gettoplevelindex(string text)
        {
            return this.treeView1.Nodes.IndexOfKey(text);
        }

        bool has(TreeNodeEx parentnode, string text)
        {
            return parentnode.Nodes.IndexOfKey(text) > -1;
        }

        int getindexof(TreeNodeEx parentnode, string text)
        {
            return parentnode.Nodes.IndexOfKey(text);
        }

        TreeNodeEx getKey(TreeNodeEx parent, string text)
        {
            if (parent == null) return null;
            foreach (TreeNodeEx c in parent.Nodes)
            {
                if (c.NodeKey == text)
                    return c;
            }
            return null;
        }

        private void exportToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void treeView1_NodeMouseDoubleClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            string text = e.Node.Text;
            string key = (e.Node as TreeNodeEx).NodeKey;

            if (fentrys.ContainsKey(key))
            {
                openentry(key, text, new ZipEntry());
                return;
            }

            if (entrys.ContainsKey(key))
            {
                ZipEntry entry = entrys[key];

                openentry(key, text, entry);
            }
        }

        public void openentry(string nodekey, string text, ZipEntry entry)
        {
            if (customTabControl1.TabPages[nodekey] != null)
            {
                customTabControl1.SelectTab(nodekey);
                if (!string.IsNullOrEmpty(comboBox1.Text))
                {
                    int index = customTabControl1.SelectedIndex;
                    texts[index].hightlight = new Regex(comboBox1.Text, RegexOptions.Compiled);
                    texts[index].Invalidate();
                }
                else
                {
                    int index = customTabControl1.SelectedIndex;
                    texts[index].hightlight = null;
                    texts[index].Invalidate();
                }
                return;
            }

            if (customTabControl1.TabCount == 1 && (nodekey.Contains(".lua") || nodekey.Contains(".json")))
            {
                if (customTabControl1.TabPages[0].Text == "Untitled")
                {
                    texts.RemoveAt(0);
                    customTabControl1.TabPages.RemoveAt(0);
                }
            }

            if (entry.FileName == null)
            {
                if (fentrys.ContainsKey(nodekey))
                {
                    string[] flines = File.ReadAllLines(fentrys[nodekey]);
                    List<string> lines = luaparser.formatfile(flines, ref luaparser.defs);

                    customTabControl1.TabPages.Add(nodekey, text);

                    luatextbox t = new luatextbox();
                    texts.Add(t);
                    customTabControl1.TabPages[nodekey].Controls.Add(t);
                    t.Dock = DockStyle.Fill;

                    for (int i = 0; i < lines.Count; i++)
                        lines[i] += '\n';



                    t.Text = String.Join("", lines);

                    int sinde = customTabControl1.SelectedIndex;

                    customTabControl1.SelectTab(nodekey);

                    if (sinde == customTabControl1.SelectedIndex)
                    {
                        customTabControl1_SelectedIndexChanged(this, new EventArgs());
                    }
                }
                return;
            }

            if (entry.FileName.Contains(".lua"))
            {
                using (MemoryStream m = new MemoryStream())
                {
                    entry.Extract(m);

                    m.Position = 0;
                    List<string> rows = new List<string>();
                    using (var reader = new StreamReader(m, Encoding.ASCII))
                    {
                        string line;
                        while ((line = reader.ReadLine()) != null)
                        {
                            rows.Add(line);
                        }
                    }

                    customTabControl1.TabPages.Add(nodekey, text);

                    luatextbox t = new luatextbox();
                    texts.Add(t);
                    customTabControl1.TabPages[nodekey].Controls.Add(t);
                    t.Dock = DockStyle.Fill;

                    List<string> lines = luaparser.formatfile(rows.ToArray(), ref luaparser.defs);
                    for (int i = 0; i < lines.Count; i++)
                        lines[i] += '\n';


                    t.Text = String.Join("", lines);

                    int sinde = customTabControl1.SelectedIndex;

                    customTabControl1.SelectTab(nodekey);

                    if (sinde == customTabControl1.SelectedIndex)
                    {
                        customTabControl1_SelectedIndexChanged(this, new EventArgs());
                    }

                }
            }
            else if (entry.FileName.Contains(".json"))
            {
                using (MemoryStream m = new MemoryStream())
                {
                    entry.Extract(m);

                    m.Position = 0;
                    List<string> rows = new List<string>();
                    using (var reader = new StreamReader(m, Encoding.ASCII))
                    {
                        string line;
                        while ((line = reader.ReadLine()) != null)
                        {
                            rows.Add(line + '\n');
                        }
                    }

                    customTabControl1.TabPages.Add(nodekey, text);

                    luatextbox t = new luatextbox();
                    texts.Add(t);
                    customTabControl1.TabPages[nodekey].Controls.Add(t);
                    t.Dock = DockStyle.Fill;

                    t.Text = String.Join("", rows);
                    int sinde = customTabControl1.SelectedIndex;

                    customTabControl1.SelectTab(nodekey);

                    if (sinde == customTabControl1.SelectedIndex)
                    {
                        customTabControl1_SelectedIndexChanged(this, new EventArgs());
                    }
                }
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            comboBox1.Items.Add(comboBox1.Text);

            if (checkBox1.Checked)
            {
                Regex r = new Regex(comboBox1.Text, RegexOptions.Compiled);

                List<KeyValuePair<string, ZipEntry>> matchentries = new List<KeyValuePair<string, ZipEntry>>();
                List<string> mat = new List<string>();
                List<int> lineindex = new List<int>();

                int tindex = customTabControl1.SelectedIndex;

                if (tindex > -1)
                {
                    if (string.IsNullOrEmpty(comboBox1.Text))
                    {
                        texts[tindex].hightlight = null;
                        texts[tindex].Invalidate();
                    }
                    else
                    {
                        texts[tindex].hightlight = new Regex(comboBox1.Text, RegexOptions.Compiled);
                        texts[tindex].Invalidate();
                    }
                }

                foreach (var c in entrys)
                {
                    if (c.Value.FileName.Contains(".lua"))
                    {
                        using (MemoryStream m = new MemoryStream())
                        {
                            c.Value.Extract(m);

                            m.Position = 0;
                            List<string> rows = new List<string>();
                            using (var reader = new StreamReader(m, Encoding.ASCII))
                            {
                                string line;
                                while ((line = reader.ReadLine()) != null)
                                {
                                    rows.Add(line);
                                }
                            }

                            List<string> lines = luaparser.formatfile(rows.ToArray(), ref luaparser.defs);

                            for (int i = 0; i < lines.Count; i++)
                            {
                                if (r.Match(lines[i]).Success)
                                {
                                    matchentries.Add(c);
                                    mat.Add(lines[i].Trim());
                                    lineindex.Add(i + 1);
                                }
                            }
                        }
                    }
                    else if (c.Value.FileName.Contains(".json"))
                    {
                        using (MemoryStream m = new MemoryStream())
                        {
                            c.Value.Extract(m);

                            m.Position = 0;
                            List<string> rows = new List<string>();

                            using (var reader = new StreamReader(m, Encoding.ASCII))
                            {
                                string line;
                                int index = 0;
                                while ((line = reader.ReadLine()) != null)
                                {
                                    if (r.Match(line).Success)
                                    {
                                        matchentries.Add(c);
                                        mat.Add(line.Trim());
                                        lineindex.Add(index + 1);
                                    }
                                    index++;
                                }
                            }
                        }
                    }
                }

                matchingwindow mm = new matchingwindow(this, comboBox1.Text, matchentries, mat, lineindex);
                mm.Show();

            }
            else
            {
                int index = customTabControl1.SelectedIndex;

                if (index > -1)
                {
                    if (string.IsNullOrEmpty(comboBox1.Text))
                    {
                        texts[index].hightlight = null;
                        texts[index].Invalidate();
                    }
                    else
                    {
                        texts[index].hightlight = new Regex(comboBox1.Text, RegexOptions.Compiled);
                        texts[index].Invalidate();
                    }
                }
            }
        }

        private void customTabControl1_TabClosing(object sender, TabControlCancelEventArgs e)
        {
            if (customTabControl1.TabCount == 1)
            {
                e.Cancel = true;
                return;
            }
            texts.RemoveAt(e.TabPageIndex);
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
        }

        private void customTabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {
            int index = customTabControl1.SelectedIndex;

            if (index == -1) return;
            if (string.IsNullOrEmpty(comboBox1.Text))
            {

                texts[index].hightlight = null;
                texts[index].Invalidate();
            }
            else
            {
                texts[index].hightlight = new Regex(comboBox1.Text, RegexOptions.Compiled);
                texts[index].Invalidate();
            }
        }

        public void scrollto(int line)
        {
            int index = customTabControl1.SelectedIndex;

            if (!texts[index].VerticalScroll.Visible)
                return;

            float perline = (int)texts[index].VerticalScroll.Maximum / (int)texts[index].LinesCount;

            float per = line * perline;

            texts[index].VerticalScroll.Value = (int)per - (int)perline;
        }

        private void textBox1_KeyPress(object sender, KeyPressEventArgs e)
        {
        }

        private void Form1_DragEnter(object sender, DragEventArgs e)
        {
            e.Effect = DragDropEffects.All;
        }

        private void Form1_DragDrop(object sender, DragEventArgs e)
        {
            string[] fl = (string[])e.Data.GetData(DataFormats.FileDrop, false);
            foreach (var pathe in fl)
            {
                string g = pathe.Split('\\').Last();
                g = g.Replace(".smod", "");
                g = g.Replace(".zip", "");


                if (pathe.Contains(".smod") || pathe.Contains(".zip"))
                {
                    TreeNodeEx n = new TreeNodeEx();
                    n.Text = g;
                    n.NodeKey = g;

                    treeView1.Nodes.Add(n);
                    ZipFile f = new ZipFile(pathe);

                    foreach (var c in f.Entries)
                    {
                        string[] p = c.FileName.Split('/');
                        List<string> path = p.ToList();

                        for (int i = path.Count - 1; i > 0; i--)
                        {
                            if (path[i] == "" || path[i] == "/" || path[i] == "Thumbs.db")
                                path.RemoveAt(i);
                        }

                        string key = "";

                        if (!c.IsDirectory)
                        {
                            if (entrys.ContainsKey(path.Last()))
                            {
                                int index = 1;
                                while (true)
                                {
                                    if (entrys.ContainsKey(path.Last() + index.ToString()))
                                    {
                                        index++;
                                    }
                                    else
                                    {
                                        key = path.Last() + index;
                                        break;
                                    }
                                }
                            }
                            else
                                entrys.Add(path.Last(), c);
                        }
                        TreeNodeEx parent = null;
                        for (int i = 0; i < path.Count; i++)
                        {
                            if (i == 0)
                            {
                                TreeNodeEx ex = gettoplevelkey(path[i]);
                                if (ex != null)
                                    parent = ex;
                            }
                            else
                            {
                                TreeNodeEx ex = getKey(parent, path[i]);
                                if (ex != null)
                                {
                                    parent = ex;
                                }
                            }
                        }

                        TreeNodeEx newnode = new TreeNodeEx();
                        newnode.Text = path.Last();
                        newnode.NodeKey = !string.IsNullOrEmpty(key) ? key : path.Last();

                        parent.Nodes.Add(newnode);
                    }
                }
                else if (pathe.Contains(".lua"))
                {
                    string[] path = pathe.Split('\\');
                    string key = "";
                    int index = 1;
                    if (fentrys.ContainsKey(path.Last()))
                    {
                        while (true)
                        {
                            if (fentrys.ContainsKey(path.Last() + index.ToString()))
                            {
                                index++;
                            }
                            else
                            {
                                key = path.Last() + index;
                                break;
                            }
                        }
                    }
                    if (entrys.ContainsKey(path.Last()))
                    {
                        while (true)
                        {
                            if (entrys.ContainsKey(path.Last() + index.ToString()))
                            {
                                index++;
                            }
                            else
                            {
                                key = path.Last() + index;
                                break;
                            }
                        }
                    }
                    fentrys.Add(path.Last(), pathe);

                    TreeNodeEx parent = null;
                    for (int i = 0; i < path.Length; i++)
                    {
                        if (i == 0)
                        {
                            TreeNodeEx ex = gettoplevelkey(path[i]);
                            if (ex != null)
                                parent = ex;
                        }
                        else
                        {
                            TreeNodeEx ex = getKey(parent, path[i]);
                            if (ex != null)
                            {
                                parent = ex;
                            }
                        }
                    }

                    TreeNodeEx newnode = new TreeNodeEx();
                    newnode.Text = path.Last();
                    newnode.NodeKey = !string.IsNullOrEmpty(key) ? key : path.Last();

                    if (parent != null)
                        parent.Nodes.Add(newnode);
                    else
                        this.treeView1.Nodes.Add(newnode);
                }
                else if (Directory.Exists(pathe))
                {
                    ListDirectory(this.treeView1, pathe);
                }
            }
        }

        private void ListDirectory(TreeView treeView, string path)
        {
            var rootDirectoryInfo = new DirectoryInfo(path);
            treeView.Nodes.Add(CreateDirectoryNode(rootDirectoryInfo));
        }


        private static TreeNode CreateDirectoryNode(DirectoryInfo directoryInfo)
        {
            var directoryNode = new TreeNodeEx();
            directoryNode.Text = directoryInfo.Name;
            directoryNode.NodeKey = "dafdsfsueru34324234890f";
            foreach (var directory in directoryInfo.GetDirectories())
                directoryNode.Nodes.Add(CreateDirectoryNode(directory));
            foreach (var file in directoryInfo.GetFiles())
            {
                string name = file.Name;
                string key = file.Name;

                int index = 1;
                if (fentrys.ContainsKey(file.Name))
                {
                    while (true)
                    {
                        if (fentrys.ContainsKey(file.Name + index.ToString()))
                        {
                            index++;
                        }
                        else
                        {
                            key = file.Name + index;
                            break;
                        }
                    }
                }
                if (entrys.ContainsKey(file.Name))
                {
                    while (true)
                    {
                        if (entrys.ContainsKey(file.Name + index.ToString()))
                        {
                            index++;
                        }
                        else
                        {
                            key = file.Name + index;
                            break;
                        }
                    }
                }
                fentrys.Add(key, file.FullName);

                TreeNodeEx n = new TreeNodeEx();
                n.NodeKey = key;
                n.Text = name;

                directoryNode.Nodes.Add(n);
            }
            return directoryNode;
        }

        private void comboBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                button1.Focus();
                button1.PerformClick();
            }
        }
    }
}
