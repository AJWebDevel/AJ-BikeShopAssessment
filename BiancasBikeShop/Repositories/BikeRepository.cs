using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using BiancasBikeShop.Models;
using Microsoft.Extensions.Configuration;

namespace BiancasBikeShop.Repositories
{
    public class BikeRepository : IBikeRepository
    {
        private SqlConnection Connection
        {
            get
            {
                return new SqlConnection("server=localhost\\SQLExpress;database=BiancasBikeShop;integrated security=true;TrustServerCertificate=true");
            }
        }

        public List<Bike> GetAllBikes()
        {
            using (var conn = Connection) 
            {
                conn.Open();
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"SELECT b.Id AS BikeId, b.Brand, b.Color, b.OwnerId AS BikeOwnerId,
	                                    o.Id AS OwnerId, o.Name AS OwnerName
                                    FROM Bike b
                                    JOIN Owner o ON o.Id = b.OwnerId;";

                    SqlDataReader reader = cmd.ExecuteReader();

                    var bikes = new List<Bike>();

                        while(reader.Read())
                        {
                        bikes.Add(new Bike
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("BikeId")),
                            Brand = reader.GetString(reader.GetOrdinal("Brand")),
                            Color = reader.GetString(reader.GetOrdinal("Color")),
                            Owner = new Owner()
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("OwnerId")),
                                Name = reader.GetString(reader.GetOrdinal("OwnerName"))
                            }
                        });
                        }   
                    return bikes;
                    }
                }
            }

           
        
       

        public Bike GetBikeById(int id)
        {
            using (var conn = Connection)
            {
                conn.Open();
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"SELECT b.Id AS BikeId, b.Brand, b.Color, b.OwnerId AS BikeOwnerId, b.BikeTypeId,
	                                    o.Id AS OwnerId, o.Name AS OwnerName, o.Address,
	                                    w.DateInitiated, w.BikeId AS OrderBikeId, w.DateCompleted, w.Description, w.Id AS OrderId,
	                                    bT.Id AS TypeId, bT.Name AS TypeName
                                     FROM Bike b
                                     JOIN BikeType bT ON bT.Id = b.BikeTypeId
                                     JOIN Owner o ON o.Id = b.OwnerId
                                     JOIN WorkOrder w ON w.BikeId = b.Id
                                    WHERE b.Id = @id;";

                    cmd.Parameters.AddWithValue("@id", id);

                    SqlDataReader reader = cmd.ExecuteReader();

                    Bike bike = null;

                    while (reader.Read())
                    {
                        if (bike == null)
                        {
                        bike = new Bike()
                        {
                            Id = id,
                            Brand = reader.GetString(reader.GetOrdinal("Brand")),
                            Color = reader.GetString(reader.GetOrdinal("Color")),
                            BikeType = new BikeType()
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("TypeId")),
                                Name = reader.GetString(reader.GetOrdinal("TypeName"))
                            },
                            Owner = new Owner()
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("OwnerId")),
                                Name = reader.GetString(reader.GetOrdinal("OwnerName")),
                                Address = reader.GetString(reader.GetOrdinal("Address"))
                            },
                            WorkOrders = new List<WorkOrder>() 
                        };
                        }
                        if (!reader.IsDBNull(reader.GetOrdinal("OrderId")))
                        {
                            while (reader.Read())
                            {
                            WorkOrder workOrder = new WorkOrder()
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("OrderId")),
                                
                                
                                DateCompleted = !reader.IsDBNull(reader.GetOrdinal("DateCompleted")) ? reader.GetDateTime(reader.GetOrdinal("DateCompleted")) : null ,
                                Description = reader.GetString(reader.GetOrdinal("Description")),
                                DateInitiated = reader.GetDateTime(reader.GetOrdinal("DateInitiated")),
                                Bike = new Bike()
                                {
                                    Id = id,
                                    Brand = reader.GetString(reader.GetOrdinal("Brand")),
                                    Color = reader.GetString(reader.GetOrdinal("Color")),
                                    BikeType = new BikeType()
                                    {
                                        Id = reader.GetInt32(reader.GetOrdinal("TypeId")),
                                        Name = reader.GetString(reader.GetOrdinal("TypeName"))
                                    },
                                    Owner = new Owner()
                                    {
                                        Id = reader.GetInt32(reader.GetOrdinal("OwnerId")),
                                        Name = reader.GetString(reader.GetOrdinal("OwnerName"))
                                    }

                                }

                            };
                            bike.WorkOrders.Add(workOrder);
                            }
                        
                        }
                        
                    }
                    return bike;
                }
            }
           
        }

        public int GetBikesInShopCount()
        {
         
            using (var conn = Connection)
            {
                conn.Open();
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"SELECT DISTINCT Bike.Id AS BikeId
                                        FROM Bike
                                        INNER JOIN WorkOrder ON Bike.Id = WorkOrder.BikeId 
                                        WHERE WorkOrder.DateCompleted IS NULL  ";
                    SqlDataReader reader = cmd.ExecuteReader();

                    var bikes = new List<Bike>();
                    while (reader.Read())
                    {
                        bikes.Add(new Bike ()
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("BikeId"))
                     
                        });
                    }
                    return bikes.Count;
                }


            }
        }
    }
}
 