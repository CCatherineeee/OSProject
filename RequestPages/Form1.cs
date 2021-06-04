using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;


namespace RequestPages
{
    public partial class Form1 : Form
    {
        //指令编号数组
        Label[] InsNumArr = new Label[4];
        //指令页号数组
        Label[] InsPageNumArr = new Label[4];
        //缺页数组
        Label[] MissPageArr = new Label[4];
        Label[] InsExetimesCntArr = new Label[4];
        public Form1()
        {
            InitializeComponent();

            InsNumArr[0] = InsNum1;
            InsNumArr[1] = InsNum2;
            InsNumArr[2] = InsNum3;
            InsNumArr[3] = InsNum4;

            InsPageNumArr[0] = InsPage1;
            InsPageNumArr[1] = InsPage2;
            InsPageNumArr[2] = InsPage3;
            InsPageNumArr[3] = InsPage4;


            MissPageArr[0] = MissPage1;
            MissPageArr[1] = MissPage2;
            MissPageArr[2] = MissPage3;
            MissPageArr[3] = MissPage4;

            InsExetimesCntArr[0] = InsExetimesCnt1;
            InsExetimesCntArr[1] = InsExetimesCnt2;
            InsExetimesCntArr[2] = InsExetimesCnt3;
            InsExetimesCntArr[3] = InsExetimesCnt4;

            for (int i = 0; i < 4; i++)
            {
                InsNumArr[i].Text = (-1).ToString();
                InsPageNumArr[i].Text = (-1).ToString();
                MissPageArr[i].Text = "空闲";
                InsExetimesCntArr[i].Text = (-1).ToString();
            }
            ExeStatus.Text = "未开始";


        }
        public class MemoryBlock
        {

            public int insnum;  //记录存放的指令编号
            public int insPage;//记录指令页号
            public int insTime;     //指令进入时间
            public bool isava;
            public MemoryBlock()
            {
                this.insnum = -1;
                this.insTime = 1000;
                this.insPage = -1;
                this.isava = true;
            }

        }
        public class MainMemo
        {
            public int executeType;     //0 FIFO 1 LRU
            public int inscnt;  //用于记录执行的指令数量
            public int loscnt;  //用于记录缺页数量
            public bool isExecute;//是否在执行
            public int PrioInsType;//0顺后 1前 2顺前 3后
            public int mid;
            public int[] takenIns = new int[321];     //确定哪些指令被执行过
            public MemoryBlock[] memo = new MemoryBlock[4];     //4个内存块
            public MainMemo()
            {
                //RadomIns();
                this.executeType = -1;
                this.inscnt = 0;
                this.loscnt = 0;
                this.isExecute = false;
                this.PrioInsType = 1;
                this.mid = 320;
                for (int i = 0; i < 4; i++)
                    memo[i] = new MemoryBlock();
                for(int i=0;i<320;i++)
                {
                    takenIns[i] = -1;
                }
            }

        }
        MainMemo M = new MainMemo();      //总控制内存


