using SmartTransport.Models;

namespace SmartTransport.Core
{
    public interface IBookingService
    {
        Booking CreateBooking(int customerId, double weightKg, double distanceKm, PriorityLevel priority);
        bool CancelBooking(int bookingId, int requestingUserId);
        bool UpdateBookingStatus(int bookingId, BookingStatus newStatus, int adminId);
        Booking GetBookingById(int bookingId);
        List<Booking> ListAllBookings();
        List<Booking> ListBookingsByCustomer(int customerId);
    }

    public interface IPricingService
    {
        double CalculatePrice(double weightKg, double distanceKm, VehicleType vehicleType);
        double ApplyWeightCharges(double weightKg, VehicleType vehicleType);
        double ApplyDistanceCharges(double distanceKm, VehicleType vehicleType);
    }
}