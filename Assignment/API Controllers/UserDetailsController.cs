using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Assignment.Common_Classes;
using OfficeOpenXml;
using System.IO;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;


namespace Assignment.API_Controllers
{
    [RoutePrefix("api/UserDetails")]
    public class UserDetailsController : ApiController
    {
        private UserMgntEntities db = new UserMgntEntities();
        [HttpPost]
        [Authorize(Roles = "Admin")]
        [Route("searchusers")]
        public IHttpActionResult SearchUsers(UserSearchCriteria searchCriteria)
        {
            // Check if the required fields are present in the request body
            if (searchCriteria == null || string.IsNullOrWhiteSpace(searchCriteria.FieldName) || string.IsNullOrWhiteSpace(searchCriteria.FieldValue))
            {
                return BadRequest("Field Name and Field Value are required for filtering");
            }

            // Get IQueryable<User> to apply filters
            var query = db.Users.AsQueryable();

            // Apply filtering based on Field Name and Field Value
            switch (searchCriteria.FieldName.ToLower())
            {
                case "username":
                    query = query.Where(u => u.Username.Contains(searchCriteria.FieldValue));
                    break;
                case "age":
                    if (int.TryParse(searchCriteria.FieldValue, out int age))
                    {
                        query = query.Where(u => u.Age == age);
                    }
                    else
                    {
                        return BadRequest("Invalid age value");
                    }
                    break;
                case "hobbies":
                    query = query.Where(u => u.Hobbies.Contains(searchCriteria.FieldValue));
                    break;
                default:
                    return BadRequest("Invalid Field Name");
            }

            // Apply pagination and sorting
            int pageSize = searchCriteria.PageSize ?? 10; // Default page size is 10
            int pageNumber = searchCriteria.PageNumber ?? 1; // Default page number is 1

            // Calculate skip count based on pagination
            int skipCount = (pageNumber - 1) * pageSize;

            // Apply sorting
            if (!string.IsNullOrWhiteSpace(searchCriteria.SortField))
            {
                switch (searchCriteria.SortField.ToLower())
                {
                    case "username":
                        query = searchCriteria.SortOrder == "desc" ?
                                query.OrderByDescending(u => u.Username) :
                                query.OrderBy(u => u.Username);
                        break;
                    case "age":
                        query = searchCriteria.SortOrder == "desc" ?
                                query.OrderByDescending(u => u.Age) :
                                query.OrderBy(u => u.Age);
                        break;
                    case "hobbies":
                        query = searchCriteria.SortOrder == "desc" ?
                                query.OrderByDescending(u => u.Hobbies) :
                                query.OrderBy(u => u.Hobbies);
                        break;
                    default:
                        return BadRequest("Invalid Sort Field");
                }
            }

            // Apply pagination
            var paginatedUsers = query.Skip(skipCount).Take(pageSize).ToList();

            return Ok(paginatedUsers);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [Route("exportusers")]
        public IHttpActionResult ExportUsers(UserSearchCriteria searchCriteria, string exportFormat)
        {
            // Validate export format
            if (string.IsNullOrWhiteSpace(exportFormat) || (exportFormat.ToLower() != "pdf" && exportFormat.ToLower() != "excel"))
            {
                return BadRequest("Invalid export format. Supported formats: PDF, Excel.");
            }

            // Get filtered users based on search criteria (similar to SearchUsers action)
            var filteredUsers = GetFilteredUsers(searchCriteria);

            // Generate export file
            byte[] fileContents;
            string contentType;
            string fileName;

            if (exportFormat.ToLower() == "pdf")
            {
                // Generate PDF
                fileContents = GeneratePdf(filteredUsers);
                contentType = "application/pdf";
                fileName = $"UsersExport_{DateTime.Now.ToString("yyyyMMddHHmmss")}.pdf";
            }
            else
            {
                // Generate Excel
                fileContents = GenerateExcel(filteredUsers);
                contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                fileName = $"UsersExport_{DateTime.Now.ToString("yyyyMMddHHmmss")}.xlsx";
            }

            // Create response with export file
            var response = new System.Net.Http.HttpResponseMessage(System.Net.HttpStatusCode.OK);
            response.Content = new System.Net.Http.ByteArrayContent(fileContents);
            response.Content.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("attachment")
            {
                FileName = fileName
            };
            response.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(contentType);

            return ResponseMessage(response);
        }

        private IQueryable<User> GetFilteredUsers(UserSearchCriteria searchCriteria)
        {
            // Similar to the SearchUsers action, apply filters based on search criteria
            var query = db.Users.AsQueryable();

            // Apply filtering based on Field Name and Field Value
            // (similar switch case as in SearchUsers action)

            // ...

            return query;
        }

        private byte[] GeneratePdf(IQueryable<User> users)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                var writer = new PdfWriter(stream);
                var pdf = new PdfDocument(writer);
                var document = new Document(pdf);

                // Add header with current date
                document.Add(new Paragraph($"Export Date: {DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}"));

                // Add user data
                var table = new Table(new float[] { 1, 1, 1, 1, 1 }); // Specify column widths as a ratio (adjust as needed)

                // Add header cells
                table.AddHeaderCell("Username");
                table.AddHeaderCell("Password");
                table.AddHeaderCell("IsAdmin");
                table.AddHeaderCell("Age");
                table.AddHeaderCell("Hobbies");

                foreach (var user in users)
                {
                    // Add data cells
                    table.AddCell(user.Username);
                    table.AddCell(user.Password);
                  
                    table.AddCell(user.Age.ToString());
                    table.AddCell(user.Hobbies);
                }

                document.Add(table);


                // Add footer with page number
                document.Add(new Paragraph($"Page {pdf.GetNumberOfPages()}"));

                document.Close();
                return stream.ToArray();
            }
        }


        private byte[] GenerateExcel(IQueryable<User> users)
        {
            using (var package = new ExcelPackage())
            {
                // Create worksheet
                var worksheet = package.Workbook.Worksheets.Add("Users");

                // Add header row
                var headers = new string[] { "Username", "Password", "IsAdmin", "Age", "Hobbies" };
                for (int i = 0; i < headers.Length; i++)
                {
                    worksheet.Cells[1, i + 1].Value = headers[i];
                }

                // Add user data
                int row = 2;
                foreach (var user in users)
                {
                    worksheet.Cells[row, 1].Value = user.Username;
                    worksheet.Cells[row, 2].Value = user.Password;                   
                    worksheet.Cells[row, 4].Value = user.Age;
                    worksheet.Cells[row, 5].Value = user.Hobbies;
                    row++;
                }

                // Add header with current date
                worksheet.Cells["A1"].AddComment($"Export Date: {DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}", "Author");

                // Add footer with page number
                worksheet.Cells[$"A{row + 1}"].Value = $"Page {worksheet.Dimension.End.Row}";

                return package.GetAsByteArray();
            }




        }
    }

}