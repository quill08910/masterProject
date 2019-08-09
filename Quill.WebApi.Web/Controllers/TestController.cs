﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Autofac;
using Autofac.Features.Indexed;

namespace Quill.WebApi.Web.Controllers
{
    public class MsgContainer
    {
        public static string Output = string.Empty;
        public static void Append(object msg) { Output += msg + "<br />"; }
    }

    public class TestController : Controller
    {
        public string Index()
        {
            MsgContainer.Output = string.Empty;

            MsgContainer.Append("开始运行");
            var cb = new ContainerBuilder();

            cb.RegisterModule<CoreModule>();
            cb.RegisterModule<SmsCoreModule>();
            cb.RegisterModule<ConsoleSmsModule>();
            cb.RegisterModule<HttpApiSmsModule>();

            var container = cb.Build();

            var userBll = container.Resolve<IUserBll>();
            var login = userBll.Login("yueluo", "newbe");
            MsgContainer.Append(login);

            login = userBll.Login("newbe", "yueluo");
            MsgContainer.Append(login);

            MsgContainer.Append("结束运行");


            return MsgContainer.Output;
        }
    }

    public class CoreModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            base.Load(builder);
            builder.RegisterType<UserDal>().As<IUserDal>();
            builder.RegisterType<UserBll>().As<IUserBll>();
            builder.RegisterType<ConfigProvider>().As<IConfigProvider>();
        }
    }

    public class SmsCoreModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            base.Load(builder);
            builder.RegisterType<SmsSenderFactory>()
                .As<ISmsSenderFactory>();
        }
    }

    public class ConsoleSmsModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            base.Load(builder);
            builder.RegisterType<ConsoleSmsSender>().AsSelf();// 此注册将会使得 ConsoleSmsSender.Factory 能够被注入
            builder.RegisterType<ConsoleSmsSenderFactoryHandler>()
                .Keyed<ISmsSenderFactoryHandler>(SmsSenderType.Console);
        }
    }

    public class HttpApiSmsModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            base.Load(builder);
            builder.RegisterType<HttpApiSmsSender>().AsSelf();
            builder.RegisterType<SmsSenderFactoryHandler>()
                .Keyed<ISmsSenderFactoryHandler>(SmsSenderType.HttpAPi);
        }
    }

    public class UserBll : IUserBll
    {
        private readonly IUserDal _userDal;
        private readonly ISmsSenderFactory _smsSenderFactory;

        public UserBll(
            IUserDal userDal,
            ISmsSenderFactory smsSenderFactory)
        {
            _userDal = userDal;
            _smsSenderFactory = smsSenderFactory;
        }

        public bool Login(string phone, string password)
        {
            var re = _userDal.Exists(phone, password);
            if (re)
            {
                var smsSender = _smsSenderFactory.Create();
                smsSender.Send(phone, "您已成功登录系统");
            }

            return re;
        }
    }

    public enum SmsSenderType
    {
        /// <summary>
        /// 控制台发送短信
        /// </summary>
        Console,

        /// <summary>
        /// 通过HttpApi进行发送短信
        /// </summary>
        HttpAPi,
    }

    public interface ISmsSenderFactory
    {
        ISmsSender Create();
    }

    public class SmsSenderFactory : ISmsSenderFactory
    {
        private readonly IConfigProvider _configProvider;
        private readonly IIndex<SmsSenderType, ISmsSenderFactoryHandler> _smsSenderFactoryHandlers;

        public SmsSenderFactory(
            IConfigProvider configProvider,
            IIndex<SmsSenderType, ISmsSenderFactoryHandler> smsSenderFactoryHandlers)
        {
            _configProvider = configProvider;
            _smsSenderFactoryHandlers = smsSenderFactoryHandlers;
        }

        public ISmsSender Create()
        {
            // 短信发送者创建，从配置管理中读取当前的发送方式，并创建实例
            var smsConfig = _configProvider.GetSmsConfig();
            // 通过工厂方法的方式，将如何创建具体短信发送者的逻辑从这里移走，实现了这个方法本身的稳定。
            var factoryHandler = _smsSenderFactoryHandlers[smsConfig.SmsSenderType];
            var smsSender = factoryHandler.Create();
            return smsSender;
        }
    }

    /// <summary>
    /// 工厂方法接口
    /// </summary>
    public interface ISmsSenderFactoryHandler
    {
        ISmsSender Create();
    }

    #region 控制台发送短信接口实现

    public class ConsoleSmsSenderFactoryHandler : ISmsSenderFactoryHandler
    {
        private readonly ConsoleSmsSender.Factory _factory;

        public ConsoleSmsSenderFactoryHandler(
            ConsoleSmsSender.Factory factory)
        {
            _factory = factory;
        }

        public ISmsSender Create()
        {
            return _factory();
        }
    }

    /// <summary>
    /// 调试·短信API
    /// </summary>
    public class ConsoleSmsSender : ISmsSender
    {
        public delegate ConsoleSmsSender Factory();
        public void Send(string phone, string message)
        {
            MsgContainer.Append(string.Format("已给{0}发送消息：{1}", phone, message));
        }
    }

    #endregion

    #region API发送短信接口实现

    public class SmsSenderFactoryHandler : ISmsSenderFactoryHandler
    {
        private readonly HttpApiSmsSender.Factory _factory;

        public SmsSenderFactoryHandler(
            HttpApiSmsSender.Factory factory)
        {
            _factory = factory;
        }

        public ISmsSender Create()
        {
            return _factory();
        }
    }

    /// <summary>
    /// 真·短信API
    /// </summary>
    public class HttpApiSmsSender : ISmsSender
    {
        public delegate HttpApiSmsSender Factory();
        public void Send(string phone, string message)
        {
            MsgContainer.Append(string.Format("已调用API给{0}发送消息：{1}", phone, message));
        }
    }

    #endregion

    public class SmsConfig
    {
        public SmsSenderType SmsSenderType { get; set; }
    }

    public interface IConfigProvider
    {
        SmsConfig GetSmsConfig();
    }

    public class ConfigProvider : IConfigProvider
    {
        private readonly SmsConfig _smsConfig = new SmsConfig
        {
            SmsSenderType = SmsSenderType.Console
        };

        public SmsConfig GetSmsConfig()
        {
            // 此处直接使用了写死的短信发送配置，实际项目中往往是通过配置读取的方式，实现该配置的加载。
            return _smsConfig;
        }
    }

    /// <summary>
    /// 发送短信
    /// </summary>
    public interface ISmsSender
    {
        /// <summary>
        /// 发送短信
        /// </summary>
        /// <param name="phone">手机</param>
        /// <param name="message">短信  </param>
        void Send(string phone, string message);
    }

    /// <summary>
    /// 用户业务
    /// </summary>
    public interface IUserBll
    {
        /// <summary>
        /// 登录
        /// </summary>
        /// <param name="phone"></param>
        /// <param name="password"></param>
        bool Login(string phone, string password);
    }

    public interface IUserDal
    {
        bool Exists(string phone, string password);
    }

    public class UserDal : IUserDal
    {
        private const string StaticPassword = "newbe";
        private const string StaticPhone = "yueluo";

        public bool Exists(string phone, string password)
        {
            // 使用固定的账号密码验证
            return phone == StaticPhone && password == StaticPassword;
        }
    }
}