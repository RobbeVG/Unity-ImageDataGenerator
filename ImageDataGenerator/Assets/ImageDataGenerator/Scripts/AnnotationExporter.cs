using UnityEngine;
using System.IO;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using System;

[DisallowMultipleComponent]
public class AnnotationExporter : MonoBehaviour
{
    #region Enums
    private enum ImageOutputFormat
    {
        JPG,
        PNG,
        EXR,
        TGA
    }

    public enum Format
    {
        Text,
        Date,
        Time,
        SceneName
    }

    enum DateFlags
    {
        None = 0,
        Year = (1 << 0),
        Month = (1 << 1),
        Day = (1 << 2),
        FullDate = ~0
    }

    enum TimeFlags
    {
        None = 0,
        Hours = (1 << 0),
        Minutes = (1 << 1),
        Seconds = (1 << 2),
        FullTime = ~0
    }
    #endregion Enums

    [SerializeField]
    DateFlags dateFlag = DateFlags.FullDate;

    [SerializeField]
    TimeFlags timeFlag = TimeFlags.FullTime;

    [SerializeField]
    List<Format> formats = new List<Format>();

    [SerializeField]
    List<string> texts = null;

    [SerializeField]
    private string outputPath = "";

    [SerializeField]
    private ImageOutputFormat imageOutputFormat;

    public void AddFormatText(string text = "", int index = -1)
    {
        if (index < -1 || index >= formats.Count)
            index = formats.Count - 1;
        formats.Insert(index, Format.Text);
        texts.Insert(index, text);
    }

    public void RemoveFormatAt(int index = -1) 
    {
        if (index < -1 || index >= formats.Count)
            index = formats.Count - 1;
        formats.RemoveAt(index);
        texts.RemoveAt(index);
    }

    private void Awake()
    {
        if (outputPath.Length == 0) 
        {
            string path = Application.persistentDataPath;
            ;
            string directoryName = "AnnotationCaptures_" + DateTime.Now.ToString("MM-dd-yy hhmmss");
            uint counter = 0;

            if (Directory.Exists(path + directoryName))
            {
                while (Directory.Exists(path + directoryName + "_" + counter.ToString())) { counter++; }
                directoryName += "_" + counter.ToString();
            }

            outputPath = Path.Combine(path, directoryName);
            Directory.CreateDirectory(outputPath); // returns a DirectoryInfo object
        }

        if (formats.Count == 0) 
        {
            formats = new List<Format> { Format.Date, Format.Time };
            texts = new List<string>() { "", "" };
        }
    }

    private string GetFileName() 
    {
        System.DateTime dateTime = System.DateTime.Now;
        string fileName = "";
        List<int> paramFormat = new List<int>();

        for (int index = 0; index < formats.Count; index++)
        {
            switch (formats[index])
            {
                case Format.Text: //Adding text
                    fileName += texts[index];
                    break;

                case Format.Date: //Adding date - Rechecking the date every time -> Could be runned for over a day.
                    paramFormat.Clear();
                    if (dateFlag.HasFlag(DateFlags.Year))
                        paramFormat.Add(dateTime.Year);
                    if (dateFlag.HasFlag(DateFlags.Month))
                        paramFormat.Add(dateTime.Month);
                    if (dateFlag.HasFlag(DateFlags.Day))
                        paramFormat.Add(dateTime.Day);

                    fileName += GenerateFormat(texts[index], paramFormat);
                    break;

                case Format.Time: //Adding time
                    paramFormat.Clear();
                    if (timeFlag.HasFlag(TimeFlags.Hours))
                        paramFormat.Add(dateTime.Hour);
                    if (timeFlag.HasFlag(TimeFlags.Minutes))
                        paramFormat.Add(dateTime.Minute);
                    if (timeFlag.HasFlag(TimeFlags.Hours))
                        paramFormat.Add(dateTime.Second);

                    fileName += GenerateFormat(texts[index], paramFormat);
                    break;

                case Format.SceneName: //Adding scene name
                    fileName += SceneManager.GetActiveScene().name; //Is also set by custom editor but could change during runtime!
                    break;
                default:
                    break;
            }
        }
        return fileName;
    }

    private string GetFileExtension()
    {
        string fileExtension;
        switch (imageOutputFormat)
        {
            case ImageOutputFormat.JPG:
                fileExtension = ".jpg";
                break;
            case ImageOutputFormat.PNG:
                fileExtension = ".png";
                break;
            case ImageOutputFormat.EXR:
                fileExtension = ".exr";
                break;
            case ImageOutputFormat.TGA:
                fileExtension = ".tga";
                break;
            default:
                fileExtension = ".txt";
                break;
        }
        return fileExtension;
    }

    private string GenerateFormat(string delimiter, List<int> parametersFormat)
    {
        //string format = "";
        //for (int count = 0; count < parametersFormat.Count; count++)
        //{
        //    format += "{" + count.ToString() + "}";
        //    if (count != parametersFormat.Count - 1)
        //        format += delimiter;
        //}

        //return string.Format(format, parametersFormat.ToArray()); //Kept throwing errors...

        string format = "";
        for (int index = 0; index < parametersFormat.Count; index++)
        {
            format += parametersFormat[index];
            if (index != parametersFormat.Count - 1)
                format += delimiter;
        }
        return format;
    }

    public void Export(AnnotationCamera annotationCam)
    {
        string currentfilename = GetFileName();
        string fileExtension = GetFileExtension();

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
        switch (imageOutputFormat)
        {
            case ImageOutputFormat.JPG:
                bytes = image.EncodeToJPG();
                break;
            case ImageOutputFormat.PNG:
                bytes = image.EncodeToPNG();
                break;
            case ImageOutputFormat.EXR:
                bytes = image.EncodeToEXR();
                break;
            case ImageOutputFormat.TGA:
                bytes = image.EncodeToTGA();
                break;
            default:
                bytes = new byte[] { };
                break;
        }

        // destroy texture2D
        Destroy(image);

        // Write encoded data to a file in the project folder

        string path = string.Format("{0}/{1}", outputPath, currentfilename + fileExtension);

        uint counter = 0;
        while (File.Exists(path)) 
        {
            path = string.Format("{0}/{1}", outputPath, currentfilename + "_" + counter.ToString() + fileExtension);
            counter++;
        }
        File.WriteAllBytes(path, bytes);
    }
}
