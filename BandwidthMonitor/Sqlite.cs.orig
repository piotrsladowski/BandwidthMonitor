using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Data.SQLite;
using System.Data;
using System.Net.NetworkInformation;

namespace BandwidthMonitor
{
    public class Sqlite : InterfacesClass
    {
        SQLiteConnection m_dbConnection = new SQLiteConnection(@"Data Source=Resources\MyDB.sqlite;Version=3;");
        DataTable dt = new DataTable { TableName = "MyTableName" };
        List<string> interfacesFromDatabase = new List<string>();

        private void OpenConnection()
        {
            m_dbConnection.Open();
        }
        private void CloseConnection()
        {
            m_dbConnection.Close();
        }
        private void CommandExecuteNonQuery(string sqlQuery, SQLiteConnection connection)
        {
            SQLiteCommand command = new SQLiteCommand(sqlQuery, connection);
            command.ExecuteNonQuery();
        }

        //Creates a table if not exists
        public void InitBinding(List<NetworkInterface> interfaces)
        {
            m_dbConnection.Open();

            foreach (NetworkInterface ni in interfaces) {
                string niID = ni.Id + "-" + ni.Name;
                string createTable = $"CREATE TABLE IF NOT EXISTS '{niID}' (Id INTEGER PRIMARY KEY AUTOINCREMENT, Day datetime_text , BytesRecived DOUBLE, BytesSent DOUBLE, Total DOUBLE)";
                CommandExecuteNonQuery(createTable, m_dbConnection);
            }

            //string sql = "CREATE TABLE IF NOT EXISTS UsefulInterfaces (Id INTEGER PRIMARY KEY AUTOINCREMENT, IdString STRING NOT NULL UNIQUE, IntName STRING  NOT NULL, BytesRecived DOUBLE, BytesSent    DOUBLE)";
            //CommandExecuteNonQuery(sql, m_dbConnection);

            m_dbConnection.Close();
        }
        public void CheckIfAnyRowsExists(List<NetworkInterface> interfaces)
        {
            OpenConnection();
            foreach (NetworkInterface ni in interfaces) {
                string howMany = $"SELECT COUNT(*) from '{ni.Id + "-" + ni.Name}'";
                SQLiteCommand command = new SQLiteCommand(howMany, m_dbConnection);
                int result = int.Parse(command.ExecuteScalar().ToString());
                if (result == 0) {
                    string date = DateTime.Now.ToString("yyyy-MM-dd");
                    string insertFirstDateRow = $"INSERT INTO '{ni.Id + "-" + ni.Name}' (Day, BytesRecived, BytesSent, Total) values ('{date}', 0, 0, 0)";
                    CommandExecuteNonQuery(insertFirstDateRow, m_dbConnection);
                }
            }
            CloseConnection();

        }
        public void CheckIfCurrentDayExists(List<NetworkInterface> interfaces)
        {
            OpenConnection();
            foreach (NetworkInterface ni in interfaces) {
                string getLastRow = $"SELECT Day FROM '{ni.Id + "-" + ni.Name}' ORDER BY Id DESC LIMIT 1 ";
                SQLiteCommand command = new SQLiteCommand(getLastRow, m_dbConnection);
                object result = command.ExecuteScalar();
                string date = DateTime.Now.ToString("yyyy-MM-dd");
                if (result.ToString() != date) {
                    string insertFirstDateRow = $"INSERT INTO '{ni.Id + "-" + ni.Name}' (Day, BytesRecived, BytesSent, Total) values ('{date}', 0, 0, 0)";
                    CommandExecuteNonQuery(insertFirstDateRow, m_dbConnection);
                }
            }
            CloseConnection();
        }
        public double[] GetLast7Days(NetworkInterface interfejs)
        {
            OpenConnection();
            double[] Bytes = new double[2];
            string stm = $"SELECT * FROM '{interfejs.Id + "-" + interfejs.Name}' ORDER BY Id DESC LIMIT 7";
            double BytesRecived = 0;
            double BytesSent = 0;
            try {
                using (SQLiteCommand cmd = new SQLiteCommand(stm, m_dbConnection)) {
                    using (SQLiteDataReader rdr = cmd.ExecuteReader()) {
                        while (rdr.Read()) {
                            BytesRecived += double.Parse(rdr["BytesRecived"].ToString());
                            BytesSent += double.Parse(rdr["BytesSent"].ToString());
                        }
                    }
                }
            }
            catch (Exception) {


            }
            Bytes[0] = BytesRecived;
            Bytes[1] = BytesSent;
            CloseConnection();
            return Bytes;
        }

