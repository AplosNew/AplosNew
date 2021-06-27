using Library.Core;
using Library.Model.Productions;
using Library.Service.Core;

namespace Library.Service.Productions
{
    public interface IRecipeOperationService : IService<RecipeOperation>
    {
        GridModel Query(GridParameter parameters);
        decimal GetAutoSequence();
    }
}