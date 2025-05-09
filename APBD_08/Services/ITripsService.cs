using APBD_08.Models_DTOS.DTOs;

namespace APBD_08.Services;

public interface ITripsService
{
    Task<List<TripDTO>> GetTrips();
    Task<List<TripDTO>> GetClientTrips(int id);
}