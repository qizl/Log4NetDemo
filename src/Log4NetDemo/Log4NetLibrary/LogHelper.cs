using log4net;
using log4net.Config;
using System;

namespace Log4NetLibrary
{
    public class LogHelper
    {
        private static LogHelper _logHelper;
        private static ILog _netlog;

        private LogHelper()
        {
            XmlConfigurator.Configure();
            _netlog = LogManager.Exists("Log4NetTest");
            if (_netlog == null)
                throw new Exception("无法从App.congfig中读取log4net的初始化配置");
        }

        public static void Initialize()
        {
            if (_logHelper == null)
                _logHelper = new LogHelper();
        }

        public static void WriteInfo(Log log)
        { _netlog.Info(log); }
    }
}
