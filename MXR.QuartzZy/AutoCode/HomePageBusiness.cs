using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Quartz;
using System.Text;
using System.Data;

namespace MXR.QuartzZy.AutoCode
{
    public class HomePageBusiness : IJob
    {

        private static string LastCountTime = DateTime.Now.AddDays(-10).ToString("yyyy-MM-dd");
        public void Execute(IJobExecutionContext request)
        {
            try
            {
                UploadMainBoard();
                UploadTemplateFloow();
            }
            catch
            {

            }
        }

        private void UploadMainBoard()
        {
            string dt = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            string strSql = $@"
                UPDATE {SystemDBConfig.homepage_template_mainboard_group}
                SET del_flag = 2, update_time = GETDATE(), version = temp.version, name = temp.name, publish_time = temp.publish_time
                FROM {SystemDBConfig.homepage_template_mainboard_group_temp} temp
                WHERE homepage_template_mainboard_group.id = temp.group_id AND temp.publish_time <='{dt}';

                --删除主表中有，临时表中没有的数据
                UPDATE {SystemDBConfig.homepage_template_mainboard}
                SET del_flag = 1
                WHERE id IN(
                    SELECT board.id FROM {SystemDBConfig.homepage_template_mainboard} board
                        INNER JOIN {SystemDBConfig.homepage_template_mainboard_group_temp} tempGrp ON tempGrp.group_id = board.group_id
                        LEFT JOIN {SystemDBConfig.homepage_template_mainboard_temp} temp ON temp.ref_id = board.ref_id
                            AND temp.group_id = board.group_id AND temp.type = board.type
                    WHERE tempGrp.publish_time <= '{dt}');

                --更新主表中有，临时表中有的数据
                UPDATE {SystemDBConfig.homepage_template_mainboard}
                SET del_flag=0,sort=temp.sort,type_sort=temp.type_sort,type_name=temp.type_name,version=temp.version
                FROM {SystemDBConfig.homepage_template_mainboard_temp} temp 
	                INNER JOIN {SystemDBConfig.homepage_template_mainboard_group_temp} tempGrp ON tempGrp.group_id=temp.group_id
                WHERE {SystemDBConfig.homepage_template_mainboard}.group_id=temp.group_id 
                    AND {SystemDBConfig.homepage_template_mainboard}.ref_id=temp.ref_id
	                AND {SystemDBConfig.homepage_template_mainboard}.type=temp.type 
                    AND tempGrp.publish_time<='{dt}';

                 --增加主表中没有，临时表中有的数据
                INSERT INTO {SystemDBConfig.homepage_template_mainboard} 
                     ([type_name],[ref_id],[type],[del_flag],[sort],[type_sort],[group_id],[version])
                SELECT temp.type_name,temp.ref_id,temp.type,0,temp.sort,temp.type_sort,temp.group_id,temp.version
                FROM {SystemDBConfig.homepage_template_mainboard_temp} temp
                    INNER JOIN {SystemDBConfig.homepage_template_mainboard_group} grp ON grp.id = temp.group_id
                    LEFT  JOIN {SystemDBConfig.homepage_template_mainboard} board ON board.ref_id = temp.ref_id AND board.type = temp.type AND temp.group_id = board.group_id
                WHERE board.id IS NULL AND grp.publish_time <= '{dt}';
                DELETE FROM homepage_template_mainboard_temp 
                WHERE group_id IN(SELECT group_id FROM {SystemDBConfig.homepage_template_mainboard_group_temp} WHERE publish_time<='{dt}');
                DELETE FROM {SystemDBConfig.homepage_template_mainboard_group_temp} WHERE publish_time<='{dt}';
                UPDATE {SystemDBConfig.homepage_template_mainboard_group} SET del_flag=0 WHERE del_flag=2 AND publish_time<''";
            SqlDataAccess.sda.ExecuteNonQuery(strSql);
        }

        /// <summary>
        /// 模板上线
        /// </summary>
        private void UploadTemplateFloow()
        {
            string dt = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            string strSql = $@"
                UPDATE {SystemDBConfig.homepage_template_info}
                   SET[name] = temp.name
                      ,[description] = temp.[description]
                      ,[update_time] = temp.create_time
                      ,[del_flag] = 0
                      ,[publish_time] = temp.publish_time
                FROM {SystemDBConfig.homepage_template_info_temp} temp
                WHERE temp.publish_time<='{dt}' AND {SystemDBConfig.homepage_template_info}.id=temp.templateId  AND temp.id IS NOT NULL;

            --删除主表中有，临时表中没有的数据
                UPDATE {SystemDBConfig.homepage_template_record}
                SET [del_flag] = 1
                WHERE id IN(
                SELECT record.id FROM {SystemDBConfig.homepage_template_record} record
                    LEFT JOIN {SystemDBConfig.homepage_template_info_temp} tempTemp ON tempTemp.templateId= record.template_id
                    LEFT JOIN {SystemDBConfig.homepage_template_record_temp} temp ON record.module_id= temp.module_id AND record.template_id= temp.template_id
                WHERE tempTemp.publish_time<='{dt}' AND tempTemp.id IS NOT NULL AND temp.id IS NULL);

                --更新主表中有，临时表中有的数据
                UPDATE {SystemDBConfig.homepage_template_record}
                SET [del_flag] = 0,update_time=GETDATE()
                       WHERE id IN(
                       SELECT record.id
                       FROM {SystemDBConfig.homepage_template_record} record
                           INNER JOIN {SystemDBConfig.homepage_template_info_temp} tempTemp ON tempTemp.templateId= record.template_id       
                           INNER JOIN {SystemDBConfig.homepage_template_record_temp} temp ON record.module_id= temp.module_id AND record.template_id= temp.template_id
                       WHERE tempTemp.publish_time<='{dt}' AND tempTemp.id IS NOT NULL AND temp.id IS NULL);

                 --增加主表中没有，临时表中有的数据
                INSERT INTO {SystemDBConfig.homepage_template_record}
                ([template_id],[module_id],[create_time],[update_time],[del_flag],[sort])
                SELECT temp.template_id, temp.module_id, GETDATE(),GETDATE(),0,temp.sort
                  FROM {SystemDBConfig.homepage_template_record_temp} temp
                    INNER JOIN {SystemDBConfig.homepage_template_info_temp} info ON temp.template_id=info.templateId
                    LEFT JOIN {SystemDBConfig.homepage_template_record} record ON record.module_id=temp.module_id AND temp.template_id=record.template_id
                WHERE info.publish_time<='{dt}' AND record.id IS NULL;

                --删除临时信息
                DELETE FROM {SystemDBConfig.homepage_template_record_temp}
                WHERE template_id IN(SELECT templateId FROM {SystemDBConfig.homepage_template_info_temp} WHERE publish_time<'{dt}');
                DELETE FROM {SystemDBConfig.homepage_template_info_temp} WHERE publish_time<='{dt}';";

            SqlDataAccess.sda.ExecuteNonQuery(strSql);
        }
    }
}