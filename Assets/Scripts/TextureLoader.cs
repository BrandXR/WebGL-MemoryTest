/*******************************************************
 * TextureLoader.cs
 * 
 * Loads textures from the cache or web.
 * 
 * If you have the KtxUnity plugin installed and the KTX scripting define symbol in project settings, 
 * you can also use this class to load .ktx or .basis textures
 * 
 *******************************************************/
using System;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;
using Unity.Collections;

#if KTX
using KtxUnity;
#endif

namespace BrandXR.Textures
{
    public class TextureLoader : Singleton<TextureLoader>
    {
        #region VARIABLES
        private const string CACHE_NAME = "Textures";
        private string cacheFolderPath = "";
        
        public class TextureOrientation
        {
            public bool IsXFlipped = false;
            public bool IsYFlipped = false;
        }
        #endregion

        #region STARTUP LOGIC
        //---------------------------------------------------------------//
        private void Awake()
        //---------------------------------------------------------------//
        {
            SetupCacheFolderPath();

        } //END Awake

        //---------------------------------------------------------------//
        private void SetupCacheFolderPath()
        //---------------------------------------------------------------//
        {
            if( string.IsNullOrEmpty( cacheFolderPath ) )
            {
                cacheFolderPath = Application.persistentDataPath + Path.DirectorySeparatorChar + CACHE_NAME;
            }

            Directory.CreateDirectory( cacheFolderPath );

        } //END SetupCacheFolderPath
        #endregion

        #region LOAD FROM CACHE OR DOWNLOAD

        //---------------------------------------------------------------//
        /// <summary>
        /// Loads a Texture2D from the devices local storage cache or a web resource.
        /// If the file does not already exist we will download and cache it
        /// </summary>
        /// <param name="url">The URL to the texture</param>
        /// <param name="successCallback">Sends you a Texture2D and the path to the cached texture when the load has completed</param>
        /// <param name="errorCallback">Contains the UnityWebRequest error, or lets you know if your URL is empty</param>
        /// <param name="progressCallback">Returns a float from 0-1 while downloading occurs</param>
        /// <returns></returns>
        public void LoadFromCacheOrDownload(
            string url,
            Action<Texture2D, string, TextureOrientation> successCallback,
            Action<string> errorCallback = null,
            Action<float> progressCallback = null,
            bool tryLoadFromCache = true )
        //-----------------------------------------------------------------------------------------------------------------------------------------------------//
        {

            StartCoroutine( ILoadFromCacheOrDownload( url, successCallback, errorCallback, progressCallback, tryLoadFromCache ) );

        } //END LoadFromCacheOrDownload

        //-----------------------------------------------------------------------------------------------------------------------------------------------------//
        /// <summary>
        /// The underlying IEnumerator that loads a texture from the cache or downloads it. Use the regular LoadFromCacheOrDownload() unless you need the coroutine this method returns to cancel the logic while in progress
        /// </summary>
        /// <param name="url">The URL to the texture</param>
        /// <param name="successCallback">Sends you a Texture2D and the path to the cached texture when the load has completed</param>
        /// <param name="errorCallback">Contains the UnityWebRequest error, or lets you know if your URL is empty</param>
        /// <param name="progressCallback">Returns a float from 0-1 while downloading occurs</param>
        /// <returns></returns>
        public IEnumerator ILoadFromCacheOrDownload(
            string url,
            Action<Texture2D, string, TextureOrientation> successCallback,
            Action<string> errorCallback = null,
            Action<float> progressCallback = null,
            bool tryLoadFromCache = true )
        //-----------------------------------------------------------------------------------------------------------------------------------------------------//
        {
            SetupCacheFolderPath();

            //If the url is empty, we can't continue
            if( string.IsNullOrEmpty( url ) )
            {
                errorCallback?.Invoke( "TextureLoader.cs FromURL() passed in URL is null or empty" );
                yield break;
            }

            //Flag that will prevent us from downloading from the web if we find the texture in our cache
            //We can't use a 'yield break' command inside of our anonymous success function, so this flag does the trick instead.
            bool foundInCache = false;
            bool checkingCache = false;

#if !UNITY_EDITOR && UNITY_WEBGL
            tryLoadFromCache = false;
#endif

            //Should we try to pull from the cache?
            if( tryLoadFromCache )
            {
                string cachePath = cacheFolderPath + Path.DirectorySeparatorChar + Path.GetFileName( url );
                string requestCachePath = cachePath;
                checkingCache = true;

                //On mobile devices we need to add 'File://'
#if !UNITY_EDITOR && ( UNITY_ANDROID || UNITY_IOS || UNITY_PC || UNITY_MAC || UNITY_WEBGL )
            if( !requestCachePath.Contains( "File://" ) ) { requestCachePath = "File://" + requestCachePath; }
#endif

                RequestTexture
                (
                    requestCachePath,
                    ( Texture2D texture, string path, TextureOrientation orientation ) =>
                    {
                        //Debug.Log( "TextureLoader.cs LoadFromCacheOrDownload() Restored from cache = " + cachePath );
                        successCallback?.Invoke( texture, path, orientation );
                        foundInCache = true;
                        checkingCache = false;
                    },
                    ( string error ) =>
                    {
                        checkingCache = false;
                        //Debug.Log( "TextureLoader.cs LoadFromCacheOrDownload() Couldn't find in cache = " + cachePath );
                    }, null
                );

            }

            while( checkingCache )
            {
                yield return null;
            }

            //We either don't have the file in the cache or we couldn't locate it, let's try to download it from the given URL
            if( !foundInCache )
            {
                RequestTexture
                (
                    url,
                    ( Texture2D texture, string path, TextureOrientation orientation ) =>
                    {
                        successCallback?.Invoke( texture, path, orientation );
                    },
                    ( string error ) =>
                    {
                        errorCallback?.Invoke( error );
                    },
                    ( float progress ) =>
                    {
                        progressCallback?.Invoke( 1f );
                    }
                );
            }


        } //END LoadFromCacheOrDownload

