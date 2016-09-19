using NuGet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChocoCheckUpdates
{
    class Program
    {
        static void Main(string[] args)
        {
            IPackageRepository pubRepo = PackageRepositoryFactory.Default.CreateRepository("https://chocolatey.org/api/v2/");
            IPackageRepository choRepo= PackageRepositoryFactory.Default.CreateRepository(@"\\nas\Choco\Packages");

            var packages = choRepo.GetPackages();

            foreach (IPackage p in packages)
            {
                Console.WriteLine("=> Package {0}", p.GetFullName());

                var package = pubRepo.FindPackage(p.Id);
                if (package != null)
                {
                    if (package.Version != p.Version)
                    {
                        Console.WriteLine("==> Version mismatch! {0} <> {1}", p.Version, package.Version);
                        package.
                    }
                }
            }

            Console.ReadLine();
        }
    }
}
