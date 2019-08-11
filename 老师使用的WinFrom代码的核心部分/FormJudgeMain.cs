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
using System.Diagnostics;
using System.Threading;
using System.Collections;

namespace Judge2017
{
    public partial class FormJudge : Form
    {
        ArrayList submitSet = new ArrayList();
        private tysrData.sPaperItem currentSubmit;
        List<tysrData.sProgramData> dataSet;
        static public string name = "";
        private string filename;
        private Process process = new Process();
        private DateTime TimerRunTime = new DateTime();//本次计时器的时间，也就是当前进程的实现时间。
        private Thread thread;
        private double[] threadRunTime;
        private int[] threadRunSpace;
        private string[] threadInput;
        private string[] threadError;
        private string[] threadOutput;
        private int[] result;//1表示compile error；2表示accept；3表示Error；4表示TLE；5表示SLE
        private int ResultState;//说明同上
        private int RunState=-2;//开始
        private int threadNumber;//测试数据的索引
        private int acceptCount;//正确的测试数据个数
        private string output;
        public FormJudge()
        {
            InitializeComponent();
        }

        private void buttonClear_Click(object sender, EventArgs e)
        {
            this.textBoxMessage.Text = "";
        }
        /*
         * RunState：
         *   -2 读取WAIT状态的提交
         *   -1 读取该提交的测试数据文件
         */
        private void timerJudge_Tick(object sender, EventArgs e)
        {
            try
            {
                if (RunState == -2) //-2表示一切从新开始
                {//读取用户提交的WAIT状态的代码
                    
                    try
                    {
                        if (submitSet.Count <= 0)
                            throw new Exception("没有WAIT状态的提交了！");
                        this.textBoxMessage.AppendText("\r\n读取新的提交");
                        currentSubmit = (tysrData.sPaperItem)submitSet[0];
                        RunState = -1;//
                    }
                    catch (Exception ex)
                    {
                        //this.textBoxMessage.AppendText("\r\n"+ex.Message);
                        return;
                    }
                    return;
                }
                if (RunState == -1) //-1 表示读取数据
                {
                    try
                    {
                        this.textBoxMessage.AppendText("\r\n读取新的数据");
                        this.textBoxMessage.ScrollToCaret();
                        currentSubmit=(tysrData.sPaperItem)submitSet[0];
                        dataSet = Program.tysrDatabase.GetDatasByProblemId(currentSubmit.problemId);
                        if (dataSet.Count() <= 0)
                        {
                            RunState = -2; //回复为
                            submitSet.RemoveAt(0);
                            throw new Exception(currentSubmit.problemId + "：没有测试数据");
                        }
                        RunState = 0;
                    }
                    catch (Exception ex)
                    {
                        this.textBoxMessage.AppendText("\r\n"+ex.Message);
                        this.textBoxMessage.ScrollToCaret();
                        return;
                    }
                    return;
                }
                 if (RunState == 0) //0表示开始 ，从数据中获取相关数据
                {//首先把字符串存入文件中
                    try
                    {
                        Random rand = new Random();
                        filename = "temp\\tysr2017";
                        FileStream Fs = new FileStream(filename + ".cpp", FileMode.Create);
                        StreamWriter sw = new StreamWriter(Fs);
                        sw.Write(currentSubmit.answer + "\r\n");
                        sw.Close();
                        Fs.Close();
                        RunState = 1;//把状态修改为编译的开始状态
                    }
                    catch (Exception ex)
                    {
                        this.textBoxMessage.AppendText("\r\n" + ex.Message);
                        this.textBoxMessage.ScrollToCaret();
                        return;
                    }
                    return;
                }
               if (RunState == 1)//1表示编译的开始状态
                {
                    this.textBoxMessage.AppendText("\r\n开始编译...");
                    this.textBoxMessage.ScrollToCaret();
                    thread = new Thread(new ThreadStart(Compile));//绑定thread
                    thread.Start();
                    RunState = 2;
                }
                 //对文件进行编译
                if (RunState == 2)//表示编译中的状态
                {
                    if (thread.ThreadState != System.Threading.ThreadState.Stopped)
                    {
                        this.textBoxMessage.AppendText(".");
                        this.textBoxMessage.ScrollToCaret();
                    }
                    else
                    {
                        RunState = 3;
                    }
                    return;
                }
               if (RunState == 3)//3表示编译结束的状态
                {
                    try
                    {
                        thread.Abort();
                        string Error = process.StandardError.ReadToEnd();
                        process.WaitForExit();
                        process.Close();
                        thread.Abort();
                        while (Error.IndexOf("句柄无效") >= 0)
                            Error = Error.Remove(Error.IndexOf("句柄无效"));
                        string ts = filename.Substring(4);
                        while (Error.IndexOf(ts) >= 0)
                        {
                            int t = Error.IndexOf(ts);
                            Error = Error.Substring(0, t) + "test" + Error.Substring(t + ts.Length);
                        }
                        Error = Error.Trim();
                        if (Error.Length > 0)
                        {
                            this.textBoxMessage.AppendText("\r\n编译错误：\r\n"+Error);
                            this.textBoxMessage.ScrollToCaret();
                            RunState = -2;//结束
                            submitSet.RemoveAt(0);
                            return;
                        }
                        else
                        {
                            this.textBoxMessage.AppendText("\r\n编译完成.....");
                            this.textBoxMessage.ScrollToCaret();
                            RunState = 4;//
                        }
                    }
                    catch (Exception ex)
                    {
                        this.textBoxMessage.AppendText("\r\n编译异常。。。");
                        this.textBoxMessage.ScrollToCaret();
                        RunState = -2;//结束
                        submitSet.RemoveAt(0);
                        return;
                    }
                    return;
                }
                 if (RunState == 4)//执行的开始态 做各种准备工作
                {
                    //判断
                    this.textBoxMessage.AppendText("\r\n开始执行....\r\n");
                    this.textBoxMessage.ScrollToCaret();
                    
                    threadInput = new string[dataSet.Count()];
                    threadOutput = new string[dataSet.Count()];
                    threadRunTime = new double[dataSet.Count()];
                    threadRunSpace = new int[dataSet.Count()];
                    result = new int[dataSet.Count()];
                    acceptCount = 0;
                    for (threadNumber = 0; threadNumber < dataSet.Count(); threadNumber++)
                    {
                        threadInput[threadNumber] = dataSet[threadNumber].dataInput;
                        threadOutput[threadNumber] = dataSet[threadNumber].dataOutput;
                        threadRunTime[threadNumber] = 0;
                        threadRunSpace[threadNumber] = 0;
                    }
                    threadNumber = 0;
                    RunState = 5;
                    TimerRunTime = DateTime.Now;
                }
               if (RunState == 5)//某一次执行的开始态。为执行准备环境，启动可执行程序
                {
                    this.textBoxMessage.AppendText("\r\n"+ "第" + (threadNumber + 1).ToString() + "组数据,执行中..");
                    thread = new Thread(new ThreadStart(Run));
                    thread.Start();
                    RunState = 6;
                }
                 if (RunState == 6) //某一次执行中
                {
                    if (thread.ThreadState != System.Threading.ThreadState.Stopped)
                    {
                        //          if (threadRunSpace[threadNumber] < process.WorkingSet)
                        //          {
                        //              threadRunSpace[threadNumber] = process.WorkingSet;
                        this.textBoxMessage.AppendText(".");
                        if (TimerRunTime.AddSeconds(2) < DateTime.Now) //强制停止
                        {
                            threadRunTime[threadNumber] = 2001;
                           // process.Close();
                            process.Kill();
                            thread.Abort();
                            RunState = 7;
                        }
                        //          }
                    }
                    else
                    {
                        RunState = 7;
                    }
                }
                if (RunState == 7) //某一次执行结束态
                {

                    output = process.StandardOutput.ReadToEnd();
                    threadRunTime[threadNumber] = this.process.TotalProcessorTime.TotalMilliseconds;//DateTime.Now.Subtract( TimerRunTime).TotalMilliseconds;
                    process.Close();
                    //比较
                    if (output.Length>0&&output[output.Length - 1] == '\n')
                       output = output.Remove(output.Length - 2);//去掉某位一个回车
                    if (output == threadOutput[threadNumber])
                    {
                        
                        if (threadRunTime[threadNumber] <= 1000)
                        {
                            this.textBoxMessage.AppendText("\r\naccept\r\n");
                            result[threadNumber] = 2;
                            acceptCount++;
                        }
                        else
                        {
                            this.textBoxMessage.AppendText("\r\nTime limit exceeded！\r\n");
                            result[threadNumber] = 3;
                        }
                        this.textBoxMessage.AppendText("\r\n耗时：" + threadRunTime[threadNumber].ToString() + "毫秒\r\n\r\n");

                    }
                    else
                    {
                        if (threadRunTime[threadNumber] > 1000)
                        {
                            this.textBoxMessage.AppendText("\r\nTime limit exceeded！\r\n");
                            result[threadNumber] = 3;
                        }
                        else
                        {
                            this.textBoxMessage.AppendText("\r\nerror!！\r\n");
                            result[threadNumber] = 4;
                        }
                        
                    }
                    threadNumber++;
                    TimerRunTime = DateTime.Now;
                    if (threadNumber >= dataSet.Count())
                        RunState = 8;
                    else
                        RunState = 5;
                }
                if (RunState == 8) //执行的结束态
                {
                    thread.Abort();//中断thread
                    submitSet.RemoveAt(0);
                    RunState = -2;//准备下一次读入数据
                    submitSet.RemoveAt(0);
                    ResultState = 2;
                    foreach (int t in result)
                        if (t >ResultState)
                            ResultState = t;
                    if (ResultState == 2)
                    {
                        this.textBoxMessage.AppendText("\r\n本题最终结果：Accept!\r\n");
                    }
                    else if (ResultState == 3)
                    {
                        this.textBoxMessage.AppendText("\r\n本题最终结果：Error!\r\n");
                    }
                    else if (ResultState == 4)
                    {
                        this.textBoxMessage.AppendText("\r\n本题最终结果：Time Limit Exceeded!\r\n");
                    }
                    //删除所有的temp目录下，有关文件
                    if (Directory.Exists("temp"))
                    {
                        string[] s = Directory.GetFiles("temp");
                        foreach (string ts in s)
                            File.Delete(ts);
                    }
                    //把结果存入到数据库中
                    currentSubmit.score=acceptCount * 10 / dataSet.Count();
                    Program.tysrDatabase.paperCorrect.Update(currentSubmit);
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        private void Compile()
        {
            process.StartInfo.FileName = "MinGW64\\bin\\g++.exe ";
            process.StartInfo.Arguments = "" + filename + ".cpp -o " + filename + ".exe";
            //process.StartInfo.WorkingDirectory = "MinGW64\\bin";
            process.StartInfo.RedirectStandardError = true;
            process.StartInfo.RedirectStandardInput = false;
            process.StartInfo.RedirectStandardOutput = false;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.CreateNoWindow = true;
            process.Start();
        }
        private void Run()
        {
            process.StartInfo.FileName = filename + ".exe";
            process.StartInfo.RedirectStandardInput = true;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = false;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.CreateNoWindow = true;
            process.Start();
            process.StandardInput.Write(threadInput[threadNumber]);
            process.StandardInput.Close();//不关闭，不停止。在这地方耽误了快一天了。
            process.WaitForExit(2000);
            //     process.Close();
        }

        private void button_start_Click(object sender, EventArgs e)
        {
            this.timerJudge.Enabled = true;
            Program.tysrDatabase.paper.Select();
            Program.tysrDatabase.programData.Select();
            foreach (tysrData.sPaperItem paperItem in Program.tysrDatabase.paper.paperItemSet)
                if (paperItem.problemId.Substring(0, 3) == "109")
                    submitSet.Add(paperItem);
        }
    
    
    }
}
