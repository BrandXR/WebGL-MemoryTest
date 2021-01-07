/*******************************************************
 * SpriteLoader.cs
 * 
 * Loads sprites from the cache or web. Uses TextureLoader.cs but passes the resulting texture back as a Sprite.
 * Optionally you can use versions of these functions that return the Coroutine handler to you, which is useful for controlling the coroutine in your own logic
 * 
 * If you have the KtxUnity plugin installed and the KTX scripting define symbol in project settings, 
 * you can also use this class (and the underlying TextureLoader.cs class) to load .ktx or .basis textures
 * 
 *******************************************************/
using System;
using System.Collections;
using UnityEngine;

namespace BrandXR.Textures
{
    public class SpriteLoader: Singleton<SpriteLoader>
    {

        //----------------------------------------------------------//
        /// <summary>
        /// Loads a sprite from the devices local storage cache or a web resource.
        /// If the file does not already exist we will download and cache it
        /// </summary>
        /// <param name="url">The URL to the texture, for the caching system to work this URL will need to contain the file type extension</param>
        /// <param name="successCallback">Sends you a sprite and the local path to the cached texture when the load has completed</param>
        /// <param name="errorCallback">Contains the UnityWebRequest error, or lets you know if your URL is empty</param>
        /// <param name="progressCallback">Returns a float from 0-1 while downloading occurs</param>
        /// <param name="cacheName">The name of the file used when storing or retrieving from local storage. We will append the file type from the end or the URL to this. If left empty we will not use the caching system</param>
        /// <param name="cacheFolder">When storing or retrieving from local storage, we will use this path. If left empty we will store the file in PersistentData/Textures</param>
        /// <param name="extension">Optional file extension type (EX: '.jpg'). If left blank we will try to find the etension from the url value</param>
        /// <returns></returns>
        public void LoadFromCacheOrDownload(
            string url,
            Action<Sprite, string> successCallback,
            Action<string> errorCallback = null,
            Action<float> progressCallback = null,
            bool tryLoadFromCache = true )
        //----------------------------------------------------------//
        {
            TextureLoader.Instance.LoadFromCacheOrDownload
            (
                url,
                ( Texture2D texture, string cache, TextureLoader.TextureOrientation orientation ) =>
                {
                    var sprite = Sprite.Create( texture, GetSpriteRect( texture.width, texture.height, orientation.IsXFlipped, orientation.IsYFlipped ), Vector2.zero ); //new Vector2( 0.5f, 0.5f )
                    successCallback?.Invoke( sprite, cache );
                },
                ( string error ) =>
                {
                    errorCallback?.Invoke( error );
                },
                ( float progress ) =>
                {
                    progressCallback?.Invoke( progress );
                },
                tryLoadFromCache
            );

        } //END LoadFromCacheOrDownload

        //----------------------------------------------------------//
        /// <summary>
        /// The underlying IEnumerator that loads a sprite from cache or web and returns the controlling coroutine via TextureLoader.cs. Use the regular LoadFromCacheOrDownload() from this class unless you need the coroutine this method returns to cancel the logic while in progress
        /// </summary>
        /// <param name="url">The URL to the texture, for the caching system to work this URL will need to contain the file type extension</param>
        /// <param name="successCallback">Sends you a sprite and the local path to the cached texture when the load has completed</param>
        /// <param name="errorCallback">Contains the UnityWebRequest error, or lets you know if your URL is empty</param>
        /// <param name="progressCallback">Returns a float from 0-1 while downloading occurs</param>
        /// <param name="cacheName">The name of the file used when storing or retrieving from local storage. We will append the file type from the end or the URL to this. If left empty we will not use the caching system</param>
        /// <param name="cacheFolder">When storing or retrieving from local storage, we will use this path. If left empty we will store the file in PersistentData/Textures</param>
        /// <param name="extension">Optional file extension type (EX: '.jpg'). If left blank we will try to find the etension from the url value</param>
        /// <returns></returns>
        public Coroutine ILoadFromCacheOrDownload(
            string url,
            Action<Sprite, string> successCallback,
            Action<string> errorCallback = null,
            Action<float> progressCallback = null,
            bool tryLoadFromCache = true )
        //----------------------------------------------------------//
        {
            return StartCoroutine( TextureLoader.Instance.ILoadFromCacheOrDownload
            (
                url,
                ( Texture2D texture, string cache, TextureLoader.TextureOrientation orientation ) =>
                {
                    var sprite = Sprite.Create( texture, GetSpriteRect( texture.width, texture.height, orientation.IsXFlipped, orientation.IsYFlipped ), Vector2.zero  ); //new Vector2( 0.5f, 0.5f )
                    successCallback?.Invoke( sprite, cache );
                },
                ( string error ) =>
                {
                    errorCallback?.Invoke( error );
                },
                ( float progress ) =>
                {
                    progressCallback?.Invoke( progress );
                },
                tryLoadFromCache
            ) );

        } //END ILoadFromCacheOrDownload

        //----------------------------------------------//
        public void RequestSprite( 
            string url,
            Action<Sprite, string> successCallback,
            Action<string> errorCallback = null,
            Action<float> progressCallback = null )
        //----------------------------------------------//
        {

            TextureLoader.Instance.RequestTexture(
                url,
                (Texture2D texture, string cache, TextureLoader.TextureOrientation orientation ) =>
                {
                    var sprite = Sprite.Create( texture, GetSpriteRect( texture.width, texture.height, orientation.IsXFlipped, orientation.IsYFlipped ), Vector2.zero ); //new Vector2( 0.5f, 0.5f )
                    successCallback?.Invoke( sprite, cache );
                },
                (string error ) =>
                {
                    errorCallback?.Invoke( error );
                },
                (float progress ) =>
                {
                    progressCallback?.Invoke( progress );
                }
            );

        } //END RequestSprite

        //---------------------------------------------------//
        public Rect GetSpriteRect( float width, float height, bool isXFlipped, bool isYFlipped )
        //---------------------------------------------------//
        {
            Vector2 pos = new Vector2( 0, 0 );
            Vector2 size = new Vector2( width, height );

            if( isXFlipped )
            {
                pos.x = size.x;
                size.x *= -1;
            }

            if( isYFlipped )
            {
                pos.y = size.y;
                size.y *= -1;
            }

            return new Rect( pos, size );

        } //END GetSpriteRect

    } //END class

} //END namespace