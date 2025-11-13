// Program.cs - Modbus TCP Simulator console application
using System.Net;
using System.Net.Sockets;

namespace Dashboard.Simulator;

class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("===========================================");
        Console.WriteLine("Dashboard Modbus TCP Simulator");
        Console.WriteLine("===========================================");
        Console.WriteLine();

        var port = args.Length > 0 && int.TryParse(args[0], out var p) ? p : 5020;

        Console.WriteLine($"Starting Modbus TCP server on port {port}...");
        Console.WriteLine("Press Ctrl+C to stop");
        Console.WriteLine();

        Console.WriteLine("⚠️  Modbus TCP slave not yet implemented");
        Console.WriteLine("This is a placeholder. Actual implementation will:");
        Console.WriteLine("  - Listen on TCP port 5020");
        Console.WriteLine("  - Respond to Modbus read/write requests");
        Console.WriteLine("  - Simulate process variables based on docs/modbus-map.json");
        Console.WriteLine();

        var tcs = new TaskCompletionSource<bool>();
        Console.CancelKeyPress += (s, e) =>
        {
            Console.WriteLine("\nShutting down...");
            e.Cancel = true;
            tcs.SetResult(true);
        };

        await tcs.Task;
    }
}


