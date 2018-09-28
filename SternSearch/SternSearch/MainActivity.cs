using Android.App;
using Android.Widget;
using Android.OS;
using Android.Content;
using Android.Net;
using Android.Views.InputMethods;
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

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Main);

            currentSearchResults = new List<List<string>>();

            httpClient = new HttpClient();
            string api_key = "xxx";
            httpClient.DefaultRequestHeaders.Add("api-key", api_key);

            // Get our button from the layout resource,
            // and attach an event to it
            Button searchButton = FindViewById<Button>(Resource.Id.searchButton);
            EditText searchTextfield = FindViewById<EditText>(Resource.Id.searchTextField);
            resultLabel = FindViewById<TextView>(Resource.Id.resultLabel);
            resultList = FindViewById<ListView>(Resource.Id.listView);

            resultLabel.Visibility = Android.Views.ViewStates.Invisible;
            resultList.Visibility = Android.Views.ViewStates.Invisible;

            searchButton.Click += delegate
            {
                currentSearchResults.Clear();
                myListAdapter.Clear();
                currentSearchTerm = searchTextfield.Text;

                StartSearch();

                InputMethodManager imm = (InputMethodManager)GetSystemService(Context.InputMethodService);
                imm.HideSoftInputFromWindow(searchTextfield.WindowToken, 0);
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

        private string GetDisplayTextForRawList(string data, string title) {
            List<String> keyPhrases = JsonConvert.DeserializeObject<List<String>>(data.ToString());
            int count = 0;
            string displayString = "\n" + title + "\n\n";
            foreach (string kp in keyPhrases)
            {
                if (count != 0)
                {
                    displayString = displayString + ", " + kp;
                }
                else
                {
                    displayString = displayString + kp;
                }

                count++;
            }

            return displayString + "\n";
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
                    resultLabel.Visibility = Android.Views.ViewStates.Visible;
                    resultList.Visibility = Android.Views.ViewStates.Visible;

                    resultLabel.Text = ((JArray)item.Value).Count + " results for " + currentSearchTerm;
                    List<string> persistResults = new List<string>();

                    if (item.Value == null || ((JArray)item.Value).Count == 0)
                    {
                        myListAdapter.Clear();
                        myListAdapter.Add("no results found :(");
                        myListAdapter.NotifyDataSetChanged();
                        return;
                    }

                    foreach (var result in (JArray)item.Value)
                    {
                        persistResults.Clear();

                        string storage_path = result["metadata_storage_path"].ToString();
                        string titleValPrefix = storage_path.Substring(0, 15);
                        string titleValPostfix = storage_path.Substring(storage_path.Length - 15, 15);
                        myListAdapter.Add(titleValPrefix + "..." + titleValPostfix);

                        var keyPhrasesField = result["keyphrases"];
                        string text = this.GetDisplayTextForRawList(keyPhrasesField.ToString(), "Keyphrases: ");
                        persistResults.Add(text);

                        var imagetagField = result["imageTags"];
                        text = this.GetDisplayTextForRawList(imagetagField.ToString(), "Image Tags: ");
                        persistResults.Add(text);

                        var peopleField = result["people"];
                        text = this.GetDisplayTextForRawList(peopleField.ToString(), "People: ");
                        persistResults.Add(text);

                        var locationsField = result["locations"];
                        text = this.GetDisplayTextForRawList(locationsField.ToString(), "Locations: ");
                        persistResults.Add(text);


                        var imageCaptionField = result["imageCaption"];
                        List<object> imageCaptions = JsonConvert.DeserializeObject<List<object>>(imageCaptionField.ToString());
                        string imageCaptionString = "\nImage Captions: \n";

                        foreach (var dict in imageCaptions)
                        {
                            Dictionary<string, object> realDict = JsonConvert.DeserializeObject<Dictionary<string, object>>(dict.ToString());
                            var captions = realDict["captions"];

                            List<Dictionary<string, object>> captionVals = JsonConvert.DeserializeObject<List<Dictionary<string, object>>>(captions.ToString());
                            if (captionVals != null && captionVals.Count > 0)
                            {
                                var caption = captionVals[0];
                                var captionText = caption["text"];

                                imageCaptionString = imageCaptionString + "\n -> " + captionText;
                            }

                        }

                        persistResults.Add(imageCaptionString + "\n");

                        this.saveSearchResult(persistResults);
                    }
                }
            }

            myListAdapter.NotifyDataSetChanged();
        }

    }
}

