/*************************************************************
* Loader.js
*
* Loads the Unity WebGL instance, using configuration settings from the editor
*
**************************************************************/

//Relative path to the build folder
var buildUrl = "Build";
var loaderUrl = buildUrl + "/{{{ LOADER_FILENAME }}}";
		 
//Create the unity configuration based on our editor settings
var config = 
{
	dataUrl: buildUrl + "/{{{ DATA_FILENAME }}}",
	frameworkUrl: buildUrl + "/{{{ FRAMEWORK_FILENAME }}}",
	codeUrl: buildUrl + "/{{{ CODE_FILENAME }}}",

#if MEMORY_FILENAME
	memoryUrl: buildUrl + "/{{{ MEMORY_FILENAME }}}",
#endif

#if SYMBOLS_FILENAME
	symbolsUrl: buildUrl + "/{{{ SYMBOLS_FILENAME }}}",
#endif

	streamingAssetsUrl: "StreamingAssets",
	companyName: "{{{ COMPANY_NAME }}}",
	productName: "{{{ PRODUCT_NAME }}}",
	productVersion: "{{{ PRODUCT_VERSION }}}",
};
		 
var container = document.querySelector("#unity-container");
var canvas = document.querySelector("#unity-canvas");
var loadingBar = document.querySelector("#unity-loading-bar");
	 
//Get the pixel ratio of the viewport, depending on whether this is a mobile device
const maxPixelRatioMobile = 2.0;
const maxPixelRatioDesktop = 1.5;
	
var isMobile = /iPhone|iPad|iPod|Android/i.test(navigator.userAgent);
var maxDevicePixelRatio = isMobile? maxPixelRatioMobile: maxPixelRatioDesktop;
var pixelRatio = Math.min(window.devicePixelRatio, maxDevicePixelRatio);
		 
if( isMobile ) 
{
	container.className = "unity-mobile";
	config.devicePixelRatio = 1;
} 
else 
{
#if FULLSCREEN_CANVAS
	container.className = "unity-mobile";
#else
	canvas.style.width = "{{{ WIDTH }}}px";
	canvas.style.height = "{{{ HEIGHT }}}px";
#endif
}

#if BACKGROUND_FILENAME
	canvas.style.background = "url('" + buildUrl + "/{{{ BACKGROUND_FILENAME.replace(/'/g, '%27') }}}') center / cover";
#endif

loadingBar.style.display = "block";

var script = document.createElement("script");
script.src = loaderUrl;

script.onload = () => 
{
	createUnityInstance(canvas, config, (progress) => 
	{
		if (progress == 1) 
		{
			document.getElementById("ProgressLine").style.width = (200 * (progress)) + "px";
			document.getElementById("loadingInfo").innerHTML = "loading";
			document.getElementById("progressC").innerHTML = Math.floor((100 * progress)) + "%";
		} 
		else 
		{
			document.getElementById("loadingInfo").innerHTML = "downloading";
			document.getElementById("progressC").innerHTML = Math.floor((100 * progress)) + "%";
			document.getElementById("ProgressLine").style.width = (200 * (progress)) + "px";
		}
	})
	.then((unityInstance) => 
	{
		window.unityInstance = unityInstance;
		$('#overlay').delay(350).animate({"margin-left": '-=1700'}, 'slow').fadeOut();
		$('#overlay-clip').delay(350).animate({opacity:0}, 'slow').fadeOut();
		$('#counter').delay(200).animate({opacity:0, "margin-top": '-=100'}, 150, function(){ $(this).css("display","none"); });
		$('#loadingBox').delay(200).animate({"margin-top": '+=100', opacity:0}, '200',function(){ $(this).css("display","none");});
		loadingBar.style.display = "none";
	})
	.catch((message) => 
	{
		alert(message);
	});
};
	
document.body.appendChild(script);
		