using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace PropertyBitPackDocs.Pages.ToDevelopers;

public class Page : PageModel
{
    [FromRoute]
    public string Slug { get; set; }
}
