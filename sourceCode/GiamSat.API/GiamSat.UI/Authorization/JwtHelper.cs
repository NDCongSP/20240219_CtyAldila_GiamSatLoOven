using System.Security.Claims;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;


namespace GiamSat.UI
{
    public class JwtHelper
    {
        public static ClaimsPrincipal GetClaimsPrincipalFromJwt(string jwt)
        {
            return new ClaimsPrincipal(new ClaimsIdentity(GetClaimsFromJwt(jwt)));
        }

        public static IEnumerable<Claim> GetClaimsFromJwt(string jwt)
        {
            var claims = new List<Claim>();
            string payload = jwt.Split('.')[1];
            byte[] jsonBytes = ParseBase64WithoutPadding(payload);
            var keyValuePairs = JsonSerializer.Deserialize<Dictionary<string, object>>(jsonBytes);
            if (keyValuePairs is not null)
            {
                foreach (var kvp in keyValuePairs)
                {
                    Console.WriteLine(kvp.Key);
                    Console.WriteLine(kvp.Value.ToString() ?? string.Empty);
                    Console.WriteLine(kvp.Value.GetType());

                    if (kvp.Value is JsonElement elm)
                    {
                        if (elm.ValueKind == JsonValueKind.Array)
                        {
                            var enume = elm.EnumerateArray();
                            while (enume.MoveNext())
                            {
                                claims.Add(new Claim(kvp.Key, enume.Current.ToString()));
                            }
                        }
                        else
                        {
                            claims.Add(new Claim(kvp.Key, kvp.Value.ToString() ?? string.Empty));
                        }
                    }

                    // claims.Add(new Claim(kvp.Key, kvp.Value.ToString() ?? string.Empty));

                    //if (kvp.Key == ClaimRoleValue)
                    //{
                    //    List<string> roles = JsonSerializer.Deserialize<List<string>>(kvp.Value.ToString());
                    //    foreach (var role in roles)
                    //    {
                    //        claims.Add(new Claim(ClaimTypes.Role, role));
                    //    }
                    //}

                }
                // claims.AddRange(keyValuePairs.Select(kvp => new Claim(kvp.Key, kvp.Value.ToString() ?? string.Empty)));

                //foreach (var item in claimsIdentity.Claims.ToList())
                //{
                //    Console.WriteLine(item.Type + "Value = " + item.Value);
                //    if (item.Type == ClaimRoleValue)
                //    {
                //        List<string> roles = JsonSerializer.Deserialize<List<string>>(item.Value);
                //        foreach (var role in roles)
                //        {
                //            claimsIdentity.AddClaim(new Claim(ClaimTypes.Role, role));
                //        }
                //    }
                //}
            }
            return claims;
        }

        private static byte[] ParseBase64WithoutPadding(string payload)
        {
            payload = payload.Trim().Replace('-', '+').Replace('_', '/');
            var base64 = payload.PadRight(payload.Length + (4 - payload.Length % 4) % 4, '=');
            return Convert.FromBase64String(base64);
        }
    }
}
