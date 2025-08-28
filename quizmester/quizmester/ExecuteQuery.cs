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
        public string connectionString { get; } = "Data Source=localhost\\sqlexpress;Initial Catalog=database;Integrated Security=True;TrustServerCertificate=True";

        public int ExecuteQueryCount(string query)
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
                    return (int)cmd.ExecuteScalar();
                }
            }
        }

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
    }
}
