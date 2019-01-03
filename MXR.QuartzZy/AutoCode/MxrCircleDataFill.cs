using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using Quartz;

namespace MXR.QuartzZy.AutoCode
{
    public class MxrCircleDataFill : IJob
    {
        public void Execute(IJobExecutionContext request)
        {
            FillTopicInfoResource();
        }

        /// <summary>
        /// 填充梦想圈的话题来源
        /// </summary>
        public void FillTopicInfoResource()
        {
            //1、获取需要更新的话题
            string strSql = $"SELECT TOP 10 id FROM {SystemDBConfig.community_topic_info} WITH(NOLOCK) WHERE src_dynamic_id = 0";
            DataTable dtTopic = SqlDataAccess.sda.ExecSqlTableQuery(strSql);
            if(dtTopic==null||dtTopic.Rows.Count==0)
            {
                return;
            }

            //2、提取话题下最早的动态
            foreach (DataRow drTopic in dtTopic.Rows)
            {
                string strSqlDynamic = $"SELECT TOP 1 id FROM {SystemDBConfig.community_dynamic_info} WITH(NOLOCK) WHERE topic_ids LIKE '%,{drTopic["id"]},%' ORDER BY id ";
                DataTable dtDynamic = SqlDataAccess.sda.ExecSqlTableQuery(strSqlDynamic);
                string strSqlUpdate = string.Empty;
                if (dtDynamic==null||dtDynamic.Rows.Count==0)
                {
                    strSqlUpdate = $"UPDATE {SystemDBConfig.community_topic_info} SET src_dynamic_id = -1 WHERE ID = {drTopic["id"].ToString()}";
                }
                else
                {
                    strSqlUpdate = $"UPDATE {SystemDBConfig.community_topic_info} SET src_dynamic_id = {dtDynamic.Rows[0]["id"].ToString()} WHERE ID = {drTopic["id"].ToString()}";
                }
                SqlDataAccess.sda.ExecuteNonQuery(strSqlUpdate);
            }
        }
    }
}