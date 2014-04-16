using System;
using System.IO;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.Provider;
using Android.Widget;
using Android.OS;
using Xamarin.Media;
using Console = System.Console;
using Uri = Android.Net.Uri;

namespace FlashCastPhotoApp
{
  [Activity(Label = "FlashCastPhotoApp", MainLauncher = true, Icon = "@drawable/icon")]
  public class Activity1 : Activity
  {
    private ImageView picture;

    protected override void OnCreate(Bundle bundle)
    {
      base.OnCreate(bundle);

      SetContentView(Resource.Layout.Main);

      var buttonTakePhoto = FindViewById<Button>(Resource.Id.button_take_picture);
      var buttonSharePhoto = FindViewById<Button>(Resource.Id.button_share);
      var buttonPickPhoto = FindViewById<Button>(Resource.Id.button_pick_photo);

      picture = FindViewById<ImageView>(Resource.Id.picture);

      buttonTakePhoto.Click += (sender, args) => TakePhoto();

      buttonSharePhoto.Click += (sender, args) => SharePhoto();

      buttonPickPhoto.Click += (sender, args) => PickPhoto();
    }

    private void PickPhoto()
    {
      var picker = new MediaPicker(this);
      if (!picker.PhotosSupported)
      {
        Console.WriteLine("Photo's aren't supported");
        return;
      }
      

      var intent = picker.GetPickPhotoUI();

      StartActivityForResult(intent, 1);
    }

    private void TakePhoto()
    {
      var picker = new MediaPicker(this);
      if (!picker.IsCameraAvailable)
      {
        Console.WriteLine("No Camera :(");
        return;
      }

      var options = new StoreCameraMediaOptions
      {
        Name = "temp.jpg",
        Directory = "flashcast"
      };

      var intent = picker.GetTakePhotoUI(options);

      StartActivityForResult(intent, 1);
    }

    private void SharePhoto()
    {
      var share = new Intent(Intent.ActionSend);
      share.SetType("image/jpeg");

      var values = new ContentValues();
      values.Put(MediaStore.Images.ImageColumns.Title, "title");
      values.Put(MediaStore.Images.ImageColumns.MimeType, "image/jpeg");
      Uri uri = ContentResolver.Insert(MediaStore.Images.Media.ExternalContentUri, values);


      Stream outstream;
      try
      {
        
        outstream = ContentResolver.OpenOutputStream(uri);
        finalBitmap.Compress(Bitmap.CompressFormat.Jpeg, 100, outstream);
        outstream.Close();
      }
      catch (Exception e)
      {

      }
      share.PutExtra(Intent.ExtraStream, uri);
      share.PutExtra(Intent.ExtraText, "Sharing some images from android!");
      StartActivity(Intent.CreateChooser(share, "Share Image"));
    }

    private Bitmap finalBitmap;
    protected async override void OnActivityResult(int requestCode, Result resultCode, Intent data)
    {
      if (resultCode == Result.Canceled)
        return;

      
      var mediaFile = await data.GetMediaFileExtraAsync(this);

      var drawable = await BitmapDrawable.CreateFromStreamAsync(mediaFile.GetStream(), "temp.jpg");
      var bitmap = ((BitmapDrawable) drawable).Bitmap;

      await Task.Factory.StartNew(() =>
      {
        
        finalBitmap = Bitmap.CreateScaledBitmap(bitmap, bitmap.Width/4, bitmap.Height/4, true);
        
        RunOnUiThread(() => {
                              picture.SetImageBitmap(finalBitmap);
        });
      });
    }
  }
}

