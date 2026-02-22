using System.Reflection;

namespace HyacineCore.Server.GameServer.Server.Packet;

public static class HandlerManager
{
    public static Dictionary<int, Handler> handlers = [];

    public static void Init()
    {
        var classes = Assembly.GetExecutingAssembly().GetTypes(); // Get all classes in the assembly
        foreach (var cls in classes)
        {
            var attribute = (Opcode?)Attribute.GetCustomAttribute(cls, typeof(Opcode));
            if (attribute == null) continue;
            if (!typeof(Handler).IsAssignableFrom(cls) || cls.IsAbstract) continue;

            var instance = (Handler)Activator.CreateInstance(cls)!;
            if (!handlers.TryAdd(attribute.CmdId, instance))
                Console.WriteLine(
                    $"[HandlerManager] Duplicate opcode {attribute.CmdId}: {handlers[attribute.CmdId].GetType().Name} vs {cls.Name}. Skipped {cls.Name}.");
        }
    }

    public static Handler? GetHandler(int cmdId)
    {
        try
        {
            return handlers[cmdId];
        }
        catch
        {
            return null;
        }
    }
}
