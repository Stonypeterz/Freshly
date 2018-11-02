using Microsoft.AspNetCore.Razor.TagHelpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Freshly.TagHelpers
{
    [HtmlTargetElement("div", Attributes = "alert")]
    public class AlertTagHelper : TagHelper
    {
        public ResponseObject Alert { get; set; }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            var closer = "<span data-dismiss=\"alert\" class=\"close\">&times;</span>";
            var id = context.AllAttributes["id"]?.Value.ToString().ToLower();
            if (string.IsNullOrEmpty(id)) id = "none";
            if (Alert.Msg.Contains("alert alert-") && id != "toast") {
                Alert.Msg = Alert.Msg.Replace(closer, "");
                output.Content.AppendHtml(Alert.Msg);
            } else {
                if (!Alert.Msg.Contains("alert alert-")) {
                    var clsattr = context.AllAttributes["class"];
                    string cls = "";
                    if (clsattr != null) {
                        cls = clsattr.Value.ToString();
                        output.Attributes.Remove(clsattr);
                    }
                    var attr = $"alert alert-{Alert.Alert.ToString()} {cls}";
                    output.Attributes.SetAttribute("class", attr);
                    if (id == "toast") output.Content.AppendHtml($"{closer}<h4>{Alert.Title}</h4><p>{Alert.Msg}</p>");
                    else output.Content.AppendHtml($"<p>{Alert.Msg}</p>");
                } else {
                    if (id == "toast") output.Content.AppendHtml($"{closer}<h4>{Alert.Title}</h4><p>{Alert.Msg.Replace(closer, "")}</p>");
                    else output.Content.AppendHtml(Alert.Msg);
                }
            }
        }
    }
}
