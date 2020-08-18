using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using maddweb.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace maddweb.Controllers
{
    public class AdminController : Controller
    {
        private AppDbContext _dbContext;
        public AdminController(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        public IActionResult index()
        {
            //string userName = User.FindFirst(ClaimTypes.NameIdentifier).Value;
            //ViewData["name"] = userName;
            string sql = @"select * from Entry";
            string sql2 = @"select * from Entry where Temperature>=38";
            int all = DBUtl.GetTable(sql).Rows.Count;
            int feverOnly = DBUtl.GetTable(sql2).Rows.Count;
            int noFever = all - feverOnly;
            int[] data = new int[] { noFever, feverOnly };
            
            string[] value = new[] { "Don't have fever", "Had Fever" };
            ViewData["Data"] = data;
         
            
            ViewData["Labels"] = value;


            string entry6 = @"select * from Entry where DATEPART(HOUR,EntryTime)=6;";
            int am6 = DBUtl.GetTable(entry6).Rows.Count;

            string entry7 = @"select * from Entry where DATEPART(HOUR,EntryTime)=7;";
            int am7 = DBUtl.GetTable(entry7).Rows.Count;

            string entry8 = @"select * from Entry where DATEPART(HOUR,EntryTime)=8;";
            int am8= DBUtl.GetTable(entry8).Rows.Count;


            string entry9 = @"select * from Entry where DATEPART(HOUR,EntryTime)=9;";
            int am9 = DBUtl.GetTable(entry9).Rows.Count;

            int[] records = new[] { am6, am7, am8, am9 };
            string[] colors = new[] { "red", "blue", "orange", "green" };
            ViewData["Legend"] = "Number of students entered at each timing";
            ViewData["Colors"] = colors;
            ViewData["Time"] = records;


            return View();
        }
       public IActionResult indexDark()
        {
            return View();
        }
        public IActionResult ViewAllEntry()
        {
            //string userName = User.FindFirst(ClaimTypes.Name).Value;
            //ViewData["name"] = userName;
            string sql = "select * from Entry inner join MaddUser on MaddUser.UserID=Entry.UserID ";
            DataTable dt = DBUtl.GetTable(sql);
            return View(dt.Rows);
        }

        public IActionResult ViewAllEntryDark()
        {
            string sql = "select * from Entry inner join MaddUser on MaddUser.UserID=Entry.UserID ";
            DataTable dt = DBUtl.GetTable(sql);
            return View(dt.Rows);
         
        }

        public IActionResult DangerStudents() {

            DbSet<Entry> dbsEntry = _dbContext.Entry;

            var model = dbsEntry
                        .Where(e => e.Temperature >= 37.5)
                        .Include(e => e.User)
                        .ToList();
            return View(model);
        }

        public IActionResult DangerList() {

            DbSet<Entry> dbsEntry = _dbContext.Entry;

            var model = dbsEntry
                        .Where(e => e.Temperature >= 37.5 && e.EntryTime.Date >= DateTime.Now.AddDays(-14))
                        .Include(e => e.User)
                        .ToList();

            return View(model);
        }


        public IActionResult DangerStudentsDark()
        {

            DbSet<Entry> dbsEntry = _dbContext.Entry;

            var model = dbsEntry
                        .Where(e => e.Temperature >= 37.5)
                        .Include(e => e.User)
                        .ToList();

            return View(model);
        }

        public IActionResult DangerListDark()
        {

            DbSet<Entry> dbsEntry = _dbContext.Entry;

            var model = dbsEntry
                        .Where(e => e.Temperature >= 37.5 && e.EntryTime.Date >= DateTime.Now.AddDays(-14))
                        .Include(e => e.User)
                        .ToList();

            return View(model);
        }

        public IActionResult Forbidden()
        {
            return View();
        }

        public IActionResult ViewAllTracingInfo()
        {
            //string userName = User.FindFirst(ClaimTypes.Name).Value;
            //ViewData["name"] = userName;

            string sql = "select * from TracingInfo";
            DataTable dt = DBUtl.GetTable(sql);
            return View(dt.Rows);

        }




    }
}