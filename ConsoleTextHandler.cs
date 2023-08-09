using System.Net;

namespace MasterResolver9000
{
    public class ConsoleTextHandler
    {
        public List<(string Type, string Value)> ReadArgs(string[] args)
        {

            if (HasDuplicateSpecialArgs(args))
            {
                throw new ArgumentException("'input', 'output', and 'dns' arguments can only be used once each.");
            }

            if (args.Length == 0)
            {
                throw new ArgumentException("No arguments provided.");
            }

            List<(string Type, string Value)> parsedArgs = new List<(string Type, string Value)>();



            for (int i = 0; i < args.Length; i++)
            {
                string arg = args[i];
                string lowerArg = arg.ToLower();

                if (arg.StartsWith("-"))
                {
                    string type = lowerArg;
                    if (i + 1 < args.Length && !args[i + 1].StartsWith("-"))
                    {
                        if (type == "-input" || type == "-output")
                        {
                            parsedArgs.Add((type, args[i + 1]));
                            i++;
                        }
                        else
                        {
                            parsedArgs.Add((type, args[i + 1]));
                            i++;
                        }
                    }
                    else
                    {
                        throw new ArgumentException($"Missing argument for '{arg}'.");
                    }
                }
                else if (IsDnsAddress(arg))
                {
                    parsedArgs.Add(("dns", arg));
                }
                else
                {
                    parsedArgs.Add(("domain", arg));
                }
            }

            return parsedArgs;
        }

        public List<string> GetDomainArgumentsFromArgs(List<(string Type, string Value)> parsedArgs)
        {
            List<string> domainArgs = new List<string>();

            foreach (var parsedArg in parsedArgs)
            {
                if (parsedArg.Type.Equals("domain", StringComparison.OrdinalIgnoreCase))
                {
                    domainArgs.Add(parsedArg.Value);
                }
            }

            return domainArgs;
        }

        private static bool HasDuplicateSpecialArgs(string[] args)
        {
            Dictionary<string, int> specialArgCounts = new Dictionary<string, int>();

            foreach (string arg in args)
            {
                string lowerArg = arg.ToLower();

                if (lowerArg == "-input" || lowerArg == "-output" || lowerArg == "dns")
                {
                    if (specialArgCounts.ContainsKey(lowerArg))
                    {
                        specialArgCounts[lowerArg]++;
                    }
                    else
                    {
                        specialArgCounts[lowerArg] = 1;
                    }

                    if (specialArgCounts[lowerArg] > 1)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private static bool IsDnsAddress(string text)
        {
            IPAddress address;
            return IPAddress.TryParse(text, out address);
        }
    }
}
