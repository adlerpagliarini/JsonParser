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
                               'obj2': { 'B': 'B', 'C': { 'CC': 1 }, 'D': { 'DD': 2 }, Arr: [{ 'Ar': 1 }, { 'Ar': 2 }], ArrValue: [1,2,3] }
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
                    var nestedObjectsArray = nestedArrays.Where(array => array.Value.All(elements => elements.GetType().Name == "JObject")).ToList();
                    var nestedValuesArray = nestedArrays.Where(array => array.Value.All(elements => elements.GetType().Name == "JValue")).ToList();

                    nestedArrays.ForEach(p => groupRenamed.Remove(p.Name));
                    nestedObjects.ForEach(p => groupRenamed.Remove(p.Name));

                    var tempNestedObjectList = new List<JObject>();
                    foreach (var innerObject in nestedObjects)
                    {
                        var temp = CreateObject(innerObject.Value.ToObject<JObject>(), innerObject.Name);
                        tempNestedObjectList.Add(temp.First());
                    }

                    var tempNestedArraysObjectList = new List<JObject>();
                    foreach (var innerArray in nestedObjectsArray)
                    {
                        var name = innerArray.Name;
                        var objects = innerArray.Value;
                        var renamedObjects = objects?.Select(obj => RenameByParentName(obj.ToObject<JObject>(), name)).ToList();
                        foreach (var innerObject in renamedObjects) {
                            var temp = CreateObject(innerObject.ToObject<JObject>());
                            tempNestedArraysObjectList.Add(temp.First());
                        }
                    }

                    var tempNestedArraysValueList = new List<JObject>();
                    foreach (var innerArray in nestedValuesArray)
                    {
                        var name = innerArray.Name;
                        var values = innerArray.Value;
                        foreach (var innerValue in values)
                        {
                            tempNestedArraysValueList.Add(new JObject(new JProperty(name, innerValue.ToObject<JValue>())));
                        }
                    }

                    var groupedNestedObjects = tempNestedObjectList.Aggregate(groupRenamed, (acc, next) => JoinObject(acc, next));

                    var joined = new List<JObject>();
                    if (!tempNestedArraysObjectList.Any())
                        joined.Add(groupedNestedObjects);
                    else
                        tempNestedArraysObjectList.ForEach(e => joined.Add(JoinObject(groupedNestedObjects, e)));

                    // To all values, join with Inners for (e: values, inner: inners)
                    if (tempNestedArraysValueList.Any())
                        inners.AddRange(tempNestedArraysValueList.SelectMany(value => joined, (value, joinedItem) => JoinObject(joinedItem, value)).ToList());
                    else
                        inners.AddRange(joined);
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
