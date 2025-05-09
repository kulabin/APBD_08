using System.Data;
using System.Data;
using Microsoft.Data.SqlClient;
using APBD_08.Models_DTOS.DTOs;


namespace APBD_08.Services;

public class TripsService : ITripsService
{
    private readonly string _connectionString =
        "Data Source=db-mssql;Initial Catalog=2019SBD;Integrated Security=True;Trust Server Certificate=True";

    public async Task<List<TripDTO>> GetTrips()
    {
        var trips = new List<TripDTO>();

        string command =
            "SELECT t.IdTrip, t.Name, t.Description, t.DateFrom, t.DateTo, t.MaxPeople, c.IdCountry, c.Name AS CountryName FROM Trip t LEFT JOIN Country_Trip ct ON t.IdTrip = ct.IdTrip LEFT JOIN Country c ON ct.IdCountry = c.IdCountry  ORDER BY t.IdTrip";

        using (SqlConnection conn = new SqlConnection(_connectionString))
        using (SqlCommand cmd = new SqlCommand(command, conn))
        {
            await conn.OpenAsync();

            using (var reader = await cmd.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    int idOrdinal = reader.GetOrdinal("IdTrip");
                    var trip = new TripDTO()
                    {
                        Id = reader.GetInt32(idOrdinal),
                        Name = reader.GetString("Name"),
                        Description = reader.GetString("Description"),
                        DateFrom = reader.GetDateTime("DateFrom"),
                        DateTo = reader.GetDateTime("DateTo"),
                        MaxPeople = reader.GetInt32("MaxPeople"),
                        Countries = new List<CountryDTO>()
                    };
                    var country = new CountryDTO()
                    {
                        IdCountry = reader.GetInt32("IdCountry"),
                        Name = reader.GetString("Name")
                    };

                    trip.Countries.Add(country);
                    trips.Add(trip);
                }
            }
        }

