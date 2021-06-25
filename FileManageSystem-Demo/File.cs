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
    public partial class File : Form
    {
        public File(string FileName)
        {
            InitializeComponent();
            label2.Text = FileName;
        }
        bool flag;
        private void Button1_Click(object sender, EventArgs e)
        {
            Close();
            flag = false;
        }

        private void Button2_Click(object sender, EventArgs e)
        {
            DialogResult dr = MessageBox.Show("是否取消保存", "对话框标题", MessageBoxButtons.OKCancel, MessageBoxIcon.Question);
            if (dr == DialogResult.OK)
            {
                Close();
                flag = true;
            }


        }
        public string getData()
        {
            string data = InputData.Text;
            return data;
        }
        public bool IsCancel()
        {
            return flag;
        }
        public void setFile(string str)
        {

            InputData.Text = str;
        }
    }
}
