using Android.App;
using Android.Widget;
using Android.OS;
using Android.Content;
using Android.Net;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using System.Net.Http;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Text;
using System.Data;
using System;

namespace SternSearch
{
    [Activity(Label = "SternSearch", MainLauncher = true, Icon = "@mipmap/icon")]
    public class MainActivity : Activity
    {
        private string currentSearchTerm = "";
        List<List<String>> currentSearchResults;


        private HttpClient httpClient;

        List<string> allSearchResults;

        TextView resultLabel;
        ListView resultList;
        ArrayAdapter myListAdapter;

        public Dictionary<string, object> Dic { get; private set; }

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Main);

            currentSearchResults = new List<List<string>>();

            httpClient = new HttpClient();
            string api_key = "7948644A316B48A1F037F04915E4BA3A";
            httpClient.DefaultRequestHeaders.Add("api-key", api_key);

            // Get our button from the layout resource,
            // and attach an event to it
            Button searchButton = FindViewById<Button>(Resource.Id.searchButton);
            EditText searchTextfield = FindViewById<EditText>(Resource.Id.searchTextField);
            resultLabel = FindViewById<TextView>(Resource.Id.resultLabel);
            resultList = FindViewById<ListView>(Resource.Id.listView);

            searchButton.Click += delegate
            {
                currentSearchTerm = searchTextfield.Text;
                StartSearch();
            };

            allSearchResults = new List<String> { "no results yet..." };
            myListAdapter = new ArrayAdapter(this, Android.Resource.Layout.SimpleListItem1, allSearchResults);
            resultList.Adapter = myListAdapter;
            resultList.ItemClick += (s, e) =>
            {
                Intent intent = new Intent(this, typeof(ResultDetailsScreen));
                var dataToSend = currentSearchResults[e.Position];
                intent.PutStringArrayListExtra("data", dataToSend);
                StartActivity(intent);
            };
        }

        private void saveSearchResult (List<string> result) {
            currentSearchResults.Add(result);

            Console.WriteLine("currently added: " + result);
        }


        private async void StartSearch()
        {
            myListAdapter.Clear();

            string Base_URL = "https://<yourServiceName>.search.windows.net";
            string Index_URL = "/indexes/azureblob-index/docs";
            string Query = "?api-version=2017-11-11&search=";

            string stringURL = Base_URL + Index_URL + Query + currentSearchTerm;

            string content = await httpClient.GetStringAsync(stringURL);

            Dictionary<string, object> procContent = JsonConvert.DeserializeObject<Dictionary<string, object>>(content);

            foreach (var item in procContent)
            {
                if (item.Key == "value")
                {
                    resultLabel.Text = "Results for " + currentSearchTerm;
                    List<string> persistResults = new List<string>();

                    foreach (var result in (JArray)item.Value)
                    {
                        persistResults.Clear();

                        string storage_path = result["metadata_storage_path"].ToString();
                        string titleValPrefix = storage_path.Substring(0, 15);
                        string titleValPostfix = storage_path.Substring(storage_path.Length - 15, 15);
                        myListAdapter.Add(titleValPrefix + "..." + titleValPostfix);

                        var keyPhrasesField = result["keyphrases"];
                        persistResults.Add(keyPhrasesField.ToString());

                        var imagetagField = result["imageTags"];
                        persistResults.Add(imagetagField.ToString());

                        var imageCaptionField = result["imageCaption"];
                        persistResults.Add(imageCaptionField.ToString());

                        this.saveSearchResult(persistResults);
                    }
                }
            }

            myListAdapter.NotifyDataSetChanged();
        }

    }
}

