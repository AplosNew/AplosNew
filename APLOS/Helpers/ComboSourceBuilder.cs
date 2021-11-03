using Library.Core;
using System.Collections.Generic;
using System.Text;

namespace Aplos.Helpers
{
    public class ComboSourceBuilder
    {
        public static string GetComboSource(List<ComboModel> list)
        {
            return GetComboSource(list, false);
        }

        public static string GetComboSource(List<ComboModel> list, bool addEmpty)
        {
            var source = new StringBuilder();
            if (addEmpty)
                source.Append("<option value=>Select One</option>");
            foreach (var item in list)
            {
                source.Append("<option value=");
                source.Append(item.Value);
                source.Append(">");
                source.Append(item.Text);
                source.Append("</option>");
            }
            return source.ToString();
        }
    }
}