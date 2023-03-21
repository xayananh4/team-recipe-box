using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace RecipeBox.Models
{
  public class Recipe
  {
    public int RecipeId { get; set; }
    [Required(ErrorMessage = "The Recipe's description can't be empty!")]
    public string Description { get; set; }
    public string Instruction { get; set; }
    public string Rating { get; set; }
    public List<RecipeTag> JoinEntities { get;}

    public List<RecipeIngredient> JoinEntitiesIngredients { get;}
    public ApplicationUser User { get; set; }

  }
}