using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization.Formatters;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    public enum AllocationPolicy
    {
        firstFreeElement
    }

    /// <summary>
    /// The ArrayAllocator classes allows allocation and de-allocation of array elements with
    /// automatic empty-cell filling
    /// </summary>
    /// <typeparam name="T">Array type</typeparam>
    public class AllocatedArray<T>
    {   
        AllocationPolicy policy;
        
        public T[] objectArray {get; private set;}
        public bool[] allocationMap { get; private set;}
        public bool[] preDeletionMap { get; private set;}

        /// <summary>
        /// The total array size, combining allocated and free cells
        /// </summary>
        public int size { get; private set; }

        /// <summary>
        /// The total number of allocated objects in the array
        /// </summary>
        public int allocatedCount { get; private set; }
        
        public int itemsInPreDeletionCount { get; private set; }

        /// <summary>
        /// Creates a new instance of an allocated array
        /// </summary>
        /// <param name="arraySize">Size of the array</param>
        /// <param name="allocationPolicy">The allocation policy to use</param>
        public AllocatedArray(int arraySize, AllocationPolicy allocationPolicy) 
        {
            policy = allocationPolicy;
            size = arraySize;
            allocatedCount = 0;
            itemsInPreDeletionCount = 0;

            objectArray = new T[arraySize];
            allocationMap = new bool[arraySize];
            preDeletionMap = new bool[arraySize];

            for (int i = 0; i < arraySize; i++)
            {
                allocationMap[i] = false;
                preDeletionMap[i] = false;
            }
        }

        /// <summary>
        /// Marks a cell as pre-deleted, item will be delete as soon as the
        /// DeleteAllMarked() method is called
        /// </summary>
        /// <param name="index">Index of the object to delete</param>
        /// <returns>The input index, -1 if the cell (@ index) is already empty</returns>
        public int MarkForDeletion(int index)
        {
            if (allocationMap[index] == false)
                return -1;

            //Mark the index as pre-deleted
            preDeletionMap[index] = true;

            //Increase pre-del count
            itemsInPreDeletionCount++;
            return index;
        }

        /// <summary>
        /// Deletes all items marked as pre-deleted
        /// </summary>
        /// <returns>Number of deleted objects</returns>
        public int DeleteAllMarked()
        {
            int deletedCount = 0;
            for(int i = 0;i < size;i++)
            {
                //If object is marked for pre-deletion
                if (preDeletionMap[i] == true)
                {
                    //Remove the item
                    if(RemoveItem(i) < 0) return -1;
                    
                    //Reset the pre-deletion flag
                    preDeletionMap[i] = false;
                    
                    //Decrease pre-del count
                    itemsInPreDeletionCount--;

                    deletedCount++;
                }
            }

            return deletedCount;
        }

        /// <summary>
        /// Adds an item to the array according to the current policy
        /// </summary>
        /// <param name="item">The element to insert</param>
        /// <returns>The index of the inserted element, -1 if array is full</returns>
        public int AddItem(T item)
        {
            if (policy == AllocationPolicy.firstFreeElement)
                return _AddWithPolicy_firstFreeElement(item);
            else
                return -1;
        }

        /// <summary>
        /// Removes an item from the array
        /// </summary>
        /// <param name="index">remove index</param>
        /// <returns>The index of deleted object, -1 if index is out of bounds</returns>
        public int RemoveItem(int index)
        {
            //Checking that index is in-bounds

            //Marking the cell as free
            if (allocationMap[index] == true && index >= 0 && index < size)
            {
                allocationMap[index] = false;
                allocatedCount--;
                return index;
            }
            else
                return -1;
        }

        /// <summary>
        /// Returns the index of the first free element in the array
        /// used to initiate an iterator
        /// </summary>
        /// <returns>Index of the first allocated element, -1 if array is empty</returns>
        public int ArrayStart()
        {
            for(int i = 0; i < size; i++)
            {
                if (allocationMap[i] == true) return i;
            }

            return -1;
        }

        /// <summary>
        /// Gets the index of the next allocated item in the array
        /// </summary>
        /// <param name="lastIndex">Current index</param>
        /// <returns>The next allocated item index, -1 if there is no further allocated elements</returns>
        public int NextItem(int currentIndex)
        {

            if (currentIndex == ArrayStop())
                return currentIndex + 1;

            for(int i = currentIndex + 1; i < size; i++ )
            {
                if (allocationMap[i] == true) 
                {
                    return i; 
                }
            }
            return ArrayStop();
        }

        /// <summary>
        /// Returns the greatest index of allocated element
        /// </summary>
        /// <returns>End of allocated array, -1 if array is empty</returns>
        public int ArrayStop()
        {
            for(int i = size-1; i >= 0; i--)
            {
                if (allocationMap[i] == true) return i;
            }

            return -1;
        }

        /// <summary>
        /// Empty the array
        /// </summary>
        public void Clear()
        {
            for(int i = 0; i < size; i++)
            {
                RemoveItem(i);
            }
        }

        /// <summary>
        /// Returns an enumerable on the allocated elements,
        /// </summary>
        /// <returns>An enumerable</returns>
        public IEnumerable<T> AggregatedArray()
        {
            int it = ArrayStart();
            if (it < 0) yield break;


            for (it = ArrayStart(); it <= ArrayStop(); it = NextItem(it))
            {
                yield return objectArray[it];
            }
        }

        private int _AddWithPolicy_firstFreeElement(T item)
        {
            //Finding the index of the first free element,
            //Note that a free element is marked as 'false'
            //in the allocation map.
            int freeItemIndex = -1;
            for(int i = 0;i < size; i++) 
            {
                if (allocationMap[i] == false)
                {
                    freeItemIndex = i;
                    break;
                }
            }

            //If freeItemIndex is still -1, array is full. Cannot add anymore objects
            //return -1 to indicate that the function has encountered issue
            if(freeItemIndex == -1) return -1;

            //Add element to the first free cell of the array and update the map
            objectArray[freeItemIndex] = item;
            allocationMap[freeItemIndex] = true;
            allocatedCount++;

            //Finally, return the newly allocated element index
            return freeItemIndex;
        }

        public override string ToString()
        {
            string o = "";
            for(int i = 0; i < size; i++)
            {
                o += "[" + allocationMap[i].ToString() + "] ";
            }
            return o;
        }


    }



}