        return trips;
    }

    public async Task<List<TripDTO>> GetClientTrips(int id)
    {
        if (!await ClientExists(id))
        {
            throw new ArgumentException("Client does not exist");
        }
        
        var clientTrips = new List<TripDTO>();
        string command = @"
            SELECT t.IdTrip, t.Name, t.Description, t.DateFrom, t.DateTo, t.MaxPeople,
               c.IdCountry, c.Name AS CountryName,
               ct.RegisteredAt, ct.PaymentDate
            FROM Trip t
            INNER JOIN Client_Trip ct ON t.IdTrip = ct.IdTrip
            LEFT JOIN Country_Trip ctrip ON t.IdTrip = ctrip.IdTrip
            LEFT JOIN Country c ON c.IdCountry = ctrip.IdCountry
            WHERE ct.IdClient = @ClientId";

        using (SqlConnection conn = new SqlConnection(_connectionString))
        using (SqlCommand cmd = new SqlCommand(command, conn))
        {
            cmd.Parameters.AddWithValue("@ClientId", id);
            await conn.OpenAsync();

            using (var reader = await cmd.ExecuteReaderAsync())
            {
                int currentTripId = -1;
                TripDTO currentTrip = null;

                while (await reader.ReadAsync())
                {  
                    int tripId = reader.GetInt32("IdTrip");

                    if (currentTripId != tripId)
                    {
                        currentTrip = new TripDTO
                        {
                            Id = tripId,
                            Name = reader.GetString("Name"),
                            Description = reader.GetString("Description"),
                            DateFrom = reader.GetDateTime("DateFrom"),
                            DateTo = reader.GetDateTime("DateTo"),
                            MaxPeople = reader.GetInt32("MaxPeople"),
                            Countries = new List<CountryDTO>(),
                            RegisteredAt = reader.GetDateTime("RegisteredAt"),
                            PaymentDate = reader.IsDBNull("PaymentDate") ? null : reader.GetDateTime("PaymentDate")
                        };
                        clientTrips.Add(currentTrip);
                        currentTripId = tripId;
                }

                    if (!reader.IsDBNull("IdCountry"))
                    {
                        currentTrip.Countries.Add(new CountryDTO
                        {
                            IdCountry = reader.GetInt32("IdCountry"),
                            Name = reader.GetString("CountryName")
                        });
                    }
                }
            }
        }

        if (!clientTrips.Any())
        {
            throw new ArgumentException("Client has no registered trips");
        }

        return clientTrips;
    }

    public async Task<bool> ClientExists(int clientId)
    {
        const string sql = "SELECT COUNT(1) FROM Client WHERE IdClient = @id";
        using var conn = new SqlConnection(_connectionString);
        using var cmd  = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@id", clientId);
        await conn.OpenAsync();
        int count = (int)await cmd.ExecuteScalarAsync();
        return count > 0;
    }

    public async Task<ClientDTO> CreateClient(ClientDTO client)
    {
        if (string.IsNullOrEmpty(client.FirstName) || string.IsNullOrEmpty(client.LastName) || 
            string.IsNullOrEmpty(client.Email) || string.IsNullOrEmpty(client.Telephone) || 
            string.IsNullOrEmpty(client.Pesel))
        {
            throw new ArgumentException("All fields are required");
        }

        string command = "INSERT INTO Client (FirstName, LastName, Email, Telephone, Pesel) " +
                         "OUTPUT INSERTED.IdClient " +
                         "VALUES (@FirstName, @LastName, @Email, @Telephone, @Pesel)";

        using (SqlConnection conn = new SqlConnection(_connectionString))
        using (SqlCommand cmd = new SqlCommand(command, conn))
        {
            cmd.Parameters.AddWithValue("@FirstName", client.FirstName);
            cmd.Parameters.AddWithValue("@LastName", client.LastName);
            cmd.Parameters.AddWithValue("@Email", client.Email);
            cmd.Parameters.AddWithValue("@Telephone", client.Telephone);
            cmd.Parameters.AddWithValue("@Pesel", client.Pesel);

            await conn.OpenAsync();
            int newId = (int)await cmd.ExecuteScalarAsync();

            return new ClientDTO
            {
                Id = newId,
                FirstName = client.FirstName,
                LastName = client.LastName,
                Email = client.Email,
                Telephone = client.Telephone,
                Pesel = client.Pesel
            };
        }
    }

    public async Task UpdateTrip(int clientId, int tripId)
    {
        if (!await ClientExists(clientId))
        {
            throw new ArgumentException("Client does not exist");
        }

        string checkTripCommand = @"
        SELECT t.MaxPeople, COUNT(ct.IdClient) as CurrentParticipants
        FROM Trip t
        LEFT JOIN Client_Trip ct ON t.IdTrip = ct.IdTrip
        WHERE t.IdTrip = @TripId
        GROUP BY t.MaxPeople";

        using (SqlConnection conn = new SqlConnection(_connectionString))
        {
            await conn.OpenAsync();

            using (SqlCommand checkCmd = new SqlCommand(checkTripCommand, conn))
            {
                checkCmd.Parameters.AddWithValue("@TripId", tripId);
                using (var reader = await checkCmd.ExecuteReaderAsync())
                {
                    if (!await reader.ReadAsync())
                    {
                        throw new ArgumentException("Trip does not exist");
                    }

                    int maxPeople = reader.GetInt32("MaxPeople");
                    int currentParticipants = reader.GetInt32("CurrentParticipants");

                    if (currentParticipants >= maxPeople)
                    {
                        throw new ArgumentException("Trip is full");
                    }
                }
            }

            string insertCommand = @"
            INSERT INTO Client_Trip (IdClient, IdTrip, RegisteredAt)
            VALUES (@ClientId, @TripId, @RegisteredAt)";

            using (SqlCommand insertCmd = new SqlCommand(insertCommand, conn))
            {
                insertCmd.Parameters.AddWithValue("@ClientId", clientId);
                insertCmd.Parameters.AddWithValue("@TripId", tripId);
                insertCmd.Parameters.AddWithValue("@RegisteredAt", DateTime.UtcNow);
                await insertCmd.ExecuteNonQueryAsync();
            }
        }
        
        
    }

    public async Task DeleteClient(int clientId, int tripId)
    {
        string checkCommand = @"
                SELECT COUNT(1) 
                FROM Client_Trip 
                WHERE IdClient = @ClientId AND IdTrip = @TripId";

        using (SqlConnection conn = new SqlConnection(_connectionString))
        {
            await conn.OpenAsync();

            using (SqlCommand checkCmd = new SqlCommand(checkCommand, conn))
            {
                checkCmd.Parameters.AddWithValue("@ClientId", clientId);
                checkCmd.Parameters.AddWithValue("@TripId", tripId);
            
                int exists = (int)await checkCmd.ExecuteScalarAsync();
                if (exists == 0)
                {
                    throw new ArgumentException("Registration not found");
                }
            }

            string deleteCommand = @"
                DELETE FROM Client_Trip 
                WHERE IdClient = @ClientId AND IdTrip = @TripId";

            using (SqlCommand deleteCmd = new SqlCommand(deleteCommand, conn))
            {
                deleteCmd.Parameters.AddWithValue("@ClientId", clientId);
                deleteCmd.Parameters.AddWithValue("@TripId", tripId);
                await deleteCmd.ExecuteNonQueryAsync();
            }
        }
    }
}