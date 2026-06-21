using illusion.Gateway;

namespace illusion.Gateway.CLI;

internal static class Program
{
    private static readonly IllusionGatewayService Service = new();

    private static int Main(string[] args)
    {
        if (args.Length == 0 || args[0] is "--help" or "-h")
        {
            PrintHelp();
            return 0;
        }

        try
        {
            return args[0] switch
            {
                "info" => PrintInfo(),
                "prove-direct" => ProveDirect(args),
                "generate-direct" => GenerateDirect(args),
                "upp-put" => UppPut(args),
                "upp-get" => UppGet(args),
                _ => Fail($"unknown command: {args[0]}")
            };
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"error: {ex.Message}");
            return 1;
        }
    }

    private static int PrintInfo()
    {
        var info = Service.GetInfo();
        Console.WriteLine($"System: {info.SystemName}");
        Console.WriteLine($"Log level: {info.LogLevel}");
        Console.WriteLine($"UPP encryption: {info.UppEncryptionEnabled}");
        Console.WriteLine($"UPP storage: {info.UppStorageDirectory}");
        return 0;
    }

    private static int ProveDirect(IReadOnlyList<string> args)
    {
        if (args.Count != 4)
            return Fail("usage: illusion.Gateway.CLI prove-direct <predicate> <object-id> <object-name>");

        var response = Service.ProveDirect(new ProofRequest(args[1], args[2], args[3]));
        Console.WriteLine(response.IsSuccess ? "PROVED" : "NOT PROVED");
        Console.WriteLine($"Trace: {response.TraceId}");
        foreach (var step in response.Steps)
            Console.WriteLine(step);
        if (!response.IsSuccess && response.ErrorMessage is not null)
            Console.WriteLine(response.ErrorMessage);
        return response.IsSuccess ? 0 : 2;
    }

    private static int GenerateDirect(IReadOnlyList<string> args)
    {
        if (args.Count != 4)
            return Fail("usage: illusion.Gateway.CLI generate-direct <predicate> <object-id> <object-name>");

        Console.Write(Service.GenerateDirect(new ProofRequest(args[1], args[2], args[3])).Code);
        return 0;
    }

    private static int UppPut(IReadOnlyList<string> args)
    {
        if (args.Count != 5)
            return Fail("usage: illusion.Gateway.CLI upp-put <namespace> <key> <value> <passphrase>");

        Service.PutPrivateRecord(args[1], args[2], args[3], args[4]);
        Console.WriteLine("STORED");
        return 0;
    }

    private static int UppGet(IReadOnlyList<string> args)
    {
        if (args.Count != 4)
            return Fail("usage: illusion.Gateway.CLI upp-get <namespace> <key> <passphrase>");

        Console.WriteLine(Service.GetPrivateRecord(args[1], args[2], args[3]));
        return 0;
    }

    private static int Fail(string message)
    {
        Console.Error.WriteLine(message);
        PrintHelp();
        return 1;
    }

    private static void PrintHelp()
    {
        Console.WriteLine("Illusionary-AI deterministic reasoning CLI");
        Console.WriteLine();
        Console.WriteLine("Commands:");
        Console.WriteLine("  info                                      Print configured system metadata");
        Console.WriteLine("  prove-direct <predicate> <id> <name>      Prove a proposition provided as an axiom");
        Console.WriteLine("  generate-direct <predicate> <id> <name>   Emit C# only after a successful proof");
        Console.WriteLine("  upp-put <ns> <key> <value> <passphrase>   Store encrypted local private data");
        Console.WriteLine("  upp-get <ns> <key> <passphrase>           Read encrypted local private data");
    }
}
