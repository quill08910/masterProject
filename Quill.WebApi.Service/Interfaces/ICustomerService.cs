using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Quill.WebApi.Model;

namespace Quill.WebApi.Service.Interfaces
{
    public interface ICustomerService : IInjectionService
    {
        DBAccess.Customer Query(string phone);
        Result<string> Register(string phone);
        Result Login(string phone);
    }
}
