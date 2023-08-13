using System;
using System.Collections.Generic;
using System.Text;

namespace y2mate.Config
{
    internal class Messages
    {
        // Error messages
        public static readonly string EmptyResponseError = $"Server ({GlobalConfig.ApiDomain}) vrátil prázdnou odpověď!";
        public static readonly string StatusCodeError = $"Server ({GlobalConfig.ApiDomain}) vrátil status code:";
        public static readonly string NoInternetConnectionError = "Připojení k internetu není dostupné.";
        public static readonly string ServerNotReachableError = $"Server ({GlobalConfig.ApiDomain}) není v tuto chvíli dostupný!";
        public static readonly string NoJsonReturned = $"Server ({GlobalConfig.ApiDomain}) nevrátil potřebný JSON! Vrátil:\n\n";
        
        public static readonly string FullUrlRequiredError = "Vložte prosím úplnou URL adresu!\n\nPř.\nhttps://www.youtube.com/watch?v=MNeX4EGtR5Y";

    }
}
