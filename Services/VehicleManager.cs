using SmartTransport.Core;
using SmartTransport.Models;

namespace SmartTransport.Services
{
    public class VehicleManager
    {
        private readonly Dictionary<int, Vehicle> _vehicles = new();

        public void LoadVehicles(List<Vehicle> vehicles)
        {
            _vehicles.Clear();
            foreach (var v in vehicles) _vehicles[v.VehicleId] = v;
        }

        public Vehicle AddVehicle(string name, VehicleType type, User admin)
        {
            UserManager.RequireAdmin(admin);
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Vehicle name cannot be empty.");
            var v = new Vehicle(name.Trim(), type);
            _vehicles[v.VehicleId] = v;
            return v;
        }

        public void RemoveVehicle(int vehicleId, User admin)
        {
            UserManager.RequireAdmin(admin);
            var v = GetVehicleById(vehicleId);
            if (v.CurrentLoadKg > 0) throw new InvalidOperationException("Cannot remove a vehicle with active cargo.");
            _vehicles.Remove(vehicleId);
        }

        public void UpdateVehicleDetails(int vehicleId, User admin, string? name, double? capacity)
        {
            UserManager.RequireAdmin(admin);
            GetVehicleById(vehicleId).UpdateDetails(name, capacity);
        }

        public List<Vehicle> GetAvailableVehicles() => _vehicles.Values.Where(v => v.IsAvailable).ToList();
        public List<Vehicle> GetAllVehicles()        => _vehicles.Values.ToList();

        public Vehicle GetVehicleById(int vehicleId) =>
            _vehicles.TryGetValue(vehicleId, out var v) ? v
            : throw new KeyNotFoundException($"Vehicle #{vehicleId} not found.");
    }
}