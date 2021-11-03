#region Using

using Library.Model.Processes;

using Library.Service.Core;
using System.Collections.Generic;

#endregion Using

namespace Library.Service.Processes
{
    public interface IProcessConfigService : IService<ProcessConfig>
    {
        IEnumerable<object> GetProcessConfigBomOrRecipeCbo();

        IEnumerable<object> GetProcessConfigLevelCbo();

        IEnumerable<object> GetProcessConfigMaterialTaggingTypeCbo();

        IEnumerable<object> Query(string materialMasterId);

        IEnumerable<object> GetCharacteristicsName(string materialMasterId);

        void Insert(IEnumerable<ProcessConfig> processConfig);

        void Archive(IEnumerable<ProcessConfig> processConfig);
    }
}