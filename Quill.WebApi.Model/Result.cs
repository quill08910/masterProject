using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quill.WebApi.Model
{
    public class Result
    {
        public string Code { get; set; }
        public string Msg { get; set; }

        public Result Assign(string code, string msg)
        {
            this.Code = code;
            this.Msg = msg;

            return this;
        }
    }

    public class Result<T> : Result
    {
        public T Data { get; set; }

        public new Result<T> Assign(string code, string msg)
        {
            this.Code = code;
            this.Msg = msg;

            return this;
        }
    }
}
