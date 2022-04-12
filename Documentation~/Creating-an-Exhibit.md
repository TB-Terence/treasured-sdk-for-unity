# Creating an Exhibit

Once your scene is complete and ready, in the `Hierarchy` tab, Right-click > `Treasured > Create Empty Map`. 
> Alternatively, in the Unity Toolbar: GameObject > Treasured > Create Empty Map

> Note that this option will be disabled if one of the parent game object on the one you selected has the TreasuredMap component attached to it.

With the Treasured Map selected in the Hierarchy tab, you can now add or modify contents of your exhibit.

## Page Info

This tab allow you to modify the look of the landing page of your exhibit.

|Property       |Description|
|-              |-|
|Author         |Your name or the name of your company.|
|Title          |The name of your exhibit.|
|Description    |A short description of the exhibit.|
|Audio Url      |The URL for the background music for the landing page.|
|Mute On Start  | If unchecked, it will automatically play the background music provide above when visiting the exhibit.|
|Template Loader/Template| The style of the landing page. Use the dropdown on the right to select pre-defined template.|
|Head HTML|Custom head html to inject. Currently not supported.|

**UI Settings**

|Property   |Description|
|-|-|
|Show Hotspot Buttons||
|Show Interactable Buttons||
|Project Dome Onto Geometry||
|Show Onboarding||
|Show Cursor||

**Features**

|Property|Description|
|-|-|
|Matterport Style Navigation||

## Objects

This tab allow you to easliy manage the objects in your exhibit.

Each category represents the type of object you can add. Click the `+` button on the right to create a new object. This creates a new object under the `TreasuredMap/[category]`.

The order of the Hotspot path is based on the order on the `Hierarchy` tab.

You can temporarily disable the export of certain object by uncheck it on the left.

You can left click to preview the position and rotation of the camera in the Unity Editor.
> Note that the preview is not guarantee to be identical in the browser. Although most of the time this is very close.

You can right click on the object to perform object specific functions(e.g., Rename and Remove).

You can left click on the `menu(3 vertical dots)` button to perform category sepecific functions(e.g., Relect All)

[Next - Exporting an Exhibit](Exporting-an-Exhibit.md)