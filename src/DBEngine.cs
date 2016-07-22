using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlServerCe;
using System.Data;
using System.Windows;

namespace Squirt
{
    internal class DBEngine
    {
        private string dataSource;

        public DBEngine(string dataSource)
        {
            this.dataSource = dataSource;
            Initialize();
        }

        public void ExecuteCommand(string command)
        {
            using (var conn = new SqlCeConnection(this.dataSource))
            {
                conn.Open();
                SqlCeCommand cmd = new SqlCeCommand(command, conn);
                cmd.ExecuteNonQuery();
                conn.Close();
            }
        }

        private void Initialize()
        {
            try
            {
                if (!ExistVersionTable())
                {
                    CreateVersionTable();
                }
            }
            catch (Exception e)
            {
                MessageBox.Show("Es ist ein schwerwiegender Fehler mit der Dtaenbankverbindung aufgetreten.", e.Message);
            }
        }

        private void CreateVersionTable()
        {
            ExecuteCommand(@"      CREATE TABLE VersionEntities (
                                   ID bigint IDENTITY(1,1) NOT NULL,
                                   PrimaryVersion int  NOT NULL,
                                   SecondaryVersion int  NOT NULL,
                                   Installdate datetime  NOT NULL
                                   );"
                           );
        }

        /// <summary>
        /// Diese Methode führt die SQL-Queries durch und aktualisiert somit die Datenbank
        /// </summary>
        /// <param name="sqlQueries">
        /// Die SQL-Queries. Die einzelnen Queries sind durch Semiklions getrennt
        /// </param>
        public void ExecuteSqlQuery(string sqlQueries, string version, string subversion)
        {
            using (SqlCeConnection connection = new SqlCeConnection(this.dataSource))
            {
                connection.Open();
                using (SqlCeTransaction transaction = connection.BeginTransaction())
                {
                    try
                    {
                        string[] queriesSplitted = sqlQueries.Split(';');
                        for (int i = 0; i < queriesSplitted.Length; i++)
                        {
                            if (string.IsNullOrWhiteSpace(queriesSplitted[i]))
                            {
                                continue;
                            }
                            try
                            {
                                string singleQuery = queriesSplitted[i].Trim() + ";";
                                SqlCeCommand cmd = new SqlCeCommand(singleQuery, connection, transaction);
                                cmd.ExecuteNonQuery();
                            }
                            catch (Exception ex)
                            {
                                string message = ex + string.Format("(Version:{0}; SubVersion:{1}; Line:{2})", version, subversion, i);
                                MainWindow.Instance.AddProtocolEntry(message);
                            }
                        }
                        transaction.Commit();
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
                connection.Close();
            }
        }

        /// <summary>
        /// Prüft ob die Tabelle VersionEntities zur Speicherung der Versionen vorhanden ist
        /// </summary>
        /// <returns></returns>
        public bool ExistVersionTable()
        {
            DataSet ds = new DataSet();
            using (var conn = new SqlCeConnection(this.dataSource))
            {
                conn.Open();
                SqlCeDataAdapter da = new SqlCeDataAdapter("SELECT * FROM information_schema.tables WHERE table_name = 'VersionEntities';", conn);
                da.Fill(ds);
                conn.Close();
            }
            return ds.Tables[0].Rows.Count != 0;
        }

        /// <summary>
        /// DieseMethode fügt den Eintrag <paramref name="versionToInsert"/> in die Versionstabelle ein
        /// </summary>
        /// <param name="versionToInsert"></param>
        public void InsertVersion(VersionsSqlVersion versionToInsert)
        {
            string query = string.Format(@"INSERT into VersionEntities(PrimaryVersion, SecondaryVersion, Installdate)
                                           VALUES
                                           ({0}, {1}, GetDate());", versionToInsert.Version, versionToInsert.Subversion);
            ExecuteCommand(query);
        }

        /// <summary>
        /// Prüft ob eine SQL-Version bereits in der Datenbank existiert
        /// </summary>
        /// <param name="versionToCheck"></param>
        /// <returns>
        /// true wenn die Version mit Subversion existiert, ansonsten false
        /// </returns>
        public bool ExistsVersion(VersionsSqlVersion versionToCheck)
        {
            DataSet ds = new DataSet();
            using (var conn = new SqlCeConnection(this.dataSource))
            {
                conn.Open();
                string query = string.Format(@"SELECT * FROM VersionEntities
                                               WHERE PrimaryVersion = {0}
                                               AND SecondaryVersion = {1};", versionToCheck.Version, versionToCheck.Subversion);
                SqlCeDataAdapter da = new SqlCeDataAdapter(query, conn);
                da.Fill(ds);
                conn.Close();
            }
            return ds.Tables[0].Rows.Count != 0;

        }

    }

}

