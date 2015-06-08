/*
 * Created by SharpDevelop.
 * User: SibVic
 * Date: 18.05.2007
 * Time: 20:39 
 */
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text.RegularExpressions;
using System.IO;

namespace RenRX
{
	class MainClass
	{
        struct ReplacePattern
        {
            public ReplacePattern(String regExFileMask, String newFileName)
            {
                RegExFileMask = regExFileMask;
                NewFileName = newFileName;
            }

            public String RegExFileMask;
            public String NewFileName;
        }

		public static void Main(string[] args)
		{
			PrintTitle();
			if (args.Length == 0)
			{
				PrintHelp();
				return;
			}

            List<ReplacePattern> matches = new List<ReplacePattern>();

            bool singlePass = true;
            if (args.Length == 1)
            {
                singlePass = false;
                string filename = args[0].Substring(3);
                string appPath = System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                string[] tokens = System.IO.File.ReadAllLines(System.IO.Path.Combine(appPath, filename));
                for (int i = 0; i + 1 < tokens.Length; i += 2)
                {
                    matches.Add(new ReplacePattern(tokens[i], tokens[i + 1]));
                }
            }
            else
                matches.Add(new ReplacePattern(args[0], args[1]));

			if (args.Length > 2)
				ParseParams(args);

            if (mRecursiveDir)
            {
                string[] subdirectories = System.IO.Directory.GetDirectories(System.IO.Directory.GetCurrentDirectory(), "*", System.IO.SearchOption.AllDirectories);
                foreach (string directory in subdirectories)
                    Rename(matches, singlePass, directory);
            }
            else
                Rename(matches, singlePass, System.IO.Directory.GetCurrentDirectory());
		}

        static void Rename(List<ReplacePattern> matches, bool singlePass, string directory)
        {
            if (mDirectories)
                RenameDirectories(matches, singlePass, directory);
            else
                RenameFiles(matches, singlePass, directory);
        }
		
        static string getNewName(string fileName, List<ReplacePattern> regexes, bool singlePass)
        {
            foreach (ReplacePattern regex in regexes)
            {
                Regex rx = new Regex(regex.RegExFileMask, mCaseSensetive ? RegexOptions.None : RegexOptions.IgnoreCase);
                while (rx.IsMatch(fileName))
                {
                    fileName = rx.Replace(fileName, regex.NewFileName);
                    if (singlePass)
                        break;
                }
            }

            return fileName;
        }

        private static void RenameFiles(List<ReplacePattern> matches, bool singlePass, string directory)
		{
			string[] files = System.IO.Directory.GetFiles(directory);
			foreach (string file in files)
			{
				string fileName = System.IO.Path.GetFileName(file);
                string replacedFileName = getNewName(fileName, matches, singlePass);
				try
				{
                    if (string.Compare(fileName, replacedFileName, true) == 0)
                        continue;
                    if (mTestMode)
                        Console.WriteLine("{0} -> {1}", fileName, replacedFileName);
                    else
                        System.IO.File.Move(Path.Combine(directory, fileName), Path.Combine(directory, replacedFileName));
				}
				catch
				{
					Console.WriteLine("Imposible to rename {0}", file);
				}
			}
		}

        private static void RenameDirectories(List<ReplacePattern> matches, bool singlePass, string directory)
        {
            string[] dirs = System.IO.Directory.GetDirectories(directory);
            foreach (string dir in dirs)
            {
                string fileName = System.IO.Path.GetFileName(dir);
                string replacedFileName = getNewName(fileName, matches, singlePass);
                try
                {
                    if (string.Compare(fileName, replacedFileName, true) == 0)
                        continue;
                    if (mTestMode)
                        Console.WriteLine("{0} -> {1}", fileName, replacedFileName);
                    else
                        System.IO.Directory.Move(Path.Combine(directory, fileName), Path.Combine(directory, replacedFileName));
                }
                catch
                {
                    Console.WriteLine("Imposible to rename {0}", dir);
                }
            }
        }

        #region Parameters
        private static bool mTestMode = false;
        private static bool mCaseSensetive = false;
        private static bool mRecursiveDir = false;
        private static bool mDirectories = false;

		private static void ParseParams(string[] args)
		{
            for (int i = 2; i < args.Length; i++)
            {
                switch (args[i])
                {
                    case "/t":
                    case "\t":
                        mTestMode = true;
                        break;
                    case "/cs":
                        mCaseSensetive = true;
                        break;
                    case "/r":
                        mRecursiveDir = true;
                        break;
                    case "/d":
                        mDirectories = true;
                        break;
                    default:
                        throw new Exception("Unknown key");
                }
            }
		}
        #endregion

        private static void PrintTitle()
		{
			AssemblyName assembly = new AssemblyName(Assembly.GetExecutingAssembly().FullName);
			Console.WriteLine("SibVic RegExp renamer v.{0}.{1}.{2}", 
			                  assembly.Version.Major, assembly.Version.Minor, assembly.Version.Build);
            Console.WriteLine("  Copyright (C) Tereschenko Victor 2007. sibvic@mail.ru | http://sibvic.h14.ru");
		}
		
		private static void PrintHelp()
		{
			Console.WriteLine("How to use: renrx regular_expression replace_with_that");
			Console.WriteLine("Example: renrx badfile\\.(avi|mpg) goodfile.$1");
			Console.WriteLine("\nOptions:");
			Console.WriteLine("  /t - Test mode: do not rename files, just print list of files wich will be renamed");
            Console.WriteLine("  /cs - Case sensetive");
            Console.WriteLine("  /r - Recursive (in subdirectories)");
            Console.WriteLine("  /d - Rename directories instead of files");
		}
    }
}
