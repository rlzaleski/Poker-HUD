using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace PokerHUD.Util
{
    /**
     * Credit to Epix and Heartstone Deck Tracker
     */
    class XmlManager<T>
    {
        public static T Load(string path)
        {
            Logger.WriteLine("Loading file: " + path, "XmlManager", 1);
            T instance;
            using (TextReader reader = new StreamReader(path))
            {
                var xml = new XmlSerializer(typeof(T));
                instance = (T)xml.Deserialize(reader);
            }

            Logger.WriteLine("File loaded: " + path, "XmlManager", 1);
            return instance;
        }

        public static T LoadFromString(string xmlString)
        {
            T instance;
            using (TextReader reader = new StringReader(xmlString))
            {
                var xml = new XmlSerializer(typeof(T));
                instance = (T)xml.Deserialize(reader);
            }
            return instance;
        }

        public static void Save(string path, object obj)
        {
            var i = 0;
            var deleteBackup = true;
            var backupPath = path.Replace(".xml", "_backup.xml");

            //make sure not to overwrite backups that could not be restored (were not deleted)
            while (File.Exists(backupPath))
                backupPath = path.Replace(".xml", "_backup" + i++ + ".xml");

            Logger.WriteLine("Saving file: " + path, "XmlManager", 1);

            //create backup
            if (File.Exists(path))
                File.Copy(path, backupPath);
            try
            {
                //standard serialization
                using (TextWriter writer = new StreamWriter(path))
                {
                    var xml = new XmlSerializer(typeof(T));
                    xml.Serialize(writer, obj);
                }
                Logger.WriteLine("File saved: " + path, "XmlManager", 1);
            }
            catch (Exception e)
            {
                Logger.WriteLine("Error saving file: " + path + "\n" + e, "XmlManager", 1);
                try
                {
                    //restore backup
                    File.Delete(path);
                    if (File.Exists(backupPath))
                        File.Move(backupPath, path);
                }
                catch (Exception e2)
                {
                    //restoring failed 
                    deleteBackup = false;
                    Logger.WriteLine("Error restoring backup for: " + path + "\n" + e2, "XmlManager", 1);
                }
            }
            finally
            {
                if (deleteBackup && File.Exists(backupPath))
                {
                    try
                    {
                        File.Delete(backupPath);
                    }
                    catch (Exception)
                    {
                        //note sure, todo?
                    }
                }
            }
        }
    }
}
