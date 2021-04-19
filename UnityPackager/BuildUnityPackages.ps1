$runtimeFiles = Get-ChildItem -Path "../ImageDataGenerator/Assets/ImageDataGenerator/*"  -Recurse -File -Force
$runtimeFiles = $runtimeFiles.FullName.Replace("\", "/")

$editorFiles = Get-ChildItem -Path "../ImageDataGenerator/Assets/Editor/ImageDataGenerator/*.cs" -Recurse -File
$editorFiles = $editorFiles.FullName.Replace("\", "/")

Write-Host $editorFiles.Count


# Check if any file contains a space! UnityPackager.exe does not allow spaces in file names
$allFiles = $runtimeFiles + $editorFiles
$checkSuccessful = $true
For ($i=0; $i -lt $allFiles.Count; $i++)
{
    if ($allFiles.Get($i) -match '\s')
    {
      Write-Host $allFiles.Get($i)
      Write-Error -Message "Error: Filename contains a space." -TargetObject $allFiles.Get($i) -Category SyntaxError
      $checkSuccessful = $false
    }
}

if ($checkSuccessful -eq $true)
{
  $myPath = (Get-Item -Path ".\").FullName;

  $destination = Split-Path -Path $myPath -Parent

  $destination = $destination.Replace("\", "/")
  Write-Host $destination


  $runtimeBasePath = -join ($destination, "/ImageDataGenerator/Assets/ImageDataGenerator/")
  $editorBasePath = -join ($destination, "/ImageDataGenerator/Assets/Editor/ImageDataGenerator/")

  $builderPath = -join ($myPath, "/Libraries/UnityPackager/UnityPackager.exe")
  $packageOutPath = -join ($myPath, "/ImageDataGenerator.unitypackage")

  # Args for library and installer generation
  $packageBuildArgs = -join ("null", " ", $packageOutPath, " ")

  # Add runtimeFiles files to package
  Write-Host "Adding runtime files" -ForegroundColor green
  For ($i=0; $i -lt $runtimeFiles.Count; $i++)
  {
      Write-Host $runtimeFiles.Get($i).Replace($runtimeBasePath, "")
      $packageBuildArgs += -join ($runtimeFiles.Get($i), " ", "Assets/ImageDataGenerator/", $runtimeFiles.Get($i).Replace($runtimeBasePath, ""), " ")
  }
  Write-Host "Done adding runtime files" -ForegroundColor green

  # Add editor files to package under a different path
  Write-Host "Adding editor files" -ForegroundColor green
  For ($i=0; $i -lt $editorFiles.Count; $i++)
  {
      $packageBuildArgs += -join ($editorFiles.Get($i), " ", "Assets/Editor/ImageDataGenerator/", $editorFiles.Get($i).Replace($editorBasePath, ""), " ")
      Write-Host $editorFiles.Get($i).Replace($runtimeBasePath, "")
  }
  Write-Host "Done adding editor files" -ForegroundColor green

  Start-Process -FilePath $builderPath -ArgumentList $packageBuildArgs
}

exit
