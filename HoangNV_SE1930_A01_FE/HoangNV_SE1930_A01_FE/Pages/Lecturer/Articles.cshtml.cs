using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HoangNV_SE1930_A01_FE.Pages.Lecturer;

[Authorize(Roles = "Lecturer")]
public class ArticlesModel : PageModel
{
    public void OnGet()
    {
    }
}
