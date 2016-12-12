using System;
using System.Data;
using System.Diagnostics;
using DeviceCirculationSystem.bean;
using DeviceCirculationSystem.bean.@enum;

namespace DeviceCirculationSystem.Util
{
    internal class RepositoryPresenter
    {
        /// <summary>
        ///     查询库存情况表
        /// </summary>
        /// <param name="facility">包含用户名，设备类别</param>
        /// <returns></returns>
        public static DataTable QueryStorageLimitUser(Facility facility)
        {
            Debug.WriteLine("facility.Category:"+ facility.Category+ ";facility.OwnUser:" + facility.OwnUser);
            return BitkyMySql.QueryStorageLimitUser(facility.Category, facility.OwnUser, KySet.TableStatusRepertory);
        }

        /// <summary>
        ///     查询借出或归还情况表
        /// </summary>
        /// <param name="facility">包含用户名，设备类别</param>
        /// <returns></returns>
        public DataTable QueryDeviceInputOutputLog(Facility facility)
        {
            switch (facility.Status)
            {
                case DeviceStatus.Loan:
                    return BitkyMySql.QueryStorageLimitUser(facility.Category, facility.OwnUser, KySet.TableLogLoan);
                case DeviceStatus.Return:
                    return BitkyMySql.QueryStorageLimitUser(facility.Category, facility.OwnUser, KySet.TableLogReturn);
            }
            throw new Exception("查询借出或归还情况表异常，设置有误");
        }
    }
}