using System.ComponentModel.DataAnnotations;

namespace RecipeBox.Models
{
    public class TwoFactor
    {
        [Required]
        public string TwoFactorCode { get; set; }
    }
}
