using HyacineCore.Server.GameServer.Server;
using HyacineCore.Server.Kcp;
using HyacineCore.Server.Util;

namespace HyacineCore.Server.Command.Command;

public class CommandArg
{
    public CommandArg(string raw, ICommandSender sender, Connection? con = null)
    {
        Raw = raw;
        Sender = sender;
        var args = raw.Split([' ', '\t', '\r', '\n'], StringSplitOptions.RemoveEmptyEntries);
        for (var i = 0; i < args.Length; i++)
        {
            var arg = args[i];
            if (string.IsNullOrEmpty(arg)) continue;

            // target: @10001
            if (arg[0] == '@' && arg.Length > 1)
            {
                CharacterArgs["@"] = arg[1..];
                Args.Add(arg);
                continue;
            }

            // target: @ 10001
            if (arg == "@" && i + 1 < args.Length && int.TryParse(args[i + 1], out _))
            {
                CharacterArgs["@"] = args[i + 1];
                Args.Add(arg);
                Args.Add(args[i + 1]);
                i++;
                continue;
            }

            if (TryParseCharacterArg(arg, out var key, out var value))
            {
                CharacterArgs[key] = value;
                Args.Add(arg);
                continue;
            }

            // short flag with separated value: l 80 / -l 80 / --l 80
            if (TryParseShortFlagKey(arg, out key) && i + 1 < args.Length &&
                int.TryParse(args[i + 1], out _))
            {
                CharacterArgs[key] = args[i + 1];
                Args.Add(arg);
                Args.Add(args[i + 1]);
                i++;
                continue;
            }

            BasicArgs.Add(arg);
            Args.Add(arg);
        }

        if (con != null) Target = con;

        CharacterArgs.TryGetValue("@", out var target);
        if (target == null) return;
        if (HyacineCoreListener.Connections.Values.ToList()
                .Find(item => (item as Connection)?.Player?.Uid.ToString() == target) is Connection connection)
            Target = connection;
    }

    public string Raw { get; }
    public List<string> Args { get; } = [];
    public List<string> BasicArgs { get; } = [];
    public Dictionary<string, string> CharacterArgs { get; } = [];
    public Connection? Target { get; set; }
    public ICommandSender Sender { get; }

    public int GetInt(int index)
    {
        if (BasicArgs.Count <= index) return 0;
        _ = int.TryParse(BasicArgs[index], out var res);
        return res;
    }

    public async ValueTask SendMsg(string msg)
    {
        await Sender.SendMsg(msg);
    }

    public override string ToString()
    {
        return $"BasicArg: {BasicArgs.ToArrayString()}. CharacterArg: {CharacterArgs.ToJsonString()}.";
    }

    private static bool TryParseCharacterArg(string arg, out string key, out string value)
    {
        key = "";
        value = "";

        if (!TryParseShortFlagKey(arg, out key))
            return false;

        // compact: l80 / -l80 / --l80
        var compactValue = arg[(arg.LastIndexOf(key, StringComparison.Ordinal) + key.Length)..];
        if (!string.IsNullOrEmpty(compactValue) && int.TryParse(compactValue, out _))
        {
            value = compactValue;
            return true;
        }

        // pair style: l=80 / l:80 / -l=80 / --l:80
        var separatorIndex = arg.IndexOfAny(['=', ':']);
        if (separatorIndex < 0 || separatorIndex >= arg.Length - 1)
            return false;

        var valuePart = arg[(separatorIndex + 1)..];
        if (!int.TryParse(valuePart, out _))
            return false;

        value = valuePart;
        return true;
    }

    private static bool TryParseShortFlagKey(string arg, out string key)
    {
        key = "";
        if (string.IsNullOrEmpty(arg))
            return false;

        var trimmed = arg.TrimStart('-', '/');
        if (trimmed.Length == 0 || !char.IsLetter(trimmed[0]))
            return false;

        if (trimmed.Length > 1)
        {
            var next = trimmed[1];
            if (!char.IsDigit(next) && next != '+' && next != '-' && next != '=' && next != ':')
                return false;
        }

        key = trimmed[..1];
        return true;
    }
}
