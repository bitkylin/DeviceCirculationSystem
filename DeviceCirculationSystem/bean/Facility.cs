using System;
using DeviceCirculationSystem.bean.@enum;

namespace DeviceCirculationSystem.bean
{
    public class Facility
    {
        public Facility(DeviceStatus status)
        {
            Status = status;
        }

        /// <summary>
        ///     编号
        /// </summary>
        public string Id { set; get; }

        /// <summary>
        ///     类别
        /// </summary>
        public string Category { set; get; }

        /// <summary>
        ///     名称
        /// </summary>
        public string Name { set; get; }

        /// <summary>
        ///     型号
        /// </summary>
        public string ModelNum { set; get; }

        /// <summary>
        ///     规格
        /// </summary>
        public string Parameter { set; get; }

        /// <summary>
        ///     数量
        /// </summary>
        public int Num { set; get; }

        /// <summary>
        ///     当前操作者
        /// </summary>
        public string OwnUser { set; get; }

        /// <summary>
        ///     目标(未来)操作者
        /// </summary>
        public string ToUser { set; get; }

        /// <summary>
        ///     日期时间
        /// </summary>
        public DateTime DateTime { set; get; }

        /// <summary>
        ///     价格
        /// </summary>
        public double Price { set; get; }

        /// <summary>
        ///     备注
        /// </summary>
        public string Note { set; get; }

        /// <summary>
        ///     设备状态
        /// </summary>
        public DeviceStatus Status { private set; get; }
    }
}