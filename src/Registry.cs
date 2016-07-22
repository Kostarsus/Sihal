using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Win32;

namespace Squirt
{
    /// <summary>
    /// Diese Klasse dient zum Zugriff auf die Registry
    /// </summary>
    internal class Registry
    {
        private const string SOURCE_KEY = "Source";
        private const string FOLDER_KEY = "Folder";


        /// <summary>
        /// Liest den Wert zum angegebenen KeyNamen aus der Registry
        /// </summary>
        /// <param name="keyName"></param>
        /// <returns>
        /// Der gelesene Wert
        /// </returns>
        private string Read(string keyName)
        {
            // Opening the registry key

            RegistryKey rk = Microsoft.Win32.Registry.CurrentUser;
            rk = rk.OpenSubKey("Software")?.OpenSubKey("Squirt");
            if (rk != null)
            {
                try
                {
                    return (string)rk.GetValue(keyName.ToUpper());
                }
                catch
                {
                    return null;
                }
            }

            return null;
        }

        /// <summary>
        /// Schreibt den Wert und den Keynamen in die Registry
        /// </summary>
        /// <param name="keyName"></param>
        /// <param name="keyValue"></param>
        private void Write(string keyName, string keyValue)
        {
            RegistryKey rk = Microsoft.Win32.Registry.CurrentUser;
            rk = rk.OpenSubKey("Software", true);
            rk = rk.CreateSubKey("Squirt");
            rk.SetValue(keyName, keyValue);
        }

        public string DataSourceValue
        {
            get
            {
                return Read(SOURCE_KEY);
            }
            set
            {
                Write(SOURCE_KEY, value);
            }
        }

        public string FolderValue
        {
            get
            {
                return Read(FOLDER_KEY);
            }
            set
            {
                Write(FOLDER_KEY, value);
            }
        }
    }
}
