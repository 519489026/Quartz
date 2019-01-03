using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Quartz;
using System.Text;
using System.Data;
namespace MXR.QuartzZy.AutoCode
{
    public class CountDataBusiness : IJob
    {
        private static string LastCountTime = DateTime.Now.AddDays(-9).ToString("yyyy-MM-dd");
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
                result += CountDownLoadBook();
            }
            catch (Exception ex)
            {
                LogHelper.AddLog("COUNTDATA", "图书下载统计异常：" + ex.Message + "；统计日期：" + LastCountTime);
                isError = true;
            }
            try
            {
                result += CountUGC();
            }
            catch (Exception ex)
            {
                LogHelper.AddLog("COUNTDATA", "图书UGC统计异常：" + ex.Message + "；统计日期：" + LastCountTime);
                isError = true;
            }
            try
            {
                result += CountClick();
            }
            catch (Exception ex)
            {
                LogHelper.AddLog("COUNTDATA", "图书点击统计异常：" + ex.Message + "；统计日期：" + LastCountTime);
                isError = true;
            }
            try
            {
                result += CountBannerClick();
            }
            catch (Exception ex)
            {
                LogHelper.AddLog("COUNTDATA", "Banner点击统计异常：" + ex.Message + "；统计日期：" + LastCountTime);
                isError = true;
            }
            try
            {
                result += CountTwoCode();
            }
            catch (Exception ex)
            {
                LogHelper.AddLog("COUNTDATA", "二维码统计异常：" + ex.Message + "；统计日期：" + LastCountTime);
                isError = true;
            }
            if (result == 0&&!isError)
            {
                LastCountTime = Convert.ToDateTime(LastCountTime).AddDays(1).ToString("yyyy-MM-dd");
            }
            return;
        }

        /// <summary>
        /// 统计图书下载
        /// </summary>
        /// <param name="dt"></param>
        private static int CountDownLoadBook()
        {
            //string strSql = string.Format($@"
            //    INSERT INTO {SystemDBConfig.download_book_count}
            //        ([DBCBookGuid], [DBCCountDate], [DBCCount], [DBCCreateTime])
            //    SELECT TOP 400 db.isbn,'{LastCountTime}',COUNT(db.flow_id),GETDATE()
            //    FROM {SystemDBConfig.download_book} db WITH(NOLOCK)
            //        LEFT JOIN {SystemDBConfig.download_book_count} dbc WITH(NOLOCK) ON dbc.DBCBookGuid COLLATE Chinese_PRC_90_CI_AS  = db.isbn AND dbc.DBCCountDate = '{LastCountTime}'
            //    WHERE dbc.DBCId IS NULL AND db.time BETWEEN '{LastCountTime}' AND '{ConvertHelper.ToDateTime(LastCountTime).AddDays(1)}'
            //    GROUP BY db.isbn");
            //int result = SqlDataAccess.sda.ExecuteNonQuery(strSql);
            //return result;

            string strSql = string.Format($@"
                INSERT INTO {SystemDBConfig.download_book_count}
                    ([DBCBookGuid], [DBCCountDate], [DBCCount], [DBCCreateTime])
                SELECT db.isbn,'{LastCountTime}',COUNT(db.flow_id),GETDATE()
                FROM {SystemDBConfig.download_book} db WITH(NOLOCK)
                    LEFT JOIN {SystemDBConfig.download_book_count} dbc WITH(NOLOCK) ON dbc.DBCBookGuid COLLATE Chinese_PRC_90_CI_AS  = db.isbn AND dbc.DBCCountDate = '{LastCountTime}'
                WHERE dbc.DBCId IS NULL AND db.time BETWEEN '{LastCountTime}' AND '{ConvertHelper.ToDateTime(LastCountTime).AddDays(1)}'
                GROUP BY db.isbn");
            SqlDataAccess.sda.ExecuteNonQuery(strSql);
            string strSql1 = $@"
                UPDATE {SystemDBConfig.download_book_count}
                SET DBCRegisterCount = A.AllCount, DBCRegisterPersonCount = A.PersonCount
                FROM(
                SELECT isbn, '{LastCountTime}' AS CountDate, COUNT(1) AS AllCount, COUNT(DISTINCT userID) AS PersonCount
                FROM {SystemDBConfig.download_book} WITH(NOLOCK)
                WHERE userid > 0 AND time >= '{LastCountTime}' AND time < '{ConvertHelper.ToDateTime(LastCountTime).AddDays(1)}'
                GROUP BY isbn
                ) AS A
                WHERE A.isbn COLLATE Chinese_PRC_90_CI_AS = download_book_count.DBCBookGuid  AND A.CountDate ='{LastCountTime}'";
            SqlDataAccess.sda.ExecuteNonQuery(strSql1);
            string strSql2 = $@"
                UPDATE {SystemDBConfig.download_book_count}
                SET DBCNoRegisterPersonCount = A.PersonCount
                FROM(
                SELECT isbn, '{LastCountTime}' AS CountDate, COUNT(DISTINCT phoneID) AS PersonCount
                FROM {SystemDBConfig.download_book} WITH(NOLOCK)
                WHERE userid = 0 AND time >= '{LastCountTime}' AND time < '{ConvertHelper.ToDateTime(LastCountTime).AddDays(1)}'
                GROUP BY isbn
                ) AS A
                WHERE A.isbn COLLATE Chinese_PRC_90_CI_AS = download_book_count.DBCBookGuid  AND A.CountDate = '{LastCountTime}'";
            SqlDataAccess.sda.ExecuteNonQuery(strSql2);
            return 0;
        }
        /// <summary>
        /// 统计UGC
        /// </summary>
        /// <param name="dt"></param>
        /// <returns></returns>
        private static int CountUGC()
        {
            string strSql = string.Format($@"
                SELECT TOP 410 bu.bookGuid,bu.ugcType,COUNT(1) AS AllCount
                FROM {SystemDBConfig.BookUGC} bu WITH(NOLOCK)
                    LEFT JOIN {SystemDBConfig.BookUGC_Count} buc WITH(NOLOCK) ON bu.bookGuid COLLATE Chinese_PRC_CI_AS=buc.BUCBookGuid AND buc.BUCCountDate='{LastCountTime}'
                WHERE bu.createTime BETWEEN '{LastCountTime}' AND '{ConvertHelper.ToDateTime(LastCountTime).AddDays(1)}' 
                    AND buc.BUCId IS NULL AND bu.bookGuid IS NOT NULL
                GROUP BY bu.bookGuid,bu.ugcType
                ORDER BY bu.bookGuid"
                , LastCountTime
                , ConvertHelper.ToDateTime(LastCountTime).AddDays(1));
            DataTable dtUGC = SqlDataAccess.sda.ExecSqlTableQuery(strSql);
            if (dtUGC == null || dtUGC.Rows.Count == 0)
            {
                return 0;
            }
            StringBuilder sbSqlInsert = new StringBuilder();

            string currentGuid = dtUGC.Rows[0]["bookGuid"].ToString();
            StringBuilder sbCurrentColumns = new StringBuilder();
            StringBuilder sbCurrentValues = new StringBuilder();
            if (dtUGC.Rows.Count < 410)
            {
                for (int i = 0; i < dtUGC.Rows.Count; i++)
                {
                    if (currentGuid != dtUGC.Rows[i]["bookGuid"].ToString())
                    {
                        sbSqlInsert.AppendFormat($@"
                            INSERT INTO {SystemDBConfig.BookUGC_Count}({sbCurrentColumns.ToString()}BUCBookGuid,BUCCountDate,BUCCreateTime) 
                            VALUES({sbCurrentValues.ToString()}'{currentGuid}','{LastCountTime}',GETDATE());");
                        sbCurrentColumns.Clear();
                        sbCurrentValues.Clear();
                        currentGuid = dtUGC.Rows[i]["bookGuid"].ToString();
                    }
                    switch (dtUGC.Rows[i]["ugcType"].ToString())
                    {
                        case "audio":
                            sbCurrentColumns.Append("BUCAudioCount,");
                            sbCurrentValues.Append(dtUGC.Rows[i]["AllCount"].ToString() + ",");
                            break;
                        case "image":
                            sbCurrentColumns.Append("BUCImageCount,");
                            sbCurrentValues.Append(dtUGC.Rows[i]["AllCount"].ToString() + ","); break;
                        case "video":
                            sbCurrentColumns.Append("BUCVideoCount,");
                            sbCurrentValues.Append(dtUGC.Rows[i]["AllCount"].ToString() + ","); break;
                        case "faceRecord":
                            sbCurrentColumns.Append("BUCFaceRecordCount,");
                            sbCurrentValues.Append(dtUGC.Rows[i]["AllCount"].ToString() + ","); break;
                        case "web":
                            sbCurrentColumns.Append("BUCWebCount,");
                            sbCurrentValues.Append(dtUGC.Rows[i]["AllCount"].ToString() + ","); break;
                        case "comment":
                            sbCurrentColumns.Append("BUCCommentCount,");
                            sbCurrentValues.Append(dtUGC.Rows[i]["AllCount"].ToString() + ","); break;
                        case "website":
                            sbCurrentColumns.Append("BUCWebsiteCount,");
                            sbCurrentValues.Append(dtUGC.Rows[i]["AllCount"].ToString() + ","); break;
                        case "":
                            sbCurrentColumns.Append("BUCEmptyCount,");
                            sbCurrentValues.Append(dtUGC.Rows[i]["AllCount"].ToString() + ","); break;
                    }
                    if (i == dtUGC.Rows.Count - 1)
                    {
                        sbSqlInsert.AppendFormat($@"
                            INSERT INTO {SystemDBConfig.BookUGC_Count}({sbCurrentColumns.ToString()}BUCBookGuid,BUCCountDate,BUCCreateTime) 
                            VALUES({sbCurrentValues.ToString()}'{currentGuid}','{LastCountTime}',GETDATE());");
                        sbCurrentColumns.Clear();
                        sbCurrentValues.Clear();
                        currentGuid = dtUGC.Rows[i]["bookGuid"].ToString();
                    }
                }
            }
            else
            {
                for (int i = 0; i < dtUGC.Rows.Count; i++)
                {
                    if (i == dtUGC.Rows.Count - 1 || currentGuid != dtUGC.Rows[i]["bookGuid"].ToString())
                    {
                        sbSqlInsert.AppendFormat($@"
                            INSERT INTO {SystemDBConfig.BookUGC_Count}({sbCurrentColumns.ToString()}BUCBookGuid,BUCCountDate,BUCCreateTime) 
                            VALUES({sbCurrentValues.ToString()}'{currentGuid}','{LastCountTime}',GETDATE());");
                        sbCurrentColumns.Clear();
                        sbCurrentValues.Clear();
                        if (i > 400)
                        {
                            break;
                        }
                        currentGuid = dtUGC.Rows[i]["bookGuid"].ToString();
                    }
                    switch (dtUGC.Rows[i]["ugcType"].ToString())
                    {
                        case "audio":
                            sbCurrentColumns.Append("BUCAudioCount,");
                            sbCurrentValues.Append(dtUGC.Rows[i]["AllCount"].ToString() + ",");
                            break;
                        case "image":
                            sbCurrentColumns.Append("BUCImageCount,");
                            sbCurrentValues.Append(dtUGC.Rows[i]["AllCount"].ToString() + ","); break;
                        case "video":
                            sbCurrentColumns.Append("BUCVideoCount,");
                            sbCurrentValues.Append(dtUGC.Rows[i]["AllCount"].ToString() + ","); break;
                        case "faceRecord":
                            sbCurrentColumns.Append("BUCFaceRecordCount,");
                            sbCurrentValues.Append(dtUGC.Rows[i]["AllCount"].ToString() + ","); break;
                        case "web":
                            sbCurrentColumns.Append("BUCWebCount,");
                            sbCurrentValues.Append(dtUGC.Rows[i]["AllCount"].ToString() + ","); break;
                        case "comment":
                            sbCurrentColumns.Append("BUCCommentCount,");
                            sbCurrentValues.Append(dtUGC.Rows[i]["AllCount"].ToString() + ","); break;
                        case "website":
                            sbCurrentColumns.Append("BUCWebsiteCount,");
                            sbCurrentValues.Append(dtUGC.Rows[i]["AllCount"].ToString() + ","); break;
                        case "":
                            sbCurrentColumns.Append("BUCEmptyCount,");
                            sbCurrentValues.Append(dtUGC.Rows[i]["AllCount"].ToString() + ","); break;
                    }
                }
            }
            return SqlDataAccess.sda.ExecuteNonQuery(sbSqlInsert.ToString());
        }

        /// <summary>
        /// 点击统计
        /// </summary>
        /// <param name="dt"></param>
        /// <returns></returns>
        private static int CountClick()
        {
            string strSql = string.Format($@"
                SELECT TOP 410 ch.bookGuid,ch.isOnLine,ch.buttonType,COUNT(1) AS AllCount
                FROM {SystemDBConfig.click_hotspot} ch WITH(NOLOCK)
                    LEFT JOIN {SystemDBConfig.click_hotspot_count} chc WITH(NOLOCK) ON chc.CHCBookGuid = ch.bookGuid COLLATE Chinese_PRC_90_CI_AS AND chc.CHCCountDate='{LastCountTime}'
                WHERE[time] BETWEEN '{LastCountTime}' AND '{ConvertHelper.ToDateTime(LastCountTime).AddDays(1)}' AND chc.CHCId IS NULL 
                GROUP BY ch.bookGuid, ch.isOnLine, ch.buttonType
                ORDER BY ch.bookGuid,ch.isOnLine");
            DataTable dtClick = SqlDataAccess.sda.ExecSqlTableQuery(strSql);
            if (dtClick == null || dtClick.Rows.Count == 0)
            {
                return 0;
            }
            StringBuilder sbSqlInsert = new StringBuilder();

            string currentGuid = dtClick.Rows[0]["bookGuid"].ToString();
            string currentIsonline = dtClick.Rows[0]["isOnLine"].ToString();
            StringBuilder sbCurrentColumns = new StringBuilder();
            StringBuilder sbCurrentValues = new StringBuilder();
            int CHCXmkhCount = 0;
            if (dtClick.Rows.Count < 410)
            {
                for (int i = 0; i < dtClick.Rows.Count; i++)
                {
                    if (currentGuid != dtClick.Rows[i]["bookGuid"].ToString() || currentIsonline != dtClick.Rows[i]["isOnLine"].ToString())
                    {
                        if (CHCXmkhCount > 0)
                        {
                            sbCurrentColumns.Append("CHCXmkhCount,");
                            sbCurrentValues.Append(CHCXmkhCount + ",");
                            CHCXmkhCount = 0;
                        }
                        sbSqlInsert.AppendFormat($@"
                            INSERT INTO {SystemDBConfig.click_hotspot_count}({sbCurrentColumns.ToString()}CHCBookGuid,CHCCountDate,CHCIsOnLine,CHCCreateTime) 
                            VALUES({sbCurrentValues.ToString()}'{currentGuid}','{LastCountTime}','{currentIsonline}',GETDATE());");
                        sbCurrentColumns.Clear();
                        sbCurrentValues.Clear();
                        currentGuid = dtClick.Rows[i]["bookGuid"].ToString();
                        currentIsonline = dtClick.Rows[i]["isOnLine"].ToString();
                    }

                    switch (dtClick.Rows[i]["buttonType"].ToString())
                    {
                        case "audio":
                            sbCurrentColumns.Append("CHCAudioCount,");
                            sbCurrentValues.Append(dtClick.Rows[i]["AllCount"].ToString() + ",");
                            break;
                        case "image":
                            sbCurrentColumns.Append("CHCImageCount,");
                            sbCurrentValues.Append(dtClick.Rows[i]["AllCount"].ToString() + ","); break;
                        case "video":
                            sbCurrentColumns.Append("CHCVideoCount,");
                            sbCurrentValues.Append(dtClick.Rows[i]["AllCount"].ToString() + ","); break;
                        case "model":
                            sbCurrentColumns.Append("CHCModelCount,");
                            sbCurrentValues.Append(dtClick.Rows[i]["AllCount"].ToString() + ","); break;
                        case "web":
                            sbCurrentColumns.Append("CHCWebCount,");
                            sbCurrentValues.Append(dtClick.Rows[i]["AllCount"].ToString() + ","); break;
                        case "comment":
                            sbCurrentColumns.Append("CHCCommentCount,");
                            sbCurrentValues.Append(dtClick.Rows[i]["AllCount"].ToString() + ","); break;
                        case "unknow":
                            sbCurrentColumns.Append("CHCUnknowCount,");
                            sbCurrentValues.Append(dtClick.Rows[i]["AllCount"].ToString() + ","); break;
                        case "website":
                            sbCurrentColumns.Append("CHCWebsiteCount,");
                            sbCurrentValues.Append(dtClick.Rows[i]["AllCount"].ToString() + ","); break;
                        case "":
                            sbCurrentColumns.Append("CHCEmptyCount,");
                            sbCurrentValues.Append(dtClick.Rows[i]["AllCount"].ToString() + ","); break;
                        default:
                            if (dtClick.Rows[i]["buttonType"].ToString().StartsWith("xmkh30f"))
                            {
                                CHCXmkhCount += ConvertHelper.ToInt32(dtClick.Rows[i]["AllCount"]);
                                //sbCurrentValues.Append(dtClick.Rows[i]["AllCount"].ToString() + ",");
                            }
                            break;
                    }
                    if (i == dtClick.Rows.Count - 1)
                    {
                        if (CHCXmkhCount > 0)
                        {
                            sbCurrentColumns.Append("CHCXmkhCount,");
                            sbCurrentValues.Append(CHCXmkhCount + ",");
                            CHCXmkhCount = 0;
                        }
                        sbSqlInsert.AppendFormat($@"
                            INSERT INTO {SystemDBConfig.click_hotspot_count}({sbCurrentColumns.ToString()}CHCBookGuid,CHCCountDate,CHCIsOnLine,CHCCreateTime) 
                            VALUES({sbCurrentValues.ToString()}'{currentGuid}','{LastCountTime}','{currentIsonline}',GETDATE());");
                        sbCurrentColumns.Clear();
                        sbCurrentValues.Clear();
                        currentGuid = dtClick.Rows[i]["bookGuid"].ToString();
                        currentIsonline = dtClick.Rows[i]["isOnLine"].ToString();
                    }
                }
            }
            else
            {
                for (int i = 0; i < dtClick.Rows.Count; i++)
                {
                    if (currentGuid != dtClick.Rows[i]["bookGuid"].ToString() || currentIsonline != dtClick.Rows[i]["isOnLine"].ToString())
                    {
                        if (CHCXmkhCount > 0)
                        {
                            sbCurrentColumns.Append("CHCXmkhCount,");
                            sbCurrentValues.Append(CHCXmkhCount + ",");
                            CHCXmkhCount = 0;
                        }
                        sbSqlInsert.AppendFormat($@"
                            INSERT INTO {SystemDBConfig.click_hotspot_count}({sbCurrentColumns.ToString()}CHCBookGuid,CHCCountDate,CHCIsOnLine,CHCCreateTime) 
                            VALUES({sbCurrentValues.ToString()}'{currentGuid}','{LastCountTime}','{currentIsonline}',GETDATE());");
                        sbCurrentColumns.Clear();
                        sbCurrentValues.Clear();
                        if (i > 400)
                        {
                            break;
                        }
                        currentGuid = dtClick.Rows[i]["bookGuid"].ToString();
                        currentIsonline = dtClick.Rows[i]["isOnLine"].ToString();
                    }

                    switch (dtClick.Rows[i]["buttonType"].ToString())
                    {
                        case "audio":
                            sbCurrentColumns.Append("CHCAudioCount,");
                            sbCurrentValues.Append(dtClick.Rows[i]["AllCount"].ToString() + ",");
                            break;
                        case "image":
                            sbCurrentColumns.Append("CHCImageCount,");
                            sbCurrentValues.Append(dtClick.Rows[i]["AllCount"].ToString() + ","); break;
                        case "video":
                            sbCurrentColumns.Append("CHCVideoCount,");
                            sbCurrentValues.Append(dtClick.Rows[i]["AllCount"].ToString() + ","); break;
                        case "model":
                            sbCurrentColumns.Append("CHCModelCount,");
                            sbCurrentValues.Append(dtClick.Rows[i]["AllCount"].ToString() + ","); break;
                        case "web":
                            sbCurrentColumns.Append("CHCWebCount,");
                            sbCurrentValues.Append(dtClick.Rows[i]["AllCount"].ToString() + ","); break;
                        case "comment":
                            sbCurrentColumns.Append("CHCCommentCount,");
                            sbCurrentValues.Append(dtClick.Rows[i]["AllCount"].ToString() + ","); break;
                        case "unknow":
                            sbCurrentColumns.Append("CHCUnknowCount,");
                            sbCurrentValues.Append(dtClick.Rows[i]["AllCount"].ToString() + ","); break;
                        case "website":
                            sbCurrentColumns.Append("CHCWebsiteCount,");
                            sbCurrentValues.Append(dtClick.Rows[i]["AllCount"].ToString() + ","); break;
                        case "":
                            sbCurrentColumns.Append("CHCEmptyCount,");
                            sbCurrentValues.Append(dtClick.Rows[i]["AllCount"].ToString() + ","); break;
                        default:
                            if (dtClick.Rows[i]["buttonType"].ToString().StartsWith("xmkh30f"))
                            {
                                CHCXmkhCount += ConvertHelper.ToInt32(dtClick.Rows[i]["AllCount"]);
                                //sbCurrentValues.Append(dtClick.Rows[i]["AllCount"].ToString() + ",");
                            }
                            break;
                    }
                }
            }
            return SqlDataAccess.sda.ExecuteNonQuery(sbSqlInsert.ToString());
        }

        /// <summary>
        /// Banner点击统计
        /// </summary>
        /// <returns></returns>
        private static int CountBannerClick()
        {
            string strSql = string.Format($@"
                INSERT INTO {SystemDBConfig.T_Device_ClickBanner_Count}
                ([DBCBannerId],[DBCClickCount],[DBCCountDate],[DBCCreateTime])
                SELECT TOP 100 dcb.[F_BannerID],COUNT(1) AS AllCount,'{LastCountTime}', GETDATE()
                FROM {SystemDBConfig.T_Device_ClickBanner} dcb WITH(NOLOCK)
                    LEFT JOIN {SystemDBConfig.T_Device_ClickBanner_Count} dbc WITH(NOLOCK) ON dbc.[DBCBannerId] = dcb.F_BannerID AND dbc.DBCCountDate='{LastCountTime}'
                WHERE dbc.[DBCBannerId] IS NULL AND dcb.F_CreateTime BETWEEN '{LastCountTime}' AND '{ConvertHelper.ToDateTime(LastCountTime).AddDays(1)}'
                GROUP BY dcb.[F_BannerID]");
            return SqlDataAccess.sda.ExecuteNonQuery(strSql);
        }

        /// <summary>
        /// 二维码访问统计
        /// </summary>
        /// <returns></returns>
        private static int CountTwoCode()
        {
            int result = 0;
            string strSql = $@"
                INSERT INTO {SystemDBConfig.T_CountToolCount}
                    ([CTC_CTRId],[CTCCountDate],[CTCBrowseCount]
                    ,[CTCBrowsePersonCount],[CTCDownloadCount],[CTCCreateTime])
                SELECT hi.CTH_CTRId,'{LastCountTime}',0,0,0, GETDATE()
                FROM {SystemDBConfig.T_CountToolHistory} hi 
                    LEFT JOIN {SystemDBConfig.T_CountToolCount} ct ON ct.CTC_CTRId = hi.CTH_CTRId AND ct.CTCCountDate = '{LastCountTime}'
                WHERE hi.CTHVisitDate = '{LastCountTime}' AND ct.CTC_CTRId IS NULL
                GROUP BY hi.CTH_CTRId";
            result += SqlDataAccess.sda.ExecuteNonQuery(strSql);

            //strSql = $@"
            //     INSERT INTO {SystemDBConfig.T_CountToolCount}
            //         ([CTC_CTRId],[CTCCountDate]
            //                ,[CTCBrowseCount]
            //                ,[CTCBrowsePersonCount]
            //                ,[CTCDownloadCount]
            //                ,[CTCCreateTime])
            //    SELECT hi.CTH_CTRId,'{LastCountTime}', 0,0,0, GETDATE()
            //      FROM {SystemDBConfig.T_CountToolHistory} hi  
            //          LEFT JOIN {SystemDBConfig.T_CountToolCount} ct ON ct.CTC_CTRId = hi.CTH_CTRId AND ct.CTCCountDate = '{LastCountTime}'
            //      WHERE hi.CTHType = 1 AND hi.CTHVisitDate = '{LastCountTime}' AND ct.CTC_CTRId IS NULL--AND ct.CTCBrowseCount = 0
            //      GROUP BY hi.CTH_CTRId";
            //result += SqlDataAccess.sda.ExecuteNonQuery(strSql);

            strSql = $@"
                UPDATE {SystemDBConfig.T_CountToolCount}
                SET CTCDownloadCount = A.AllCount
                FROM(
                SELECT hi.CTH_CTRId, COUNT(1) AS AllCount
                FROM {SystemDBConfig.T_CountToolHistory} hi
                     INNER JOIN {SystemDBConfig.T_CountToolCount} ct ON ct.CTC_CTRId = hi.CTH_CTRId AND ct.CTCCountDate = '{LastCountTime}'
                WHERE hi.CTHType = 3 AND ct.CTC_CTRId > 0 AND hi.CTHVisitDate = '{LastCountTime}'
                GROUP BY hi.CTH_CTRId) AS A
                WHERE A.CTH_CTRId = {SystemDBConfig.T_CountToolCount}.CTC_CTRId AND {SystemDBConfig.T_CountToolCount}.CTCCountDate='{LastCountTime}'";
            result += SqlDataAccess.sda.ExecuteNonQuery(strSql);



            strSql = $@"
              UPDATE {SystemDBConfig.T_CountToolCount}
              SET CTCBrowseCount = A.AllCount, CTCBrowsePersonCount = A.AllPersonContent
              FROM(
              SELECT hi.CTH_CTRId, ct.CTC_CTRId, COUNT(1) AS AllCount, COUNT(DISTINCT hi.CTCookie) AS AllPersonContent
              FROM  {SystemDBConfig.T_CountToolHistory} hi  
                  INNER JOIN {SystemDBConfig.T_CountToolCount} ct ON ct.CTC_CTRId = hi.CTH_CTRId AND ct.CTCCountDate = '{LastCountTime}'
              WHERE hi.CTHType = 1 AND ct.CTC_CTRId > 0 AND hi.CTHVisitDate = '{LastCountTime}'
              GROUP BY hi.CTH_CTRId, ct.CTC_CTRId) AS A
              WHERE A.CTC_CTRId = {SystemDBConfig.T_CountToolCount}.CTC_CTRId AND {SystemDBConfig.T_CountToolCount}.CTCCountDate='{LastCountTime}'";
            result += SqlDataAccess.sda.ExecuteNonQuery(strSql);
            return 0;
        }



    }
}
