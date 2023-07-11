using System.ComponentModel.DataAnnotations;

namespace Api_Usuario.Models
{
    public class UpdateStorage
    {
        [Required]
        public int Id { get; set; }
        [Required]
        public int Repeticiones { get; set; }
        [Required]
        public string Intervalo { get; set; }
    }
}
