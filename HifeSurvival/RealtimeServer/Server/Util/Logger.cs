using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.Json;

namespace Server
{
    public class Logger
    {
        FileStream fs = null;
        StreamWriter sw = null;

        private static Logger _ins;
        private string _titleName;
        private bool _bConsoleWirte = true; // TODO: Config
        private JsonSerializerOptions _jsonSerializerOptions = new JsonSerializerOptions()
        {
            WriteIndented = true,
        };

        public static Logger GetInstance()
        {
            _ins ??= new Logger();
            return _ins;
        }

        private Logger()
        {
            _titleName = System.Reflection.Assembly.GetEntryAssembly().GetName().Name;
            CreateLogFile();
        }

        private void CreateLogFile()
        {
            string title = _titleName + DateTime.Now.ToString(" yyyy-MM-dd-HH-mm-ss");

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

            if (_bConsoleWirte)
            {
                Console.ResetColor();
                switch (verbose)
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


        public void Trace(IPacket packet)
        {
            string trace = $"{JsonSerializer.Serialize(packet, packet.GetType(), _jsonSerializerOptions)}";
            sw.WriteLine(trace);

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
