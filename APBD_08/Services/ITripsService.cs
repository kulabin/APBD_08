using APBD_08.Models_DTOS.DTOs;

namespace APBD_08.Services;

public interface ITripsService
{
    Task<List<TripDTO>> GetTrips();
    Task<List<TripDTO>> GetClientTrips(int id);
    Task<bool> ClientExists(int clientId);
    Task<ClientDTO> CreateClient(ClientDTO client);
    Task UpdateTrip(int clientId, int tripId);
    Task DeleteClient(int clientId, int tripId);
}