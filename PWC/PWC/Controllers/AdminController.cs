using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using PWC.Models;
using PWC.Common;
using PWC.Filter;

namespace PWC.Controllers
{
    public class AdminController : Controller
    {
        //
        // GET: /Admin/
        PWCEntities entity = new PWCEntities();

        [MyFilter]
        public ActionResult Index(FormCollection col)
        {
            int adminid = Convert.ToInt32(Session["adminid"]);
            Admin admin = entity.Admin.Where(p => p.ID == adminid).FirstOrDefault();
            return View(admin);
        }
        
        //登录视图
        public ActionResult Login()
        {
            return View();
        }

        //登录控制
        public void LoginCheck(FormCollection col)
        {
            string name = col["Username"];
            string password = col["Password"];

            if ((name != null) && (password != null))
            {

                Admin admin = entity.Admin.Where(p => p.Name == name && p.Password == password).FirstOrDefault();
                if (admin != null)
                {

                    Session["adminid"] = admin.ID;
                    Session["adminname"] = admin.Name;
                    Response.Write(JsHelper.Messagebox("登录成功！", "Admin", "Index"));
                }

            }

            Response.Write(JsHelper.Messagebox("登录失败！", "Admin", "Login"));
        }

        //管理员退出系统
        public void LoginOut()
        {
            Session.Clear();
            Response.Write(JsHelper.Messagebox("退出成功！", "Admin", "Login"));
        }

        //管理员修改密码
        public ActionResult Update(string password)
        {
            int id = Convert.ToInt16(Session["adminid"]);
            Admin admin = entity.Admin.Where(p => p.ID == id).FirstOrDefault();
            if(admin!=null)
            {
                admin.Password = password;
                entity.Entry<Admin>(admin).State = System.Data.EntityState.Modified;
                entity.SaveChanges();
                
                return Content("1");
            }

            return Content("0");
        }

        //管理员信息列表
        public JsonResult GetAdminList()
        {
            List<Admin> list = entity.Admin.OrderBy(p => p.ID).ToList();
            var json = new
            {
                total = list.Count,
                rows = (from r in list
                        select new Admin()
                        {
                            Name = r.Name,
                            Age = r.Age,
                            Sex = r.Sex,
                            QQ= r.QQ,
                            Tel = r.Tel
                        }).ToArray()
            };
            return Json(json, JsonRequestBehavior.AllowGet);
        }

        //用户信息列表
        public JsonResult GetUserList()
        {
            List<User> list = entity.User.OrderBy(p => p.ID).ToList();
            var json = new
            {
                total = list.Count,
                rows = (from r in list
                        select new User()
                        {
                            Name = r.Name,
                            Age = r.Age,
                            Sex = r.Sex,
                            QQ= r.QQ,
                            Tel = r.Tel,
                            Introduce = r.Introduce,
                            Problems = r.Problems
                        }).ToArray()
            };
            return Json(json, JsonRequestBehavior.AllowGet);
        }

        //题目信息列表
        public JsonResult GetProblemList()
        {
            List<Problem> list = entity.Problem.OrderBy(p => p.ID).ToList();
            var json = new
            {
                total = list.Count,
                rows = (from r in list
                          select new 
                          {
                              Name = r.Name,
                              TypeName = r.ProblemType.Type,
                              TimeLimit = r.TimeLimit,
                              MemLimit = r.MemLimit,
                              Difficulty = r.Difficulty,
                              Describe = r.Describe,
                              Input = r.Input,
                              Output = r.Output,
                              SampleInput = r.SampleInput,
                              SampleOutput = r.SampleOutput,
                              Source = r.Source,
                              Evaluate = r.Evaluate,
                              CreationDate = r.CreationDate.ToString(),
                              AdminName = r.Admin.Name,
                              Answers = r.Answers
                          }).ToArray()                  
            };
            return Json(json, JsonRequestBehavior.AllowGet);
        }

        //比赛信息列表
        public JsonResult GetOnlineContestList()
        {
            List<OnlineContest> list = entity.OnlineContest.OrderBy(p => p.ID).ToList();
            var json = new
            {
                total = list.Count,
                rows = (from r in list
                        select new 
                        {
                            Name = r.Name,
                            CompetitionState = r.CompetitionState==0?"已经结束":"正在进行",
                            Number = r.Number,
                            StartTime = r.StartTime.ToString(),
                            EndTime = r.EndTime.ToString(),
                            Place = r.Place,
                            Describe = r.Describe,
                            AdminName = r.Admin.Name,
                            CreationDate = r.CreationDate.ToString(),
                            Problems = ProblemNames(r),
                            Buttons = "<button type='submit'href='/Admin/test'>查看题目</button>"
                        }).ToArray()
            };
            return Json(json, JsonRequestBehavior.AllowGet);
        }

