using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.IO;
namespace MXR.QuartzZy.AutoCode
{
    public class LogHelper
    {
        public static string strLog = string.Empty;

        public static void AddLog(string JobName, string Message)
        {
            lock (strLog)
            {
                string content = string.Format("定时任务{0}日志，时间：{1}；内容：{2}\r\n", JobName, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), Message);
                string dir = System.Web.Configuration.WebConfigurationManager.AppSettings["LogDir"] + "/Log/" + DateTime.Now.ToString("yyyy/MM/dd") + "/";
                if (!Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }
                string path = dir + DateTime.Now.ToString("yyyy-MM-dd-HH") + ".txt";
                try
                {
                    File.AppendAllText(path, content);
                }
                catch (Exception ex)
                {

                }
            }
        }

        public static void AddError(string JobName, string Message)
        {
            string content = string.Format("定时任务{0}异常，时间：{1}；内容：{2}\r\n", JobName, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), Message);
            string dir = System.Web.Configuration.WebConfigurationManager.AppSettings["LogDir"] + "/Error/" + DateTime.Now.ToString("yyyy/MM/dd") + "/";
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }
            string path = dir + DateTime.Now.ToString("yyyy-MM-dd-HH") + ".txt";
            lock (strLog)
            {
                try
                {
                    File.AppendAllText(path, content);
                }
                catch
                {

                }
            }
        }
    }
}