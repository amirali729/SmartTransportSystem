using SmartTransport.Core;
using SmartTransport.Models;
using SmartTransport.Services;
using static SmartTransport.UI.ConsoleUI;

namespace SmartTransport.UI
{
    // Handles: Main menu, Register, Login, Logout, ForgotPassword, Account settings
    public class AuthMenus
    {
        private readonly UserManager _um;
        private readonly Action _saveUsers;
        public User? CurrentUser { get; private set; }
        public void SaveUsers() => _saveUsers();

        public AuthMenus(UserManager um, Action saveUsers) { _um = um; _saveUsers = saveUsers; }

        public void ShowMainMenu()
        {
            ShowMenu("SMART TRANSPORT SYSTEM", new[] { "Register", "Login", "Forgot Password" });
            switch (Ask("Choose"))
            {
                case "1": Register();       break;
                case "2": Login();          break;
                case "3": ForgotPassword(); break;
                case "0": ExitApp();        break;
                default:  Invalid();        break;
            }
        }

        public void Register()
        {
            Header("REGISTER");
            Console.WriteLine("  Set a Recovery Code — used to reset your password if forgotten.\n");
            string username = Ask("Username");
            string password = AskPassword("Password (min 4 chars)");
            string code     = Ask("Recovery Code (min 3 chars)");
            Try(() =>
            {
                var u = _um.RegisterUser(username, password, code);
                _saveUsers();
                Ok($"Account created! Welcome, {u.Username}. You can now login.");
            });
            Pause();
        }

        public void Login()
        {
            Header("LOGIN");
            string username = Ask("Username");
            string password = AskPassword("Password");
            Try(() =>
            {
                CurrentUser = _um.LoginUser(username, password);
                Ok(CurrentUser.Role == UserRole.SuperUser ? "Welcome." : $"Logged in as {CurrentUser.Username} [{CurrentUser.Role}]");
            });
            Pause();
        }

        public void Logout()
        {
            Try(() =>
            {
                string name = CurrentUser!.Username;
                _um.LogoutUser(CurrentUser!.UserId);
                CurrentUser = null;
                Ok($"Goodbye, {name}.");
            });
            Pause();
        }

        public void ForgotPassword()
        {
            Header("FORGOT PASSWORD");
            Console.WriteLine("  Enter your username and Recovery Code to reset your password.\n");
            string username = Ask("Username");
            var    user     = _um.FindUserByUsername(username);

            if (user == null || string.IsNullOrWhiteSpace(user.RecoveryCode))
            {
                Warn("No account found or no recovery code set. Contact administrator.");
                Pause(); return;
            }

            string code = Ask("Recovery Code");
            if (!user.ValidateRecoveryCode(code)) { Warn("Incorrect recovery code. Reset denied."); Pause(); return; }

            Ok("Code accepted. Set your new password.");
            Console.WriteLine();
            string newPass = AskPassword("New password (min 4 chars)");
            string confirm = AskPassword("Confirm new password");
            if (newPass != confirm) { Warn("Passwords do not match."); Pause(); return; }

            Try(() => { _um.ResetPasswordWithRecoveryCode(user.UserId, code, newPass); _saveUsers(); Ok("Password reset. You can now login."); });
            Pause();
        }

        // ── Account settings (shared by all roles) ────────────────────────────
        public void AccountUpdateUsername()
        {
            if (CurrentUser!.Role == UserRole.SuperUser) { Warn("Super User username cannot be changed."); Pause(); return; }
            Header("UPDATE USERNAME");
            Print("Current", CurrentUser!.Username);
            Console.WriteLine();
            string pass = AskPassword("Current password");
            string name = Ask("New username");
            Try(() => { _um.ChangeUsername(CurrentUser!.UserId, pass, name); _saveUsers(); Ok($"Username updated to '{CurrentUser.Username}'."); });
            Pause();
        }

        public void AccountChangePassword()
        {
            Header("CHANGE PASSWORD");
            string cur = AskPassword("Current password");
            string nw  = AskPassword("New password (min 4 chars)");
            string cf  = AskPassword("Confirm new password");
            if (nw != cf) { Warn("Passwords do not match."); Pause(); return; }
            Try(() => { _um.ChangePassword(CurrentUser!.UserId, cur, nw); _saveUsers(); Ok("Password changed."); });
            Pause();
        }

        public void AccountUpdateRecoveryCode()
        {
            Header("UPDATE RECOVERY CODE");
            string pass = AskPassword("Current password");
            string code = Ask("New Recovery Code (min 3 chars)");
            Try(() => { _um.UpdateRecoveryCode(CurrentUser!.UserId, pass, code); _saveUsers(); Ok("Recovery code updated."); });
            Pause();
        }
    }
}