using TrainingCenterApi.Models;

namespace TrainingCenterApi.Data;

public static class InMemoryDataStore
{
    public static readonly object SyncRoot = new();

    public static readonly List<Room> Rooms =
    [
        new Room
        {
            Id = 1,
            Name = "Lab 101",
            BuildingCode = "A",
            Floor = 1,
            Capacity = 18,
            HasProjector = true,
            IsActive = true
        },
        new Room
        {
            Id = 2,
            Name = "Lab 204",
            BuildingCode = "B",
            Floor = 2,
            Capacity = 24,
            HasProjector = true,
            IsActive = true
        },
        new Room
        {
            Id = 3,
            Name = "Sala Konsultacyjna 12",
            BuildingCode = "A",
            Floor = 0,
            Capacity = 8,
            HasProjector = false,
            IsActive = true
        },
        new Room
        {
            Id = 4,
            Name = "Audytorium 301",
            BuildingCode = "C",
            Floor = 3,
            Capacity = 60,
            HasProjector = true,
            IsActive = true
        },
        new Room
        {
            Id = 5,
            Name = "Sala Remontowana",
            BuildingCode = "B",
            Floor = 1,
            Capacity = 16,
            HasProjector = false,
            IsActive = false
        }
    ];

    public static readonly List<Reservation> Reservations =
    [
        new Reservation
        {
            Id = 1,
            RoomId = 2,
            OrganizerName = "Anna Kowalska",
            Topic = "Warsztaty z HTTP i REST",
            Date = new DateOnly(2026, 5, 10),
            StartTime = new TimeOnly(10, 0),
            EndTime = new TimeOnly(12, 30),
            Status = "confirmed"
        },
        new Reservation
        {
            Id = 2,
            RoomId = 1,
            OrganizerName = "Piotr Nowak",
            Topic = "Podstawy C#",
            Date = new DateOnly(2026, 5, 11),
            StartTime = new TimeOnly(9, 0),
            EndTime = new TimeOnly(11, 0),
            Status = "planned"
        },
        new Reservation
        {
            Id = 3,
            RoomId = 4,
            OrganizerName = "Marta Zielinska",
            Topic = "Konsultacje projektowe",
            Date = new DateOnly(2026, 5, 12),
            StartTime = new TimeOnly(13, 0),
            EndTime = new TimeOnly(15, 0),
            Status = "confirmed"
        },
        new Reservation
        {
            Id = 4,
            RoomId = 3,
            OrganizerName = "Jan Wisniewski",
            Topic = "Przeglad prac zaliczeniowych",
            Date = new DateOnly(2026, 5, 13),
            StartTime = new TimeOnly(12, 0),
            EndTime = new TimeOnly(13, 30),
            Status = "planned"
        },
        new Reservation
        {
            Id = 5,
            RoomId = 2,
            OrganizerName = "Ewa Mazur",
            Topic = "REST API w praktyce",
            Date = new DateOnly(2026, 5, 10),
            StartTime = new TimeOnly(14, 0),
            EndTime = new TimeOnly(16, 0),
            Status = "cancelled"
        }
    ];
}
