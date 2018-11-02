using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Freshly.Identity.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

namespace Freshly.UI
{
    public class FreshlyHelpers
    {
        //internal static void SetOptions(FreshlyOptions obj)
        //{
        //    obj.JwtOptions = new JwtBearerOptions()
        //    {
        //        RequireHttpsMetadata = false,
        //        SaveToken = true,
        //        TokenValidationParameters = new TokenValidationParameters()
        //        {
        //            ValidIssuer = Configuration["Tokens:Issuer"],
        //            ValidAudience = Configuration["Tokens:Issuer"],
        //            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["Tokens:Key"]))
        //        };
        //    }
        //}
    }
}
