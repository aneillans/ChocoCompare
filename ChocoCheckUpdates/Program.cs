using NuGet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace ChocoCompare
{
    class Program
    {
        static void Main(string[] args)
        {
            Properties.Settings.Default.Upgrade();

            string chocoRepo = string.Empty;
            string localRepo = string.Empty;
            bool downloadUpdates = false;

            if (args.Count() < 2)
            {
                localRepo = Properties.Settings.Default.LocalRepo;
                chocoRepo = Properties.Settings.Default.ChocoRepo;

                // No parameters provides, so check the config - and if there are none there prompt the user interactively.
                if (string.IsNullOrEmpty(Properties.Settings.Default.LocalRepo) || string.IsNullOrEmpty(Properties.Settings.Default.ChocoRepo))
                {
                    Console.WriteLine("No settings found, please specify your repository locations");
                    Console.Write("Chocolatey Repository [{0}] (Press enter to keep): ", chocoRepo);
                    string respone = Console.ReadLine();
                    if (!string.IsNullOrEmpty(respone))
                    {
                        chocoRepo = respone;
                    }
                    Console.Write("Local Repository [{0}] (Press enter to keep): ", localRepo);
                    respone = Console.ReadLine();
                    if (!string.IsNullOrEmpty(respone))
                    {
                        localRepo = respone;
                    }
                }

                Properties.Settings.Default.ChocoRepo = chocoRepo;
                Properties.Settings.Default.LocalRepo = localRepo;
                Properties.Settings.Default.Save();

                if (args.Count() == 1)
                {
                    downloadUpdates = bool.Parse(args[0]);
                }
            }
            else
            {
                // Parse the command line for the two repository values
                if (args.Count() > 2)
                {
                    Console.WriteLine("Syntax:");
                    Console.WriteLine(" ChocoCompare.exe <chocolatey repository> <local repository>");
                    Console.WriteLine(" -OR-");
                    Console.WriteLine(" ChocoCompare.exe <download updates true/false>");
                    Console.WriteLine(" -OR-");
                    Console.WriteLine(" ChocoCompare.exe");
                    Console.WriteLine();
                    Console.WriteLine("Exit Codes above 10 are errors, exit code 1 indicates updates available");
                    Environment.Exit(10);
                }

                if (args.Count() == 2)
                {
                    chocoRepo = args[0];
                    localRepo = args[1];
                }
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
            IPackageRepository choRepo = PackageRepositoryFactory.Default.CreateRepository(localRepo);

            var localPackages = choRepo.GetPackages();
            List<IPackage> packagesToUpdate = new List<IPackage>();

            var packagesGroup = from localp in localPackages group localp by localp.Id into packageIdGroup select packageIdGroup;

            foreach (var grouping in packagesGroup)
            {
                // Get the latest version of this package.
                IPackage p = grouping.OrderByDescending(s => s.Version).First();
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

            if (downloadUpdates)
            {
                string tempFolder = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "ChocoCompare");
                System.IO.Directory.CreateDirectory(tempFolder);

                Console.WriteLine();
                ConsoleColor originalColour = Console.ForegroundColor;
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("Downloading updates to temp folder: {0}", tempFolder);
                Console.ForegroundColor = originalColour;

                foreach (IPackage package in packagesToUpdate)
                {
                    DataServicePackage pkg = package as DataServicePackage;
                    if (pkg != null)
                    {
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine("Package: {0}", pkg.Title);
                        Console.ForegroundColor = originalColour;
                        // Download the package to a temporary location first
                        using (WebClient client = new WebClient())
                        {
                            Console.WriteLine("Downloading to {0}", System.IO.Path.Combine(tempFolder, pkg.Id + "." + pkg.Version + ".nupkg"));
                            client.DownloadFile(pkg.DownloadUrl, System.IO.Path.Combine(tempFolder, pkg.Id + "." + pkg.Version + ".nupkg"));
                        }
                    }
                }

                // All downloaded, so go back over and copy them to the package folder
                Console.WriteLine();
                Console.WriteLine("Copying files to the local package repository: {0}", localRepo);
                Console.WriteLine();
                foreach (IPackage package in packagesToUpdate)
                {
                    DataServicePackage pkg = package as DataServicePackage;
                    if (pkg != null)
                    {
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine("Package: {0}", pkg.Title);
                        Console.ForegroundColor = originalColour;

                        System.IO.File.Move(System.IO.Path.Combine(tempFolder, pkg.Id + "." + pkg.Version + ".nupkg"), System.IO.Path.Combine(localRepo, pkg.Id + "." + pkg.Version + ".nupkg"));
                    }
                }
            }

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
