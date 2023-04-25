using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CefSharp;
using CefSharp.OffScreen;
using Newtonsoft.Json;

namespace carscom
{
    internal class Program
    {
        private static ChromiumWebBrowser browser;

        private const string testUrl = "https://www.cars.com/";

        public static void Main(string[] args)
        {
            CefSharpSettings.SubprocessExitIfParentProcessClosed = true;

            var settings = new CefSettings()
            {
                CachePath = Path.Combine(Environment.GetFolderPath(
                                         Environment.SpecialFolder.LocalApplicationData), "CefSharp\\Cache")
            };
            Cef.Initialize(settings, performDependencyCheck: true, browserProcessHandler: null);
            browser = new ChromiumWebBrowser(testUrl);
            

            
            
            browser.LoadingStateChanged += BrowserLoadingStateChanged;
            browser.AddressChanged += Browser_AddressChanged;
            Console.ReadKey();
            Cef.Shutdown();
        }
        private static void Browser_FrameLoadStart(object sender, FrameLoadStartEventArgs e)
        {
            Console.WriteLine($"{e.Url} loading!");
        }

        static int pageCount = 0;
        private static void Browser_FrameLoadEnd(object sender, FrameLoadEndEventArgs e)
        {
        }

        static bool isLoggedIn = false;
        private static void BrowserLoadingStateChanged(object sender, LoadingStateChangedEventArgs e)
        {
            if (!e.IsLoading)
            {
                if (!isLoggedIn)
                {
                    var clickmenuScript = @"document.querySelector('.nav-user-name').click();";


                    browser.EvaluateScriptAsync(clickmenuScript).ContinueWith(u => {
                        Console.WriteLine("Click Menu");
                    });
                    var openSigninScript = @"document.querySelector('body > div.global-header-container > cars-global-header').shadowRoot.querySelector('spark-modal > div:nth-child(3) > div > spark-button:nth-child(1)').click();";


                    browser.EvaluateScriptAsync(openSigninScript).ContinueWith(u => {
                        Console.WriteLine("Open Sign in");
                    });


                    // fill the username and password fields with their respective values, then click the submit button
                    var loginScript = @"document.querySelector('#auth-modal-email').value = 'johngerson808@gmail.com';
                               document.querySelector('#auth-modal-current-password').value = 'test8008';
                               document.querySelector('body > div.global - header - container > cars - global - header > cars - auth - modal').shadowRoot.querySelector('spark - modal > form > spark - button').click();";
                    

                    browser.EvaluateScriptAsync(loginScript).ContinueWith(u => {
                        isLoggedIn = true;
                        Console.WriteLine("User Logged in");
                    });
                    

                    // push the "success" field and the text from all 'li' elements into an array
                    var makeSearchScript = @"document.querySelector('#makes optgroup option').value = 'Tesla';";
                    browser.EvaluateScriptAsync(makeSearchScript).ContinueWith(u =>
                    {
                        Console.WriteLine("Tesla models gathered");
                       

                    
                    });
                    var makeSearchScript2 = @"document.querySelector('#make-model-search-stocktype > option').value = 'used';
                               document.querySelector('#makes > optgroup > option').value = 'Tesla';
                               document.querySelector('#models > option').value = 'tesla-model_s';
                               document.querySelector('#make-model-max-price > option').value = '100000';
                               document.querySelector('#make-model-maximum-distance > option').value = 'all';
                               document.querySelector('#make-model-zip').value = '94596';
                               document.querySelector('#by-make-tab > div > div.sds-field.sds-home-search__submit > button').click();";
                    browser.EvaluateScriptAsync(makeSearchScript2).ContinueWith(u =>
                    {
                        Console.WriteLine("Search made");
                        
                    });
                  
                }
                else
                {
                    if (currentUrl != testUrl)
                    {
                        var pageQueryScript = @"
                    (function(){
                        var stockTypelist = document.querySelectorAll('.stock-type');
                        var titlelist = document.querySelectorAll('.title');
                        var mileagelist = document.querySelectorAll('.mileage');                
                        var primarypricelist = document.querySelectorAll('.primary-price');
                        var monthlypaymentlist = document.querySelectorAll('.js-estimated-monthly-payment-formatted-value-with-abr');
                        var dealernamelist = document.querySelectorAll('.dealer-name > strong');
                        var farlist = document.querySelectorAll('.miles-from');
                        var result = [];
                        for(var i=0; i < dealernamelist.length; i++) {
                            let stocktype=stockTypelist[i].innerText;
                            let title=titlelist[i].innerText;
                            let mileage=mileagelist[i].innerText;
                            let primaryprice=primarypricelist[i].innerText;
                            let monthlypayment=monthlypaymentlist[i].innerText;
                            let dealername=dealernamelist[i].innerText;
                            let far=farlist[i].innerText;
                            let obj={'stockType':stocktype,'title':title,'mileage':mileage,
                            'primaryprice':primaryprice,'monthlypayment':monthlypayment,'dealername':dealername,
                            'far from user':far};
                            result.push(obj)} 
                        return result; 
                    })()";
                        var scriptTask = browser.EvaluateScriptAsync(pageQueryScript);
                        scriptTask.ContinueWith(u =>
                        {
                            if (u.Result.Success && u.Result.Result != null)
                            {
                                Console.WriteLine("Bot output received!nn");
                                var filePath = "output.json";
                                var response = (List<dynamic>)u.Result.Result;
                                bool first = false;
                                //string fileName = "output.json";

                                foreach (object v in response)
                                {
                                    string jsonString = JsonConvert.SerializeObject(v);
                                    //carobjects carinfos = new carobjects();
                                    if (first == false)
                                    {
                                        
                                        File.WriteAllText(filePath, jsonString);
                                        first = true;                                        
                                    }
                                    else
                                    {
                                        File.AppendAllText(filePath, jsonString);
                                    }

                                    

                                    Console.WriteLine(v);
                                    foreach (var item in (dynamic)v)
                                    {                                      
                                        object key = item.Key;
                                        object value = item.Value;                                        
                                        Console.WriteLine(key + " :" +value);
                                    }


                                }                                
                                Console.WriteLine($"Bot output saved to {filePath}");
                                Console.WriteLine("Press any key to close.");
                            }
                        });

                    }
                }
            }
        
        }
       
        static string currentUrl = "";
        private static void Browser_AddressChanged(object sender, AddressChangedEventArgs e)
        {
            currentUrl = e.Address;
        }
    }
   
}
