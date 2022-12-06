using DigitalDashboard.DAL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DigitalDashboard.BLL.Interfaces
{
    public interface IRegulatorySKURepository
    {
        Task<BMSRegulatorySKUFilterData> GetFilteredRegulatorySKUAsync();

        Task<RegulatorySKUDataWithFilterList> GetRegulatorySKUWithFilterListAsync(BMSRegulatorySKUInput sKUInput, 
                                                                                  int dataRange, 
                                                                                  int skipRecordCount);

        Task<BMSRegulatorySKUWithTotalCount> GetSKUWithTotalCountAsync(BMSRegulatorySKUInput sKUInput,
                                                                   int dataRange,
                                                                   int skipRecordCount);

        Task<ColumnSKUList> GetSKUSearchListAsync(string searchText,
                                               int dataRange,
                                               int skipRecordCount);

        Task<ColumnSKUList> GetSKUSearchListByCascadingFilterAsync(BMSRegulatorySKUInput sKUInput,
                                                                int dataRange,
                                                                int skipRecordCount);
    }
}
