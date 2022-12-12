using DigitalDashboard.BLL.Interfaces;
using DigitalDashboard.DAL.Models;
using Microsoft.AspNetCore.Mvc;
using static System.Reflection.Metadata.BlobBuilder;

namespace DigitalDashboard.API.Controllers
{
    //[Route("api/RegulationByCountry")]
    [ApiController]
    public class RegulationByCountryController : ControllerBase
    {
        private readonly IRegulationsByCountryRepository countryRepository;

        // Constructor:
        //    IRegulationsByCountryRepository instance is retrieved from DI via constructor injection
        public RegulationByCountryController(IRegulationsByCountryRepository countryRepository) =>
            this.countryRepository = countryRepository;

        // Summary:
        //    POST: This API is used to get 'regulation by country' data
        //          based on certain criteria such as date range and skip record count.
        [HttpPost, Route("digitaldashboard/RegulationByCountry/GetRegulationsByCountry/{dataRange:int}/{skipRecordCount:int}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(BMSRegulatoryCountryWithTotalCount))]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<BMSRegulatoryCountryWithTotalCount>> GetRegulationsByCountry([FromBody] BMSRegulatoryCountryInput countryInput,
                                                                                                    int dataRange,
                                                                                                    int skipRecordCount)
        {
            if (dataRange <= 0 || skipRecordCount < 0)
            {
                return BadRequest();
            }

            var result = await countryRepository
                         .GetRegulationsByCountryAsync(countryInput,
                                                       dataRange,
                                                       skipRecordCount);

            return result == null ? NoContent() : Ok(result);
        }

        #region New Service Added On: 01 Dec 2022
        // New Version
        [HttpPost, Route("digitaldashboard/RegulationByCountry/GetRegulationsByCountry/New/{dataRange:int}/{skipRecordCount:int}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(BMSRegulatoryCountryWithTotalCount))]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<BMSRegulatoryCountryWithTotalCount>> GetRegulationsByCountryWithFilters([FromBody] BMSRegulatoryCountryInput countryInput,
                                                                                                    int dataRange,
                                                                                                    int skipRecordCount)
        {
            if (dataRange <= 0 || skipRecordCount < 0)
            {
                return BadRequest();
            }

            var result = await countryRepository
                         .GetRegulationsByCountryWithFiltersAsync(countryInput,
                                                       dataRange,
                                                       skipRecordCount);

            return result == null ? NoContent() : Ok(result);
        }
        #endregion

        // Summary:
        //    POST: This API is used to get 'regulation by country' data
        //          based on search text.
        [HttpPost, Route("digitaldashboard/RegulationByCountry/SearchRegulationsByCountry/{searchText}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(BMSRegulatoryCountry))]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<BMSRegulatoryCountry>> SearchRegulationsByCountryUsingText(string searchText)
        {
            if (string.IsNullOrEmpty(searchText))
            {
                return BadRequest();
            }

            var result = await countryRepository.SearchRegulationsByCountryUsingTextAsync(searchText);
            return result == null ? NoContent() : Ok(result);
        }

        // Summary:
        //    GEt: This API is used to get filtered regulatory category
        [HttpGet, Route("digitaldashboard/RegulationByCountry/GetFilteredRegulatoryCategory")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(RegulatoryCategoryList))]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<ActionResult<RegulatoryCategoryList>> GetFilteredRegulatoryCategory()
        {
            var result = await countryRepository.GetFilteredRegulatoryCategoryAsync();
            return result == null ? NoContent() : Ok(result);
        }
    }
}
