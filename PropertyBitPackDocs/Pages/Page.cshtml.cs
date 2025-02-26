using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace PropertyBitPackDocs;

public class Page : PageModel
{
    [FromRoute]
    public string Slug { get; set; }
}