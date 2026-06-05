using SmartTransport.Core;
using SmartTransport.Models;

namespace SmartTransport.Services
{
    public class BookingManager : IBookingService
    {
        private readonly Dictionary<int, Booking> _bookings = new();
        private readonly UserManager              _users;
        private readonly SmartAssignmentService   _assign;
        private readonly VehicleManager           _vehicles;

        public BookingManager(UserManager u, SmartAssignmentService a, VehicleManager v)
        { _users = u; _assign = a; _vehicles = v; }

        public void LoadBookings(List<Booking> bookings)
        {
            _bookings.Clear();
            foreach (var b in bookings) _bookings[b.BookingId] = b;
        }

        public Booking CreateBooking(int customerId, double weight, double distance, PriorityLevel priority)
        {
            var customer = _users.GetUserById(customerId);
            UserManager.RequireLoggedIn(customer);
            Validation.CheckPositive(weight,   "Weight");
            Validation.CheckPositive(distance, "Distance");
            _assign.ValidateLoadCapacity(weight);

            var b = new Booking(customerId, weight, distance, priority);
            _bookings[b.BookingId] = b;
            try { _assign.AutoAssignVehicle(b); } catch { /* stays Pending */ }
            return b;
        }

        public bool CancelBooking(int bookingId, int requestingUserId)
        {
            var booking = GetBookingById(bookingId);
            var user    = _users.GetUserById(requestingUserId);
            UserManager.RequireLoggedIn(user);

            if (booking.CustomerId != requestingUserId && user.Role != UserRole.Admin)
                throw new UnauthorizedAccessException("You cannot cancel this booking.");
            if (booking.Status == BookingStatus.Delivered || booking.Status == BookingStatus.Cancelled)
                throw new InvalidOperationException($"Cannot cancel a {booking.Status} booking.");

            if (booking.AssignedVehicleId.HasValue)
                _vehicles.GetVehicleById(booking.AssignedVehicleId.Value).UnloadCargo(booking.WeightKg);

            booking.UpdateStatus(BookingStatus.Cancelled);
            return true;
        }

        public bool UpdateBookingStatus(int bookingId, BookingStatus newStatus, int adminId)
        {
            UserManager.RequireAdmin(_users.GetUserById(adminId));
            var booking = GetBookingById(bookingId);

            if (booking.Status == BookingStatus.Cancelled || booking.Status == BookingStatus.Rejected)
                throw new InvalidOperationException($"Cannot update a {booking.Status} booking.");

            if (newStatus == BookingStatus.Delivered && booking.AssignedVehicleId.HasValue)
                _vehicles.GetVehicleById(booking.AssignedVehicleId.Value).UnloadCargo(booking.WeightKg);

            booking.UpdateStatus(newStatus);
            return true;
        }

        public Booking GetBookingById(int bookingId) =>
            _bookings.TryGetValue(bookingId, out var b) ? b
            : throw new KeyNotFoundException($"Booking #{bookingId} not found.");

        public List<Booking> ListAllBookings()                    => _bookings.Values.ToList();
        public List<Booking> ListBookingsByCustomer(int customerId) =>
            _bookings.Values.Where(b => b.CustomerId == customerId).ToList();
    }

    // ─────────────────────────────────────────────────────────────────────────
    public class PriorityScheduler
    {
        private readonly BookingManager _bm;
        private readonly Queue<Booking> _queue = new();

        public PriorityScheduler(BookingManager bm) => _bm = bm;
        public int QueueCount => _queue.Count;

        public List<Booking> SortBookingsByPriority() =>
            _bm.ListAllBookings()
               .Where(b => b.Status == BookingStatus.Assigned || b.Status == BookingStatus.Pending)
               .OrderByDescending(b => (int)b.Priority).ThenBy(b => b.CreatedAt).ToList();

        public int ScheduleDeliveries()
        {
            _queue.Clear();
            SortBookingsByPriority().ForEach(b => _queue.Enqueue(b));
            return _queue.Count;
        }

        public Booking? DequeueNextDelivery() => _queue.Count > 0 ? _queue.Dequeue() : null;
    }

    // ─────────────────────────────────────────────────────────────────────────
    public class TrackingService
    {
        private readonly BookingManager _bm;
        private readonly UserManager    _um;
        private static readonly BookingStatus[] Stages =
        {
            BookingStatus.Pending, BookingStatus.Approved,
            BookingStatus.Assigned, BookingStatus.InTransit, BookingStatus.Delivered
        };

        public TrackingService(BookingManager bm, UserManager um) { _bm = bm; _um = um; }

        public (BookingStatus Status, string Bar) TrackBookingStatus(int bookingId)
        {
            var status  = _bm.GetBookingById(bookingId).Status;
            int current = Array.IndexOf(Stages, status);
            string bar  = current < 0 ? $"[{status}]"
                : string.Join(" -> ", Stages.Select((s, i) => i <= current ? $"[*{s}]" : $"[ {s}]"));
            return (status, bar);
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    public class AdminControl
    {
        private readonly BookingManager _bm;
        private readonly UserManager    _um;

        public AdminControl(BookingManager bm, VehicleManager vm, UserManager um)
        { _bm = bm; _um = um; }

        public void ApproveBooking(int bookingId, int adminId)
        {
            UserManager.RequireAdmin(_um.GetUserById(adminId));
            var b = _bm.GetBookingById(bookingId);
            if (b.Status != BookingStatus.Pending)
                throw new InvalidOperationException($"Booking #{bookingId} is not Pending.");
            _bm.UpdateBookingStatus(bookingId, BookingStatus.Approved, adminId);
        }

        public void RejectBooking(int bookingId, int adminId)
        {
            UserManager.RequireAdmin(_um.GetUserById(adminId));
            var b = _bm.GetBookingById(bookingId);
            if (b.Status == BookingStatus.Delivered || b.Status == BookingStatus.Cancelled)
                throw new InvalidOperationException($"Cannot reject a {b.Status} booking.");
            _bm.UpdateBookingStatus(bookingId, BookingStatus.Rejected, adminId);
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    public class ReportingService
    {
        private readonly BookingManager _bm;
        private readonly VehicleManager _vm;

        public ReportingService(BookingManager bm, VehicleManager vm) { _bm = bm; _vm = vm; }

        public Report GenerateBookingReport() => new Report
        {
            TotalBookings     = _bm.ListAllBookings().Count,
            CompletedBookings = _bm.ListAllBookings().Count(b => b.Status == BookingStatus.Delivered),
            CancelledBookings = _bm.ListAllBookings().Count(b => b.Status == BookingStatus.Cancelled),
            TotalRevenue      = CalculateTotalRevenue(),
            VehicleUsage      = GetVehicleUsageStats()
        };

        public double CalculateTotalRevenue() =>
            Math.Round(_bm.ListAllBookings()
                .Where(b => b.Status != BookingStatus.Cancelled && b.Status != BookingStatus.Rejected)
                .Sum(b => b.Price), 2);

        public Dictionary<VehicleType, int> GetVehicleUsageStats()
        {
            var stats = new Dictionary<VehicleType, int>();
            foreach (var b in _bm.ListAllBookings().Where(b => b.AssignedVehicleId.HasValue))
            {
                try
                {
                    var type = _vm.GetVehicleById(b.AssignedVehicleId!.Value).Type;
                    stats[type] = stats.GetValueOrDefault(type) + 1;
                }
                catch { /* vehicle removed */ }
            }
            return stats;
        }
    }
}