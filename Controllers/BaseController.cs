using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Cors;
using WorxLogisticsAPIs.Models;

namespace WorxLogisticsAPIs.Controllers
{
    [EnableCors(origins: "*", headers: "*", methods: "*")]
    public class BaseController : ApiController
    {
        [Route("api/query")]
        [HttpPost]
        public async Task<Response> query([FromBody] JObject data)
        {
            Response res = new Response();
            string op = (data["op"] != null) ? data["op"].ToString() : string.Empty;
            string query = (data["query"] != null) ? data["query"].ToString() : string.Empty;
            
            if(op == string.Empty || op != "query" || query == string.Empty)
            {
                res.success = false;
                res.message = "Invalid Parameters";
                res.code = -1;
                return res;
            }

            if (query.IndexOf(';') < 0)
            {
                res.success = false;
                res.message = "Parse Error";
                res.code = -1;
                return res;
            }

            string execquery = query.Split(';')[0];

            if(execquery.Contains("insert") || execquery.Contains("drop") || execquery.Contains("create") || execquery.Contains("alter") || execquery.Contains("'='")){
                res.success = false;
                res.message = "Operation not Allowed";
                res.code = -1;
                return res;
            }
            try
            {
                ICollection<object> queryResult = new List<object>();
                
                var dataList = new List<string[]>();
                //var dbcontext = new AuditLogEntities();
                var dbETGPortal = new Worx_ELogisticsEntities();
                using (var command = dbETGPortal.Database.Connection.CreateCommand())
                {
                    command.CommandText = execquery;
                    command.CommandType = CommandType.Text;

                    await dbETGPortal.Database.Connection.OpenAsync();

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        var tempCol = new string[reader.FieldCount];
                        for (var i = 0; i < reader.FieldCount; i++)
                        {
                            tempCol[i] = reader.GetName(i);

                        }
                        dataList.Add(tempCol);

                        JArray dataset = new JArray();
                        while (await reader.ReadAsync())
                        {
                            JObject datarow = new JObject();
                            var tempRow = new string[reader.FieldCount];
                            for (var i = 0; i < reader.FieldCount; i++)
                            {
                                datarow.Add(tempCol[i], Convert.ToString(reader.GetValue(i)));
                                //datarow.Add(Convert.ToString(reader.GetValue(i)));
                                tempRow[i] = Convert.ToString(reader.GetValue(i));
                            }
                            dataset.Add(datarow);
                            //dataset.Add(tempRow[0]);
                            dataList.Add(tempRow);

                        }
                        res.success = true;
                        res.result = new JObject();
                        res.result.Add("cols", JArray.FromObject(dataList));
                        //res.result.Add(JArray.FromObject(dataList));
                        res.result.Add("queryresult", dataset);
                    }
                    
                }
                

                //return StatusCode(200, usersWithRoles); // Get all users   
            }
            catch (Exception e)
            {
                //return StatusCode(500, e);
                res.success = false;
                res.message = e.ToString();
                res.code = -1;
                
            }
            
            return res;
        }


        [Route("api/create")]
        [HttpPost]
        public async Task<Response> create([FromBody] JObject data)
        {
            Response res = new Response();
            string op = (data["op"] != null) ? data["op"].ToString() : string.Empty;
            string entity = (data["entity"] != null) ? data["entity"].ToString() : string.Empty;
            string attributes = (data["attributes"] != null) ? data["attributes"].ToString() : string.Empty;

            if (op == string.Empty || op != "create" || entity == string.Empty || attributes == string.Empty)
            {
                res.success = false;
                res.message = "Invalid Parameters";
                res.code = -1;
                return res;
            }

            
            try
            {
                
                using (var context = new Worx_ELogisticsEntities())
                {
                    JObject attributesobject = JObject.Parse(attributes);

                    Dictionary<string, string> dictObj = attributesobject.ToObject<Dictionary<string, string>>();
                    
                    string fields = String.Join(",", dictObj.Keys.ToArray()); 
                    string values = String.Join("','", dictObj.Select(x => x.Value));

                    string querystring = "insert into " + entity + "(" + fields + ") values(" + "'" + values + "'" + ")";
                    int noOfRowInserted = context.Database.ExecuteSqlCommand(querystring);
                        res.success = true;
                        res.result = new JObject();
                        res.result.Add("affectedrows", noOfRowInserted);
                    

                }


                //return StatusCode(200, usersWithRoles); // Get all users   
            }
            catch (Exception e)
            {
                //return StatusCode(500, e);
                res.success = false;
                res.message = e.ToString();
                res.code = -1;

            }

            return res;
        }

