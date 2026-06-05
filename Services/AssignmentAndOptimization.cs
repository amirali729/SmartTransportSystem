using SmartTransport.Core;
using SmartTransport.Models;

namespace SmartTransport.Services
{
    public class SmartAssignmentService
    {
        private readonly VehicleManager _vehicleManager;
        private readonly PricingService _pricing;

        public SmartAssignmentService(VehicleManager vehicleManager, PricingService pricing)
        {
            _vehicleManager = vehicleManager;
            _pricing = pricing;
        }

        public Vehicle AutoAssignVehicle(Booking booking)
        {
            ValidateLoadCapacity(booking.WeightKg);
            var optimal = SelectOptimalVehicle(booking.WeightKg);

            if (!optimal.LoadCargo(booking.WeightKg))
                throw new InvalidOperationException($"Failed to load cargo onto vehicle #{optimal.VehicleId}.");

            double price = _pricing.CalculatePrice(booking.WeightKg, booking.DistanceKm, optimal.Type);
            booking.AssignVehicle(optimal.VehicleId, price);
            return optimal;
        }

        public void ValidateLoadCapacity(double weightKg)
        {
            Validation.CheckPositive(weightKg, nameof(weightKg));
            if (weightKg > 20000)
                throw new InvalidOperationException($"Load {weightKg} kg exceeds maximum system capacity of 20,000 kg.");
        }

        public Vehicle SelectOptimalVehicle(double weightKg)
        {
            var available = _vehicleManager.GetAvailableVehicles()
                .Where(v => v.RemainingCapacityKg >= weightKg)
                .OrderBy(v => v.MaxCapacityKg)
                .ThenBy(v => v.CurrentLoadKg)
                .ToList();

            if (available.Count == 0)
                throw new InvalidOperationException($"No available vehicle can handle {weightKg} kg.");

            return available[0];
        }
    }

    public class LoadOptimizationService
    {
        private readonly VehicleManager _vehicleManager;
        private readonly PricingService _pricing;
        private const double PartialLoadThreshold = 0.6;

        public LoadOptimizationService(VehicleManager vehicleManager, PricingService pricing)
        {
            _vehicleManager = vehicleManager;
            _pricing = pricing;
        }

        public List<Vehicle> DetectPartialLoads()
        {
            return _vehicleManager.GetAllVehicles()
                .Where(v => v.CurrentLoadKg > 0 &&
                            v.CurrentLoadKg / v.MaxCapacityKg < PartialLoadThreshold)
                .ToList();
        }

        public List<List<Booking>> MergeBookings(List<Booking> pendingBookings)
        {
            if (pendingBookings == null || pendingBookings.Count == 0)
                return new List<List<Booking>>();

            var groups    = new List<List<Booking>>();
            var available = _vehicleManager.GetAvailableVehicles()
                .OrderBy(v => v.MaxCapacityKg).ToList();
            var unassigned = new List<Booking>(pendingBookings);

            foreach (var vehicle in available)
            {
                if (unassigned.Count == 0) break;
                var group     = new List<Booking>();
                double remaining = vehicle.RemainingCapacityKg;

                foreach (var booking in unassigned.ToList())
                {
                    if (booking.WeightKg <= remaining)
                    {
                        group.Add(booking);
                        remaining -= booking.WeightKg;
                    }
                }

                if (group.Count > 1)
                {
                    groups.Add(group);
                    foreach (var b in group) unassigned.Remove(b);
                }
            }
            return groups;
        }

        public (List<List<Booking>> Groups, List<(List<Booking> Group, double IndividualTotal, double SharedPrice, double Savings, Vehicle? Vehicle)> Suggestions)
            SuggestSharedTransport(List<Booking> pendingBookings)
        {
            var groups      = MergeBookings(pendingBookings);
            var suggestions = new List<(List<Booking>, double, double, double, Vehicle?)>();

            foreach (var group in groups)
            {
                double totalWeight = group.Sum(b => b.WeightKg);
                double avgDist     = group.Average(b => b.DistanceKm);
                var vehicle = _vehicleManager.GetAvailableVehicles()
                    .FirstOrDefault(v => v.RemainingCapacityKg >= totalWeight);

                if (vehicle == null) continue;

                double sharedPrice     = _pricing.CalculatePrice(totalWeight, avgDist, vehicle.Type);
                double individualTotal = group.Sum(b =>
                    _pricing.CalculatePrice(b.WeightKg, b.DistanceKm, vehicle.Type));
                double savings = individualTotal - sharedPrice;
                suggestions.Add((group, individualTotal, sharedPrice, savings, vehicle));
            }
            return (groups, suggestions);
        }

        public void ApplyMerge(List<Booking> group)
        {
            foreach (var b in group) b.MarkShared();
        }
    }
}