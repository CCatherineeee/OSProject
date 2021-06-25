using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace FileManageSystem_Demo
{
    public partial class Form1 : Form
    {
        ImageList imagelist1;
        [Serializable]
        public class FCB
        {
            public string filename;
            public string filepath;
            public int filesize;   //文件大小
            public List<PhysBlock> fileblock;  //对应的物理块集合
            public FCB()
            {
                fileblock =new List<PhysBlock>();
                filename = "";
                filepath = "";
                filesize = 0;
            }
           
        }

        //物理块
        [Serializable]
        public class PhysBlock
        {
            public int blocknum;   //块号

            public List<int> data;    //块存储的数据
            public PhysBlock()
            {
                data = new List<int>();
            }

        }
        [Serializable]
        public class Memory
        {
            public PhysBlock[] PhysBlockList;
            public bool[] BitMap = new bool[1000];
            public int blocksize;  //块大小 
            public CatalogNode root;
            public CatalogNode curCataNode;
            public Memory()
            {
                blocksize = 512;
                PhysBlockList = new PhysBlock[1000];
                for (int i = 0; i < 1000; i++)
                {
                    PhysBlockList[i] = new PhysBlock();
                    PhysBlockList[i].blocknum = i;
                    BitMap[i] = false;
                }
                root = new CatalogNode();
                curCataNode = new CatalogNode();
                curCataNode = root;
            }
            public List<int> FindBlock(int n)
            {
                List<int> list = new List<int>();
                int cnt = 0;
                for(int i=0;i<PhysBlockList.Length;i++)
                {

                    if(!BitMap[i])
                    {
                        cnt++;
                        list.Add(i);
                    }
                    if (cnt == n)
                        break;
                }
                if (cnt != n)
                    return null;
                else
                    return list;
            }
            public void FileBlockClear(CatalogNode OpNode)
            {
                for (int i = 0; i < OpNode._file._FCB.fileblock.Count; i++)
                {
                    BitMap[OpNode._file._FCB.fileblock[i].blocknum] = false;
                    PhysBlockList[OpNode._file._FCB.fileblock[i].blocknum].data.Clear();
                    
                }
                OpNode._file._FCB.fileblock.Clear();

            }

        }

        //目录
        [Serializable]
        public class CatalogNode
        {
            public List<CatalogNode> chilenode;    //存储子节点
            public string name;
            public CatalogNode fathernode; //存储父节点
            public string catapath;
            public int size;
            public int nodetype;   //区分节点类型 1文件夹 0文件
            public file _file; //指明节点对应的文件
            public CatalogNode()
            {
                catapath = "root\\";
                nodetype = 1;
                name = "root";
                chilenode = new List<CatalogNode>();
                size = 0;
                //fathernode = new CatalogNode();
                _file = new file();

            }
            public CatalogNode(string _name,CatalogNode FNode, int _type,string _path)
            {
                name = _name;
                fathernode = FNode;
                nodetype = _type;
                catapath = _path;
                size = 0;
                chilenode = new List<CatalogNode>();
                _file = new file();
            }
            

        }
        [Serializable]
        public class file
        {
            public FCB _FCB;
            public file()
            {
                _FCB = new FCB();
            }

        }

        Memory MainMemo;
        public Form1()
        {
            InitializeComponent();
            //MainMemo.curCataNode = MainMemo.root;
            imagelist1 = new ImageList();
            imagelist1.Images.Add(Image.FromFile(@"file.jpg"));
            imagelist1.Images.Add(Image.FromFile(@"folder.jpg"));

            if (System.IO.File.Exists("./MainMemo.dat"))
            {
                FileStream stream1 = new FileStream("./MainMemo.dat", FileMode.Open);
               
                BinaryFormatter bFormat = new BinaryFormatter();
                MainMemo = (Memory)bFormat.Deserialize(stream1);

                stream1.Close();

            }
            else
            {
                MainMemo = new Memory();
            }
            UpdateListView();
        }
                    
        private void 新建文件夹ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            newForm form = new newForm(1);
            string sgName;
            CatalogNode OpNode;

            OpNode = MainMemo.curCataNode;
         
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
                    string[] s = curNode.chilenode[i].name.Split('.')[0].Split('(');

                    if (s[0] == sgName)
                        cnt++;
                }
            }           

            string suffix;
            if (cnt == 0)
                suffix = "";
            else
                suffix = "("+(cnt+1).ToString()+")";

            sgName += suffix;
            return sgName;
        }

        private void 新建文件ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            newForm form = new newForm(0);
            string sgName;
            CatalogNode OpNode;

            OpNode = MainMemo.curCataNode;
            form.setInputName("新建文件");
            form.ShowDialog();
            if (form.IsCancel())
                return;
            string _name = form.getInputName();
            sgName = SuggestName(OpNode, 0, _name);
            CatalogNode newNode = new CatalogNode(sgName, OpNode, 0, OpNode.catapath);//文件的路径
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

            OpNode = MainMemo.curCataNode;
            CatalogNode ctlogNode = OpNode.chilenode.Find(s => s.name == _name);
            OpNode.chilenode.RemoveAll(s => (s.name == _name));


            UpdateListView();
        }
        public void UpdateListView()
        {
            //遍历目录文件中的项对ListView进行更新
            textBox1.Text = MainMemo.curCataNode.catapath;
            listView1.Items.Clear();
            if (MainMemo.curCataNode.chilenode.Count != 0)
            {
                for (int i = 0; i < MainMemo.curCataNode.chilenode.Count; i ++)
                {
                    ListViewItem node = new ListViewItem();
                    if (MainMemo.curCataNode.chilenode[i].nodetype == 0)
                    {
                        node.ImageIndex = 0;
                        node.Text = MainMemo.curCataNode.chilenode[i].name;

                        //listView1.Items.Add(node);
                    }
                    else
                    {
                        node.ImageIndex = 1;
                        //listView1.Items.Add(node);
                        node.Text = MainMemo.curCataNode.chilenode[i].name;


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
            OpNode = MainMemo.curCataNode;
            CatalogNode ctlogNode = OpNode.chilenode.Find(s => s.name == _name);
            newForm form = new newForm(3);
            form.setInputName(ctlogNode.name);
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

            OpNode = MainMemo.curCataNode;

            CatalogNode ctlogNode = OpNode.chilenode.Find(s => s.name == _name);
            if (ctlogNode.nodetype == 1)
            {
                MainMemo.curCataNode = ctlogNode;
                UpdateListView();
            }
            else
            {
                string filedata = "";
                for(int i=0;i< ctlogNode._file._FCB.fileblock.Count;i++)
                {
                    for(int j=0;j< ctlogNode._file._FCB.fileblock[i].data.Count;j++)
                    {
                        string alpha = ((char)ctlogNode._file._FCB.fileblock[i].data[j]).ToString();
                        // return (strCharacter);
                        filedata += alpha;
                    }
                    
                }

                File form = new File(_name);
                form.setFile(filedata);

                form.ShowDialog();
                
                if (form.IsCancel())
                    return;


                MainMemo.FileBlockClear(ctlogNode);

                string data = form.getData();

                List<int> asciiList = StringToAscii(data);
                int _size = asciiList.Count();
                ctlogNode.size = _size;

                int tblock = _size / MainMemo.blocksize + 1;
                if (tblock == 0)
                    return;

                List<int> list;
                //MainMemo.FileBlockClear(ctlogNode);
                list =MainMemo.FindBlock(tblock);

                if(list==null)
                {
                    MessageBox.Show("没有足够空间保存文件，保存失败！", "提示信息",MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }


                int _s = 0;
                int _e;
                if (tblock > 1)
                    _e = MainMemo.blocksize;
                else
                    _e = _size;

                // _e = 0;
                for (int i=0;i<list.Count;i++)
                {
                    for(int j=_s;j<_e;j++)
                    {
                        MainMemo.PhysBlockList[list[i]].data.Add(asciiList[j]);
                        MainMemo.BitMap[list[i]] = true;
                    }
                     _e += MainMemo.blocksize;
                    if (_e > _size)
                        _e = _size;
                    ctlogNode._file._FCB.fileblock.Add(MainMemo.PhysBlockList[list[i]]);
                }

                /* string Binary = StringToBinary(data);
                 int _s = 0;
                 int _e;
                 if (tblock > 1)
                     _e = MainMemo.blocksize;
                 else
                     _e = _size;
                 for (int i=0;i<list.Count;i++)
                 {
                     MainMemo.PhysBlockList[list[i]].data = Binary.Substring(_s, _e);
                     if (_size - _e > 0)
                     {
                         _s = _e;
                         if (i + 1 == list.Count)
                             _e = _size;
                         else
                             _e += MainMemo.blocksize;
                     }
                     MainMemo.BitMap[list[i]] = true;
                     OpNode._file._FCB.fileblock.Add(MainMemo.PhysBlockList[list[i]]);
                 }
                 */
            }
                

            

        }
        private List<int> StringToAscii(string str)
        {
            List<int> list = new List<int>();
            for(int i=0;i<str.Length;i++)
            {
                list.Add((int)str[i]);
                //label2.Text = "1";
            }
            return list;
        }

        private void Button1_Click(object sender, EventArgs e)
        {
            if (MainMemo.curCataNode.name == "root")
                return;
            MainMemo.curCataNode = MainMemo.curCataNode.fathernode;
            UpdateListView();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            //调用序列化函数
            FileStream stream1 = new FileStream("./MainMemo.dat", FileMode.Create);
            BinaryFormatter bFormat = new BinaryFormatter();
            bFormat.Serialize(stream1, MainMemo);

            stream1.Close();
        }

        private void Button2_Click(object sender, EventArgs e)
        {
            DialogResult dr = MessageBox.Show("是否格式化", "格式化", MessageBoxButtons.OKCancel, MessageBoxIcon.Question);
            if (dr == DialogResult.OK)
            {
                MainMemo = new Memory();
                UpdateListView();
            }

        }

        private void 属性ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            listView1.MultiSelect = false;
            if (listView1.SelectedItems.Count == 0)
            {
                MessageBox.Show("没有任何选中！", "提示信息", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            String _name = listView1.SelectedItems[0].Text; //获取选中文件名
            CatalogNode OpNode;
            OpNode = MainMemo.curCataNode;
            CatalogNode ctlogNode = OpNode.chilenode.Find(s => s.name == _name);

            Info form = new Info();
            form.setInfo(ctlogNode.nodetype,ctlogNode.name,ctlogNode.size.ToString(), ctlogNode.catapath);
            form.ShowDialog();
        }

        private void Button3_Click(object sender, EventArgs e)
        {
            help form = new help();
            form.ShowDialog();
        }
    }

    

}
