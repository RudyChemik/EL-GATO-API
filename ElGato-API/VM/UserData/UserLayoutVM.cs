using ElGato_API.Models.User;
using System.ComponentModel.DataAnnotations;

namespace ElGato_API.VM.UserData
{
    public class UserLayoutVM
    {
        public bool Animations { get; set; } = true;
        public List<ChartStack> ChartStack { get; set; } = new List<ChartStack>();
    }
}
