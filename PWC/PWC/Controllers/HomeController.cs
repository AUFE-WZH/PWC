using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data;

using PWC.Models;
using PWC.Common;
using PWC.Filter;
using System.Text.RegularExpressions;

using System.IO;
using System.Diagnostics;
using System.Threading;

namespace PWC.Controllers
{
    public class HomeController : Controller
    {
        //
        // GET: /Home/

        PWCEntities entity = new PWCEntities();
        HttpCookie cookie = new HttpCookie("AUFElogin");

        Process process = new Process();//进程
        string filepath;
        string filename;                //文件名
        private Thread thread;          //线程

        private string CompileError;    //编译错误

        private DateTime RunTime = new DateTime();//运行时间
        private double AvgRunTime;      //平均运行时间
        private int RunState;           //运行状态

        private int TestNumber;         //测试数据数量
        private string[] Test;          //测试数据
        private string[] RunOut;        //运行输出结果
        private string[] TrueOut;       //正确结果

        //首页显示功能
        public ActionResult Index()
        {

            SetSessionAndCookie();

            List<Result> results = entity.Result.OrderBy(p => p.ID).ToList();
            ViewBag.AC = results.Where(p => p.ID == 2).FirstOrDefault().ProNumber;
            ViewBag.WA = results.Where(p => p.ID == 3).FirstOrDefault().ProNumber;
            ViewBag.RTE = results.Where(p => p.ID == 4).FirstOrDefault().ProNumber;
            ViewBag.TLE = results.Where(p => p.ID == 5).FirstOrDefault().ProNumber;
            ViewBag.PE = results.Where(p => p.ID == 6).FirstOrDefault().ProNumber;
            ViewBag.ME = results.Where(p => p.ID == 7).FirstOrDefault().ProNumber;
            ViewBag.CE = results.Where(p => p.ID == 8).FirstOrDefault().ProNumber;

            List<Notice> notice = entity.Notice.Where(p => p.State == 1).OrderBy(p => p.ID).ToList();
            List<User> users = entity.User.OrderByDescending(p => p.Problems).ToList();
            List<Problem> problems = entity.Problem.OrderByDescending(p => p.Answers).ToList();
            List<Record> records = entity.Record.OrderBy(p => p.TodayDate).ToList();

            var tupleList = new Tuple<List<Notice>, List<User>, List<Problem>,
                List<Record>>(notice, users, problems, records);
            return View(tupleList);
        }

        //登录功能
        public void Login(FormCollection col)
        {
            string UserName = col["UserName"];
            string Password = col["Password"];
            string Checkbox = col["checkbox"];
            string yzm = col["yzm"];
            string Href = col["Href"];

            if (!yzm.Equals(TempData["SecurityCode"]))
            {
                Response.Write(JsHelper.Messagebox("验证码不正确，登录不成功！", Href));
            }
            else
            {
                User user = entity.User.Where(p => p.Name == UserName && p.Password == Password).FirstOrDefault();
                //判断用户是否存在
                if (user == null)
                {
                    Session["UId"] = null;
                    Response.Write(JsHelper.Messagebox("登录不成功，请检查用户名或密码是否正确！", Href));
                }
                else
                {
                    Session["Login"] = 1;
                    Session["UId"] = user.ID;
                    Session["UserName"] = UserName;
                    Session["Password"] = Password;
                    Session["QQ"] = user.QQ;
                    Session["Tel"] = user.Tel;
                    Session["Introduce"] = user.Introduce;
                    Session["coockie"] = 0;

                    //是否记住
                    if (Checkbox != null)
                    {
                        if (Checkbox.Equals("on"))
                        {
                            //保存cookie
                            Session["coockie"] = 1;
                            cookie["UId"] = user.ID.ToString();
                            cookie["Password"] = Password;
                            cookie["UserName"] = HttpUtility.UrlEncode(UserName);
                            cookie.Expires = System.DateTime.Now.AddDays(7.0);
                            Response.Cookies.Add(cookie);
                        }
                    }


                    //记录表和用户记录表添加记录
                    AddRecord(user, Href, Href, 1, 1);

                }
            }

            

            
            
        }

