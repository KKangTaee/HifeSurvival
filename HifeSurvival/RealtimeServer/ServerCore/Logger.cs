using System;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace ServerCore
{
    public class Logger
    {
        FileStream fs = null;
        StreamWriter sw = null;

        private static Logger ins;
        private string titleName;
        private bool bConsoleWirte = true; // TODO: Config

        public static Logger GetInstance()
        {
            if(ins == null)
                ins = new Logger();
            return ins;
        }

        private Logger()
        {
            titleName = System.Reflection.Assembly.GetEntryAssembly().GetName().Name;
            CreateLogFile();
        }

        private void CreateLogFile()
        {
            string title = titleName + DateTime.Now.ToString(" yyyy-MM-dd-HH-mm-ss");

            if (Directory.Exists("logs") == false)
                Directory.CreateDirectory("logs");

            fs = new FileStream($"logs\\{title}.log", FileMode.CreateNew);
            sw = new StreamWriter(fs, Encoding.UTF8);
        }

        public void Debug(string message)
        {
            Log("DBG", message);
        }
        public void Info(string message)
        {
            Log("INF", message);
        }
        public void Error(string message)
        {
            Log("ERR", message);
        }
        public void Warn(string message)
        {
            Log("WRN", message);
        }


        public void Log(string verbose, string message, string manualMethodName = "")
        {
            var stackFrame = new StackFrame(2, true);

            var methodName = manualMethodName == "" ? stackFrame.GetMethod().Name : manualMethodName;

            string logMsg = $"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff")}   {verbose}   {methodName} - {message}";
            sw.WriteLine(logMsg);

            if(bConsoleWirte)
            {
                Console.ResetColor();
                switch(verbose)
                {
                    case "INF":
                        break;
                    case "DBG":
                        Console.ForegroundColor = ConsoleColor.DarkCyan;
                        break;
                    case "WRN":
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        break;
                    case "ERR":
                        Console.ForegroundColor = ConsoleColor.Red;
                        break;
                    default:
                        break;
                }

                Console.WriteLine(logMsg);
                Console.ResetColor();
            }

            sw.Flush();
            fs.Flush();

            if (fs.Length > 8_388_608)    //8MB
            {
                sw.Close();
                fs.Close();
                CreateLogFile();
            }
        }
    }
}
