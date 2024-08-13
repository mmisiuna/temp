﻿using System;
using System.Collections.Generic;
using System.Linq;
using BeEventy.Data.Models;
using Microsoft.EntityFrameworkCore;
using PostgreSQL.Data;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using static BeEventy.Data.Models.Login;

namespace BeEventy.Data.Repositories
{
    public class EventRepository
    {
        private readonly AppDbContext _context;

        public EventRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<Event>> GetAllEventsAsync()
        {
            return await _context.Events.ToListAsync();
        }

        public async Task<Event> GetEventByIdAsync(int id)
        {
            return await _context.Events.FirstOrDefaultAsync(e => e.Id == id);
        }

        public async Task<Event> GetEventByNameAsync(string name)
        {
            return await _context.Events.FirstOrDefaultAsync(e => e.Name == name);
        }
        public async Task<List<Event>> SearchEventsByNameAsync(string searchTerm, string sortType)
        {
            var currentDate = DateTime.Now;
            IQueryable<Event> query = _context.Events
                .Where(e => e.DateOfEnd >= currentDate.AddDays(1));

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                query = query.Where(e => e.Name.ToLower().Contains(searchTerm.ToLower()));
            }

            switch (sortType.ToLower())
            {
                case "date":
                    query = query.OrderBy(e => e.DateOfStart);
                    break;
                case "closest":
                    query = query.OrderBy(e => Math.Abs((e.DateOfStart - currentDate).Ticks));
                    break;
                case "votes":
                default:
                    query = query.OrderByDescending(e => e.Pluses)
                                 .ThenByDescending(e => e.Minuses);
                    break;
            }

            return await query.ToListAsync();
        }

