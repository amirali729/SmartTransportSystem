namespace SmartTransport.Core
{
    public enum BookingStatus
    {
        Pending,
        Approved,
        Assigned,
        InTransit,
        Delivered,
        Cancelled,
        Rejected
    }

    public enum PriorityLevel
    {
        Low = 1,
        Normal = 2,
        High = 3,
        Urgent = 4
    }

    public enum VehicleType
    {
        Bike,
        Van,
        Truck,
        HeavyTruck
    }

    public enum UserRole
    {
        Customer,
        Admin,
        SuperUser
    }
}