using log4net;
using log4net.Config;
using System;
using System.IO;

namespace Log4NetExtension
{
    public class LogHelper
    {
        private static log4net.ILog loginfo = log4net.LogManager.GetLogger("Log4NetTest");

        public static void Initialize()
        {
            XmlConfigurator.Configure();
        }
        public static void Initialize(string path)
        {
            XmlConfigurator.ConfigureAndWatch(new FileInfo(path));
        }

        #region WriteInfo
        public static void WriteInfo(Log obj)
        { if (loginfo.IsInfoEnabled)loginfo.Info(obj); }

        /// <summary/>
        /// <param name="message"></param>
        /// <param name="ip"></param>
        /// <param name="operatorID">操作人ID</param>
        public static void WriteInfo(string message, string ip, long staffValue)
        { WriteInfo(new Log() { sStaffValue = staffValue, sMessage = message, sIP = ip }); }

        /// <summary/>
        /// <param name="message"></param>
        /// <param name="resultCount">查询数据集数量</param>
        /// <param name="ip"></param>
        /// <param name="staffValue"></param>
        public static void WriteInfo(string message, int recordCount, string ip, long staffValue)
        { WriteInfo(new Log() { sStaffValue = staffValue, sMessage = message + ",View Record Count:" + recordCount, sIP = ip }); }

        public static void WriteInfo(string message)
        { WriteInfo(new Log() { sMessage = message }); }
        #endregion

        #region WriteError
        public static void WriteError(Log log)
        { if (loginfo.IsErrorEnabled)loginfo.Error(log); }

        public static void WriteError(string message, string ip, long staffValue)
        { WriteError(new Log() { sStaffValue = staffValue, sMessage = message, sIP = ip }); }

        public static void WriteError(string message, Exception ex, string ip, long staffValue)
        { WriteError(new Log() { sStaffValue = staffValue, sMessage = message, sException = GetExceptionMessage(ex) }); }

        public static void WriteError(string message, Exception ex)
        { WriteError(new Log() { sMessage = message, sException = GetExceptionMessage(ex) }); }
        #endregion

        #region WriteDebug
        public static void WriteDebug(Log log)
        { if (loginfo.IsErrorEnabled)loginfo.Debug(log); }

        public static void WriteDebug(string message, string ip, long staffValue)
        { WriteDebug(new Log() { sStaffValue = staffValue, sMessage = message, sIP = ip }); }

        public static void WriteDebug(string message, Exception ex, string ip, long staffValue)
        { WriteDebug(new Log() { sStaffValue = staffValue, sMessage = message, sException = GetExceptionMessage(ex) }); }

        public static void WriteDebug(string message, Exception ex)
        { WriteDebug(new Log() { sMessage = message, sException = GetExceptionMessage(ex) }); }
        #endregion

        private static string GetExceptionMessage(Exception ex)
        {
            string errorMsg = string.Format("异常类型：{0} 异常信息：{1} 堆栈调用：{2}",
                new object[] { ex.GetType().Name, ex.Message, ex.StackTrace });
            errorMsg = errorMsg.Replace("\r\n", "");
            errorMsg = errorMsg.Replace("位置", "<strong style=\"color:red\">位置</strong>");
            return errorMsg;
        }
    }
}
