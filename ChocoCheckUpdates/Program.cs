using NuGet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChocoCompare
{
    class Program
    {
        static void Main(string[] args)
        {
            string chocoRepo = string.Empty;
            string localRepo = string.Empty;
            
            if (args.Count() == 0)
            {
                // No parameters provides, so check the config - and if there are none there prompt the user interactively.
                if (string.IsNullOrEmpty(Properties.Settings.Default.LocalRepo) || string.IsNullOrEmpty(Properties.Settings.Default.ChocoRepo))
                {
                    localRepo = Properties.Settings.Default.LocalRepo;
                    chocoRepo = Properties.Settings.Default.ChocoRepo;
                    Console.WriteLine("No settings found, please specify your repository locations");
                    Console.Write("Chocolatey Repository [{0}] (Please enter to keep): ", chocoRepo);
                    string respone = Console.ReadLine();
                    if (!string.IsNullOrEmpty(respone))
                    {
                        chocoRepo = respone;
                    }
                    Console.Write("Local Repository [{0}] (Please enter to keep): ", localRepo);
                    respone = Console.ReadLine();
                    if (!string.IsNullOrEmpty(respone))
                    {
                        localRepo = respone;
                    }
                }

                Properties.Settings.Default.ChocoRepo = chocoRepo;
                Properties.Settings.Default.LocalRepo = localRepo;
                Properties.Settings.Default.Save();
            }
            else
            {
                // Parse the command line for the two repository values
                if (args.Count() != 2)
                {
                    Console.WriteLine("Syntax:");
                    Console.WriteLine(" ChocoCompare.exe <chocolatey repository> <local repository>");
                    Console.WriteLine();
                    Console.WriteLine("Exit Codes above 10 are errors, exit code 1 indicates updates available");
                    Environment.Exit(10);
                }

                chocoRepo = args[0];
                localRepo = args[1];
            }

            // Validate configuration
            if (string.IsNullOrEmpty(chocoRepo))
            {
                Console.WriteLine("Error: Chocolatey repository must be specified and can not be left blank");
                Environment.Exit(11);
            }

            if (string.IsNullOrEmpty(localRepo))
            {
                Console.WriteLine("Error: Local repository must be specified and can not be left blank");
                Environment.Exit(12);
            }
            
            IPackageRepository pubRepo = PackageRepositoryFactory.Default.CreateRepository(chocoRepo);
            IPackageRepository choRepo= PackageRepositoryFactory.Default.CreateRepository(localRepo);

            var packages = choRepo.GetPackages();
            List<IPackage> packagesToUpdate = new List<IPackage>();

            foreach (IPackage p in packages)
            {
                string packageName = p.Title;
                if (string.IsNullOrEmpty(packageName))
                {
                    packageName = p.GetFullName();
                }
                Console.Write("Checking package {0}; local version is {1}", packageName, p.Version);

                var package = pubRepo.FindPackage(p.Id);
                if (package != null)
                {
                    Console.Write("; remote version is {0}", package.Version);
                    if (package.Version > p.Version)
                    {
                        ConsoleColor originalColour = Console.ForegroundColor;
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine();
                        Console.Write("Update available for {0} to {1}", packageName, package.Version);
                        Console.ForegroundColor = originalColour;
                        packagesToUpdate.Add(package);
                    }
                }
                Console.WriteLine();
            }

            Console.WriteLine("Finished checking packages; there are {0} packages to update.", packagesToUpdate.Count);

            if (System.Diagnostics.Debugger.IsAttached)
            {
                Console.WriteLine("Press enter to exit");
                Console.ReadLine();
            }

            if (packagesToUpdate.Count == 0)
            {
                Environment.Exit(0);
            }
            Environment.Exit(1);
        }
    }
}
