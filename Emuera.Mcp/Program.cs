using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

// era 语法分析 MCP 服务器（stdio）。
// 关键：所有日志走 stderr，保持 stdout 纯净的 JSON-RPC 通道。
var builder = Host.CreateApplicationBuilder(args);

builder.Logging.AddConsole(o =>
{
	o.LogToStandardErrorThreshold = LogLevel.Trace;
});

builder.Services
	.AddMcpServer()
	.WithStdioServerTransport()
	.WithToolsFromAssembly();

await builder.Build().RunAsync();
