﻿using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace RoadRunner
{
    class DataUtility
    {
        static JToken Find(JObject response, string field)
        {
            var split = field.Split('.');
            foreach (var s in split)
            {
                if (response[s] is JObject)
                {
                    response = response[s].ToObject<JObject>();
                }
                else
                {
                    return response[s];
                }
            }
            return null;
        }

        public static List<AssertionData> GetTestAssertionDataFromRoads(JToken road, Dictionary<string, string> globalMap, string uniqueName, Assembly assembly)
        {
            List<AssertionData> data = new List<AssertionData>();

            try
            {
                var sampleCodeName = road["sampleClassNames"]["csharp"];
                var storedFields = road["storedResponseFields"].ToObject<List<string>>();

                // remove the old input fields from map.
                foreach (var field in storedFields)
                {
                    globalMap.Remove(uniqueName + field);
                }

                var dependentSampleCode = road["prerequisiteRoad"].ToString();

                var dependentFields = road["dependentFieldMapping"].ToObject<List<string>>();

                object[] inputFields = new object[dependentFields.Count];

                bool callSampleCode = true;

                int it = 0;

                // check if required i/p fields are present in the map.
                foreach (var field in dependentFields)
                {
                    if (globalMap.ContainsKey(dependentSampleCode + field))
                    {
                        inputFields[it++] = globalMap[dependentSampleCode + field];
                    }
                    else
                    {
                        data.Add(new AssertionData(true, false, "Sample code wasn't executed as the dependent field \"" + field + "\" wasn't passed."));
                        callSampleCode = false;
                    }
                }

                // sample code isn't run if the required i/p fields aren't provided.
                if (callSampleCode)
                {
                    var className = "Cybersource_rest_samples_dotnet." + sampleCodeName;
                    Type type = assembly.GetType(className);
                    object result = null;

                    if (type != null)
                    {
                        MethodInfo mInfo = type.GetMethod("Run");

                        if (mInfo != null)
                        {
                            ParameterInfo[] parameters = mInfo.GetParameters();
                            object classInstance = Activator.CreateInstance(type, null);

                            // run the method.
                            if (parameters.Length == 0)
                            {
                                result = mInfo.Invoke(classInstance, null);
                            }
                            else
                            {
                                result = mInfo.Invoke(classInstance, inputFields);
                            }

                            var resultJson = JsonConvert.SerializeObject(result);
                            var jsonResponse = JObject.Parse(resultJson);

                            // Convert the response to json, this is to parse it.
                            foreach (var field in storedFields)
                            {
                                var value = Find(jsonResponse, field);
                                if (value != null)
                                {
                                    globalMap.Add(uniqueName + field, value.ToString());
                                }
                            }

                            if (string.IsNullOrEmpty(road["Assertions"]["httpStatus"].ToString()))
                            {
                                // get http status from sample code
                            }

                            // check the assertions and build the test data.
                            foreach (var field in road["Assertions"]["requiredFields"])
                            {
                                var actualValue = Find(jsonResponse, field.ToString());
                                data.Add(new AssertionData(true, actualValue != null, field + " - is a required field, but not present in the response."));
                            }

                            // check the assertions and build the test data.
                            foreach (var expectedField in road["Assertions"]["expectedValues"])
                            {
                                var actualValue = Find(jsonResponse, expectedField["field"].ToString());
                                data.Add(new AssertionData(expectedField["value"], actualValue, "Actual value of \"" + expectedField["field"] + "\" field doesn't match Expected value in the response."));
                            }
                        }
                        else
                        {
                            data.Add(new AssertionData(true, false, "Run method wasn't found in the sample code."));
                        }
                    }
                    else
                    {
                        data.Add(new AssertionData(true, false, "Sample Code wasn't found / Class name in Sample code is wrong."));
                    }
                }
            }
            catch(Exception ex)
            {
                data.Add(new AssertionData(true, false, "Exception occured, due to which Sample Code wasn't run. Exception is - " + ex.Message));
            }

            // return the data which has the assertions.
            return data;
        }
    }
}
