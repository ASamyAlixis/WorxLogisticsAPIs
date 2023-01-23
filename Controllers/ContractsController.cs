using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using WorxLogisticsAPIs.Models;
using System.Web.Http.Cors;
namespace WorxLogisticsAPIs.Controllers
{
    [EnableCors(origins: "*", headers: "*", methods: "*")]
    public class ContractsController : ApiController
    {
        [Route("api/processSalesContract")]
        [HttpPost]
        public async Task<Response> processSalesContract([FromBody] JObject data)
        {
            Response res = new Response();
            string op = (data["op"] != null) ? data["op"].ToString() : string.Empty;
            string entity = (data["entity"] != null) ? data["entity"].ToString() : string.Empty;
            string userid = (data["userid"] != null) ? data["userid"].ToString() : string.Empty;
            string ownerid = (data["ownerid"] != null) ? data["ownerid"].ToString() : string.Empty;
            string attributes = (data["attributes"] != null) ? data["attributes"].ToString() : string.Empty;


            if (op == string.Empty || op != "processSalesContract" || entity == string.Empty || userid==string.Empty || attributes == string.Empty)
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
                    
                    //List <Wrx_SalesContractMaster> importedsalescontracts = context.Wrx_SalesContractMaster.ToList<Wrx_SalesContractMaster>();

                    JArray attributesArray = JArray.Parse(attributes);
                    
                    
                    
                    bool failed = false;
                    //foreach (string importedsalescontract in Vals)
                    foreach (JObject rowcontract in attributesArray)
                    {

                        
                        failed = false;
                        Wrx_SalesContracts salescontract = new Wrx_SalesContracts();
                        //foreach ()
                        // {


                        string contractnumber = rowcontract["Contractnumber"].ToString();
                            Wrx_SalesContracts exisitngcontract = context.Wrx_SalesContracts.Where(crt => crt.Contractnumber == contractnumber).FirstOrDefault();
                            if (exisitngcontract != null)
                                salescontract = exisitngcontract;
                        string castname = rowcontract["CmpName"].ToString();
                            if (context.Wrx_Customers.Where(cust => cust.CustName == castname).FirstOrDefault() != null)
                                salescontract.Customerid = context.Wrx_Customers
                                                        .Where(cust => cust.CustName == castname).FirstOrDefault().id;
                            else
                                failed = true;
                        string itemnumber = rowcontract["Itemnumber"].ToString();
                            if (context.Wrx_Commodity.Where(comm => comm.Name == itemnumber).FirstOrDefault() != null)
                                salescontract.Commodityid = context.Wrx_Commodity
                                                            .Where(comm => comm.Name == itemnumber).FirstOrDefault().id;

                            else
                                failed = true;

                        string portdischarge = rowcontract["Portdischarge"].ToString();
                            if (context.Wrx_DischargePorts.Where(prt => prt.PortName == portdischarge).FirstOrDefault() != null)
                                salescontract.Portdischargeid = context.Wrx_DischargePorts
                                                             .Where(prt => prt.PortName == portdischarge).FirstOrDefault().id;
                            else
                                failed = true;

                        string originvariety = rowcontract["ORIGINVARIETY"].ToString();
                            if (context.Wrx_Origin.Where(org => org.Origin == originvariety).FirstOrDefault() != null)

                                salescontract.Originid = context.Wrx_Origin
                                                               .Where(org => org.Origin == originvariety).FirstOrDefault().id;

                            else
                                failed = true;

                        string grade = rowcontract["Grade"].ToString();
                            if (context.Wrx_Grade.Where(grd => grd.Grade == grade).FirstOrDefault() != null)
                                salescontract.Gradeid = context.Wrx_Grade
                                                                .Where(grd => grd.Grade == grade).FirstOrDefault().id;

                            else
                                failed = true;

                        string unit = rowcontract["Unit"].ToString();
                            if (context.Wrx_Units.Where(unt => unt.UnitName == unit).FirstOrDefault() != null)

                                salescontract.Unitid = context.Wrx_Units
                                                                .Where(unt => unt.UnitName == unit).FirstOrDefault().id;

                            else
                                failed = true;

                        string currency = rowcontract["Currency"].ToString();
                            if (context.Wrx_Currencies.Where(cur => cur.CurrencyName == currency).FirstOrDefault() != null)

                                salescontract.Currencyid = context.Wrx_Currencies
                                                                .Where(cur => cur.CurrencyName == currency).FirstOrDefault().id;

                            else
                                failed = true;

                            if (!failed)
                            {
                                salescontract.Amount = double.Parse(rowcontract["Amount"].ToString());
                                salescontract.Contractdate = DateTime.Parse(rowcontract["Contractdate"].ToString());
                                salescontract.Buyer = rowcontract["Buyer"].ToString();
                                salescontract.Contractnumber = rowcontract["Contractnumber"].ToString();
                                salescontract.Deliveryfromdate = DateTime.Parse(rowcontract["Deliveryfromdate"].ToString());
                                salescontract.Deliverytodate = DateTime.Parse(rowcontract["Deliverytodate"].ToString());
                                salescontract.Price = double.Parse(rowcontract["Price"].ToString());
                                salescontract.Payment = rowcontract["Payment"].ToString();
                                salescontract.Quantity = double.Parse(rowcontract["Quantity"].ToString());
                                salescontract.Termsofpayment = rowcontract["Termsofpayment"].ToString();
                                salescontract.Warehouse = rowcontract["Warehouse"].ToString();

                                var UID = int.Parse(userid);
                                var OUID = int.Parse(ownerid);

                                salescontract.modifiedby = context.Wrx_User.Where(usr => usr.id == UID).FirstOrDefault().id;
                                if (exisitngcontract == null)
                                    salescontract.createdby = context.Wrx_User.Where(usr => usr.id == UID).FirstOrDefault().id;
                                salescontract.modifiedon = DateTime.Now;
                                if (exisitngcontract == null)
                                    salescontract.createdon = DateTime.Now;
                                salescontract.Ordertype = rowcontract["Ordertype"].ToString();
                                salescontract.Deliveryterms = rowcontract["Deliveryterms"].ToString();
                                salescontract.Ownerid = context.Wrx_User.Where(usr => usr.id == OUID).FirstOrDefault().id;
                            }
                        if (exisitngcontract == null && !failed)
                        {
                            context.Wrx_SalesContracts.Add(salescontract);
                            await context.Database.ExecuteSqlCommandAsync("delete [Wrx_SalesContractMaster] where [Contractnumber] =" + "'" + contractnumber + "'");
                        }
                        else if (exisitngcontract != null)
                        {
                            await context.Database.ExecuteSqlCommandAsync("delete Wrx_SalesContractMaster where Contractnumber =" + "'" + contractnumber + "'");
                        }

                        //if(exisitngcontract != null && !failed)
                        //context.Wrx_SalesContractMaster.Remove(importedsalescontract);

                    }
                        await context.SaveChangesAsync();
                        res.success = true;
                        res.result = new JObject();
                       // res.result.Add("affectedrows", filedval.Count());

                   // }
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

        [Route("api/clearImportedData")]
        [HttpPost]
        public async Task<Response> clearImportedData([FromBody] JObject data)
        {
            Response res = new Response();
            string op = (data["op"] != null) ? data["op"].ToString() : string.Empty;
            string userid = (data["userid"] != null) ? data["userid"].ToString() : string.Empty;
            
            //string attributes = (data["attributes"] != null) ? data["attributes"].ToString() : string.Empty;

            if (op == string.Empty || op != "clearImportedData" || userid == string.Empty)
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
                    await context.Database.ExecuteSqlCommandAsync("TRUNCATE TABLE [Wrx_SalesContractMaster]");
                    
                    res.success = true;
                    
                }
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
    }
}
