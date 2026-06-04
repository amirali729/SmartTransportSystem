using SmartTransport.Core;
using SmartTransport.Models;

namespace SmartTransport.UI
{
    public static class ConsoleUI
    {
        public static void Header(string title)
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("╔══════════════════════════════════════════════════╗");
            Console.WriteLine($"║  {title.ToUpper(),-48}║");
            Console.WriteLine("╚══════════════════════════════════════════════════╝");
            Console.ResetColor();
            Console.WriteLine();
        }

        public static void Divider() => Console.WriteLine("  ──────────────────────────────────────────────────");
        public static void Print(string label, string value) => Console.WriteLine($"  {label,-16}: {value}");

        public static void Ok(string msg)   { Console.ForegroundColor = ConsoleColor.Green;  Console.WriteLine($"  + {msg}"); Console.ResetColor(); }
        public static void Warn(string msg) { Console.ForegroundColor = ConsoleColor.Yellow; Console.WriteLine($"  ! {msg}"); Console.ResetColor(); }
        public static void Invalid()        { Warn("Invalid option."); Pause(); }

        public static void Pause()
        {
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.Write("  Press any key to continue...");
            Console.ResetColor();
            Console.ReadKey(true);
        }

        public static void ExitApp()
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("\n  Goodbye!\n");
            Console.ResetColor();
            Environment.Exit(0);
        }

        public static string Ask(string prompt)
        {
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write($"  {prompt}: ");
            Console.ResetColor();
            return Console.ReadLine()?.Trim() ?? "";
        }

        public static string AskPassword(string prompt)
        {
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write($"  {prompt}: ");
            Console.ResetColor();
            string pw = "";
            ConsoleKeyInfo k;
            do
            {
                k = Console.ReadKey(intercept: true);
                if (k.Key == ConsoleKey.Backspace && pw.Length > 0) { pw = pw[..^1]; Console.Write("\b \b"); }
                else if (k.Key != ConsoleKey.Enter && k.Key != ConsoleKey.Backspace) { pw += k.KeyChar; Console.Write("*"); }
            } while (k.Key != ConsoleKey.Enter);
            Console.WriteLine();
            return pw;
        }

        public static int AskInt(string prompt)
        {
            while (true) { if (int.TryParse(Ask(prompt), out int v)) return v; Warn("Enter a valid whole number."); }
        }

        public static double AskDouble(string prompt)
        {
            while (true) { if (double.TryParse(Ask(prompt), out double v) && v > 0) return v; Warn("Enter a valid positive number."); }
        }

        public static bool Confirm(string message)
        {
            Warn(message);
            bool ok = Ask("Type YES to confirm").ToUpper() == "YES";
            if (!ok) Warn("Cancelled.");
            return ok;
        }

        public static void Try(Action action)
        {
            try { action(); }
            catch (Exception ex) { Console.ForegroundColor = ConsoleColor.Red; Console.WriteLine($"\n  Error: {ex.Message}"); Console.ResetColor(); }
        }

        public static void ShowMenu(string title, string[] options)
        {
            Header(title);
            int num = 1;
            foreach (var opt in options)
            {
                if (opt.StartsWith("--")) Console.WriteLine($"\n  {opt}");
                else Console.WriteLine($"  {num++,2}. {opt}");
            }
            Console.WriteLine($"\n   0. Exit");
            Divider();
        }

        public static void PrintBookingList(List<Booking> list)
        {
            Console.WriteLine($"  {"ID",-6} {"Weight",-9} {"Dist",-7} {"Priority",-10} {"Status",-13} Price");
            Divider();
            foreach (var b in list)
                Console.WriteLine($"  #{b.BookingId,-5} {b.WeightKg,-9} {b.DistanceKm,-7} {b.Priority,-10} {b.Status,-13} {b.Price:C}");
            Console.WriteLine();
        }

        public static void PrintBookingFull(Booking b)
        {
            Console.WriteLine(); Divider();
            Print("Booking ID",  $"#{b.BookingId}");
            Print("Customer ID", $"#{b.CustomerId}");
            Print("Weight",      $"{b.WeightKg} kg");
            Print("Distance",    $"{b.DistanceKm} km");
            Print("Priority",    b.Priority.ToString());
            Print("Status",      b.Status.ToString());
            Print("Vehicle",     b.AssignedVehicleId.HasValue ? $"#{b.AssignedVehicleId}" : "Not assigned");
            Print("Price",       $"{b.Price:C}");
            Print("Shared",      b.IsShared ? "Yes" : "No");
            Print("Created",     b.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss"));
            if (b.UpdatedAt.HasValue) Print("Updated", b.UpdatedAt.Value.ToString("yyyy-MM-dd HH:mm:ss"));
            Divider();
            Console.WriteLine("  Status History:");
            foreach (var e in b.StatusHistory) Console.WriteLine($"    {e}");
            Divider();
        }

        public static void PrintUserList(List<User> list)
        {
            Console.WriteLine($"  {"ID",-6} {"Username",-25} Role");
            Divider();
            foreach (var u in list) Console.WriteLine($"  #{u.UserId,-5} {u.Username,-25} {u.Role}");
            Console.WriteLine($"\n  Total: {list.Count}");
        }

        public static PriorityLevel AskPriority()
        {
            Console.WriteLine("\n  1.Low  2.Normal  3.High  4.Urgent");
            return Ask("Priority") switch { "1" => PriorityLevel.Low, "3" => PriorityLevel.High, "4" => PriorityLevel.Urgent, _ => PriorityLevel.Normal };
        }

        public static BookingStatus AskBookingStatus()
        {
            Console.WriteLine("\n  1.Pending  2.Approved  3.Assigned  4.InTransit  5.Delivered  6.Cancelled  7.Rejected");
            return Ask("Status") switch
            {
                "2" => BookingStatus.Approved, "3" => BookingStatus.Assigned, "4" => BookingStatus.InTransit,
                "5" => BookingStatus.Delivered, "6" => BookingStatus.Cancelled, "7" => BookingStatus.Rejected,
                _   => BookingStatus.Pending
            };
        }

        public static VehicleType AskVehicleType()
        {
            Console.WriteLine("\n  1.Bike(50kg)  2.Van(500kg)  3.Truck(5000kg)  4.HeavyTruck(20000kg)");
            return Ask("Type") switch { "1" => VehicleType.Bike, "3" => VehicleType.Truck, "4" => VehicleType.HeavyTruck, _ => VehicleType.Van };
        }
    }
}