        //注册功能
        public void register(FormCollection col)
        {
            string username = col["UserName"];
            string password = col["Password"];
            string passwordagain = col["Passwordagain"];
            string sex = col["Sex"];
            string age = col["Age"];
            string qq = col["QQ"];
            string tel = col["Tel"];
            string introduce = col["Introduce"];
           
            if (password.Equals(passwordagain))
            {
                User existuser = new User();
                existuser = entity.User.Where(p => p.Name == username).FirstOrDefault();
                if (existuser != null) Response.Write(JsHelper.Messagebox("注册失败！用户名已经存在！", "Home", "Index"));
                else 
                {
                    existuser = entity.User.Where(p => p.QQ == qq).FirstOrDefault();
                    if (existuser != null) Response.Write(JsHelper.Messagebox("注册失败！该QQ号已经绑定了！", "Home", "Index"));
                    else
                    {
                        existuser = entity.User.Where(p => p.Tel == tel).FirstOrDefault();
                        if (existuser != null) Response.Write(JsHelper.Messagebox("注册失败！该电话号码已经绑定了！", "Home", "Index"));
                        else
                        {
                            User user = new User();
                            user.Name = username;
                            user.Password = password;
                            user.Sex = sex;
                            user.Age = Convert.ToInt16(age);
                            user.QQ = qq;
                            user.Tel = tel;
                            user.Introduce = introduce;
                            user.CreationDate = DateTime.Now;
                            entity.User.Add(user);
                            int i = entity.SaveChanges();
                            if (i != 0)
                            {
                                Session["Login"] = 1;
                                Session["UId"] = user.ID;
                                Session["UserName"] = username;
                                Session["Password"] = password;
                                Session["QQ"] = user.QQ;
                                Session["Tel"] = user.Tel;
                                Session["Introduce"] = user.Introduce;
                                Session["coockie"] = 0;

                                //记录表和用户记录表添加记录
                                AddRecord(user, "", "", 0, 0);

                                Response.Write(JsHelper.Messagebox("注册成功！", "Home", "Index"));
                            }
                            else
                                Response.Write(JsHelper.Messagebox("注册失败！", "Home", "Index"));
                        }
                    }
                }
                
                
            }
            else
            {
                Response.Write(JsHelper.Messagebox("两次密码不一致，注册失败！", "Home", "Index"));
            }

        }

        //总训练页面功能
        public ActionResult Practice()
        {
            SetSessionAndCookie();
            List<ProblemType> pt = entity.ProblemType.OrderBy(p => p.ID).ToList();
            return View(pt);
        }

        //各个训练页面功能
        public ActionResult Classify(string id,int page)
        {
            SetSessionAndCookie();
            if (!string.IsNullOrEmpty(id)) 
            {
                Session["classifyid"] = id;
                int i = Convert.ToInt16(id);
                List<Problem> pro = entity.Problem.Where(p => p.TypeID == i).OrderBy(p => p.ID).ToList();
                int count = pro.Count();
                i = count / 2;
                if ((i * 2) < count)
                    i++;
                if (page == i)
                {
                    pro = pro.Skip(page * 2 - 2).ToList();
                }
                else
                {
                    pro = pro.Skip(page * 2 - 2).Take(2).ToList();
                }
                
                var tupleList = new Tuple<List<Problem>, int, int>(pro, i, page);
                return View(tupleList);
            }
            
            return View("Practice");
        }
        
        //问题内容页面功能
        public ActionResult Problem(string id,int page=1)
        {
            SetSessionAndCookie();
            if (!string.IsNullOrEmpty(id))
            {
                Session["ProblemId"] = id;
                int i = Convert.ToInt16(id);
                Problem pro = entity.Problem.Where(p => p.ID == i).FirstOrDefault();

                int login = Convert.ToInt32(Session["Login"]);
                List<UserProblem> uplist = new List<UserProblem>();
                int count = 0;
                int ii = 0;

                if (login != 0)
                {
                    int uid = Convert.ToInt32(Session["UId"]);
                    uplist = entity.UserProblem.Where(p => p.UserID == uid).OrderByDescending(p=>p.SubmitDate) .ToList();

                    count = uplist.Count();
                    ii = count / 3;
                    if ((ii * 3) < count)
                    {
                        ii++;
                    }
                    if (page == ii)
                    {
                        uplist = uplist.Skip(page * 3 - 3).ToList();
                    }
                    else
                    {
                        uplist = uplist.Skip(page * 3 - 3).Take(3).ToList();
                    }
                    Session["ResultPage"] = page;

                }

                var tupleList = new Tuple<Problem, List<UserProblem>, int, int>(pro, uplist, ii, page);
                return View(tupleList);
            }

            return View("Practice");
        }

