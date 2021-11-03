using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;


public static class Extensions
{
    public static List<T> ToList<T>(this DataTable table) where T : new()
    {
        IList<PropertyInfo> properties = typeof(T).GetProperties().ToList();
        List<T> result = new List<T>();

        foreach (var row in table.Rows)
        {
            var item = CreateItemFromRow<T>((DataRow)row, properties);
            result.Add(item);
        }

        return result;
    }

    private static T CreateItemFromRow<T>(DataRow row, IList<PropertyInfo> properties) where T : new()
    {
        T item = new T();
        foreach (var property in properties)
        {
            if (property.PropertyType == typeof(System.DayOfWeek))
            {
                DayOfWeek day = (DayOfWeek)Enum.Parse(typeof(DayOfWeek), row[property.Name].ToString());
                property.SetValue(item, day, null);
            }
            else
            {
                if (row[property.Name] == DBNull.Value)
                    property.SetValue(item, null, null);
                else
                    property.SetValue(item, row[property.Name], null);
            }
        }
        return item;
    }
}


public class CustomJsonResultService
{
    public static List<Dictionary<string, object>> DataTableToJson(DataTable dt)
    {
        List<Dictionary<string, object>> dictionaries = new List<Dictionary<string, object>>();
        foreach (DataRow row in dt.Rows)
        {
            Dictionary<string, object> dictionary = Enumerable.Range(0, dt.Columns.Count)
                .ToDictionary(i => dt.Columns[i].ColumnName, i => row.ItemArray[i]);



            dictionaries.Add(dictionary);
        }
        return dictionaries;
    }
}

