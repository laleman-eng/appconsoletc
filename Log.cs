using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.CompilerServices;
using System.Text;

namespace AppConsoleTC
{
    public class Log
    {
        public static void save(object obj, Exception ex)
        {
            try
            {
                string fecha = System.DateTime.Now.ToString("yyyy-MM-dd");
                string hora = System.DateTime.Now.ToString("HH:mm:ss");
                string path = System.Reflection.Assembly.GetExecutingAssembly().Location;
                string directory = System.IO.Path.GetDirectoryName(path) + "/TClog.txt";
                StreamWriter sw = new StreamWriter(directory, true);
                StackTrace stacktrace = new StackTrace();
                sw.WriteLine(fecha + ":" + hora + " " + obj.GetType().FullName);
                sw.WriteLine(stacktrace.GetFrame(1).GetMethod().Name + " - " + ex.Message);
                sw.WriteLine("");
                sw.Flush();
                sw.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine("Log Exeption " + e.Message + " " + e.StackTrace);
            }
        }

        public static void save(string message)
        {
            try
            {
                string fecha = System.DateTime.Now.ToString("yyyy-MM-dd");
                string hora = System.DateTime.Now.ToString("HH:mm:ss");
                string path = System.Reflection.Assembly.GetExecutingAssembly().Location;
                string directory = System.IO.Path.GetDirectoryName(path)+"/TClog.txt";
                StreamWriter sw = new StreamWriter(directory, true);
                sw.WriteLine(fecha+":" +hora + " " + message);
                sw.Flush();
                sw.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine("Log Exeption " + e.Message + " " + e.StackTrace);
            }

        }
    }
}
