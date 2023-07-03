using System.ComponentModel.DataAnnotations;

namespace Api_Usuario.Models
{
    public class Storage
    {
        [Key]
        public int IdStorage { get; set; }
        [Required]
        public string Titulo { get; set; }
     
        public string? Subtitulo { get; set; }
     
        public string? SubtituloOrginal { get; set; }
        [Required]
        public int Repeticiones { get; set; }
        [Required]
        public DateTime Fecha { get; set; }
        [Required]
        public string UrlArchivo { get; set; }
        [Required]
        public string UidUser { get; set; }
    }
}
