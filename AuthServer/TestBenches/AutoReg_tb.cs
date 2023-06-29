using Server;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuthServer.TestBenches.AutoRegistration
{
    public static class AutoReg_tb
    {

        public static void Run()
        {
            Console.WriteLine("============ AUTO REGISTER TEST =====================");
            //Creating a new autoRegArray
            AutoRegisterArray<CustomElement> autoRegisterArray = new AutoRegisterArray<CustomElement> (10, AllocationPolicy.firstFreeElement);

            //Creating custom elements to register
            CustomElement element1 = new CustomElement(10);
            element1.Bind(autoRegisterArray);
            
            CustomElement element2 = new CustomElement(11);
            element2.Bind(autoRegisterArray);

            CustomElement element3 = new CustomElement(12);
            element3.Bind(autoRegisterArray);

            //Elements registers themselves
            element1.Register();
            element2.Register();
            element3.Register();

            Console.WriteLine("Registered 3 elements array count :[" + autoRegisterArray.array.allocatedCount + "]:");
            Console.WriteLine(autoRegisterArray.array.ToString());

            Console.WriteLine("element 2 has ticket : " + element2.arrayTicket);

            Console.WriteLine("Unregistering element 2");
            element2.UnRegister();

            Console.WriteLine("2 elements should be registered count : [" + autoRegisterArray.array.allocatedCount + "]:");
            Console.WriteLine(autoRegisterArray.array.ToString());

            Console.WriteLine("Cycling through array");
            foreach(CustomElement e in  autoRegisterArray.array.AggregatedArray())
            {
                Console.Write(e.number + " ");
            }
        }
    }

    public class CustomElement : AutoRegisterableElement<CustomElement>
    {
        public int number;

        public CustomElement(int n) { number = n; }


        protected override OnRegisterEventArgs<CustomElement> __BuildRegisterArgs()
        {
            return new OnRegisterEventArgs<CustomElement>()
            {
                eventDate = DateTime.Now,
                objToRegister = this
            };
        }
    }
}
