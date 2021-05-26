using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ElevatorController
{
    public partial class Form1 : Form
    {
        //楼层标签组
        Label[] BFloor = new Label[20];
        //外部上按钮组
        Button[] Boup = new Button[20];
        //外部下按钮组
        Button[] Bodown = new Button[20];
        //电梯内部
        Button[] Bi1 = new Button[20];
        Button[] Bi2 = new Button[20];
        Button[] Bi3 = new Button[20];
        Button[] Bi4 = new Button[20];
        Button[] Bi5 = new Button[20];
        //开门键按钮组
        Button[] Dopen = new Button[5];
        //关门键按钮组
        Button[] Dclose = new Button[5];
        //警报键按钮组
        Button[] Dalarm = new Button[5];
        //每个电梯的调度Timer控件组
        Timer[] Timegroup = new Timer[5];
        //表示电梯，用于移动
        Button[] Ebutton = new Button[5];
        //用于表示电梯状态的标签组
        Label[] ELableStatus = new Label[5];
        //每个电梯的移动Timer控件组
        Timer[] MoveTimer = new Timer[5];
        private Queue<floorequrie> waitlist = new Queue<floorequrie>();

        public RichTextBox rtb;

        public elevator[] Elevators = new elevator[5];

        public struct floorequrie
        {
            //用于区分 0内部请求 1外部请求
            public int retype;
            //用于区分 0上升 1下降 3停靠
            public int dir;
            //是否有请求
            public bool havemission;
            public int rfloor;
        }

        public class elevator
        {
            //电梯状态 3停靠 0上升 1下降 4无状态,维护停靠结束的电梯 5警告状态
            public int estatus = 4;
            //维护移动方向 0上升 1下降 4无状态
            public int edir = 4;
            public int curefloor = 1;
            //电梯是否空闲
            public bool isavai = true;
            //任务清单
            public floorequrie[] relist = new floorequrie[20];
            public int stoptime = 0;
            public int alarmtime = 0;
        }
        //所有开门键响应的click函数
        private void DorOpenClick(object sender, EventArgs e)
        {
            Button button = sender as Button;
            int bnum = Array.IndexOf(Dopen, button);
            if (Elevators[bnum].estatus != 3 && Elevators[bnum].estatus != 4)
                return;
            Elevators[bnum].stoptime = 0;
            Elevators[bnum].estatus = 3;
            //ELableStatus[bnum].Text = "开门停靠";
        }
        //所有关门键响应的click函数
        private void DorCloseClick(object sender, EventArgs e)
        {
            Button button = sender as Button;
            int bnum = Array.IndexOf(Dclose, button);
            if (Elevators[bnum].estatus != 3)
                return;
            Elevators[bnum].estatus = 4;
            Elevators[bnum].stoptime = 0;
            ELableStatus[bnum].Text = "关门停靠";

            //ELableStatus[bnum].Text = "关门停靠";
            //Elevators[bnum].stoptime = 0;
            //Elevators[bnum].estatus = 4;
            //if (Elevators[bnum].relist[Elevators[bnum].curefloor - 1].retype == 0)
            
                if (bnum == 0)
                {
                    Bi1[Elevators[bnum].curefloor - 1].Enabled = true;
                }
                else if (bnum == 1)
                    Bi2[Elevators[bnum].curefloor - 1].Enabled = true;
                else if (bnum == 2)
                    Bi3[Elevators[bnum].curefloor - 1].Enabled = true;
                else if (bnum == 3)
                    Bi4[Elevators[bnum].curefloor - 1].Enabled = true;
                else
                    Bi5[Elevators[bnum].curefloor - 1].Enabled = true;
       
            if (Elevators[bnum].relist[Elevators[bnum].curefloor - 1].dir == 0)
                    Boup[Elevators[bnum].curefloor - 1].Enabled = true;
            else
                    Bodown[Elevators[bnum].curefloor - 1].Enabled = true;
           

        }
        //所有警报键响应的click函数
        private void DorAlarmClick(object sender, EventArgs e)
        {
            Button button = sender as Button;
            int bnum = Array.IndexOf(Dalarm, button);
            Elevators[bnum].estatus = 5;
            Button[] b;
            if (bnum == 0)
                b = Bi1;
            else if (bnum == 1)
                b = Bi2;
            else if (bnum == 2)
                b = Bi3;
            else if (bnum == 3)
                b = Bi4;
            else
                b = Bi5;
            for (int i = 0; i < 20; i++)
            {
                b[i].Enabled = false;
                //Elevators[bnum].estatus = 5;
                //清除所有任务
                Elevators[bnum].relist[i].havemission = false;
                if(Elevators[bnum].relist[i].retype==1)
                {
                    if (Elevators[bnum].relist[i].dir == 1)
                        Bodown[Elevators[bnum].relist[i].rfloor].Enabled = true;
                    else
                        Boup[Elevators[bnum].relist[i].rfloor].Enabled = true;
                }
                //Elevators[bnum].relist[i].retype = -1;
            }
            
        }
        //所有外请求键响应的click函数，用于分配外请求
        private void OutClick(object sender, EventArgs e)
        {
            Button button = sender as Button;
            //上行下行的type
            int type;
            int bnum = Array.IndexOf(Bodown, button);
            string t;
            if (bnum == -1)
            {
                bnum = Array.IndexOf(Boup, button);
                //上行
                type = 0;
                t = "上升";
            }
            else
            {
                type = 1;//下行
                t = "下降";
            }
            rtb.AppendText("第" + (bnum + 1) + "层楼发起" + t + "请求\n");
            bool[] ava = new bool[5];
            for (int i = 0; i < 5; i++)
                ava[i] = false;
            bool canresponse = false;
            //扫描可执行电梯
            if (type == 1)
            {
                for (int i = 0; i < 5; i++)
                {
                    //电梯空闲或正在下行，且楼层高于请求层 电梯处于警告状态则完全失效
                    if (Elevators[i].estatus != 5 && (
                       (Elevators[i].estatus == 3 )||
                        (Elevators[i].isavai == true) || 
                        (Elevators[i].estatus == 1 && bnum < Elevators[i].curefloor)))
                    {
                        ava[i] = true;
                        //canresponse = true;
                    }
                }
            }
            else
            {
                for (int i = 0; i < 5; i++)
                {
                    //电梯空闲或正在上行且楼层小于请求层
                    if (Elevators[i].estatus != 5 && (
                        (Elevators[i].estatus == 3 ) || 
                        ((Elevators[i].estatus == 0 && bnum > Elevators[i].curefloor) ||
                        Elevators[i].isavai == true)))
                    {
                        ava[i] = true;
                        //canresponse = true;
                    }

                }
            }
            for(int i=0;i<5;i++)
            {
                if (Elevators[i].relist[bnum].havemission && Elevators[i].relist[bnum].dir!=type)
                {
                    ava[i] = false;
                }
            }
            for(int i=0;i<5;i++)
            {
                if (ava[i])
                    canresponse = true;
            }
            //有电梯可以响应则响应 否则加入等待队列
            
            //多部电梯可以响应则按距离安排
            if (canresponse == true)
            {
                int mindis = 50;
                int eid = -1;
                for (int i = 0; i < 5; i++)
                {
                    int dis = System.Math.Abs(Elevators[i].curefloor - bnum);

                    if (ava[i] && dis < mindis)
                    {
                        mindis = dis;
                        eid = i;
                    }
                }
                //加入电梯任务清单
                rtb.AppendText("第" + (bnum + 1) + "层楼的外部请求被第" + (eid + 1) + "部电梯响应\n");
                if (Elevators[eid].estatus==3 && Elevators[eid].curefloor==bnum + 1)
                {
                    Elevators[eid].stoptime = 0;
                    return;
                }
                Elevators[eid].relist[bnum].retype = 1;
                Elevators[eid].relist[bnum].havemission = true;
                Elevators[eid].relist[bnum].rfloor = bnum + 1;
                if (Elevators[eid].curefloor == bnum + 1)
                {
                    Elevators[eid].relist[bnum].dir = 3;
                    return;
                }
                else
                    Elevators[eid].relist[bnum].dir = type;

                if (Elevators[eid].isavai)
                {
                    Elevators[eid].isavai = false;
                    //Elevators[eid].estatus = type;
                    //Elevators[eid].edir = type;
                }
                
            }
            else
            {
                floorequrie wrequire = new floorequrie();
                wrequire.rfloor = bnum;
                wrequire.retype = 1;
                wrequire.dir = type;
                wrequire.havemission = true;
                waitlist.Enqueue(wrequire);
                rtb.AppendText("第" + (bnum+1) + "层楼的请求暂时没有电梯可以响应\n");
            }
            button.Enabled = false;


        }

        //所有Timegroup数组内的控件响应的tick函数，用于电梯调度
        private void ControlTimer(object sender, EventArgs e)
        {
            Timer timer = sender as Timer;
            int tnum = Array.IndexOf(Timegroup, timer);
            if(Elevators[tnum].estatus==5)
            {
                Button[] b;
                if (tnum == 0)
                    b = Bi1;
                else if (tnum == 1)
                    b = Bi2;
                else if (tnum == 2)
                    b = Bi3;
                else if (tnum == 3)
                    b = Bi4;
                else 
                    b = Bi5;
                Elevators[tnum].alarmtime++;
                if(Elevators[tnum].alarmtime>30)
                {
                    for (int i = 0; i < 20; i++)
                    {
                        b[i].Enabled = true;
                    }
                    Elevators[tnum].alarmtime = 0;
                    Elevators[tnum].estatus = 4;
                    Elevators[tnum].edir = 4;
                    ELableStatus[tnum].Text = "警报解除";
                    return;
                }
                ELableStatus[tnum].Text = "警报状态,不可用,剩余"+(30- Elevators[tnum].alarmtime).ToString()+"秒";
                /*for(int i=0;i<20;i++)
                {
                    b[i].Enabled = false;
                    Elevators[tnum].estatus = 5;
                }
                */
                return;
            }

            //电梯是否空闲
            
             Elevators[tnum].isavai = true;
            for(int i=0;i<20;i++)
            {
                if(Elevators[tnum].relist[i].havemission)
                {
                    Elevators[tnum].isavai = false;
                    break;
                }
            }
            

            string n = Elevators[tnum].curefloor.ToString();
            //处理任务
            //是否需要停靠 方向一致则停靠
            if (Elevators[tnum].relist[Elevators[tnum].curefloor - 1].havemission)
            {
                //置为停靠
                Elevators[tnum].edir = Elevators[tnum].estatus;
                Elevators[tnum].estatus = 3;

                //请求处理完毕
                Elevators[tnum].relist[Elevators[tnum].curefloor - 1].havemission = false;
                rtb.AppendText("第" + (tnum + 1) + "部电梯到达了第" + (Elevators[tnum].curefloor) + "层\n");
            }

            //停靠记时
            if (Elevators[tnum].estatus == 3)
            {
                if (Elevators[tnum].stoptime < 6)
                {
                    int lefttime = 5 - Elevators[tnum].stoptime;
                    string t = lefttime.ToString();
                    ELableStatus[tnum].Text = "开门停靠，还需" + t + "秒";
                    Elevators[tnum].stoptime += 1;
                    //电梯处于无效状态
                    //Elevators[tnum].isavai = false;
                }
                else
                {

                    ELableStatus[tnum].Text = "关门停靠";
                    Elevators[tnum].stoptime = 0;
                    Elevators[tnum].estatus = 4;
                    //if (Elevators[tnum].relist[Elevators[tnum].curefloor - 1].retype == 0)
                  
                   if (tnum == 0)
                   {
                        Bi1[Elevators[tnum].curefloor - 1].Enabled = true;
                   }
                   else if (tnum == 1)
                        Bi2[Elevators[tnum].curefloor - 1].Enabled = true;
                   else if (tnum == 2)
                        Bi3[Elevators[tnum].curefloor - 1].Enabled = true;
                   else if (tnum == 3)
                        Bi4[Elevators[tnum].curefloor - 1].Enabled = true;
                   else
                        Bi5[Elevators[tnum].curefloor - 1].Enabled = true;
                    
                    //else
                    
                   if (Elevators[tnum].relist[Elevators[tnum].curefloor - 1].dir == 0)
                        Boup[Elevators[tnum].curefloor - 1].Enabled = true;
                   else
                        Bodown[Elevators[tnum].curefloor - 1].Enabled = true;
                }
                return;
            }
            if(Elevators[tnum].estatus==4)
            {
                ELableStatus[tnum].Text = "关门停靠";
            }
            bool up = false, down = false;
            //遍历任务清单
            for (int i = 0; i < 20; i++)
            {
                //上升的任务
                if (Elevators[tnum].relist[i].havemission && Elevators[tnum].curefloor < i + 1)
                {
                    up = true;
                }
                //下降的任务
                if (Elevators[tnum].relist[i].havemission && Elevators[tnum].curefloor > i + 1)
                {
                    down = true;
                }
            }
            //处理等待任务
            if (waitlist.Count > 0)
            {
                floorequrie f;
                bool canres = true;
                while (canres && waitlist.Count > 0)
                {
                    f = waitlist.Peek();
                    bool t = false;
                    for (int i = 0; i < 5; i++)
                    {
                        //可以响应 加入清单
                        if (!Elevators[i].relist[f.rfloor].havemission)
                        {
                            Elevators[i].relist[f.rfloor] = f;
                            waitlist.Dequeue();
                            t = true;
                            rtb.AppendText("第" + (f.rfloor + 1).ToString() + "层的等待请求被第" + (i + 1) + "部电梯响应");
                            break;
                        }
                    }
                    if (!t)
                        canres = false;

                }
            }
            //只处理无状态的电梯
            if (Elevators[tnum].estatus == 4)
            {
                Elevators[tnum].isavai = false;
                if (up && !down) //有上无下
                {
                    //上升状态，保持
                    Elevators[tnum].estatus = 0;
                    if (Elevators[tnum].edir == 1)  //下降状态，改变
                        Elevators[tnum].edir = 0;
                }
                else if (!up && down)   //有下无上
                {
                    Elevators[tnum].estatus = 1;
                    if (Elevators[tnum].edir == 0)  //上升状态，改变
                        Elevators[tnum].edir = 1;
                }
                else if (up && down) //有上有下 
                {
                    Elevators[tnum].estatus = Elevators[tnum].edir;
                }
                else
                {
                    Elevators[tnum].isavai = true;
                    Elevators[tnum].edir = 4;
                }
            }


        }

        //所有内请求键响应的click函数，用于分配内请求
        private void InClick(object sender, EventArgs e)
        {
            Button button = sender as Button;
            int bnum = -1, ofloor = -1;
            int[] Outre = new int[5];
            Outre[0] = Array.IndexOf(Bi1, button);
            Outre[1] = Array.IndexOf(Bi2, button);
            Outre[2] = Array.IndexOf(Bi3, button);
            Outre[3] = Array.IndexOf(Bi4, button);
            Outre[4] = Array.IndexOf(Bi5, button);
            //找到请求电梯
            for (int i = 0; i < 5; i++)
            {
                if (Outre[i] != -1)
                {
                    bnum = i;
                    ofloor = Outre[i];
                    break;
                }
            }
            int type;
            string t;
            if (Elevators[bnum].curefloor < ofloor + 1)
            {
                type = 0;
                t = "上升前往" + (ofloor + 1) + "层的";
            }
            else if (Elevators[bnum].curefloor > ofloor + 1)
            {
                type = 1;
                t = "下降前往" + (ofloor + 1) + "层的";
            }
            else
            {
                t = "停靠";
                type = 3;
            }
            rtb.AppendText("第" + (bnum + 1) + "部电梯发起" + t + "内部请求\n");
            //安排电梯
            button.Enabled = false;
            if (Elevators[bnum].relist[ofloor].havemission)
                return;
            else
            {
                //停靠任务
                Elevators[bnum].relist[ofloor].havemission = true;
                Elevators[bnum].relist[ofloor].retype = 0;
                Elevators[bnum].relist[ofloor].dir = type;

            }
            //button.Enabled = false;
        }

        //所有MoveTimer数组内的控件响应的tick函数，用于电梯移动
        private void MoveControlTimer(object sender, EventArgs e)
        {
            Timer timer = sender as Timer;
            int tnum = Array.IndexOf(MoveTimer, timer);
            elevator curE = Elevators[tnum];


            if (Elevators[tnum].estatus == 0)
            {

                ELableStatus[tnum].Text = "上升";
                Ebutton[tnum].Location = new Point(Ebutton[tnum].Location.X, Bi1[Elevators[tnum].curefloor].Location.Y);
                Ebutton[tnum].Text = (Elevators[tnum].curefloor + 1).ToString();
                Elevators[tnum].curefloor ++;

            }
            else if (Elevators[tnum].estatus == 1)
            {
                ELableStatus[tnum].Text = "下降";
                Ebutton[tnum].Location = new Point(Ebutton[tnum].Location.X, Bi1[Elevators[tnum].curefloor-2].Location.Y);
                Ebutton[tnum].Text = (Elevators[tnum].curefloor-1).ToString();
                Elevators[tnum].curefloor--;
                
            }
            /*if (curE.estatus == 0)
                ELable[tnum].Location = new Point(ELable[tnum].Location.X, ELable[tnum].Location.Y - gap/10);
            else if (curE.estatus == 1)
                ELable[tnum].Location = new Point(ELable[tnum].Location.X, ELable[tnum].Location.Y + gap/10);
                */
            /*for(int i=0;i<20;i++)
            {
                if (Ebutton[tnum].Location.Y == Bi1[i].Location.Y)
                {
                    Ebutton[tnum].Text = Bi1[i].Text;
                    Elevators[tnum].curefloor = i+1;
                    break;
                }
            }
            */
        }

        public Form1()
        {
            InitializeComponent();

            for (int i = 0; i < 5; i++)
            {
                Elevators[i] = new elevator();
                for (int j = 0; j < 10; j++)
                {
                    Elevators[i].relist[j].retype = -1;
                    Elevators[i].relist[j].dir = -1;
                    Elevators[i].relist[j].havemission = false;
                    Elevators[i].relist[j].rfloor = j + 1;
                }
            }
            Dopen[0] = button141;
            Dopen[1] = button146;
            Dopen[2] = button149;
            Dopen[3] = button152;
            Dopen[4] = button155;

            Dclose[0] = button142;
            Dclose[1] = button145;
            Dclose[2] = button148;
            Dclose[3] = button151;
            Dclose[4] = button154;

            Dalarm[0] = button143;
            Dalarm[1] = button144;
            Dalarm[2] = button147;
            Dalarm[3] = button150;
            Dalarm[4] = button153;

            Timegroup[0] = timer1;
            Timegroup[1] = timer2;
            Timegroup[2] = timer3;
            Timegroup[3] = timer4;
            Timegroup[4] = timer5;

            MoveTimer[0] = timer6;
            MoveTimer[1] = timer7;
            MoveTimer[2] = timer8;
            MoveTimer[3] = timer9;
            MoveTimer[4] = timer10;

            ELableStatus[0] = label26;
            ELableStatus[1] = label27;
            ELableStatus[2] = label28;
            ELableStatus[3] = label29;
            ELableStatus[4] = label30;

            Ebutton[0] = button156;
            Ebutton[1] = button157;
            Ebutton[2] = button158;
            Ebutton[3] = button159;
            Ebutton[4] = button160;

            BFloor[0] = label1; BFloor[1] = label2; BFloor[2] = label3; BFloor[3] = label4;
            BFloor[4] = label5; BFloor[5] = label6; BFloor[6] = label7; BFloor[7] = label8;
            BFloor[8] = label19; BFloor[9] = label10; BFloor[10] = label11; BFloor[11] = label12;
            BFloor[12] = label13; BFloor[13] = label14; BFloor[14] = label15; BFloor[15] = label16;
            BFloor[16] = label17; BFloor[17] = label18; BFloor[18] = label19; BFloor[19] = label20;

            Boup[0] = button1; Boup[1] = button2; Boup[2] = button3; Boup[3] = button4;
            Boup[4] = button5; Boup[5] = button6; Boup[6] = button7; Boup[7] = button8;
            Boup[8] = button9; Boup[9] = button10; Boup[10] = button11; Boup[11] = button12;
            Boup[12] = button13; Boup[13] = button14; Boup[14] = button15; Boup[15] = button16;
            Boup[16] = button17; Boup[17] = button18; Boup[18] = button19; Boup[19] = button20;

            Bodown[19] = button21; Bodown[18] = button22; Bodown[17] = button23; Bodown[16] = button24;
            Bodown[15] = button25; Bodown[14] = button26; Bodown[13] = button27; Bodown[12] = button28;
            Bodown[11] = button29; Bodown[10] = button30; Bodown[9] = button31; Bodown[8] = button32;
            Bodown[7] = button33; Bodown[6] = button34; Bodown[5] = button35; Bodown[4] = button36;
            Bodown[3] = button37; Bodown[2] = button38; Bodown[1] = button39; Bodown[0] = button40;

            Bi1[19] = button41; Bi1[18] = button42; Bi1[17] = button43; Bi1[16] = button44;
            Bi1[15] = button45; Bi1[14] = button46; Bi1[13] = button47; Bi1[12] = button48;
            Bi1[11] = button49; Bi1[10] = button50; Bi1[9] = button51; Bi1[8] = button52;
            Bi1[7] = button53; Bi1[6] = button54; Bi1[5] = button55; Bi1[4] = button56;
            Bi1[3] = button57; Bi1[2] = button58; Bi1[1] = button59; Bi1[0] = button60;


            Bi2[0] = button61; Bi2[1] = button62; Bi2[2] = button63; Bi2[3] = button64;
            Bi2[4] = button65; Bi2[5] = button66; Bi2[6] = button67; Bi2[7] = button68;
            Bi2[8] = button69; Bi2[9] = button70; Bi2[10] = button71; Bi2[11] = button72;
            Bi2[12] = button73; Bi2[13] = button74; Bi2[14] = button75; Bi2[15] = button76;
            Bi2[16] = button77; Bi2[17] = button78; Bi2[18] = button79; Bi2[19] = button80;

            Bi3[0] = button81; Bi3[1] = button82; Bi3[2] = button83; Bi3[3] = button84;
            Bi3[4] = button85; Bi3[5] = button86; Bi3[6] = button87; Bi3[7] = button88;
            Bi3[8] = button89; Bi3[9] = button90; Bi3[10] = button91; Bi3[11] = button92;
            Bi3[12] = button93; Bi3[13] = button94; Bi3[14] = button95; Bi3[15] = button96;
            Bi3[16] = button97; Bi3[17] = button98; Bi3[18] = button99; Bi3[19] = button100;

            Bi4[0] = button101; Bi4[1] = button102; Bi4[2] = button103; Bi4[3] = button104;
            Bi4[4] = button105; Bi4[5] = button106; Bi4[6] = button107; Bi4[7] = button108;
            Bi4[8] = button109; Bi4[9] = button110; Bi4[10] = button111; Bi4[11] = button112;
            Bi4[12] = button113; Bi4[13] = button114; Bi4[14] = button115; Bi4[15] = button116;
            Bi4[16] = button117; Bi4[17] = button118; Bi4[18] = button119; Bi4[19] = button120;

            Bi5[0] = button121; Bi5[1] = button122; Bi5[2] = button123; Bi5[3] = button124;
            Bi5[4] = button125; Bi5[5] = button126; Bi5[6] = button127; Bi5[7] = button128;
            Bi5[8] = button129; Bi5[9] = button130; Bi5[10] = button131; Bi5[11] = button132;
            Bi5[12] = button133; Bi5[13] = button134; Bi5[14] = button135; Bi5[15] = button136;
            Bi5[16] = button137; Bi5[17] = button138; Bi5[18] = button139; Bi5[19] = button140;

            for (int i = 0; i < 20; i++)
            {
                Boup[i].Text = "↑";
                Bodown[i].Text = "↓";
                string tag = (i+1).ToString();
                Bi1[i].Text = tag;
                Bi2[i].Text = tag;
                Bi3[i].Text = tag;
                Bi4[i].Text = tag;
                Bi5[i].Text = tag;
                BFloor[i].Text = (i + 1).ToString();
            }
            rtb = richTextBox1;
            rtb.Multiline = true;
            rtb.RightMargin = 150;
            rtb.ScrollBars = RichTextBoxScrollBars.Vertical;
            rtb.SelectionBullet = true;
        }
    }
}
