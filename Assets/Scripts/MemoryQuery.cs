/************************************************************************
 * MemoryQuery.cs
 * 
 * Provides static functions to query the total memory of the device, how much your using, and what memory is available to you to safely use
 * GetTotalMemory() - Returns the total memory of this device
 * GetInUseMemory() - How much memory are you using?
 * GetAvailableMemory() - How much memory can you safely use?
 * 
 * NOTE: GetAvailableMemory() has a safety buffer applied to it to prevent out of memory errors, you can adjust this with the 'availableMemorySafetyBuffer' private variable
 * 
 * NOTE: Uses the open source ByteSizeLib from https://github.com/omar/ByteSize
 * 
 * **********************************************************************/
using UnityEngine;
using UnityEngine.Profiling;
using ByteSizeLib;

namespace BrandXR.Memory
{
    public class MemoryQuery: MonoBehaviour
    {

#region VARIABLES
        //The percentage of the available memory you want to leave alone to prevent out of memory crashes
        private const double availableMemorySafetyBuffer = .30f; //Example, .70f == 70% of memory reported as available

        //We figure out our total memory available only once and re-use the value
        private static ByteSize availableMemory;
        #endregion

#region CORE LOGIC - QUERY MEMORY
        //---------------------------------------//
        public static ByteSize GetTotalMemory()
        //---------------------------------------//
        {

#if UNITY_EDITOR
            return Editor_GetTotalMemory();
#elif !UNITY_EDITOR && (UNITY_ANDROID || UNITY_IOS || UNITY_PC || UNITY_MAC || UNITY_WEBGL)
            return Mobile_GetTotalMemory();
#else
            return 0;
#endif

        } //END GetTotalMemory

        //---------------------------------------//
        public static ByteSize GetInUseMemory()
        //---------------------------------------//
        {

#if UNITY_EDITOR
            return Editor_GetInUseMemory();
#elif !UNITY_EDITOR && (UNITY_ANDROID || UNITY_IOS || UNITY_PC || UNITY_MAC || UNITY_WEBGL)
            return Mobile_GetInUseMemory();
#else
            return 0;
#endif

        } //END GetInUseMemory

        //---------------------------------------//
        public static ByteSize GetAvailableMemory()
        //---------------------------------------//
        {
#if UNITY_EDITOR
                return Editor_GetAvailableMemory();
#elif !UNITY_EDITOR && ( UNITY_ANDROID || UNITY_IOS )
                return Mobile_GetAvailableMemory();
#else
                return ByteSize.FromBytes(0);
#endif   

        } //END GetAvailableMemory

        //----------------------------------------//
        public static ByteSize GetHeapAllocatedMemory()
        //----------------------------------------//
        {

#if UNITY_EDITOR
            return Editor_GetHeapAllocatedMemory();
#elif !UNITY_EDITOR && (UNITY_ANDROID || UNITY_IOS || UNITY_PC || UNITY_MAC || UNITY_WEBGL)
                return Mobile_GetHeapAllocatedMemory();
#else
                return ByteSize.FromBytes(0);
#endif   

        } //END GetHeapAllocatedMemory

        //----------------------------------------//
        public static ByteSize GetHeapUnallocatedMemory()
        //----------------------------------------//
        {

#if UNITY_EDITOR
            return Editor_GetHeapUnallocatedMemory();
#elif !UNITY_EDITOR && (UNITY_ANDROID || UNITY_IOS || UNITY_PC || UNITY_MAC || UNITY_WEBGL)
                return Mobile_GetHeapUnallocatedMemory();
#else
                return ByteSize.FromBytes(0);
#endif   

        } //END GetHeapUnallocatedMemory

        //----------------------------------------//
        public static ByteSize GetHeapReservedMemory()
        //----------------------------------------//
        {

#if UNITY_EDITOR
            return Editor_GetHeapReservedMemory();
#elif !UNITY_EDITOR && (UNITY_ANDROID || UNITY_IOS || UNITY_PC || UNITY_MAC || UNITY_WEBGL)
                return Mobile_GetHeapReservedMemory();
#else
                return ByteSize.FromBytes(0);
#endif   

        } //END GetHeapReservedMemory

        #endregion

        #region EDITOR LOGIC - QUERY MEMORY
#if UNITY_EDITOR
        //--------------------------------------//
        /// <summary>
        /// Gets the total memory available to the editor in bytes
        /// </summary>
        /// <returns></returns>
        private static ByteSize Editor_GetTotalMemory()
        //--------------------------------------//
        {
            return ByteSize.FromMegaBytes( SystemInfo.systemMemorySize );

        } //END Editor_GetTotalMemory

        //--------------------------------------//
        /// <summary>
        /// Gets the in-use memory used by the editor in bytes
        /// </summary>
        /// <returns></returns>
        private static ByteSize Editor_GetInUseMemory()
        //--------------------------------------//
        {
            return ByteSize.FromBytes( Profiler.GetTotalAllocatedMemoryLong() );

        } //END Editor_GetInUseMemory

        //---------------------------------------//
        /// <summary>
        /// Gets how much memory is available to the editor minus a large safety buffer. Returns the value in bytes
        /// </summary>
        /// <returns></returns>
        private static ByteSize Editor_GetAvailableMemory()
        //---------------------------------------//
        {
            ByteSize total = Editor_GetTotalMemory();
            ByteSize inUse = Editor_GetInUseMemory();
            
            double value = total.Bytes;
            value = (value - ( value * availableMemorySafetyBuffer ) );         //Reserve a percentage of the system memory for safety
            value = value - inUse.Bytes;                                        //Reduce by the amount we've already used up
            return ByteSize.FromBytes( value );

        } //END Editor_GetAvailableMemory

