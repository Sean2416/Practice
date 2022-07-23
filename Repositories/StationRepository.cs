using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics.CodeAnalysis;
using TestApi.Models;

namespace TradevanPackage.SqlServer
{
    public class StationRepository
    {
        public string ConnectionString { get; set; }


        public StationRepository(IConfiguration configuration)
        {
            ConnectionString = configuration != null ? configuration.GetValue<string>("DatabaseSettings:ConnectionString") : throw new ArgumentNullException(nameof(configuration));
        }

        public bool CreateStation(Station entity)
        {
            using SqlConnection con = new SqlConnection(ConnectionString);
            try
            {
                string sql = $"INSERT INTO dbo.Station (Code, Name, CreatedDate, LastModifiedDate) " +
                      "VALUES (@Code, @Name, @CreatedDate, @LastModifiedDate) ";

                using SqlCommand cmd = new SqlCommand(sql, con);
                cmd.Parameters.Add("@Code", SqlDbType.VarChar).Value = entity.Code;
                cmd.Parameters.Add("@Name", SqlDbType.NVarChar).Value = entity.Name;
                cmd.Parameters.Add("@CreatedDate", SqlDbType.DateTime).Value = DateTime.Now;
                cmd.Parameters.Add("@LastModifiedDate", SqlDbType.DateTime).Value = DateTime.Now;
                con.Open();
                cmd.ExecuteNonQuery();
                con.Close();
                return true;
            }
            catch (SqlException e)
            {
                con.Close();
                return false;
            }
        }

        /// <summary>
        /// 查詢Log紀錄
        /// </summary>
        public List<Station> GetStations()
        {
            var result = new List<Station>();
            using SqlConnection con = new SqlConnection(ConnectionString);
            try
            {
                string sql = $"select * FROM  dbo.Station";

                SqlCommand cmd = new SqlCommand(sql, con);
                con.Open();
                SqlDataReader sdr = cmd.ExecuteReader();
                while (sdr.Read())
                {
                    result.Add(new Station
                    {
                        Code = sdr["Code"].ToString(),
                        Name = sdr["Name"].ToString(),
                        CreatedDate = DateTime.Parse(sdr["CreatedDate"].ToString()),
                        LastModifiedDate = DateTime.Parse(sdr["LastModifiedDate"].ToString())
                    });
                }
            }
            catch (SqlException e)
            {
                throw e;
            }
            finally
            {
                con.Close();
            }
            return result;
        }
    }
}
