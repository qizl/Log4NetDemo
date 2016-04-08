using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using System.IO;

using log4net.Core;
//using log4net.Layout.Pattern;
using log4net.Util;
using log4net.Layout;
using log4net.Layout.Pattern;

namespace Log4NetExtension
{
    public class Log4NetCustomLayout : log4net.Layout.LayoutSkeleton
    {
        #region Constants

        /// <summary>
        /// Default pattern string for log output. 
        /// </summary>
        /// <remarks>
        /// <para>
        /// Default pattern string for log output. 
        /// Currently set to the string <b>"%message%newline"</b> 
        /// which just prints the application supplied message. 
        /// </para>
        /// </remarks>
        public const string DefaultConversionPattern = "";

        /// <summary>
        /// A detailed conversion pattern
        /// </summary>
        /// <remarks>
        /// <para>
        /// A conversion pattern which includes Time, Thread, Logger, and Nested Context.
        /// Current value is <b>%timestamp [%thread] %level %logger %ndc - %message%newline</b>.
        /// </para>
        /// </remarks>
        public const string DetailConversionPattern = "%timestamp [%thread] %level %logger %ndc - %message%newline";

        #endregion

        #region Static Fields

        /// <summary>
        /// Internal map of converter identifiers to converter types.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This static map is overridden by the m_converterRegistry instance map
        /// </para>
        /// </remarks>
        private static Hashtable s_globalRulesRegistry;

        #endregion Static Fields

        #region Member Variables

        /// <summary>
        /// the pattern
        /// </summary>
        private string m_pattern;

        /// <summary>
        /// the head of the pattern converter chain
        /// </summary>
        private PatternConverter m_head;

        /// <summary>
        /// patterns defined on this PatternLayout only
        /// </summary>
        private Hashtable m_instanceRulesRegistry = new Hashtable();

        #endregion

        #region Static Constructor

        /// <summary>
        /// Initialize the global registry
        /// </summary>
        /// <remarks>
        /// <para>
        /// Defines the builtin global rules.
        /// </para>
        /// </remarks>
        static Log4NetCustomLayout()
        {
            s_globalRulesRegistry = new Hashtable(1);
            s_globalRulesRegistry.Add("sStaffValue", typeof(sStaffValuePatternConverter));
        }

        #endregion Static Constructor

        #region Constructors

        /// <summary>
        /// Constructs a PatternLayout using the DefaultConversionPattern
        /// </summary>
        /// <remarks>
        /// <para>
        /// The default pattern just produces the application supplied message.
        /// </para>
        /// <para>
        /// Note to Inheritors: This constructor calls the virtual method
        /// <see cref="CreatePatternParser"/>. If you override this method be
        /// aware that it will be called before your is called constructor.
        /// </para>
        /// <para>
        /// As per the <see cref="IOptionHandler"/> contract the <see cref="ActivateOptions"/>
        /// method must be called after the properties on this object have been
        /// configured.
        /// </para>
        /// </remarks>
        public Log4NetCustomLayout()
            : this(DefaultConversionPattern)
        {
        }

        /// <summary>
        /// Constructs a PatternLayout using the supplied conversion pattern
        /// </summary>
        /// <param name="pattern">the pattern to use</param>
        /// <remarks>
        /// <para>
        /// Note to Inheritors: This constructor calls the virtual method
        /// <see cref="CreatePatternParser"/>. If you override this method be
        /// aware that it will be called before your is called constructor.
        /// </para>
        /// <para>
        /// When using this constructor the <see cref="ActivateOptions"/> method 
        /// need not be called. This may not be the case when using a subclass.
        /// </para>
        /// </remarks>
        public Log4NetCustomLayout(string pattern)
        {
            // By default we do not process the exception
            IgnoresException = true;

            m_pattern = pattern;
            if (m_pattern == null)
            {
                m_pattern = DefaultConversionPattern;
            }

            ActivateOptions();
        }

        #endregion

        /// <summary>
        /// The pattern formatting string
        /// </summary>
        /// <remarks>
        /// <para>
        /// The <b>ConversionPattern</b> option. This is the string which
        /// controls formatting and consists of a mix of literal content and
        /// conversion specifiers.
        /// </para>
        /// </remarks>
        public string ConversionPattern
        {
            get { return m_pattern; }
            set { m_pattern = value; }
        }

        /// <summary>
        /// Create the pattern parser instance
        /// </summary>
        /// <param name="pattern">the pattern to parse</param>
        /// <returns>The <see cref="PatternParser"/> that will format the event</returns>
        /// <remarks>
        /// <para>
        /// Creates the <see cref="PatternParser"/> used to parse the conversion string. Sets the
        /// global and instance rules on the <see cref="PatternParser"/>.
        /// </para>
        /// </remarks>
        virtual protected PatternParser CreatePatternParser(string pattern)
        {
            PatternParser patternParser = new PatternParser(pattern);

            // Add all the builtin patterns
            foreach (DictionaryEntry entry in s_globalRulesRegistry)
            {
                patternParser.PatternConverters[entry.Key] = entry.Value;
            }
            // Add the instance patterns
            foreach (DictionaryEntry entry in m_instanceRulesRegistry)
            {
                patternParser.PatternConverters[entry.Key] = entry.Value;
            }

            return patternParser;
        }

