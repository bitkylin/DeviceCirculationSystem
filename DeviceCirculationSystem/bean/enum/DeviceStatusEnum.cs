namespace DeviceCirculationSystem.bean.@enum
{
    public enum DeviceStatus
    {
        /// <summary>
        /// 设备归还
        /// </summary>
        RETURN,

        /// <summary>
        /// 设备借出
        /// </summary>
        LOAN,

        /// <summary>
        /// 设备入库
        /// </summary>
        INPUT,

        /// <summary>
        /// 设备出库
        /// </summary>
        OUTPUT,

        /// <summary>
        /// 默认状态，设备当前存在库中
        /// </summary>
        EXIST
    }
}