        private int RadomIns()
        {

            if (M.mid == 319 || M.mid == 0)
            {
                M.PrioInsType = 1;
                M.mid = 320;
            }
            if(M.PrioInsType == 0 || M.PrioInsType == 2) //顺序
            {
                M.PrioInsType = (M.PrioInsType + 1) % 4;
                    
                if(M.takenIns[M.mid+1] == -1)
                {
                    M.takenIns[M.mid + 1] = 1;
                    
                }
                else
                {
                    M.takenIns[M.mid + 1]++;
                }

                return M.mid + 1;
            }
            else 
            {
                Random rdom = new Random();
                int n;
                if(M.PrioInsType==1) //跳转到前面
                    n = rdom.Next(0, M.mid - 1);
                else //跳转到后面
                    n = rdom.Next(M.mid + 2, 320);
                M.mid = n;
                if (M.takenIns[n] == -1)
                {
                    M.takenIns[n] = 1;
                }
                else
                {
                    M.takenIns[n]++;
                    //return -1;
                }
                M.PrioInsType = (M.PrioInsType + 1) % 4;
                return n;
            }
            /*else //跳转到后面
            {
                Random rdom = new Random();
                int n = rdom.Next(M.mid+2, 320);
                M.PrioInsType = (M.PrioInsType + 1) % 4;
                M.mid = n;
                if (M.takenIns[n] == -1)
                {
                    M.takenIns[n] = 1;
                    //return n;
                }
                else
                {
                    M.takenIns[n]++;
                    //return -1;
                }
               

                return n;

            }*/


        }
        private void ExecuteControl(object sender, EventArgs e)
        {
            if (!M.isExecute)
                return;
            ExeStatus.Text = "正在执行";
            Execute();
            double losrate = double.Parse(M.loscnt.ToString()) / double.Parse(M.inscnt.ToString());
            LosRate.Text = losrate.ToString();
            LosCnt.Text = M.loscnt.ToString();
        }
        private void Execute()
        {
            bool flag = false;
            for (int i = 0; i < 320; i++)
            {
                if (M.takenIns[i] == -1)
                {
                    flag = true;
                }
            }
            M.isExecute = flag;
            if (!flag)
            {
                ExeStatus.Text = "320条指令已全部执行完毕";
                return;
            }
            int curins_ = RadomIns();
            CurInsNumber.Text = curins_.ToString();

            M.inscnt++;
            ExecutedInsCnt.Text = M.inscnt.ToString();
            //计算页号
            int curinspage = curins_ / 10;
            bool InStorage = false;
            int pos = -1;
            for (int i = 0; i < 4; i++)
            {
                //在内存中
                if (M.memo[i].insPage == curinspage)
                {
                    InStorage = true;
                    pos = i;
                }
            }
            if(InStorage)
            {
                if (M.executeType == 0)
                    FIFO(curins_, curinspage, InStorage, pos);
                else
                    LRU(curins_, curinspage, InStorage, pos);
                return;
            }
            M.loscnt++;
            //页不在内存中，且内存有空闲
            for (int i = 0; i < 4; i++)
            {
                if (M.memo[i].isava)
                {
                    M.memo[i].insnum = curins_;
                    M.memo[i].insTime = M.inscnt - 1;
                    M.memo[i].insPage = curinspage;
                    M.memo[i].isava = false;

                    InsNumArr[i].Text = curins_.ToString();
                    InsPageNumArr[i].Text = curinspage.ToString();
                    MissPageArr[i].Text = "是";
                    InsExetimesCntArr[i].Text = M.takenIns[curins_].ToString();
                    return;
                }

            }
            //页不在内存中，且内存无空闲
            if (M.executeType == 0)
            {
                FIFO(curins_,curinspage,InStorage);
                //Algorithm.Text = "FIFO算法";
            }
            else
            {
                LRU(curins_, curinspage, InStorage);
            }
        }
        private void FIFO(int curins_,int curinspage,bool InStorage,int pos=-1)
        {
            //在内存中
            if(InStorage)
            {
                InsNumArr[pos].Text = curins_.ToString();
                InsPageNumArr[pos].Text = curinspage.ToString();
                MissPageArr[pos].Text = "否";

                M.memo[pos].insnum = curins_;
                //M.memo[pos].insTime = M.inscnt - 1;
                M.memo[pos].insPage = curinspage;
                InsExetimesCntArr[pos].Text = M.takenIns[curins_].ToString();
                return;
            }
            //页不在内存中，且内存无空闲
            int earliestTime = M.memo[0].insTime;
            int blockNum = 0;
            for (int i = 1; i < 4; i++)
            {
                if (M.memo[i].insTime < earliestTime)
                {
                    earliestTime = M.memo[i].insTime;
                    blockNum = i;
                }
            }
            M.memo[blockNum].insnum = curins_;
            M.memo[blockNum].insTime = M.inscnt - 1;
            M.memo[blockNum].isava = false;
            M.memo[blockNum].insPage = curinspage;

            InsNumArr[blockNum].Text = curins_.ToString();
            InsPageNumArr[blockNum].Text = curinspage.ToString();
            MissPageArr[blockNum].Text = "是";
            InsExetimesCntArr[blockNum].Text = M.takenIns[curins_].ToString();
        }
        private void LRU(int curins_,int curinspage, bool InStorage, int pos = -1)
        {
            //在内存中
            if (InStorage)
            {
                InsNumArr[pos].Text = curins_.ToString();
                InsPageNumArr[pos].Text = curinspage.ToString();
                MissPageArr[pos].Text = "否";

                M.memo[pos].insnum = curins_;
                M.memo[pos].insTime = M.inscnt - 1;
                M.memo[pos].insPage = curinspage;
                return;
            }
            //页不在内存中，且内存无空闲
            int earliestTime = M.memo[0].insTime;
            int blockNum = 0;
            for (int i = 1; i < 4; i++)
            {
                if (M.memo[i].insTime < earliestTime)
                {
                    earliestTime = M.memo[i].insTime;
                    blockNum = i;
                }
            }
            M.memo[blockNum].insnum = curins_;
            M.memo[blockNum].insTime = M.inscnt - 1;
            M.memo[blockNum].isava = false;
            M.memo[blockNum].insPage = curinspage;

            InsNumArr[blockNum].Text = curins_.ToString();
            InsPageNumArr[blockNum].Text = curinspage.ToString();
            MissPageArr[blockNum].Text = "是";
        }
        private void FIFOButton_Click(object sender, EventArgs e)
        {
            M.executeType = 0;
            Algorithm.Text = "FIFO算法";
        }

