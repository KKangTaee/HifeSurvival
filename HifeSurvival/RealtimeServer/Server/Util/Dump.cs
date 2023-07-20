using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace Server
{
    public static class Dump
    {
        public static void UnhandledExceptionHandler(object sender, UnhandledExceptionEventArgs e)
        {
            Exception exception = e.ExceptionObject as Exception;
            Logger.Instance.Error($"CRASH !! {exception.Message} \n{exception.StackTrace}");
            CreateDump(exception);
        }

        [Flags]
        enum MiniDumpType
        {
            WithFullMemory = 2,
            WithHandleData = 4,
            WithDataSegs = 8
        }

        [DllImport("DbgHelp.dll", SetLastError = true)]
        static extern bool MiniDumpWriteDump(IntPtr hProcess, uint processId, IntPtr hFile,
                                     MiniDumpType dumpType, IntPtr exceptionParam,
                                     IntPtr userStreamParam, IntPtr callbackParam);

        static void CreateDump(Exception exception)
        {
            try
            {
                var time = DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss");
                var dumpFileName = $"Dump {time}.dmp";
                using (var process = Process.GetCurrentProcess())
                {
                    using (var dumpFile = new FileStream(dumpFileName, FileMode.Create))
                    {
                        MiniDumpWriteDump(process.Handle,
                            (uint)process.Id,
                            dumpFile.SafeFileHandle.DangerousGetHandle(),
                            MiniDumpType.WithFullMemory,
                            IntPtr.Zero,
                            IntPtr.Zero,
                            IntPtr.Zero
                            );
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Instance.Error($"Failed while Making CRASH DUMP !! {ex.Message}");
            }
        }
    }
}
