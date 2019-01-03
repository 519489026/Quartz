using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Quartz;
namespace MXR.QuartzZy.AutoCode
{
    /// <summary>
    /// 激活码
    /// </summary>
    public class ActiveCodeBusiness : IJob
    {
        public void Execute(IJobExecutionContext request)
        {
            CreateActiveCode();
        }

        /// <summary>
        /// 最后创建时间
        /// </summary>
        private static DateTime LastAddTime = DateTime.Now.AddDays(-1);

        private static int CurrentCount = 0;

        private static DateTime LastDelTime = DateTime.Now.AddDays(-1);
        public static void CreateActiveCode()
        {

            if ((DateTime.Now - LastAddTime).TotalMinutes > 1440 &&DateTime.Now.Hour==2)//每天只在两点钟运行
            {
                //1、如果少于250万个，则停止生成一小时
                string strSql = "SELECT COUNT(1) AS AllCount FROM arshop.dbo.MAS_ACTIVATION_CODE_NoUse";

                DataTable dt = SqlDataAccess.sda.ExecSqlTableQuery(strSql);
                if (dt != null && dt.Rows.Count > 0)
                {
                    CurrentCount = Convert.ToInt32(dt.Rows[0]["AllCount"]);
                    if (CurrentCount > 2500000)
                    {
                        LastAddTime = DateTime.Now;
                    }
                    else
                    {              
                        //每次生成10万个
                        string strSqlInsert = string.Format(@"
                            DECLARE @INDEX INT = 0;
                            WHILE(@INDEX < 100000)
                            BEGIN
                                DECLARE @NewCode VARCHAR(50);
                                SELECT @NewCode = UPPER(NEWID())
                                SELECT @NewCode = REPLACE(@NewCode, 'I', '')
                                SELECT @NewCode = REPLACE(@NewCode, '1', '')
                                SELECT @NewCode = REPLACE(@NewCode, '0', '')
                                SELECT @NewCode = REPLACE(@NewCode, 'O', '')
                                SELECT @NewCode = REPLACE(@NewCode, '-', '')
                                SELECT @NewCode = SUBSTRING(@NewCode, 0, 8)
                                SET @INDEX = @INDEX + 1
                                INSERT INTO ARSHOP.DBO.MAS_ACTIVATION_CODE_NoUse VALUES(@NewCode)
                            END");
                        try
                        {
                            SqlDataAccess.sda.ExecuteNonQuery(strSqlInsert);
                        }
                        catch (Exception ex)
                        {
                            LogHelper.AddError("CREATEACTIVECODE", "生成激活码出错：" + ex.Message);
                        }
                    }
                }
            }

            //2、删除已用的激活码
            if ((DateTime.Now - LastDelTime).TotalMinutes > 1440 && DateTime.Now.Hour == 2)
            {
                try
                {
                    string strSqlDelete = string.Format(@"
                        DELETE TOP (20000)
                        FROM  arshop.dbo.MAS_ACTIVATION_CODE_NoUse
                        WHERE ActiveCode IN(
                        SELECT nouse.ActiveCode FROM arshop.dbo.MAS_ACTIVATION_CODE_NoUse nouse WITH(NOLOCK)
                            INNER JOIN arshop.DBO.MAS_ACTIVATION_CODE_TEMP temp WITH(NOLOCK) ON temp.activation_code = nouse.ActiveCode)");
                    if (SqlDataAccess.sda.ExecuteNonQuery(strSqlDelete) < 20000)
                    {
                        LastDelTime = DateTime.Now;
                    }
                }
                catch (Exception ex)
                {
                    LogHelper.AddError("CREATEACTIVECODE", "删除已用激活码出错：" + ex.Message);
                }
            }
        }
    }
}