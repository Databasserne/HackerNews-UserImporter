using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using System.IO;

namespace User_Importer
{
    class Program
    {
        static void Main(string[] args)
        {
            var stopwatch = new Stopwatch();

            stopwatch.Start();

            var users = ReadFromCSV("more_users.csv");
            users.AddRange(ReadFromCSV("users.csv"));

            var s = users.GroupBy(p => p.username.ToLower()).Select(grp => grp.First()).ToList();

            Console.WriteLine("Done importing from CSV");
            
            BulkToMySQL(s);

            stopwatch.Stop();
            Console.WriteLine("Took: " + stopwatch.ElapsedMilliseconds);
        }

        public static List<user> ReadFromCSV(string csv)
        {
            var lines = File.ReadAllLines(csv).Select(a => a.Split(','));
            List<user> users = new List<user>();
            var i = 0;

            foreach (var line in lines)
            {
                if (i == 0)
                {
                    i++;
                    continue;
                }

                users.Add(new user(){username = line[0], password = line[1]});

                i++;
            }

            return users;
        }

        public static void BulkToMySQL(List<user> users)
        {
            string ConnectionString = "Server=<Server IP>;Database=<Database Name>;Uid=<User name>;Pwd=<Password>;";
            StringBuilder sCommand = new StringBuilder("INSERT INTO user (username, password) VALUES ");
            using (MySqlConnection mConnection = new MySqlConnection(ConnectionString))
            {
                List<string> Rows = new List<string>();
                foreach (var user in users)
                {
                    Rows.Add(string.Format("('{0}','{1}')", MySqlHelper.EscapeString(user.username), MySqlHelper.EscapeString(user.password)));
                }
                sCommand.Append(string.Join(",", Rows));
                sCommand.Append(";");
                mConnection.Open();
                using (MySqlCommand myCmd = new MySqlCommand(sCommand.ToString(), mConnection))
                {
                    myCmd.CommandType = CommandType.Text;
                    myCmd.ExecuteNonQuery();
                }
            }
        }
    }
}