        //公告信息列表
        public JsonResult GetNoticeList()
        {
            List<Notice> list = entity.Notice.OrderBy(p => p.ID).ToList();
            var json = new
            {
                total = list.Count,
                rows = (from r in list
                        select new 
                        {
                            Describe = r.Describe,
                            State = r.State==0?"过期":"正在进行",
                            AdminName = r.Admin.Name,
                            CreationDate = r.CreationDate.ToString()          
                        }).ToArray()
            };
            return Json(json, JsonRequestBehavior.AllowGet);
        }

        //返回题目编号
        public string ProblemNames(OnlineContest r)
        {
            string s = null;
            List<OCProblem> ocp = r.OCProblem.OrderBy(p => p.ID).ToList();
            foreach (var i in ocp)
            {
                s = s + i.Problem.ID + " ";
            }
            return s;
        }

        public string AddAdmin(FormCollection col)
        {

            string data = "";
            string name = col["Name"];
            string password = col["Password"];
            string sex = col["Sex"];
            string age = col["Age"];
            string qq = col["QQ"];
            string tel = col["Tel"];

            Admin admin = new Admin();
            admin.Name = name;
            admin.Password = password;
            admin.Sex = sex;
            admin.Age = Convert.ToInt32(age);
            admin.QQ = qq;
            admin.Tel = tel;
            admin.State = 1;
            admin.CreationDate = DateTime.Now;
            entity.Admin.Add(admin);
            int i = entity.SaveChanges();
            if (i != 0)
            {
                data = "添加成功！";
            }
            else
            {
                data = "添加失败！";
            }

            return data;
        }


        public string AddPro(FormCollection col)
        {
            string data = "";
            string name = col["Name"];
            int TypeName = Convert.ToInt32(col["TypeName"]);
            string TimeLimit = col["TimeLimit"];
            string MemLimit = col["MemLimit"];
            string Difficulty = col["Difficulty"];
            string Describe = col["Describe"];
            string Input = col["Input"];
            string Output = col["Output"];
            string SampleInput = col["SampleInput"];
            string SampleOutput = col["SampleOutput"];
            string Source = col["Source"];
            string CreatorID = col["CreatorID"];


            ProblemType protype = entity.ProblemType.Where(p => p.ID == TypeName).FirstOrDefault();
            protype.ProNumber++;
            entity.SaveChanges();

            Problem pro = new Problem();
            pro.Name = name;
            pro.TypeID = Convert.ToInt32(TypeName);
            pro.TimeLimit = Convert.ToInt32(TimeLimit);
            pro.MemLimit = Convert.ToInt32(MemLimit);
            pro.Difficulty = Convert.ToInt16(Difficulty);
            pro.Describe = Describe;
            pro.Input = Input;
            pro.Output = Output;
            pro.SampleInput = SampleInput;
            pro.SampleOutput = SampleOutput;
            pro.Source = Source;
            pro.CreatorID = Convert.ToInt32(CreatorID);
            pro.CreationDate = DateTime.Now;

            entity.Problem.Add(pro);
            int i = entity.SaveChanges();
            if (i != 0)
            {
                data = "添加成功！";
            }
            else
            {
                data = "添加失败！";
            }

            return data;
        }


        public JsonResult Problemlist(FormCollection col)
        {
            List<Problem> prolist = entity.Problem.OrderBy(p => p.ID).ToList();
            var json = new
            {
                total = prolist.Count,
                rows = (from r in prolist
                        select new
                        {
                            id = r.ID,
                            name = r.Name,
                            typename = r.ProblemType.Type
                        }).ToArray()
            };
            return Json(json, JsonRequestBehavior.AllowGet);
        }


        public string AddOC(FormCollection col)
        {
            string data = "";

            string[]  selectpro= col["selectpro"].ToString().Split('|');
            string Name = col["Name"];
            short CompetitionState = Convert.ToInt16(col["CompetitionState"]);
            DateTime StartTime = Convert.ToDateTime(col["StartTime"]);
            DateTime EndTime = Convert.ToDateTime(col["EndTime"]);
            string place = col["Place"];
            string Describe = col["Describe"];
            int AdminID = Convert.ToInt32(col["AdminID"]);
            DateTime CreationDate = DateTime.Now;

            OnlineContest oc = new OnlineContest();
            oc.Name = Name;
            oc.CompetitionState = CompetitionState;
            oc.StartTime = StartTime;
            oc.EndTime = EndTime;
            oc.Place = place;
            oc.Describe = Describe;
            oc.AdminID = AdminID;
            oc.CreationDate = CreationDate;
            entity.OnlineContest.Add(oc);
            entity.SaveChanges();

            int judge = 1;
            int ocid = oc.ID;
            int count = selectpro.Count();
            for (int i = 0; i < count - 1; i++)
            {
                int proid = Convert.ToInt32(selectpro[i]);
                OCProblem ocp = new OCProblem();
                ocp.OCID = ocid;
                ocp.ProblemID = proid;
                entity.OCProblem.Add(ocp);
                judge = entity.SaveChanges();
                if (judge == 0) break;
            }

            if (judge != 0)
            {
                data = "添加成功！";
            }
            else
            {
                data = "添加失败！";
            }

            return data;
        }


    }
}
