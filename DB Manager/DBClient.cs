using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;

namespace InteractiveNoticeboard.DB_Manager
{
    public class DBClient
    {
        static string _DBFile = string.Empty;

        public static string DBFile
        {
            get
            {
                if (string.IsNullOrEmpty(_DBFile))
                {
                    _DBFile = Settings.AppPath.TrimEnd('\\') + @"\data\interactive_noticeboard.accdb";
                }

                return _DBFile;
            }
        }

        public static OleDbConnection GetConnection()
        {
            OleDbConnectionStringBuilder csb = new OleDbConnectionStringBuilder();
            csb.Provider = "Microsoft.ACE.OLEDB.12.0";
            csb.DataSource = DBFile;
            string password = "";

            if (!string.IsNullOrEmpty(password))
            {
                csb.Add("Jet OLEDB:Database Password", password);
            }

            string ConnectionString = csb.ConnectionString;

            return new OleDbConnection(ConnectionString);
        }

        public static DataTable ExecuteAdapter(string query)
        {
            DataTable dt = null;        

            using (OleDbConnection connection = GetConnection())
            {
                try
                {
                    connection.Open();

                    using (OleDbCommand cmd = new OleDbCommand(string.Empty, connection))
                    {
                        //string query = "";
                        cmd.CommandText = query;

                        try
                        {
                            using (DataTable tmp_dt = new DataTable())
                            {
                                using (OleDbDataAdapter adapter = new OleDbDataAdapter(cmd))
                                {
                                    adapter.Fill(tmp_dt);
                                }

                                dt = tmp_dt;
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.Message);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }

            return dt;
        }

        public static List<object> ExecuteReader(string query, string col)
        {
            List<object> results = new List<object>();

            using (OleDbConnection connection = GetConnection())
            {
                try
                {
                    connection.Open();

                    using (OleDbCommand cmd = new OleDbCommand(string.Empty, connection))
                    {
                        //string query = "";
                        cmd.CommandText = query;

                        using (OleDbDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                try
                                {
                                    var d = reader[col];
                                    results.Add(d);
                                }
                                catch { }
                            }
                        }

                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }

            return results;
        }

        public static object ExecuteScalar(string query)
        {
            object result = null;

            using (OleDbConnection connection = GetConnection())
            {
                try
                {
                    connection.Open();

                    using (OleDbCommand cmd = new OleDbCommand(string.Empty, connection))
                    {
                        //string query = "";
                        cmd.CommandText = query;
                        result = cmd.ExecuteScalar();
                    }
                }
                catch(Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }

            return result;
        }

        public static int ExecuteNonQuery(string query)
        {
            int affected_rows = -1;

            using (OleDbConnection connection = GetConnection())
            {
                try
                {
                    connection.Open();

                    using (OleDbCommand cmd = new OleDbCommand(string.Empty, connection))
                    {
                        //string query = "";
                        cmd.CommandText = query;
                        affected_rows = cmd.ExecuteNonQuery();
                        Console.WriteLine("Non query successfull. Rows affected: {0}", affected_rows);
                    }
                }
                catch (Exception ex)
                {
                    affected_rows = -1;
                    Console.WriteLine(ex.Message);
                }
            }

            return affected_rows;
        }
    }
}
