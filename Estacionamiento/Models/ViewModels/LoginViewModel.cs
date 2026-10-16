using System.ComponentModel.DataAnnotations;

namespace Estacionamiento.Models.ViewModels
{
    public class LoginViewModel
    {
        [Required]
        [Display(Name = "Usuario")]
        public string Usuario { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Contraseña")]
        public string Password { get; set; }

        public string ReturnUrl { get; set; }
    }
}
