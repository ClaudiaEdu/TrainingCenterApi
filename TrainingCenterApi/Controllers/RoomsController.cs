using Microsoft.AspNetCore.Mvc;
using TrainingCenterApi.Data;
using TrainingCenterApi.Models;

namespace TrainingCenterApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RoomsController : ControllerBase
{
    [HttpGet]
    public ActionResult<IEnumerable<Room>> GetAll(
        [FromQuery] int? minCapacity,
        [FromQuery] bool? hasProjector,
        [FromQuery] bool? activeOnly)
    {
        IEnumerable<Room> rooms = InMemoryDataStore.Rooms;

        if (minCapacity.HasValue)
        {
            rooms = rooms.Where(room => room.Capacity >= minCapacity.Value);
        }

        if (hasProjector.HasValue)
        {
            rooms = rooms.Where(room => room.HasProjector == hasProjector.Value);
        }

        if (activeOnly == true)
        {
            rooms = rooms.Where(room => room.IsActive);
        }

        return Ok(rooms);
    }

    [HttpGet("{id:int}")]
    public ActionResult<Room> GetById([FromRoute] int id)
    {
        var room = InMemoryDataStore.Rooms.FirstOrDefault(room => room.Id == id);

        if (room is null)
        {
            return NotFound();
        }

        return Ok(room);
    }

    [HttpGet("building/{buildingCode}")]
    public ActionResult<IEnumerable<Room>> GetByBuildingCode([FromRoute] string buildingCode)
    {
        var rooms = InMemoryDataStore.Rooms
            .Where(room => string.Equals(room.BuildingCode, buildingCode, StringComparison.OrdinalIgnoreCase));

        return Ok(rooms);
    }

    [HttpPost]
    public ActionResult<Room> Create([FromBody] Room room)
    {
        lock (InMemoryDataStore.SyncRoot)
        {
            room.Id = InMemoryDataStore.Rooms.Count == 0
                ? 1
                : InMemoryDataStore.Rooms.Max(existingRoom => existingRoom.Id) + 1;

            InMemoryDataStore.Rooms.Add(room);
        }

        return CreatedAtAction(nameof(GetById), new { id = room.Id }, room);
    }

    [HttpPut("{id:int}")]
    public ActionResult<Room> Update([FromRoute] int id, [FromBody] Room room)
    {
        lock (InMemoryDataStore.SyncRoot)
        {
            var existingRoom = InMemoryDataStore.Rooms.FirstOrDefault(existingRoom => existingRoom.Id == id);

            if (existingRoom is null)
            {
                return NotFound();
            }

            existingRoom.Name = room.Name;
            existingRoom.BuildingCode = room.BuildingCode;
            existingRoom.Floor = room.Floor;
            existingRoom.Capacity = room.Capacity;
            existingRoom.HasProjector = room.HasProjector;
            existingRoom.IsActive = room.IsActive;

            return Ok(existingRoom);
        }
    }

    [HttpDelete("{id:int}")]
    public IActionResult Delete([FromRoute] int id)
    {
        lock (InMemoryDataStore.SyncRoot)
        {
            var room = InMemoryDataStore.Rooms.FirstOrDefault(room => room.Id == id);

            if (room is null)
            {
                return NotFound();
            }

            var hasReservations = InMemoryDataStore.Reservations.Any(reservation => reservation.RoomId == id);

            if (hasReservations)
            {
                return Conflict("Room cannot be deleted because it has related reservations.");
            }

            InMemoryDataStore.Rooms.Remove(room);
        }

        return NoContent();
    }
}
