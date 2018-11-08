using System.Collections.Generic;

namespace FreeSpaceAlert.Configuration
{
    public class EmailConfiguration
    {
        public string Host { get; set; }
        
        public int Port { get; set; }
        
        public string Email { get; set; }
        
        public string Password { get; set; }
        
        public bool EnableSsl { get; set; }
        
        public ICollection<string> Recipients { get; set; }
    }
}