using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Xml.Serialization;

namespace PokerHUD.Util
{
    class Config
    {
        #region Settings

        public enum DatabaseType
        {
            [Description("Postgress")]
            POSTGRESS = 1,
            [Description("SQL Lite")]
            SQL_LITE
        }


        private static Config _config;
        public static readonly string AppDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)
                                             + @"\PokerHUD";

        [DefaultValue(DatabaseType.SQL_LITE)]
        public DatabaseType databaseType = DatabaseType.SQL_LITE;

        [DefaultValue(".")]
        public string DataDirPath = ".";

        //updating from <= 0.5.1: 
        //SaveConfigInAppData and SaveDataInAppData are set to SaveInAppData AFTER the config isloaded
        //=> Need to be null to avoid creating new config in appdata if config is stored locally.
        [DefaultValue(true)]
        public bool? SaveConfigInAppData = null;

        [DefaultValue(true)]
        public bool? SaveDataInAppData = null;

        [DefaultValue(0)]
        public int LogLevel = 0;

        #endregion

        #region Properties

        public string ConfigPath
        {
            get { return Instance.ConfigDir + "config.xml"; }
        }

        public string ConfigDir
        {
            get { return Instance.SaveConfigInAppData == false ? string.Empty : AppDataPath + "\\"; }
        }

        public string DataDir
        {
            get { return Instance.SaveDataInAppData == false ? DataDirPath + "\\" : AppDataPath + "\\"; }
        }

        #endregion

        public static Config Instance
        {
            get
            {
                if (_config == null)
                {
                    _config = new Config();
                }

                return _config;
            }
        }

        public static void Save()
        {
            XmlManager<Config>.Save(Instance.ConfigPath, Instance);
        }

        public static void SaveBackup(bool deleteOriginal = false)
        {
            var configPath = Instance.ConfigPath;

            if (File.Exists(configPath))
            {
                File.Copy(configPath, configPath + DateTime.Now.ToFileTime());

                if (deleteOriginal)
                    File.Delete(configPath);
            }
        }

        public static void Load()
        {
            var foundConfig = false;
            Environment.CurrentDirectory = AppDomain.CurrentDomain.BaseDirectory;
            try
            {
                if (File.Exists("config.xml"))
                {
                    _config = XmlManager<Config>.Load("config.xml");
                    foundConfig = true;
                }
                else if (File.Exists(AppDataPath + @"\config.xml"))
                {
                    _config = XmlManager<Config>.Load(AppDataPath + @"\config.xml");
                    foundConfig = true;
                }
                else if (!Directory.Exists(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)))
                    //save locally if appdata doesn't exist (when e.g. not on C)
                    Instance.SaveConfigInAppData = false;
            }
            catch (Exception e)
            {
                MessageBox.Show(
                                e.Message + "\n\n" + e.InnerException + "\n\n If you don't know how to fix this, please delete "
                                + Instance.ConfigPath, "Error loading config.xml");
                Application.Current.Shutdown();

            }

            if (!foundConfig)
            {
                if (Instance.ConfigDir != string.Empty)
                    Directory.CreateDirectory(Instance.ConfigDir);
                Save();
            }
            else if (Instance.SaveConfigInAppData != null)
            {
                if (Instance.SaveConfigInAppData.Value) //check if config needs to be moved
                {
                    if (File.Exists("config.xml"))
                    {
                        Directory.CreateDirectory(Instance.ConfigDir);
                        SaveBackup(true); //backup in case the file already exists
                        File.Move("config.xml", Instance.ConfigPath);
                        Logger.WriteLine("Moved config to appdata", "Config");
                    }
                }
                else if (File.Exists(AppDataPath + @"\config.xml"))
                {
                    SaveBackup(true); //backup in case the file already exists
                    File.Move(AppDataPath + @"\config.xml", Instance.ConfigPath);
                    Logger.WriteLine("Moved config to local", "Config");
                }
            }
        }
    }
}
