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
            var stringfy = @"{ 'name': 'Adler', 'secondName': 'Pagliarini',
                               'obj1': { 'A': 1 },
                               'obj2': { 'B': 2 },
                               'objInArr': { 'B': 'B', 'C': { 'CC': 'CC', 'D': { 'DD': 'DDD' } }, InArr: [{ 'Arr': 'Arr_1' }, { 'Arr': 'Arr_2' }] },
                               'ArrayN': [{ 'Array1': 1 }, { 'Array1': 11 }, { 'ArrayOfArray': [{ 'AofA': 'AofA_1' }, { 'AofA': 'AofA_2' }] }],
                               'ArrayValues': [33, 99]
                           }"; // OK

            //var stringfy = @"{ 'name': 'Adler', 'secondName': 'Pagliarini',
            //                   'obj1': { 'A': 1 },
            //                   'obj2': { 'B': 2 },
            //                   'objInArr': { 'B': 'B', ToFix : { 'DDAr': [{ 'DDAr': 1 }] } }
            //               }";


            //var stringfy = @"{ 'name': 'Adler', 'secondName': 'Pagliarini',
            //                   'obj1': { 'A': 1 },
            //                   'obj2': { 'B': 2 },
            //                   'objInArr': { 'B': 'B', 'C': { 'CC': 'CC', 'D': { 'DD': 'DDD', 'DDAr': [{ 'DDAr': 1 }, { 'DDAr': 2 }] } }, InArr: [{ 'Arr': 'Arr_1' }, { 'Arr': 'Arr_2' }] },
            //                   'ArrayN': [{ 'Array1': 1 }, { 'Array1': 11 }, { 'ArrayOfArray': [{ 'AofA': 'AofA_1' }, { 'AofA': 'AofA_2' }] }],
            //                   'ArrayValues': [33, 99]
            //               }";

            var jsonObject = JsonConvert.DeserializeObject<JObject>(stringfy);

            static IEnumerable<JObject> CreateObject(JObject jObject, string parentName = null)
            {
                if (!(parentName is null))
                    jObject = RenameByParentName(jObject, parentName);

                var fields = jObject.Properties().Where(p => p.Value.GetType().Name == "JValue").ToList();
                var objects = jObject.Properties().Where(p => p.Value.GetType().Name == "JObject").ToList();
                var arrays = jObject.Properties().Where(p => p.Value.GetType().Name == "JArray").ToList();
                var objectsArray = arrays.Where(array => array.Value.All(elements => elements.GetType().Name == "JObject")).ToList();
                var valuesArray = arrays.Where(array => array.Value.All(elements => elements.GetType().Name == "JValue")).ToList();

                var innerObjects = ProcessInnerObjects(objects);

                var joinedInnerObjects = innerObjects.Any() 
                    ? innerObjects.Select(innerObject => JoinObject(new JObject(fields), innerObject))
                    : new List<JObject> { new JObject(fields) };

                var arraysObjectList = GetObjectsFromArrayOfObject(objectsArray);
                var arraysValueList = GetObjectsFromArrayOfValue(valuesArray);
                var joinedAll = joinedInnerObjects.SelectMany(inner => JoinArraysWithObject(arraysObjectList, arraysValueList, inner));
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
                        tempNestedObjectList.AddRange(temp);
                    }

                    List<JObject> tempNestedArraysObjectList = GetObjectsFromArrayOfObject(nestedObjectsArray);
                    List<JObject> tempNestedArraysValueList = GetObjectsFromArrayOfValue(nestedValuesArray);
                    // AQUI
                    if (tempNestedObjectList.Count() > 1) throw new Exception("More");
                    var groupedNestedObjects = (tempNestedObjectList.Any()) ? tempNestedObjectList.Select(nested => JoinObject(groupRenamed, nested))
                        : new List<JObject> { groupRenamed };
                    var groupedNestedObjectsList = groupedNestedObjects.Select(groupedNested => JoinArraysWithObject(tempNestedArraysObjectList, tempNestedArraysValueList, groupedNested));
                    inners.AddRange(groupedNestedObjectsList.SelectMany(e => e));
                }
                return inners;
            }

            static List<JObject> JoinArraysWithObject(List<JObject> tempNestedArraysObjectList, List<JObject> tempNestedArraysValueList, JObject groupedNestedObjects)
            {
                var joinedResult = new List<JObject>();
                var joined = new List<JObject>();
                if (!tempNestedArraysObjectList.Any())
                    joined.Add(groupedNestedObjects);
                else
                    tempNestedArraysObjectList.ForEach(e => joined.Add(JoinObject(groupedNestedObjects, e)));

                // To all valueList, join with joined(list) being (e: values, joinedItem: joined), 1 item of Value with 1 item of Joined
                if (tempNestedArraysValueList.Any())
                    joinedResult.AddRange(tempNestedArraysValueList.SelectMany(value => joined, (value, joinedItem) => JoinObject(joinedItem, value)).ToList());
                else
                    joinedResult.AddRange(joined);
                return joinedResult;
            }

            static List<JObject> GetObjectsFromArrayOfObject(List<JProperty> nestedObjectsArray)
            {
                var tempNestedArraysObjectList = new List<JObject>();
                foreach (var innerArray in nestedObjectsArray)
                {
                    var name = innerArray.Name;
                    var objectsInArray = innerArray.Value;
                    var renamedObjects = objectsInArray?.Select(obj => RenameByParentName(obj.ToObject<JObject>(), name)).ToList();
                    foreach (var innerObject in renamedObjects)
                    {
                        var temp = CreateObject(innerObject.ToObject<JObject>());
                        tempNestedArraysObjectList.AddRange(temp);
                    }
                }
                return tempNestedArraysObjectList;
            }

            static List<JObject> GetObjectsFromArrayOfValue(List<JProperty> nestedValuesArray)
            {
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
                return tempNestedArraysValueList;
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
            JsonToCsv.JsonStringToCSV(stringfied);
        }
    }
}
