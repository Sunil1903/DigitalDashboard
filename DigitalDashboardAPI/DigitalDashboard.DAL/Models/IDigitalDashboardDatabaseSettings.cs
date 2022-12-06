using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DigitalDashboard.DAL.Models
{
    public interface IDigitalDashboardDatabaseSettings
    {
        string BMSRegulatorySKUCollectionName { get; set; }
        string BMSRegulationsByCountryCollectionName { get; set; }
        string LogCollectionName { get; set; }
        string ConnectionString { get; set; }
        string DatabaseName { get; set; }
    }
}
