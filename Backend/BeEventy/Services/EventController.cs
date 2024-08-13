using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using BeEventy.Data.Models;
using BeEventy.Data.Repositories;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using BeEventy.Data;
using System.Xml.Xsl;
using static BeEventy.Data.Models.Login;
using BeEventy.Data.Enums;
using System.IdentityModel.Tokens.Jwt;

namespace BeEventy.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EventController : ControllerBase
    {
        private readonly EventRepository _eventRepository;
        private readonly AccountRepository _accountRepository;
        private readonly AppDbContext _context;

        public EventController(EventRepository eventRepository, AccountRepository accountRepository, AppDbContext context)
        {
            _eventRepository = eventRepository;
            _accountRepository = accountRepository;
            _context = context;
        }

        [HttpGet("getAllEvents")]
        public async Task<ActionResult<IEnumerable<Event>>> GetAllEvents()
        {
            var events = await _eventRepository.GetAllEventsAsync();
            return Ok(events);
        }

        [HttpGet("id/{id}")]
        public async Task<ActionResult<Event>> GetEventById(int id)
        {
            var ev = await _eventRepository.GetEventByIdAsync(id);
            if (ev == null)
            {
                return NotFound();
            }
            return Ok(ev);
        }

        [HttpGet("name/{name}")]
        public async Task<ActionResult<Event>> GetEventByName(string name)
        {
            var ev = await _eventRepository.GetEventByNameAsync(name);
            if (ev == null)
            {
                return NotFound();
            }
            return Ok(ev);
        }

        [HttpGet("search")]
        public async Task<IActionResult> SearchEvents(string searchTerm, string sortType)
        {
            var events = await _eventRepository.SearchEventsByNameAsync(searchTerm, sortType);
            return Ok(events);
        }


        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateEvent(int id, Event updatedEvent)
        {
            if (id != updatedEvent.Id)
            {
                return BadRequest("Event ID mismatch.");
            }

            var existingEvent = await _eventRepository.GetEventByIdAsync(id);
            if (existingEvent == null)
            {
                return NotFound("Event not found.");
            }

            await _eventRepository.UpdateEventAsync(updatedEvent);
            return NoContent();
        }
        [HttpPut("{id}/location")]
        public async Task<IActionResult> UpdateEventLocation(int id, [FromBody] Location newLocation)
        {
            var existingEvent = await _eventRepository.GetEventByIdAsync(id);
            if (existingEvent == null)
            {
                return NotFound("Event not found.");
            }

            existingEvent.Location = newLocation;
            await _eventRepository.UpdateEventLocationAsync(existingEvent);
            return NoContent();
        }
        [HttpPut("{id}/type")]
        public async Task<IActionResult> UpdateEventLType(int id, [FromBody] EventType newType)
        {
            var existingEvent = await _eventRepository.GetEventByIdAsync(id);
            if (existingEvent == null)
            {
                return NotFound("Event not found.");
            }

            existingEvent.EventType = newType;
            await _eventRepository.UpdateEventTypeAsync(existingEvent);
            return NoContent();
        }
        [HttpPut("{id}/status")]
        public async Task<IActionResult> UpdateEventStatus(int id, [FromBody] EventStatus newStatus)
        {
            var existingEvent = await _eventRepository.GetEventByIdAsync(id);
            if (existingEvent == null)
            {
                return NotFound("Event not found.");
            }

            existingEvent.EventStatus = newStatus;
            await _eventRepository.UpdateEventStatusAsync(existingEvent);
            return NoContent();
        }

        [HttpPost]
        public async Task<ActionResult<Event>> AddEvent(Event ev)
        {
            await _eventRepository.AddEventAsync(ev);
            return CreatedAtAction(nameof(GetAllEvents), new { id = ev.Id }, ev);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteEventById(int id)
        {
            await _eventRepository.DeleteEventAsync(id);
            return NoContent();
        }

        [HttpPost("{eventId}/plus")]
        public async Task<IActionResult> AddPlusToEvent(int eventId, [FromBody] LoginResponse loginResponse)
        {
            if (loginResponse == null || string.IsNullOrEmpty(loginResponse.Token) || loginResponse.UserId <= 0)
            {
                return BadRequest("Invalid request data.");
            }

            var handler = new JwtSecurityTokenHandler();
            try
            {
                var jsonToken = handler.ReadToken(loginResponse.Token) as JwtSecurityToken;
                if (jsonToken == null)
                {
                    return Unauthorized();
                }

                var success = await _eventRepository.AddPlusToEventAsync(eventId, loginResponse.UserId);
                if (success)
                {
                    var updatedEvent = await _eventRepository.GetEventByIdAsync(eventId);
                    return Ok(updatedEvent);
                }
                else
                {
                    return BadRequest("Nie udało się dodać plusa do wydarzenia.");
                }
            }
            catch (Exception ex)
            {
                return Unauthorized();
            }
        }

        [HttpPost("{eventId}/minus")]
        public async Task<IActionResult> AddMinusToEvent(int eventId, [FromBody] LoginResponse loginResponse)
        {
            if (loginResponse == null || string.IsNullOrEmpty(loginResponse.Token) || loginResponse.UserId <= 0)
            {
                return BadRequest("Invalid request data.");
            }

            var handler = new JwtSecurityTokenHandler();
            try
            {
                var jsonToken = handler.ReadToken(loginResponse.Token) as JwtSecurityToken;
                if (jsonToken == null)
                {
                    return Unauthorized();
                }

                var success = await _eventRepository.AddMinusToEventAsync(eventId, loginResponse.UserId);
                if (success)
                {
                    var updatedEvent = await _eventRepository.GetEventByIdAsync(eventId);
                    return Ok(updatedEvent);
                }
                else
                {
                    return BadRequest("Nie udało się dodać minusa do wydarzenia.");
                }
            }
            catch (Exception ex)
            {
                return Unauthorized();
            }
        }
        [HttpPost("{eventId}/report")]
        public async Task<IActionResult> ReportEvent(int eventId, [FromBody] LoginResponse loginResponse)
        {
            if (loginResponse == null || string.IsNullOrEmpty(loginResponse.Token) || loginResponse.UserId <= 0)
            {
                return BadRequest("Invalid request data.");
            }

            var handler = new JwtSecurityTokenHandler();
            try
            {
                var jsonToken = handler.ReadToken(loginResponse.Token) as JwtSecurityToken;
                if (jsonToken == null)
                {
                    return Unauthorized();
                }

                var success = await _eventRepository.ReportEventAsync(eventId);
                if (success)
                {
                    var updatedEvent = await _eventRepository.GetEventByIdAsync(eventId);
                    return Ok(updatedEvent);
                }
                else
                {
                    return BadRequest("Nie udało się zgłosić wydarzenia.");
                }
            }
            catch (Exception ex)
            {
                return Unauthorized();
            }
        }

        [HttpPost("{eventId}/undo-report")]
        public async Task<IActionResult> UndoReportEvent(int eventId, [FromBody] LoginResponse loginResponse)
        {
            if (loginResponse == null || string.IsNullOrEmpty(loginResponse.Token) || loginResponse.UserId <= 0)
            {
                return BadRequest("Invalid request data.");
            }

            var handler = new JwtSecurityTokenHandler();
            try
            {
                var jsonToken = handler.ReadToken(loginResponse.Token) as JwtSecurityToken;
                if (jsonToken == null)
                {
                    return Unauthorized();
                }

                var success = await _eventRepository.UndoReportEventAsync(eventId);
                if (success)
                {
                    var updatedEvent = await _eventRepository.GetEventByIdAsync(eventId);
                    return Ok(updatedEvent);
                }
                else
                {
                    return BadRequest("Nie udało się cofnąć zgłoszenia.");
                }
            }
            catch (Exception ex)
            {
                return Unauthorized();
            }
        }
        [HttpGet("{eventId}/has-reported")]
        public async Task<IActionResult> HasUserReportedEvent(int eventId, [FromQuery] int userId)
        {
            if (userId <= 0)
            {
                return BadRequest("Invalid user ID.");
            }

            var hasReported = await _eventRepository.HasUserReportedEventAsync(eventId, userId);
            return Ok(new { hasReported });
        }


        [HttpGet("sort/date")]
        public async Task<ActionResult<IEnumerable<Event>>> SortEventsByDate()
        {
            var sortedEvents = await _eventRepository.SortEventsByDateAsync();
            return Ok(sortedEvents);
        }
        [HttpGet("sort/closest")]
        public async Task<ActionResult<IEnumerable<Event>>> SortEventsByClosestDate()
        {
            var sortedEvents = await _eventRepository.SortEventsByClosestDateAsync();
            return Ok(sortedEvents);
        }


        [HttpGet("sort/votes")]
        public async Task<ActionResult<IEnumerable<Event>>> SortEventsByVotes()
        {
            var sortedEvents = await _eventRepository.SortEventsByVotesAsync();
            return Ok(sortedEvents);
        }
        [HttpGet("getAllValidEvents")]
        public async Task<ActionResult<IEnumerable<Event>>> SortValidEventsByDate()
        {
            var sortedEvents = await _eventRepository.GetAllValidEventsAsync();
            return Ok(sortedEvents);
        }

    }
}
