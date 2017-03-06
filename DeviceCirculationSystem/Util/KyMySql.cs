using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.Windows.Documents;
using DeviceCirculationSystem.bean;
using DeviceCirculationSystem.bean.@enum;
using MySql.Data.MySqlClient;

namespace DeviceCirculationSystem.Util
{
    internal static class KyMySql
    {
        private static MySqlConnection _connection;

        private static void connBuild()
        {
            string mysqlLogin =
                $"server = {MySqlConsts.SERVER_IP}; User Id = {MySqlConsts.SERVER_USER_NAME}; password = {MySqlConsts.SERVER_PASSWORD}; Database = {MySqlConsts.SERVER_DATABASE}; charset = utf8; Convert Zero Datetime = True";
            _connection = new MySqlConnection(mysqlLogin);
            _connection.Open();
        }

        private static void ConnBuild_WorkManager()
        {
            string mysqlLogin =
                $"server = {MySqlConsts.SERVER_IP}; User Id = {MySqlConsts.SERVER_USER_NAME}; password = {MySqlConsts.SERVER_PASSWORD}; Database = {MySqlConsts.SERVER_DATABASE_WORK_MANAGER}; charset = utf8; Convert Zero Datetime = True";
            _connection = new MySqlConnection(mysqlLogin);
            _connection.Open();
        }

        private static void connClose()
        {
            _connection.Close();
        }

        /// <summary>
        ///     核对应用程序的登录权限,根据用户名和密码核实用户是否存在
        /// </summary>
        /// <param name="userName">键入的用户名</param>
        /// <param name="password">键入的密码</param>
        /// <returns>是否有权限</returns>
        public static bool verifyPermission(string userName, string password)
        {
            var verify = false;
            var sql =
                $"SELECT * FROM {MySqlConsts.TABLE_USER_PERMISSION} WHERE Name = '{userName}' AND Password = '{password}'";
            connBuild();
            var comm = new MySqlCommand(sql, _connection);
            var read = comm.ExecuteReader();
            if (read.HasRows)
            {
                read.Close();
                verify = true;
            }
            connClose();
            return verify;
        }

        /// <summary>
        ///     核对应用程序的登录权限,根据用户名和密码核实用户是否存在
        ///     使用网页版(ASP.NET)的数据库
        /// </summary>
        /// <param name="userName">键入的用户名</param>
        /// <param name="password">键入的密码</param>
        /// <returns>是否有权限</returns>
        public static bool VerifyPermission_WorkManager(string userName, string password)
        {
            var verify = false;
            var sql =
                $"SELECT * FROM {MySqlConsts.TABLE_USER_PERMISSION_WORK_MANAGER} WHERE Name = '{userName}' AND Password = '{password}'";
            ConnBuild_WorkManager();
            var comm = new MySqlCommand(sql, _connection);
            var read = comm.ExecuteReader();
            if (read.HasRows)
            {
                read.Close();
                verify = true;
            }
            connClose();
            return verify;
        }

        public static void changePassword(string userName, string password)
        {
            var sql = $"update {MySqlConsts.TABLE_USER_PERMISSION} set 密码 = '{password}' where 用户名 = '{userName}'";
            connBuild();
            var cmdChangeStatus = new MySqlCommand(sql, _connection);
            cmdChangeStatus.ExecuteNonQuery();
            connClose();
        }

        public static void ChangePassword_WorkManager(string userName, string password)
        {
            var sql =
                $"UPDATE {MySqlConsts.TABLE_USER_PERMISSION_WORK_MANAGER} SET Password = '{password}' WHERE Name = '{userName}'";
            ConnBuild_WorkManager();
            var cmdChangeStatus = new MySqlCommand(sql, _connection);
            cmdChangeStatus.ExecuteNonQuery();
            connClose();
        }

