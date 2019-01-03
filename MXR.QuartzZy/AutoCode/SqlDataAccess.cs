using System;
using System.Data;
using System.Configuration;

using System.Web;

using System.Data.SqlClient;
using System.Text;
using System.Collections;

namespace MXR.QuartzZy.AutoCode
{
    /// <summary>
    /// Summary description for SqlDataAccess
    /// </summary>
    public class SqlDataAccess
    {
        //sqlConnection
        private static string pvtStrSqlCon =System.Web.Configuration.WebConfigurationManager.AppSettings["DBMagic_Book"];
        //private SqlConnection pvtSqlCon = new SqlConnection(pvtStrSqlCon);

        private string pvtStrTableName = string.Empty;
        private SqlCommand pvtSqlCom = null;

        public static SqlDataAccess sda = new SqlDataAccess(pvtStrSqlCon);

        public SqlDataAccess(string strTableName)
        {
            if (pvtStrTableName == string.Empty)
            {
                pvtStrTableName = strTableName;
            }
            pvtSqlCom = new SqlCommand();
        }

        //without parameter
        public SqlDataAccess()
        {
           pvtSqlCom = new SqlCommand();
        }

        //
        ~SqlDataAccess()
        {
        }

        /// <summary>
        /// when update or insert, call this method
        /// </summary>
        /// <param name="strProcName"></param>
        /// <param name="sqlParamCol"></param>
        public void ExecProcNoneQuery(string strProcName, SqlParameterCollection sqlParamCol)
        {
            using (SqlConnection pvtSqlCon = new SqlConnection(pvtStrSqlCon))
            {
                pvtSqlCon.Open();
                pvtSqlCom.Connection = pvtSqlCon;
                pvtSqlCom.CommandTimeout = 300;
                pvtSqlCom.CommandType = CommandType.StoredProcedure;
                pvtSqlCom.CommandText = strProcName;
                for (int i = 0; i < sqlParamCol.Count; i++)
                {
                    pvtSqlCom.Parameters.Add(sqlParamCol[i].ParameterName, sqlParamCol[i].SqlDbType, sqlParamCol[i].Size, sqlParamCol[i].SourceColumn);
                    pvtSqlCom.Parameters[i].Value = sqlParamCol[i].Value;
                    pvtSqlCom.Parameters[i].Direction = sqlParamCol[i].Direction;
                }
                pvtSqlCom.ExecuteNonQuery();
                for (int i = 0; i < sqlParamCol.Count; i++)
                {
                    sqlParamCol[i].Value = pvtSqlCom.Parameters[i].Value;
                }
                pvtSqlCon.Close();
            }
        }

        //when wanna to get a data result, call this method
        public DataSet ExecProcReader(string strProcName, SqlParameterCollection sqlParamCol)
        {

            using (SqlConnection pvtSqlCon = new SqlConnection(pvtStrSqlCon))
            {
                pvtSqlCon.Open();
                pvtSqlCom.Connection = pvtSqlCon;
                pvtSqlCom.CommandTimeout = 300;
                pvtSqlCom.CommandType = CommandType.StoredProcedure;
                pvtSqlCom.CommandText = strProcName;
                for (int i = 0; i < sqlParamCol.Count; i++)
                {
                    pvtSqlCom.Parameters.Add(sqlParamCol[i].ParameterName, sqlParamCol[i].SqlDbType, sqlParamCol[i].Size, sqlParamCol[i].SourceColumn);
                    pvtSqlCom.Parameters[i].Value = sqlParamCol[i].Value;
                    pvtSqlCom.Parameters[i].Direction = sqlParamCol[i].Direction;
                }
                SqlDataAdapter daResult = new SqlDataAdapter(pvtSqlCom);
                DataSet dsResult = new DataSet();
                daResult.Fill(dsResult);
                for (int i = 0; i < sqlParamCol.Count; i++)
                {
                    sqlParamCol[i].Value = pvtSqlCom.Parameters[i].Value;
                }
                pvtSqlCon.Close();
                return dsResult;
            }
        }

        //when wanna to use sql to get a data result, call this method
        public DataSet ExecSqlReader(string strSql, SqlParameterCollection sqlParamCol)
        {

            using (SqlConnection pvtSqlCon = new SqlConnection(pvtStrSqlCon))
            {
                pvtSqlCon.Open();
                pvtSqlCom.Connection = pvtSqlCon;
                pvtSqlCom.CommandTimeout = 300;
                pvtSqlCom.CommandType = CommandType.Text;
                pvtSqlCom.CommandText = strSql;
                for (int i = 0; i < sqlParamCol.Count; i++)
                {
                    pvtSqlCom.Parameters.Add(sqlParamCol[i].ParameterName, sqlParamCol[i].SqlDbType, sqlParamCol[i].Size, sqlParamCol[i].SourceColumn);
                    pvtSqlCom.Parameters[i].Value = sqlParamCol[i].Value;
                    pvtSqlCom.Parameters[i].Direction = sqlParamCol[i].Direction;
                }
                SqlDataAdapter daResult = new SqlDataAdapter(pvtSqlCom);
                DataSet dsResult = new DataSet();
                daResult.Fill(dsResult);
                for (int i = 0; i < sqlParamCol.Count; i++)
                {
                    sqlParamCol[i].Value = pvtSqlCom.Parameters[i].Value;
                }
                pvtSqlCon.Close();
                return dsResult;
            }
        }

