$runtimeFiles = Get-ChildItem -Path "./Assets/AnnotationSystem/*"  -Recurse -File -Force
$runtimeFiles = $runtimeFiles.FullName.Replace("\", "/")

$editorFiles = Get-ChildItem -Path "./Assets/Editor/AnnotationSystem/*.cs" -Recurse -File
$editorFiles = $editorFiles.FullName.Replace("\", "/")

# Check if any file contains a space! UnityPackager.exe does not allow spaces in file names
$allFiles = $runtimeFiles + $editorFiles
$checkSuccessful = $true
For ($i=0; $i -lt $allFiles.Count; $i++)
{
    if ($allFiles.Get($i) -match '\s')
    {
      Write-Host $allFiles.Get($i)
      Write-Error -Message "Error:  : Filename contains a space." -TargetObject $allFiles.Get($i) -Category SyntaxError
      $checkSuccessful = $false
    }
}

if ($checkSuccessful -eq $true)
{
  $myPath = (Get-Item -Path ".\").FullName;
  $myPath = $myPath.Replace("\", "/")

  $runtimeBasePath = -join ($myPath, "/Assets/AnnotationSystem/")
  $editorBasePath = -join ($myPath, "/Assets/Editor/AnnotationSystem/")

  $builderPath = -join ($myPath, "/Libraries/UnityPackager/UnityPackager.exe")
  $packageOutPath = -join ($myPath, "/AnnotationSystem.unitypackage")

  # Args for library and installer generation
  $packageBuildArgs = -join ("null", " ", $packageOutPath, " ")

  # Add runtimeFiles files to package
  Write-Host "Adding runtime files" -ForegroundColor green
  For ($i=0; $i -lt $runtimeFiles.Count; $i++)
  {
      Write-Host $runtimeFiles.Get($i).Replace($runtimeBasePath, "")
      $packageBuildArgs += -join ($runtimeFiles.Get($i), " ", "Assets/AnnotationSystem/", $runtimeFiles.Get($i).Replace($runtimeBasePath, ""), " ")
  }
  Write-Host "Done adding runtime files" -ForegroundColor green

  # Add editor files to package under a different path
  Write-Host "Adding editor files" -ForegroundColor green
  For ($i=0; $i -lt $editorFiles.Count; $i++)
  {
      $packageBuildArgs += -join ($editorFiles.Get($i), " ", "Assets/Editor/AnnotationSystem/", $editorFiles.Get($i).Replace($editorBasePath, ""), " ")
      Write-Host $editorFiles.Get($i)
  }
  Write-Host "Done adding editor files" -ForegroundColor green

  Start-Process -FilePath $builderPath -ArgumentList $packageBuildArgs
}

Write-Host "Press any key...."
while( (-not $Host.UI.RawUI.KeyAvailable) ){}
exit
