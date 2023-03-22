# <img src="https://user-images.githubusercontent.com/62036141/227040593-70997ac6-f6a8-4d9f-97dc-1a24e9c6c28a.png" width="25" height="25"/> ImgBWObfus
C# class for encoding/decoding text/files to/from black and white pixelated images with optional obfuscation.

## Description
This is a C# class that provides methods for **encoding** text or files into black and white pixeled images. The images produced can be **obfuscated** by adding a salt combination to the binary string that is generated from the input data. This allows a private key to protect against unauthorized deobfuscation.

This C# class also provides methods for **decoding** the generated black and white images, by inserting a png file or a bitmap, to their original text or in their original file. _**NOTE:** If the file or text got obfuscated with a salt combination, the text or file cannot be recovered without this salt combination!_

## Getting Started
### Prerequisites
- A C# project to which you want to add this class.

### Installation
- Download ImgBWObfus.cs to your project folder.
- Or copy all code from ImgBWObfus.cs to your project.

Now you can call all methods with **ImgBWObfus.\[METHOD\](\[PARAMS\]\)**.

## How to use ImgBWObfus.cs?
_Call the methods in your project with **ImgBWObfus.\[METHOD\]\(\[PARAMS\]\)**_

### Following Methods are available:
**Load Png**
- LoadPngFromFolder(string imagePath)
- LoadPngsFromFolder(string folderPath, string baseFileName)

**Encoding**
- TextToImage(string text, string outputImagePath = null)
- TextToImage(string text, int[] salt, string outputImagePath = null)
- TextToImage(string text, int width = 1920, int height = 1080, string outputImagePath = null)
- TextToImage(string text, int[] salt, int width = 1920, int height = 1080, string outputImagePath = null)

- FileToImage(string filePath, string outputImagePath = null)
- FileToImage(string filePath, int[] salt, string outputImagePath = null)
- FileToImage(string filePath, int width = 1920, int height = 1080, string outputImagePath = null)
- FileToImage(string filePath, int[] salt, int width = 1920, int height = 1080, string outputImagePath = null)

**Decoding**
- ImageToText(Bitmap bitmap)
- ImageToText(Bitmap bitmap, int[] salt)
- ImageToText(List<Bitmap> bitmapList)
- ImageToText(List<Bitmap> bitmapList, int[] salt)

- ImageToFile(Bitmap bitmap, string outputFilePath)
- ImageToFile(Bitmap bitmap, int[] salt, string outputFilePath)
- ImageToFile(List<Bitmap> bitmap, string outputFileName)
- ImageToFile(List<Bitmap> bitmap, int[] salt, string outputFilePath)

### Load Png
**Single load:**
```
Bitmap bitmap = ImgBWObfus.LoadPngFromFolder("Hello_World_Image.png");
```

**Multi load:**
```
List<Bitmap> bitmapList = ImgBWObfus.LoadPngsFromFolder(Directory.GetCurrentDirectory(), "Images");
```

### Text example:
**Encoding:**
```
Bitmap bitmap = ImgBWObfus.TextToImage("Hello World!", "Hello_World_Image");
```
Output:
<img src="https://user-images.githubusercontent.com/62036141/227019693-05563455-6dff-40f0-95ed-136d09f3ad0b.png"/>

**Decoding:**
```
string decodedText = ImgBWObfus.ImageToText(bitmap);
Console.WriteLine(decodedText);
```
Output:
> Hello World!
<br>

### File example:
**Encoding:**
```
Bitmap bitmap = ImgBWObfus.FileToImage("Input_Image.png", "Obfuscated_Image");
```
Output:
<br>
<img src="https://user-images.githubusercontent.com/62036141/227021428-bd1673d2-9513-438d-86a7-2ce24a443fb1.png" width="100" height="100"/>

**Decoding:**
```
ImgBWObfus.ImageToFile(bitmap, "Deobfuscated_Image.png");
```
Output:
<br>
<img src="https://user-images.githubusercontent.com/62036141/227040593-70997ac6-f6a8-4d9f-97dc-1a24e9c6c28a.png" width="100" height="100"/>

## Licence
This project is licensed under the MIT License - see the [LICENSE.md](LICENSE) file for details
