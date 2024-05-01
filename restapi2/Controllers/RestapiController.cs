using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Data;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using restapi2.Models;
using Microsoft.AspNetCore.Authorization;


namespace RESTAPI.Controllers
{
   
    [ApiController]
    public class RestAPIController : Controller
    {
        public static List<Timesheet> formData = new List<Timesheet>();
       
       
        [Route("api/getData")]
        [HttpGet]
        [ProducesResponseType(typeof(List<Timesheet>), 200)]
        public List<Timesheet> Get()
        {
            SqlConnection con = new SqlConnection(ConfigurationSettings.AppSettings["dbconn"].ToString());

            string sqlquery = "SELECT * FROM [dbo].[TimeSheet]";
            SqlCommand sqlcomm = new SqlCommand(sqlquery, con);
            con.Open();

            SqlDataReader reader = sqlcomm.ExecuteReader();
            while (reader.Read())
            {
                Timesheet timeSheet = new Timesheet
                {
                    TimeSheetID = Convert.ToInt32(reader["TimeSheetID"]),
                    employeeID = Convert.ToInt32(reader["employeeID"]),
                    Date = Convert.ToDateTime(reader["Date"]),
                    projectID = Convert.ToInt32(reader["projectID"]),
                    taskID = Convert.ToInt32(reader["taskID"]),
                    Description = reader["Description"] == DBNull.Value ? string.Empty : reader["Description"].ToString(),
                    WorkStatus = reader["WorkStatus"] == DBNull.Value ? Timesheet.Status.Unknown : (Timesheet.Status)Enum.Parse(typeof(Timesheet.Status), reader["WorkStatus"].ToString()),
                    StartTime = DateTime.Parse(reader["StartTime"].ToString()), // Assuming StartTime is of type TimeSpan in your TimeSheet class
                    EndTime = DateTime.Parse(reader["EndTime"].ToString())

                };
                if (reader["HoursOfWork"] != DBNull.Value && TimeSpan.TryParse(reader["HoursOfWork"].ToString(), out TimeSpan hoursOfWork))
                {
                    timeSheet.HoursOfWork = hoursOfWork;
                }
                else
                {
                    timeSheet.HoursOfWork = TimeSpan.Zero;
                }

                formData.Add(timeSheet);
            }

            con.Close();

            return formData;
        }

        [Route("api/AddData")]
        [HttpPost]
        [ProducesResponseType(typeof(List<Timesheet>), 200)]
        public IActionResult AddData([FromBody] Timesheet timeSheet)
        {
            TimeSpan hoursOfWork = timeSheet.EndTime - timeSheet.StartTime;
            timeSheet.HoursOfWork = hoursOfWork;


            SqlConnection con = new SqlConnection(ConfigurationSettings.AppSettings["dbconn"].ToString());

            string sqlquery = "insert into [dbo].[TimeSheet] (employeeID,Date,StartTime,EndTime,HoursOfWork,projectID,taskID,Description,WorkStatus) values(@employeeID,@Date,@StartTime,@EndTime,@HoursOfWork,@projectID,@taskID,@Description,@WorkStatus)";
            SqlCommand sqlcomm = new SqlCommand(sqlquery, con);
            con.Open();

            sqlcomm.Parameters.AddWithValue("@employeeID", timeSheet.employeeID);
            sqlcomm.Parameters.AddWithValue("@Date", timeSheet.Date);
            sqlcomm.Parameters.AddWithValue("@StartTime", timeSheet.StartTime);
            sqlcomm.Parameters.AddWithValue("@EndTime", timeSheet.EndTime);
            sqlcomm.Parameters.AddWithValue("@HoursOfWork", timeSheet.HoursOfWork);
            sqlcomm.Parameters.AddWithValue("@projectID", timeSheet.projectID);
            sqlcomm.Parameters.AddWithValue("@taskID", timeSheet.taskID);
            sqlcomm.Parameters.AddWithValue("@Description", timeSheet.Description);
            sqlcomm.Parameters.AddWithValue("@WorkStatus", timeSheet.WorkStatus.ToString());
            
           
            formData.Add(timeSheet);
            sqlcomm.ExecuteNonQuery();
            con.Close();

            return Ok("Data Inserted Successfully");
        }

        [Route("api/DeleteData/{id}")]
        [HttpDelete]
        public IActionResult Delete(int id)
        {
            SqlConnection con = new SqlConnection(ConfigurationSettings.AppSettings["dbconn"].ToString());

            string sqlquery = "DELETE FROM [dbo].[TimeSheet] WHERE TimeSheetID = @TimeSheetID";
            con.Open();

            SqlCommand sqlcomm = new SqlCommand(sqlquery, con);
            sqlcomm.Parameters.AddWithValue("@TimeSheetID", id);

            var timeSheet = formData.Where(s => s.employeeID == id).FirstOrDefault();
            formData.Remove(timeSheet);
            sqlcomm.ExecuteNonQuery();
            con.Close();

            return Ok("Data Deleted Successfully");
        }

        [Route("api/UpdateData/{id}")]
        [HttpPut]
        public IActionResult Update(int id,[FromBody] Timesheet timeSheet)
        {
            TimeSpan hoursOfWork = timeSheet.EndTime - timeSheet.StartTime;
            timeSheet.HoursOfWork = hoursOfWork;


            SqlConnection con = new SqlConnection(ConfigurationSettings.AppSettings["dbconn"].ToString());
            con.Open();
            string query = "UPDATE [dbo].[TimeSheet] SET  employeeID = @employeeID, Date = @Date, projectID = @projectID, taskID = @taskID, Description = @Description, WorkStatus = @WorkStatus, StartTime = @StartTime, EndTime = @EndTime WHERE TimeSheetID="+id;

            SqlCommand sqlCommand = new SqlCommand(query, con);

            sqlCommand.Parameters.AddWithValue("@employeeID", timeSheet.employeeID);
            sqlCommand.Parameters.AddWithValue("@Date", timeSheet.Date);
            sqlCommand.Parameters.AddWithValue("@projectID", timeSheet.projectID);
            sqlCommand.Parameters.AddWithValue("@taskID", timeSheet.taskID);
            sqlCommand.Parameters.AddWithValue("@Description", timeSheet.Description);
            sqlCommand.Parameters.AddWithValue("@WorkStatus", timeSheet.WorkStatus.ToString());
            sqlCommand.Parameters.AddWithValue("@HoursOfWork", timeSheet.HoursOfWork);
            sqlCommand.Parameters.AddWithValue("@StartTime", timeSheet.StartTime);
            sqlCommand.Parameters.AddWithValue("@EndTime", timeSheet.EndTime);
            sqlCommand.Parameters.AddWithValue("@TimeSheetID", id);

            sqlCommand.ExecuteNonQuery();
            con.Close();

            return Ok("Data Updated Successfully");  
        }
    }
}
