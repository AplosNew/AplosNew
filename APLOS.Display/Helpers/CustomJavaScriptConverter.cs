using Library.Core;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Web.Script.Serialization;

namespace Aplos.Helpers
{
    public class CustomJavaScriptConverter : JavaScriptConverter
    {
        public override IEnumerable<Type> SupportedTypes
        {
            get
            {
                var retType = new List<Type>();
                retType.AddRange(Assembly.Load("Aplos.Core").GetTypes());
                retType.AddRange(Assembly.Load("Library.Model").GetTypes());
                return retType;
            }
        }
        public override object Deserialize(IDictionary<string, object> dictionary, Type type, JavaScriptSerializer serializer)
        {
            var model = Activator.CreateInstance(type);
            if (type.BaseType == typeof(BaseModel))
            {
                ((BaseModel)model).SetUnchanged();
            }
            var props = type.GetProperties();
            foreach (var key in dictionary.Keys)
            {
                var prop = props.FirstOrDefault(t => t.Name == key);
                if (prop == null) continue;
                var val = dictionary[key].IsNull() ? string.Empty : dictionary[key].ToString();
                if (prop.PropertyType == typeof(string))
                    prop.SetValue(model, val, null);
                else if (prop.PropertyType == typeof(DateTime))
                {
                    if (val.IsNullOrEmpty()) continue;
                    prop.SetValue(model, DateTime.ParseExact(val, Util.ConvertedDateFormat, DateTimeFormatInfo.InvariantInfo), null);
                }
                else
                    prop.SetValue(model, val.Value(), null);
            }

            return model;
        }

        public static JavaScriptSerializer GetCustomSerializer()
        {
            var serializer = new JavaScriptSerializer();
            serializer.RegisterConverters(new[] {new CustomJavaScriptConverter()});
            return serializer;
        }

        public override IDictionary<string, object> Serialize(object obj, JavaScriptSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }
}
