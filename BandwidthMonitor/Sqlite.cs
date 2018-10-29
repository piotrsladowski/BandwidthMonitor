using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Data.SQLite;
using System.Data;
using System.Net.NetworkInformation;
using SqlKata;

namespace BandwidthMonitor
{
    public class Sqlite : InterfacesClass
    {
        SQLiteConnection m_dbConnection = new SQLiteConnection(@"Data Source=D:\Visual Studio\BandwidthMonitor\BandwidthMonitor\bin\Debug\MyDB.sqlite;Version=3;");

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

            foreach(NetworkInterface ni in interfaces) {
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
                if(result == 0) {
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
            foreach(NetworkInterface ni in interfaces)
            {
                string getLastRow = $"SELECT Day FROM '{ni.Id + "-" + ni.Name}' ORDER BY Id DESC LIMIT 1 ";
                SQLiteCommand command = new SQLiteCommand(getLastRow, m_dbConnection);
                object result = command.ExecuteScalar();
                string date = DateTime.Now.ToString("yyyy-MM-dd");
                if (result.ToString() != date)
                {
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
            using (SQLiteCommand cmd = new SQLiteCommand(stm, m_dbConnection))
            {
                using (SQLiteDataReader rdr = cmd.ExecuteReader())
                {
                    while (rdr.Read())
                    {
                        BytesRecived += double.Parse(rdr["BytesRecived"].ToString());
                        BytesSent += double.Parse(rdr["BytesSent"].ToString());
                    }
                }
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
            using (SQLiteCommand cmd = new SQLiteCommand(stm, m_dbConnection))
            {
                using (SQLiteDataReader rdr = cmd.ExecuteReader())
                {
                    while (rdr.Read())
                    {
                        BytesRecived += double.Parse(rdr["BytesRecived"].ToString());
                        BytesSent += double.Parse(rdr["BytesSent"].ToString());
                    }
                }
            }
            Bytes[0] = BytesRecived;
            Bytes[1] = BytesSent;
            CloseConnection();
            return Bytes;
        }

        //Checks if interface exists and adds if not
        /*public void AddInterfaces(List<NetworkInterface> UsefulInterface)
        {
            OpenConnection();
            foreach (NetworkInterface nic in UsefulInterface) {
                string name = nic.Name;
                string idString = nic.Id;
                

                //string sql = $"IF * NOT EXISTS (SELECT 1 FROM UsefulInterfaces WHERE IntName = '{name}') BEGIN INSERT INTO UsefulInterfaces (IntName) VALUES ('{name}') END";
                string sql = $"SELECT * FROM UsefulInterfaces WHERE IdString = '{idString}'";
                //string sql = "select * from usefulinterfaces order by intName desc";
                //SQLiteCommand command = new SQLiteCommand(query.ToString(), m_dbConnection);
                SQLiteCommand command = new SQLiteCommand(sql, m_dbConnection);
                SQLiteDataReader reader = command.ExecuteReader();

                if(reader.HasRows == false) {
                    string insert = $"INSERT INTO UsefulInterfaces (IntName, IdString, MegaBytesRecived, MegaBytesSent) values ('{name}', '{idString}', 0, 0)";
                    SQLiteCommand commandInsertInterfaces = new SQLiteCommand(insert, m_dbConnection);
                    commandInsertInterfaces.ExecuteNonQuery();
                }

            }
            CloseConnection();
        }*/

        public void Update(List<NetworkInterface> UsefulInterfaces)
        {
            OpenConnection();

            foreach(NetworkInterface nic in UsefulInterfaces) {
                string name = nic.Name;
                string idString = nic.Id;

                IPv4InterfaceStatistics interfaceStatistics = nic.GetIPv4Statistics();

                double MegaBytesRecived = Math.Round((interfaceStatistics.BytesReceived) / (Math.Pow(1024, 2)), 2);//później do usunięcia
                double MegaBytesSent = Math.Round((interfaceStatistics.BytesSent) / (Math.Pow(1024, 2)), 2);

                double BytesRecived = interfaceStatistics.BytesReceived;
                double BytesSent = interfaceStatistics.BytesSent;

                string date = DateTime.Now.ToString("yyyy-MM-dd");

                double BytesTotal = BytesRecived + BytesSent;
                string update = $"UPDATE '{idString + "-" + name}' SET BytesRecived = '{BytesRecived}', BytesSent = '{BytesSent}', Total = '{BytesTotal}' WHERE Day='{date}'"; //WHERE IdString = '{nic.Id}'
                SQLiteCommand command = new SQLiteCommand(update, m_dbConnection);
                command.ExecuteNonQuery();
            }
            //CommandExecuteNonQuery(update, m_dbConnection);
            CloseConnection();
        }
        public void GetStatsOnStartup(List<NetworkInterface> interfaces)
        {
            OpenConnection();
            DataTable dt = new DataTable { TableName = "MyTableName" };
            dt.Clear();
            dt.Columns.Add("Interface");
            dt.Columns.Add("BytesRecived");
            dt.Columns.Add("BytesSent");
            string date = DateTime.Now.ToString("yyyy-MM-dd");

            foreach (NetworkInterface nic in interfaces) {
                    string name = nic.Name;
                    string idString = nic.Id;

                    string selectOnStartup = $"SELECT * FROM '{idString + "-" + name}' WHERE Day = '{date}'";
                    using (SQLiteCommand cmd = new SQLiteCommand(selectOnStartup, m_dbConnection)) {
                        using (SQLiteDataReader rdr = cmd.ExecuteReader()) {

                            while (rdr.Read()) {
                                                            
                            double BytesRecived = double.Parse(rdr["BytesRecived"].ToString());
                            double BytesSent = double.Parse(rdr["BytesSent"].ToString());

                            DataRow row3 = dt.NewRow();
                            row3["Interface"] = idString + "-" + name;
                            row3["BytesRecived"] = BytesRecived;
                            row3["BytesSent"] = BytesSent;
                            dt.Rows.Add(row3);
                            }
                        }
                    }
            }
            CloseConnection();
            //dt.WriteXml("dtSchemaOrStructure5.xml");
        }
       
    }
}