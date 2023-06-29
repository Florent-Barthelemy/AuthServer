using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Server
{
    /// <summary>
    ///Represents a block that can be read from,
    ///As soon as the data is read, it is destroyed 
    /// </summary>
    public class StreamBlock
    {

        private static Semaphore _pool;

        
        Queue<byte[]> availableData;

        /// <summary>
        /// Triggered when data has been pushed into the block
        /// </summary>
        public event EventHandler dataPosted;
        

        public StreamBlock() 
        {
            _pool = new Semaphore(0, 1);

            //Allow 1 thread to access the semaphore
            _pool.Release(1);
        }

        /// <summary>
        /// Data receive event, enqueue the data
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="data">Data to post</param>
        public void PostData(byte[] data)
        {
            //waiting for a semaphore release
            _pool.WaitOne();

            availableData.Enqueue(data);

            //release the semaphore
            _pool.Release(1);

            dataPosted?.Invoke(this, new EventArgs());
        }

        /// <summary>
        /// Returns and consumes all the available segments in the array
        /// </summary>
        /// <returns></returns>
        public IEnumerable<byte[]> GetAvailableSegments() 
        {
            //Copy the array for shorter access to shared ressource
            //waiting for a semaphore release
            _pool.WaitOne();

            var dataCopy = new List<byte[]>(availableData);

            //Release the semaphore
            _pool.Release(1);

            //Work on the copy
            for (int i = 0; i < dataCopy.Count; i++)
            {
                yield return availableData.Dequeue();
            }
        }


        /// <summary>
        /// Enumerates each bytes of the buffer by segments
        /// Breaking this method could result in data loss
        /// </summary>
        /// <returns></returns>
        public IEnumerable<byte> GetAvailableBytes() 
        {
            //Cycling trough all available segments
            foreach (byte[] segment in GetAvailableSegments())
            {
                //For each segments, return bytes
                for(int i = 0; i < segment.Length; i++)
                {
                    yield return segment[i];
                }
                //Deletion of segment is handeled by GetSegments method
            }
        
        }



    }
}
