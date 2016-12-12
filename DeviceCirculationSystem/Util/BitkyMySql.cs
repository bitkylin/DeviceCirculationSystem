using System;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using DeviceCirculationSystem.bean;
using DeviceCirculationSystem.bean.@enum;
using MySql.Data.MySqlClient;

namespace DeviceCirculationSystem.Util
{
    internal static class BitkyMySql
    {
        private static MySqlConnection _connection;

        private static void ConnBuild()
        {
            string mysqlLogin =
                $"server = {KySet.ServerIp}; User Id = {KySet.ServerUserName}; password = {KySet.ServerPassword}; Database = {KySet.ServerDatabase}; charset = utf8; Convert Zero Datetime = True";
            _connection = new MySqlConnection(mysqlLogin);
            _connection.Open();
        }

        private static void ConnBuild_WorkManager()
        {
            string mysqlLogin =
                $"server = {KySet.ServerIp}; User Id = {KySet.ServerUserName}; password = {KySet.ServerPassword}; Database = {KySet.ServerDatabaseWorkManager}; charset = utf8; Convert Zero Datetime = True";
            _connection = new MySqlConnection(mysqlLogin);
            _connection.Open();
        }

        private static void ConnClose()
        {
            _connection.Close();
        }

        /// <summary>
        ///     核对应用程序的登录权限,根据用户名和密码核实用户是否存在
        /// </summary>
        /// <param name="userName">键入的用户名</param>
        /// <param name="password">键入的密码</param>
        /// <returns>是否有权限</returns>
        public static bool VerifyPermission(string userName, string password)
        {
            var verify = false;
            var sql =
                $"SELECT * FROM {KySet.TableUserPermission} WHERE Name = '{userName}' AND Password = '{password}'";
            ConnBuild();
            var comm = new MySqlCommand(sql, _connection);
            var read = comm.ExecuteReader();
            if (read.HasRows)
            {
                read.Close();
                verify = true;
            }
            ConnClose();
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
                $"SELECT * FROM {KySet.TableUserPermissionWorkManager} WHERE Name = '{userName}' AND Password = '{password}'";
            ConnBuild_WorkManager();
            var comm = new MySqlCommand(sql, _connection);
            var read = comm.ExecuteReader();
            if (read.HasRows)
            {
                read.Close();
                verify = true;
            }
            ConnClose();
            return verify;
        }

        public static void ChangePassword(string userName, string password)
        {
            var sql = $"update {KySet.TableUserPermission} set 密码 = '{password}' where 用户名 = '{userName}'";
            ConnBuild();
            var cmdChangeStatus = new MySqlCommand(sql, _connection);
            cmdChangeStatus.ExecuteNonQuery();
            ConnClose();
        }

        public static void ChangePassword_WorkManager(string userName, string password)
        {
            var sql =
                $"UPDATE {KySet.TableUserPermissionWorkManager} SET Password = '{password}' WHERE Name = '{userName}'";
            ConnBuild_WorkManager();
            var cmdChangeStatus = new MySqlCommand(sql, _connection);
            cmdChangeStatus.ExecuteNonQuery();
            ConnClose();
        }

        /// <summary>
        ///     查询某类别器件在数据库某数据表中的状态,限定用户
        /// </summary>
        /// <param name="category">待查询的器件类别</param>
        /// <param name="user">限定的用户</param>
        /// <param name="tableName">待查询的表</param>
        /// <returns>对应到DataGridView的数据表</returns>
        public static DataTable QueryStorageLimitUser(string category, string user, string tableName)
        {
            DataTable dataTable = null;
            var haveCategory = (category != null) && !category.Equals("");
            var haveUser = (user != null) && !user.Equals("");
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

            ConnBuild();
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

            ConnClose();
            if (dataTable == null)
                throw new NotFoundFacilityException();
            return dataTable;
        }

        /// <summary>
        ///     用户向实验室仓库入库器件，外观模式
        /// </summary>
        /// <param name="facility"></param>
        public static void FacilityInputToRepository(Facility facility)
        {
            //用户入库器件，记录最终器件操作者为实验室
            InputFacility(facility, facility.ToUser);
            //将此条归还记录插入入库记录表，记录者为用户
            InsertLogTable(facility, DeviceStatus.Input, facility.OwnUser);
        }

