using AgricultureApp.Application.Farms;
using AgricultureApp.Application.LLM;
using Microsoft.SemanticKernel;
using System.ComponentModel;

namespace AgricultureApp.Infrastructure.LLM
{
    public class FarmPlugin(IFarmRepository farmRepository)
    {
        [KernelFunction("get_field_information_by_farm_and_field_name")]
        [Description("Get a detailed field information based on field name and farm id that includes a list of cultivation information.")]
        public async Task<LlmField?> GetFieldInfo(string farmId, string fieldName)
        {
            LlmField? field = await farmRepository.GetFieldByNameAsync(fieldName, farmId);

            return field;
        }

        [KernelFunction("get_fields_information_by_farm")]
        [Description("Get a list of field details (cultivations, etc.) currently cultivated by the farm.")]
        public async Task<IEnumerable<LlmField>?> GetFields(string farmId)
        {
            IEnumerable<LlmField>? fields = await farmRepository.GetFieldsByFarmAsync(farmId);

            return fields;
        }
    }
}
