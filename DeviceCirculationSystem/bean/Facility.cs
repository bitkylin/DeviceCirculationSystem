using System;
using DeviceCirculationSystem.bean.@enum;

namespace DeviceCirculationSystem.bean
{
    public class Facility
    {
        public Facility(DeviceStatus status)
        {
            this.status = status;
        }

        /// <summary>
        ///     编号
        /// </summary>
        public string id { set; get; }

        /// <summary>
        ///     类别
        /// </summary>
        public string category { set; get; }

        /// <summary>
        ///     名称
        /// </summary>
        public string name { set; get; }

        /// <summary>
        ///     型号
        /// </summary>
        public string modelNum { set; get; }

        /// <summary>
        ///     规格
        /// </summary>
        public string parameter { set; get; }

        /// <summary>
        ///     数量
        /// </summary>
        public int num { set; get; }

        /// <summary>
        ///     当前操作者
        /// </summary>
        public string ownUser { set; get; }

        /// <summary>
        ///     目标(未来)操作者
        /// </summary>
        public string toUser { set; get; }

        /// <summary>
        ///     日期时间
        /// </summary>
        public DateTime dateTime { set; get; }

        /// <summary>
        ///     价格
        /// </summary>
        public double price { set; get; }

        /// <summary>
        ///     备注
        /// </summary>
        public string note { set; get; }

        /// <summary>
        ///     设备状态
        /// </summary>
        public DeviceStatus status { private set; get; }
    }
}