        /// <summary>
        ///     查询某类别器件在数据库某数据表中的状态,限定用户
        /// </summary>
        /// <param name="category">待查询的器件类别</param>
        /// <param name="user">限定的用户</param>
        /// <param name="tableName">待查询的表</param>
        /// <returns>对应到DataGridView的数据表</returns>
        public static DataTable queryStorageLimitUser(string category, string user, string tableName)
        {
            DataTable dataTable = null;
            var haveCategory = (category != null) && !category.Equals("") && !category.Equals("全部");
            var haveUser = (user != null) && !user.Equals("") && !user.Equals("全部");
            Debug.WriteLine("haveCategory:" + haveCategory + "; haveUser:" + haveUser);
            var sql = "";
            //类别为空, 用户不为空
            if (!haveCategory && haveUser)
                sql = $"SELECT * FROM {tableName} WHERE 操作者 = '{user}'";
            //类别和用户均为空
            if (!haveCategory && !haveUser)
                sql = $"SELECT * FROM {tableName}";
            //类别和用户均不为空
            if (haveCategory && haveUser)
                sql = $"SELECT * FROM {tableName} WHERE 类别 = '{category}' AND 操作者 = '{user}'";
            //类别不为空, 用户为空
            if (haveCategory && !haveUser)
                sql = $"SELECT * FROM {tableName} WHERE 类别 = '{category}'";
            if (sql.Equals(""))
                throw new Exception();

            connBuild();
            Debug.WriteLine("QueryStorageLimitUser:" + sql);
            var comm = new MySqlCommand(sql, _connection);
            var read = comm.ExecuteReader();
            if (read.HasRows)
            {
                read.Close(); // 不然报错
                var sda = new MySqlDataAdapter(sql, _connection);
                var ds = new DataSet();
                sda.Fill(ds, "emp");
                dataTable = ds.Tables[0];
            }

            connClose();
            if (dataTable == null)
                throw new NotFoundFacilityException();
            return dataTable;
        }

        /// <summary>
        ///     用户向实验室仓库入库器件，外观模式
        /// </summary>
        /// <param name="facility"></param>
        public static void facilityInputToRepository(Facility facility)
        {
            //用户入库器件，记录最终器件操作者为实验室
            inputFacility(facility, facility.toUser);
            //将此条归还记录插入入库记录表，记录者为用户
            insertLogTable(facility, DeviceStatus.INPUT, facility.ownUser);
        }

        /// <summary>
        ///     用户从自己的仓库出库器件，外观模式
        /// </summary>
        /// <param name="facility"></param>
        public static void facilityOutputFromRepository(Facility facility)
        {
            //从用户拥有库存中出库器件，记录者为用户
            outputFacility(facility, facility.ownUser);
            //将此条归还记录插入归还记录表，记录者为用户
            insertLogTable(facility, DeviceStatus.OUTPUT, facility.ownUser);
        }

        /// <summary>
        ///     用户借出器件，外观模式
        /// </summary>
        /// <param name="facility"></param>
        public static void loanFacilityFromRepository(Facility facility)
        {
            //从实验室库存中出库器件，记录者为实验室
            outputFacility(facility, facility.ownUser);
            //在用户拥有库存中入库器件，记录者为用户
            inputFacility(facility, facility.toUser);
            //将此条归还记录插入借出记录表，记录者为用户
            insertLogTable(facility, DeviceStatus.LOAN, facility.toUser);
        }

        /// <summary>
        ///     用户归还器件，外观模式
        /// </summary>
        /// <param name="facility"></param>
        public static void returnFacilityToRepository(Facility facility)
        {
            //从用户拥有库存中出库器件，记录者为用户
            outputFacility(facility, facility.ownUser);
            //在实验室库存中入库器件，记录者为实验室
            inputFacility(facility, facility.toUser);
            //将此条归还记录插入归还记录表，记录者为用户
            insertLogTable(facility, DeviceStatus.RETURN, facility.ownUser);
        }

