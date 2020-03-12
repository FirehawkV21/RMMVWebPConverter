using RMMVWebPConverter.Properties;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;

namespace RMMVWebPConverter
{
    class Program
    {
        private static string _converterLocation;
        private static string _sourceLocation;
        private static string _dropLocation;
        private static int _compressionMode;
        private static bool _settingsSet;
        private static readonly StringBuilder StringBuffer = new StringBuilder();
        private const string LosslessConversionSetup = " -z 9 -sharp_yuv -mt -quiet -o ";
        private static readonly StringBuilder LossyConversionSetup = new StringBuilder(" -m 6 -q 85 -sharp_yuv -mt -quiet -o ");
        private static readonly ProcessStartInfo ConverterInfo = new ProcessStartInfo();

        static void Main(string[] args)
        {
            Console.WriteLine(Resources.SpliterText);
            Console.WriteLine(Resources.ProgramTitle);
            Console.WriteLine(Resources.ProgramVersion, Assembly.GetExecutingAssembly().GetName().Version);
            Console.WriteLine(Resources.ProgramAuthor);
            Console.WriteLine(Resources.ProgramLicense);
            Console.WriteLine(Resources.SpliterText);
            Console.WriteLine();

            for (int i = 0; i < args.Length; i++)
            {
                switch (args[i])
                {
                    case "--ConverterLocation":
                        if (i < args.Length - 1 && !args[i + 1].Contains("--"))
                        {
                            StringBuffer.Insert(0, args[i + 1]);
                            StringBuffer.Replace("\"", "");
                            if (Directory.Exists(StringBuffer.ToString()))
                            {
                                _converterLocation = StringBuffer.ToString();
                                StringBuffer.Clear();
                            }
                            else
                            {
                                Console.ForegroundColor = ConsoleColor.DarkRed;
                                Console.WriteLine(Resources.FolderDoesntExist);
                                Console.ResetColor();
                                Console.WriteLine(Resources.PushEnterToExit);
                                Console.ReadLine();
                                Environment.Exit(0);
                            }

                            if (File.Exists(Path.Combine(_converterLocation,
                                RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "cwebp.exe" : "cwebp")))
                            {
                                Console.ForegroundColor = ConsoleColor.DarkGreen;
                                Console.WriteLine(Resources.ConverterLocationSet);
                            }
                            else
                            {

                                Console.ForegroundColor = ConsoleColor.DarkRed;
                                Console.WriteLine(Resources.MissingcwebpApp);
                                Console.ResetColor();
                                Console.WriteLine(Resources.PushEnterToExit);
                                Console.ReadLine();
                                Environment.Exit(0);
                            }
                        }

                        break;
                    case "--SourceLocation":
                        if (i < args.Length - 1 && !args[i + 1].Contains("--"))
                        {
                            StringBuffer.Insert(0, args[i + 1]);
                            if (Directory.Exists(StringBuffer.ToString()))
                            {
                                _sourceLocation = StringBuffer.ToString();
                                Console.ForegroundColor = ConsoleColor.DarkGreen;
                                Console.WriteLine(Resources.SourceLocationSet);
                                StringBuffer.Clear();
                            }
                            else
                            {
                                Console.ForegroundColor = ConsoleColor.DarkRed;
                                Console.WriteLine(Resources.SourceLocationDoesntExist);
                                Console.ForegroundColor = ConsoleColor.White;
                                Console.WriteLine(Resources.PushEnterToExit);
                                Console.ReadLine();
                                Environment.Exit(0);
                            }
                        }

                        break;
                    case "--OutputLocation":
                        if (i < args.Length - 1 && !args[i + 1].Contains("--"))
                        {
                            StringBuffer.Insert(0, args[i + 1].Replace("\"", ""));
                            if (!Directory.Exists(StringBuffer.ToString())) Directory.CreateDirectory(StringBuffer.ToString());
                            _dropLocation = StringBuffer.ToString();
                            Console.WriteLine(Resources.OutputFolderSet);
                            StringBuffer.Clear();
                        }

                        break;
                    case "--LosslessConversion":
                        _compressionMode = 1;
                        break;
                    case "--LossyConversion":
                        _compressionMode = 2;
                        break;
                    case "--LossyQuality":
                        if (i < args.Length - 1 && !args[i + 1].Contains("--"))
                        {
                            if (double.TryParse(args[i + 1], out var qualitySetting))
                            {
                                Console.ForegroundColor = ConsoleColor.DarkGreen;
                                Console.WriteLine(Resources.LossyImageQualitySet, qualitySetting);
                                LossyConversionSetup.Replace("-q 85", "-q " + qualitySetting);
                                Console.ResetColor();
                            }
                        }
                        break;
                }
            }

            if (_converterLocation != null && _sourceLocation != null && _dropLocation != null)
                _settingsSet = true;
            Console.ResetColor();
            Console.WriteLine();

            if (!_settingsSet)
            {
                do
                {
                    Console.WriteLine(Resources.CwebpApplLocationQuestion);
                    _converterLocation = Console.ReadLine();
                    if (_converterLocation == null)
                    {
                        Console.ForegroundColor = ConsoleColor.DarkRed;
                        Console.WriteLine(Resources.NoCwebpAppLocationSet);
                        Console.ResetColor();
                    }
                    else if (!Directory.Exists(_converterLocation))
                    {
                        Console.ForegroundColor = ConsoleColor.DarkRed;
                        Console.Write(Resources.NoDirectoryFound);
                        Console.ResetColor();
                    }
                } while (_converterLocation == null || !Directory.Exists(_converterLocation));

                do
                {
                    Console.WriteLine(Resources.SourceFolderLocationQuestion);
                    _sourceLocation = Console.ReadLine();
                    if (_sourceLocation == null) Console.WriteLine(Resources.NoFolderSpecified);
                    else if (!Directory.Exists(_sourceLocation))
                    {
                        Console.ForegroundColor = ConsoleColor.DarkRed;
                        Console.WriteLine(Resources.NoDirectoryFound);
                        Console.ResetColor();
                    }
                } while (_sourceLocation == null || !Directory.Exists(_sourceLocation));

                do
                {
                    //Ask the user where to put the processed images. If the folder isn't there create it.
                    Console.WriteLine(Resources.DestinationFolderLocationQuestion);
                    _dropLocation = Console.ReadLine();
                    if (_dropLocation == null)
                        Console.WriteLine(Resources.NoFolderSpecified);
                    else if (!Directory.Exists(_dropLocation))
                    {
                        Console.WriteLine(Resources.FolderCreationMessage);
                        Directory.CreateDirectory(_dropLocation);
                    }
                } while (_dropLocation == null);

                if (_compressionMode == 0)
                {
                    Console.WriteLine(
                        Resources.ConversionModeQuestion);
                    var charBuffer = Console.ReadKey().KeyChar;
                    if (!int.TryParse(charBuffer.ToString(), out _compressionMode))
                    {
                        Console.WriteLine(Resources.InvalidOptionMessage);
                        _compressionMode = 1;
                    }
                }
            }

            Console.WriteLine();

            ConverterInfo.FileName = Path.Combine(_converterLocation,
                RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "cwebp.exe" : "cwebp");
            IEnumerable<string> fileMap =
                Directory.EnumerateFiles(_sourceLocation, "*.png", SearchOption.AllDirectories);
            try
            {
                foreach (string imageFile in fileMap)
                {
                    StringBuffer.Insert(0, imageFile);
                    StringBuffer.Replace(_sourceLocation, _dropLocation);
                    StringBuffer.Replace(Path.GetFileName(imageFile), "");
                    if (!Directory.Exists(StringBuffer.ToString()))
                        Directory.CreateDirectory(StringBuffer.ToString());
                    StringBuffer.Append(Path.GetFileName(imageFile));
                    ConverterInfo.Arguments =
                         "\"" + imageFile + "\" " + (_compressionMode == 2 ? LossyConversionSetup.ToString() : LosslessConversionSetup) + " \"" + StringBuffer + "\"";
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.Write(Resources.TimecodeFormatString, DateTime.Now);
                    Console.ResetColor();
                    Console.WriteLine(Resources.ImageConversionMessage, imageFile);
                    var converterProcess = Process.Start(ConverterInfo);
                    converterProcess.WaitForExit();
                    if (converterProcess.ExitCode != 0)
                    {
                        Console.ForegroundColor = ConsoleColor.Cyan;
                        Console.Write(Resources.TimecodeFormatString, DateTime.Now);
                        Console.ForegroundColor = ConsoleColor.DarkRed;
                        Console.WriteLine(Resources.ImageConversionError, imageFile,
                            converterProcess.ExitCode);
                    }
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.Cyan;
                        Console.Write(Resources.TimecodeFormatString, DateTime.Now);
                        Console.ForegroundColor = ConsoleColor.DarkGreen;
                        Console.WriteLine(Resources.ImageConversionCompleteMessage, imageFile);
                    }

                    StringBuffer.Clear();
                }
                Console.ForegroundColor = ConsoleColor.DarkGreen;
                Console.WriteLine(Resources.TaskCompletedMessage);
                Console.ResetColor();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            if (_settingsSet) return;
            Console.WriteLine(Resources.PushEnterToExit);
            Console.ReadLine();
        }
    }
}
