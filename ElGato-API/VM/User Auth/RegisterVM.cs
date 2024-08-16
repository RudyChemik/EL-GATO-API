using System.ComponentModel.DataAnnotations;

namespace ElGato_API.VM
{
    public class RegisterVM
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]       
        public string Nick {  get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Passwords are not similar.")]
        public string ComfirmedPassword { get; set; }
    }
}