        [Route("api/update")]
        [HttpPost]
        public async Task<Response> update([FromBody] JObject data)
        {
            Response res = new Response();
            string op = (data["op"] != null) ? data["op"].ToString() : string.Empty;
            string entity = (data["entity"] != null) ? data["entity"].ToString() : string.Empty;
            string entityid = (data["entityid"] != null) ? data["entityid"].ToString() : string.Empty;
            string attributes = (data["attributes"] != null) ? data["attributes"].ToString() : string.Empty;

            if (op == string.Empty || op != "update" || entity == string.Empty || entityid== string.Empty || attributes == string.Empty)
            {
                res.success = false;
                res.message = "Invalid Parameters";
                res.code = -1;
                return res;
            }


            try
            {

                using (var context = new Worx_ELogisticsEntities())
                {
                    JObject attributesobject = JObject.Parse(attributes);
                    Dictionary<string, string> dictObj = attributesobject.ToObject<Dictionary<string, string>>();
                    JObject entityidobject = JObject.Parse(entityid);
                    Dictionary<string, string> dictEntityIdObj = entityidobject.ToObject<Dictionary<string, string>>();

                    if (dictEntityIdObj.Keys.ToArray().Length > 2)
                    {
                        res.success = false;
                        res.message = "Invalid Parameters";
                        res.code = -1;
                        return res;
                    }
                    string [] fieldsarray = dictObj.Keys.ToArray();
                    for(int i=0; i < fieldsarray.Length; i++)
                    {
                        fieldsarray[i] = fieldsarray[i] + "='" + dictObj[fieldsarray[i]] + "'";
                    }
                    string fields = String.Join(",", fieldsarray);

                    //string values = String.Join(", ", dictObj.Select(x => x.Value));

                    string [] whereentityidarray = dictEntityIdObj.Keys.ToArray();
                    string whereentityid = "";
                    foreach(string entityidkey in whereentityidarray)
                    {
                        whereentityid = entityidkey + "='" + dictEntityIdObj[entityidkey] + "'";
                    }
                    string querystring = "update " + entity + " set " + fields + " where " + whereentityid;
                    int noOfRowInserted = context.Database.ExecuteSqlCommand(querystring);
                    res.success = true;
                    res.result = new JObject();
                    res.result.Add("affectedrows", noOfRowInserted);


                }


                //return StatusCode(200, usersWithRoles); // Get all users   
            }
            catch (Exception e)
            {
                //return StatusCode(500, e);
                res.success = false;
                res.message = e.ToString();
                res.code = -1;

            }

            return res;
        }
        [Route("api/delete")]
        [HttpPost]
        public Task<Response> Delete([FromBody] JObject data)
        {
            Response res = new Response();
            string op = (data["op"] != null) ? data["op"].ToString() : string.Empty;
            string entity = (data["entity"] != null) ? data["entity"].ToString() : string.Empty;
            string entityid = (data["entityid"] != null) ? data["entityid"].ToString() : string.Empty;
            //string attributes = (data["attributes"] != null) ? data["attributes"].ToString() : string.Empty;

            if (op == string.Empty || op != "delete" || entity == string.Empty || entityid == string.Empty)
            {
                res.success = false;
                res.message = "Invalid Parameters";
                res.code = -1;
                return Task.FromResult(res);
            }
            try
            {

                using (var context = new Worx_ELogisticsEntities())
                {
                    JObject entityidobject = JObject.Parse(entityid);
                    Dictionary<string, string> dictEntityIdObj = entityidobject.ToObject<Dictionary<string, string>>();

                    if (dictEntityIdObj.Keys.ToArray().Length > 2)
                    {
                        res.success = false;
                        res.message = "Invalid Parameters";
                        res.code = -1;
                        return Task.FromResult(res);
                    }
                    string[] whereentityidarray = dictEntityIdObj.Keys.ToArray();
                    string whereentityid = "";
                    foreach (string entityidkey in whereentityidarray)
                    {
                      whereentityid = entityidkey + "='" + dictEntityIdObj[entityidkey] + "'";
                    }
                    //for (int j = 0; j < whereentityidarray.Length; j++)
                    //{
                      //  whereentityidarray[j] = whereentityidarray[j] + "='" + dictEntityIdObj[whereentityidarray[j]] + "'";
                    //}
                    //whereentityid = string.Join(",", whereentityidarray);
                    string querystring = "delete from " + entity + " where " + whereentityid;
                    int noOfRowInserted = context.Database.ExecuteSqlCommand(querystring);
                    res.success = true;
                    res.result = new JObject();
                    res.result.Add("affectedrows", noOfRowInserted);
                }   
            }
            catch (Exception e)
            {
                //return StatusCode(500, e);
                res.success = false;
                res.message = e.ToString();
                res.code = -1;

            }

            return Task.FromResult(res);   
        }
        [Route("api/DeleteTow")]
        [HttpPost]
        public Task<Response> DeleteTow([FromBody] JObject data)
        {
            Response res = new Response();
            string op = (data["op"] != null) ? data["op"].ToString() : string.Empty;
            string entity = (data["entity"] != null) ? data["entity"].ToString() : string.Empty;
            string entityid = (data["entityid"] != null) ? data["entityid"].ToString() : string.Empty;
            //string attributes = (data["attributes"] != null) ? data["attributes"].ToString() : string.Empty;

            if (op == string.Empty || op != "DeleteTow" || entity == string.Empty || entityid == string.Empty)
            {
                res.success = false;
                res.message = "Invalid Parameters";
                res.code = -1;
                return Task.FromResult(res);
            }
            try
            {

                using (var context = new Worx_ELogisticsEntities())
                {
                    JObject entityidobject = JObject.Parse(entityid);
                    Dictionary<string, string> dictEntityIdObj = entityidobject.ToObject<Dictionary<string, string>>();

                    if (dictEntityIdObj.Keys.ToArray().Length > 2)
                    {
                        res.success = false;
                        res.message = "Invalid Parameters";
                        res.code = -1;
                        return Task.FromResult(res);
                    }
                    string[] whereentityidarray = dictEntityIdObj.Keys.ToArray();
                    string whereentityid = "";
                    //foreach (string entityidkey in whereentityidarray)
                    //{
                    //  whereentityid = entityidkey + "='" + dictEntityIdObj[entityidkey] + "'";
                    //}
                    for (int j = 0; j < whereentityidarray.Length; j++)
                    {
                        whereentityidarray[j] = whereentityidarray[j] + "='" + dictEntityIdObj[whereentityidarray[j]] + "'";
                    }
                    whereentityid = string.Join(" and ", whereentityidarray);
                    string querystring = "delete from " + entity + " where " + whereentityid;
                    int noOfRowInserted = context.Database.ExecuteSqlCommand(querystring);
                    res.success = true;
                    res.result = new JObject();
                    res.result.Add("affectedrows", noOfRowInserted);
                }
            }
            catch (Exception e)
            {
                //return StatusCode(500, e);
                res.success = false;
                res.message = e.ToString();
                res.code = -1;

            }

            return Task.FromResult(res);
        }
        [Route("api/createRelations")]
        [HttpPost]
        public async Task<Response> createRelations([FromBody] JObject data)
        {
            Response res = new Response();
            string op = (data["op"] != null) ? data["op"].ToString() : string.Empty;
            string FromType = (data["FromType"] != null) ? data["FromType"].ToString() : string.Empty;
            string ToType = (data["ToType"] != null) ? data["ToType"].ToString() : string.Empty;
            string attributes = (data["attributes"] != null) ? data["attributes"].ToString() : string.Empty;

            if (op == string.Empty || op != "createRelations" || attributes == string.Empty)
            {
                res.success = false;
                res.message = "Invalid Parameters";
                res.code = -1;
                return res;
            }
            try
            {
                JArray keyvalues = JArray.Parse(attributes);
                using (var context = new Worx_ELogisticsEntities())
                {
                    context.Wrx_ManyMany.RemoveRange(context.Wrx_ManyMany.Where(x => x.FromType == FromType && x.ToType == ToType));
                    //context.SaveChanges();
                    List<Wrx_ManyMany> manytomanylist = new List<Wrx_ManyMany>();
                    foreach (JObject item in keyvalues)
                    {
                        Wrx_ManyMany wrx_manymany = new Wrx_ManyMany
                        {
                            //FromID = int.Parse(item["fromid"].ToString()),
                            FromID = item["FromId"].ToString(),
                            FromType = FromType,
                            //ToID = int.Parse(item["toid"].ToString()),
                            ToID = item["ToId"].ToString(),
                            ToType = ToType
                        };
                        manytomanylist.Add(wrx_manymany);

                    }
                    context.Wrx_ManyMany.AddRange(manytomanylist);
                    bool saveFailed;
                    do
                    {
                        saveFailed = false;
                        try
                        {
                            context.SaveChanges();
                        }
                        catch (DbUpdateConcurrencyException ex)
                        {
                            saveFailed = true;

                            // Update original values from the database
                            var entry = ex.Entries.Single();
                            entry.OriginalValues.SetValues(entry.GetDatabaseValues());
                        }

                    } while (saveFailed);
                    res.result = new JObject();
                    res.result.Add("RowsAffected", manytomanylist.Count);
                }
            }
            catch (Exception e)
            {
                //return StatusCode(500, e);
                res.success = false;
                res.message = e.ToString();
                res.code = -1;

            }
            res.success = true;
            return res;
        }

    }
}

