using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using maddweb.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace maddweb.Controllers
{
    public class EntryController : Controller
    {
		[HttpGet]
		
		[Route("api/AllEntryRecords")]
		public IActionResult AllEntryInfo(string name)
		{
			List<Entry> dbList = DBUtl.GetList<Entry>($"Select * from Entry");
			return Json(dbList);
		}
		[HttpGet]
		
        [Route("api/entryrecords/{name}")]
        public IActionResult EntryInfo(string name)
        {
            List<Entry> dbList = DBUtl.GetList<Entry>($"Select * from Entry inner join MaddUser on MaddUser.UserID=Entry.UserID where UserName='{name}'");
            return Json(dbList);
        }

		[HttpPost]
		[Route("api/Entry/addentry")]
		public IActionResult addentry([FromBody] Entry usr)
		{

			if (usr == null)
			{
				return BadRequest();
			}

			//string dt = String.Format("{0:yyyy-MM-dd hh:mm:ss}", usr.EntryTime);

			string sqlInsert = @"INSERT INTO Entry(UserID, Temperature, Photo, EntryTime, Location)
                VALUES ({0}, {1}, '{2}', '{3}','{4}')";
			if (DBUtl.ExecSQL(sqlInsert, usr.UserID, usr.Temperature, usr.Photo, usr.EntryTime, usr.Location) == 1)
				return Ok();
			else
				return BadRequest(new { Message = DBUtl.DB_Message });
		}

		[HttpPost]
		[Route("api/Entry/addTracing")]
		public IActionResult addTracing([FromBody] TracingInfo record)
		{

			if (record == null)
			{
				return BadRequest();
			}
			string sqlInsert = @"INSERT INTO TracingInfo(location, nric, contact, status,entrytime)
                VALUES ('{0}', '{1}', {2}, '{3}','{4}')";
			if (DBUtl.ExecSQL(sqlInsert, record.location, record.nric, record.contact, record.status, record.entrytime) == 1)
				return Ok();
			else
				return BadRequest(new { Message = DBUtl.DB_Message });
		}
	}
}