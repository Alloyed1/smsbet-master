namespace Smsbet.Web.ViewModels
{
    public class SettingsViewModel
    {
        public string Email { get; set; }
        public string Phone { get; set; }
        public bool IsSmsPushing { get; set; }
        public bool IsEmailPushing { get; set; }
    }
}