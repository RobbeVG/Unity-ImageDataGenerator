# Unity-ImageDataGenerator

ImageDataGenerator is an Unity-package that is used to generate image data. \
E.g. generating datasets for neural networks.

## Descritpion


## Installation
You can find the asset package [here]() under releases. The unity package is as easy to install as double clicking, opening it with your preferred Unity project and installing it. \
More information about why I choice an asset package is found in section [other](##Other)

## Features

### Main classes:
* **Annotation Generator** is the core for rendering or capturing image data. This script will execute the modifiers and capture the modified frame. Will then call the exporter to export the captured image.
* **Annotation Exporter** can export the image using different formats. Sets the name of the file according to the settings.
* **Annotation Object** is an object that can be detected by all annotation modifiers.
* **Annotation Object Manager** holds all the annotation objects. You can drag either choose either to include or exclude certain objects.
* **Annotation Camera** is the camera responsible for the rendering.
* **Annotation Modifier** is the base script for all modifiers. This script is derived from a serialized object so you can save your custom modifiers.

#### Current Profiles:
|Profiles|Description|
|---|---|
|Normal| Does no modifying only, capturing the current scene |
|Missing Texture| Changing the material of an [Annotation Object](####main-classes) to null if it is visible enough |
|Aliasing| Capturing the current scene without anti-aliasing |
|Stretched| Creating a stretched texture of an [Annotation Object](####main-classes) that is visible enough |
|Z-Fighting| Creating an duplicate object and rendering both object using an enhance Z-Fighting shader. Will only work if it is visible enough |

#### Current Modifiers:
|Modifier|Description|
|---|---|
|Camera| Changing the camera anti-aliasing level |
|Material| Changing the material of an [Annotation Object](####main-classes) |
|Timer| Waiting until set time has passed |
|Duplicate| Creating a duplicate of an [Annotation Object](####main-classes) |
|Visibility| Checking how visible an [Annotation Object](####main-classes) is |
|Manual| Manually taking an annotation of the current status |

## Usage

## License
[MIT](https://choosealicense.com/licenses/mit/)

## Other
