using Newtonsoft.Json.Linq;
using WorxLogisticsAPIs.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity.Validation;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Http;
using System.Web.Http.Cors;

namespace NotesAndAttachements.Controllers
{
    //[EnableCors(origins: "*", headers: "*", methods: "*")]
    public class NotesAndAttachementsController : ApiController
    {
        [Route("api/createNote")]
        [HttpPost]
        public Response createNote([FromBody] JObject data)
        {
            Response res = new Response();
            Response allnotes = new Response();
            string Subject = (data["Subject"] == null) ? string.Empty : data["Subject"].ToString();
            string EntityID = (data["EntityID"] == null) ? string.Empty : data["EntityID"].ToString();
            string OwnerId = (data["OwnerId"] == null) ? string.Empty : data["OwnerId"].ToString();
            string OwnerName = (data["OwnerName"] == null) ? string.Empty : data["OwnerName"].ToString();
            
            string Notes = (data["Notes"] == null) ? string.Empty : data["Notes"].ToString();
            
            string MimeType = (data["MimeType"] == null) ? string.Empty : data["MimeType"].ToString();
            string Base64File = (data["Base64File"] == null) ? string.Empty : data["Base64File"].ToString();
            string FileName = (data["FileName"] == null) ? string.Empty : data["FileName"].ToString();

            if(Subject==string.Empty ||
                EntityID == string.Empty ||
                OwnerId == string.Empty ||
                OwnerName == string.Empty 
                //|| Notes == string.Empty
                )
            {
                res.code = 1;
                res.message = "Invalid Paramters";
                res.success = false;
                return res;
            }

            if((FileName != string.Empty ||
                Base64File != string.Empty ||
                MimeType != string.Empty) && 
                (FileName == string.Empty ||
                Base64File == string.Empty ||
                MimeType == string.Empty))
            {
                res.code = 1;
                res.message = "Invalid Paramters";
                res.success = false;
                return res;
            }
            try
            {
                using (Worx_ELogisticsEntities naentities = new Worx_ELogisticsEntities())
                {
                    NotesAndAttachement na = new NotesAndAttachement();
                    na.Subject = Subject;
                    na.EntityID = EntityID;
                    na.OwnerId = OwnerId;
                    na.OwnerName = OwnerName;
                    na.Notes = Notes;
                    na.CratedOn = DateTime.Now;

                    if (FileName != string.Empty)
                    {
                        na.FileName = FileName;
                        na.MimeType = MimeType;
                        na.Base64File = Base64File;
                    }
                    naentities.NotesAndAttachements.Add(na);
                    naentities.SaveChanges();
                }
                JObject JOgetNotesParameters = new JObject();
                JOgetNotesParameters.Add("EntityID", EntityID);
                allnotes = getNotesByEntityID(JOgetNotesParameters);
            }
            catch (DbEntityValidationException eve)
            {
                Exception raise = eve;
                foreach (var validationErrors in eve.EntityValidationErrors)
                {
                    foreach (var validationError in validationErrors.ValidationErrors)
                    {
                        string message = string.Format("{0}:{1}",
                            validationErrors.Entry.Entity.ToString(),
                            validationError.ErrorMessage);
                        // raise a new exception nesting  
                        // the current instance as InnerException  
                        raise = new InvalidOperationException(message, raise);
                    }
                }
                throw raise;
            }
            catch (Exception ex)
            {
                res.code = -1;
                res.message = ex.ToString();
                res.success = false;
                return res;
            }
            
            return allnotes;
        }

        [Route("api/getNoteByNoteID")]
        [HttpPost]
        public Response getNoteByNoteID([FromBody] JObject data)
        {
            Response res = new Response();

            return res;
        }
        
        

        [HttpPost]
        [Route("api/getAttachementByNoteID")]
        public IHttpActionResult getAttachementByNoteID([FromBody] JObject data)
        {
            HttpResponseMessage result = null;
            IHttpActionResult response;
            int id = (data["NoteId"]==null)? 0 : int.Parse(data["NoteId"].ToString());
            if (id == 0)
            {
                return BadRequest();
            }
            try
            {
                using (Worx_ELogisticsEntities naentities = new Worx_ELogisticsEntities())
                {
                    var naentity = naentities.NotesAndAttachements.FirstOrDefault(e => e.NotesID == id);
                    if (naentity == null)
                    {
                       return BadRequest();
                    }
                    else
                    {
                        // sendo file to client
                        byte[] bytes = Convert.FromBase64String(naentity.Base64File.Split(',')[1]);


                        result = Request.CreateResponse(HttpStatusCode.OK);
                        result.Content = new ByteArrayContent(bytes);
                        result.Content.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("attachment");
                        result.Content.Headers.ContentDisposition.FileName = naentity.FileName;
                        result.Content.Headers.ContentType = new MediaTypeHeaderValue(naentity.MimeType);
                        response = ResponseMessage(result);
                        return response;

                    }




                   return BadRequest();

                }
            }
            catch (Exception ex)
            {
                return BadRequest();

            }
        }

        [Route("api/getNotesByEntityID")]
        [HttpPost]
        public Response getNotesByEntityID([FromBody] JObject data)
        {
            Response res = new Response();
            string EntityID = (data["EntityID"] == null) ? string.Empty : data["EntityID"].ToString();
            if (EntityID == string.Empty)
            {
                res.code = 1;
                res.message = "Invalid Paramters";
                res.success = false;
                return res;
            }
            try
            {
                using (Worx_ELogisticsEntities naentities = new Worx_ELogisticsEntities())
                {
                    var naentitieslist = naentities.NotesAndAttachements.Where(e => e.EntityID == EntityID).Select(c => new
                    {
                        c.Subject,
                        c.EntityID,
                        c.FileName,
                        c.MimeType,
                        c.Notes,
                        c.NotesID,
                        c.OwnerId,
                        c.OwnerName,
                        c.CratedOn
                    }).OrderByDescending(d => d.CratedOn).ToList();

                    if (naentitieslist.Count <= 0)
                    {
                        res.code = 2;
                        res.success = false;
                        res.message = "No Records Found";
                        return res;
                    }
                    else
                    {
                        res.success = true;
                        res.result = new JObject();
                        res.result.Add("NotesList", JArray.FromObject(naentitieslist));
                        
                    }
                }
            }
            catch (Exception ex)
            {
                res.code = -1;
                res.message = ex.ToString();
                res.success = false;
                return res;
            }
            return res;
        }
    }
}
