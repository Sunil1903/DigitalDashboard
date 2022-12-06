using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.DirectoryServices.AccountManagement;

namespace DigitalDashboard.BLL.Authorization
{
    public class UserAuthorization : IUserAuthorization
    {
        // Check user access rights
        public bool VerifyAccessRights()
        {
            bool authorized = false;
            try
            {
                PrincipalContext pc = new PrincipalContext(ContextType.Domain, "lnties.com");
                if (pc != null)
                {
                    authorized = true;
                }
            }
            catch
            {
                authorized = false;
                // throw new HttpResponseException(HttpStatusCode.Unauthorized);
            }
            return authorized;
        }
    }
}
