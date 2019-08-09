using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Quill.WebApi.Model;
using Quill.WebApi.Service.Interfaces;

namespace Quill.WebApi.Service.Services
{
    [ServiceVersion]
    public class SmsService : ISmsService
    {
        public Result<string> Send(string phone, string msg)
        {
            var result = new Result<string>();

            result.Data = msg;
            return result.Assign("0000", "真实下发成功");
        }
    }

    [ServiceVersion("2.0")]
    public class SimulateSmsService : ISmsService
    {
        public Result<string> Send(string phone, string msg)
        {
            var result = new Result<string>();

            result.Data = msg;
            return result.Assign("1000", "模拟下发成功");
        }
    }
}