        //提交功能
        [MyFilter]
        [ValidateInput(false)]
        public void AnswerPro(FormCollection col)
        {
            SetSessionAndCookie();
            string mycode = col["Code"];
            string id = col["id"];
            Session["Evaluate"] = col["Eva"];

            if ((!string.IsNullOrEmpty(id)) && (Session["Evaluate"] != null))
            {
                
                short evaluate = Convert.ToInt16(Session["Evaluate"]);
                
                
                int i = Convert.ToInt16(id);
                Problem pro = entity.Problem.Where(p => p.ID == i).FirstOrDefault();
                if (pro != null)
                {
                    int userid = Convert.ToInt16(Session["UId"]);
                    int resultid = Judge(mycode, pro.ID, userid);

                    //修改问题表回答人数
                    UserProblem updateup = entity.UserProblem.Where(p => p.ProblemID == pro.ID && p.UserID == userid).FirstOrDefault();
                    if (updateup == null)
                    {
                        pro.Answers++;
                    }
                    pro.Answers = pro.Answers;
                    entity.Entry<Problem>(pro).State = System.Data.EntityState.Modified;
                    int sc = entity.SaveChanges();

                    //修改记录表答题量
                    int arp = AddRecordPro();

                    //修改用户表回答问题数
                    int uupn = UpdateUserProNumber(pro.ID, userid, resultid);

                    //插入用户问题表记录
                    UserProblem UserPro = new UserProblem();
                    UserPro.UserID = userid;
                    UserPro.ProblemID = i;
                    UserPro.ResultID = resultid;
                    UserPro.Runtime = Convert.ToInt32(AvgRunTime);
                    UserPro.Memory = pro.MemLimit;
                    UserPro.Language = "C/C++";
                    UserPro.Code = mycode;
                    UserPro.Evaluate = evaluate;
                    UserPro.SubmitDate = DateTime.Now;
                    entity.UserProblem.Add(UserPro);
                    int up = entity.SaveChanges();

                    //修改结果表中某种结果类型问题数
                    Result result = entity.Result.Where(p => p.ID == resultid).FirstOrDefault();
                    result.ProNumber++;
                    entity.Entry<Result>(result).State = System.Data.EntityState.Modified;
                    int res = entity.SaveChanges();

                    //重新统计评价
                    List<UserProblem> UPList = entity.UserProblem.Where(p => p.ProblemID == i).ToList();
                    double ProEva = 0;
                    foreach (var item in UPList)
                    {
                        ProEva += item.Evaluate;
                    }
                    ProEva /= UPList.Count();
                    if (ProEva < 5) ProEva += 1;
                    pro.Evaluate = (short)ProEva;
                    entity.Entry<Problem>(pro).State = System.Data.EntityState.Modified;
                    int eva = entity.SaveChanges();


                    if ((sc != 0) && (arp != 0) && (uupn != 0) && (up != 0) && (res != 0) && (eva != 0))
                        Response.Write(JsHelper.Messagebox("提交成功！", "Problem?id=" + id));
                    Response.Write(JsHelper.Messagebox("提交不成功！", "Problem?id=" + id));
                }
                Response.Write(JsHelper.Messagebox("提交不成功！", "Problem?id=" + id));
            }

            Response.Write(JsHelper.Messagebox("提交不成功！", "Problem?id=" + id));
        }

        //关于页面
        public ActionResult About()
        {
            SetSessionAndCookie();
            return View();
        }


        //搜索功能
        public void Search(string mysearch)
        {
            SetSessionAndCookie();
            string regex = @"^-?\d+\.?\d*$";
            bool result = Regex.IsMatch(mysearch, regex);
            if (result)
            {
                int id = Convert.ToInt32(mysearch);
                Problem pro = entity.Problem.Where(p => p.ID == id).FirstOrDefault();
                if (pro != null)
                {
                    Response.Write(JsHelper.Jump("Home", "Problem?id=" + id));
                }
                else
                {
                    Response.Write(JsHelper.mygoback("没有此题"));
                }
                
            }
            else
            {
                Problem mypro = null;
                List<Problem> prolist = entity.Problem.OrderBy(p => p.ID).ToList();
                foreach (Problem p in prolist)
                {
                    if (p.Name.Equals(mysearch))
                    {
                        mypro = p;                 
                        break;
                    }
                    else if (p.Describe.Contains(mysearch))
                    {
                        mypro = p;
                        break;
                    }
                }
                if (mypro == null)
                {
                    Response.Write(JsHelper.mygoback("没有此题"));
                }
                else
                {
                    Response.Write(JsHelper.Jump("Home", "Problem?id=" + mypro.ID));
                }
            }
            //Response.Write(JsHelper.Jump("Home", "Index"));
        }

