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
            var stringfy = @"{ 'name': 'adler', 'age': 28, 'obj1': { 'A': 'A' }, 'obj2': { 'B': 'B', 'C': { 'CC': 1 } } }";

            var jsonObject = JsonConvert.DeserializeObject<JObject>(stringfy);

            static IEnumerable<JObject> CreateObject(JObject jObject, string parentName = null)
            {
                if (!(parentName is null))
                    jObject = RenameByParentName(jObject, parentName);

                var fields = jObject.Properties().Where(p => p.Value.GetType().Name == "JValue").ToList();
                var objects = jObject.Properties().Where(p => p.Value.GetType().Name == "JObject").ToList();
                var arrays = jObject.Properties().Where(p => p.Value.GetType().Name == "JArray").ToList();

                var renamed = objects.Select(obj => RenameByParentName(obj.Value.ToObject<JObject>(), obj.Name)).ToList();
                var innerObjects = new List<JProperty>();
                var groupRenamed = new JObject();
                if (renamed.Count > 0)
                {
                    groupRenamed = renamed.Aggregate((acc, next) => JoinObject(acc, next));
                    var nestedObjects = groupRenamed.Properties().Where(p => p.Value.GetType().Name == "JObject").ToList();
                    innerObjects.AddRange(nestedObjects);
                    nestedObjects.ForEach(p => groupRenamed.Remove(p.Name));
                }

                var inners = new List<JObject>();
                foreach (var inner in innerObjects)
                {
                    var temp = CreateObject(inner.Value.ToObject<JObject>(), inner.Name).First();
                    inners.Add(temp);
                }

                var interJoined = JoinObject(new JObject(fields), groupRenamed);
                var joinedAll = inners.Aggregate(interJoined, (joined, next) => JoinObject(joined, next));

                return new List<JObject> { joinedAll };
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
