using System.ComponentModel.DataAnnotations;

namespace Libreria_Elia_V0._0.Models
{
    public class Reservation
    {
        [Required]
        public int Id { get; set; }

        [Required]
        public DateTime RegDate { get; set; }

        [Required]
        public DateTime ExpDate { get; set; }

        [Required]
        public string? UserMail { get; set; }

        [Required]
        public string? BookTitle { get; set; }

        [Required]
        public int? BookState { get; set; }

        [Required]
        public int BookId { get; set; }
    }
}