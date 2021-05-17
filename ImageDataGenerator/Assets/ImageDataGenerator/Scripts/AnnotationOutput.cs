using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

[CreateAssetMenu(fileName = "AnnotationOutput", menuName = "AnnotationSystem/Output")]
public class AnnotationOutput : ScriptableObject
{
    #region Enums
    public enum ImageFormat
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
    private ImageFormat imageOutputFormat = ImageFormat.PNG;

    [SerializeField]
    private string outputPath = "";

    public string Filename { get { return GetFileBaseName() + GetFileExtension(); } }
    public string Path { get { return outputPath; } }
    public ImageFormat ImageFormatValue { get {return imageOutputFormat; }}


    private void OnEnable()
    {
        if (string.IsNullOrEmpty(outputPath)) 
        {
            string path = Application.persistentDataPath;
            string directoryName = "AnnotationCaptures_" + DateTime.Now.ToString("MM-dd-yy hhmmss");

            uint counter = 0;

            if (Directory.Exists(path + directoryName))
            {
                while (Directory.Exists(path + directoryName + "_" + counter.ToString())) { counter++; }
                directoryName += "_" + counter.ToString();
            }
            outputPath = System.IO.Path.Combine(path, directoryName);
            Directory.CreateDirectory(outputPath); // returns a DirectoryInfo object
        }

        if (formats.Count == 0)
        {
            formats = new List<Format> { Format.Date, Format.Time };
            texts = new List<string>() { "", "" };
        }
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

    public string GetFileBaseName()
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

    public string GetFileExtension()
    {
        string fileExtension;
        switch (imageOutputFormat)
        {
            case ImageFormat.JPG:
                fileExtension = ".jpg";
                break;
            case ImageFormat.PNG:
                fileExtension = ".png";
                break;
            case ImageFormat.EXR:
                fileExtension = ".exr";
                break;
            case ImageFormat.TGA:
                fileExtension = ".tga";
                break;
            default:
                fileExtension = ".txt";
                break;
        }
        return fileExtension;
    }
}
