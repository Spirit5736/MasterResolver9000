namespace MasterResolver9000
{
    public static class FileHandler
    {

        public static List<string> ReadFile(string inputFilePath)
        {
            List<string> words = new List<string>();
            using (StreamReader reader = new StreamReader(inputFilePath))
            {
                while (!reader.EndOfStream)
                {
                    string line = reader.ReadLine();
                    string[] delimiters = new string[] { " ", "\t", "\n", "\r\n" };
                    string[] lineWords = line.Split(delimiters, StringSplitOptions.RemoveEmptyEntries);
                    words.AddRange(lineWords);
                }
            }
            return words;
        }

        public static void SaveListToFile(List<string> list, string outputfilePath)
        {
            using (StreamWriter writer = new StreamWriter(outputfilePath))
            {
                foreach (string item in list)
                {
                    writer.WriteLine(item);
                }
            }
        }

    }
}
