using OpdrachtAPI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OpdrachtAPI.Services
{
    public interface IUserService
    {
        User Authenticate(string username, string password);
    }
}
