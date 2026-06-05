using SmartTransport.Core;
using SmartTransport.Models;

namespace SmartTransport.Services
{
    public class UserManager
    {
        private readonly Dictionary<int, User>    _users         = new();
        private readonly Dictionary<string, int>  _usernameIndex = new();

        private const string SuName = "amir ali";
        private const string SuPass = "rayquaza10";
        private const string SuCode = "mastercode";

        public void LoadUsers(List<User> users)
        {
            _users.Clear(); _usernameIndex.Clear();
            foreach (var u in users) Index(u);
        }

        public void EnsureSuperUser()
        {
            if (_usernameIndex.ContainsKey(SuName.ToLower())) return;
            var su = new User(SuName, SuPass, UserRole.SuperUser, SuCode);
            Index(su);
        }

        // ── Register ──────────────────────────────────────────────────────────
        public User RegisterUser(string username, string password, string recoveryCode)
        {
            username = username.Trim();
            Guard(!string.IsNullOrWhiteSpace(username),               "Username cannot be empty.");
            Guard(password.Length >= 4,                                "Password must be at least 4 characters.");
            Guard(recoveryCode.Trim().Length >= 3,                     "Recovery code must be at least 3 characters.");
            Guard(!_usernameIndex.ContainsKey(username.ToLower()),     $"Username '{username}' is already taken.");
            Guard(username.ToLower() != SuName.ToLower(),              "That username is not available.");

            var user = new User(username, password, UserRole.Customer, recoveryCode);
            Index(user);
            return user;
        }

        // ── Login ─────────────────────────────────────────────────────────────
        public User LoginUser(string username, string password)
        {
            Guard(!string.IsNullOrWhiteSpace(username) && !string.IsNullOrWhiteSpace(password),
                  "Username and password are required.");

            var user = FindUserByUsername(username)
                ?? throw new InvalidOperationException("Invalid username or password.");
            Guard(user.ValidatePassword(password), "Invalid username or password.");
            Guard(!user.IsLoggedIn,                "This account is already logged in.");

            user.Login();
            return user;
        }

        public void LogoutUser(int userId)
        {
            var user = GetUserById(userId);
            Guard(user.IsLoggedIn, "User is not logged in.");
            user.Logout();
        }

        // ── Username / Password / Recovery ────────────────────────────────────
        public void ChangeUsername(int userId, string currentPassword, string newUsername)
        {
            newUsername = newUsername.Trim();
            var user = Authenticate(userId, currentPassword);
            Guard(!string.IsNullOrWhiteSpace(newUsername),              "New username cannot be empty.");
            Guard(!_usernameIndex.ContainsKey(newUsername.ToLower()) ||
                   _usernameIndex[newUsername.ToLower()] == userId,     $"Username '{newUsername}' is already taken.");
            Guard(newUsername.ToLower() != SuName.ToLower() ||
                   user.Role == UserRole.SuperUser,                     "That username is not available.");

            _usernameIndex.Remove(user.Username.ToLower());
            user.ChangeUsername(newUsername);
            _usernameIndex[newUsername.ToLower()] = userId;
        }

        public void ChangePassword(int userId, string currentPassword, string newPassword)
        {
            var user = Authenticate(userId, currentPassword);
            Guard(currentPassword != newPassword, "New password must be different.");
            user.ChangePassword(newPassword);
        }

        public void UpdateRecoveryCode(int userId, string currentPassword, string newCode)
        {
            Authenticate(userId, currentPassword).SetRecoveryCode(newCode);
        }

        public void ResetPasswordWithRecoveryCode(int userId, string code, string newPassword)
        {
            var user = GetUserById(userId);
            Guard(!string.IsNullOrWhiteSpace(user.RecoveryCode), "No recovery code set. Contact administrator.");
            Guard(user.ValidateRecoveryCode(code),               "Recovery code is incorrect.");
            user.ChangePassword(newPassword);
        }

        // ── Role management ───────────────────────────────────────────────────
        public void AssignAdminRole(int suId, int targetId)
        {
            RequireSuperUser(GetUserById(suId));
            var t = GetUserById(targetId);
            Guard(t.Role != UserRole.SuperUser, "Cannot change the Super User role.");
            Guard(t.Role != UserRole.Admin,     $"'{t.Username}' is already an Admin.");
            t.AssignRole(UserRole.Admin);
        }

        public void RevokeAdminRole(int suId, int targetId)
        {
            RequireSuperUser(GetUserById(suId));
            var t = GetUserById(targetId);
            Guard(t.Role != UserRole.SuperUser,  "Cannot change the Super User role.");
            Guard(t.Role != UserRole.Customer,   $"'{t.Username}' is already a Customer.");
            t.AssignRole(UserRole.Customer);
        }

        public void DeleteUser(int suId, int targetId)
        {
            RequireSuperUser(GetUserById(suId));
            Guard(targetId != suId, "You cannot delete the Super User account.");
            var t = GetUserById(targetId);
            Guard(t.Role != UserRole.SuperUser, "The Super User account cannot be deleted.");
            _usernameIndex.Remove(t.Username.ToLower());
            _users.Remove(targetId);
        }

        // ── Getters ───────────────────────────────────────────────────────────
        public User? FindUserByUsername(string username) =>
            _usernameIndex.TryGetValue(username.ToLower().Trim(), out int uid) ? _users[uid] : null;

        public User GetUserById(int userId) =>
            _users.TryGetValue(userId, out var u) ? u
            : throw new KeyNotFoundException($"User #{userId} not found.");

        public List<User> GetAllVisibleUsers() =>
            _users.Values.Where(u => u.Role != UserRole.SuperUser).ToList();

        public List<User> GetAllUsers() => _users.Values.ToList();

        // ── Guards & helpers ──────────────────────────────────────────────────
        private void Index(User u) { _users[u.UserId] = u; _usernameIndex[u.Username.ToLower()] = u.UserId; }

        private User Authenticate(int userId, string password)
        {
            var user = GetUserById(userId);
            Guard(user.ValidatePassword(password), "Current password is incorrect.");
            return user;
        }

        private static void Guard(bool condition, string message)
        {
            if (!condition) throw new InvalidOperationException(message);
        }

        public static void RequireAdmin(User u)
        {
            if (u.Role != UserRole.Admin && u.Role != UserRole.SuperUser)
                throw new UnauthorizedAccessException("Admin privileges required.");
        }

        public static void RequireSuperUser(User u)
        {
            if (u.Role != UserRole.SuperUser)
                throw new UnauthorizedAccessException("Only the Super User can do this.");
        }

        public static void RequireLoggedIn(User u)
        {
            if (!u.IsLoggedIn)
                throw new UnauthorizedAccessException("You must be logged in.");
        }
    }
}