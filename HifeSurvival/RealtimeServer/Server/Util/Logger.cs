using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.Json;

namespace Server
{
    public class Logger
    {
        private object _lock = new object();
        private FileStream _fs = null;
        private StreamWriter _sw = null;

        private static Logger _ins = null;
        private string _titleName;
        private bool _bConsoleWirte = true; // TODO: Config
        private JsonSerializerOptions _jsonSerializerOptions = new JsonSerializerOptions()
        {
            WriteIndented = true,
        };

        public static Logger Instance
        {
            get
            {
                _ins ??= new Logger();
                return _ins;
            }
        }

        private Logger()
        {
            _titleName = System.Reflection.Assembly.GetEntryAssembly().GetName().Name;
            CreateLogFile();
        }

        private void CreateLogFile()
        {
            lock (_lock)
            {
                string title = _titleName + DateTime.Now.ToString(" yyyy-MM-dd-HH-mm-ss");

                if (Directory.Exists("logs") == false)
                {
                    Directory.CreateDirectory("logs");
                }

                _fs = new FileStream($"logs\\{title}.log", FileMode.CreateNew);
                _sw = new StreamWriter(_fs, Encoding.UTF8);
            }
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
            _sw.WriteLine(logMsg);

            if(_bConsoleWirte)
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

            _sw.Flush();
            _fs.Flush();

            if (_fs.Length > 8_388_608)    //8MB
            {
                _sw.Close();
                _fs.Close();
                CreateLogFile();
            }
        }

        public void Trace(IPacket packet)
        {
            string trace = $"{JsonSerializer.Serialize(packet, packet.GetType(), _jsonSerializerOptions)}";
            _sw.WriteLine(trace);

            _sw.Flush();
            _fs.Flush();

            if (_fs.Length > 8_388_608)    //8MB
            {
                _sw.Close();
                _fs.Close();
                CreateLogFile();
            }
        }


        public void DataCheckInfo(string message)
        {
            Log("INF", message, "DataCheck");
        }

        public void DataCheckError(string message)
        {
            Log("ERR", message, "DataCheck");
        }
    }
}
