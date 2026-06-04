using SmartTransport.Core;
using SmartTransport.Models;
using SmartTransport.Services;
using static SmartTransport.UI.ConsoleUI;

namespace SmartTransport.UI
{
    // Handles: Customer menu + Super User menu
    public class CustomerMenus
    {
        private readonly BookingManager          _bm;
        private readonly VehicleManager          _vm;
        private readonly LoadOptimizationService _opt;
        private readonly TrackingService         _track;
        private readonly UserManager             _um;
        private readonly AuthMenus               _auth;
        private readonly Action                  _saveBookings;
        private readonly Action                  _saveVehicles;

        public CustomerMenus(BookingManager bm, VehicleManager vm, LoadOptimizationService opt,
            TrackingService track, UserManager um, AuthMenus auth, Action saveB, Action saveV)
        { _bm = bm; _vm = vm; _opt = opt; _track = track; _um = um; _auth = auth; _saveBookings = saveB; _saveVehicles = saveV; }

        public void ShowCustomerMenu(User user)
        {
            ShowMenu($"CUSTOMER MENU  |  {user.Username}", new[]
            {
                "-- BOOKINGS --", "Create Booking", "View My Bookings", "View Booking Details",
                "Cancel Booking", "Track Booking",
                "-- VEHICLES --", "View Available Vehicles",
                "-- SHARING --",  "Shared Transport Suggestions",
                "-- MY ACCOUNT --", "Update Username", "Change Password", "Update Recovery Code", "Logout"
            });
            switch (Ask("Choose"))
            {
                case "1":  CreateBooking(user);             break;
                case "2":  ViewMyBookings(user);            break;
                case "3":  ViewBookingDetails(user, false); break;
                case "4":  CancelBooking(user, false);      break;
                case "5":  TrackBooking(user);              break;
                case "6":  ViewVehicles();                  break;
                case "7":  ShowSharedSuggestions();         break;
                case "8":  _auth.AccountUpdateUsername();   break;
                case "9":  _auth.AccountChangePassword();   break;
                case "10": _auth.AccountUpdateRecoveryCode(); break;
                case "11": _auth.Logout();                  break;
                case "0":  ExitApp();                       break;
                default:   Invalid();                       break;
            }
        }

        public void ShowSuperUserMenu(User user)
        {
            ShowMenu($"SYSTEM CONTROL  |  {user.Username}", new[]
            {
                "-- USER MANAGEMENT --", "View All Users", "Promote Customer to Admin",
                "Revoke Admin to Customer", "Delete User",
                "-- MY ACCOUNT --", "Change Password", "Update Recovery Code", "Logout"
            });
            switch (Ask("Choose"))
            {
                case "1": ViewAllUsers();                   break;
                case "2": ChangeRole(user, true);           break;
                case "3": ChangeRole(user, false);          break;
                case "4": DeleteUser(user);                 break;
                case "5": _auth.AccountChangePassword();    break;
                case "6": _auth.AccountUpdateRecoveryCode(); break;
                case "7": _auth.Logout();                   break;
                case "0": ExitApp();                        break;
                default:  Invalid();                        break;
            }
        }

        // ── Customer actions ──────────────────────────────────────────────────
        void CreateBooking(User user)
        {
            Header("CREATE BOOKING");
            double weight   = AskDouble("Cargo weight (kg)");
            double distance = AskDouble("Distance (km)");
            var priority    = AskPriority();
            Try(() =>
            {
                var b = _bm.CreateBooking(user.UserId, weight, distance, priority);
                _saveBookings(); _saveVehicles();
                Ok($"Booking #{b.BookingId} created!");
                Print("Weight", $"{b.WeightKg} kg"); Print("Distance", $"{b.DistanceKm} km");
                Print("Priority", b.Priority.ToString()); Print("Status", b.Status.ToString());
                if (b.AssignedVehicleId.HasValue)
                {
                    var v = _vm.GetVehicleById(b.AssignedVehicleId.Value);
                    Print("Vehicle", $"{v.Name} ({v.Type})"); Print("Price", $"{b.Price:C}");
                }
                else Warn("No vehicle available. Booking is Pending.");
            });
            Pause();
        }

        void ViewMyBookings(User user)
        {
            Header("MY BOOKINGS");
            var list = _bm.ListBookingsByCustomer(user.UserId);
            if (list.Count == 0) { Warn("No bookings yet."); Pause(); return; }
            PrintBookingList(list); Pause();
        }

        public void ViewBookingDetails(User user, bool isAdmin)
        {
            Header("BOOKING DETAILS");
            var list = isAdmin ? _bm.ListAllBookings() : _bm.ListBookingsByCustomer(user.UserId);
            if (list.Count == 0) { Warn("No bookings."); Pause(); return; }
            PrintBookingList(list);
            int id = AskInt("Booking ID (0 = back)"); if (id == 0) return;
            Try(() =>
            {
                var b = _bm.GetBookingById(id);
                if (!isAdmin && b.CustomerId != user.UserId) throw new UnauthorizedAccessException("Not your booking.");
                PrintBookingFull(b);
            });
            Pause();
        }

