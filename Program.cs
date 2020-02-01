using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;

namespace JsonParser
{
    class Program
    {
        static void Main(string[] args)
        {
            var stringfy = @"{ 'name': 'adler', 'age': 28, 'obj': { 'A': 1 }, 'obj2': { 'B': 'B', 'C': { 'CC': 1 }, 'D': { 'DD': 2 } }
                           }";

            var jsonObject = JsonConvert.DeserializeObject<JObject>(stringfy);

            static IEnumerable<JObject> CreateObject(JObject jObject, string parentName = null)
            {
                if (!(parentName is null))
                    jObject = RenameByParentName(jObject, parentName);

                var fields = jObject.Properties().Where(p => p.Value.GetType().Name == "JValue").ToList();
                var objects = jObject.Properties().Where(p => p.Value.GetType().Name == "JObject").ToList();

                var inners = ProcessInnerObjects(objects);

                var joinedAll = inners.Aggregate(new JObject(fields), (joined, next) => JoinObject(joined, next));

                return new List<JObject> { joinedAll };
            }

            static List<JObject> ProcessInnerObjects(List<JProperty> jObjects)
            {
                var inners = new List<JObject>();
                var renamed = jObjects?.Select(obj => RenameByParentName(obj.Value.ToObject<JObject>(), obj.Name)).ToList();
                
                if (renamed?.Count > 0)
                {
                    var groupRenamed = renamed.Aggregate((acc, next) => JoinObject(acc, next));
                    var nestedObjects = groupRenamed.Properties().Where(p => p.Value.GetType().Name == "JObject").ToList();
                    nestedObjects.ForEach(p => groupRenamed.Remove(p.Name));

                    var tempList = new List<JObject>();
                    foreach (var inner in nestedObjects)
                    {
                        var temp = CreateObject(inner.Value.ToObject<JObject>(), inner.Name).First();
                        tempList.Add(temp);
                    }

                    var grouped = tempList.Aggregate(groupRenamed, (acc, next) => JoinObject(acc, next));
                    inners.Add(grouped);
                }
                return inners;
            }

            static JObject RenameByParentName(JObject jObject, string parentName)
            {
                var properties = jObject.Properties().ToList().Select(p => new JProperty($"{parentName}_{p.Name}", p.Value));
                return new JObject(properties);
            }

            static JObject JoinObject(JObject joinIt, JObject joinWith)
            {
                var joined = new JObject
                {
                    joinIt.Properties(),
                    joinWith.Properties()
                };
                return joined;
            }

            var created = CreateObject(jsonObject);
        }
    }
}
