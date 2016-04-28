using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.Storage.Pickers;
using System.Diagnostics;
using FaceEmotion.ViewModels;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI;
using Windows.UI.Xaml.Shapes;
// Use the following namespace for FaceServiceClient
using Microsoft.ProjectOxford.Face;
// Use the following namespace for EmotionServiceClient 
using Microsoft.ProjectOxford.Emotion;
using Microsoft.ProjectOxford.Emotion.Contract;
using System.Linq;

namespace FaceEmotion
{
    /// <summary>
    /// Eine leere Seite, die eigenständig verwendet oder zu der innerhalb eines Rahmens navigiert werden kann.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        //Create Project Oxford Face API Service client
        private readonly FaceServiceClient faceServiceClient = new FaceServiceClient("897c6563d3d54d418a11ffb84fa68e32");
        //Create Project Oxford Emotion API Service client
        private readonly EmotionServiceClient emotionServiceClient = new EmotionServiceClient("7bd4c9b319ab4adbbaa0495de7a1d6a8");

        private double scale;

        public MainPage()
        {
            this.InitializeComponent();
        }

        private async void BrowseButton_Click(object sender, RoutedEventArgs e)
        {

            //Upload picture 
            FileOpenPicker openPicker = new FileOpenPicker();
            openPicker.ViewMode = PickerViewMode.Thumbnail;
            openPicker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;
            openPicker.FileTypeFilter.Add(".jpg");
            openPicker.FileTypeFilter.Add(".jpeg");
            openPicker.FileTypeFilter.Add(".png");

            StorageFile file = await openPicker.PickSingleFileAsync();
            var bitmapImage = new BitmapImage();

            var stream = await file.OpenAsync(FileAccessMode.Read);
            bitmapImage.SetSource(stream);

            if (file != null)
            {
                FacePhoto.Source = bitmapImage;
                scale = FacePhoto.Width / bitmapImage.PixelWidth;
            }
            else
            {
                Debug.WriteLine("Operation cancelled.");
            }

            //Remove any existing rectangles from previous events 
            FacesCanvas.Children.Clear();

            //DetectFaces
            var s = await file.OpenAsync(FileAccessMode.Read);
            List<MyFaceModel> faces = await DetectFaces(s.AsStream());
            var t = await file.OpenAsync(FileAccessMode.Read);
            //Detect emotions
            Emotion[] emotions = await DetectEmotions(t.AsStream());
            DrawFaces(faces, emotions);
        }

        private async Task<List<MyFaceModel>> DetectFaces(Stream imageStream)
        {
            var collection = new List<MyFaceModel>();
            var attributes = new List<FaceAttributeType>();
            attributes.Add(FaceAttributeType.Gender);
            attributes.Add(FaceAttributeType.Age);

            try
            {
                using (var s = imageStream)
                {
                    var faces = await faceServiceClient.DetectAsync(s, true, true, attributes);


                    foreach (var face in faces)
                    {
                        collection.Add(new MyFaceModel()
                        {
                            FaceId = face.FaceId.ToString(),
                            FaceRect = face.FaceRectangle,
                            Gender = face.FaceAttributes.Gender,
                            Age = face.FaceAttributes.Age
                        });
                    }
                }
            }
            catch (InvalidOperationException e)
            {
                Debug.WriteLine("Error Message: " + e.Message);
            }
            return collection;
        }
        private async Task<Emotion[]> DetectEmotions(Stream imageStream)
        {
            try
            {
                Emotion[] emotionResult = await emotionServiceClient.RecognizeAsync(imageStream);
                return emotionResult;
            }          
            catch (Exception exception)
            {
                Debug.WriteLine(exception.ToString());
                return null;
            }
        }
        /// <summary>
        /// Highlight the faces and display estimated age, gender and emotion.
        /// </summary>
        /// <param name="faces">The faces detected in the image</param>
        /// <param name="emotions">The emotions detected in the image</param>
        private void DrawFaces(List<MyFaceModel> faces, Emotion[] emotions)
        {
            //Check if the are any faces in this image
            if (faces != null)
            {
                //For each detected face 
                for (int i = 0; i < faces.Count; i++)
                {
                    Debug.WriteLine("Age: " + faces[i].Age);
                    Debug.WriteLine("Gender: " + faces[i].Gender);
                    Debug.WriteLine("Emotion: " + emotions[i].Scores.Happiness);
                    Rectangle faceBoundingBox = new Rectangle();

                    //Set bounding box stroke properties 
                    faceBoundingBox.StrokeThickness = 3;

                    //Highlight the first face in the set 
                    faceBoundingBox.Stroke = (i == 0 ? new SolidColorBrush(Colors.White) : new SolidColorBrush(Colors.DeepSkyBlue));
                    if (scale == 0)
                    {
                        scale = 1;
                    }
                    faceBoundingBox.Margin = new Thickness(faces[i].FaceRect.Left * scale, faces[i].FaceRect.Top * scale, faces[i].FaceRect.Height * scale, faces[i].FaceRect.Width * scale);
                    faceBoundingBox.Width = faces[i].FaceRect.Width * scale;
                    faceBoundingBox.Height = faces[i].FaceRect.Height * scale;
                    TextBlock age = new TextBlock();
                    var predictedAge = Convert.ToInt32(faces[i].Age);

                    Dictionary<float, string> detectedEmotion = GetEmotions(emotions[i]);

                    // Acquire keys and sort them.
                    var list = detectedEmotion.Keys.ToList();
                    list.Sort();

                    // Loop through keys.
                    //foreach (var key in list)
                    //{
                    //    Debug.WriteLine("{0}: {1}", key, detectedEmotion[key]);
                    //}


                    age.Text = faces[i].Gender + ", " + predictedAge + ", " + detectedEmotion[list.Last()];
                    age.Margin = new Thickness(faces[i].FaceRect.Left * scale, (faces[i].FaceRect.Top * scale) - 20, faces[i].FaceRect.Height * scale, faces[i].FaceRect.Width * scale);
                    age.Foreground = (i == 0 ? new SolidColorBrush(Colors.White) : new SolidColorBrush(Colors.DeepSkyBlue));
                    //Add grid to canvas containing all face UI objects 
                    FacesCanvas.Children.Add(faceBoundingBox);
                    FacesCanvas.Children.Add(age);
                }
            }
            else
            {
                Debug.WriteLine("No faces identified");
            }
        }

        private Dictionary<float,string> GetEmotions(Emotion emotion)
        {
            var emotions = new Dictionary<float, string>();
            emotions.Add(emotion.Scores.Happiness, "happy");
            emotions.Add(emotion.Scores.Neutral, "neutral");
            emotions.Add(emotion.Scores.Sadness, "sad");
            emotions.Add(emotion.Scores.Surprise, "surprised");
            emotions.Add(emotion.Scores.Fear, "scared");
            emotions.Add(emotion.Scores.Disgust, "disgusted");
            emotions.Add(emotion.Scores.Contempt, "contemptuous");
            emotions.Add(emotion.Scores.Anger, "angry");
            
            return emotions;
        }


    }
}