        public void CancelBooking(User user, bool isAdmin)
        {
            Header("CANCEL BOOKING");
            var list = (isAdmin ? _bm.ListAllBookings() : _bm.ListBookingsByCustomer(user.UserId))
                .Where(b => b.Status != BookingStatus.Cancelled && b.Status != BookingStatus.Delivered && b.Status != BookingStatus.Rejected).ToList();
            if (list.Count == 0) { Warn("No active bookings to cancel."); Pause(); return; }
            PrintBookingList(list);
            int id = AskInt("Booking ID to cancel (0 = back)"); if (id == 0) return;
            Try(() => { _bm.CancelBooking(id, user.UserId); _saveBookings(); _saveVehicles(); Ok($"Booking #{id} cancelled."); });
            Pause();
        }

        void TrackBooking(User user)
        {
            Header("TRACK BOOKING");
            var list = _bm.ListBookingsByCustomer(user.UserId);
            if (list.Count == 0) { Warn("No bookings."); Pause(); return; }
            PrintBookingList(list);
            int id = AskInt("Booking ID (0 = back)"); if (id == 0) return;
            Try(() =>
            {
                var b = _bm.GetBookingById(id);
                if (b.CustomerId != user.UserId) throw new UnauthorizedAccessException("Not your booking.");
                var (status, bar) = _track.TrackBookingStatus(id);
                Console.WriteLine($"\n  Booking #{id} — Status: {status}\n\n  {bar}");
            });
            Pause();
        }

        void ViewVehicles()
        {
            Header("AVAILABLE VEHICLES");
            var list = _vm.GetAvailableVehicles();
            if (list.Count == 0) { Warn("No vehicles available."); Pause(); return; }
            Console.WriteLine($"  {"ID",-5} {"Name",-22} {"Type",-13} {"Max (kg)",-12} Free (kg)");
            Divider();
            foreach (var v in list)
                Console.WriteLine($"  #{v.VehicleId,-4} {v.Name,-22} {v.Type,-13} {v.MaxCapacityKg,-12} {v.RemainingCapacityKg}");
            Pause();
        }

        public void ShowSharedSuggestions()
        {
            Header("SHARED TRANSPORT SUGGESTIONS");
            var pending = _bm.ListAllBookings().Where(b => b.Status == BookingStatus.Pending).ToList();
            if (pending.Count < 2) { Warn("Not enough pending bookings."); Pause(); return; }
            var (_, suggestions) = _opt.SuggestSharedTransport(pending);
            if (suggestions.Count == 0) { Warn("No sharing opportunities found."); Pause(); return; }
            foreach (var (group, indTotal, sharedPrice, savings, vehicle) in suggestions)
            {
                Console.WriteLine($"\n  Bookings : [{string.Join(", ", group.Select(b => "#" + b.BookingId))}]");
                Console.WriteLine($"  Weight   : {group.Sum(b => b.WeightKg)} kg  |  Vehicle: {vehicle?.Type}");
                Console.WriteLine($"  Individual: {indTotal:C}  |  Shared: {sharedPrice:C}  |  Savings: {savings:C}");
            }
            Pause();
        }

        // ── Super User actions ────────────────────────────────────────────────
        void ViewAllUsers()
        {
            Header("ALL USERS");
            var list = _um.GetAllVisibleUsers();
            if (list.Count == 0) { Warn("No users yet."); Pause(); return; }
            PrintUserList(list); Pause();
        }

        void ChangeRole(User su, bool promote)
        {
            Header(promote ? "PROMOTE TO ADMIN" : "REVOKE TO CUSTOMER");
            var list = _um.GetAllVisibleUsers()
                .Where(u => u.Role == (promote ? UserRole.Customer : UserRole.Admin)).ToList();
            if (list.Count == 0) { Warn(promote ? "No customers to promote." : "No admins to revoke."); Pause(); return; }
            PrintUserList(list);
            int id = AskInt("User ID (0 = back)"); if (id == 0) return;
            Try(() =>
            {
                if (promote) _um.AssignAdminRole(su.UserId, id);
                else         _um.RevokeAdminRole(su.UserId, id);
                _auth.SaveUsers();
                var u = _um.GetUserById(id);
                Ok($"'{u.Username}' is now {u.Role}.");
            });
            Pause();
        }

        void DeleteUser(User su)
        {
            Header("DELETE USER");
            var list = _um.GetAllVisibleUsers();
            if (list.Count == 0) { Warn("No users to delete."); Pause(); return; }
            PrintUserList(list);
            int id = AskInt("User ID (0 = back)"); if (id == 0) return;
            if (!Confirm($"Delete User #{id}? Cannot be undone.")) return;
            Try(() => { _um.DeleteUser(su.UserId, id); _auth.SaveUsers(); Ok($"User #{id} deleted."); });
            Pause();
        }
    }
}