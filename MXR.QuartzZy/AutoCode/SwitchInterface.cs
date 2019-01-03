using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml;
using Quartz;
namespace MXR.QuartzZy.AutoCode
{
    public class SwitchInterface
    {
        public static void Execute()
        {
            XmlDocument xmldoc = new XmlDocument();
            xmldoc.Load(HttpContext.Current.Server.MapPath("/XMLFiles/TaskList.xml"));
            XmlNodeList xnlTask = xmldoc.SelectNodes("ROOT/TASK");
            try
            {
                foreach (XmlNode nodeTask in xnlTask)
                {
                    switch (nodeTask.Attributes["KEY"].Value)
                    {
                        case "COUNTDATA": BaseFactory.JobsFactory<CountDataBusiness>(nodeTask.Attributes["KEY"].Value, nodeTask.Attributes["DESC"].Value, ConvertHelper.ToInt32(nodeTask.Attributes["TIMEOUT"].Value)); break;
                        case "CREATEACTIVECODE": BaseFactory.JobsFactory<ActiveCodeBusiness>(nodeTask.Attributes["KEY"].Value, nodeTask.Attributes["DESC"].Value, ConvertHelper.ToInt32(nodeTask.Attributes["TIMEOUT"].Value)); break;
                        case "BOOKCOUNT": BaseFactory.JobsFactory<BookCountBusiness>(nodeTask.Attributes["KEY"].Value, nodeTask.Attributes["DESC"].Value, ConvertHelper.ToInt32(nodeTask.Attributes["TIMEOUT"].Value)); break;
                        case "MxrCircleDataFill": BaseFactory.JobsFactory<MxrCircleDataFill>(nodeTask.Attributes["KEY"].Value, nodeTask.Attributes["DESC"].Value, ConvertHelper.ToInt32(nodeTask.Attributes["TIMEOUT"].Value)); break;
                        case "HomePagePublish": BaseFactory.JobsFactory<HomePageBusiness>(nodeTask.Attributes["KEY"].Value, nodeTask.Attributes["DESC"].Value, ConvertHelper.ToInt32(nodeTask.Attributes["TIMEOUT"].Value)); break;
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.AddError("SwitchInterface异常", ex.Message);
            }
        }
    }
}