        public DataSet ExecSqlQuery(string strSql)
        {
            using (SqlConnection pvtSqlCon = new SqlConnection(pvtStrSqlCon))
            {
                pvtSqlCon.Open();
                pvtSqlCom.Connection = pvtSqlCon;
                pvtSqlCom.CommandTimeout = 300;
                pvtSqlCom.CommandType = CommandType.Text;
                pvtSqlCom.CommandText = strSql;
                SqlDataAdapter daResult = new SqlDataAdapter(pvtSqlCom);
                DataSet dsResult = new DataSet();
                daResult.Fill(dsResult);
                pvtSqlCon.Close();
                return dsResult;
            }
        }
        public DataSet ExecSqlQuery(string strSql, SqlParameterCollection sqlParamCol)
        {
            using (SqlConnection pvtSqlCon = new SqlConnection(pvtStrSqlCon))
            {
                pvtSqlCon.Open();
                pvtSqlCom.Connection = pvtSqlCon;
                pvtSqlCom.CommandType = CommandType.Text;
                pvtSqlCom.CommandText = strSql;
                pvtSqlCom.CommandTimeout = 300;
                for (int i = 0; i < sqlParamCol.Count; i++)
                {
                    pvtSqlCom.Parameters.Add(sqlParamCol[i].ParameterName, sqlParamCol[i].SqlDbType, sqlParamCol[i].Size, sqlParamCol[i].SourceColumn);
                    pvtSqlCom.Parameters[i].Value = sqlParamCol[i].Value;
                    pvtSqlCom.Parameters[i].Direction = sqlParamCol[i].Direction;
                }
                SqlDataAdapter daResult = new SqlDataAdapter(pvtSqlCom);
                DataSet dsResult = new DataSet();
                daResult.Fill(dsResult);
                pvtSqlCom.Parameters.Clear();
                pvtSqlCon.Close();
                return dsResult;
            }
        }

        public DataTable ExecSqlTableQuery(string strSql)
        {
            using (SqlConnection pvtSqlCon = new SqlConnection(pvtStrSqlCon))
            {
                pvtSqlCon.Open();
                pvtSqlCom.Connection = pvtSqlCon;
                pvtSqlCom.CommandTimeout = 300;
                pvtSqlCom.CommandType = CommandType.Text;
                pvtSqlCom.CommandText = strSql;
                SqlDataAdapter daResult = new SqlDataAdapter(pvtSqlCom);
                DataTable dtResult = new DataTable();
                daResult.Fill(dtResult);
                pvtSqlCon.Close();
                return dtResult;
            }
        }
        public DataTable ExecSqlTableQuery(string strSql, SqlParameterCollection sqlParamCol)
        {
            using (SqlConnection pvtSqlCon = new SqlConnection(pvtStrSqlCon))
            {
                pvtSqlCon.Open();
                pvtSqlCom.Connection = pvtSqlCon;
                pvtSqlCom.CommandType = CommandType.Text;
                pvtSqlCom.CommandText = strSql;
                pvtSqlCom.CommandTimeout = 300;
                for (int i = 0; i < sqlParamCol.Count; i++)
                {
                    pvtSqlCom.Parameters.Add(sqlParamCol[i].ParameterName, sqlParamCol[i].SqlDbType, sqlParamCol[i].Size, sqlParamCol[i].SourceColumn);
                    pvtSqlCom.Parameters[i].Value = sqlParamCol[i].Value;
                    pvtSqlCom.Parameters[i].Direction = sqlParamCol[i].Direction;
                }
                SqlDataAdapter daResult = new SqlDataAdapter(pvtSqlCom);
                DataTable dtResult = new DataTable();
                daResult.Fill(dtResult);
                pvtSqlCom.Parameters.Clear();
                pvtSqlCon.Close();
                return dtResult;
            }
        }
        public int ExecSqlHandel(string strSql)
        {
            strSql = strSql + ";SELECT SCOPE_IDENTITY()";
            using (SqlConnection pvtSqlCon = new SqlConnection(pvtStrSqlCon))
            {
                pvtSqlCon.Open();
                pvtSqlCom.Connection = pvtSqlCon;
                pvtSqlCom.CommandTimeout = 300;
                pvtSqlCom.CommandType = CommandType.Text;
                pvtSqlCom.CommandText = strSql;
                string sResult = pvtSqlCom.ExecuteScalar().ToString();
                pvtSqlCon.Close();
                int nResult = 0;
                if (!sResult.Equals(""))
                {
                    nResult = int.Parse(sResult);
                }
                return nResult;
            }
        }
        public int ExecSqlHandel(string strSql, SqlParameterCollection sqlParamCol)
        {
            strSql = strSql + ";SELECT SCOPE_IDENTITY()";
            using (SqlConnection pvtSqlCon = new SqlConnection(pvtStrSqlCon))
            {
                pvtSqlCon.Open();
                pvtSqlCom.Connection = pvtSqlCon;
                pvtSqlCom.CommandTimeout = 300;
                pvtSqlCom.CommandType = CommandType.Text;
                pvtSqlCom.CommandText = strSql;
                for (int i = 0; i < sqlParamCol.Count; i++)
                {
                    pvtSqlCom.Parameters.Add(sqlParamCol[i].ParameterName, sqlParamCol[i].SqlDbType, sqlParamCol[i].Size, sqlParamCol[i].SourceColumn);
                    pvtSqlCom.Parameters[i].Value = sqlParamCol[i].Value;
                    pvtSqlCom.Parameters[i].Direction = sqlParamCol[i].Direction;
                }
                string sResult = pvtSqlCom.ExecuteScalar().ToString();
                pvtSqlCom.Parameters.Clear();
                pvtSqlCon.Close();
                int nResult = string.IsNullOrEmpty(sResult) ? 0 : Convert.ToInt32(sResult);
                return nResult;
            }
        }