        /// <summary>
        ///     用户从自己的仓库出库器件，外观模式
        /// </summary>
        /// <param name="facility"></param>
        public static void FacilityOutputFromRepository(Facility facility)
        {
            //从用户拥有库存中出库器件，记录者为用户
            OutputFacility(facility, facility.OwnUser);
            //将此条归还记录插入归还记录表，记录者为用户
            InsertLogTable(facility, DeviceStatus.Output, facility.OwnUser);
        }

        /// <summary>
        ///     用户借出器件，外观模式
        /// </summary>
        /// <param name="facility"></param>
        public static void LoanFacilityFromRepository(Facility facility)
        {
            //从实验室库存中出库器件，记录者为实验室
            OutputFacility(facility, facility.OwnUser);
            //在用户拥有库存中入库器件，记录者为用户
            InputFacility(facility, facility.ToUser);
            //将此条归还记录插入借出记录表，记录者为用户
            InsertLogTable(facility, DeviceStatus.Loan, facility.ToUser);
        }

        /// <summary>
        ///     用户归还器件，外观模式
        /// </summary>
        /// <param name="facility"></param>
        public static void ReturnFacilityToRepository(Facility facility)
        {
            //从用户拥有库存中出库器件，记录者为用户
            OutputFacility(facility, facility.OwnUser);
            //在实验室库存中入库器件，记录者为实验室
            InputFacility(facility, facility.ToUser);
            //将此条归还记录插入归还记录表，记录者为用户
            InsertLogTable(facility, DeviceStatus.Return, facility.OwnUser);
        }

        /// <summary>
        ///     插入借出或归还历史记录表
        /// </summary>
        /// <param name="facility">器件信息</param>
        /// <param name="status">插入表的类型</param>
        /// <param name="user">操作者姓名</param>
        private static void InsertLogTable(Facility facility, DeviceStatus status, string user)
        {
            //计算出库器件总价
            var priceTotal = (facility.Price*facility.Num).ToString(CultureInfo.CurrentCulture);
            string logTable;

            switch (status)
            {
                case DeviceStatus.Return:
                    logTable = KySet.TableLogReturn;
                    break;
                case DeviceStatus.Loan:
                    logTable = KySet.TableLogLoan;
                    break;
                case DeviceStatus.Input:
                    logTable = KySet.TableLogInput;
                    break;
                case DeviceStatus.Output:
                    logTable = KySet.TableLogOutput;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(status), status, "插入借出或归还历史记录表时，状态设置异常");
            }
            if (logTable == null) throw new Exception();
            string sqlLogTable =
                $"INSERT INTO {logTable} values ( '{facility.Id}','{facility.Category}','{facility.Name}','{facility.ModelNum}','{facility.Parameter}','{facility.Num}','{user}','{facility.DateTime.ToString("yyyy-MM-dd HH:mm:ss")}','{facility.Price}','{priceTotal}','{facility.Note}')";
            if (sqlLogTable == null)
                throw new Exception();
            ConnBuild();
            var cmdChangeLog = new MySqlCommand(sqlLogTable, _connection);
            cmdChangeLog.ExecuteNonQuery();
            ConnClose();
        }

