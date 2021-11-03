#region Using

using Library.Core;
using Library.Model.Productions;
using Library.Service.Core;

#endregion Using

namespace Library.Service.Productions
{
    public interface IButtonRecipeConfigService : IService<ButtonRecipeConfig>
    {
		GridModel Query(GridParameter parameters, string plantId);
		void Delete(string id);
	}
}