using SmartTransport.Core;
using SmartTransport.Models;

namespace SmartTransport.Services
{
    public class PricingService : IPricingService
    {
        private static readonly Dictionary<VehicleType, double> BaseRate = new()
        {
            { VehicleType.Bike,       100 },
            { VehicleType.Van,        300 },
            { VehicleType.Truck,      700 },
            { VehicleType.HeavyTruck, 1500 }
        };

        private static readonly Dictionary<VehicleType, double> PerKmRate = new()
        {
            { VehicleType.Bike,       5 },
            { VehicleType.Van,        10 },
            { VehicleType.Truck,      18 },
            { VehicleType.HeavyTruck, 30 }
        };

        private static readonly Dictionary<VehicleType, double> PerKgRate = new()
        {
            { VehicleType.Bike,       2 },
            { VehicleType.Van,        1.5 },
            { VehicleType.Truck,      1 },
            { VehicleType.HeavyTruck, 0.7 }
        };

        public double CalculatePrice(double weightKg, double distanceKm, VehicleType vehicleType)
        {
            Validation.CheckPositive(weightKg, nameof(weightKg));
            Validation.CheckPositive(distanceKm, nameof(distanceKm));
            return Math.Round(
                BaseRate[vehicleType]
                + ApplyWeightCharges(weightKg, vehicleType)
                + ApplyDistanceCharges(distanceKm, vehicleType), 2);
        }

        public double ApplyWeightCharges(double weightKg, VehicleType vehicleType)
            => weightKg * PerKgRate[vehicleType];

        public double ApplyDistanceCharges(double distanceKm, VehicleType vehicleType)
            => distanceKm * PerKmRate[vehicleType];
    }
}