        /// <summary>
        ///     从数据库库存状态表中出库器件
        /// </summary>
        /// <param name="facility">数据库中已有的原始器件状态信息</param>
        /// <param name="user">出库器件的原操作者</param>
        private static void OutputFacility(Facility facility, string user)
        {
            var rawNum = QueryDeviceNum(facility, user); //查询操作者器件数量
            var remainNum = rawNum - facility.Num; //出库后，剩余库存数量

            //剩余库存数量小于0，抛出异常
            if (remainNum < 0)
                throw new NumBelowZeroException("更改后的当前器件数量不能为负数");
            string sqlStatusTable;
            //剩余库存数量等于0，删除当前器件条目
            if (remainNum == 0)
            {
                sqlStatusTable =
                    $"DELETE FROM {KySet.TableStatusRepertory} WHERE 编号 = '{facility.Id}' AND 类别 = '{facility.Category}' AND 名称 = '{facility.Name}' AND 型号 = '{facility.ModelNum}' AND 规格 = '{facility.Parameter}' AND 操作者 = '{user}'";
            }

            //剩余库存数量大于0，更新当前器件库存数量
            else
            {
                var priceTotalRemain = (facility.Price*remainNum).ToString(CultureInfo.CurrentCulture); //计算剩余器件总价
                sqlStatusTable =
                    $"UPDATE {KySet.TableStatusRepertory} SET 数量 = '{remainNum}', 总价（元） = '{priceTotalRemain}' WHERE 编号 = '{facility.Id}' AND 类别 = '{facility.Category}' AND 名称 = '{facility.Name}' AND 型号 = '{facility.ModelNum}' AND 规格 = '{facility.Parameter}' AND 操作者 = '{user}'";
            }
            ConnBuild();
            if (sqlStatusTable == null)
                throw new Exception();
            Debug.WriteLine("OutputFacility:" + sqlStatusTable);
            var cmdChangeStatus = new MySqlCommand(sqlStatusTable, _connection);
            cmdChangeStatus.ExecuteNonQuery();
            ConnClose();
        }

        /// <summary>
        ///     往数据库库存状态表中入库器件
        /// </summary>
        /// <param name="facility">数据库中已有的原始器件状态信息</param>
        /// <param name="user">入库器件的预计未来操作者</param>
        private static void InputFacility(Facility facility, string user)
        {
            var rawNum = QueryDeviceNum(facility, user); //查询该操作者拥有的该器件数量
            var remainNum = rawNum + facility.Num; //入库后，该操作者拥有的库存数量
            var priceTotalRemain = (facility.Price*remainNum).ToString(CultureInfo.CurrentCulture); //计算该操作者拥有的该器件总价
            Debug.WriteLine("rawNum:" + rawNum + ";remainNum:" + remainNum);
            string sqlStatusTable = null;
            //判断当前所选器件是否为新器件
            if (rawNum > 0) //现操作者的库存中已有该器件
                sqlStatusTable =
                    $"UPDATE {KySet.TableStatusRepertory} SET 数量 = '{remainNum}', 单价（元） = '{facility.Price}', 总价（元） = '{priceTotalRemain}', 备注 = '{facility.Note}' WHERE 编号 = '{facility.Id}' AND 类别 = '{facility.Category}' AND 名称 = '{facility.Name}' AND 型号 = '{facility.ModelNum}' AND 规格 = '{facility.Parameter}' AND 操作者 = '{user}'";
            if (rawNum == 0) //现操作者的库存中没有该器件，是新器件
                sqlStatusTable =
                    $"INSERT INTO {KySet.TableStatusRepertory} values ( '{facility.Id}','{facility.Category}','{facility.Name}','{facility.ModelNum}','{facility.Parameter}','{facility.Num}','{user}','{facility.DateTime.ToString("yyyy-MM-dd HH:mm:ss")}','{facility.Price}','{priceTotalRemain}','{facility.Note}')";
            if (sqlStatusTable == null)
                throw new Exception();
            Debug.WriteLine("InputFacility:" + sqlStatusTable);

            ConnBuild();
            var cmdChangeStatus = new MySqlCommand(sqlStatusTable, _connection);
            cmdChangeStatus.ExecuteNonQuery();
            ConnClose();
        }

        /// <summary>
        ///     从数据库中获取所选器件的数量
        /// </summary>
        /// <param name="facility">所选器件信息</param>
        /// <param name="user">使用者bean</param>
        /// <returns>所选器件的数量</returns>
        private static int QueryDeviceNum(Facility facility, string user)
        {
            var sqlExec =
                $"SELECT 数量 FROM {KySet.TableStatusRepertory} WHERE 类别 = '{facility.Category}' AND 名称 = '{facility.Name}' AND 型号 = '{facility.ModelNum}' AND 规格 = '{facility.Parameter}' AND 操作者 = '{user}'";
            int needNum;
            ConnBuild();
            Debug.WriteLine("QueryDeviceNum:" + sqlExec);
            var comm5 = new MySqlCommand(sqlExec, _connection);
            var reader = comm5.ExecuteReader();
            if (reader.Read())
                needNum = int.Parse(reader["数量"].ToString());
            else
                needNum = 0;
            ConnClose();
            return needNum;
        }
    }
}