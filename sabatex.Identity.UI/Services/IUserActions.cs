using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace sabatex.Identity.UI.Services
{
    public interface IUserActions 
    {
        void UserAdded(object user);
    }
}
