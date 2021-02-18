using System;
using System.Data.SqlClient;
using System.IO;
using System.IO.Compression;
using System.Threading;

namespace ModusDatabaseBackup
{
    class Program
    {
        static void Main(string[] args)
        {
            // get the arguments
            if (args.Length < 3)
            {
                Console.WriteLine(@"Invalid number of arguments, arguments are:
    1. Modus Version
    2. Program Name
    3. Zip File Name

Press any key to continue");
                Console.ReadKey();
                return;
            }


            double modusVersion = Convert.ToDouble(args[0]);

            FileInfo fi = new FileInfo(args[1]);
            FileInfo zipFile = new FileInfo(args[2]);

            string programName = fi.Name.Replace(fi.Extension, string.Empty);

            string backupDirectory = @$"c:\temp\{programName}\";

            // delete the backup directory
            if (Directory.Exists(backupDirectory))
                Directory.Delete(backupDirectory, true);

            // recreate the backup directory
            Directory.CreateDirectory(backupDirectory);

            string[] majorMinorVersion = modusVersion.ToString("f2").Split(new char[] { '.' });

            // get the version string
            string versionString = null;

            if (double.Parse(majorMinorVersion[1]) > 10)
                versionString = $"{majorMinorVersion[0]}{majorMinorVersion[1]}0";
            else
                versionString = $"0{modusVersion.ToString("F2").Replace(".", string.Empty)}";

            // get the database name
            string databaseName = $"LK_Insp_{programName}";

            // create the connection string
            string connectionString = $"Data Source={Environment.MachineName}\\MODUS_SERVER{versionString};Initial Catalog={databaseName};Integrated Security=True";

            // create the backup file name
            string backupFile = $"{backupDirectory}Insp_{programName}.bak";

            // backup the database
            Console.WriteLine("Backing up the database...");
            using (SqlConnection sqlCon = new SqlConnection(connectionString))
            {
                sqlCon.Open();
                string query = @$"USE master
ALTER DATABASE [{databaseName}] SET RECOVERY FULL

BACKUP DATABASE [{databaseName}] TO DISK='{backupFile}' WITH FORMAT
";
                using (SqlCommand cmd = new SqlCommand(query, sqlCon))
                {

                    cmd.ExecuteNonQuery();
                }
            }

            // copy the binary data
            string databaseFilesPath = @$"C:\Program Files (x86)\Microsoft SQL Server\MSSQL12.MODUS_SERVER{versionString}\MSSQL\DATA\SQLBinaryData{versionString}";
            string binaryDataFolder = $"{databaseFilesPath}\\{programName}";

            const string COPY_MESSAGE = "Copying Binary Data";
            Console.Write(COPY_MESSAGE);
            Thread t = new Thread(() =>
            {
                CopyBinaryData(binaryDataFolder, backupDirectory + programName + "\\");
            });

            t.IsBackground = true;
            t.Start();
            int count = 0;
            while (t.IsAlive)
            {
                Thread.Sleep(250);
                Console.SetCursorPosition(COPY_MESSAGE.Length, Console.CursorTop);
                Console.Write(new string('.', count) + new string(' ', 3 - count));
                count++;
                if (count == 4)
                    count = 0;
            }

            Console.WriteLine();
            const string ZIP_MESSAGE = "Creating Zip File";
            Console.Write(ZIP_MESSAGE);
            t = new Thread(() =>
            {
                CopyBinaryData(binaryDataFolder, backupDirectory + programName + "\\");
            });

            t.IsBackground = true;
            t.Start();
            count = 0;
            while (t.IsAlive)
            {
                Thread.Sleep(250);
                Console.SetCursorPosition(ZIP_MESSAGE.Length, Console.CursorTop);
                Console.Write(new string('.', count) + new string(' ', 3 - count));
                count++;
                if (count == 4)
                    count = 0;
            }
            // create the zip file
            if (zipFile.Exists)
                zipFile.Delete();
            ZipFile.CreateFromDirectory(backupDirectory, zipFile.FullName);

            Directory.Delete(backupDirectory, true);

        }

        /// <summary>
        /// Copies the binary data to the backup location for the backup.
        /// </summary>
        /// <param name="binaryDataFolder"></param>
        /// <param name="outputFolder"></param>
        private static void CopyBinaryData(string binaryDataFolder, string outputFolder)
        {
            Directory.CreateDirectory(outputFolder);
            DirectoryInfo di = new DirectoryInfo(binaryDataFolder);
            FileInfo[] files = di.GetFiles();
            foreach (FileInfo file in files)
            {
                string newPath = outputFolder + file.Name;
                file.CopyTo(newPath, true);
            }

            DirectoryInfo[] subDirectories = di.GetDirectories();

            foreach (DirectoryInfo directory in subDirectories)
            {
                di.CreateSubdirectory(directory.Name);
                CopyBinaryData(directory.FullName, outputFolder + "\\" + directory.Name + "\\");
            }
        }
    }
}
