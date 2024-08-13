using Microsoft.AspNetCore.Authorization;

namespace QLSVVV.Models
{
 
    public class User
    {
        public int Id { get; set; }
        public String? UserName { get; set; }
        public String? Password { get; set; }
        public String? Role { get; set; }
        public User()
        {
        }
    }
}
