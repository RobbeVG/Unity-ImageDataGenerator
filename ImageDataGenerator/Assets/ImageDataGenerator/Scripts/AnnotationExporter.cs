using UnityEngine;
using System.IO;

public class AnnotationExporter
{
    public void Export(AnnotationCamera annotationCam, AnnotationOutput output)
    {
        // The Render Texture in RenderTexture.active is the one
        // that will be read by ReadPixels.
        RenderTexture target = annotationCam.Component.targetTexture;

        // Store original render texture, and set input as acitve.
        RenderTexture temp = RenderTexture.active;
        RenderTexture.active = target;

        // Make a new texture and read the active Render Texture into it.
        Texture2D image = new Texture2D(target.width, target.height, TextureFormat.RGB24, false);
        image.ReadPixels(new Rect(0, 0, target.width, target.height), 0, 0);
        image.Apply();

        // Set active render texture back to original
        RenderTexture.active = temp;

        byte[] bytes;
        switch (output.ImageFormatValue)
        {
            case AnnotationOutput.ImageFormat.JPG:
                bytes = image.EncodeToJPG();
                break;
            case AnnotationOutput.ImageFormat.PNG:
                bytes = image.EncodeToPNG();
                break;
            case AnnotationOutput.ImageFormat.EXR:
                bytes = image.EncodeToEXR();
                break;
            case AnnotationOutput.ImageFormat.TGA:
                bytes = image.EncodeToTGA();
                break;
            default:
                bytes = new byte[] { };
                break;
        }


        // destroy texture2D
        UnityEngine.Object.Destroy(image);

        // Write encoded data to a file in the project folder

        if (!Directory.Exists(output.Path)) 
            Directory.CreateDirectory(output.Path);

        string path = string.Format("{0}/{1}", output.Path, output.Filename);
        uint counter = 0;
        while (File.Exists(path)) 
        {
            path = string.Format("{0}/{1}", output.Path, output.GetFileBaseName() + "_" + counter.ToString() + output.GetFileExtension());
            counter++;
        }
        File.WriteAllBytes(path, bytes);
    }
}
