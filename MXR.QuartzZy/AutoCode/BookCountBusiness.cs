using Quartz;
using System;
using System.Data;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Text;
namespace MXR.QuartzZy.AutoCode
{
    public class BookCountBusiness : IJob
    {
        private static string LastCountTime = DateTime.Now.AddDays(-8).ToString("yyyy-MM-dd");
        public void Execute(IJobExecutionContext request)
        {
            if (LastCountTime == DateTime.Now.ToString("yyyy-MM-dd"))
            {
                return;
            }
            int result = 0;
            bool isError = false;


            try
            {
                result += CountMxbBuy();
            }
            catch (Exception ex)
            {
                LogHelper.AddLog("BOOKCOUNT", "统计数据异常：" + ex.Message);
                isError = true;
            }
            try
            {
                result += CountReadLength();
            }
            catch (Exception ex)
            {
                isError = true;
                LogHelper.AddError("BOOKCOUNT", "阅读时长统计异常：" + ex.Message);
            }

            try
            {
                result += CountCustionDownload();
            }
            catch (Exception ex)
            {
                isError = true;
                LogHelper.AddError("BOOKCOUNT", "二维码下载统计异常：" + ex.Message);
            }
            if (result == 0 && !isError)
            {
                LogHelper.AddLog("BOOKCOUNT", "统计完成，日期：" + LastCountTime);
                LastCountTime = Convert.ToDateTime(LastCountTime).AddDays(1).ToString("yyyy-MM-dd");

            }
        }

        /// <summary>
        /// 梦想币购书记录日统计
        /// </summary>
        /// <returns></returns>
        private static int CountMxbBuy()
        {

            int result = 0;
            string strSql = $@"
                    INSERT INTO {SystemDBConfig.T_DevicePurchase_History_Count}
                         ([DHCBookGuid],[DHCCoinCount],[DHCRowCount],[DHCCountDate],[DHCCreateTime])
                    SELECT  F_PurchaseContent,SUM(F_UsedCoinNum) AS AllSum,COUNT(1) AS AllCount,'{LastCountTime}',GETDATE() 
                    FROM {SystemDBConfig.T_DevicePurchase_History} dph WITH(NOLOCK)
	                    LEFT JOIN {SystemDBConfig.T_DevicePurchase_History_Count} dhc WITH(NOLOCK) ON dhc.DHCBookGuid=dph.F_PurchaseContent AND dhc.DHCCountDate='{LastCountTime}'
                    WHERE dph.F_CreateTime>='{LastCountTime}' AND dph.F_CreateTime<'{ConvertHelper.ToDateTime(LastCountTime).AddDays(1)}' AND dhc.DHCBookGuid IS NULL AND dph.F_PurchaseType=1
                    GROUP BY F_PurchaseContent ";
            result += SqlDataAccess.sda.ExecuteNonQuery(strSql);

            strSql = $@"
                    INSERT INTO {SystemDBConfig.T_DevicePurchase_History_Count}
                         ([DHCBookGuid],[DHCMxzCount],[DHCMxzRowCount],[DHCCountDate],[DHCCreateTime])
                    SELECT TOP 20 F_PurchaseContent,SUM(F_UsedCoinNum) AS AllSum,COUNT(1) AS AllCount,'{LastCountTime}',GETDATE() 
                    FROM {SystemDBConfig.T_DevicePurchase_History} dph WITH(NOLOCK)
	                    LEFT JOIN {SystemDBConfig.T_DevicePurchase_History_Count} dhc WITH(NOLOCK) ON dhc.DHCBookGuid=dph.F_PurchaseContent AND dhc.DHCCountDate='{LastCountTime}'
                    WHERE dph.F_CreateTime>='{LastCountTime}' AND dph.F_CreateTime<'{ConvertHelper.ToDateTime(LastCountTime).AddDays(1)}' AND dhc.DHCBookGuid IS NULL AND dph.F_PurchaseType=4
                    GROUP BY F_PurchaseContent ";
            result = SqlDataAccess.sda.ExecuteNonQuery(strSql);

            strSql = $@"
                    UPDATE {SystemDBConfig.T_DevicePurchase_History_Count}
                    SET DHCMxzCount = A.CoinNum, DHCMxzRowCount = A.AllCount
                    FROM(
                        SELECT COUNT(1) AS AllCount
                            , SUM(F_UsedCoinNum) AS CoinNum
                            , F_PurchaseContent
                            , CONVERT(VARCHAR(10), F_CreateTime, 120) AS CountDate
                        FROM {SystemDBConfig.T_DevicePurchase_History}
                        WHERE F_PurchaseType = 4 AND F_CreateTime > '{LastCountTime}' AND F_CreateTime < '{ConvertHelper.ToDateTime(LastCountTime).AddDays(1)}'
                        GROUP BY F_PurchaseContent, CONVERT(VARCHAR(10), F_CreateTime, 120)
                    ) AS A
                    WHERE A.CountDate = T_DevicePurchase_History_COUNT.DHCCountDate 
                        AND A.F_PurchaseContent = T_DevicePurchase_History_COUNT.DHCBookGuid ";
            result += SqlDataAccess.sda.ExecuteNonQuery(strSql);
            return result;
        }