        private void LRUButton_Click(object sender, EventArgs e)
        {
            M.executeType = 1;
            Algorithm.Text = "LRU算法";
        }
        private void StartButton_Click(object sender, EventArgs e)
        {
            if (M.isExecute || M.executeType == -1)
                return;
            RadomIns();
            M.isExecute = true;
        }

        private void EndButton_Click(object sender, EventArgs e)
        {
            M.isExecute = false;
            ExeStatus.Text = "执行中断";
        }

        private void ResetButton_Click(object sender, EventArgs e)
        {
            M.executeType = -1;
            M.inscnt = 0;
            M.loscnt = 0;
            M.isExecute = false;
            M.PrioInsType = 1;
            M.mid = 320;
            for (int i = 0; i < 4; i++)
                M.memo[i] = new MemoryBlock();
            for (int i = 0; i < 320; i++)
            {
                M.takenIns[i] = -1;
            }
            ExecutedInsCnt.Text = M.inscnt.ToString();
            CurInsNumber.Text = (-1).ToString();
            LosCnt.Text = (0).ToString();
            LosRate.Text = (0).ToString();
            Algorithm.Text = "未选择";
            for (int i = 0; i < 4; i++)
            {
                InsNumArr[i].Text = (-1).ToString();
                InsPageNumArr[i].Text = (-1).ToString();
                MissPageArr[i].Text = "空闲";
                InsExetimesCntArr[i].Text = (-1).ToString();
            }
            ExeStatus.Text = "未开始";
        }

        private void Button4_Click(object sender, EventArgs e)
        {
            Button button = sender as Button;
            ExeInterval.Text = button.Text;
            MainControl.Interval = 50;
        }

        private void Button3_Click(object sender, EventArgs e)
        {
            Button button = sender as Button;
            ExeInterval.Text = button.Text;
            MainControl.Interval = 100;
            
        }

        private void Button2_Click(object sender, EventArgs e)
        {
            Button button = sender as Button;
            ExeInterval.Text = button.Text;
            MainControl.Interval = 500;
        }

        private void Button1_Click(object sender, EventArgs e)
        {
            Button button = sender as Button;
            ExeInterval.Text = button.Text;
            MainControl.Interval = 1000;
        }

        private void Button6_Click(object sender, EventArgs e)
        {
            Button button = sender as Button;
            ExeInterval.Text = button.Text;
            MainControl.Interval = 2000;
        }

        private void Button5_Click(object sender, EventArgs e)
        {
            Button button = sender as Button;
            ExeInterval.Text = button.Text;
            MainControl.Interval = 5000;
        }

    }
}
