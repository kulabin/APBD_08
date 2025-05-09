using System.Data;
using System.Data;
using Microsoft.Data.SqlClient;
using APBD_08.Models_DTOS.DTOs;


namespace APBD_08.Services;

public class TripsService : ITripsService
{
    private readonly string _connectionString = "Data Source=db-mssql;Initial Catalog=2019SBD;Integrated Security=True;Trust Server Certificate=True";
    
    public async Task<List<TripDTO>> GetTrips()
    {
        var trips = new List<TripDTO>();

        string command = "SELECT Trip.IdTrip, Trip.Name, Description, DateFrom, DateTo, MaxPeople, C.IdCountry as \"countryId\",C.Name as \"countryName\"FROM TripLEFT JOIN Country_Trip CT on Trip.IdTrip = CT.IdTripLEFT JOIN Country C on C.IdCountry = CT.IdCountry";
        
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
}