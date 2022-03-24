using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace WebApplication_Playground.DepedencyInjection
{

    public interface CustomInjectionInterface
    {
        public abstract string getCurrentLocale();

        public string prefix { get; set; }

        public string currentLocale { get; }
    }

    public sealed class CustomInjection : CustomInjectionInterface
    {
        public string prefix { get; set; } = "Prefix";

        public string currentLocale { get => this.getCurrentLocale(); }

        public string getCurrentLocale()
        {

            return $"({this.prefix}) Current culture is: {CultureInfo.CurrentCulture.DisplayName}";
        }

    }
}
