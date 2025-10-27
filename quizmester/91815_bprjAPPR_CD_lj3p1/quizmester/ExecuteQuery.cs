using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace quizmester
{
    class ExecuteQuery
    {
        /// <summary>
        /// string for the connection to the database
        /// </summary>
        public string connectionString { get; } = "Data Source=localhost\\sqlexpress;Initial Catalog=database;Integrated Security=True;TrustServerCertificate=True";

        /// <summary>
        /// this function will execute a query and return the count of rows affected.
        /// ExecuteScalar is used to get a single value from the database
        /// </summary>
        /// <param name="query">the command you want to have executed</param>
        /// <returns></returns>
        public object ExecuteScalar(string query, params SqlParameter[] parameters)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    if (parameters != null)
                    {
                        cmd.Parameters.AddRange(parameters);
                    }

                    object result = cmd.ExecuteScalar();
                    return result;
                }
            }
        }

        /// <summary>
        /// this function will execute a query and return the number of rows affected.
        /// ExecuteNonQuery is used for insert, update, delete commands
        /// </summary>
        /// <param name="query">the command you want to have executed</param>
        /// <returns></returns>
        public int ExecuteQueryNonQuery(string query)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                if (conn.State == ConnectionState.Closed)
                {
                    conn.Open();
                }
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                    return cmd.ExecuteNonQuery();
                }
            }
        }

        /// <summary>
        /// this is used to get information from the database and return it as a datatable
        /// </summary>
        /// <param name="query">the command you want to have executed</param>
        /// <returns></returns>
        public DataTable GetDataTable(string query)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                if (conn.State == ConnectionState.Closed)
                {
                    conn.Open();
                }
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    adapter.Fill(dt);
                    return dt;
                }
            }
        }
    }
}
