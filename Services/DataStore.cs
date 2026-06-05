using System.Text.Json;
using SmartTransport.Models;

namespace SmartTransport.Services
{
    /// <summary>
    /// Saves and loads all data to/from JSON files.
    /// Each entity type has its own file.
    /// New records are appended; updates overwrite only the changed record.
    /// </summary>
    public static class DataStore
    {
        private static readonly string DataDir = Path.Combine(
            AppDomain.CurrentDomain.BaseDirectory, "data");

        private static readonly string UsersFile    = Path.Combine(DataDir, "users.json");
        private static readonly string VehiclesFile = Path.Combine(DataDir, "vehicles.json");
        private static readonly string BookingsFile = Path.Combine(DataDir, "bookings.json");

        private static readonly JsonSerializerOptions Opts = new()
        {
            WriteIndented       = true,
            PropertyNameCaseInsensitive = true
        };

        // ── Init ──────────────────────────────────────────────────────────────
        public static void EnsureDataDirectory()
        {
            if (!Directory.Exists(DataDir))
                Directory.CreateDirectory(DataDir);
        }

        // ── Users ─────────────────────────────────────────────────────────────
        public static List<User> LoadUsers()
        {
            if (!File.Exists(UsersFile)) return new List<User>();
            try
            {
                string json = File.ReadAllText(UsersFile);
                return JsonSerializer.Deserialize<List<User>>(json, Opts) ?? new List<User>();
            }
            catch { return new List<User>(); }
        }

        public static void SaveUsers(IEnumerable<User> users)
        {
            EnsureDataDirectory();
            File.WriteAllText(UsersFile, JsonSerializer.Serialize(users.ToList(), Opts));
        }

        // ── Vehicles ──────────────────────────────────────────────────────────
        public static List<Vehicle> LoadVehicles()
        {
            if (!File.Exists(VehiclesFile)) return new List<Vehicle>();
            try
            {
                string json = File.ReadAllText(VehiclesFile);
                return JsonSerializer.Deserialize<List<Vehicle>>(json, Opts) ?? new List<Vehicle>();
            }
            catch { return new List<Vehicle>(); }
        }

        public static void SaveVehicles(IEnumerable<Vehicle> vehicles)
        {
            EnsureDataDirectory();
            File.WriteAllText(VehiclesFile, JsonSerializer.Serialize(vehicles.ToList(), Opts));
        }

        // ── Bookings ──────────────────────────────────────────────────────────
        public static List<Booking> LoadBookings()
        {
            if (!File.Exists(BookingsFile)) return new List<Booking>();
            try
            {
                string json = File.ReadAllText(BookingsFile);
                return JsonSerializer.Deserialize<List<Booking>>(json, Opts) ?? new List<Booking>();
            }
            catch { return new List<Booking>(); }
        }

        public static void SaveBookings(IEnumerable<Booking> bookings)
        {
            EnsureDataDirectory();
            File.WriteAllText(BookingsFile, JsonSerializer.Serialize(bookings.ToList(), Opts));
        }
    }
}