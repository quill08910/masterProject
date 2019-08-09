using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Quill.WebApi.Model;
using Quill.WebApi.Service.Interfaces;

namespace Quill.WebApi.Service.Services
{
    public class CustomerService : ICustomerService
    {
        private readonly DBAccess.ChannelApiContext _db;

        public CustomerService(DBAccess.ChannelApiContext db)
        {
            this._db = db;
        }

        public DBAccess.Customer Query(string phone)
        {
            return this._db.Customer.FirstOrDefault(o => o.CellPhone == phone);
        }

        public Result<string> Register(string phone)
        {
            var result = new Result<string>();

            var customer = this.Query(phone);
            if (customer != null)
                return result.Assign("0001", "用户已存在");

            var service = ServiceContainer.Get<ISmsService>("2.0");
            return service.Send(phone, "恭喜你注册成功");
        }

        public Result Login(string phone)
        {
            var result = new Result();

            var customer = this.Query(phone);
            if (customer == null)
                return result.Assign("0001", "用户不存在");

            customer.LastLoginTime = DateTime.Now;
            this._db.SaveChanges();
            return result.Assign("0000", "成功");
        }
    }

    public class InsideCustomerService : ICustomerService
    {
        private readonly DBAccess.ChannelApiContext _db;

        public InsideCustomerService(DBAccess.ChannelApiContext db)
        {
            this._db = db;
        }

        public DBAccess.Customer Query(string phone)
        {
            return this._db.Customer.FirstOrDefault(o => o.CellPhone == phone);
        }

        public Result<string> Register(string phone)
        {
            var result = new Result<string>();

            var customer = this.Query(phone);
            if (customer != null)
                return result.Assign("0001", "用户已存在");

            var service = ServiceContainer.Get<ISmsService>("2.0");
            return service.Send(phone, "恭喜你注册成功");
        }

        public Result Login(string phone)
        {
            var result = new Result();

            var customer = this.Query(phone);
            if (customer == null)
                return result.Assign("0001", "用户不存在");

            customer.LastLoginTime = DateTime.Now;
            this._db.SaveChanges();
            return result.Assign("0000", "成功");
        }
    }
}
