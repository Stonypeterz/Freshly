using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Razor.TagHelpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Freshly.UI.TagHelpers
{
    [HtmlTargetElement("a", Attributes = "is-parent")]
    public class ActiveLinkTagHelper : TagHelper
    {
        private IHttpContextAccessor ctxAccess;
        public bool IsParent { get; set; }

        public ActiveLinkTagHelper(IHttpContextAccessor _ctx)
        {
            ctxAccess = _ctx;
        }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            var Req = ctxAccess.HttpContext.Request.AbsoluteUri();
            var url = context.AllAttributes["href"]?.Value?.ToString();
            url = url?.Replace("~/", "");
            bool flag = IsParent ? Req.ToLower().Contains(url.ToLower()) : Req.ToLower().EndsWith(url.ToLower());
            if (flag) {
                var clsattr = context.AllAttributes["class"];
                string cls = "";
                if (clsattr != null) {
                    cls = clsattr.Value.ToString();
                    output.Attributes.Remove(clsattr);
                }
                var attr = $"active {cls}";
                output.Attributes.SetAttribute("class", attr);
            }
        }
    }
}