        /// <summary>
        ///     插入借出或归还历史记录表
        /// </summary>
        /// <param name="facility">器件信息</param>
        /// <param name="status">插入表的类型</param>
        /// <param name="user">操作者姓名</param>
        private static void insertLogTable(Facility facility, DeviceStatus status, string user)
        {
            //计算出库器件总价
            var priceTotal = (facility.price*facility.num).ToString(CultureInfo.CurrentCulture);
            string logTable;

            switch (status)
            {
                case DeviceStatus.RETURN:
                    logTable = MySqlConsts.TABLE_LOG_RETURN;
                    break;
                case DeviceStatus.LOAN:
                    logTable = MySqlConsts.TABLE_LOG_LOAN;
                    break;
                case DeviceStatus.INPUT:
                    logTable = MySqlConsts.TABLE_LOG_INPUT;
                    break;
                case DeviceStatus.OUTPUT:
                    logTable = MySqlConsts.TABLE_LOG_OUTPUT;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(status), status, "插入借出或归还历史记录表时，状态设置异常");
            }
            if (logTable == null) throw new Exception();
            string sqlLogTable =
                $"INSERT INTO {logTable} values ( '{facility.id}','{facility.category}','{facility.name}','{facility.modelNum}','{facility.parameter}','{facility.num}','{user}','{facility.dateTime.ToString("yyyy-MM-dd HH:mm:ss")}','{facility.price}','{priceTotal}','{facility.note}')";
            if (sqlLogTable == null)
                throw new Exception();
            connBuild();
            var cmdChangeLog = new MySqlCommand(sqlLogTable, _connection);
            cmdChangeLog.ExecuteNonQuery();
            connClose();
        }

        /// <summary>
        ///     从数据库库存状态表中出库器件
        /// </summary>
        /// <param name="facility">数据库中已有的原始器件状态信息</param>
        /// <param name="user">出库器件的原操作者</param>
        private static void outputFacility(Facility facility, string user)
        {
            var rawNum = queryDeviceNum(facility, user); //查询操作者器件数量
            var remainNum = rawNum - facility.num; //出库后，剩余库存数量

            //剩余库存数量小于0，抛出异常
            if (remainNum < 0)
                throw new NumBelowZeroException("更改后的当前器件数量不能为负数");
            string sqlStatusTable;
            //剩余库存数量等于0，删除当前器件条目
            if (remainNum == 0)
            {
                sqlStatusTable =
                    $"DELETE FROM {MySqlConsts.TABLE_STATUS_REPERTORY} WHERE 编号 = '{facility.id}' AND 类别 = '{facility.category}' AND 名称 = '{facility.name}' AND 型号 = '{facility.modelNum}' AND 规格 = '{facility.parameter}' AND 操作者 = '{user}'";
            }

            //剩余库存数量大于0，更新当前器件库存数量
            else
            {
                var priceTotalRemain = (facility.price*remainNum).ToString(CultureInfo.CurrentCulture); //计算剩余器件总价
                sqlStatusTable =
                    $"UPDATE {MySqlConsts.TABLE_STATUS_REPERTORY} SET 数量 = '{remainNum}', 总价（元） = '{priceTotalRemain}' WHERE 编号 = '{facility.id}' AND 类别 = '{facility.category}' AND 名称 = '{facility.name}' AND 型号 = '{facility.modelNum}' AND 规格 = '{facility.parameter}' AND 操作者 = '{user}'";
            }
            connBuild();
            if (sqlStatusTable == null)
                throw new Exception();
            Debug.WriteLine("OutputFacility:" + sqlStatusTable);
            var cmdChangeStatus = new MySqlCommand(sqlStatusTable, _connection);
            cmdChangeStatus.ExecuteNonQuery();
            connClose();
        }