        //修改用户信息
        public void UpdateUser(FormCollection col)
        {
            string username = col["UserName"];
            string password = col["Password"];
            string passwordagain = col["Passwordagain"];
            string sex = col["Sex"];
            string age = col["Age"];
            string qq = col["QQ"];
            string tel = col["Tel"];
            string introduce = col["Introduce"];

            int UserID = Convert.ToInt16(Session["UId"]);
            User myuser = entity.User.Where(p => p.ID == UserID).First();

            //Response.Write(JsHelper.Messagebox("11"));

            //判断两次新密码否一致
            if ( password != "" && passwordagain != "" && password.Equals(passwordagain))
            {
                //Response.Write(JsHelper.Messagebox("22"));

                User existuser = new User();
                existuser = entity.User.Where(p => p.QQ == qq).FirstOrDefault();
                //判断QQ号是否已经绑定
                if (existuser != null)
                {
                    if(existuser.ID != myuser.ID)
                    {
                        Response.Write(JsHelper.Messagebox("修改失败！该QQ号已经绑定了！", "Home", "Index"));
                    }
                }                   

                existuser = entity.User.Where(p => p.Tel == tel).FirstOrDefault();
                //判断电话号码号是否已经绑定
                if (existuser != null)
                {
                    if (existuser.ID != myuser.ID)
                    {
                        Response.Write(JsHelper.Messagebox("修改失败！该电话号码已经绑定了！", "Home", "Index"));
                    }
                }    

                myuser.Password = password;
                myuser.Sex = sex;
                myuser.Age = Convert.ToInt16(age);
                myuser.QQ = qq;
                myuser.Tel = tel;
                myuser.Introduce = introduce;
                myuser.CreationDate = DateTime.Now;
                
                int i = entity.SaveChanges();
                if (i != 0)
                {
                    Session["Login"] = 1;
                    Session["UId"] = myuser.ID;
                    Session["UserName"] = myuser.Name;
                    Session["Password"] = myuser.Password;
                    Session["QQ"] = myuser.QQ;
                    Session["Tel"] = myuser.Tel;
                    Session["Introduce"] = myuser.Introduce;
                    Session["coockie"] = 0;
                    Response.Write(JsHelper.Messagebox("修改成功！", "Home", "Index"));
                }
                else
                    Response.Write(JsHelper.Messagebox("修改失败！", "Home", "Index"));
                
              
            }
            else
            {
                Response.Write(JsHelper.Messagebox("两次新密码不一致，修改失败！", "Home", "Index"));
            }

        }

        //退出功能
        public void goback()
        {
            if (Convert.ToInt16(Session["coockie"]) != 0)
            {
                HttpCookie hc = Request.Cookies["AUFElogin"];
                hc.Expires = DateTime.Now.AddDays(-1);
                Response.Cookies.Add(hc);
            }
            Session.Abandon();
            Response.Write(JsHelper.Messagebox("退出成功！","Home","Index"));
        }

        //忘记密码功能
        public void forget(string forgetemail)
        {
            string email = forgetemail;
            User user = entity.User.Where(p => p.QQ == email).FirstOrDefault();
            if (user != null)
            {
                SendM(email);
                Session["email"] = email;
                Response.Write(JsHelper.Messagebox("邮件发送成功！", "Home", "Index"));

                
            }
            else
                Response.Write(JsHelper.Messagebox("邮箱不存在！", "Home", "Index"));
            
        }

        //使用邮件修改密码功能
        public void forgetemail()
        {
            string s = Session["email"].ToString();
            User user = entity.User.Where(p => p.QQ == s).FirstOrDefault();
            user.Password = "12345678";
            entity.Entry<User>(user).State = System.Data.EntityState.Modified;
            entity.SaveChanges();
            Response.Write(JsHelper.Messagebox("密码修改成功！", "Home", "Index"));
        }

        public ActionResult Gobang()
        {
            return View();
        }

        public ActionResult Pinball()
        {
            return View();
        }

        public ActionResult Snake()
        {
            return View();
        }
        

        //显示验证码
        public ActionResult SecurityCode()
        {
            string oldcode = TempData["SecurityCode"] as string;
            string code = JsHelper.CreateRandomCode(4); //验证码的字符为4个
            TempData["SecurityCode"] = code; //验证码存放在TempData中
            return File(JsHelper.CreateValidateGraphic(code), "image/Jpeg");
        }


