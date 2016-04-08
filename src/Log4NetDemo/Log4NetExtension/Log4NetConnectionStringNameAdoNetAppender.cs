using System;
using log4net.Appender;
using log4net;
using System.Configuration;

namespace Log4NetExtension
{
    /// <summary>
    /// http://issues.apache.org/jira/browse/LOG4NET-88
    /// An appender for Log4Net that uses a database based on the connection string name.
    /// </summary>
    public class Log4NetConnectionStringNameAdoNetAppender : AdoNetAppender
    {
        private static ILog _Log;

        /// <summary>
        /// Gets the log.
        /// </summary>
        /// <value>The log.</value>
        protected static ILog Log
        {
            get
            {
                if (_Log == null)
                    _Log = LogManager.GetLogger(typeof(Log4NetConnectionStringNameAdoNetAppender));
                return _Log;
            }
        }

        private string _ConnectionStringName;

        /// <summary>
        /// Initialize the appender based on the options set
        /// </summary>
        /// <remarks>
        /// <para>
        /// This is part of the <see cref="T:log4net.Core.IOptionHandler"/> delayed object
        /// activation scheme. The <see cref="M:log4net.Appender.AdoNetAppender.ActivateOptions"/> method must
        /// be called on this object after the configuration properties have
        /// been set. Until <see cref="M:log4net.Appender.AdoNetAppender.ActivateOptions"/> is called this
        /// object is in an undefined state and must not be used.
        /// </para>
        /// <para>
        /// If any of the configuration properties are modified then
        /// <see cref="M:log4net.Appender.AdoNetAppender.ActivateOptions"/> must be called again.
        /// </para>
        /// </remarks>
        public override void ActivateOptions()
        {
            PopulateConnectionString();
            base.ActivateOptions();
        }

        /// <summary>
        /// Populates the connection string.
        /// </summary>
        private void PopulateConnectionString()
        {
            // if connection string already defined, do nothing
            if (!String.IsNullOrEmpty(ConnectionString)) return;

            // if connection string name is not available, do nothing
            if (String.IsNullOrEmpty(ConnectionStringName)) return;

            // grab connection string settings
            ConnectionStringSettings settings = ConfigurationManager.ConnectionStrings[ConnectionStringName];

            // if connection string name was not found in settings
            if (settings == null)
            {
                // log error
                if (Log.IsErrorEnabled)
                    Log.ErrorFormat("Connection String Name not found in Configuration: {0}",
                        ConnectionStringName);
                // do nothing more
                return;
            }

            // retrieve connection string from the name
            ConnectionString = settings.ConnectionString;
        }

        /// <summary>
        /// Gets or sets the name of the connection string.
        /// </summary>
        /// <value>The name of the connection string.</value>
        public string ConnectionStringName
        {
            get { return _ConnectionStringName; }
            set { _ConnectionStringName = value; }
        }
    }
}
