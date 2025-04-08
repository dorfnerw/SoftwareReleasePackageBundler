using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
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

        //Default values - Used to create database if it doesn't exist. Running values are stored in database so you can change them depending on the user
        static string FileName = @"2.0.0.0.0.0.0";
        static string APMACS_HMI_Location = @"C:\Users\bdorfner\source\repos\NPAcontrols\APMACShmi\bin";
        static string APMACS_TC_Location = @"C:\Users\bdorfner\source\repos\APMACS_1\APMACS_1\_Boot\TC";
        static string Version_Change_Utility_Location = @"C:\Users\bdorfner\source\repos\NPAcontrols\VersionChangeUtility\bin";
        static string CreateLocation = @"C:\Users\bdorfner\Desktop";
        static void Main(string[] args)
        {
            Console.Title = Assembly.GetExecutingAssembly().GetName().Name + "     " + Assembly.GetExecutingAssembly().GetName().Version;

            if (CheckForDatabase())
            {
                Thread thread = new Thread(ThreadFunction);
                thread.Start();

                Console.Write("Processing");
                while (!ProcessFinished)
                {
                    Console.Write(".");
                    Thread.Sleep(500);
                }
            }

            Console.ReadKey();
        }

        private static void ThreadFunction()
        {
            try
            {
                MoveCompiledFolder(@"C:\Users\bdorfner\source\repos\NPAcontrols\APMACShmi\bin", @"C:\Users\bdorfner\Desktop\2.0.0.0.0.0.0\bin");
                MoveCompiledFolder(@"C:\Users\bdorfner\source\repos\NPAcontrols\VersionChangeUtility\bin", @"C:\Users\bdorfner\Desktop\2.0.0.0.0.0.0\VersionChangeUtility");
                CopyDirectory(@"C:\Users\bdorfner\source\repos\APMACS TwinCAT Shell Project\APMACS Shell Project", @"C:\Users\bdorfner\Desktop\2.0.0.0.0.0.0\APMACS TwinCAT Shell Project\APMACS Shell Project");
                File.Copy(@"C:\Users\bdorfner\source\repos\APMACS TwinCAT Shell Project\APMACS Shell Project.sln", @"C:\Users\bdorfner\Desktop\2.0.0.0.0.0.0\APMACS TwinCAT Shell Project\APMACS Shell Project.sln", true);

                if (File.Exists(CreateLocation + @"\" + FileName + @".zip"))
                {
                    File.Delete(CreateLocation + @"\" + FileName + @".zip");
                }
                ZipFile.CreateFromDirectory(CreateLocation + @"\" + FileName + @"\", CreateLocation + @"\" + FileName + @".zip");

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
                if (File.Exists(usbDrive.RootDirectory + @"\" + FileName + @".zip"))
                {
                    File.Delete(usbDrive.RootDirectory + @"\" + FileName + @".zip");
                }
                File.Move(CreateLocation + @"\" + FileName + @".zip", usbDrive.RootDirectory + @"\" + FileName + @".zip");
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

        public static void CopyDirectory(string sourceDir, string destinationDir)
        {
            // Get information about the source directory
            var dir = new DirectoryInfo(sourceDir);

            // Check if the source directory exists
            if (!dir.Exists)
                throw new DirectoryNotFoundException($"Source directory not found: {dir.FullName}");

            // Cache directories before we start copying
            DirectoryInfo[] dirs = dir.GetDirectories();

            // Create the destination directory
            Directory.CreateDirectory(destinationDir);

            // Get the files in the source directory and copy to the destination directory
            foreach (FileInfo file in dir.GetFiles())
            {
                string targetFilePath = Path.Combine(destinationDir, file.Name);
                file.CopyTo(targetFilePath, true);
            }

            // Recursively call this method to copy sub-directories
            foreach (DirectoryInfo subDir in dirs)
            {
                string newDestinationDir = Path.Combine(destinationDir, subDir.Name);
                CopyDirectory(subDir.FullName, newDestinationDir);
            }
        }
    }
}
