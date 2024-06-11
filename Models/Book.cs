using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace Libreria_Elia_V0._0.Models
{
    public class Book
    {
        [Required]
        public int Book_id { get; set; }
        [Required]
        public string? Isbn { get; set; }

        [Required]
        public string? Title { get; set; }

        public string? Subtitle { get; set; }

        [Required]
        public string? Author01 { get; set; }

        public string? Author02 { get; set; }

        public string? Author03 { get; set;}

        [Required]
        public string? Genre { get; set;}

        [Required]
        public string? PublishingHouse { get; set; }

        [Required]
        public string? ReleaseYear { get; set; }

        [Required]
        public string? Language { get; set; }

        public string? Volume { get; set; }

        [Required]
        public string? InventoryNumber { get; set; }

        [Required]
        public string? Placement { get; set; }

        [Required]
        public string? Operator { get; set; }

        [Required]
        public int Availability { get; set; }

        public string? Abstract { get; set; }

        public int NumberOfcopys { get; set; }
        
	}
}