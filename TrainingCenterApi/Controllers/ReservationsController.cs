using Microsoft.AspNetCore.Mvc;
using TrainingCenterApi.Data;
using TrainingCenterApi.Models;

namespace TrainingCenterApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReservationsController : ControllerBase
{
    [HttpGet]
    public ActionResult<IEnumerable<Reservation>> GetAll(
        [FromQuery] DateOnly? date,
        [FromQuery] string? status,
        [FromQuery] int? roomId)
    {
        IEnumerable<Reservation> reservations = InMemoryDataStore.Reservations;

        if (date.HasValue)
        {
            reservations = reservations.Where(reservation => reservation.Date == date.Value);
        }

        if (!string.IsNullOrWhiteSpace(status))
        {
            reservations = reservations.Where(reservation =>
                string.Equals(reservation.Status, status, StringComparison.OrdinalIgnoreCase));
        }

        if (roomId.HasValue)
        {
            reservations = reservations.Where(reservation => reservation.RoomId == roomId.Value);
        }

        return Ok(reservations);
    }

    [HttpGet("{id:int}")]
    public ActionResult<Reservation> GetById([FromRoute] int id)
    {
        var reservation = InMemoryDataStore.Reservations.FirstOrDefault(reservation => reservation.Id == id);

        if (reservation is null)
        {
            return NotFound();
        }

        return Ok(reservation);
    }

    [HttpPost]
    public ActionResult<Reservation> Create([FromBody] Reservation reservation)
    {
        lock (InMemoryDataStore.SyncRoot)
        {
            var roomValidationResult = ValidateRoomForReservation(reservation.RoomId);

            if (roomValidationResult is not null)
            {
                return roomValidationResult;
            }

            if (HasTimeConflict(reservation))
            {
                return Conflict("Reservation conflicts with an existing reservation for the same room.");
            }

            reservation.Id = InMemoryDataStore.Reservations.Count == 0
                ? 1
                : InMemoryDataStore.Reservations.Max(existingReservation => existingReservation.Id) + 1;

            InMemoryDataStore.Reservations.Add(reservation);
        }

        return CreatedAtAction(nameof(GetById), new { id = reservation.Id }, reservation);
    }

    [HttpPut("{id:int}")]
    public ActionResult<Reservation> Update([FromRoute] int id, [FromBody] Reservation reservation)
    {
        lock (InMemoryDataStore.SyncRoot)
        {
            var existingReservation = InMemoryDataStore.Reservations.FirstOrDefault(existingReservation => existingReservation.Id == id);

            if (existingReservation is null)
            {
                return NotFound();
            }

            var roomValidationResult = ValidateRoomForReservation(reservation.RoomId);

            if (roomValidationResult is not null)
            {
                return roomValidationResult;
            }

            reservation.Id = id;

            if (HasTimeConflict(reservation, id))
            {
                return Conflict("Reservation conflicts with an existing reservation for the same room.");
            }

            existingReservation.RoomId = reservation.RoomId;
            existingReservation.OrganizerName = reservation.OrganizerName;
            existingReservation.Topic = reservation.Topic;
            existingReservation.Date = reservation.Date;
            existingReservation.StartTime = reservation.StartTime;
            existingReservation.EndTime = reservation.EndTime;
            existingReservation.Status = reservation.Status;

            return Ok(existingReservation);
        }
    }

    [HttpDelete("{id:int}")]
    public IActionResult Delete([FromRoute] int id)
    {
        lock (InMemoryDataStore.SyncRoot)
        {
            var reservation = InMemoryDataStore.Reservations.FirstOrDefault(reservation => reservation.Id == id);

            if (reservation is null)
            {
                return NotFound();
            }

            InMemoryDataStore.Reservations.Remove(reservation);
        }

        return NoContent();
    }

    private static ActionResult? ValidateRoomForReservation(int roomId)
    {
        var room = InMemoryDataStore.Rooms.FirstOrDefault(room => room.Id == roomId);

        if (room is null)
        {
            return new NotFoundObjectResult("Room does not exist.");
        }

        if (!room.IsActive)
        {
            return new ConflictObjectResult("Room is inactive.");
        }

        return null;
    }

    private static bool HasTimeConflict(Reservation reservation, int? ignoredReservationId = null)
    {
        if (IsCancelled(reservation))
        {
            return false;
        }

        return InMemoryDataStore.Reservations.Any(existingReservation =>
            existingReservation.Id != ignoredReservationId &&
            existingReservation.RoomId == reservation.RoomId &&
            existingReservation.Date == reservation.Date &&
            !IsCancelled(existingReservation) &&
            reservation.StartTime < existingReservation.EndTime &&
            reservation.EndTime > existingReservation.StartTime);
    }

    private static bool IsCancelled(Reservation reservation)
    {
        return string.Equals(reservation.Status, "cancelled", StringComparison.OrdinalIgnoreCase);
    }
}
