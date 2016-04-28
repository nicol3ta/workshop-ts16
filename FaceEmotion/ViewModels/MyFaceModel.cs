using Microsoft.ProjectOxford.Face.Contract;

namespace FaceEmotion.ViewModels
{
    class MyFaceModel
    {
        public object Age { get; internal set; }
        public string FaceId { get; internal set; }
        public FaceRectangle FaceRect { get; internal set; }
        public object Gender { get; internal set; }
    }
}
