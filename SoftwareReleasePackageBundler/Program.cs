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
                MoveCompiledFolder(APMACS_HMI_Location, CreateLocation + @"\" + FileName + @"\bin");
                MoveCompiledFolder(APMACS_TC_Location, CreateLocation + @"\" + FileName + @"\TC");
                MoveCompiledFolder(Version_Change_Utility_Location, CreateLocation + @"\" + FileName + @"\VersionChangeUtility");

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

        static private bool CheckForDatabase()
        {
            string databaseName = "SoftwareReleasePackageBundler_Settings";
            string databaseExtension = ".sqlite";
            string strExeFilePath = System.Reflection.Assembly.GetExecutingAssembly().Location;
            string strExeFolderPath = System.IO.Path.GetDirectoryName(strExeFilePath);

            //Check if file exists
            if(!File.Exists(strExeFolderPath+"/"+databaseName+databaseExtension))
            {
                //Create file
                SQLiteConnection.CreateFile(strExeFolderPath + "/" + databaseName + databaseExtension);

                //Create Table
                using (SQLiteConnection connect = new SQLiteConnection("Data Source=" + strExeFolderPath + "/" + databaseName + databaseExtension + ";Version=3;"))
                {
                    connect.Open();

                    string sql = "Create Table 'Settings' ('Key' TEXT, 'Value' TEXT, PRIMARY KEY('Key'))";
                    SQLiteCommand command = new SQLiteCommand(sql, connect);
                    command.ExecuteNonQuery();
                }

                //Create entries
                Dictionary<string,string> defaultDatabase = new Dictionary<string, string>()
                {
                    { "FileName", FileName},
                    { "APMACS_HMI_Location", APMACS_HMI_Location},
                    { "APMACS_TC_Location", APMACS_TC_Location},
                    { "Version_Change_Utility_Location", Version_Change_Utility_Location},
                    { "CreateLocation", CreateLocation}
                };

                string strRow;
                string strVal;
                foreach (KeyValuePair<string, string> keyValuePair in defaultDatabase)
                {
                    strRow = keyValuePair.Key;
                    strVal = keyValuePair.Value;
                    using (SQLiteConnection connect = new SQLiteConnection("Data Source=" + strExeFolderPath + "/" + databaseName + databaseExtension + ";Version=3;"))
                    {
                        connect.Open();

                        using (SQLiteCommand cmd = connect.CreateCommand())
                        {
                            //cmd verifys table exists
                            cmd.CommandText = @"SELECT name FROM sqlite_master WHERE type='table' AND name='Settings'";
                            cmd.CommandType = CommandType.Text;
                            SQLiteDataReader check = cmd.ExecuteReader();
                            while (check.Read())
                            {
                                using (SQLiteCommand colcmd = connect.CreateCommand())
                                {
                                    //colcmd reads from table
                                    colcmd.CommandText = @"INSERT into 'Settings' ('Key','Value') values ('" + strRow + "', '" + strVal + "')";
                                    colcmd.CommandType = CommandType.Text;
                                    SQLiteDataReader r = colcmd.ExecuteReader();
                                }
                            }
                        }
                    }
                }

                //Exited without running, created database
                Console.Write("Exited without running, created database");
                return false;
            }
            else
            {
                //Read file

                using (SQLiteConnection connect = new SQLiteConnection("Data Source=" + strExeFolderPath + "/" + databaseName + databaseExtension + ";Version=3;"))
                {
                    connect.Open();

                    using (SQLiteCommand cmd = connect.CreateCommand())
                    {
                        //cmd verifys table exists
                        cmd.CommandText = @"SELECT name FROM sqlite_master WHERE type='table' AND name='Settings'";
                        cmd.CommandType = CommandType.Text;
                        SQLiteDataReader check = cmd.ExecuteReader();
                        while (check.Read())
                        {
                            using (SQLiteCommand colcmd = connect.CreateCommand())
                            {
                                //colcmd reads from table
                                colcmd.CommandText = @"SELECT ALL " + "*" + " FROM Settings";
                                colcmd.CommandType = CommandType.Text;
                                SQLiteDataReader r = colcmd.ExecuteReader();
                                while (r.Read())    //-- read new row
                                {
                                    // DEK 2023-04-18 when checking null for database columns, must check if it is DBNull, not just null.
                                    string Key = r["Key"] != System.DBNull.Value ? Convert.ToString(r["Key"]) : "";
                                    string Value = r["Value"] != System.DBNull.Value ? Convert.ToString(r["Value"]) : "";

                                    if (Key != "" && Value != "")
                                    {
                                        if (Key == "FileName")
                                        {
                                            FileName = Value;
                                        }
                                        if (Key == "APMACS_HMI_Location")
                                        {
                                            APMACS_HMI_Location = Value;
                                        }
                                        if (Key == "APMACS_TC_Location")
                                        {
                                            APMACS_TC_Location = Value;
                                        }
                                        if (Key == "Version_Change_Utility_Location")
                                        {
                                            Version_Change_Utility_Location = Value;
                                        }
                                        if (Key == "CreateLocation")
                                        {
                                            CreateLocation = Value;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }


            return true;
        }
    }
}
