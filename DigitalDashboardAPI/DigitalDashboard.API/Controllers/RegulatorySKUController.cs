using DigitalDashboard.BLL;
using DigitalDashboard.BLL.Interfaces;
using DigitalDashboard.DAL.Models;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ActionConstraints;

namespace DigitalDashboard.API.Controllers
{
    [ApiController]
    public class RegulatorySKUController : ControllerBase
    {
        private readonly IRegulatorySKURepository sKURepository;

        // Constructor:
        //    IRegulatorySKURepository instance is retrieved from DI via constructor injection
        public RegulatorySKUController(IRegulatorySKURepository sKURepository)
        {
            this.sKURepository = sKURepository;
        }

        // GET: This API is used to get all limited regulatory SKU.
        [HttpGet, Route("digitaldashboard/RegulatorySKU/GetAllRegulatorySKU")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(BMSRegulatorySKUFilterData))]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> GetFilteredRegulatorySKU()
        {
            var result = await sKURepository.GetFilteredRegulatorySKUAsync();

            return result == null ? NoContent() : Ok(result);
        }

        // POST: This API is used to get regulatory SKU with FilterList
        //       based on certain criteria such as date range and skip record count.
        [HttpPost, Route("digitaldashboard/RegulatorySKU/GetRegulatorySKUFilterList/{dataRange:int}/{skipRecordCount:int}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(RegulatorySKUDataWithFilterList))]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<RegulatorySKUDataWithFilterList>> GetRegulatorySKUWithFilterList([FromBody] BMSRegulatorySKUInput sKUInput,
                                                                                                        int dataRange,
                                                                                                        int skipRecordCount)
        {
            if (dataRange <= 0 || skipRecordCount < 0)
            {
                return BadRequest();
            }

            var result = await sKURepository
                               .GetRegulatorySKUWithFilterListAsync(sKUInput, dataRange, skipRecordCount);

            return result == null ? NoContent() : Ok(result);
        }

        // POST: This API is used to get Regulatory_SKU data
        //       based on certain criteria such as date range and skip record count
        [HttpPost, Route("digitaldashboard/RegulatorySKU/GetRegulatorySKUWithTotalCount/{dataRange:int}/{skipRecordCount:int}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(BMSRegulatorySKUWithTotalCount))]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<BMSRegulatorySKUWithTotalCount>> GetSKUWithTotalCount([FromBody] BMSRegulatorySKUInput sKUInput,
                                                                                             int dataRange,
                                                                                             int skipRecordCount)
        {
            if (dataRange <= 0 || skipRecordCount < 0)
            {
                return BadRequest();
            }

            var result = await sKURepository
                               .GetSKUWithTotalCountAsync(sKUInput,
                                                          dataRange,
                                                          skipRecordCount);

            return result.TotalCount == 0 ? NoContent() : Ok(result);
        }
        // POST: This API is used to get SKU 'SearchList'
        //       based on certain criteria such as date range and skip record count
        [HttpPost, Route("digitaldashboard/RegulatorySKU/GetSKUSearchList/{dataRange:int}/{skipRecordCount:int}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ColumnSKUList))]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ColumnSKUList>> GetSKUSearchList([FromBody] SearchText searchText,
                                                                     int dataRange,
                                                                     int skipRecordCount)
        {
            if (dataRange <= 0 || skipRecordCount < 0)
            {
                return BadRequest();
            }

            var result = await sKURepository
                               .GetSKUSearchListAsync(searchText.Text, dataRange, skipRecordCount);

            return result.TotalCount == 0 ? NoContent() : Ok(result);
        }

        // POST: This API is used to get SKU 'SearchList' by cascading filter
        //       based on certain criteria such as date range and skip record count
        [HttpPost, Route("digitaldashboard/RegulatorySKU/GetSKUSearchListByCascadingFilter/{dataRange:int}/{skipRecordCount:int}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ColumnSKUList))]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ColumnSKUList>> GetSKUSearchListByCascadingFilter([FromBody] BMSRegulatorySKUInput sKUInput,
                                                                                      int dataRange,
                                                                                      int skipRecordCount)
        {
            if (dataRange <= 0 || skipRecordCount < 0)
            {
                return BadRequest();
            }

            var result = await sKURepository
                         .GetSKUSearchListByCascadingFilterAsync(sKUInput, dataRange, skipRecordCount);

            return result == null ? NoContent() : Ok(result);
        }
    }
}
