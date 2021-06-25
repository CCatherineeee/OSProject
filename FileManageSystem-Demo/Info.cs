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
    public partial class Info : Form
    {
        public Info()
        {
            InitializeComponent();
        }
        public void setInfo(int type,string _name,string _size,string _path)
        {
            if (type == 1)
                label6.Text = "文件夹";
            else
                label6.Text = "文件";
            name.Text = _name;
            label3.Text = _size + "B";
            textBox2.Text = _path;
        }
    }
}
