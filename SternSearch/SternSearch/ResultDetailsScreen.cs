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
    [Activity(Label = "ResultDetailsScreen")]
    public class ResultDetailsScreen : Activity
    {

        List<string> result;

        ListView resultList;
        ArrayAdapter myListAdapter;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.ResultDetailsScreen);

            resultList = FindViewById<ListView>(Resource.Id.searchResultView);
            result = new List<string>() { "no data yet ..." };

            myListAdapter = new ArrayAdapter(this, Android.Resource.Layout.SimpleListItem1, result);
            resultList.Adapter = myListAdapter;
            resultList.ItemClick += (s, e) => {
                //if (e.Position <= result.Count)
                //{
                //    var t = result[e.Position];
                //    Android.Widget.Toast.MakeText(this, t, Android.Widget.ToastLength.Long).Show();
                //}
            };

            myListAdapter.Clear();

            var receivedData = Intent.GetStringArrayListExtra("data");
            result = new List<string>(receivedData);
            myListAdapter.AddAll(result);
            RunOnUiThread(() => myListAdapter.NotifyDataSetChanged());

        }
    
    }


}
