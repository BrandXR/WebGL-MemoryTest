using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using BrandXR.Memory;

namespace BrandXR.Textures
{
    public class MemoryTest: MonoBehaviour
    {

        #region VARIABLES
        public string url;
        public TextMeshProUGUI availableMemory;
        public TextMeshProUGUI inUseMemory;
        public TextMeshProUGUI totalMemory;

        public TMP_InputField inputField;
        public TextMeshProUGUI textureCount;
        public Image imagePreview;

        [NonSerialized]
        public int totalNumberOfTextures;

        public TextMeshProUGUI reservedHeap;
        public TextMeshProUGUI allocatedHeap;
        public TextMeshProUGUI unallocatedHeap;
        #endregion

        #region STARTUP LOGIC

        //-------------------------------//
        public void Start()
        //-------------------------------//
        {
            UpdateValues();

        } //END Start

        #endregion

        #region UPDATE LOGIC

        //-------------------------------//
        public void Update()
        //-------------------------------//
        {
            UpdateValues();

        } //END Update

        //-------------------------------//
        public void UpdateValues()
        //-------------------------------//
        {
            
            totalMemory.text = "Total Memory: " + MemoryQuery.GetTotalMemory().ToString();
            inUseMemory.text = "In-Use Memory: " + MemoryQuery.GetInUseMemory().ToString();
            availableMemory.text = "Available Memory: " + MemoryQuery.GetAvailableMemory().ToString();

            reservedHeap.text = "Reserved Heap: " + MemoryQuery.GetHeapReservedMemory().ToString();
            allocatedHeap.text = "Allocated Heap: " + MemoryQuery.GetHeapAllocatedMemory().ToString();
            unallocatedHeap.text = "Unallocated Heap: " + MemoryQuery.GetHeapUnallocatedMemory().ToString();
            
        } //END UpdateValues

        #endregion

        #region BUTTON PRESSED

        //-------------------------------------------//
        public void ButtonPressed()
        //-------------------------------------------//
        {
            StartCoroutine( _ButtonPressed() );

        } //END ButtonPressed

        //-------------------------------------------//
        private IEnumerator _ButtonPressed()
        //-------------------------------------------//
        {
            int numTextures;

            if( Int32.TryParse( inputField.text, out numTextures ) )
            {
                if( numTextures <= 0 )
                {
                    Debug.LogError( "Please enter a number above zero" );
                    yield return null;
                }
                else
                {
                    for( int i = 0; i < numTextures; i++ )
                    {
                        Debug.Log( "Loading " + ( totalNumberOfTextures + ( i + 1 ) ) );

                        Coroutine coroutine = SpriteLoader.Instance.ILoadFromCacheOrDownload
                        (
                            url,
                            ( Sprite sprite, string path ) =>
                            {
                                if( imagePreview.sprite == null )
                                {
                                    imagePreview.sprite = sprite;
                                    imagePreview.preserveAspect = true;
                                }
                            },
                            ( string error ) =>
                            {
                                Debug.LogError( error );
                            },
                            ( float progress ) =>
                            {
                                //Debug.Log( progress );
                            }
                        );

                        yield return coroutine;
                    }

                    totalNumberOfTextures += numTextures;
                    textureCount.text = "Texture Count: " + totalNumberOfTextures;
                }
            }
            else
            {
                Debug.LogError( "Please enter a number in the input field" );
            }

        } //END ButtonPressed

        #endregion

    } //END class

} //END Namespace