        /// <summary>
        ///     往数据库库存状态表中入库器件
        /// </summary>
        /// <param name="facility">数据库中已有的原始器件状态信息</param>
        /// <param name="user">入库器件的预计未来操作者</param>
        private static void inputFacility(Facility facility, string user)
        {
            var rawNum = queryDeviceNum(facility, user); //查询该操作者拥有的该器件数量
            var remainNum = rawNum + facility.num; //入库后，该操作者拥有的库存数量
            var priceTotalRemain = (facility.price*remainNum).ToString(CultureInfo.CurrentCulture); //计算该操作者拥有的该器件总价
            Debug.WriteLine("rawNum:" + rawNum + ";remainNum:" + remainNum);
            string sqlStatusTable = null;
            //判断当前所选器件是否为新器件
            if (rawNum > 0) //现操作者的库存中已有该器件
                sqlStatusTable =
                    $"UPDATE {MySqlConsts.TABLE_STATUS_REPERTORY} SET 数量 = '{remainNum}', 单价（元） = '{facility.price}', 总价（元） = '{priceTotalRemain}', 备注 = '{facility.note}' WHERE 编号 = '{facility.id}' AND 类别 = '{facility.category}' AND 名称 = '{facility.name}' AND 型号 = '{facility.modelNum}' AND 规格 = '{facility.parameter}' AND 操作者 = '{user}'";
            if (rawNum == 0) //现操作者的库存中没有该器件，是新器件
                sqlStatusTable =
                    $"INSERT INTO {MySqlConsts.TABLE_STATUS_REPERTORY} values ( '{facility.id}','{facility.category}','{facility.name}','{facility.modelNum}','{facility.parameter}','{facility.num}','{user}','{facility.dateTime.ToString("yyyy-MM-dd HH:mm:ss")}','{facility.price}','{priceTotalRemain}','{facility.note}')";
            if (sqlStatusTable == null)
                throw new Exception();
            Debug.WriteLine("InputFacility:" + sqlStatusTable);

            connBuild();
            var cmdChangeStatus = new MySqlCommand(sqlStatusTable, _connection);
            cmdChangeStatus.ExecuteNonQuery();
            connClose();
        }

        /// <summary>
        ///     从数据库中获取所选器件的数量
        /// </summary>
        /// <param name="facility">所选器件信息</param>
        /// <param name="user">使用者bean</param>
        /// <returns>所选器件的数量</returns>
        private static int queryDeviceNum(Facility facility, string user)
        {
            var sqlExec =
                $"SELECT 数量 FROM {MySqlConsts.TABLE_STATUS_REPERTORY} WHERE 类别 = '{facility.category}' AND 名称 = '{facility.name}' AND 型号 = '{facility.modelNum}' AND 规格 = '{facility.parameter}' AND 操作者 = '{user}'";
            int needNum;
            connBuild();
            Debug.WriteLine("QueryDeviceNum:" + sqlExec);
            var comm5 = new MySqlCommand(sqlExec, _connection);
            var reader = comm5.ExecuteReader();
            if (reader.Read())
                needNum = int.Parse(reader["数量"].ToString());
            else
                needNum = 0;
            connClose();
            return needNum;
        }

        public static List<string> queryUserNameAll()
        {
            var list = new List<string>();

            var sqlExec =
                $"SELECT Name FROM {MySqlConsts.TABLE_USER_PERMISSION_WORK_MANAGER} WHERE Name != '实验室' AND Name != 'admin'";
            ConnBuild_WorkManager();
            var comm = new MySqlCommand(sqlExec, _connection);
            var read = comm.ExecuteReader();
            if (read.HasRows)
            {
                while (read.Read())
                {
                    list.Add(read.GetString(0));
                }
            }
            connClose();
            return list;
        }

        public static List<string> queryDistinctDevice()
        {
            var list = new List<string>();

            var sqlExec = $"SELECT distinct 类别 FROM {MySqlConsts.TABLE_STATUS_REPERTORY}";
            connBuild();
            var comm = new MySqlCommand(sqlExec, _connection);
            var read = comm.ExecuteReader();
            if (read.HasRows)
            {
                while (read.Read())
                {
                    list.Add(read.GetString(0));
                }
            }
            connClose();
            return list;
        }
    }
}