        /// <summary>
        /// 批量保存数量
        /// </summary>
        /// <param name="dt">要保存的数据</param>
        public void DoSave(DataTable dt, string demoSql)
        {
            using (SqlConnection pvtSqlCon = new SqlConnection(pvtStrSqlCon))
            {
                pvtSqlCon.Open();
                pvtSqlCom.Connection = pvtSqlCon;
                pvtSqlCom.CommandTimeout = 300;
                SqlDataAdapter sd = new SqlDataAdapter(demoSql, pvtStrSqlCon);
                SqlCommandBuilder buile = new SqlCommandBuilder(sd);
                try
                {
                    sd.Update(dt);
                }
                catch (Exception ex)
                {
                    throw ex;
                }
                pvtSqlCon.Close();
            }

        }

        /// <summary>
        /// 增删改数据 事务
        /// </summary>
        /// <param name="aList"></param>
        /// <returns></returns>
        public int ExecuteNonQuery(ArrayList aList)
        {
            SqlTransaction transaction = null;
            using (SqlConnection pvtSqlCon = new SqlConnection(pvtStrSqlCon))
            {
                pvtSqlCon.Open();

                transaction = pvtSqlCon.BeginTransaction();
                pvtSqlCom.Transaction = transaction;
                pvtSqlCom.CommandTimeout = 300;
                pvtSqlCom.Connection = pvtSqlCon;
                pvtSqlCom.CommandType = CommandType.Text;

                int sResult = -1;
                try
                {
                    foreach (string str in aList)
                    {
                        pvtSqlCom.CommandText = str;
                        sResult = (int)pvtSqlCom.ExecuteNonQuery();
                        if (sResult < 0)
                        {
                            transaction.Rollback();
                        }
                    }
                    transaction.Commit();
                }
                catch (Exception)
                {
                    sResult = -1;
                    transaction.Rollback();
                }
                finally
                {
                    pvtSqlCon.Close();
                }


                return sResult;
            }
        }


        /// <summary>
        /// 增删改数据 返回影响行数
        /// </summary>
        /// <param name="aList"></param>
        /// <returns></returns>
        public int ExecuteNonQuery(string strSql, SqlParameterCollection sqlParamCol)
        {

            using (SqlConnection pvtSqlCon = new SqlConnection(pvtStrSqlCon))
            {
                pvtSqlCon.Open();
                pvtSqlCom.CommandTimeout = 300;
                pvtSqlCom.Connection = pvtSqlCon;
                pvtSqlCom.CommandType = CommandType.Text;
                pvtSqlCom.CommandText = strSql;
                for (int i = 0; i < sqlParamCol.Count; i++)
                {
                    pvtSqlCom.Parameters.Add(sqlParamCol[i].ParameterName, sqlParamCol[i].SqlDbType, sqlParamCol[i].Size, sqlParamCol[i].SourceColumn);
                    pvtSqlCom.Parameters[i].Value = sqlParamCol[i].Value;
                    pvtSqlCom.Parameters[i].Direction = sqlParamCol[i].Direction;
                }
                int sResult = -1;
                sResult = pvtSqlCom.ExecuteNonQuery();
                pvtSqlCom.Parameters.Clear();
                pvtSqlCon.Close();
                return sResult;
            }
        }

        /// <summary>
        /// 增删改数据 返回影响行数
        /// </summary>
        /// <param name="strSql"></param>
        /// <returns></returns>
        public int ExecuteNonQuery(string strSql)
        {
            using (SqlConnection pvtSqlCon = new SqlConnection(pvtStrSqlCon))
            {
                pvtSqlCon.Open();
                pvtSqlCom.Connection = pvtSqlCon;
                pvtSqlCom.CommandType = CommandType.Text;
                pvtSqlCom.CommandText = strSql;
                pvtSqlCom.CommandTimeout = 300;
                int sResult = pvtSqlCom.ExecuteNonQuery();
                pvtSqlCom.Parameters.Clear();
                pvtSqlCon.Close();
                return sResult;
            }
        }
    }
}