# face-detection
This sample app provides an example on how to use the Face and Emotion APIs from Cognitive Services 

## Step-by-step - How to add face and emotion detection to a UWP app

* If you haven't already, install Visual Studio 2015. 
In Visual Studio, on the File menu, click New Project.
In the Installed Templates list, select C# as your programming language and choose the Blank Application template.
Name the Project and press enter.

* First let's create the interface. In MainPage.xaml replace the automatically generated Gris with this RelativePanel.

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

As you can see we have a simple interface with a banner and a button for choosing a picture. 
For the banner we need the MicrosoftLogo. Simply create a new folder, call it "Images", and copy there the microsoftLogo.png, which you can find in this repo.
