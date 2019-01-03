using Quartz;
using Quartz.Impl;
using Quartz.Impl.Triggers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MXR.QuartzZy.AutoCode
{
    public class BaseFactory
    { /// <summary>
      /// 任务工厂
      /// </summary>
      /// <typeparam name="T">工作类</typeparam>
      /// <param name="DetailName">工作名称</param>
      /// <param name="TriggerName">触发器名称</param>
      /// <param name="Minute">多长时间出发一次</param>
        public static void JobsFactory<T>(string DetailName, string TriggerName, int Minute)
            where T : IJob
        {
            //工厂1
            ISchedulerFactory factory = new StdSchedulerFactory();
            //启动
            IScheduler scheduler = factory.GetScheduler();
            scheduler.Start();
            //描述工作
            IJobDetail jobDetail = new JobDetailImpl(DetailName, null, typeof(T));
            //触发器
            ISimpleTrigger trigger = new SimpleTriggerImpl(TriggerName,
                null,
                DateTime.Now,
                null,
                SimpleTriggerImpl.RepeatIndefinitely,
                TimeSpan.FromSeconds(Minute));
            //执行
            scheduler.ScheduleJob(jobDetail, trigger);
        }
    }
}