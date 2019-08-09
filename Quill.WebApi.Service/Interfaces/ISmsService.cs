using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Quill.WebApi.Model;

namespace Quill.WebApi.Service.Interfaces
{
    public interface ISmsService : IMultiVersionService
    {
        Result<string> Send(string phone, string msg);
    }
}
