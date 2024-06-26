using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using RemoteEduApp.Data;
using RemoteEduApp.Services;

namespace RemoteEduApp.Pages.Teacher
{
    [Authorize(Policy = "BelongToTeacher")]
    public class NewContentModel : PageModel
    {
        private readonly IFileUploadService fileUploadService;
        public string? FilePath;
        string _errorMessage = "";

        DataContextDapper _dapper;

        public string ErrorMessage { get => _errorMessage; set => _errorMessage = value; }

        public NewContentModel(IFileUploadService fileUploadService, IConfiguration config)
        {
            _dapper = new DataContextDapper(config);
            this.fileUploadService = fileUploadService;
        }

        public async Task OnPost(IFormFile file)
        {
            string? CourseId = Request.Query["id"];
            string filePath = "";

            if (!(file == null))
            {
                if (Request.Form["type"] == "Лекция")
                {
                    filePath = Path.Combine(@"attachments\Lectures", file.FileName);
                }
                else if (Request.Form["type"] == "ДЗ")
                {
                    filePath = Path.Combine(@"attachments\Homeworks", file.FileName);
                }

                if (Path.GetExtension(file.FileName) != ".pdf")
                {
                    ErrorMessage = "Недопустимый формат файла!";
                    return;
                }

                FilePath = await fileUploadService.UploadFileAsync(file, filePath);
            }

            string sql = "INSERT INTO [RemoteEduDB].[dbo].[Content] (Name, [Type], Attachment, [Description], DateOfAdding,  CourseId, TeacherId) " +
                "VALUES ('" + Request.Form["Name"] + "', '" + Request.Form["type"] + "', '/" + filePath + "', '" + Request.Form["description"]
                + "', GETDATE(), " + CourseId + ", " + User.FindFirst("Id").Value + ");";

            _dapper.ExecuteSql(sql);

            Response.Redirect("/Teacher/Course?course=" + CourseId);
        }
    }
}
