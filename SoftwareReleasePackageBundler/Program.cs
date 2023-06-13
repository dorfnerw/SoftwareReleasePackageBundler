using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;

namespace SoftwareReleasePackageBundler
{
    class Program
    {
        static bool ProcessFinished = false;

        static void Main(string[] args)
        {
            Console.Title = Assembly.GetExecutingAssembly().GetName().Name + "     " + Assembly.GetExecutingAssembly().GetName().Version;

            Thread thread = new Thread(ThreadFunction);
            thread.Start();

            Console.Write("Processing");
            while (!ProcessFinished)
            {
                Console.Write(".");
                Thread.Sleep(500);
            }

            Console.ReadKey();
        }

        private static void ThreadFunction()
        {
            try
            {
                MoveCompiledFolder(@"C:\Users\bdorfner\source\repos\NPAcontrols\APMACShmi\bin", @"C:\Users\bdorfner\Desktop\2.0.0.0.0.0.0\bin");
                MoveCompiledFolder(@"C:\Users\bdorfner\source\repos\APMACS_1\APMACS_1\_Boot\TC", @"C:\Users\bdorfner\Desktop\2.0.0.0.0.0.0\TC");
                MoveCompiledFolder(@"C:\Users\bdorfner\source\repos\NPAcontrols\VersionChangeUtility\bin", @"C:\Users\bdorfner\Desktop\2.0.0.0.0.0.0\VersionChangeUtility");

                if (File.Exists(@"C:\Users\bdorfner\Desktop\2.0.0.0.0.0.0.zip"))
                {
                    File.Delete(@"C:\Users\bdorfner\Desktop\2.0.0.0.0.0.0.zip");
                }
                ZipFile.CreateFromDirectory(@"C:\Users\bdorfner\Desktop\2.0.0.0.0.0.0\", @"C:\Users\bdorfner\Desktop\2.0.0.0.0.0.0.zip");

                DriveInfo usbDrive = null;
                foreach (DriveInfo drive in DriveInfo.GetDrives())
                {
                    if (drive.DriveType == DriveType.Removable)
                    {
                        usbDrive = drive;
                        break;
                    }
                }
                if (usbDrive is null)
                {
                    Console.WriteLine("\nNo USB drive!");
                    ProcessFinished = true;
                    return;
                }
                if (File.Exists(usbDrive.RootDirectory + @"\2.0.0.0.0.0.0.zip"))
                {
                    File.Delete(usbDrive.RootDirectory + @"\2.0.0.0.0.0.0.zip");
                }
                File.Move(@"C:\Users\bdorfner\Desktop\2.0.0.0.0.0.0.zip", usbDrive.RootDirectory + @"\2.0.0.0.0.0.0.zip");
                Console.WriteLine("\nFinished.");
                ProcessFinished = true;
            }
            catch (Exception err)
            {
                Console.WriteLine('\n' + err.Message + '\n' + err.StackTrace);
                ProcessFinished = true;
                return;
            }
        }

        private static void MoveCompiledFolder(string path_to_comp_folder, string path_to_dest_folder)
        {
            if (!Directory.Exists(path_to_comp_folder))
            {
                return;
            }
            if (Directory.Exists(path_to_dest_folder))
            {
                Directory.Delete(path_to_dest_folder, true);
            }
            Directory.Move(path_to_comp_folder, path_to_dest_folder);
        }
    }
}
