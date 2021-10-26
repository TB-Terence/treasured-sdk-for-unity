## Version 0.7.1 (October 26, 2021)

### Improvements

- Enhance data validation and error message prompt.

### Add

- Add quality percentage control.

### Fix

- Reorder action group not working.

## Version 0.7.0 (October 24, 2021)

### Changes

- Exports will now be in cubemap instead of equirectangular.

### Add

- Add camera preview when object selection changes.
- Add search object by name or Id.
- Add debug lines for visible targets on the Hotspot component.
- Add export options
- Add the ability to cancel export.

## Version 0.6.1 (October 17, 2021)

### Fix

- Fix hotspot image being on same location.

### Add

- Add HotspotCamera component.
- Add Hitbox component.
- JsonConverters for new components.

### Changes

- Change gizmos for TreasureObject. Hitbox will now display as transparent box. Camera will now display as frustum.
- Update JSON to match the changes.
- Downgrade JsonConverter to use non-generic converters.

## Version 0.6.0 (October 14, 2021)

### Add

- Add Image Processor plugin
- Add WebP format for Panoramic Images to reduce file size
- Add Mask Image output to export use for highlighting
- Add Action Group to Treasured Object

## Version 0.5.3 (September 28, 2021)

### Add

- Add `created` time in the JSON file

### New features

- Add functionality to export Object Ids. Make sure to setup `Interactable Layer` for all interactables in the Object Management group

## Version 0.5.2 (September 24, 2021)

- Add `title` and `description` validation
- Add `target` for Select-Object action validation

## Version 0.5.1 (September 23, 2021)

### Bug Fix

- Fix GameObject context menu for creating TreasuredMap/Hotspot and Interactable.

### Add

- Add a regenerate guid button for ids.
- Add Create New button for object management.
- Add Camera Transform to JSON.
- Add Camera Rotation Offset to Hotspot.
- Add hotspot button to the object management.

### Remove

- Remove unused scripts

## Version 0.5.0 (September 23, 2021)

**_The system has changed internally and must be manually upgraded by click Upgrade to v0.5.0.0 from the Treasured Map component's context menu_**

### Update

- Default output location is fixed to project root under a folder called `Treasured Data`. The output exhibit folder is customizable and will have the scene name as default.

### Change

- The output path for image is changed to `Treasured Data/{folder name}/images/{hotspot id}/{quality}.{extension}`
- `Target` for `SelectObjectAction` is replaced with object field. The assigned target must belong to the same map.
- Replaced `displayMode` with `position` for `EmbedAction`

### Add

- Add transform tools for Hotspot camera position.

### Remove

- All object controls have been removed. The same functionality can be achieved by click the list item.
- Hotspot overwrite has been removed and replaced with `Camera Position Offset` on the Hotspot component. The same functionality can be achieved by selecting all hotspots and adjusting the `Camera Position Offset`.
- Multi-object editing has been removed to make the scene view much cleaner.

## Version 0.4.1 (September 17, 2021)

- Add `volume` property to `Play Audio` action

## Version 0.3.6 (August 25, 2021)

- Added `style` field for `Show Text` action

## Version 0.3.5 (August 23, 2021)

- Fix output quality folder to be lower case
- Add hitbox gizmos for interactables

## Version 0.3.4 (August 19, 2021)

- Fixed find hitbox center position to be based on size of the hitbox instead of ground point.
- Add utility to find transform in scene

## Version 0.3.3 (August 19, 2021)

- Fixed gizmos for Previewer
- Improved process of finding hitbox position.

## Version 0.3.2 (August 17, 2021)

- Add Visible Targets to Json
- Add Layer Mask for export settings

## Version 0.3.1 (August 17, 2021)

#### Fixes

- Change output format jpg from jpeg and lower case the format in JSON
- Auto-generate id for the first item in the list
- Update image output file name

#### Features

- Add dropdown for selecting Target ID for `Select Object` action
- Add button to regenerate Action ID
- Add tabs for Hotspots and Interactables along with some utilities to work with the list
- Add Utility for Transform
- Add Utility for Hitbox

## Version 0.3.0 (August 14, 2021)

#### New export workflow

- New design for data model
- New design for Editor GUI
- Optimized Editor Drawer
- Added Treasured Data Previewer with modification tool

## Version 0.2.0 (August 9, 2021)

#### minor refactoring

## Version 0.1.0 (August 6, 2021)

#### initial development
