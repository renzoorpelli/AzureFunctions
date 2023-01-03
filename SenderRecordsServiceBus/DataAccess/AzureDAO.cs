using DataAccess.Properties;
using Entidades.Domain.DTO;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Resources;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess
{
    public class AzureDAO
    {
        private readonly string connectionString;

        public AzureDAO(string connectionString)
        {
            if(this.connectionString is null)
            {
                this.connectionString = connectionString;
            }
        }
        /// <summary>
        /// metodo encargado de traer todos los registros de la base de datos que correspondan con la fecha actual
        /// </summary>
        /// <returns></returns>
        public async Task<List<VueloDTO>> GetNewRecordsFromDatabase()
        {
            var lista = new List<VueloDTO>();
            //string cadenaConexion = Resources.databaseConnection;
            if (connectionString is not null)
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    string query = "SELECT v.id, v.numero_vuelo, v.fecha_vuelo FROM registro_vuelos v WHERE CAST (v.fecha_vuelo AS date) = CAST( GETDATE() AS Date ) and v.registrado = 0";

                    SqlCommand cmd = new SqlCommand(query, connection);
                    await connection.OpenAsync();
                    SqlDataReader reader = cmd.ExecuteReader();
                    while (await reader.ReadAsync())
                    {
                        var objectFromDatabase = new VueloDTO
                        {
                            Id = reader.GetInt32(0),
                            NumeroVuelo = reader.GetString(1),
                            FechaVuelo = reader.GetDateTime(2),
                        };
                        lista.Add(objectFromDatabase);
                    }
                }
                if (lista.Count > 0)
                {
                    await UpdateRecordsAsRegistered(lista);
                }
            }
            return lista;
        }

        /// <summary>
        /// Metodo encargado de actualizar el estado de los registros para que no vuelvan a ser enviados a Azure Bus Service
        /// </summary>
        /// <param name="lista">listado de registros obteneidos de la base de datos</param>
        /// <returns></returns>
        public async Task UpdateRecordsAsRegistered(List<VueloDTO> lista)
        {
            //string cadenaConexion = Resources.databaseConnection;
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string query = "UPDATE registro_vuelos SET registro_vuelos.registrado = 1 WHERE registro_vuelos.id = @id";
                await connection.OpenAsync();
                foreach (var item in lista)
                {
                    SqlCommand cmd = new SqlCommand(query, connection);
                    cmd.Parameters.AddWithValue("@id", item.Id);
                    cmd.ExecuteNonQuery();
                }

            }
        }

        /// <summary>
        /// metodo encargado de registrar el mensaje en la base de datos
        /// </summary>
        /// <param name="messageModel"></param>
        /// <returns></returns>
        public async Task<bool> SetRecordsFromServiceBus(VueloDTO messageModel)
        {
            if (messageModel is not null)
            {
                //string cadenaConexion = Resources.databaseConnection;
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    await connection.OpenAsync();
                    string query = "INSERT INTO vuelos_dia (id, numero_vuelo,fecha_vuelo) VALUES (@id, @numeroVuelo, @fecha)";
                    SqlCommand cmd = new SqlCommand(query, connection);

                    cmd.Parameters.AddWithValue("@id", messageModel.Id);
                    cmd.Parameters.AddWithValue("@numeroVuelo", messageModel.NumeroVuelo);
                    cmd.Parameters.AddWithValue("@fecha", messageModel.FechaVuelo);
                    cmd.ExecuteNonQuery();
                    return true;
                }
            }
            return false;
        }
    }
}
