using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace GeneticSearch
{
    class Program
    {
        struct Protein
        {
            public string name; // название белка
            public string organism; //название организма
            public string amino_acids; // аминокислоты
        }

        struct Command
        {
            public string name;
            public string parameter1;
            public string parameter2;
        }

        static List<Command> ReadCommands(string filename)
        {
            StreamReader reader = new StreamReader(filename);
            List<Command> commands = new List<Command>();

            Command command;
            command.name = String.Empty;
            command.parameter1 = String.Empty;
            command.parameter2 = String.Empty;

            while (!reader.EndOfStream)
            {
                string line = reader.ReadLine();
                string[] parts = line.Split('\t');

                if (parts.Length == 2)
                {
                    command.name = parts[0];
                    command.parameter1 = parts[1];
                    command.parameter2 = String.Empty;
                }
                else
                {
                    command.name = parts[0];
                    command.parameter1 = parts[1];
                    command.parameter2 = parts[2];
                }
                commands.Add(command);
            }
            reader.Close();
            return commands;
        }

        static List<Protein> ReadData(string filename)
        {
            StreamReader reader = new StreamReader(filename);

            List<Protein> data = new List<Protein>();

            while (!reader.EndOfStream)
            {
                string line = reader.ReadLine();
                string[] parts = line.Split('\t');
                Protein protein;
                protein.name = parts[0];
                protein.organism = parts[1];
                protein.amino_acids = Decoding(parts[2]);
                data.Add(protein);
            }
            reader.Close();
            return data;
        }
        static string Encoding(string amino_acids)
        {
            string encoded = String.Empty;
            for (int i = 0; i < amino_acids.Length; i++)
            {
                char ch = amino_acids[i];
                int count = 1;
                while (i < amino_acids.Length - 1 && amino_acids[i + 1] == ch)
                {
                    count++;
                    i++;
                }
                if (count > 2) encoded = encoded + count + ch;
                if (count == 1) encoded = encoded + ch;
                if (count == 2) encoded = encoded + ch + ch;
            }
            return encoded;
        }

        static string Decoding(string amino_acids)
        {
            string decoded = String.Empty;
            for (int i = 0; i < amino_acids.Length; i++)
            {
                char ch = amino_acids[i];
                if (char.IsDigit(ch))
                {
                    char letter = amino_acids[i + 1];
                    int count = ch - '0';
                    for (int j = 1; j < count; j++)
                        decoded = decoded + letter;
                }
                else decoded = decoded + ch;
            }
            return decoded;
        }

        static void PrintData(List<Protein> data)
        {
            for (int i = 0; i < data.Count; i++)
            {
                Console.WriteLine("Protein " + (i + 1));
                Console.WriteLine(data[i].name);
                Console.WriteLine(data[i].organism);
                Console.WriteLine(data[i].amino_acids);
                Console.WriteLine("========================");
            }
        }

        static void PrintCommands(List<Command> commands)
        {
            for (int i = 0; i < commands.Count; i++)
            {
                Console.WriteLine("Command " + (i + 1));
                Console.WriteLine(commands[i].name);
                Console.WriteLine(commands[i].parameter1);
                Console.WriteLine(commands[i].parameter2);
                Console.WriteLine("========================");
            }
        }

        static void HandleSearch(List<Protein> proteins, string searchSequence, StreamWriter writer)
        {
            string decodedSearch = Decoding(searchSequence);
            bool found = false;

            writer.WriteLine("organism\t\t\t\tprotein");

            for (int i = 0; i < proteins.Count; i++)
            {
                if (proteins[i].amino_acids.Contains(decodedSearch))
                {
                    writer.WriteLine(proteins[i].organism + "\t\t" + proteins[i].name);
                    found = true;
                }
            }

            if (!found)
            {
                writer.WriteLine("NOT FOUND");
            }
        }

        static void HandleDiff(List<Protein> proteins, string protein1Name, string protein2Name, StreamWriter writer)
        {
            Protein? p1 = null;
            Protein? p2 = null;

            for (int i = 0; i < proteins.Count; i++)
            {
                if (proteins[i].name == protein1Name)
                    p1 = proteins[i];
                if (proteins[i].name == protein2Name)
                    p2 = proteins[i];
            }

            writer.WriteLine("amino-acids difference:");

            if (p1 == null || p2 == null)
            {
                string missing = "";
                if (p1 == null) missing += protein1Name;
                if (p2 == null)
                {
                    if (missing != "") missing += ", ";
                    missing += protein2Name;
                }
                writer.WriteLine("MISSING: " + missing);
                return;
            }

            string seq1 = p1.Value.amino_acids;
            string seq2 = p2.Value.amino_acids;
            int maxLength = Math.Max(seq1.Length, seq2.Length);
            int differences = 0;

            for (int i = 0; i < maxLength; i++)
            {
                char c1 = i < seq1.Length ? seq1[i] : '\0';
                char c2 = i < seq2.Length ? seq2[i] : '\0';
                if (c1 != c2)
                    differences++;
            }

            writer.WriteLine(differences.ToString());
        }

        static void HandleMode(List<Protein> proteins, string proteinName, StreamWriter writer)
        {
            Protein? foundProtein = null;

            for (int i = 0; i < proteins.Count; i++)
            {
                if (proteins[i].name == proteinName)
                {
                    foundProtein = proteins[i];
                    break;
                }
            }

            writer.WriteLine("amino-acid occurs:");

            if (foundProtein == null)
            {
                writer.WriteLine("MISSING: " + proteinName);
                return;
            }

            string sequence = foundProtein.Value.amino_acids;
            Dictionary<char, int> frequency = new Dictionary<char, int>();

            for (int i = 0; i < sequence.Length; i++)
            {
                char c = sequence[i];
                if (frequency.ContainsKey(c))
                    frequency[c]++;
                else
                    frequency[c] = 1;
            }

            char mostFrequent = '\0';
            int maxCount = 0;

            List<char> sortedChars = new List<char>(frequency.Keys);
            sortedChars.Sort();

            for (int i = 0; i < sortedChars.Count; i++)
            {
                if (frequency[sortedChars[i]] > maxCount)
                {
                    maxCount = frequency[sortedChars[i]];
                    mostFrequent = sortedChars[i];
                }
            }

            writer.WriteLine(mostFrequent + "          " + maxCount);
        }

        static void CommandHandler(List<Protein> proteins, List<Command> commands, string outputFilename, string authorName)
        {
            using (StreamWriter writer = new StreamWriter(outputFilename))
            {
                writer.WriteLine(authorName);
                writer.WriteLine("Genetic Searching");
                writer.WriteLine("--------------------------------------------------------------------------");

                int commandNumber = 1;
                for (int i = 0; i < commands.Count; i++)
                {
                    Command cmd = commands[i];
                    string number = commandNumber.ToString("D3");
                    string param1 = cmd.name == "search" ? Decoding(cmd.parameter1) : cmd.parameter1;
                    string commandLine = number + "   " + cmd.name + "   " + param1;
                    if (!string.IsNullOrEmpty(cmd.parameter2))
                        commandLine += "   " + cmd.parameter2;

                    if (cmd.name == "diff") commandLine += " ";
                    if (cmd.name == "mode") commandLine += "  ";

                    writer.WriteLine(commandLine);

                    if (cmd.name == "search")
                    {
                        HandleSearch(proteins, cmd.parameter1, writer);
                    }
                    else if (cmd.name == "diff")
                    {
                        HandleDiff(proteins, cmd.parameter1, cmd.parameter2, writer);
                    }
                    else if (cmd.name == "mode")
                    {
                        HandleMode(proteins, cmd.parameter1, writer);
                    }

                    writer.WriteLine("--------------------------------------------------------------------------");
                    commandNumber++;
                }
            }
        }

        static void PrintStartupInfo(string sequencesFile, string commandsFile, string outputFile, int proteinsCount, int commandsCount)
        {
            Console.WriteLine("=== GENETIC SEARCH ===");
            Console.WriteLine($"Input sequences: {sequencesFile}");
            Console.WriteLine($"Input commands: {commandsFile}");
            Console.WriteLine($"Output file: {outputFile}");
            Console.WriteLine();
            Console.WriteLine($"Loaded {proteinsCount} proteins");
            Console.WriteLine($"Loaded {commandsCount} commands");
            Console.WriteLine();
        }

        static void Main(string[] args)
        {
            string sequencesFile = "sequences.2.txt";
            string commandsFile = "commands.2.txt";
            string outputFile = "genedata.2.txt";
            string authorName = "Алексей Полупанов";

            if (args.Length >= 1)
                sequencesFile = args[0];
            if (args.Length >= 2)
                commandsFile = args[1];
            if (args.Length >= 3)
                outputFile = args[2];
            if (args.Length >= 4)
                authorName = args[3];

            Console.WriteLine("=== GENETIC SEARCH ===");
            Console.WriteLine($"Input sequences: {sequencesFile}");
            Console.WriteLine($"Input commands: {commandsFile}"); 
                Console.WriteLine($"Output file: {outputFile}");
            Console.WriteLine();

            List<Protein> data = ReadData(sequencesFile);
            Console.WriteLine($"Loaded {data.Count} proteins");

            List<Command> commands = ReadCommands(commandsFile);
            Console.WriteLine($"Loaded {commands.Count} commands");
            Console.WriteLine();

            PrintStartupInfo(sequencesFile, commandsFile, outputFile, data.Count, commands.Count);

            CommandHandler(data, commands, outputFile, authorName);

            Console.WriteLine($"Done! Output written to {outputFile}");
        }
    }
}