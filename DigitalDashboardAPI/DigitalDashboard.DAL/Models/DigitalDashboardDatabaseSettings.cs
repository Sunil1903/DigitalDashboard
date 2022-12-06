using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DigitalDashboard.DAL.Models
{
    public class DigitalDashboardDatabaseSettings : IDigitalDashboardDatabaseSettings
    {
        public string BMSRegulatorySKUCollectionName { get; set; } = string.Empty;
        public string BMSRegulationsByCountryCollectionName { get; set; } = string.Empty;
        public string LogCollectionName { get; set; } = string.Empty;
        public string ConnectionString { get; set; } = string.Empty;
        public string DatabaseName { get; set; } = string.Empty;
    }
}
