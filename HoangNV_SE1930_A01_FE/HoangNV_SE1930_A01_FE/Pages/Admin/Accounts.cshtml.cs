using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HoangNV_SE1930_A01_FE.Pages.Admin;

[Authorize(Roles = "Admin")]
public class AccountsModel : PageModel
{
    public void OnGet()
    {
    }
}