        //---------------------------------------//
        /// <summary>
        /// Get the size of the mono code heap
        /// </summary>
        /// <returns></returns>
        private static ByteSize Editor_GetHeapAllocatedMemory()
        //---------------------------------------//
        {

            return ByteSize.FromBytes( Profiler.GetMonoUsedSizeLong() );

        } //END Editor_GetHeapAllocatedMemory

        //---------------------------------------//
        /// <summary>
        /// Get the total amount of reserved memory accessible by the mono heap
        /// </summary>
        /// <returns></returns>
        private static ByteSize Editor_GetHeapReservedMemory()
        //---------------------------------------//
        {
            return ByteSize.FromBytes( Profiler.GetMonoHeapSizeLong() );

        } //END Editor_GetHeapReservedMemory

        //---------------------------------------//
        /// <summary>
        /// Get the size of the mono code heap
        /// </summary>
        /// <returns></returns>
        private static ByteSize Editor_GetHeapUnallocatedMemory()
        //---------------------------------------//
        {
            return ByteSize.FromBytes( Profiler.GetMonoHeapSizeLong() - Profiler.GetMonoUsedSizeLong() );

        } //END Editor_GetHeapUnallocatedMemory
#endif
        #endregion

        #region MOBILE - QUERY MEMORY
#if !UNITY_EDITOR && ( UNITY_ANDROID || UNITY_IOS || UNITY_PC || UNITY_MAC || UNITY_WEBGL )
        //--------------------------------------//
        /// <summary>
        /// Gets the total memory available for this device
        /// </summary>
        /// <returns></returns>
        private static ByteSize Mobile_GetTotalMemory()
        //--------------------------------------//
        {
            if( TryUpdateMemoryInfo() )
            {
#if UNITY_ANDROID
                return ByteSize.FromKiloBytes( meminfo.minf.memtotal );
#elif UNITY_IOS
                return ByteSize.FromBytes( meminfo.minf.memtotal );
#endif
            }

            return ByteSize.FromBytes(0);

        } //END Mobile_GetTotalMemory

        //--------------------------------------//
        /// <summary>
        /// Gets the total amount of memory being used (allocated) along with the extra buffer (Reserved) by this process
        /// </summary>
        /// <returns></returns>
        private static ByteSize Mobile_GetInUseMemory()
        //--------------------------------------//
        {
            return ByteSize.FromBytes( Profiler.GetTotalReservedMemoryLong() );
            
        } //END Mobile_GetInUseMemory

        //--------------------------------------//
        /// <summary>
        /// Gets the available memory available for this process to use
        /// </summary>
        /// <returns></returns>
        private static ByteSize Mobile_GetAvailableMemory()
        //--------------------------------------//
        {
            if( TryUpdateMemoryInfo() )
            {
#if UNITY_ANDROID
                if( availableMemory.Bytes == 0 )
                {
                    availableMemory = ByteSize.FromKiloBytes( meminfo.minf.memavailable );
                    double value = availableMemory.Bytes;
                    value = value - ( value * availableMemorySafetyBuffer );    //Remove a large buffer from the total for safety  
                    availableMemory = ByteSize.FromBytes( value );
                }

                return availableMemory - Mobile_GetInUseMemory();
#elif UNITY_IOS
                if( availableMemory.Bytes == 0 ) 
                {
                    availableMemory = ByteSize.FromBytes( meminfo.minf.memavailable ); 
                    double value = availableMemory.Bytes;
                    value = value - ( value * availableMemorySafetyBuffer );    //Remove a large buffer from the total for safety  
                    availableMemory = ByteSize.FromBytes( value );
                }

                return availableMemory - Mobile_GetInUseMemory();
#endif
            }

            return ByteSize.FromBytes(0);

        } //END Mobile_GetAvailableMemory

        
        //---------------------------------------//
        /// <summary>
        /// Get the size of the mono code heap
        /// </summary>
        /// <returns></returns>
        private static ByteSize Mobile_GetHeapAllocatedMemory()
        //---------------------------------------//
        {

            return ByteSize.FromBytes( Profiler.GetMonoUsedSizeLong() );

        } //END Mobile_GetHeapAllocatedMemory

        //---------------------------------------//
        /// <summary>
        /// Get the total amount of reserved memory accessible by the mono heap
        /// </summary>
        /// <returns></returns>
        private static ByteSize Mobile_GetHeapReservedMemory()
        //---------------------------------------//
        {
            return ByteSize.FromBytes( Profiler.GetMonoHeapSizeLong() );

        } //END Mobile_GetHeapReservedMemory

        //---------------------------------------//
        /// <summary>
        /// Get the size of the mono code heap
        /// </summary>
        /// <returns></returns>
        private static ByteSize Mobile_GetHeapUnallocatedMemory()
        //---------------------------------------//
        {

            return ByteSize.FromBytes( Profiler.GetMonoHeapSizeLong() - Profiler.GetMonoUsedSizeLong() );

        } //END Mobile_GetHeapUnallocatedMemory

        //--------------------------------------//
        private static bool TryUpdateMemoryInfo()
        //--------------------------------------//
        {
#if !UNITY_WEBGL
            return meminfo.getMemInfo();
#else
            return false;
#endif

        } //END TryUpdateMemoryInfo
#endif
        #endregion

    } //END class

} //END namespace