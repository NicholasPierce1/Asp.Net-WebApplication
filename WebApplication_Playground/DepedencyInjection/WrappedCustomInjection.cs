using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace WebApplication_Playground.DepedencyInjection
{

    public interface IWrappedCustomInjection
    {
        public abstract Dictionary<string, string> wrappedCultureInfo { get; }
    }

    public sealed class WrappedCustomInjection : IWrappedCustomInjection
    {

        private readonly CustomInjectionInterface customInjectionInterface;

        public WrappedCustomInjection(CustomInjectionInterface customInjectionInterface)
        {
            this.customInjectionInterface = customInjectionInterface;
        }

        public Dictionary<string,string> wrappedCultureInfo {
            get => new Dictionary<string, string>
            {
                {"cultureInfo", this.customInjectionInterface.currentLocale},
                {"currentDateTime", DateTime.Now.ToString(new CultureInfo("en-US"))}
            };
        }

    }
}
