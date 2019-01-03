using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.SessionState;
using Quartz;
using MXR.QuartzZy.AutoCode;
using System.IO;

namespace MXR.QuartzZy
{
    public class Global : System.Web.HttpApplication
    {
        void Application_Start(object sender, EventArgs e)
        {
            //启动任务
            SwitchInterface.Execute();
        }

    }
}