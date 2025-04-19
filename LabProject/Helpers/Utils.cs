using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace LabProject.Helpers

{

    // AI Prompt: Create a singleton class in C# that provides a method to export a list of objects to JSON format. The method should allow for optional filtering of properties to include in the JSON output. The class should be thread-safe and should handle null or empty lists gracefully.
    // The class should also include error handling for invalid property names and should provide a way to customize the JSON serialization options.
    // The class should be named Utils and should be in a namespace called LabProject.Helpers. The method should be named ExportToJson and should take a list of objects, an optional list of property names to include, and return a JSON string.
    public sealed class Utils
    {
        private static readonly Lazy<Utils> lazyInstance = new(() => new Utils());

        public static Utils Instance => lazyInstance.Value;

        private Utils()
        {
        }

        public string ExportToJson<T>(List<T> data, List<string> selectedProperties = null)
        {
            if (data == null || data.Count == 0)
                return "{}";

            var options = new JsonSerializerOptions { WriteIndented = true };

            if (selectedProperties == null || selectedProperties.Count == 0)
            {
                return JsonSerializer.Serialize(data, options);
            }

            var filteredList = new List<Dictionary<string, object>>();
            foreach (var item in data)
            {
                var dict = new Dictionary<string, object>();
                foreach (var prop in typeof(T).GetProperties())
                {
                    if (selectedProperties.Contains(prop.Name))
                    {
                        dict[prop.Name] = prop.GetValue(item);
                    }
                }
                filteredList.Add(dict);
            }

            return JsonSerializer.Serialize(filteredList, options);
        }
    }
}
