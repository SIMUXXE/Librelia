using System.ComponentModel.DataAnnotations;

namespace Libreria_Elia_V0._0.Models
{
    public class User
    {
        [Required]
        public string Email { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public string Surname { get; set; }
        [Required]
        public int IsAdmin { get; set; }
        [Required]
        [DataType(DataType.Password)]
        [RegularExpression(@"^(?=.*[!@#$%^&*()-_+=|\\[\]{};:'""<>,.?\/]).{6,}$", ErrorMessage = "the password must have at least 6 chars and contain at least 1 special char")]
        public string Password { get; set; }

        public User() {}

        public User(string email, string name, string surname, int isAdmin, string password)
        {
            Email = email;
            Name = name;
            Surname = surname;
            IsAdmin = isAdmin;
            Password = password;
        }
    }
}