        //发送邮件
        public void SendM(string ss)
        {
            string emails = ss;
            string from="464407764@qq.com";
            string Title = "AUFEOJ修改密码";
            string password="lyqmrklrlltxcajd";
            string body = "AUFEOJ修改密码链接(复制粘贴):<label>http://localhost:12683/Home/forgetemail</label><br/>密码被设置为12345678";

            var sendMail = new SendMail(emails,from,body,Title,password);
            //sendMail.Attachments(filename);
            sendMail.Send();
        }

       
        //登录时：记录表和用户记录表添加记录
        /*
         * Href1: 成功时跳转位置
         * Href2: 失败时跳转位置
         * flag1: 成功时是否需要跳转
         * flag2: 失败时是否需要跳转
         */
        public void AddRecord(User user ,string Href1,string Href2,int flag1,int flag2)
        {
            List<Record> records = entity.Record.OrderBy(p => p.TodayDate).ToList();
            int flag = 0;
            Record record = new Record();
            foreach (var item in records)
            {
                if (item.TodayDate.ToShortDateString() == DateTime.Now.ToShortDateString())
                {
                    flag = 1;
                    record = item;

                    break;
                }
            }
            //Record表中有今天的记录
            if (flag == 1)
            {
                record.LoginNumber++;
                entity.Entry<Record>(record).State = System.Data.EntityState.Modified;
                int i = entity.SaveChanges();
                if (i != 0)
                {
                    //插入用户记录表RecordUser表
                    AddRecordUser(user, record, Href1, flag1);
                }

                else
                {
                    record.LoginNumber--;
                    entity.Entry<Record>(record).State = System.Data.EntityState.Modified;
                    entity.SaveChanges();
                    if(flag2 == 1)
                        Response.Write(JsHelper.Messagebox("登录失败！", Href2));
                }

            }
            else
            {
                record.LoginNumber++;
                record.TodayDate = DateTime.Now;
                entity.Record.Add(record);
                int i = entity.SaveChanges();
                if (i != 0)
                {
                    //插入用户记录表RecordUser表
                    AddRecordUser(user, record, Href1, flag);
                }
                else
                {
                    record.LoginNumber--;
                    entity.Entry<Record>(record).State = System.Data.EntityState.Modified;
                    entity.SaveChanges();
                    if(flag2 == 1)
                        Response.Write(JsHelper.Messagebox("登录失败！", Href2));
                }
            }

        }


        //登录时：用户记录表添加记录
        public void AddRecordUser(User user ,Record record,string Href,int flag)
        {
            RecordUser recorduser = new RecordUser();
            recorduser.RecordID = record.ID;
            recorduser.UserID = user.ID;
            recorduser.LoginTime = DateTime.Now;
            entity.RecordUser.Add(recorduser);
            entity.SaveChanges();
            if(flag == 1)
                Response.Write(JsHelper.Messagebox("登录成功！", Href));
        }

        //回答问题时：记录表添加记录
        //返回int（1：成功，0：不成功）
        public int AddRecordPro()
        {
            int result = 0;

            List<Record> records = entity.Record.OrderBy(p => p.TodayDate).ToList();
            int flag = 0;
            Record record = new Record();
            foreach (var item in records)
            {
                if (item.TodayDate.ToShortDateString() == DateTime.Now.ToShortDateString())
                {
                    flag = 1;
                    record = item;

                    break;
                }
            }
            if (flag == 1)
            {
                record.AllAnswerPro++;
                entity.Entry<Record>(record).State = System.Data.EntityState.Modified;
                int i = entity.SaveChanges();
                if (i == 0)
                {
                    record.AllAnswerPro--;
                    entity.Entry<Record>(record).State = System.Data.EntityState.Modified;
                    entity.SaveChanges();
                    return result;
                }
                result = 1;
            } 
            return result;
           
        }

        //修改用户表回答问题数
        //返回int（1：成功，0：不成功）
        public int UpdateUserProNumber(int proid,int userid,int resultid)
        {
            int  result = 0;
            User user = entity.User.Where(p => p.ID == userid).FirstOrDefault();
            UserProblem updateup = entity.UserProblem.Where(p => p.ProblemID == proid && p.UserID == userid).FirstOrDefault();
            if (updateup == null)
            {
                user.Problems++;
                if (resultid == 2)
                {
                    user.Accepteds++;
                }
            }
            user.Problems = user.Problems;

            entity.Entry<User>(user).State = System.Data.EntityState.Modified;
            int i = entity.SaveChanges();
            if (i == 0)
            {
                user.Problems--;
                entity.Entry<User>(user).State = System.Data.EntityState.Modified;
                entity.SaveChanges();
                return result;
            }
            result = 1;
            return result;
        }

        //判题处理
        /***************************************/
        //创建文件
        public void CreateFile(string filepath ,string filename,string content)
        {
            string fn = filepath + filename;
            if (!System.IO.File.Exists(fn))
            {
                FileStream fs1 = new FileStream(fn, FileMode.Create, FileAccess.Write);//创建写入文件 
                StreamWriter sw = new StreamWriter(fs1);
                sw.WriteLine(content);//开始写入值
                sw.Close();
                fs1.Close();
            }
            else
            {
                FileStream fs = new FileStream(fn, FileMode.Open, FileAccess.Write);
                StreamWriter sr = new StreamWriter(fs);
                sr.WriteLine(content);//开始写入值
                sr.Close();
                fs.Close();
            }
        }

        //删除文件
        public void DeleteFile(string filepath,string filename)
        {
            string fn = filepath + filename;
            if (System.IO.File.Exists(fn))
            {
                System.IO.File.Delete(fn);
            }
        }

