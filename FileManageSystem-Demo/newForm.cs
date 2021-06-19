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
    public partial class newForm : Form
    {
        public newForm(int type)
        {

            InitializeComponent();
            if (type == 0)
                this.Text = "创建新文件";
            else if (type == 1)
                this.Text = "创建新文件夹";
            else
                this.Text = "更改名字";
        }
        bool flag;
        private void Button1_Click(object sender, EventArgs e)
        {

            if (getInputName() == "")
            {
                MessageBox.Show("文件夹名不能为空！", "提示信息", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            flag = false;
            Close();
        }

        private void Button2_Click(object sender, EventArgs e)
        {
            Close();
            flag = true;
        }
        public string getInputName()
        {
            return InputName.Text; 
        }
        public void setInputName(string name)
        {
            InputName.Text = name;
        }
        public bool IsCancel()
        {
            return flag;
        }
    }
}
