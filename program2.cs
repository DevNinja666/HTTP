using System.Net;
using System.Text;
using System.Text.Json;
using TodoViewer.Models;

HttpListener listener = new HttpListener();
listener.Prefixes.Add("http://127.0.0.1:8080/");
listener.Start();

Console.WriteLine("HTML Server running at http://127.0.0.1:8080/tasks");

HttpClient client = new HttpClient();

while (true)
{
    var ctx = listener.GetContext();
    var path = ctx.Request.Url.AbsolutePath;
    string html = "";

    if (path == "/tasks")
    {
        var json = await client.GetStringAsync("http://localhost:5000/api/todo");
        var todos = JsonSerializer.Deserialize<List<ToDoItem>>(json);

        html = "<h1>All Tasks</h1><ul>";
        foreach (var t in todos)
        {
            html += $"<li><a href='/tasks/{t.Id}'>{t.Title}</a> [{t.Tags}]</li>";
        }
        html += "</ul>";
    }
    else if (path.StartsWith("/tasks/"))
    {
        var id = path.Replace("/tasks/", "");
        var json = await client.GetStringAsync("http://localhost:5000/api/todo/" + id);
        var t = JsonSerializer.Deserialize<ToDoItem>(json);

        html = $"<h1>{t.Title}</h1>";
        html += $"<p>{t.Description}</p>";
        html += $"<b>Tags:</b> {t.Tags}";
    }

    byte[] buffer = Encoding.UTF8.GetBytes(html);
    ctx.Response.ContentType = "text/html";
    ctx.Response.OutputStream.Write(buffer);
    ctx.Response.Close();
}
