using Markdig.Extensions.CustomContainers;
using Markdig.Renderers;
using Markdig.Renderers.Html;
using Markdig.Syntax;
using System.Text;

namespace PropertyBitPackDocs;

public class MermaidContainer : HtmlObjectRenderer<CustomContainer>
{

    protected override void Write(HtmlRenderer renderer, CustomContainer obj)
    {
        renderer.EnsureLine();

        var attrs = obj.TryGetAttributes()!;

        var summary = obj.Arguments ?? "source";


        var rawContent = obj[0] is CodeBlock codeBlock
            ? codeBlock.Lines.ToString()
            : string.Empty;


        renderer.Write("<div").WriteAttributes(obj).Write('>');
        {
            renderer.Write("<pre class=\"mermaid\">");
            renderer.WriteEscape(rawContent);
            renderer.WriteLine("</pre>");

            renderer.EnsureLine();

            renderer.Write("<details>");
            {
                renderer.Write("<summary>");
                renderer.Write(summary);
                renderer.WriteLine("</summary>");

                renderer.Write("<pre>");
                {
                    renderer.WriteEscape(rawContent);
                }
                renderer.WriteLine("</pre>");
            }
            renderer.WriteLine("</details>");

        }
        renderer.WriteLine("</div>");

    }
}
