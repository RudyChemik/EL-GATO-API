using ElGato_API.VMO.Questionary;
using System.ComponentModel.DataAnnotations;

namespace ElGato_API.VM.User_Auth
{
    public class RegisterWithQuestVM
    {

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Required]
        public QuestionaryVM Questionary { get; set; }
    }
}
