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
        public static DataTable queryStorageLimitUser(Facility facility)
        {
            return KyMySql.queryStorageLimitUser(facility.category, facility.ownUser, KySet.TableStatusRepertory);
        }

        /// <summary>
        ///     查询借出或归还情况表
        /// </summary>
        /// <param name="facility">包含用户名，设备类别</param>
        /// <returns></returns>
        public DataTable queryDeviceInputOutputLog(Facility facility)
        {
            switch (facility.status)
            {
                case DeviceStatus.LOAN:
                    return KyMySql.queryStorageLimitUser(facility.category, facility.ownUser, KySet.TableLogLoan);
                case DeviceStatus.RETURN:
                    return KyMySql.queryStorageLimitUser(facility.category, facility.ownUser, KySet.TableLogReturn);
                case DeviceStatus.INPUT:
                    return KyMySql.queryStorageLimitUser(facility.category, facility.ownUser, KySet.TableLogInput);
                case DeviceStatus.OUTPUT:
                    return KyMySql.queryStorageLimitUser(facility.category, facility.ownUser, KySet.TableLogOutput);
            }
            throw new Exception("查询借出或归还情况表异常，设置有误");
        }

        public static List<string> queryUserNameAll()
        {
            return KyMySql.queryUserNameAll();
        }

        public static List<string> getDefaultDevices()
        {
            var defaultDeviceist = new List<string> {"全部", "图书", "电脑", "元器件", "开发工具"};

            var newDeviceList = KyMySql.queryDistinctDevice();
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
