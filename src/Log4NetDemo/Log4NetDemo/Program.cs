using Log4NetLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Log4NetDemo
{
    class Program
    {
        static void Main(string[] args)
        {
            LogHelper.Initialize();
            LogHelper.WriteInfo(new Log() { sStaffValue = 123 });
        }
    }
}
