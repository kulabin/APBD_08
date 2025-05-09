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

        [HttpPost]
        public async Task<IActionResult> CreateClient()
        {
            return Ok();
        }

        [HttpGet("{id}/trips")]
        public async Task<IActionResult> GetClientTrips(int id)
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