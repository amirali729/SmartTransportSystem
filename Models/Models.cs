using SmartTransport.Core;
using System.Text.Json.Serialization;

namespace SmartTransport.Models
{
    public class User
    {
        private static int _nextId = 1;

        public int      UserId       { get; set; }
        public string   Username     { get; set; } = "";
        public string   PasswordHash { get; set; } = "";
        public string   RecoveryCode { get; set; } = ""; // hashed
        public UserRole Role         { get; set; }

        [JsonIgnore]
        public bool IsLoggedIn { get; private set; }

        public User() { }

        public User(string username, string password, UserRole role, string recoveryCode = "")
        {
            UserId       = _nextId++;
            Username     = username;
            PasswordHash = HashValue(password);
            Role         = role;
            RecoveryCode = string.IsNullOrWhiteSpace(recoveryCode)
                               ? "" : HashValue(recoveryCode.ToLower().Trim());
        }

        public static void SetNextId(int id) { if (id > _nextId) _nextId = id; }

        public bool ValidatePassword(string password)    => PasswordHash == HashValue(password);
        public bool ValidateRecoveryCode(string code)    => RecoveryCode == HashValue(code.ToLower().Trim());

        public void ChangePassword(string newPassword)
        {
            if (string.IsNullOrWhiteSpace(newPassword) || newPassword.Length < 4)
                throw new ArgumentException("New password must be at least 4 characters.");
            PasswordHash = HashValue(newPassword);
        }

        public void ChangeUsername(string newUsername)
        {
            if (string.IsNullOrWhiteSpace(newUsername))
                throw new ArgumentException("Username cannot be empty.");
            Username = newUsername;
        }

        public void SetRecoveryCode(string newCode)
        {
            if (string.IsNullOrWhiteSpace(newCode) || newCode.Length < 3)
                throw new ArgumentException("Recovery code must be at least 3 characters.");
            RecoveryCode = HashValue(newCode.ToLower().Trim());
        }

        public void Login()  => IsLoggedIn = true;
        public void Logout() => IsLoggedIn = false;
        public void AssignRole(UserRole newRole) => Role = newRole;

        public static string HashValue(string value)
        {
            int hash = 17;
            foreach (char c in value) hash = hash * 31 + c;
            return hash.ToString("X8");
        }
    }

    // ── Vehicle ───────────────────────────────────────────────────────────────
    public class Vehicle
    {
        private static int _nextId = 1;

        public int         VehicleId     { get; set; }
        public string      Name          { get; set; } = "";
        public VehicleType Type          { get; set; }
        public double      MaxCapacityKg { get; set; }
        public double      CurrentLoadKg { get; set; }
        public bool        IsAvailable   { get; set; }

        [JsonIgnore]
        public double RemainingCapacityKg => MaxCapacityKg - CurrentLoadKg;

        public Vehicle() { }

        public Vehicle(string name, VehicleType type)
        {
            VehicleId     = _nextId++;
            Name          = name;
            Type          = type;
            MaxCapacityKg = GetDefaultCapacity(type);
            CurrentLoadKg = 0;
            IsAvailable   = true;
        }

        public static void SetNextId(int id) { if (id > _nextId) _nextId = id; }

        private static double GetDefaultCapacity(VehicleType type) => type switch
        {
            VehicleType.Bike       => 50,
            VehicleType.Van        => 500,
            VehicleType.Truck      => 5000,
            VehicleType.HeavyTruck => 20000,
            _ => throw new ArgumentException("Unknown vehicle type")
        };

        public void UpdateDetails(string? newName, double? newCapacity)
        {
            if (!string.IsNullOrWhiteSpace(newName)) Name = newName;
            if (newCapacity.HasValue && newCapacity.Value > 0)
            {
                if (newCapacity.Value < CurrentLoadKg)
                    throw new InvalidOperationException("New capacity cannot be less than current load.");
                MaxCapacityKg = newCapacity.Value;
            }
        }

        public bool LoadCargo(double kg)
        {
            if (kg <= 0 || CurrentLoadKg + kg > MaxCapacityKg) return false;
            CurrentLoadKg += kg;
            if (CurrentLoadKg >= MaxCapacityKg) IsAvailable = false;
            return true;
        }

        public void UnloadCargo(double kg)
        {
            CurrentLoadKg = Math.Max(0, CurrentLoadKg - kg);
            IsAvailable   = true;
        }
    }

    // ── Booking ───────────────────────────────────────────────────────────────
    public class Booking
    {
        private static int _nextId = 1;

        public int           BookingId         { get; set; }
        public int           CustomerId        { get; set; }
        public double        WeightKg          { get; set; }
        public double        DistanceKm        { get; set; }
        public PriorityLevel Priority          { get; set; }
        public BookingStatus Status            { get; set; }
        public int?          AssignedVehicleId { get; set; }
        public double        Price             { get; set; }
        public DateTime      CreatedAt         { get; set; }
        public DateTime?     UpdatedAt         { get; set; }
        public bool          IsShared          { get; set; }
        public List<string>  StatusHistory     { get; set; } = new();

        public Booking() { }

        public Booking(int customerId, double weightKg, double distanceKm, PriorityLevel priority)
        {
            BookingId  = _nextId++;
            CustomerId = customerId;
            WeightKg   = weightKg;
            DistanceKm = distanceKm;
            Priority   = priority;
            Status     = BookingStatus.Pending;
            CreatedAt  = DateTime.Now;
            StatusHistory.Add($"{CreatedAt:HH:mm:ss} — Created with status Pending");
        }

        public static void SetNextId(int id) { if (id > _nextId) _nextId = id; }

        public void AssignVehicle(int vehicleId, double price)
        {
            AssignedVehicleId = vehicleId;
            Price             = price;
            UpdateStatus(BookingStatus.Assigned);
        }

        public void UpdateStatus(BookingStatus newStatus)
        {
            Status    = newStatus;
            UpdatedAt = DateTime.Now;
            StatusHistory.Add($"{UpdatedAt:HH:mm:ss} — Status changed to {newStatus}");
        }

        public void MarkShared()                     => IsShared = true;
        public void SetPriority(PriorityLevel level) => Priority  = level;
    }

    // ── Report ────────────────────────────────────────────────────────────────
    public class Report
    {
        public int                          TotalBookings     { get; set; }
        public int                          CompletedBookings { get; set; }
        public int                          CancelledBookings { get; set; }
        public double                       TotalRevenue      { get; set; }
        public Dictionary<VehicleType, int> VehicleUsage      { get; set; } = new();
        public DateTime                     GeneratedAt       { get; set; } = DateTime.Now;
    }
}