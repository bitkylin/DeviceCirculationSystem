using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Windows.Documents;
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
                case DeviceStatus.Input:
                    return BitkyMySql.QueryStorageLimitUser(facility.Category, facility.OwnUser, KySet.TableLogInput);
                case DeviceStatus.Output:
                    return BitkyMySql.QueryStorageLimitUser(facility.Category, facility.OwnUser, KySet.TableLogOutput);
            }
            throw new Exception("查询借出或归还情况表异常，设置有误");
        }

        public static List<string> QueryUserNameAll()
        {
            return BitkyMySql.queryUserNameAll();
        }

        public static List<string> GetDefaultDevices()
        {
            var defaultDeviceist = new List<string> {"全部", "图书", "电脑", "元器件", "开发工具"};

            var newDeviceList = BitkyMySql.queryDistinctDevice();
            newDeviceList.ForEach(str =>
            {
                var isNew = true;
                defaultDeviceist.ForEach(strDefault =>
                {
                    if (str.Equals(strDefault))
                    {
                        isNew = false;
                    }
                });
                if (isNew)
                {
                    defaultDeviceist.Add(str);
                }
            });

            return defaultDeviceist;
        }
    }
}
