namespace EventDrive.Utils.Extensions;

using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;

public static class CommonExtensions
{
    private static readonly IContractResolver DefaultContractResolver = new DefaultContractResolver();

    public static bool HasPassed(this DateTime date) => date < DateTime.Now;

    public static string Beautify(this object obj, IContractResolver contractResolver = null) => JsonConvert
        .SerializeObject(obj, new JsonSerializerSettings
        {
            NullValueHandling = NullValueHandling.Ignore,
            Formatting = Formatting.Indented,
            ContractResolver = contractResolver ?? DefaultContractResolver
        });
}