        #region Implementation of IOptionHandler

        /// <summary>
        /// Initialize layout options
        /// </summary>
        /// <remarks>
        /// <para>
        /// This is part of the <see cref="IOptionHandler"/> delayed object
        /// activation scheme. The <see cref="ActivateOptions"/> method must 
        /// be called on this object after the configuration properties have
        /// been set. Until <see cref="ActivateOptions"/> is called this
        /// object is in an undefined state and must not be used. 
        /// </para>
        /// <para>
        /// If any of the configuration properties are modified then 
        /// <see cref="ActivateOptions"/> must be called again.
        /// </para>
        /// </remarks>
        override public void ActivateOptions()
        {
            if (string.IsNullOrEmpty(m_pattern))
                return;
            m_head = CreatePatternParser(m_pattern).Parse();
            PatternConverter curConverter = m_head;

            //m_head = CreatePatternParser(m_pattern).PatternConverters[
            //PatternConverter curConverter = m_head;
            while (curConverter != null)
            {
                PatternLayoutConverter layoutConverter = curConverter as PatternLayoutConverter;
                if (layoutConverter != null)
                {
                    if (!layoutConverter.IgnoresException)
                    {
                        // Found converter that handles the exception
                        this.IgnoresException = false;

                        break;
                    }
                }
                curConverter = curConverter.Next;
            }
        }

        #endregion

        #region Override implementation of LayoutSkeleton

        /// <summary>
        /// Produces a formatted string as specified by the conversion pattern.
        /// </summary>
        /// <param name="loggingEvent">the event being logged</param>
        /// <param name="writer">The TextWriter to write the formatted event to</param>
        /// <remarks>
        /// <para>
        /// Parse the <see cref="LoggingEvent"/> using the patter format
        /// specified in the <see cref="ConversionPattern"/> property.
        /// </para>
        /// </remarks>
        override public void Format(TextWriter writer, LoggingEvent loggingEvent)
        {
            if (writer == null)
            {
                throw new ArgumentNullException("writer");
            }
            if (loggingEvent == null)
            {
                throw new ArgumentNullException("loggingEvent");
            }

            PatternConverter c = m_head;

            // loop through the chain of pattern converters
            while (c != null)
            {
                c.Format(writer, loggingEvent);
                c = c.Next;
            }
        }

        #endregion

        /// <summary>
        /// Add a converter to this PatternLayout
        /// </summary>
        /// <param name="converterInfo">the converter info</param>
        /// <remarks>
        /// <para>
        /// This version of the method is used by the configurator.
        /// Programmatic users should use the alternative <see cref="AddConverter(string,Type)"/> method.
        /// </para>
        /// </remarks>
        public void AddConverter(ConverterInfo converterInfo)
        {
            AddConverter(converterInfo.Name, converterInfo.Type);
        }

        /// <summary>
        /// Add a converter to this PatternLayout
        /// </summary>
        /// <param name="name">the name of the conversion pattern for this converter</param>
        /// <param name="type">the type of the converter</param>
        /// <remarks>
        /// <para>
        /// Add a named pattern converter to this instance. This
        /// converter will be used in the formatting of the event.
        /// This method must be called before <see cref="ActivateOptions"/>.
        /// </para>
        /// <para>
        /// The <paramref name="type"/> specified must extend the 
        /// <see cref="PatternConverter"/> type.
        /// </para>
        /// </remarks>
        public void AddConverter(string name, Type type)
        {
            if (name == null) throw new ArgumentNullException("name");
            if (type == null) throw new ArgumentNullException("type");

            if (!typeof(PatternConverter).IsAssignableFrom(type))
            {
                throw new ArgumentException("The converter type specified [" + type + "] must be a subclass of log4net.Util.PatternConverter", "type");
            }
            m_instanceRulesRegistry[name] = type;
        }

        /// <summary>
        /// Wrapper class used to map converter names to converter types
        /// </summary>
        /// <remarks>
        /// <para>
        /// Pattern converter info class used during configuration to
        /// pass to the <see cref="PatternLayout.AddConverter(ConverterInfo)"/>
        /// method.
        /// </para>
        /// </remarks>
        public sealed class ConverterInfo
        {
            private string m_name;
            private Type m_type;

            /// <summary>
            /// default constructor
            /// </summary>
            public ConverterInfo()
            {
            }

            /// <summary>
            /// Gets or sets the name of the conversion pattern
            /// </summary>
            /// <remarks>
            /// <para>
            /// The name of the pattern in the format string
            /// </para>
            /// </remarks>
            public string Name
            {
                get { return m_name; }
                set { m_name = value; }
            }

            /// <summary>
            /// Gets or sets the type of the converter
            /// </summary>
            /// <remarks>
            /// <para>
            /// The value specified must extend the 
            /// <see cref="PatternConverter"/> type.
            /// </para>
            /// </remarks>
            public Type Type
            {
                get { return m_type; }
                set { m_type = value; }
            }
        }
    }
    internal sealed class sStaffValuePatternConverter : PatternLayoutConverter
    {
        override protected void Convert(TextWriter writer, LoggingEvent loggingEvent)
        {
            Log log = loggingEvent.MessageObject as Log;
            if (log != null)
                writer.Write(log.sStaffValue);
        }
    }
}
