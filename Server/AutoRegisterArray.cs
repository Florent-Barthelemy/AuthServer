using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    /// <summary>
    /// Arrays that handle element auto registration
    /// </summary>
    /// <typeparam name="T">TypeName</typeparam>
    public class AutoRegisterArray<T>
    {
        /// <summary>
        /// The base array
        /// </summary>
        public AllocatedArray<T> array;

        /// <summary>
        /// Creates a new instance of the autoRegArray
        /// </summary>
        /// <param name="size"></param>
        /// <param name="policy"></param>
        public AutoRegisterArray(int size, AllocationPolicy policy) 
        {
            array = new AllocatedArray<T>(size, policy);
        }
        public void OnElementRegisters(object sender, OnRegisterEventArgs<T> e)
        {
            //Allocating a position to the object
            int AllocatedPosition = array.AddItem(e.objToRegister);

            //Giving its ticket (position) to enable un-registration
            ((AutoRegisterableElement<T>)sender).UpdateArrayTicket(AllocatedPosition);
        }

        public void OnElementUnRegisters(object sender, int ticketNumber)
        {
            array.RemoveItem(ticketNumber);
        }

        public void OnElementPreDelete(object sender, int ticketNumber)
        {
            array.MarkForDeletion(ticketNumber);
        }

    }

    /// <summary>
    /// Abstract that defines an auto-registerable element
    /// </summary>
    /// <typeparam name="T">Type name</typeparam>
    public abstract class AutoRegisterableElement<T>
    {
        public event EventHandler<OnRegisterEventArgs<T>> OnRegisterEvent;
        public event EventHandler<int> OnUnregisterEvent;
        public event EventHandler<int> OnPreDeleteEvent;

        public AutoRegisterableElement() { }

        public int arrayTicket;

        /// <summary>
        /// Binds events 
        /// </summary>
        /// <param name="array"></param>
        public void Bind(AutoRegisterArray<T> array)
        {
            this.OnRegisterEvent += array.OnElementRegisters;
            this.OnUnregisterEvent += array.OnElementUnRegisters;
            this.OnPreDeleteEvent += array.OnElementPreDelete;
        }

        public void Unbind(AutoRegisterArray<T> array)
        {
            this.OnRegisterEvent -= array.OnElementRegisters;
            this.OnUnregisterEvent -= array.OnElementUnRegisters;
        }

        /// <summary>
        /// Unregisters the element from the binding
        /// </summary>
        public void UnRegister()
        {
            OnUnregisterEvent?.Invoke(this, arrayTicket);
        }

        /// <summary>
        /// Marks the element for pre-un-registration
        /// </summary>
        public void PreUnregister()
        {
            OnPreDeleteEvent?.Invoke(this, arrayTicket);
        }

        /// <summary>
        /// Registers the object into a AutoRegisterArray
        /// </summary>
        /// <param name="array"></param>
        public void Register()
        {
            OnRegisterEvent?.Invoke(this, __BuildRegisterArgs());
        }

        /// <summary>
        /// Implementation of the event class construction
        /// </summary>
        /// <returns></returns>
        protected abstract OnRegisterEventArgs<T> __BuildRegisterArgs();

        public void UpdateArrayTicket(int ticket)
        {
            arrayTicket = ticket;
        }
    }

    public class OnRegisterEventArgs<T>
    {
        public DateTime eventDate { get; set; }
        public T objToRegister { get; set; }
    }
}
