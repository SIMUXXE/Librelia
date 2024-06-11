using System.ComponentModel.DataAnnotations;

namespace Libreria_Elia_V0._0.Models
{
    public class LoggedUser
    {
        [Required]
        public string Email { get; set; }
        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        public LoggedUser() { }

        public LoggedUser(string email, string password)
        {
            Email = email;
            Password = password;
        }
    }
}
