using System;
using System.Data;
using DeviceCirculationSystem.bean;
using DeviceCirculationSystem.bean.@enum;

namespace DeviceCirculationSystem.Util
{
    class RepositoryPresenter
    {
        /// <summary>
        /// 查询库存情况表
        /// </summary>
        /// <param name="facility">包含用户名，设备名</param>
        /// <returns></returns>
        public static DataTable QueryStorageLimitUser(Facility facility)
        {
            if (facility.OwnUser == "")
                return BitkyMySql.QueryStorageUnlimitUser(facility.Name, BitkyMySql.TableStatusRepertory);
            else
                return BitkyMySql.QueryStorageLimitUser(facility.Name, facility.OwnUser,
                    BitkyMySql.TableStatusRepertory);
        }

        /// <summary>
        /// 查询借出或归还情况表
        /// </summary>
        /// <param name="facility">包含用户名，设备名</param>
        /// <returns></returns>
        public DataTable QueryDeviceInputOutputLog(Facility facility)
        {
            if (facility.Status == DeviceStatus.Output)
                return BitkyMySql.QueryStorageLimitUser(facility.Name, facility.OwnUser, BitkyMySql.TableLogOutput);
            if (facility.Status == DeviceStatus.Input)
                return BitkyMySql.QueryStorageLimitUser(facility.Name, facility.OwnUser, BitkyMySql.TableLogInput);
            throw new Exception("查询借出或归还情况表异常，设置有误");
        }
    }
}