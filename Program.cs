using System.Runtime.CompilerServices;

namespace MasterResolver9000
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            ConsoleTextHandler consoleTextHandler = new ConsoleTextHandler();

            List<(string Type, string Value)> inputText = consoleTextHandler.ReadArgs(args);
            string inputfilePath = inputText.FirstOrDefault(item => item.Type.Equals("-input")).Value;
            string outputfilePath = inputText.FirstOrDefault(item => item.Type.Equals("-output")).Value;
            List<string> emailDomain = new List<string>();

            if (inputfilePath != null)
            {
                emailDomain = FileHandler.ReadFile(inputfilePath);
            }
            else
            {
                emailDomain = consoleTextHandler.GetDomainArgumentsFromArgs(inputText);
            }

            string defaultDnsServer = "8.8.8.8";
            string dnsServer = inputText.FirstOrDefault(item => item.Type.Equals("dns")).Value ?? defaultDnsServer;
            var dnsLite = new DNSParser.DnsLite();
            var dnsMxRecords = await dnsLite.GetMXRecordsAsync(emailDomain, dnsServer);
            List<string> result = new List<string>();

                foreach(var dnsMxRecord in dnsMxRecords)
                { 
                    result.Add($"{dnsMxRecord.InputMail} MX preference = {dnsMxRecord.Preference}, mail exchanger = {dnsMxRecord.MailServer} {dnsMxRecord.DnsServer}");
                }

            foreach (var line in result)
            {
                Console.WriteLine(line.ToString());
            }

            if (outputfilePath != null)
            {
                FileHandler.SaveListToFile(result, outputfilePath);
            }
        }
    }
}
