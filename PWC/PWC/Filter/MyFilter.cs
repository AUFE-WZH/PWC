using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;


using System.Web.Mvc;
using PWC.Common;

namespace PWC.Filter
{
    public class MyFilter : ActionFilterAttribute, IAuthorizationFilter
    {
        public void OnAuthorization(AuthorizationContext filterContext)
        {
            //获取当前url
            string url = HttpContext.Current.Request.Path;
            //如果是回答问题
            if (url.Equals("/Home/AnswerPro"))
            {
                //判断session是否为空
                if (HttpContext.Current.Session["UId"] == null)
                {
                    string id = HttpContext.Current.Session["ProblemId"].ToString();
                    //为空就跳转到Index去
                    filterContext.Result = new RedirectResult("PleaseLogin?id=" + id);
                }
            }

            //判断是否进入管理员界面
            //string con = url.Substring(0, 6);
            //if (con.Equals("/Admin"))
            //{
            //    if (HttpContext.Current.Session["AId"] == null)
            //    {
            //        filterContext.Result = new RedirectResult("Login");
            //    }
            //}
            
            
        }


    }
}