using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace restapi2
{
    public class AuthorizedRoleAttribute : Attribute
    {
        public string[] AllowedRoles { get; set; }

        public AuthorizedRoleAttribute(params string[] roles)
        {
            AllowedRoles = roles;
        }
    }
}
