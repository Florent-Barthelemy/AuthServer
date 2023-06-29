using Server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;

namespace AuthServer.TestBenches
{
    public static class AllocatedArray_tb
    {
        static AllocatedArray<int> allocationArray = new AllocatedArray<int>(10, AllocationPolicy.firstFreeElement);

        public static void Run()
        {
            Console.Write("Testing array allocator class");
            allocationArray.AddItem(10);
            allocationArray.AddItem(000000); //DELETE
            allocationArray.AddItem(5);
            allocationArray.AddItem(5);
            allocationArray.AddItem(000000); //DELETE
            allocationArray.AddItem(5);
            allocationArray.AddItem(6);

            Console.WriteLine("Allocated 3 elements :\n" + allocationArray.ToString());
            Console.WriteLine("deleting element 1 & 4");

            allocationArray.RemoveItem(1);
            allocationArray.RemoveItem(4);

            Console.WriteLine(allocationArray.ToString());

            Console.WriteLine("Cycling through all elements in the list");


            foreach (int i in allocationArray.AggregatedArray())
            {
                Console.Write(i + " ");
            }

            Console.WriteLine();

            Console.WriteLine("Adding a new element");
            allocationArray.AddItem(109);
            Console.WriteLine(allocationArray.ToString());
            foreach (int i in allocationArray.AggregatedArray())
            {
                Console.Write(i + " ");
            }
            Console.WriteLine();

            Console.WriteLine("Adding a new element");
            allocationArray.AddItem(109);
            Console.WriteLine(allocationArray.ToString());
            foreach (int i in allocationArray.AggregatedArray())
            {
                Console.Write(i + " ");
            }
            Console.WriteLine();

            Console.WriteLine("elt count : " + allocationArray.allocatedCount);

            Console.WriteLine("Adding a new element");
            allocationArray.AddItem(109);
            Console.WriteLine(allocationArray.ToString());
            foreach (int i in allocationArray.AggregatedArray())
            {
                Console.Write(i + " ");
            }
            Console.WriteLine();

            Console.WriteLine("Clearing the map...");
            allocationArray.Clear();
            foreach (int i in allocationArray.AggregatedArray())
            {
                Console.Write(i + " ");
            }
            Console.WriteLine();

            Console.WriteLine("Final elt count : " + allocationArray.allocatedCount);
        }
    }
}
