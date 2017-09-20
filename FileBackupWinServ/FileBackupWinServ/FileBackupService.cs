//Author: Brent Kuzmanich
//Comment: Windows Service 

using FileBackup.FileBackup.Factories;
using FileBackup.FileObserver;
using FileBackup.FileObserver.Factories;
using FileBackup.Utility;
using Microsoft.Practices.Unity;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Security.Permissions;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace FileBackupWinServ
{
    [PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
    public partial class FileBackupService : ServiceBase
    {
        private static IUnityContainer container;
        private IPathObserver observer;
        
        public FileBackupService(IUnityContainerFactory containerFactory)
        {
            //unity
            container = containerFactory.GetContainer();
            InitializeComponent();
            eventLog1 = new EventLog();
            if (!EventLog.SourceExists("FileBackupService"))
            {
                EventLog.CreateEventSource("FileBackupService", "Application");
            }
            eventLog1.Source = "FileBackupService";
            eventLog1.Log = "Application";
        }


        protected override void OnStart(string[] args)
        {
            try
            {                
                string INDEX = ConfigurationManager.AppSettings["indexFileName"];
                int delay = Convert.ToInt16(ConfigurationManager.AppSettings["delayTime"]);
                var path = GetProjectDir() + INDEX;
                
                //Setup observer to begin watching paths
                var fact = container.Resolve<IPathObserverFactory>();
                observer = fact.GetPathObserver(path, delay, RepositoryType.File);

                eventLog1.WriteEntry("FileBackupService started");
            }
            catch(Exception ex)
            {
                eventLog1.WriteEntry("FileBackupService failed to start");
                throw ex;
            }
        }

        protected override void OnStop()
        {
            try
            {
                eventLog1.WriteEntry("FileBackupService stopped");
                //Tear down
                observer.Dispose();
            }
            catch(Exception ex)
            {
                eventLog1.WriteEntry("FileBackupService failed to stop");
                throw ex;
            }
        }

        //Resolve path to bin folder
        private static string GetProjectDir()
        {
            var dir = AppDomain.CurrentDomain.BaseDirectory;
            var bin = dir.IndexOf("bin");
            if (bin > 0)
            {
                dir = dir.Substring(0, bin);
            }
            return dir;
        }
    }
}