        //判提初始化
        public void JudgeInit(int proid)
        {
            List<TestData> testdatalist = entity.TestData.Where(p => p.ProblemID == proid).ToList();
            TestNumber = testdatalist.Count();
            Test = new string[TestNumber];
            for (int i = 0; i < TestNumber; i++)
            {
                Test[i] = testdatalist[i].Data.Trim();
            }

            TrueOut = new string[TestNumber];
            for (int i = 0; i < TestNumber; i++)
            {
                TrueOut[i] = testdatalist[i].Result.Trim();
            }

            RunOut = new string[TestNumber];


        }

        //编译程序
        private void Compile()
        {
            string fn = filepath + filename;
            process.StartInfo.FileName = @"E:\Dev-Cpp\MinGW64\bin\g++.exe";
            process.StartInfo.Arguments = fn + ".cpp -o " + fn + ".exe";
            process.StartInfo.RedirectStandardError = true;
            process.StartInfo.RedirectStandardInput = true;
            process.StartInfo.RedirectStandardOutput = false;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.CreateNoWindow = true;
            process.Start();

            process.WaitForExit();

        }


        //运行程序
        private void Run(Object obj)
        {
            string fn = filepath + filename;
            process.StartInfo.FileName = fn + ".exe";
            process.StartInfo.RedirectStandardInput = true;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = false;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.CreateNoWindow = true;
            process.Start();
            process.StandardInput.Write(Test[Convert.ToInt32(obj)]);
            process.StandardInput.Close();
            process.WaitForExit(2000);
        }

        //判题
        public int Judge(string code,int proid,int userid)
        {
            int result = 1;

            //判定初始化
            JudgeInit(proid);
            List<UserProblem> uplist = entity.UserProblem.Where(p => p.ProblemID == proid && p.UserID == userid).ToList();

            int upnumber = uplist.Count;


            //创建文件
            filepath = Server.MapPath("~/Temp/");
            filename = "" + userid + "_" + proid + "_" + upnumber + "";
            CreateFile(filepath, filename + ".cpp", code);

            //创建线程
            thread = new Thread(new ThreadStart(Compile));

            //运行线程
            thread.Start();

            //等待线程结束
            while (thread.IsAlive) ;

            CompileError = process.StandardError.ReadToEnd().Trim();

            process.Close();
            thread.Abort();

            if (CompileError.Length > 0)
            {
                result = 8;
                DeleteFile(filepath, filename + ".cpp");
            }
            else
            {
                DeleteFile(filepath, filename + ".cpp");
                RunState = 1;
                //编译成功执行
                //开始执行程序
                for (int i = 0; i < TestNumber; i++)
                {
                    RunTime = DateTime.Now;
                    thread = new Thread(new ParameterizedThreadStart(Run));
                    thread.Start(i);
                    RunTime = RunTime.AddSeconds(2);

                    //判断是否超时
                    while (thread.IsAlive)
                    {
                        if (RunTime < DateTime.Now)
                        {
                            RunState = 2;
                            result = 5;
                            process.Close();
                            break;
                        }
                    }

                    thread.Abort();


                    if (RunState == 1)
                    {
                        AvgRunTime += process.TotalProcessorTime.TotalMilliseconds;
                        RunOut[i] = process.StandardOutput.ReadToEnd().Trim();
                        process.Close();
                    }
                    process.Close();

                }


                if (RunState == 1)
                {
                    for (int i = 0; i < TestNumber; i++)
                    {
                        if (!TrueOut[i].Equals(RunOut[i]))
                        {
                            RunState = 0;
                            result = 3;
                            break;
                        }
                    }
                }


                if (RunState == 1)
                {
                    result = 2;
                    AvgRunTime = AvgRunTime / TestNumber;
                }

            }


            thread.Abort();

            if (RunState == 1)
            {
                
                DeleteFile(filepath, filename + ".exe");
            }


            
            return result;
        }
        /************************************/

