//Author : Viral Surani
//Date   : 17 July 2016

//To execute this windows service, execute belowe commands in powershell
//sc.exe create BackupServiceDemo binpath= "D:\Study\Projects\BackupService\BackupService\BackupService\bin\Debug\BackupService.exe" start= auto displayname= "Backup Service Demo"
//sc.exe start BackupServiceDemo "C:\Users\Viral\Desktop\test\"
//sc.exe description BackupServiceDemo "Protecting Backup files. Author: Viral Surani"

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.ServiceProcess;

namespace BackupService
{
    public partial class BackupService : ServiceBase
    {
        private List<string> _protectedFiles = new List<string>();
        private string _directoryPath = string.Empty;

        public BackupService()
        {
            InitializeComponent();
            this.ServiceName = "Backup Service";
            this.EventLog.Source = this.ServiceName;
            this.EventLog.Log = "Application";            
        }

        protected override void OnStart(string[] args)
        {
            try
            {                
                _directoryPath = File.ReadAllText(Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location)+@"\Input.txt"); 
                if (String.IsNullOrEmpty(_directoryPath))
                    throw new Exception("Directory Path is empty");

                ProtectFiles();

                FileSystemWatcher fileSystemWatcher = new FileSystemWatcher();                
                fileSystemWatcher.Path = _directoryPath;
                fileSystemWatcher.Filter = "*.*";
                fileSystemWatcher.NotifyFilter = NotifyFilters.Size | NotifyFilters.LastWrite | NotifyFilters.LastAccess | NotifyFilters.DirectoryName | NotifyFilters.FileName;
                fileSystemWatcher.IncludeSubdirectories = true;
                fileSystemWatcher.Created += fileSystemWatcher_Created;
                fileSystemWatcher.Changed += fileSystemWatcher_Changed;
                fileSystemWatcher.EnableRaisingEvents = true;
            }
            catch (Exception exception)
            {
                EventLog.WriteEntry("OnStart: " + exception.Message, EventLogEntryType.Error);   
            }
        }

        void fileSystemWatcher_Changed(object sender, FileSystemEventArgs e)
        {
            try
            {
                ProtectFiles();
            }
            catch (Exception exception)
            {
                EventLog.WriteEntry("fileSystemWatcher_Changed: " + exception.Message, EventLogEntryType.Error);
            }
        }

        void fileSystemWatcher_Created(object sender, FileSystemEventArgs e)
        {
            try
            {
                ProtectFiles();
            }
            catch (Exception exception)
            {
                EventLog.WriteEntry("fileSystemWatcher_Created: " + exception.Message, EventLogEntryType.Error);
            }
        }        

        /// <summary>
        /// To lock all the files
        /// </summary>
        private void ProtectFiles()
        {
            this.EventLog.WriteEntry("Protecting Backup Files");
            List<string> files = Directory.GetFiles(_directoryPath, "*.*", SearchOption.AllDirectories).ToList<string>();            

            files = files.Except(_protectedFiles).ToList();

            foreach (string item in files)
            {
                GC.KeepAlive(new FileStream(item, FileMode.Open, FileAccess.ReadWrite, FileShare.None));
            }

            _protectedFiles.AddRange(files);
        }

        protected override void OnStop()
        {
        }
    }
}