        public async Task<List<Event>> GetAllValidEventsAsync()
        {
            var currentDate = DateTime.Now;
            return await _context.Events
                .Where(e => e.DateOfEnd >= currentDate.AddDays(1))
                .ToListAsync();
        }
        public async Task AddEventAsync(Event ev)
        {
            _context.Events.Add(ev);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteEventAsync(int id)
        {
            var ev = await _context.Events.FindAsync(id);
            if (ev != null)
            {
                _context.Events.Remove(ev);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> UserHasVotedForEventAsync(int userId, int eventId)
        {
            return await _context.Votes
                .AnyAsync(uev => uev.UserId == userId && uev.EventId == eventId);
        }


        public async Task<bool> AddVoteToEventAsync(int eventId, int userId, bool isPlus)
        {
            // Sprawdź, czy istnieje wydarzenie o podanym identyfikatorze
            var eventExists = await _context.Events.FindAsync(eventId);
            if (eventExists == null)
            {
                return false; // Wydarzenie nie istnieje
            }

            // Sprawdź, czy użytkownik już oddał głos na to wydarzenie
            var userVote = await _context.Votes.FirstOrDefaultAsync(v => v.UserId == userId && v.EventId == eventId);
            if (userVote != null)
            {
                if (isPlus && userVote.IsPlus)
                {
                    return false; // Użytkownik już oddał plusa dla tego wydarzenia
                }
                else if (!isPlus && !userVote.IsPlus)
                {
                    return false; // Użytkownik już oddał minusa dla tego wydarzenia
                }
                else
                {
                    // Użytkownik zmienia swój głos, więc zmniejsz odpowiednio plusy lub minusy
                    if (isPlus)
                    {
                        eventExists.Pluses++;
                        eventExists.Minuses--;
                    }
                    else
                    {
                        eventExists.Pluses--;
                        eventExists.Minuses++;
                    }

                    userVote.IsPlus = isPlus; // Aktualizujemy informację o głosie w bazie danych
                }
            }
            else
            {
                // Tworzymy nowy głos
                var vote = new Vote
                {
                    UserId = userId,
                    EventId = eventId,
                    IsPlus = isPlus
                };

                // Dodajemy głos do bazy danych
                _context.Votes.Add(vote);

                // Zwiększamy odpowiednio liczbę plusów lub minusów dla wydarzenia
                if (isPlus)
                {
                    eventExists.Pluses++;
                }
                else
                {
                    eventExists.Minuses++;
                }
            }

            try
            {
                // Zapisujemy zmiany w bazie danych
                await _context.SaveChangesAsync();
                return true; // Pomyślnie dodano głos do wydarzenia
            }
            catch (Exception ex)
            {
                // Obsługa błędu zapisu
                return false; // Wystąpił błąd podczas dodawania głosu do wydarzenia
            }
        }
        public async Task<bool> ReportEventAsync(int eventId)
        {
            var ev = await _context.Events.FindAsync(eventId);
            if (ev == null || ev.IsConfirmed)
            {
                Console.WriteLine("Zgłoszone wydarzenie zostało już sprawdzone i zatwierdzone przez moderację.");
                return false;
            }

            ev.NumberOfReports++;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UndoReportEventAsync(int eventId)
        {
            var ev = await _context.Events.FindAsync(eventId);
            if (ev == null || ev.IsConfirmed)
            {
                Console.WriteLine("Zgłoszone wydarzenie zostało już sprawdzone i zatwierdzone przez moderację.");
                return false;
            }

            if (ev.NumberOfReports > 0)
            {
                ev.NumberOfReports--;
            }

            await _context.SaveChangesAsync();
            return true;
        }
        public async Task<bool> HasUserReportedEventAsync(int eventId, int userId)
        {
            return await _context.Reports
                .AnyAsync(r => r.EventId == eventId && r.UserId == userId);
        }

        public async Task<bool> AddPlusToEventAsync(int eventId, int userId)
        {
            return await AddVoteToEventAsync(eventId, userId, true);
        }

        public async Task<bool> AddMinusToEventAsync(int eventId, int userId)
        {
            return await AddVoteToEventAsync(eventId, userId, false);
        }



        public async Task UpdateEventAsync(Event updatedEvent)
        {
            _context.Entry(updatedEvent).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }
        public async Task UpdateEventLocationAsync(Event updatedEvent)
        {
            _context.Entry(updatedEvent).Property(e => e.Location).IsModified = true;
            await _context.SaveChangesAsync();
        }
        public async Task UpdateEventTypeAsync(Event updatedEvent)
        {
            _context.Entry(updatedEvent).Property(e => e.EventType).IsModified = true;
            await _context.SaveChangesAsync();
        }
        public async Task UpdateEventStatusAsync(Event updatedEvent)
        {
            _context.Entry(updatedEvent).Property(e => e.EventStatus).IsModified = true;
            await _context.SaveChangesAsync();
        }
        public async Task<List<Event>> SortEventsByDateAsync()
        {
            var currentDate = DateTime.UtcNow;
            var validEvents = await _context.Events
                .Where(e => e.DateOfEnd >= currentDate.AddDays(1))
                .OrderBy(e => DateTime.SpecifyKind(e.DateOfStart, DateTimeKind.Utc))
                .ToListAsync();

            // Konwersja dat na UTC przed zwróceniem wyników
            foreach (var ev in validEvents)
            {
                ev.DateOfStart = DateTime.SpecifyKind(ev.DateOfStart, DateTimeKind.Utc);
                ev.DateOfEnd = DateTime.SpecifyKind(ev.DateOfEnd, DateTimeKind.Utc);
                ev.DateOfUpload = DateTime.SpecifyKind(ev.DateOfUpload, DateTimeKind.Utc);
            }

            return validEvents;
        }
        public async Task<List<Event>> SortEventsByClosestDateAsync()
        {
            var currentDate = DateTime.Now;
            // Pobierz wydarzenia, które kończą się po bieżącej dacie plus jeden dzień
            var validEvents = await _context.Events
                .Where(e => e.DateOfEnd >= currentDate.AddDays(1))
                .ToListAsync();

            // Konwersja dat na UTC przed sortowaniem
            foreach (var ev in validEvents)
            {
                ev.DateOfStart = DateTime.SpecifyKind(ev.DateOfStart, DateTimeKind.Utc);
                ev.DateOfEnd = DateTime.SpecifyKind(ev.DateOfEnd, DateTimeKind.Utc);
                ev.DateOfUpload = DateTime.SpecifyKind(ev.DateOfUpload, DateTimeKind.Utc);
            }

            // Sortowanie na poziomie klienta
            var sortedEvents = validEvents
                .OrderBy(e => Math.Abs((e.DateOfStart - currentDate).Ticks))
                .ToList();

            return sortedEvents;
        }


        public async Task<List<Event>> SortEventsByVotesAsync()
        {
            var currentDate = DateTime.Now;
            var validEvents = await _context.Events
                .Where(e => e.DateOfEnd >= currentDate.AddDays(1)) // Dodajemy warunek daty zakończenia
                .OrderByDescending(e => e.Pluses)
                .ThenByDescending(e => e.Minuses)
                .ToListAsync();
            // Konwersja dat na UTC przed zwróceniem wyników
            foreach (var ev in validEvents)
            {
                ev.DateOfStart = DateTime.SpecifyKind(ev.DateOfStart, DateTimeKind.Utc);
                ev.DateOfEnd = DateTime.SpecifyKind(ev.DateOfEnd, DateTimeKind.Utc);
                ev.DateOfUpload = DateTime.SpecifyKind(ev.DateOfUpload, DateTimeKind.Utc);
            }
            return validEvents;
        }
        public async Task<List<Event>> GetEventsExcludingTypesAsync(List<int> excludedEventTypes)
        {
            return await _context.Events
                .Where(e => !excludedEventTypes.Contains((int)e.EventType))
                .ToListAsync();
        }

    }
}
