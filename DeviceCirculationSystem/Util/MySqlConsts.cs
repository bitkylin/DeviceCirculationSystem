namespace DeviceCirculationSystem.Util
{
    public static class MySqlConsts
    {
        /// <summary>
        /// 设备归还历史记录表
        /// </summary>
        public const string TABLE_LOG_RETURN = "facility_return_log";

        /// <summary>
        /// 设备借出历史记录表
        /// </summary>
        public const string TABLE_LOG_LOAN = "facility_loan_log";

        /// <summary>
        /// 设备入库历史记录表
        /// </summary>
        public const string TABLE_LOG_INPUT = "facility_input_log";

        /// <summary>
        /// 设备出库历史记录表
        /// </summary>
        public const string TABLE_LOG_OUTPUT = "facility_output_log";

        /// <summary>
        /// 设备当前库存记录表
        /// </summary>
        public const string TABLE_STATUS_REPERTORY = "facility_repertory_status";

        /// <summary>
        /// 用户权限管理表
        /// </summary>
        public const string TABLE_USER_PERMISSION = "user_permission";

        /// <summary>
        /// 用户权限管理表[整合表]
        /// </summary>
        public const string TABLE_USER_PERMISSION_WORK_MANAGER = "tab_name";


        /// <summary>
        /// 服务器IP
        /// </summary>
        public const string SERVER_IP = "172.16.64.73";

        /// <summary>
        /// 数据库登录账号
        /// </summary>
        public const string SERVER_USER_NAME = "everyone";

        /// <summary>
        /// 数据库登录密码
        /// </summary>
        public const string SERVER_PASSWORD = "123456";

        /// <summary>
        /// 设备自助借还系统数据库
        /// </summary>
        public const string SERVER_DATABASE = "bitky_device_circulation_system";

        /// <summary>
        /// 日志系统数据库
        /// </summary>
        public const string SERVER_DATABASE_WORK_MANAGER = "work_manager";
    }
}