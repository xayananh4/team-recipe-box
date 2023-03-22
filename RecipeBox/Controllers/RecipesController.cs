using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using RecipeBox.Models;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;
using System.Security.Claims;

namespace RecipeBox.Controllers
{
    [Authorize]
    public class RecipesController : Controller
    {
        private readonly RecipeBoxContext _db;
        private readonly UserManager<ApplicationUser> _userManager;

        public RecipesController(UserManager<ApplicationUser> userManager, RecipeBoxContext db)
        {
            _userManager = userManager;
            _db = db;
        }

        public async Task<IActionResult> Index(string sortOrder)
        {
            string userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            ApplicationUser currentUser = await _userManager.FindByIdAsync(userId);
            List<Recipe> userRecipes = _db.Recipes
                                .Where(entry => entry.User.Id == currentUser.Id)
                                .Include(recipe => recipe.JoinEntitiesIngredients)
                                .Include(recipe => recipe.JoinEntities)
                                .ToList();
                // return View(userRecipes);

            var recipes = from r in userRecipes
                          select r;
            
            switch (sortOrder)
            {
                case "description_desc":
                    recipes = userRecipes.OrderBy(r => r.Description);
                    break;
                case "rating_desc":
                    recipes = userRecipes.OrderByDescending(r => r.Rating);
                    break;
                default:
                    recipes = userRecipes.OrderBy(r => r.RecipeId);
                    break;
            }
            // return View(await recipes.AsNoTracking().ToListAsync());
                    return View(recipes);
        }

        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> Create(Recipe recipe)
        {
            if (!ModelState.IsValid)
            {
                return View(recipe);
            }
            else
            {
                string userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                ApplicationUser currentUser = await _userManager.FindByIdAsync(userId);
                recipe.User = currentUser;
                _db.Recipes.Add(recipe);
                _db.SaveChanges();
                return RedirectToAction("Index");
            }
        }

        public ActionResult Details(int id)
        {
            Recipe thisRecipe = _db.Recipes
                .Include(recipe => recipe.JoinEntities)
                .ThenInclude(join => join.Tag)
                .Include(recipe => recipe.JoinEntitiesIngredients)
                .ThenInclude(join => join.Ingredient)
                .FirstOrDefault(recipe => recipe.RecipeId == id);
            return View(thisRecipe);
        }

        public ActionResult Edit(int id)
        {
            Recipe thisRecipe = _db.Recipes.FirstOrDefault(recipe => recipe.RecipeId == id);
            //ViewBag.CategoryId = new SelectList(_db.Categories, "CategoryId", "Name");
            return View(thisRecipe);
        }


        [HttpPost]
        public ActionResult Edit(Recipe recipe)
        {
            _db.Recipes.Update(recipe);
            _db.SaveChanges();
            return RedirectToAction("Index");
        }
        public ActionResult Rate(int id)
        {
            Recipe thisRecipe = _db.Recipes.FirstOrDefault(recipe => recipe.RecipeId == id);
            //ViewBag.CategoryId = new SelectList(_db.Categories, "CategoryId", "Name");
            return View(thisRecipe);
        }

        [HttpPost]
        public ActionResult Rate(Recipe recipe)
        {
            _db.Recipes.Update(recipe);
            _db.SaveChanges();
            return RedirectToAction("Index");
        }



        public ActionResult Delete(int id)
        {
            Recipe thisRecipe = _db.Recipes.FirstOrDefault(recipe => recipe.RecipeId == id);
            return View(thisRecipe);
        }

        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int id)
        {
            Recipe thisRecipe = _db.Recipes.FirstOrDefault(recipe => recipe.RecipeId == id);
            _db.Recipes.Remove(thisRecipe);
            _db.SaveChanges();
            return RedirectToAction("Index");
        }

        public ActionResult AddTag(int id)
        {
            Recipe thisRecipe = _db.Recipes.FirstOrDefault(recipes => recipes.RecipeId == id);
            ViewBag.TagId = new SelectList(_db.Tags, "TagId", "Title");
            return View(thisRecipe);
        }

        [HttpPost]
        public ActionResult AddTag(Recipe recipe, int tagId)
        {
#nullable enable
            RecipeTag? joinEntity = _db.RecipeTags.FirstOrDefault(join => (join.TagId == tagId && join.RecipeId == recipe.RecipeId));
#nullable disable
            if (joinEntity == null && tagId != 0)
            {
                _db.RecipeTags.Add(new RecipeTag() { TagId = tagId, RecipeId = recipe.RecipeId });
                _db.SaveChanges();
            }
            return RedirectToAction("Details", new { id = recipe.RecipeId });
        }
        public ActionResult AddIngredient(int id)
        {
            Recipe thisRecipe = _db.Recipes.FirstOrDefault(recipes => recipes.RecipeId == id);
            ViewBag.IngredientId = new SelectList(_db.Ingredients, "IngredientId", "Name");
            return View(thisRecipe);
        }
        [HttpPost]
        public ActionResult AddIngredient(Recipe recipe, int ingredientId)
        {
#nullable enable
            RecipeIngredient? joinEntityIng = _db.RecipeIngredients.FirstOrDefault(join => (join.IngredientId == ingredientId && join.RecipeId == recipe.RecipeId));
#nullable disable
            if (joinEntityIng == null && ingredientId != 0)
            {
                _db.RecipeIngredients.Add(new RecipeIngredient() { IngredientId = ingredientId, RecipeId = recipe.RecipeId });
                _db.SaveChanges();
            }
            return RedirectToAction("Details", new { id = recipe.RecipeId });
        }

        [HttpPost]
        public ActionResult DeleteJoin(int joinId)
        {
            RecipeTag joinEntry = _db.RecipeTags.FirstOrDefault(entry => entry.RecipeTagId == joinId);
            _db.RecipeTags.Remove(joinEntry);
            _db.SaveChanges();
            return RedirectToAction("Index");
        }
    }
}
