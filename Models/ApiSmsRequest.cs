namespace ZenerpSMS.Models
{
    public class ApiSmsRequest
    {
        public string Timestamp { get; set; }
        public string PhoneNumber { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public string Message { get; set; }
        public string Language { get; set; }
    }
}
