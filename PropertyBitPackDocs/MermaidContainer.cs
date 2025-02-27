using Markdig.Extensions.CustomContainers;
using Markdig.Renderers;
using Markdig.Renderers.Html;
using Markdig.Syntax;
using System.Text;

namespace PropertyBitPackDocs;

public class MermaidContainer : HtmlObjectRenderer<CustomContainer>
{
    public static string GetRawText(Block block)
    {
        // Если это листовой блок, то собираем строки
        if (block is LeafBlock leafBlock)
        {
            // Если у блока нет строк, вернём пустую строку
            if (leafBlock.Lines.Lines == null)
            {
                return string.Empty;
            }

            // Превращаем коллекцию StringLine в обычный текст
            var lines = leafBlock.Lines.Lines;
            var lineStrings = new string[lines.Length];
            for (var i = 0; i < lines.Length; i++)
            {
                lineStrings[i] = lines[i].ToString();
            }
            return string.Join("\n", lineStrings);
        }
        // Если это контейнерный блок, обходим всех детей рекурсивно
        else if (block is ContainerBlock container)
        {
            var sb = new StringBuilder();
            foreach (var child in container)
            {
                var childRaw = GetRawText(child);
                if (!string.IsNullOrEmpty(childRaw))
                {
                    // Чтобы между блоками была хотя бы новая строка
                    if (sb.Length > 0)
                    {
                        sb.AppendLine();
                    }

                    sb.Append(childRaw);
                }
            }
            return sb.ToString();
        }
        // На всякий случай, если это какой-то неизвестный тип блока
        return string.Empty;
    }

    protected override void Write(HtmlRenderer renderer, CustomContainer obj)
    {
        renderer.EnsureLine();

        var attrs = obj.TryGetAttributes()!;

        var summary = obj.Arguments ?? "source";

        var rawContent = GetRawText(obj);

        renderer.Write("<div").WriteAttributes(obj).Write('>');
        {
            renderer.Write("<pre class=\"mermaid\">");
            renderer.Write(rawContent);
            renderer.WriteLine("</pre>");

            renderer.EnsureLine();

            renderer.Write("<details>");
            {
                renderer.Write("<summary>");
                renderer.Write(summary);
                renderer.WriteLine("</summary>");

                renderer.Write("<pre>");
                {
                    renderer.Write("<code class=\"language-mermaid\">");
                    {
                        renderer.Write(rawContent);
                    }
                    renderer.Write("</code>");
                }
                renderer.WriteLine("</pre>");
            }
            renderer.WriteLine("</details>");

        }
        renderer.WriteLine("</div>");

    }
}
