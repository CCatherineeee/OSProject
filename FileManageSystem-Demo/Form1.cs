using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FileManageSystem_Demo
{
    public partial class Form1 : Form
    {
        ImageList imagelist1;
        public class FCB
        {
            public string filename;
            public string filepath;
            public int filesize;   //文件大小
            public List<PhysBlock> fileblock;  //对应的物理块集合
        }

        //物理块

        public class PhysBlock
        {
            public int blocknum;   //块号
            public int blocksize;  //快大小 
            public string data;    //块存储的数据
        }
        //目录

        public class Catalog
        {
            public List<CatalogNode> catalog;
        }

        public class CatalogNode
        {
            public List<CatalogNode> chilenode;    //存储子节点
            public string name;
            public CatalogNode fathernode; //存储父节点
            public string catapath;
            public int nodetype;   //区分节点类型 1文件夹 0文件
            public file _file; //指明节点对应的文件
            public CatalogNode()
            {
                catapath = "root\\";
                nodetype = 1;
                //_file = new file();
                //fathernode = new CatalogNode();
                name = "root";
                chilenode = new List<CatalogNode>();

            }
            public CatalogNode(string _name,CatalogNode FNode, int _type,string _path)
            {
                name = _name;
                fathernode = FNode;
                nodetype = _type;
                catapath = _path;
                chilenode = new List<CatalogNode>();
            }
            

        }

        public class file
        {
            public FCB _FCB;
        }
        CatalogNode root = new CatalogNode();
        CatalogNode curCataNode = new CatalogNode();

        public Form1()
        {
            InitializeComponent();
            curCataNode = root;
            imagelist1 = new ImageList();
            imagelist1.Images.Add(Image.FromFile(@"file.jpg"));
            imagelist1.Images.Add(Image.FromFile(@"folder.jpg"));
        }

        private void 新建文件夹ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            newForm form = new newForm(1);
            string sgName;
            CatalogNode OpNode;

            OpNode = curCataNode;
         
            form.setInputName("新建文件夹");
            form.ShowDialog();

            if (form.IsCancel())
                return;
            string _name = form.getInputName();
            sgName = SuggestName(OpNode, 1, _name);
            CatalogNode newNode = new CatalogNode(sgName, OpNode, 1, OpNode.catapath + sgName + "\\");
            OpNode.chilenode.Add(newNode);
            ListViewItem folder = new ListViewItem(sgName, 1);
            listView1.Items.Add(folder);
        }

        public string SuggestName(CatalogNode curNode,int type,string sgName)
        {
            //string sgName;
            int cnt = 0;
            int sufflen = sgName.Length;
            
            for (int i = 0; i < curNode.chilenode.Count(); i++)
            {
                if (curNode.chilenode[i].nodetype == type)
                {
                    if (curNode.chilenode[i].name.Substring(0,sufflen) == sgName)
                        cnt++;
                }
            }           

            string suffix;
            if (cnt == 0)
                suffix = "";
            else
                suffix = "("+(cnt).ToString()+")";

            sgName += suffix;
            return sgName;
        }

        private void 新建文件ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            newForm form = new newForm(0);
            string sgName;
            CatalogNode OpNode;

            OpNode = curCataNode;
            form.setInputName("新建文件");
            form.ShowDialog();
            if (form.IsCancel())
                return;
            string _name = form.getInputName();
            sgName = SuggestName(OpNode, 0, _name);
            CatalogNode newNode = new CatalogNode(sgName, OpNode, 0, OpNode.catapath);//文件的路径应该怎么弄？
            OpNode.chilenode.Add(newNode);
            ListViewItem folder = new ListViewItem(sgName, 0);
            listView1.Items.Add(folder);
        }

        private void 删除ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            listView1.MultiSelect = false;
            if (listView1.SelectedItems.Count == 0)
            {
                MessageBox.Show("没有任何选中！", "提示信息", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            String _name = listView1.SelectedItems[0].Text; //获取选中文件名
            CatalogNode OpNode;

            OpNode = curCataNode;

            OpNode.chilenode.RemoveAll(s => (s.name == _name));
            UpdateListView();
        }
        public void UpdateListView()
        {
            //遍历目录文件中的项对ListView进行更新
            _path_.Text = curCataNode.catapath;
            listView1.Items.Clear();
            if (curCataNode.chilenode.Count != 0)
            {
                for (int i = 0; i < curCataNode.chilenode.Count; i ++)
                {
                    ListViewItem node = new ListViewItem();
                    if (curCataNode.chilenode[i].nodetype == 0)
                    {
                        node.ImageIndex = 0;
                        node.Text = curCataNode.chilenode[i].name;

                        //listView1.Items.Add(node);
                    }
                    else
                    {
                        node.ImageIndex = 1;
                        //listView1.Items.Add(node);
                        node.Text = curCataNode.chilenode[i].name;


                    }
                    listView1.Items.Add(node);
                }
            }
        }

        private void 重命名ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            listView1.MultiSelect = false;
            if (listView1.SelectedItems.Count == 0)
            {
                MessageBox.Show("没有任何选中！", "提示信息", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            String _name = listView1.SelectedItems[0].Text; //获取选中文件名
            CatalogNode OpNode;
            OpNode = curCataNode;
            CatalogNode ctlogNode = OpNode.chilenode.Find(s => s.name == _name);
            newForm form = new newForm(3);
            string sgName;
            //form.setInputName("新建文件");
            form.ShowDialog();
            if (form.IsCancel())
                return;
            sgName = form.getInputName();
            sgName = SuggestName(OpNode, ctlogNode.nodetype, sgName);
            listView1.SelectedItems[0].Text = sgName;

            ctlogNode.name = sgName;

            UpdateListView();
        }

        private void ListView1_DoubleClick(object sender, EventArgs e)
        {
            listView1.MultiSelect = false;
            if (listView1.SelectedItems.Count == 0)
                return;
            String _name = listView1.SelectedItems[0].Text; //获取选中文件名
            CatalogNode OpNode;

            OpNode = curCataNode;

            CatalogNode ctlogNode = OpNode.chilenode.Find(s => s.name == _name);
            if (ctlogNode.nodetype == 1)
            {
                curCataNode = ctlogNode;
                UpdateListView();
            }
            else
            {
                File form = new File(_name);
                form.ShowDialog();
            }

        }
    }

    

}
