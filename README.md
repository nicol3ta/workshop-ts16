# Workshop Cognitive Services and Microsoft Bot Framework - Technical Summit 2016
This sample app provides an example on how to use the Face and Emotion APIs from Cognitive Services 

## Step-by-Step - How to Add Face and Emotion Detection to a UWP App

* If you haven't already, install Visual Studio 2015. 
* In Visual Studio, on the **File** menu, click **New Project**. In the **Installed Templates** list, select **C#** as your programming language and choose the **Blank Application** template. Name the project as you wish and press enter to create it.

* First let's create the interface. In **MainPage.xaml** replace the automatically generated Grid with this RelativePanel.

```csharp
   <RelativePanel Background="{ThemeResource ApplicationPageBackgroundThemeBrush}">

        <Grid Height="90" x:Name="LogoStackPanel"
              Background="#0078D7"
             RelativePanel.AlignRightWithPanel="True"
             RelativePanel.AlignLeftWithPanel="True">

            <StackPanel Orientation="Vertical"
                        HorizontalAlignment="Left"
                        Margin="20,4,0,0">
                <Image Height="24"
                       Margin="2,0,0,0"
                       HorizontalAlignment="Left"
                       Source="Images/microsoftLogo.png" />
                <TextBlock Text="Cognitive Serivces"
                           FontSize="26"
                           Foreground="WhiteSmoke"></TextBlock>
                <TextBlock Text="Face and Emotion Detection"
                           FontSize="18"
                           Foreground="WhiteSmoke"></TextBlock>
            </StackPanel>
        </Grid>
        <Grid RelativePanel.AlignLeftWith="LogoStackPanel"
              RelativePanel.Below="LogoStackPanel">
           
            <Image x:Name="FacePhoto"
                   Width="600"
                   Height="800"
                   Margin="20,10,40,44"
                   Stretch="Uniform"
                   VerticalAlignment="Top"
                   HorizontalAlignment="Left" />
            
            <Canvas Name="FacesCanvas"
                    Margin="20,10,40,44" />
            
            <Button x:Name="BrowseButton"
                Margin="20,5,0,10"
                Height="32"
                VerticalAlignment="Bottom"
                Content="Browse..."
                Click="BrowseButton_Click" />
        </Grid>
    </RelativePanel>
```

* As you can see we have a simple interface with a banner and a button for choosing a picture. 
For the banner we need the Microsoft logo. Simply create a new folder, call it **Images**, and copy there the **microsoftLogo.png**, which you can find in this repo.

* Let's add now the **FaceDetection** client libray to our project. In Solution Explorer, right click on **References** and choose **Manage NuGet Packages**. 
Click the **Browse** tab, search for **Microsoft.Project.Oxford.Face**, select that package in the list, and click **Install**.

* In **MainPage.xaml.cs** reference the library 
```csharp
using Microsoft.ProjectOxford.Face;
```

* Create a new folder and name it **ViewModels**. Right click on it and add a **New Item**. Choose **Class** from the list and name it **MyFaceModel.cs**. In the newly created class reference again the **Microsoft.ProjectOxford.Face** library and add the following Face attributes:

```csharp
class MyFaceModel
{
        public object Age { get; internal set; }
        public string FaceId { get; internal set; }
        public FaceRectangle FaceRect { get; internal set; }
        public object Gender { get; internal set; }
}
```
* Back in the **MainWindow** class insert the following code: 
```csharp 
private readonly IFaceServiceClient faceServiceClient = new FaceServiceClient("Your subscription key"); 
``` 
<**Note** Please set the subscription key from your account. You can sign up [here](https://www.microsoft.com/cognitive-services/en-us/sign-up)

* Insert the following code inside the **MainWindow** class for the **Browse** button 

```csharp

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

            //TODO DetectFaces
        }
```

* We also need to add a variable for scaling the image so that it is displayed correctly.

```csharp
private double scale;
```

* Now we can create the method **DetectFaces**. The most straightforward way to detect faces is by calling the Face - Detect API by uploading the image file directly. When using the client library, this can be done by using an asynchronous method **DetectAsync** of **FaceServiceClient**. 

```csharp
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
```
* Let's replace now the **//TODO** comment in the **BrowseButton_Click** method with the following code snippet. We're basically calling the **DetectFaces** and the **DrawFaces** methods after the user chose the picture to analyse.

```csharp
//Remove any existing rectangles from previous events 
FacesCanvas.Children.Clear();

//DetectFaces
var s = await file.OpenAsync(FileAccessMode.Read);
List<MyFaceModel> faces = await DetectFaces(s.AsStream());
var t = await file.OpenAsync(FileAccessMode.Read);
DrawFaces(faces);
```

* We can now create the method **DrawFaces** to mark the faces in the image

```csharp
        /// <summary>
        /// Highlight the faces and display estimated age, gender and emotion.
        /// </summary>
        /// <param name="faces">The faces detected in the image</param>
        /// <param name="emotions">The emotions detected in the image</param>
        private void DrawFaces(List<MyFaceModel> faces)
        {
            //Check if the are any faces in this image
            if (faces != null)
            {
                //For each detected face 
                for (int i = 0; i < faces.Count; i++)
                {
                    Debug.WriteLine("Age: " + faces[i].Age);
                    Debug.WriteLine("Gender: " + faces[i].Gender);
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

                    age.Text = faces[i].Gender + ", " + predictedAge;
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

```

* We can run the program and test it. We are able now to browse for a photo and display it in the window. The faces in the picture are automatically detected and marked.

* Next step is to detect also the emotions of the people in this photo.  

* First of all we need to add the NuGet package for **Emotion** to the project and add the reference to it in the **MainWindow** class.

```csharp
using Microsoft.ProjectOxford.Emotion.Contract;
``` 

* Insert the following code in the **MainWindow** class and replace the text with your subscription key. 
```csharp
private readonly EmotionServiceClient emotionServiceClient = new EmotionServiceClient("Your subscription key");  
``` 

* We add a method **DetectEmotions** 
```csharp
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
``` 

* We only want the emotion with the highest probability, that's why we implement a method **GetEmotion** which returns the most likely identified emotion.
```csharp
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
``` 

* Let's modify the **DrawFaces** method so it also displays the recognized emotions.

```csharp
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
``` 

* Last step, we need to call **DetectEmotions** in the **BrowseButton_Click** method.

```csharp
//Detect emotions
Emotion[] emotions = await DetectEmotions(t.AsStream());
DrawFaces(faces, emotions);
``` 
       
