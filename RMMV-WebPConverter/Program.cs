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
        private const string LosslessConversionSetup = "-z 9 -sharp_yuv -mt -quiet -o";
        private static readonly StringBuilder LossyConversionSetup = new StringBuilder(" - m 6 - q 85 - sharp_yuv - mt - quiet - o ");
        private static readonly ProcessStartInfo _converterInfo = new ProcessStartInfo();

        static void Main(string[] args)
        {
            Console.WriteLine("========================================================");
            Console.WriteLine("= WebP Conversion and Preparation Tool for RPG Maker MV");
            Console.WriteLine("= Version R1.00 ({0})", Assembly.GetExecutingAssembly().GetName().Version);
            Console.WriteLine("= Developed by AceOfAces.");
            Console.WriteLine("= Licensed under the MIT license.");
            Console.WriteLine("========================================================\n");

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
                                Console.WriteLine("The folder does not exist.");
                                Console.ResetColor();
                                Console.WriteLine("Press Enter/Return to exit.");
                                Console.ReadLine();
                                Environment.Exit(0);
                            }

                            if (File.Exists(Path.Combine(_converterLocation,
                                RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "cwebp.exe" : "cwebp")))
                            {
                                Console.ForegroundColor = ConsoleColor.DarkGreen;
                                Console.WriteLine("The location for the converter is set.");
                            }
                            else
                            {

                                Console.ForegroundColor = ConsoleColor.DarkRed;
                                Console.WriteLine("There is no cwebp in the folder.");
                                Console.ResetColor();
                                Console.WriteLine("Press Enter/Return to exit.");
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
                                Console.WriteLine("The location for source is set.");
                                StringBuffer.Clear();
                            }
                            else
                            {
                                Console.ForegroundColor = ConsoleColor.DarkRed;
                                Console.WriteLine("The source location doesn't exist.");
                                Console.ForegroundColor = ConsoleColor.White;
                                Console.WriteLine("Press Enter/Return to exit");
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
                            Console.WriteLine("The location for the output is set.");
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
                                Console.WriteLine("The quality setting for the lossy conversion is set to {0}", qualitySetting);
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
                    Console.WriteLine("Where's the location of the cwebp?");
                    _converterLocation = Console.ReadLine();
                    if (_converterLocation == null)
                    {
                        Console.ForegroundColor = ConsoleColor.DarkRed;
                        Console.WriteLine("Please insert the path for cwebp.\n");
                        Console.ResetColor();
                    }
                    else if (!Directory.Exists(_converterLocation))
                    {
                        Console.ForegroundColor = ConsoleColor.DarkRed;
                        Console.Write("The directory isn't there. Please select an existing folder.\n");
                        Console.ResetColor();
                    }
                } while (_converterLocation == null || !Directory.Exists(_converterLocation));

                do
                {
                    Console.WriteLine("\nWhere are the files you want to convert to? ");
                    _sourceLocation = Console.ReadLine();
                    if (_sourceLocation == null) Console.WriteLine("Please specify the location of the folder.\n");
                    else if (!Directory.Exists(_sourceLocation))
                    {
                        Console.ForegroundColor = ConsoleColor.DarkRed;
                        Console.WriteLine("The folder you've selected isn't present.\n");
                        Console.ResetColor();
                    }
                } while (_sourceLocation == null || !Directory.Exists(_sourceLocation));

                do
                {
                    //Ask the user where to put the processed audio files. If the folder isn't there create it.
                    Console.WriteLine("\nWhere to put the converted files?");
                    _dropLocation = Console.ReadLine();
                    if (_dropLocation == null)
                        Console.WriteLine("Please specify the location of the folder.\n");
                    else if (!Directory.Exists(_dropLocation))
                    {
                        Console.WriteLine("Creating folder...\n");
                        Directory.CreateDirectory(_dropLocation);
                    }
                } while (_dropLocation == null);

                if (_compressionMode == 0)
                {
                    Console.WriteLine(
                        "Should the conversion be:\n 1. Lossless (default, preserves a lot of detail but space savings are smaller)?\n 2. Lossy(smaller file size at the cost of image quality)?");
                    var charBuffer = Console.ReadKey().KeyChar;
                    if (!int.TryParse(charBuffer.ToString(), out _compressionMode))
                    {
                        Console.WriteLine("Looks like you gave a non-integer number. Applying the default setting.");
                        _compressionMode = 1;
                    }
                }
            }

            Console.WriteLine();

            _converterInfo.FileName = Path.Combine(_converterLocation,
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
                    _converterInfo.Arguments =
                         "\"" + imageFile + "\" " + (_compressionMode == 2 ? LossyConversionSetup.ToString() : LosslessConversionSetup) + " \"" + StringBuffer + "\"";
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.Write("[{0}] ", DateTime.Now);
                    Console.ResetColor();
                    Console.WriteLine("Converting {0} to WebP...", imageFile);
                    var converterProcess = Process.Start(_converterInfo);
                    converterProcess.WaitForExit();
                    if (converterProcess.ExitCode != 0)
                    {
                        Console.ForegroundColor = ConsoleColor.Cyan;
                        Console.Write("[{0}] ", DateTime.Now);
                        Console.ForegroundColor = ConsoleColor.DarkRed;
                        Console.WriteLine("cwebp failed to compile {0}. It returned error code {1}.", imageFile,
                            converterProcess.ExitCode);
                    }
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.Cyan;
                        Console.Write("[{0}] ", DateTime.Now);
                        Console.ForegroundColor = ConsoleColor.DarkGreen;
                        Console.WriteLine("Finished converting {0}.", imageFile);
                    }

                    StringBuffer.Clear();
                }
                Console.ForegroundColor = ConsoleColor.DarkGreen;
                Console.WriteLine("\nThe task was completed.");
                Console.ResetColor();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            if (_settingsSet) return;
            Console.WriteLine("Press Enter/Return to exit.");
            Console.ReadLine();
        }
    }
}