        #endregion

        #region REQUEST TEXTURE
        //---------------------------------------------------//
        public void RequestTexture( 
            string url,
            Action<Texture2D, string, TextureOrientation> successCallback,
            Action<string> errorCallback = null,
            Action<float> progressCallback = null )
        //---------------------------------------------------//
        {

            StartCoroutine( IRequestTexture( url, successCallback, errorCallback, progressCallback ) );

        } //END RequestTexture

        //---------------------------------------------------//
        public IEnumerator IRequestTexture(
            string url,
            Action<Texture2D, string, TextureOrientation> successCallback,
            Action<string> errorCallback = null,
            Action<float> progressCallback = null )
        //---------------------------------------------------//
        {
            string cachePath = cacheFolderPath + Path.DirectorySeparatorChar + Path.GetFileName( url );
            TextureOrientation bxrOrientation = new TextureOrientation();
            string name = Path.GetFileName( url );
            string mimeType = "image/" + Path.GetExtension( url ).Remove( 0, 1 );

            //.KTX or .BASIS
            if( mimeType == "image/ktx" || mimeType == "image/ktx2" || mimeType == "image/basis" )
            {
#if !KTX
                errorCallback?.Invoke( "KTX and basis texture support is not enabled, try enabling 'KTX' scripting define symbol in project settings and make sure KtxUnity plugin is in your project" );
                yield return null;
#else

                var www = UnityWebRequest.Get( url );
                yield return www.SendWebRequest();
                while( !www.isDone ) { progressCallback?.Invoke( www.downloadProgress ); }

                //Unity never sends a final progress callback, so we do it ourselves
                progressCallback?.Invoke( 1f );

                if( www.isNetworkError || www.isHttpError )
                {
                    errorCallback?.Invoke( www.error );
                }
                else
                {
                    try
                    {
                        //Debug.Log( "About to transcode = " + Path.GetFileName( url ) );

                        //Guide on Native memory arrays and allocator options
                        //https://www.jacksondunstan.com/articles/5446
                        NativeArray<byte> data = new NativeArray<byte>( www.downloadHandler.data, KtxNativeInstance.defaultAllocator );

                        //Create a KtxUnity plugin TextureBase component, which will handle the transcoding of the bytes to the texture
                        TextureBase textureBase = null;

                        if( mimeType == "image/ktx" || mimeType == "image/ktx2" )
                            textureBase = new KtxTexture();
                        else if( mimeType == "image/basis" )
                            textureBase = new BasisUniversalTexture();

                        textureBase.onTextureLoaded += ( Texture2D tex, KtxUnity.TextureOrientation orientation ) =>
                        {
                            if( tex != null )
                            {
                                //Debug.Log( "orientation = " + orientation.ToString() + ", IsXFlipped = " + orientation.IsXFlipped() + ", IsYFlipped = " + orientation.IsYFlipped() );
                                tex.name = name;
                                bxrOrientation.IsXFlipped = orientation.IsXFlipped();
                                bxrOrientation.IsYFlipped = orientation.IsYFlipped();

                                successCallback?.Invoke( tex, url, bxrOrientation );
                            }
                            else
                            {
                                errorCallback?.Invoke( "Unable to transcode " + mimeType + " texture[ " + data.Length + " ], from path = " + Path.GetFileName( url ) );
                            }
                        };
                        textureBase.LoadFromBytes( data, this );
                        
                        //Save our bytes to persistent storage cache
                        //Debug.Log( "TextureLoader.cs IRequestTexture() Saving to cache = " + cachePath );
                        File.WriteAllBytes( cachePath, data.ToArray() );

                        //Dispose of our Native array, otherwise we'll have a memory leak
                        data.Dispose();

                    }
                    catch( Exception e )
                    {
                        errorCallback?.Invoke( e.Message );
                    }
                }

#endif
            }
            else //.JPG or .PNG
            {
                using( UnityWebRequest www = UnityWebRequestTexture.GetTexture( url, false ) )
                {
                    yield return www.SendWebRequest();

                    while( !www.isDone )
                    {
                        progressCallback?.Invoke( www.downloadProgress );
                    }

                    //Unity never sends a final progress callback, so we do it ourselves
                    progressCallback?.Invoke( 1f );

                    if( www.isNetworkError || www.isHttpError )
                    {
                        errorCallback?.Invoke( www.error );
                    }
                    else
                    {
                        //Save our bytes to persistent storage cache
                        //Debug.Log( "TextureLoader.cs IRequestTexture() Saving to cache = " + cachePath );
                        if( !File.Exists( cachePath ) )
                            File.WriteAllBytes( cachePath, www.downloadHandler.data );

                        Texture2D tex = DownloadHandlerTexture.GetContent( www );
                        tex.name = name;
                        successCallback?.Invoke( tex, url, bxrOrientation );
                    }
                }
            }
            
        } //END RequestTexture
        #endregion

    } //END class

} //END namespace