        //设置Session和Cookie
        public void SetSessionAndCookie()
        {
            Session["Login"] = 0;
            //判断Session或Cookie中是否存在
            if ((Session["UId"] != null && Session["UserName"] != null && Session["Password"] != null) || (Request.Cookies["AUFElogin"] != null))
            {

                HttpCookie hc = Request.Cookies["AUFElogin"];
                //判断Session中是否保存Cookie存在
                if ((Session["coockie"] != null) && Convert.ToInt16(Session["coockie"]) != 0)
                {
                    int UId = Convert.ToInt32(hc.Values["UId"]);
                    User user = entity.User.Where(p => p.ID == UId).First();
                    //由于保存的cookie乱码则加密解密
                    if (user.Name.Equals(HttpUtility.UrlDecode(hc.Values["UserName"])) && user.Password.Equals(hc.Values["Password"]))
                    {
                        Session["coockie"] = 1;
                        Session["Login"] = 1;
                        Session["UId"] = user.ID;
                        Session["UserName"] = user.Name;
                        Session["Password"] = user.Password;
                        Session["QQ"] = user.QQ;
                        Session["Tel"] = user.Tel;
                        Session["Introduce"] = user.Introduce;

                    }
                }
                else
                {
                    //Session存在
                    if (Session["UId"] != null)
                    {
                        int UId = Convert.ToInt32(Session["UId"]);
                        User user = entity.User.Where(p => p.ID == UId).FirstOrDefault();

                        if (user.Name.Equals(Session["UserName"]) && user.Password.Equals(Session["Password"]))
                        {
                            Session["Login"] = 1;
                        }
                    }
                    //Cookie存在
                    else
                    {
                        int UId = Convert.ToInt32(hc.Values["UId"]);
                        User user = entity.User.Where(p => p.ID == UId).First();
                        //由于保存的cookie乱码则加密解密
                        if (user.Name.Equals(HttpUtility.UrlDecode(hc.Values["UserName"])) && user.Password.Equals(hc.Values["Password"]))
                        {
                            Session["Login"] = 1;
                            Session["coockie"] = 1;
                            Session["UId"] = user.ID;
                            Session["UserName"] = user.Name;
                            Session["Password"] = user.Password;
                            Session["QQ"] = user.QQ;
                            Session["Tel"] = user.Tel;
                            Session["Introduce"] = user.Introduce;


                        }
                    }



                }

            }
            else
            {
                Session["UId"] = null;
            }
        }

        //没有登录先登录
        public void PleaseLogin(string id)
        {
            Response.Write(JsHelper.Messagebox("请先登录后再试！", "Home", "Problem?id=" + id));
        }


        //竞赛模块
        public ActionResult OnlineCon(int page=1)
        {
            SetSessionAndCookie();
            List<OnlineContest> ocl = entity.OnlineContest.OrderByDescending(p => p.EndTime).ToList();
            foreach (OnlineContest oc in ocl)
            {
                if (DateTime.Compare(oc.EndTime, DateTime.Now) <= 0)
                {
                    oc.CompetitionState = 0;
                    entity.SaveChanges();
                }
                else if (DateTime.Compare(oc.StartTime, DateTime.Now) > 0)
                {
                    oc.CompetitionState = 2;
                    entity.SaveChanges();
                }
                else
                {
                    oc.CompetitionState = 1;
                    entity.SaveChanges();
                }
            }
            int count = ocl.Count();
            int i = count / 3;
            if ((i * 3) < count)
            {
                i++;
            }
            if (page == i)
            {
                ocl = ocl.Skip(page * 3 - 3).ToList();
            }
            else
            {
                ocl = ocl.Skip(page * 3 - 3).Take(3).ToList();
            }
            Session["OCPage"] = page;
            var tupleList = new Tuple<List<OnlineContest>, int, int>(ocl,i, page);
            return View(tupleList);
        }

        //竞赛登录模块
        public void OCLogin(int page,int OCid)
        {
            if (Convert.ToInt32(Session["Login"]) == 1)
            {
                Response.Write(JsHelper.Jump("Home", "OnlineCP?id=" + OCid));
            }
            else
                Response.Write(JsHelper.Messagebox("请先登录！", "OnlineCon?page=" + page));
        }

        //竞赛问题列表模块
        public ActionResult OnlineCP(int? id)
        {
            SetSessionAndCookie();
            if (id != null)
            { 
                OnlineContest oc = entity.OnlineContest.Where(p => p.ID == id).FirstOrDefault();
                Session["OCState"] = oc.CompetitionState;
                Session["OnlineContestid"] = oc.ID;
                Session["OnlineContest"] = oc.Name;
                List<OCProblem> ocplist = entity.OCProblem.Where(p => p.OCID == id).ToList();
                List<Problem> plist = new List<Problem>();
                foreach (OCProblem item in ocplist)
                {
                    Problem pro = entity.Problem.Where(p => p.ID == item.ProblemID).FirstOrDefault();
                    plist.Add(pro);
                }
                return View(plist);
            }
            else
                return View("Error");
        }

        //竞赛问题题目展示模块
        public ActionResult OnlineCPShow(int id)
        {
            SetSessionAndCookie();
            Session["ProblemId"] = id;
            int i = Convert.ToInt16(id);
            Problem pro = entity.Problem.Where(p => p.ID == i).FirstOrDefault();

            return View(pro);
            
        }

