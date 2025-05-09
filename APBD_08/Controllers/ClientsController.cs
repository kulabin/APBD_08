using APBD_08.Models_DTOS.DTOs;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using APBD_08.Services;

namespace APBD_08.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ClientsController : ControllerBase
    {
        private readonly ITripsService _tripsService;

        public ClientsController(ITripsService tripsService)
        {
            _tripsService = tripsService;   
        }
        
        [HttpGet("{id}/trips")]
        public async Task<IActionResult> GetClientTrips(int id)
        {
            try
            {
                var clientTrips = await _tripsService.GetClientTrips(id);
                return Ok(clientTrips);
            }
            catch (ArgumentException ex)
            {
                return NotFound(ex.Message);
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateClient(ClientDTO client)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var newClient = await _tripsService.CreateClient(client);
                return CreatedAtAction(nameof(GetClientTrips), new { id = newClient.Id }, newClient);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("{id}/trips/{tripId}")]
        public async Task<IActionResult> UpdateTrip(int id, int tripId)
        {
            try
            {
                await _tripsService.UpdateTrip(id, tripId);
                return Ok("Client successfully registered for the trip");
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("{id}/trips/{tripId}")]
        public async Task<IActionResult> DeleteTrip(int id, int tripId)
        {
            try
            {
                await _tripsService.DeleteClient(id, tripId);
                return Ok("Trip registration successfully deleted");
            }
            catch (ArgumentException ex)
            {
                return NotFound(ex.Message);
            }
        }
    }
}