        public double[] GetLast30Days(NetworkInterface interfejs)
        {
            OpenConnection();
            double[] Bytes = new double[2];
            string stm = $"SELECT * FROM '{interfejs.Id + "-" + interfejs.Name}' ORDER BY Id DESC LIMIT 30";
            double BytesRecived = 0;
            double BytesSent = 0;
            try {
                using (SQLiteCommand cmd = new SQLiteCommand(stm, m_dbConnection)) {
                    using (SQLiteDataReader rdr = cmd.ExecuteReader()) {
                        while (rdr.Read()) {
                            BytesRecived += double.Parse(rdr["BytesRecived"].ToString());
                            BytesSent += double.Parse(rdr["BytesSent"].ToString());
                        }
                    }
                }
            }
            catch (Exception) {
            }
            Bytes[0] = BytesRecived;
            Bytes[1] = BytesSent;
            CloseConnection();
            return Bytes;
        }
        //Jak program jest włączany jeszcze raz, bez restartu systemu, to nie działa
        public void Update(List<NetworkInterface> UsefulInterfaces)
        {
            OpenConnection();

            foreach (NetworkInterface nic in UsefulInterfaces) {
                string name = nic.Name;
                string idString = nic.Id;

                IPv4InterfaceStatistics interfaceStatistics = nic.GetIPv4Statistics();

                double BytesRecived2 = 0;
                double BytesSent2 = 0;

                DataRow[] result = dt.Select($"Interface = '{idString + "-" + name}'");
                foreach (DataRow row in result) {
                    BytesRecived2 = Convert.ToDouble(row[1]);
                    BytesSent2 = Convert.ToDouble(row[2]);
                }

                double BytesRecived = interfaceStatistics.BytesReceived + BytesRecived2;
                double BytesSent = interfaceStatistics.BytesSent + BytesSent2;

                string date = DateTime.Now.ToString("yyyy-MM-dd");

                double BytesTotal = BytesRecived + BytesSent;
                string update = $"UPDATE '{idString + "-" + name}' SET BytesRecived = '{BytesRecived}', BytesSent = '{BytesSent}', Total = '{BytesTotal}' WHERE Day='{date}'"; //WHERE IdString = '{nic.Id}'
                SQLiteCommand command = new SQLiteCommand(update, m_dbConnection);
                command.ExecuteNonQuery();
            }
            CloseConnection();
        }
        public void GetStatsOnStartup(List<NetworkInterface> interfaces)
        {
            OpenConnection();

            dt.Clear();
            dt.Columns.Add(new DataColumn("Interface", typeof(string)));
            dt.Columns.Add(new DataColumn("BytesRecived", typeof(double)));
            dt.Columns.Add(new DataColumn("BytesSent", typeof(double)));
            string date = DateTime.Now.ToString("yyyy-MM-dd");
            /*
            foreach (NetworkInterface nic in interfaces) {
                string name = nic.Name;
                string idString = nic.Id;

                string selectOnStartup = $"SELECT * FROM '{idString + "-" + name}' WHERE Day = '{date}'";
                using (SQLiteCommand cmd = new SQLiteCommand(selectOnStartup, m_dbConnection)) {
                    using (SQLiteDataReader rdr = cmd.ExecuteReader()) {

                        while (rdr.Read()) {
                            double BytesRecived = double.Parse(rdr["BytesRecived"].ToString());
                            double BytesSent = double.Parse(rdr["BytesSent"].ToString());
                            dt.Rows.Add($"{idString + "-" + name}", BytesRecived, BytesSent);
                        }
                    }
                }
            }*/
            
            for(int i = 1; i < interfacesFromDatabase.Count; i++) {
                string nameAndId = interfacesFromDatabase[i];
                string selectOnStartup = $"SELECT * FROM '{nameAndId}' WHERE Day = '{date}'";
                using (SQLiteCommand cmd = new SQLiteCommand(selectOnStartup, m_dbConnection)) {
                    using (SQLiteDataReader rdr = cmd.ExecuteReader()) {

                        while (rdr.Read()) {
                            double BytesRecived = double.Parse(rdr["BytesRecived"].ToString());
                            double BytesSent = double.Parse(rdr["BytesSent"].ToString());
                            dt.Rows.Add($"{nameAndId}", BytesRecived, BytesSent);
                        }
                    }
                }
            }

            CloseConnection();
            //dt.WriteXml("dtSchemaOrStructure4.xml");
        }
        public void FillDataTable()
        {
            /*
            //string query = "SELECT name FROM sqlite_master where type='table' order by name";
            SQLiteCommand command = new SQLiteCommand("SELECT name FROM sqlite_master where type='table' order by name", m_dbConnection);
            SQLiteDataAdapter myAdapter = new SQLiteDataAdapter(command);
            DataSet myDataSet = new DataSet();
            myAdapter.Fill(myDataSet, "name");
            //MessageBox.Show("xD0");
            return myDataSet;
            */

            OpenConnection();
            string stm = @"SELECT name FROM sqlite_master WHERE type='table' ORDER BY name";
            using (SQLiteCommand cmd = new SQLiteCommand(stm, m_dbConnection)) {
                using (SQLiteDataReader rdr = cmd.ExecuteReader()) {
                    while (rdr.Read()) {
                        interfacesFromDatabase.Add(rdr.GetString(0));
                    }
                }

            }
            //MessageBox.Show(interfacesFromDatabase[0]);
            CloseConnection();
        }
    }
}