        //排名模块
        public ActionResult RankList(int page=1)
        {
            SetSessionAndCookie();
            List<User> userlist = entity.User.OrderByDescending(p => p.Accepteds).OrderByDescending(p => p.Problems).ToList();

            int count = userlist.Count();
            int i = count / 3;
            if ((i * 3) < count)
            {
                i++;
            }
            if (page == i)
            {
                userlist = userlist.Skip(page * 3 - 3).ToList();
            }
            else
            {
                userlist = userlist.Skip(page * 3 - 3).Take(3).ToList();
            }
            Session["RLPage"] = page;
            var tupleList = new Tuple<List<User>, int, int>(userlist, i, page);
            return View(tupleList);
        }

        //讨论列表模块
        public ActionResult DiscussList(int page = 1)
        {
            SetSessionAndCookie();
            List<Discuss> discusslist = entity.Discuss.OrderByDescending(p => p.QuestionTime).ToList();

            int count = discusslist.Count();
            int i = count / 3;
            if ((i * 3) < count)
            {
                i++;
            }
            if (page == i)
            {
                discusslist = discusslist.Skip(page * 3 - 3).ToList();
            }
            else
            {
                discusslist = discusslist.Skip(page * 3 - 3).Take(3).ToList();
            }
            Session["DLPage"] = page;
            var tupleList = new Tuple<List<Discuss>, int, int>(discusslist, i, page);
            return View(tupleList);
        }

        //回复列表模块
        public ActionResult AnswerList(int id,int page=1)
        {
            SetSessionAndCookie();
            Session["DiscussID"] = id;
            List<Answer> answerlist = entity.Answer.Where(p => p.DiscussID == id).OrderByDescending(p=>p.AnswerTime).ToList();
            Discuss discuss = entity.Discuss.Where(p => p.ID == id).FirstOrDefault();

            int count = answerlist.Count();
            int i = count / 3;
            if ((i * 3) < count)
            {
                i++;
            }
            if (page == i)
            {
                answerlist = answerlist.Skip(page * 3 - 3).ToList();
            }
            else
            {
                answerlist = answerlist.Skip(page * 3 - 3).Take(3).ToList();
            }
            Session["AnswerListPage"] = page;
            List<int> listint = new List<int>();
            listint.Add(i);
            listint.Add(page);
            listint.Add(count);
            var tupleList = new Tuple<List<Answer>, Discuss, List<int>>(answerlist, discuss, listint);

            return View(tupleList);
        }

        //回复处理模块
        public void AnswerDiscuss(FormCollection col)
        {
            SetSessionAndCookie();
            string discussid = col["Discussid"];
            string answerid = col["Answerid"];
            string answerdis = col["Answer"];
            Answer answer = new Answer();
            answer.UserID = Convert.ToInt32(answerid);
            answer.DiscussID = Convert.ToInt32(discussid);
            answer.AnswerTime = DateTime.Now;
            answer.Content = answerdis;
            entity.Answer.Add(answer);
            entity.SaveChanges();

            int disid = Convert.ToInt32(Session["DiscussID"]);
            int page = Convert.ToInt32(Session["AnswerListPage"]);
            Response.Write(JsHelper.Jump("Home", "AnswerList?id=" + disid + "&page=" + page));
        }

        //添加问题讨论模块
        public void DisucssSubmit(FormCollection col)
        {
            SetSessionAndCookie();
            int id = Convert.ToInt32(Session["UId"]);
            string title = col["title"];
            string describe = col["describe"];

            Discuss discuss = new Discuss();
            discuss.UserID = id;
            discuss.QuestionTime = DateTime.Now;
            discuss.Title = title;
            discuss.ProblemDescription = describe;
            entity.Discuss.Add(discuss);
            entity.SaveChanges();

            Session["DiscussID"] = discuss.ID;
            int disid = discuss.ID;
            Response.Write(JsHelper.Jump("Home", "AnswerList?id=" + disid));

            
        }

        //状态列表模块
        public ActionResult StateList(int page=1)
        {
            SetSessionAndCookie();

            List<UserProblem> userprolist = entity.UserProblem.OrderByDescending(p => p.SubmitDate).ToList();

            int count = userprolist.Count();
            int i = count / 3;
            if ((i * 3) < count)
            {
                i++;
            }
            if (page == i)
            {
                userprolist = userprolist.Skip(page * 3 - 3).ToList();
            }
            else
            {
                userprolist = userprolist.Skip(page * 3 - 3).Take(3).ToList();
            }
            Session["AnswerListPage"] = page;

            var tupleList = new Tuple<List<UserProblem>, int, int>(userprolist, i, page);
            return View(tupleList);
        }

        //运行结果模块
        public ActionResult Result(int id)
        {
            SetSessionAndCookie();
            UserProblem up = entity.UserProblem.Where(p => p.ID == id).FirstOrDefault();

            return View(up);
        }

    }
}
