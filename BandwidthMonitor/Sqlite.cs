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
            
            string sql = "CREATE TABLE IF NOT EXISTS UsefulInterfaces (Id INTEGER PRIMARY KEY AUTOINCREMENT, IdString STRING NOT NULL UNIQUE, IntName STRING  NOT NULL, BytesRecived DOUBLE, BytesSent    DOUBLE)";

            CommandExecuteNonQuery(sql, m_dbConnection);

            m_dbConnection.Close();
            UpdateDay();
        }
        public void CheckIfAnyRowsExists(List<NetworkInterface> interfaces)
        {
            OpenConnection();
            foreach (NetworkInterface ni in interfaces) {
                string howMany = $"SELECT COUNT(*) from '{ni.Id + "-" + ni.Name}'";
                SQLiteCommand command = new SQLiteCommand(howMany, m_dbConnection);
                int result = int.Parse(command.ExecuteScalar().ToString());
                if(result == 2) {
                    string date = DateTime.Now.ToString("yyyy-MM-dd");

                    string insertFirstDateRow = $"INSERT INTO '{ni.Id + "-" + ni.Name}' (Day, BytesRecived, BytesSent, Total) values ('{date}', 0, 0, 0)";
                    CommandExecuteNonQuery(insertFirstDateRow, m_dbConnection);
                }
            }
            CloseConnection();
            
        }

        public void GetLastWeek(NetworkInterface interfejs)
        {
            OpenConnection();
            string howMany = $"SELECT COUNT(*) from '{interfejs.Id + "-" + interfejs.Name}'";
            SQLiteCommand command = new SQLiteCommand(howMany, m_dbConnection);
            int result = int.Parse(command.ExecuteScalar().ToString());
            /*for(int i = 0; i < 7; i++) {
                string getLat7Items = $"SELECT * FROM '{interfejs.Id + "-" + interfejs.Name}' ORDER BY BytesRecived DESC LIMIT 7";
                DataTable dataTable = new DataTable("dfs");
                DataColumn dataColumn = new DataColumn();
                //dataTable.Columns.Add(dataColumn);
                
                SQLiteCommand command2 = new SQLiteCommand(getLat7Items, m_dbConnection);
                SQLiteDataAdapter da = new SQLiteDataAdapter(command2);
                da.Fill(dataTable);
                DataRow[] result3 = dataTable.Select("Day = '2018-10-13' AND BytesRecived = 0");
                foreach(DataRow row in result3) {
                    MessageBox.Show(row[1].ToString());
                }
                MessageBox.Show(result3.ElementAt(0).ToString());
            }*/
            string stm = "SELECT * FROM '{B248262D-03F9-4997-BB84-A243C3B03577}-Wi-Fi' LIMIT 7";
            using (SQLiteCommand cmd = new SQLiteCommand(stm, m_dbConnection))
            {
                using (SQLiteDataReader rdr = cmd.ExecuteReader())
                {
                    while (rdr.Read())
                    {
                        //MessageBox.Show(rdr["BytesRecived"].ToString());
                    }
                }
            }
            CloseConnection();
        }

        //Checks if interface exists and adds if not
        public void AddInterfaces(List<NetworkInterface> UsefulInterface)
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

            

            //string dupa = "fds";
            //string sql = $"IF NOT EXISTS (SELECT 1 FROM UsefulInterfaces WHERE name = '{dupa}')";
            //MessageBox.Show("fd");
            CloseConnection();
        }

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

            //string update = "update UsefulInterfaces SET IntName = 'dupa'";
            // WHERE IdString = '{ACDC777C-24A4-421C-AB79-9CF5496BC780}'";


            //CommandExecuteNonQuery(update, m_dbConnection);
            CloseConnection();
        }
        public void UpdateDay()
        {
            //date = DateTime.Now.ToString("yyyy-MM-dd");
            //var dateAndTime = DateTime.Now;
            // var date = dateAndTime.ToShortDateString();
            //MessageBox.Show(date.ToString());

        }
        public void GetStatsOnStartup()
        {

        }
       
    }
}