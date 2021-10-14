using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZerodhaTaxHarvester.model;

namespace ZerodhaTaxHarvester
{
    class DatabaseHelper
    {
        private string connectionString;

        public DatabaseHelper(string connectionString)
        {
            this.connectionString = connectionString;
        }

        public void AddEntry(MFRecord mFRecord)
        {
            SqlConnection con = new SqlConnection(this.connectionString);
            var date = mFRecord.Date.ToString("yyyy-MM-dd");
            string query = $"insert into mfentry(schemeId,schemeName,PurchaseUnit,PurchaseDate,PurchaseUnitPrice) values({mFRecord.MutualFund.SchemeCode},'{mFRecord.MutualFund.Name}',{mFRecord.PurchaseUnit},'{date}','{mFRecord.PurchaseUnitPrice}')";
            SqlCommand command = new SqlCommand(query, con);            
            con.Open();
            command.ExecuteNonQuery();
            con.Close();
        }

        public List<MFRecord> ListAll()
        {
            SqlConnection con = new SqlConnection(this.connectionString);
            string query = $"select * from mfentry";
            SqlCommand command = new SqlCommand(query, con);            
            var mFRecords = new List<MFRecord>();
            con.Open();
            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    var mutualFund = new MutualFund(reader["schemeName"].ToString(), (int)reader["schemeId"]);
                    var mfRecord = new MFRecord { MutualFund = mutualFund, PurchaseUnit = (int)reader["PurchaseUnit"], Date = (DateTime)reader["PurchaseDate"],PurchaseUnitPrice=(double)reader["PurchaseUnitPrice"], DBId = (int)reader["Id"] };
                    mFRecords.Add(mfRecord);
                }
            }

            con.Close();
            return mFRecords;
        }


        public void DeleteEntry(int id)
        {
            SqlConnection con = new SqlConnection(this.connectionString);
            string query = $"delete from mfentry where Id = {id}";
            SqlCommand command = new SqlCommand(query, con);
            con.Open();
            int rowsAffected = command.ExecuteNonQuery();
            Console.WriteLine(rowsAffected);
            
            con.Close();

        }
    }
}
