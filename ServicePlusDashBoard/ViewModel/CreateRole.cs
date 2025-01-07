using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace ServicePlusDashBoard.ViewModel
{
    public class CreateRole
    {
        public string Role { get; set; }
        public List<string> ReportPermissions { get; set; }
        public SelectList PermissionSelectList { get; set; }
    }
}
