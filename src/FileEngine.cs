using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Squirt
{
    class FileEngine
    {
        /// <summary>
        /// List alle Dateinamen in <paramref name="directory"/>
        /// </summary>
        /// <param name="directory"></param>
        /// <returns>
        /// Die Liste aller Dateinamn
        /// </returns>
        private static List<string> GetFilenames(string directory)
        {
            return new List<string>(Directory.EnumerateFiles(directory));
        }

        /// <summary>
        /// Liest alle Dateien in der Liste
        /// </summary>
        /// <param name="fileNames"></param>
        /// <returns>
        /// Die gelesenen Update-Informationen
        /// </returns>
        private static List<VersionsSqlVersion> ReadFiles(List<string> fileNames)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(Versions));
            List<VersionsSqlVersion> result = new List<VersionsSqlVersion>();
            foreach (string fileName in fileNames)
            {
                try
                {
                    using (var fileStream = new FileStream(fileName, FileMode.Open))
                    {
                        var versionsOfFile = serializer.Deserialize(fileStream) as Versions;
                        result.AddRange(versionsOfFile.Items);
                    }
                }
                catch(Exception e)
                {
                    MainWindow.Instance.AddProtocolEntry(string.Format("Fehler beim Lesen von {0}: {1}", fileName, e.Message));
                }
            }

            return result;
        }

        /// <summary>
        /// Liest alle Update-Anweisungen in den Dateien des <paramref name="zupdateDirectory"/>
        /// </summary>
        /// <param name="updateDirectory"></param>
        /// <returns>
        /// Die gelesenen Update-Informationen
        /// </returns>
        public static List<VersionsSqlVersion> ReadSqlVersions(string updateDirectory)
        {
            var fileNames = GetFilenames(updateDirectory);
            return ReadFiles(fileNames);
        }
    }
}
