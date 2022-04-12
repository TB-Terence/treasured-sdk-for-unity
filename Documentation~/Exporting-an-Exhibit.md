# Exporting an Exhibit

After you are done with setting up and adding contents of your own exhibit, it's time to export it.

 The default export root will be `the path of your Unity project/Treasured Data/[Folder Name]`. You can config your export root to a folder of your choice. This can be down by click the `dropdown` button besides `Export` button.

Export settings can be config under `Export Settings` tab.

**Export Settings**
|Property | Description|
|-|-|
|Folder Name|The name of output data folder.|

**Exporters**

There are various exporters to generate different files. All exporters must be turn on to generate valid output.

**Json Exporter**

|Property | Description|
|-|-|
|Formatting| Formatting of the JSON file. None is recommended.|

**Cubemap Exporter**

|Property | Description|
|-|-|
|Image Format| Cubemap image format. Choose `Ktx 2`. |
|Image Quality| The resolution of the cubemap. The higher the better. Lower resolution is mainly for quick export. |
|Cubemap Format| Cubemap file structure. Choose `Individual Face`. |

**Mesh Exporter**

|Property | Description|
|-|-|
|Use Tag||
|Use LayerMask||

## Export

Once everything is setup, click the `Export` button to export the current scene. This will take a while.

 Click the `folder` buton to show the exported folder in the File Explorer.
 > This function is disabled if the folder is not found. Usually before your first export.