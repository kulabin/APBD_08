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
            var clientTrips = await _tripsService.GetClientTrips(id);
            return Ok(clientTrips);
        }

        [HttpPost]
        public async Task<IActionResult> CreateClient()
        {
            return Ok();
        }

        [HttpPut("{id}/trips/{tripId}")]
        public async Task<IActionResult> UpdateTrip(int id, int tripId)
        {
            return Ok();
        }

        [HttpDelete("{id}/trips/{tripId}")]
        public async Task<IActionResult> DeleteTrip(int id, int tripId)
        {
            return Ok();
        }
    }
}