        /// <summary>
        /// 图书阅读时长日统计
        /// </summary>
        /// <returns></returns>
        private int CountReadLength()
        {
            string strSql = $@"
                    INSERT INTO {SystemDBConfig.T_Book_ReadingDuration_Logs_Count}
                        ([BRCDuration],[BRCCount],[BRCPersonCount],[BRCBookGuid],[BRCCountDate],[BRCCreateTime])
                    SELECT TOP 400 SUM(F_ReadingDuration),COUNT(1),COUNT(DISTINCT F_DeviceId),F_BookGuid,'{LastCountTime}',GETDATE()
                    FROM {SystemDBConfig.T_Book_ReadingDuration_Logs} brl WITH(NOLOCK)
                        LEFT JOIN {SystemDBConfig.T_Book_ReadingDuration_Logs_Count} brc WITH(NOLOCK) ON brc.BRCBookGuid=brl.F_BookGuid AND brc.BRCCountDate= '{LastCountTime}'
                    WHERE F_CreateTime BETWEEN '{LastCountTime}' AND '{ConvertHelper.ToDateTime(LastCountTime).AddDays(1)}' AND brl.F_ReadingDuration<30000 AND brl.F_ReadingDuration>0 AND brc.BRCBookGuid IS NULL
                    GROUP BY F_BookGuid";
            return SqlDataAccess.sda.ExecuteNonQuery(strSql);
        }

        /// <summary>
        /// 统计二维码下载量
        /// </summary>
        /// <returns></returns>
        private int CountCustionDownload()
        {
            string strSqlSelect = $@"
                SELECT TOP 100 dba.f_source_key_int,ci.F_Customization_Url
                    , COUNT(1) AS AllCount, COUNT(DISTINCT dba.f_device_id) AS AllPersonCount
                FROM {SystemDBConfig.T_DownLoaded_Book_Data} dba WITH(NOLOCK)
                    INNER JOIN {SystemDBConfig.T_Customization_Info} ci WITH(NOLOCK) ON ci.F_Customization_ID=dba.f_source_key_int
                    LEFT JOIN {SystemDBConfig.T_Download_Book_Count} dbc WITH(NOLOCK)ON dbc.cus_id = dba.f_source_key_int AND dbc.count_date='{LastCountTime}'
                WHERE dba.f_source_type = 1 AND dba.F_Create_Time BETWEEN '{LastCountTime}' AND '{ConvertHelper.ToDateTime(LastCountTime).AddDays(1)}'
                GROUP BY dba.f_source_key_int,ci.F_Customization_Url";

            DataTable dtResource = SqlDataAccess.sda.ExecSqlTableQuery(strSqlSelect);
            if (dtResource.Rows.Count == 0)
            {
                return 0;
            }
            dtResource.Columns.Add("toolId", typeof(int));

            string strUrls = ",";
            foreach (DataRow dr in dtResource.Rows)
            {
                strUrls += $"'{dr["F_Customization_Url"].ToString().Replace("'", "") }',";
            }
            strUrls = strUrls.Trim(',');
            if (!string.IsNullOrEmpty(strUrls))
            {
                strUrls = $"SELECT [CTRId],[CTRUrl] FROM {SystemDBConfig.T_CountToolResource} WITH(NOLOCK) WHERE CTRUrl IN ({strUrls})";
                DataTable dtTool = SqlDataAccess.sda.ExecSqlTableQuery(strUrls);
                if (dtTool != null && dtTool.Rows.Count <= 0)
                {
                    foreach (DataRow drTool in dtTool.Rows)
                    {
                        DataRow[] drArrResource = dtResource.Select($"F_Customization_Url='{drTool["CTRUrl"].ToString().Replace("'", "")}'");
                        if (drArrResource != null && drArrResource.Length > 0)
                        {
                            foreach (DataRow drResource in drArrResource)
                            {
                                drResource["toolId"] = ConvertHelper.ToInt32(drTool["CTRId"]);
                            }
                        }
                    }
                }
            }

            StringBuilder sbSql = new StringBuilder();
            foreach (DataRow dr in dtResource.Rows)
            {
                sbSql.Append($@"
                    INSERT INTO {SystemDBConfig.T_Download_Book_Count}
                        ([cus_id],[count_tool_id]
                        ,[download_count],[download_person_count]
                        ,[count_date])
                    VALUES({ConvertHelper.ToInt32(dr["f_source_key_int"])},{ConvertHelper.ToInt32(dr["toolId"])}
                        , {ConvertHelper.ToInt32(dr["AllCount"])},{ConvertHelper.ToInt32(dr["AllPersonCount"])}
                        , '{LastCountTime}');");
            }
            if(sbSql.Length>0)
            {
                return SqlDataAccess.sda.ExecuteNonQuery(sbSql.ToString());
            }
            return 0;
        }
    }
}