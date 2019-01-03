using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Quartz;

namespace MXR.QuartzZy.AutoCode
{
    public class TestBusiness : IJob
    {
        public void Execute(IJobExecutionContext request)
        {
            //我的逻辑
        }
    }
}