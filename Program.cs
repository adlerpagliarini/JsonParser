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
            var stringfy = @"{ 'name': 'adler', 'age': 28,
                               'obj1': { 'B': 'B', 'C': { 'CC': 1 }, 'D': { 'DD': 2 }, Arr: [{ 'Ar': 1 }, { 'Ar': 2 }] },
                               'obj2': { 'B': 'B', 'C': { 'CC': 1 }, 'D': { 'DD': 2 }, Arr: [{ 'Ar': 1 }, { 'Ar': 2 }] }
                           }";

            var jsonObject = JsonConvert.DeserializeObject<JObject>(stringfy);

            static IEnumerable<JObject> CreateObject(JObject jObject, string parentName = null)
            {
                if (!(parentName is null))
                    jObject = RenameByParentName(jObject, parentName);

                var fields = jObject.Properties().Where(p => p.Value.GetType().Name == "JValue").ToList();
                var objects = jObject.Properties().Where(p => p.Value.GetType().Name == "JObject").ToList();

                var innerObjects = ProcessInnerObjects(objects);

                var joinedAll = innerObjects.Any() 
                    ? innerObjects.Select(innerObject => JoinObject(new JObject(fields), innerObject))
                    : new List<JObject> { new JObject(fields) };

                return joinedAll;
            }

            static List<JObject> ProcessInnerObjects(List<JProperty> jObjects)
            {
                var inners = new List<JObject>();
                var renamed = jObjects?.Select(obj => RenameByParentName(obj.Value.ToObject<JObject>(), obj.Name)).ToList();
                
                if (renamed?.Count > 0)
                {
                    var groupRenamed = renamed.Aggregate((acc, next) => JoinObject(acc, next));
                    
                    var nestedObjects = groupRenamed.Properties().Where(p => p.Value.GetType().Name == "JObject").ToList();
                    var nestedArrays = groupRenamed.Properties().Where(p => p.Value.GetType().Name == "JArray").ToList();

                    nestedArrays.ForEach(p => groupRenamed.Remove(p.Name));
                    nestedObjects.ForEach(p => groupRenamed.Remove(p.Name));

                    var tempNestedObjectList = new List<JObject>();
                    foreach (var innerObject in nestedObjects)
                    {
                        var temp = CreateObject(innerObject.Value.ToObject<JObject>(), innerObject.Name);
                        tempNestedObjectList.Add(temp.First());
                    }

                    var tempNestedArraysObjectList = new List<JObject>();
                    foreach (var innerArray in nestedArrays)
                    {
                        var name = innerArray.Name;
                        var objects = innerArray.Value;
                        var renamedObjects = objects?.Select(obj => RenameByParentName(obj.ToObject<JObject>(), name)).ToList();
                        foreach (var innerObject in renamedObjects) {
                            var temp = CreateObject(innerObject.ToObject<JObject>());
                            tempNestedArraysObjectList.Add(temp.First());
                        }
                    }

                    var groupedNestedObjects = tempNestedObjectList.Aggregate(groupRenamed, (acc, next) => JoinObject(acc, next));

                    if (!tempNestedArraysObjectList.Any())
                        inners.Add(groupedNestedObjects);
                    else
                        tempNestedArraysObjectList.ForEach(e => inners.Add(JoinObject(groupedNestedObjects, e)));
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
            var stringfied = JsonConvert.SerializeObject(created);
        }
    }
}
